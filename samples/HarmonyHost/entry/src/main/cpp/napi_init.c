// HarmonyHost C shim：ArkTS (libentry.so) 与 .NET (libapp.so) 之间的桥
// 职责：
//   1. 设置 NativeAOT 运行时引导参数（必须在 CLR 首次初始化前）
//   2. dlopen libapp.so 并转发 HarmonyInit / HarmonyBuildUI
//   3. 作为 napi 模块被 ArkTS 页面 import
#include "napi/native_api.h"
#include <dlfcn.h>
#include <hilog/log.h>
#include <stdlib.h>

#undef LOG_DOMAIN
#undef LOG_TAG
#define LOG_DOMAIN 0x0000
#define LOG_TAG "HarmonyHost"

typedef int (*harmony_init_t)(void* env);
typedef int (*harmony_buildui_t)(void* env, void* nodeContentValue);

static void* g_app = NULL;

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
    // 鸿蒙系统 ICU 数据路径（社区移植配方）
    setenv("ICU_DATA", "/system/usr/ohos_icu", 1);

    g_app = dlopen("libapp.so", RTLD_NOW | RTLD_LOCAL);
    if (!g_app) {
        loge("dlopen libapp.so", dlerror());
        return false;
    }
    return true;
}

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

static napi_value InitDotnet(napi_env env, napi_callback_info info)
{
    (void)info;
    ensure_runtime(env);
    return NULL;
}

static napi_value PassNodeContent(napi_env env, napi_callback_info info)
{
    if (!ensure_runtime(env)) return NULL;

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

EXTERN_C_START
static napi_value ModuleInit(napi_env env, napi_value exports)
{
    napi_property_descriptor desc[] = {
        {"initDotnet", NULL, InitDotnet, NULL, NULL, NULL, napi_default, NULL},
        {"passNodeContent", NULL, PassNodeContent, NULL, NULL, NULL, napi_default, NULL},
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
