// HarmonyHost C shim：ArkTS (libentry.so) 与 .NET (libapp.so) 之间的桥
// 职责：
//   1. 设置 NativeAOT 运行时引导参数（必须在 CLR 首次初始化前）
//   2. dlopen libapp.so 并转发 HarmonyInit / HarmonyBuildUI
//   3. 作为 napi 模块被 ArkTS 页面 import
#include "napi/native_api.h"
#include <web/native_interface_arkweb.h>
#include <web/arkweb_scheme_handler.h>
#include <dlfcn.h>
#include <hilog/log.h>
#include <pthread.h>
#include <stdatomic.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#undef LOG_DOMAIN
#undef LOG_TAG
#define LOG_DOMAIN 0x0000
#define LOG_TAG "HarmonyHost"

typedef int (*harmony_init_t)(void* env);
typedef int (*harmony_buildui_t)(void* env, void* nodeContentValue);
typedef int (*harmony_poppage_t)(void* env);
typedef int (*harmony_themechanged_t)(void* env, int colorMode);
typedef int (*harmony_web_init_t)(void* env, void* webViewValue);
typedef int (*harmony_web_init_tagged_t)(void* env, void* webViewValue, void* tagValue);
typedef int (*harmony_web_init_node_t)(void* env, void* webViewValue, void* tagValue, void* frameNodeValue);
typedef int (*harmony_web_shutdown_t)(void* env, const char* webTag);
typedef int (*harmony_web_page_ended_t)(void* env, void* urlValue, void* tagValue);
typedef int (*harmony_web_should_intercept_t)(void* env, void* urlValue, void* tagValue);
typedef int (*harmony_web_receive_t)(void* env, void* messageValue);
typedef int (*harmony_web_proxy_ready_t)(void* env);
typedef int (*harmony_web_intercept_native_t)(void* env, int requestId, const char* url);
typedef int (*harmony_engine_init_t)(void* env, void* bridgeValue);

// ArkTS 引擎实验通道（EngineLab）：dlsym 惰性解析，非引擎应用无导出时短路
static harmony_engine_init_t g_engineInit = NULL;
static bool g_engineResolved = false;

// Web 组件的固定 webTag：ArkTS 侧 WebviewController('harmony-web') 与 NDK ArkWeb API 按 tag 寻址同一组件
#define HARMONY_WEB_TAG "harmony-web"
// Blazor/MAUI Web 的虚拟源（与 C# 侧 HarmonyBlazorWebViewManager.AppOrigin 约定一致）
#define HARMONY_WEB_ORIGIN "https://localhost/"

static void* g_app = NULL;

// Web 通道导出符号缓存：dlsym 仅在首次调用时解析（surface 可选，无导出时保持 NULL 并短路）
static harmony_web_init_t g_webInit = NULL;
static harmony_web_receive_t g_webReceive = NULL;
static harmony_web_proxy_ready_t g_webProxyReady = NULL;
static harmony_web_intercept_native_t g_webInterceptNative = NULL;
static harmony_web_init_tagged_t g_webInitTagged = NULL;
static harmony_web_init_node_t g_webInitNode = NULL;
static harmony_web_shutdown_t g_webShutdown = NULL;
static harmony_web_page_ended_t g_webPageEnded = NULL;
static harmony_web_should_intercept_t g_webShouldIntercept = NULL;
static bool g_webSymbolsResolved = false;

static void resolve_web_symbols(void)
{
    if (g_webSymbolsResolved) return;
    g_webSymbolsResolved = true;
    if (!g_app) return;
    g_webInit = (harmony_web_init_t)dlsym(g_app, "HarmonyWebInit");
    g_webReceive = (harmony_web_receive_t)dlsym(g_app, "HarmonyWebReceiveMessage");
    g_webProxyReady = (harmony_web_proxy_ready_t)dlsym(g_app, "HarmonyWebProxyReady");
    g_webInterceptNative = (harmony_web_intercept_native_t)dlsym(g_app, "HarmonyWebInterceptNative");
    g_webInitTagged = (harmony_web_init_tagged_t)dlsym(g_app, "HarmonyWebInitTagged");
    g_webInitNode = (harmony_web_init_node_t)dlsym(g_app, "HarmonyWebInitWithNode");
    g_webShutdown = (harmony_web_shutdown_t)dlsym(g_app, "HarmonyWebShutdown");
    g_webPageEnded = (harmony_web_page_ended_t)dlsym(g_app, "HarmonyWebPageEnded");
    g_webShouldIntercept = (harmony_web_should_intercept_t)dlsym(g_app, "HarmonyWebShouldInterceptNavigation");
}

