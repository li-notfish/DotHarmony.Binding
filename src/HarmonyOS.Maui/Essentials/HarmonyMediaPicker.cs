using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Bindings.Runtime;
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
        return uris.Count > 0 ? ToFileResult(uris[0]) : null;
    }

    public async Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null)
        => (await SelectAsync("image/*", MultiPickMax)).Select(ToFileResult).ToList();

    public async Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null)
    {
        var uris = await SelectAsync("video/*", 1);
        return uris.Count > 0 ? ToFileResult(uris[0]) : null;
    }

    public async Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null)
        => (await SelectAsync("video/*", MultiPickMax)).Select(ToFileResult).ToList();

    public async Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
        => await CaptureAsync(HarmonyOS.ArkUI.PickerMediaType.Photo);

    public async Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
        => await CaptureAsync(HarmonyOS.ArkUI.PickerMediaType.Video);

    /// <summary>PhotoViewPicker.Select → photoUris（maxSelectNumber=1 时单选）</summary>
    private static async Task<List<string>> SelectAsync(string mimeType, int maxSelectNumber)
    {
        var picker = new PickerPhotoViewPicker();
        // 入参对象经 Dictionary 构造（选项包装类为只读视图，不可回填）
        var optionsPtr = NativeValue.From(new Dictionary<string, object?>
        {
            ["MIMEType"] = mimeType,
            ["maxSelectNumber"] = maxSelectNumber,
        });
        var result = await picker.SelectAsync(new PickerPhotoSelectOptions(optionsPtr));
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
        var result = await CameraPicker.PickAsync(
            HarmonyPreferences.Context,
            new[] { mediaType },
            new PickerProfile(profilePtr));
        if (result.ResultCode != 0)
            return null;
        return ToFileResult(result.ResultUri);
    }

    /// <summary>file:// uri → FileResult（MIME 由扩展名推断）</summary>
    private static FileResult ToFileResult(string uri)
    {
        var fileName = Path.GetFileName(uri);
        return new FileResult(uri, MimeTypeOf(fileName));
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
