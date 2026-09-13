// ISecureStorage 鸿蒙实现：@ohos.security.asset（应用内加密存储）。
// AssetMap 键为数字 Tag（SECRET=BYTES|0x01 / ALIAS=BYTES|0x02 / ACCESSIBILITY=NUMBER|0x03 /
// RETURN_TYPE=NUMBER|0x40 / CONFLICT_RESOLUTION=NUMBER|0x44），经 IDictionary 路径以字符串键构造；
// BYTES Tag 值要求 Uint8Array（Runtime 补 FromUint8Array），NUMBER Tag 值为 number。
// ACCESSIBILITY=DEVICE_FIRST_UNLOCKED(1)；CONFLICT_RESOLUTION=OVERWRITE(0)；RETURN_TYPE=ALL(0)。
#nullable enable
using Microsoft.Maui.Storage;
using HAsset = HarmonyOS.Bindings.Api.Asset;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public class HarmonySecureStorage : ISecureStorage
{
    const string TagSecret = "805306369";              // BYTES|0x01
    const string TagAlias = "805306370";               // BYTES|0x02
    const string TagAccessibility = "536870915";       // NUMBER|0x03
    const string TagReturnType = "536870976";          // NUMBER|0x40
    const string TagConflictResolution = "536870980";  // NUMBER|0x44

    const double AccessibilityFirstUnlocked = 1.0; // DEVICE_FIRST_UNLOCKED
    const double ConflictOverwrite = 0.0;          // OVERWRITE
    const double ReturnAll = 0.0;                  // ALL

    public Task SetAsync(string key, string value)
    {
        HAsset.AddSync(NativeValue.From(BuildAddMap(key, value)));
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        var query = new Dictionary<string, object?> { [TagAlias] = Alias(key) };
        // PreQuery 返回每资产 1 字节存在标志（0 = 不存在）
        var flags = HAsset.PreQuerySync(NativeValue.From(query));
        if (flags.Length == 0 || flags[0] == 0)
            return Task.FromResult<string?>(null);

        var fullQuery = new Dictionary<string, object?>
        {
            [TagAlias] = Alias(key),
            [TagReturnType] = ReturnAll,
        };
        var results = HAsset.QuerySync(NativeValue.From(fullQuery));
        if (results.Length == 0)
            return Task.FromResult<string?>(null);

        var secret = NodeApi.GetProperty(results[0], TagSecret);
        if (secret == IntPtr.Zero)
            return Task.FromResult<string?>(null);
        return Task.FromResult<string?>(System.Text.Encoding.UTF8.GetString(NativeValue.ToByteArray(secret)));
    }

    public bool Remove(string key)
    {
        try
        {
            HAsset.RemoveSync(NativeValue.From(new Dictionary<string, object?> { [TagAlias] = Alias(key) }));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void RemoveAll()
    {
        // 空查询：移除本应用全部资产
        HAsset.RemoveSync(NativeValue.From(new Dictionary<string, object?>()));
    }

    internal static Dictionary<string, object?> BuildAddMap(string key, string value)
    {
        var map = new Dictionary<string, object?>
        {
            [TagAccessibility] = AccessibilityFirstUnlocked,
            [TagConflictResolution] = ConflictOverwrite,
        };
        map[TagAlias] = Alias(key);
        map[TagSecret] = NativeValue.FromUint8Array(System.Text.Encoding.UTF8.GetBytes(value));
        return map;
    }

    // BYTES Tag 值要求 Uint8Array（From(byte[]) 的 ArrayBuffer 不被 asset 接受）
    static IntPtr Alias(string key) =>
        NativeValue.FromUint8Array(System.Text.Encoding.UTF8.GetBytes(key));
}