// NDK ArkWeb proxy-valid → JS 线程跳板：
// OnValidCallback 在 ArkWeb 自己的线程触发（无 napi_env），经 TSFN 投递回 env 线程后再进 .NET。
static napi_threadsafe_function g_webReadyTsfn = NULL;

static void OnWebProxyReadyJs(napi_env env, napi_value jsCb, void* ctx, void* data)
{
    (void)jsCb; (void)ctx; (void)data;
    if (!g_webProxyReady) return;
    int r = g_webProxyReady((void*)env);
    if (r == 0) {
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                     "web JS proxy reported valid, .NET notified");
    } else {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG,
                     "HarmonyWebProxyReady failed: %{public}d", r);
    }
}

static void OnWebProxyValid(const char* name)
{
    (void)name;
    if (g_webReadyTsfn) {
        napi_call_threadsafe_function(g_webReadyTsfn, NULL, napi_tsfn_nonblocking);
    }
}

// ---- NDK scheme 拦截（通道化第二步）：替换 ArkTS onInterceptRequest 的同步 napi 往返 ----
// 线程模型：OnRequestStart 在 ArkWeb IO 线程（禁碰 napi）→ 按 location 前缀同步决策
// → TSFN 投递到 JS env 线程执行 C# 处理器（内部需要 NAPI 读 rawfile）→ C# 回调
// HarmonyHostWebRespond（任意线程）完成 DidReceive* 序列。句柄生命周期在 OnRequestStop 统一回收。
typedef struct WebInterceptEntry {
    int id;
    atomic_bool completed;
    ArkWeb_ResourceRequest* request;
    const ArkWeb_ResourceHandler* handler;
    struct WebInterceptEntry* next;
} WebInterceptEntry;

typedef struct WebInterceptPayload {
    int id;
    char* url;
} WebInterceptPayload;

static napi_threadsafe_function g_webInterceptTsfn = NULL;
static ArkWeb_SchemeHandler* g_schemeHandler = NULL;
static atomic_int g_interceptSeq = 1;
static pthread_mutex_t g_interceptLock = PTHREAD_MUTEX_INITIALIZER;
static WebInterceptEntry* g_interceptList = NULL;

static WebInterceptEntry* intercept_find_locked(int id)
{
    for (WebInterceptEntry* e = g_interceptList; e; e = e->next) {
        if (e->id == id) return e;
    }
    return NULL;
}

static void intercept_destroy_entry(WebInterceptEntry* e)
{
    if (e->request) OH_ArkWebResourceRequest_Destroy(e->request);
    if (e->handler) OH_ArkWebResourceHandler_Destroy(e->handler);
    free(e);
}

static void intercept_remove_locked(int id)
{
    WebInterceptEntry** pp = &g_interceptList;
    while (*pp) {
        if ((*pp)->id == id) {
            WebInterceptEntry* victim = *pp;
            *pp = victim->next;
            intercept_destroy_entry(victim);
            return;
        }
        pp = &(*pp)->next;
    }
}

static void OnInterceptDeliverJs(napi_env env, napi_value jsCb, void* ctx, void* data)
{
    (void)jsCb; (void)ctx;
    WebInterceptPayload* payload = (WebInterceptPayload*)data;
    if (!payload) return;
    if (g_webInterceptNative) {
        g_webInterceptNative((void*)env, payload->id, payload->url);
    }
    free(payload->url);
    free(payload);
}

