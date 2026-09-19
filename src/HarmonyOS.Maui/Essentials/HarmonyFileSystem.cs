// IFileSystem 鸿蒙实现：目录经 ability 上下文（filesDir/cacheDir）；包内文件经
// @ohos.resourceManager.getRawFileContent（rawfile 相对路径，HAP 的 resources/rawfile/）。
// file.fs（灰度）本接口不需要——四个成员全是目录与包内读取。
#nullable enable
using Microsoft.Maui.Storage;
using HarmonyOS.Interop;
using HResourceManager = HarmonyOS.Bindings.Api.ResourceManager;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyFileSystem : IFileSystem
{
    public string CacheDirectory => ReadContextPath("cacheDir");

    public string AppDataDirectory => ReadContextPath("filesDir");

    public async Task<Stream> OpenAppPackageFileAsync(string filename)
    {
        var rm = await HResourceManager.GetResourceManagerAsync();
        var bytes = await rm.GetRawFileContentAsync(filename);
        return new MemoryStream(bytes, writable: false);
    }

    public async Task<bool> AppPackageFileExistsAsync(string filename)
    {
        try
        {
            var rm = await HResourceManager.GetResourceManagerAsync();
            var bytes = await rm.GetRawFileContentAsync(filename);
            return bytes.Length > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static string ReadContextPath(string property)
    {
        var ctx = HarmonyPreferences.Context;
        return NativeValue.ToString(NodeApi.GetProperty(ctx, property)) ?? string.Empty;
    }
}
