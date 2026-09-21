using HarmonyOS.Interop;
using Microsoft.Maui;
using System.Text;

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
        // 已带 scheme（file:// / http(s):// / data:）原样返回；纯路径视为本地绝对路径
        return path.Contains("://") ? path : $"file://{path}";
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