static void OnSchemeRequestStart(const ArkWeb_SchemeHandler* schemeHandler,
                                 ArkWeb_ResourceRequest* resourceRequest,
                                 const ArkWeb_ResourceHandler* resourceHandler,
                                 bool* intercept)
{
    (void)schemeHandler;
    char* url = NULL;
    OH_ArkWebResourceRequest_GetUrl(resourceRequest, &url);
    bool hit = url != NULL && g_webInterceptNative != NULL
               && strncmp(url, HARMONY_WEB_ORIGIN, strlen(HARMONY_WEB_ORIGIN)) == 0;
    *intercept = hit;
    if (!hit) {
        if (url) OH_ArkWeb_ReleaseString(url);
        return;
    }

    WebInterceptEntry* entry = (WebInterceptEntry*)calloc(1, sizeof(WebInterceptEntry));
    entry->id = atomic_fetch_add(&g_interceptSeq, 1);
    atomic_store(&entry->completed, false);
    entry->request = resourceRequest;
    entry->handler = resourceHandler;
    pthread_mutex_lock(&g_interceptLock);
    entry->next = g_interceptList;
    g_interceptList = entry;
    pthread_mutex_unlock(&g_interceptLock);

    WebInterceptPayload* payload = (WebInterceptPayload*)malloc(sizeof(WebInterceptPayload));
    payload->id = entry->id;
    payload->url = url;  // 所有权移交 js_cb（释放用 free——ReleaseString 的缓冲就是 malloc 族）
    if (g_webInterceptTsfn) {
        napi_call_threadsafe_function(g_webInterceptTsfn, payload, napi_tsfn_nonblocking);
    } else {
        free(payload->url);
        free(payload);
        pthread_mutex_lock(&g_interceptLock);
        WebInterceptEntry* e = intercept_find_locked(entry->id);
        if (e) {
            OH_ArkWebResourceHandler_DidFailWithError(e->handler, ARKWEB_ERR_FAILED);
            atomic_store(&e->completed, true);
        }
        pthread_mutex_unlock(&g_interceptLock);
    }
}

static void OnSchemeRequestStop(const ArkWeb_SchemeHandler* schemeHandler,
                                const ArkWeb_ResourceRequest* resourceRequest)
{
    (void)schemeHandler;
    pthread_mutex_lock(&g_interceptLock);
    WebInterceptEntry** pp = &g_interceptList;
    while (*pp) {
        if ((*pp)->request == resourceRequest) {
            WebInterceptEntry* victim = *pp;
            *pp = victim->next;
            intercept_destroy_entry(victim);
            break;
        }
        pp = &(*pp)->next;
    }
    pthread_mutex_unlock(&g_interceptLock);
}

// C# → libentry 回写通道（Maui.Web 经 NativeLibrary.TryGetExport 寻址；任意线程可调用）
__attribute__((visibility("default")))
void HarmonyHostWebRespond(int requestId, int status, const char* statusText,
                           const char* mime, const uint8_t* data, int64_t len)
{
    pthread_mutex_lock(&g_interceptLock);
    WebInterceptEntry* e = intercept_find_locked(requestId);
    if (e && !atomic_load(&e->completed)) {
        atomic_store(&e->completed, true);
        pthread_mutex_unlock(&g_interceptLock);

        ArkWeb_Response* resp = NULL;
        OH_ArkWeb_CreateResponse(&resp);
        OH_ArkWebResponse_SetStatus(resp, status);
        OH_ArkWebResponse_SetStatusText(resp, statusText ? statusText : "OK");
        OH_ArkWebResponse_SetMimeType(resp, mime ? mime : "text/plain");
        OH_ArkWebResponse_SetCharset(resp, "utf-8");
        // 与原 ArkTS 拦截回包保持一致：Blazor 静态资产加载要求 CORS ping 放行
        OH_ArkWebResponse_SetHeaderByName(resp, "Access-Control-Allow-Origin", "null", true);
        OH_ArkWebResponse_SetHeaderByName(resp, "Access-Control-Allow-Credentials", "true", true);
        OH_ArkWebResourceHandler_DidReceiveResponse(e->handler, resp);
        OH_ArkWeb_DestroyResponse(resp);
        if (data && len > 0) {
            OH_ArkWebResourceHandler_DidReceiveData(e->handler, data, len);
        }
        OH_ArkWebResourceHandler_DidFinish(e->handler);
    } else {
        pthread_mutex_unlock(&g_interceptLock);
    }
}

