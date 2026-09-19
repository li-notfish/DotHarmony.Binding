using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api.File;
using HarmonyOS.Bindings.Api.Graphics;
using HarmonyOS.Bindings.Api.Multimedia;
using HarmonyOS.Bindings.Api.Util;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
// 生成的 Path 绑定与 System.IO.Path 同名，显式别名消歧
using Path = System.IO.Path;

namespace HarmonyOS.Maui.Essentials;

/// <summary>
/// MediaPicker：@ohos.file.picker（选图/选视频）+ @ohos.multimedia.camera.picker（拍照/录像）支撑。
/// 选图/选视频走 PhotoViewPicker.Select（MIMEType + maxSelectNumber 入参对象）；
/// 拍照/录像走 CameraPicker.Pick（ability 上下文 + PickerProfile）。
/// options.Title 暂无对应通道（OHOS 选择器不收标题入参，忽略）。
/// </summary>
internal class HarmonyMediaPicker : IMediaPicker
{
    public bool IsCaptureSupported => true;

    public async Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null)
    {
        var uris = await SelectAsync("image/*", 1);
        return uris.Count > 0 ? await ToFileResult(uris[0]) : null;
    }

    public async Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null)
        => await ToFileResults(await SelectAsync("image/*", MultiPickMax));

    public async Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null)
    {
        var uris = await SelectAsync("video/*", 1);
        return uris.Count > 0 ? await ToFileResult(uris[0]) : null;
    }

    public async Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null)
        => await ToFileResults(await SelectAsync("video/*", MultiPickMax));

    public async Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
        => await CaptureAsync(HarmonyOS.ArkUI.PickerMediaType.Photo);

    public async Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
        => await CaptureAsync(HarmonyOS.ArkUI.PickerMediaType.Video);

    /// <summary>PhotoViewPicker.Select → photoUris（maxSelectNumber=1 时单选）</summary>
    private static async Task<List<string>> SelectAsync(string mimeType, int maxSelectNumber)
    {
        // JsObject 无 finalizer：picker/options/result 全部显式 Dispose，不释放则 NapiReference 永久滞留
        using var picker = new PickerPhotoViewPicker();
        // 入参对象经 Dictionary 构造（选项包装类为只读视图，不可回填）
        var optionsPtr = NativeValue.From(new Dictionary<string, object?>
        {
            ["MIMEType"] = mimeType,
            ["maxSelectNumber"] = maxSelectNumber,
        });
        using var options = new PickerPhotoSelectOptions(optionsPtr);
        using var result = await picker.SelectAsync(options);
        return new List<string>(result.PhotoUris);
    }

    /// <summary>CameraPicker.Pick（ability 上下文）→ resultCode 0 成功 → resultUri</summary>
    private static async Task<FileResult?> CaptureAsync(HarmonyOS.ArkUI.PickerMediaType mediaType)
    {
        // cameraPosition 必填：POSITION_UNSPECIFIED=0（系统自选前后摄）
        var profilePtr = NativeValue.From(new Dictionary<string, object?>
        {
            ["cameraPosition"] = 0,
        });
        using var profile = new PickerProfile(profilePtr);
        var result = await CameraPicker.PickAsync(
            HarmonyPreferences.Context,
            new[] { mediaType },
            profile);
        if (result.ResultCode != 0)
            return null;
        return await ToFileResult(result.ResultUri);
    }

    private static async Task<List<FileResult>> ToFileResults(List<string> uris)
    {
        var results = new List<FileResult>(uris.Count);
        foreach (var uri in uris)
            results.Add(await ToFileResult(uri));
        return results;
    }

    /// <summary>
    /// picker URI → FileResult。picker 返回的 URI 是会话内临时授权（进程重启即失效），
    /// 复制到 cache 后以真实路径构造（FileResult.OpenRead 直接打开 FullPath）；
    /// 复制失败回退 URI 直接构造（向后兼容，读取期仍可能可用）。
    /// </summary>
    private static async Task<FileResult> ToFileResult(string uri)
    {
        var fileName = Path.GetFileName(uri);
        var mimeType = MimeTypeOf(fileName);
        try
        {
            var dest = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
            // fs.copyFile 的 src 只收沙箱路径/FD——picker 的 file:// URI 直传即 13900002（ENOENT）。
            // 官方模式：open(uri)（临时授权对 open 有效）拿 fd → copyFile(fd, dest)。
            // 生成的 Fs 绑定 src 塌缩为 string 联合成员，fd 重载未发射——经 NodeApi 直调同模块方法
            using var src = await Fs.OpenAsync(uri); // open 默认 READ_ONLY；FsFile.Dispose 只释放 napi 引用
            try
            {
                await NodeApi.CallMethodAsyncVoid(Fs.Module, "copyFile", src.Fd, dest);
            }
            finally
            {
                Fs.CloseSync(src.Fd); // OS fd 须显式关，否则句柄滞留至进程退出
            }
            HarmonyOS.Interop.HiLog.Debug("Essentials", $"MediaPicker copy: {uri} -> {dest}");
            return new FileResult(dest, mimeType);
        }
        catch (Exception ex)
        {
            HarmonyOS.Interop.HiLog.Warn("Essentials",
                $"MediaPicker cache copy failed, returning original uri: {ex.GetType().Name}: {ex.Message}");
            return new FileResult(uri, mimeType);
        }
    }

    private const int MultiPickMax = 50;

    private static string MimeTypeOf(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        ".heic" => "image/heic",
        ".mp4" => "video/mp4",
        ".mov" => "video/quicktime",
        ".3gp" => "video/3gpp",
        _ => "application/octet-stream",
    };
}
