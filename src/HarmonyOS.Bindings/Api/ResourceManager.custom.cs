// ResourceManager 手写扩展：getRawFileContent 的返回值是 JS Uint8Array（类型化数组），
// 生成包装的 ConvertArray 仅支持普通 Array（GetArrayElements 实测 napi_array_expected）。
// 字节载荷统一走 NativeValue.ToByteArray（TypedArray/ArrayBuffer 双通道）。
#nullable enable
using System.Threading.Tasks;
using HarmonyOS.Interop;

namespace HarmonyOS.Bindings.Api;

public sealed partial class ResourceManagerObject
{
    /// <summary>getRawFileContentSync 的字节载荷修正版（Uint8Array → byte[]）。</summary>
    public byte[] GetRawFileContentBytesSync(string path)
        => CallMethod(_getRawFileContentSync, static h => NativeValue.ToByteArray(h), path);

    /// <summary>getRawFileContent（Promise）的字节载荷修正版（Uint8Array → byte[]）。</summary>
    public Task<byte[]> GetRawFileContentBytesAsync(string path)
        => CallMethodAsync(_getRawFileContent, static h => NativeValue.ToByteArray(h), path);
}
