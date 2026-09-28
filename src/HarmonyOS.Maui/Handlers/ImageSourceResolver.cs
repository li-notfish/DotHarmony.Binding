using HarmonyOS.Interop;
using Microsoft.Maui;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using HResourceManager = HarmonyOS.Bindings.Api.ResourceManager;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// IImageSource → ArkUI NODE_IMAGE_SRC 字符串（URI）的解析器。
/// ArkUI C API 的 NODE_IMAGE_SRC 只接受 URI 字符串或 DrawableDescriptor，
/// 流式来源经异步落盘（<see cref="ResolveStreamAsync"/>，不在 UI 线程同步等待）
/// 再以 file:// 提供（M1；像素级 PixelMap 通道后续经 image native 模块接入）。
/// </summary>
internal static class ImageSourceResolver
{
    private const string TempFilePrefix = "imgsrc_";

    public static string? Resolve(IImageSource? source) => source switch
    {
        UriImageSource uri => uri.Uri?.ToString(),
        FileImageSource file => ResolveFile(file.File),
        // 流式来源为异步 I/O：同步等待会在 UI 线程阻塞（宿主未装 SynchronizationContext 的
        // promise 续体环境下仍会造成帧卡顿）——由 handler 层走 ResolveStreamAsync 异步路径
        StreamImageSource => null,
        _ => null, // FontImageSource 等：无对应通道，记为 gap
    };