__attribute__((visibility("default")))
void HarmonyHostWebRespondFail(int requestId)
{
    pthread_mutex_lock(&g_interceptLock);
    WebInterceptEntry* e = intercept_find_locked(requestId);
    if (e && !atomic_load(&e->completed)) {
        atomic_store(&e->completed, true);
        pthread_mutex_unlock(&g_interceptLock);
        OH_ArkWebResourceHandler_DidFailWithError(e->handler, ARKWEB_ERR_FAILED);
    } else {
        pthread_mutex_unlock(&g_interceptLock);
    }
}

// 主题回调注册表：.NET 库代码（HarmonyOS.Maui）在初始化时经 libentry 导出符号
// 主动注册函数指针——ILC 只导出入口程序集的 [UnmanagedCallersOnly]，库内方法无法
// 经 dlsym 发现，但取函数指针不受此限。注册后优先于 dlsym 回退路径。
typedef int (*theme_changed_fn)(void* env, int colorMode);
static theme_changed_fn g_themeChangedCallback = NULL;

typedef int (*lifecycle_changed_fn)(void* env, int lifecycleEvent);
static lifecycle_changed_fn g_lifecycleChangedCallback = NULL;

__attribute__((visibility("default")))
void HarmonyHostSetThemeChangedCallback(void* fn)
{
    g_themeChangedCallback = (theme_changed_fn)fn;
}

__attribute__((visibility("default")))
void HarmonyHostSetLifecycleChangedCallback(void* fn)
{
    g_lifecycleChangedCallback = (lifecycle_changed_fn)fn;
}

static void loge(const char* what, const char* detail)
{
    OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG, "%{public}s: %{public}s",
                 what, detail ? detail : "(no detail)");
}

static bool load_dotnet(void)
{
    if (g_app) return true;

    // 引导参数必须在任何 C# 导出函数被调用之前 setenv：
    // NativeAOT 运行时在首次进入导出函数时才初始化 CLR。
    // GC 硬上限：region 模式默认预留 ~256G 虚拟内存，会触发 mmap 限制；1GiB 物理足够样例。
    setenv("DOTNET_GCHeapHardLimit", "0x40000000", 1);
    // 云真机 seccomp 会拦截 get_mempolicy；禁用 GC NUMA 探测与亲和化。
    setenv("DOTNET_GCNumaAware", "0", 1);
    setenv("DOTNET_GCNoAffinitize", "1", 1);
    // 鸿蒙系统 ICU 数据路径（社区移植配方）
    setenv("ICU_DATA", "/system/usr/ohos_icu", 1);

    g_app = dlopen("libapp.so", RTLD_NOW | RTLD_LOCAL);
    if (!g_app) {
        loge("dlopen libapp.so", dlerror());
        return false;
    }
    return true;
}

// 页面重建（恢复/多次 aboutToAppear）会让 TS 再走 passNodeContent；.NET 侧 UI 树只应该建一次
static bool g_uiBuilt = false;

static bool ensure_runtime(napi_env env)
{
    if (!load_dotnet()) return false;

    harmony_init_t init = (harmony_init_t)dlsym(g_app, "HarmonyInit");
    if (!init) {
        loge("dlsym HarmonyInit", dlerror());
        return false;
    }
    int r = init((void*)env);
    if (r != 0) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG, "HarmonyInit failed: %{public}d", r);
        return false;
    }
    OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                 ".NET runtime initialized, env=%{public}p", (void*)env);
    return true;
}

