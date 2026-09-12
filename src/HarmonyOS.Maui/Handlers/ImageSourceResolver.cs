using HarmonyOS.Bindings.Runtime;
using Microsoft.Maui;
using System.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// IImageSource → ArkUI NODE_IMAGE_SRC 字符串（URI）的解析器。
/// ArkUI C API 的 NODE_IMAGE_SRC 只接受 URI 字符串或 DrawableDescriptor，
/// 流式来源落盘为临时文件再以 file:// 提供（M1；像素级 PixelMap 通道后续经 image native 模块接入）。
/// </summary>
internal static class ImageSourceResolver
{

    public static string? Resolve(IImageSource? source) => source switch
    {
        UriImageSource uri => uri.Uri?.ToString(),
        FileImageSource file => ResolveFile(file.File),
        StreamImageSource stream => ResolveStream(stream),
        _ => null, // FontImageSource 等：无对应通道，记为 gap
    };

    private static string? ResolveFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        // 已带 scheme（file:// / http(s):// / data:）原样返回；纯路径视为本地绝对路径
        return path.Contains("://") ? path : $"file://{path}";
    }

    private static string? ResolveStream(StreamImageSource source)
    {
        try
        {
            using var stream = source.Stream?.Invoke(System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            if (stream is null)
                return null;

            var dir = TempDir;
            if (dir is null)
            {
                HiLog.Warn("Image", "No writable temp dir for stream image source");
                return null;
            }

            var path = Path.Combine(dir, $"imgsrc_{Guid.NewGuid():N}.png");
            using (var file = File.Create(path))
                ((System.IO.Stream)stream).CopyTo(file);
            return $"file://{path}";
        }
        catch (Exception ex)
        {
            HiLog.Warn("Image", $"Stream image source failed: {ex.Message}");
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
}