    private static string? ResolveFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        // 已带 scheme（file:// / http(s):// / data:）原样返回；绝对路径直接 file://。
        // 裸相对名（MAUI 资源约定）由 handler 层走 ResolveRawfileAsync 异步通道
        // （resourceManager.getRawFileContent），miss 后回退本方法的 file://{path}。
        return path.Contains("://") ? path : $"file://{path}";
    }

    /// <summary>
    /// 裸相对名 → 包内 rawfile（HarmonyStageResources 暂存的 maui/ 前缀）解析。
    /// 走 ability context 的 resourceManager.getRawFileContentSync（模块级无参
    /// getResourceManager 在本运行时返回对象类型不匹配，必须 context 作用域；沙盒内
    /// rawfile 无稳定 file 路径可探测）。命中后落盘缓存并以 file:// URI 提供；
    /// 结果缓存（同名资源只读一次）。须在 UI/napi 线程调用。
    /// </summary>
    public static Task<string?> ResolveRawfileAsync(string logicalName)
    {
        var lazy = _rawfileCache.GetOrAdd(logicalName, _ =>
            new Lazy<Task<string?>>(() => ResolveRawfileCoreAsync(logicalName),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return lazy.Value;
    }

    private static async Task<string?> ResolveRawfileCoreAsync(string logicalName)
    {
        var bytes = GetRawfileBytesSync($"maui/{logicalName}");
        if (bytes is null || bytes.Length == 0)
            return null;

        // 优先 ability context 的规范 cacheDir（图像框架按沙盒路径识别），探测式 TempDir 兜底
        var dir = ContextCacheDir() ?? TempDir;
        if (dir is null)
            return null;
        var safe = logicalName.Replace('/', '_');
        var path = Path.Combine(dir, $"rawfile_{safe}");
        await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
        var uri = $"file://{path}";
        HiLog.Info("Image", $"rawfile resolved: {logicalName} -> {path}");
        return uri;
    }

    private static string? ContextCacheDir()
    {
        try
        {
            var dir = new Essentials.HarmonyFileSystem().CacheDirectory;
            if (string.IsNullOrEmpty(dir))
                return null;
            Directory.CreateDirectory(dir);
            return dir;
        }
        catch (Exception ex)
        {
            HiLog.Debug("Image", $"context cacheDir unavailable: {ex.Message}");
            return null;
        }
    }

    private static readonly ConcurrentDictionary<string, Lazy<Task<string?>>> _rawfileCache =
        new(StringComparer.Ordinal);
    private static global::HarmonyOS.Bindings.Api.ResourceManagerObject? _contextResourceManager;
    // _rawfileCache / _contextResourceManager 的并发保护：影子加载走后台 Task，
    // 同名资源多发起请求应当在 _gate 内协作（getRawFileContent 路径本机实测单线程，
    // 但 MAUI 绑定判定回调并不保证仅 UI 线程）
    private static readonly object _gate = new();

    private static byte[]? GetRawfileBytesSync(string rawfilePath)
    {
        global::HarmonyOS.Bindings.Api.ResourceManagerObject? rm;
        lock (_gate)
        {
            // 惰性初始化只发生一次；裸 JS 句柄跨作用域会失效——Preferences 同款教训
            rm = _contextResourceManager ??= CreateResourceManager();
        }
        if (rm is null)
            return null;
        try
        {
            return rm.GetRawFileContentBytesSync(rawfilePath);
        }
        catch (Exception ex)
        {
            // 未暂存/不存在的资源属常态：降级 debug（handler 层回退 file:// 相对路径）
            HiLog.Debug("Image", $"rawfile miss for '{rawfilePath}': {ex.Message}");
            return null;
        }
    }

    private static global::HarmonyOS.Bindings.Api.ResourceManagerObject? CreateResourceManager()
    {
        try
        {
            var ctx = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext");
            if (ctx == IntPtr.Zero)
                return null;
            var handle = NodeApi.GetProperty(ctx, "resourceManager");
            if (handle == IntPtr.Zero)
                return null;
            return new global::HarmonyOS.Bindings.Api.ResourceManagerObject(handle);
        }
        catch (Exception ex)
        {
            HiLog.Debug("Image", $"resourceManager unavailable: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 流式来源 → 临时文件 URI（异步；可在任意线程 await）。
    /// 文件以 imgsrc_*.png 落入可写缓存目录；TempDir 探测时顺带清扫 24h 前的历史临时文件。
    /// </summary>
    public static async Task<string?> ResolveStreamAsync(StreamImageSource source)
    {
        try
        {
            var streamFunc = source.Stream;
            if (streamFunc is null)
                return null;
            var stream = await streamFunc(System.Threading.CancellationToken.None)
                .ConfigureAwait(false);
            if (stream is null)
                return null;

            var dir = TempDir;
            if (dir is null)
            {
                HiLog.Warn("Image", "No writable temp dir for stream image source");
                return null;
            }

            var path = Path.Combine(dir, $"{TempFilePrefix}{Guid.NewGuid():N}.png");
            await using var input = stream;
            using var file = File.Create(path);
            await input.CopyToAsync(file).ConfigureAwait(false);
            return $"file://{path}";
        }
        catch (Exception ex)
        {
            HiLog.Warn("Image", $"Stream image source failed: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 可写临时目录（C# 14 field 关键字：后备字段由编译器生成，探测结果缓存其中）。
    /// OHOS 沙盒下 /tmp 常不可写，逐个尝试；全部失败时返回 null（不缓存，下次重试）。
    /// </summary>
    private static string? TempDir
    {
        get
        {
            if (field is not null)
                return field;

            string[] candidates =
            [
                Path.GetTempPath(),
                "/data/storage/el2/base/haps/entry/cache",
                "/data/storage/el2/base/cache",
            ];
            foreach (var dir in candidates)
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    var probe = Path.Combine(dir, ".probe");
                    File.WriteAllText(probe, "1");
                    File.Delete(probe);
                    SweepStaleTempFiles(dir);
                    field = dir;
                    HiLog.Debug("Image", $"Temp dir for stream sources: {dir}");
                    return dir;
                }
                catch
                {
                    // 尝试下一个候选
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 清扫 24 小时前的 imgsrc_* 临时文件（临时文件无精确生命周期锚点，
    /// 以时间窗兜底防磁盘无限增长；仅在 TempDir 首次探测时执行一次）
    /// </summary>
    private static void SweepStaleTempFiles(string dir)
    {
        try
        {
            var cutoff = DateTimeOffset.UtcNow - TimeSpan.FromHours(24);
            foreach (var file in Directory.EnumerateFiles(dir, $"{TempFilePrefix}*.png"))
            {
                try
                {
                    if (File.GetLastWriteTimeUtc(file) < cutoff.UtcDateTime)
                        File.Delete(file);
                }
                catch { /* 占用中/无权限：跳过 */ }
            }
        }
        catch { /* 目录不可列：跳过 */ }
    }
}