// 返回键：调 C# 页面栈 Pop；栈底返回 0 交还系统默认行为
static napi_value PopPage(napi_env env, napi_callback_info info)
{
    (void)info;
    if (!ensure_runtime(env)) return NULL;

    harmony_poppage_t pop = (harmony_poppage_t)dlsym(g_app, "HarmonyPopPage");
    if (!pop) {
        loge("dlsym HarmonyPopPage", dlerror());
        return NULL;
    }
    int consumed = pop((void*)env);
    napi_value result;
    napi_get_boolean(env, consumed != 0, &result);
    return result;
}

static napi_value InitDotnet(napi_env env, napi_callback_info info)
{
    (void)info;
    ensure_runtime(env);
    // 探针：尽可能早地（Web 组件 build 之前）挂 proxy-valid 回调，
    // 验证它是"错过时刻后不再补发"还是"仅 NDK 注册路径触发"。
    if (g_webReadyTsfn == NULL) {
        napi_value resourceName;
        napi_create_string_utf8(env, "web-proxy-ready", NAPI_AUTO_LENGTH, &resourceName);
        napi_create_threadsafe_function(env, NULL, NULL, resourceName, 1,
                                        1, NULL, NULL, NULL, OnWebProxyReadyJs, &g_webReadyTsfn);
    }
    OH_NativeArkWeb_SetJavaScriptProxyValidCallback(HARMONY_WEB_TAG, OnWebProxyValid);
    OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                 "web proxy-valid callback armed at initDotnet");
    return NULL;
}

static napi_value PassNodeContent(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;
    if (g_uiBuilt) {
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                     "passNodeContent: UI already built, skipping rebuild");
        return NULL;
    }
    g_uiBuilt = true;

    size_t argc = 1;
    napi_value argv[1];
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    if (argc < 1) {
        loge("passNodeContent", "missing NodeContent argument");
        return NULL;
    }

    harmony_buildui_t build = (harmony_buildui_t)dlsym(g_app, "HarmonyBuildUI");
    if (!build) {
        loge("dlsym HarmonyBuildUI", dlerror());
        return NULL;
    }

    int r = build((void*)env, (void*)argv[0]);
    if (r == 0) {
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG, ".NET UI built successfully");
    } else {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG, "HarmonyBuildUI failed: %{public}d", r);
    }
    return NULL;
}

// ArkTS 引擎通道（共享 shim 内置，EngineLab 场景）：桥对象交给 libapp.so 的 HarmonyEngineInit 导出；
// 引擎 ETS 经 Host overlay 注入，C 通道自此不再需要 overlay 自持副本。
static napi_value InitEngine(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 1;
    napi_value argv[1];
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    if (argc < 1) {
        loge("initEngine", "missing engine bridge argument");
        return NULL;
    }

    if (!g_engineResolved) {
        g_engineResolved = true;
        g_engineInit = (harmony_engine_init_t)dlsym(g_app, "HarmonyEngineInit");
    }
    if (!g_engineInit) {
        loge("initEngine", "HarmonyEngineInit export not found in libapp.so");
        return NULL;
    }

    int r = g_engineInit((void*)env, (void*)argv[0]);
    if (r == 0) {
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG, "ArkTS engine bridge attached");
    } else {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG, "HarmonyEngineInit failed: %{public}d", r);
    }
    return NULL;
}

static napi_value PassWebView(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 3;
    napi_value argv[3] = {NULL, NULL, NULL};
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    if (argc < 1) {
        loge("passWebView", "missing WebviewController argument");
        return NULL;
    }
    // 可选第二参数：webTag（多实例区分 surface）；可选第三参数：BuilderNode 的 FrameNode（内嵌模式）
    napi_value webTagValue = argc >= 2 ? argv[1] : NULL;
    napi_value frameNodeValue = argc >= 3 ? argv[2] : NULL;

    resolve_web_symbols();
    if (!g_webInit) {
        loge("passWebView", "HarmonyWebInit export not found in libapp.so");
        return NULL;
    }

    // 就绪信号优先通道：ArkWeb proxy 生效时经 TSFN 通知 .NET（.NET 侧保留 500ms 兜底延迟）
    // 回调必须在 Web 组件加载前挂载——实测 passWebView（onAppear 时刻）已错过首个 valid 时刻，
    // 提前挂载由 InitDotnet 完成，此处不再重复。
    if (g_webReadyTsfn == NULL) {
        napi_value resourceName;
        napi_create_string_utf8(env, "web-proxy-ready", NAPI_AUTO_LENGTH, &resourceName);
        napi_create_threadsafe_function(env, NULL, NULL, resourceName, 1,
                                        1, NULL, NULL, NULL, OnWebProxyReadyJs, &g_webReadyTsfn);
    }
    // NDK scheme 拦截：TSFN 专用通道 + https 处理器（browser context 已在 Web onAppear 时创建）
    if (g_webInterceptTsfn == NULL) {
        napi_value resourceName;
        napi_create_string_utf8(env, "web-intercept", NAPI_AUTO_LENGTH, &resourceName);
        napi_create_threadsafe_function(env, NULL, NULL, resourceName, 0,
                                        1, NULL, NULL, NULL, OnInterceptDeliverJs, &g_webInterceptTsfn);
    }
    if (g_schemeHandler == NULL) {
        OH_ArkWeb_CreateSchemeHandler(&g_schemeHandler);
        OH_ArkWebSchemeHandler_SetOnRequestStart(g_schemeHandler, OnSchemeRequestStart);
        OH_ArkWebSchemeHandler_SetOnRequestStop(g_schemeHandler, OnSchemeRequestStop);
        if (OH_ArkWeb_SetSchemeHandler("https", HARMONY_WEB_TAG, g_schemeHandler)) {
            OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                         "NDK scheme handler registered for https://%s", HARMONY_WEB_TAG);
        } else {
            loge("passWebView", "OH_ArkWeb_SetSchemeHandler failed");
        }
    }

    int r;
    if (g_webInitNode && frameNodeValue) {
        r = g_webInitNode((void*)env, (void*)argv[0], (void*)webTagValue, (void*)frameNodeValue);
    } else if (g_webInitTagged) {
        r = g_webInitTagged((void*)env, (void*)argv[0], (void*)webTagValue);
    } else {
        r = g_webInit((void*)env, (void*)argv[0]);
    }
    if (r == 0) {
        OH_LOG_Print(LOG_APP, LOG_INFO, LOG_DOMAIN, LOG_TAG,
                     ".NET WebView bridge initialized");
    } else {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG,
                     "HarmonyWebInit failed: %{public}d", r);
    }
    return NULL;
}

static napi_value NotifyWebPageEnded(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 2;
    napi_value argv[2] = {NULL, NULL};
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    resolve_web_symbols();
    if (!g_webPageEnded) return NULL;

    int r = g_webPageEnded((void*)env,
                           argc >= 1 ? (void*)argv[0] : NULL,
                           argc >= 2 ? (void*)argv[1] : NULL);
    if (r != 0) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG,
                     "HarmonyWebPageEnded failed: %{public}d", r);
    }
    return NULL;
}

static napi_value ShouldInterceptNavigation(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 2;
    napi_value argv[2] = {NULL, NULL};
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    resolve_web_symbols();

    napi_value result;
    if (!g_webShouldIntercept) {
        napi_get_boolean(env, false, &result);
        return result;
    }

    int r = g_webShouldIntercept((void*)env,
                                 argc >= 1 ? (void*)argv[0] : NULL,
                                 argc >= 2 ? (void*)argv[1] : NULL);
    napi_get_boolean(env, r == 1, &result);
    return result;
}

static napi_value ShutdownWebView(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 1;
    napi_value argv[1] = {NULL};
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    resolve_web_symbols();
    if (!g_webShutdown) return NULL;

    g_webShutdown((void*)env, argc >= 1 ? (void*)argv[0] : NULL);
    return NULL;
}

static napi_value ReceiveWebViewMessage(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 1;
    napi_value argv[1];
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    if (argc < 1) {
        loge("receiveWebViewMessage", "missing message");
        return NULL;
    }

    resolve_web_symbols();
    if (!g_webReceive) {
        loge("receiveWebViewMessage", "HarmonyWebReceiveMessage export not found in libapp.so");
        return NULL;
    }

    if (g_webReceive((void*)env, (void*)argv[0]) != 0) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG, "HarmonyWebReceiveMessage failed");
    }
    return NULL;
}

// 系统 colorMode 变化：转发给 .NET，走 MAUI IApplication.ThemeChanged 标准路径
static napi_value NotifyThemeChanged(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 1;
    napi_value argv[1];
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    int colorMode = -1;
    if (argc >= 1) {
        napi_get_value_int32(env, argv[0], &colorMode);
    }

    // 优先走 .NET 侧注册的函数指针；旧版 app.so（导出 HarmonyThemeChanged）回退 dlsym
    harmony_themechanged_t themeChanged = g_themeChangedCallback;
    if (!themeChanged) {
        themeChanged = (harmony_themechanged_t)dlsym(g_app, "HarmonyThemeChanged");
    }
    if (!themeChanged) {
        loge("dlsym HarmonyThemeChanged", dlerror());
        return NULL;
    }

    int r = themeChanged((void*)env, colorMode);
    if (r != 0) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG,
                     "HarmonyThemeChanged failed: %{public}d", r);
    }
    return NULL;
}

static napi_value NotifyLifecycleChanged(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

    size_t argc = 1;
    napi_value argv[1];
    napi_get_cb_info(env, info, &argc, argv, NULL, NULL);
    int lifecycleEvent = -1;
    if (argc >= 1) {
        napi_get_value_int32(env, argv[0], &lifecycleEvent);
    }

    if (!g_lifecycleChangedCallback) {
        loge("notifyLifecycleChanged", "lifecycle callback is not registered");
        return NULL;
    }

    int r = g_lifecycleChangedCallback((void*)env, lifecycleEvent);
    if (r != 0) {
        OH_LOG_Print(LOG_APP, LOG_ERROR, LOG_DOMAIN, LOG_TAG,
                     "Harmony lifecycle callback failed: %{public}d", r);
    }
    return NULL;
}

EXTERN_C_START
static napi_value ModuleInit(napi_env env, napi_value exports)
{
    napi_property_descriptor desc[] = {
        {"initDotnet", NULL, InitDotnet, NULL, NULL, NULL, napi_default, NULL},
        {"popPage", NULL, PopPage, NULL, NULL, NULL, napi_default, NULL},
        {"passNodeContent", NULL, PassNodeContent, NULL, NULL, NULL, napi_default, NULL},
        {"passWebView", NULL, PassWebView, NULL, NULL, NULL, napi_default, NULL},
        {"receiveWebViewMessage", NULL, ReceiveWebViewMessage, NULL, NULL, NULL, napi_default, NULL},
        {"notifyWebPageEnded", NULL, NotifyWebPageEnded, NULL, NULL, NULL, napi_default, NULL},
        {"shouldInterceptNavigation", NULL, ShouldInterceptNavigation, NULL, NULL, NULL, napi_default, NULL},
        {"shutdownWebView", NULL, ShutdownWebView, NULL, NULL, NULL, napi_default, NULL},
        {"initEngine", NULL, InitEngine, NULL, NULL, NULL, napi_default, NULL},
        {"notifyThemeChanged", NULL, NotifyThemeChanged, NULL, NULL, NULL, napi_default, NULL},
        {"notifyLifecycleChanged", NULL, NotifyLifecycleChanged, NULL, NULL, NULL, napi_default, NULL},
    };
    napi_define_properties(env, exports, sizeof(desc) / sizeof(desc[0]), desc);
    return exports;
}
EXTERN_C_END

static napi_module harmonyHostModule = {
    .nm_version = 1,
    .nm_flags = 0,
    .nm_filename = NULL,
    .nm_register_func = ModuleInit,
    .nm_modname = "entry", // import native from 'libentry.so'
    .nm_priv = ((void*)0),
    .reserved = {0},
};

__attribute__((constructor)) static void RegisterHarmonyHostModule(void)
{
    napi_module_register(&harmonyHostModule);
}
