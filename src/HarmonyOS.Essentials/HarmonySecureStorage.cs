// ISecureStorage 鸿蒙实现：@ohos.security.asset（应用内加密存储）。
// AssetMap = Map<Tag, Value>——真正的 JS Map（普通对象被拒，实测 "Expect Map type."），
// 键为数字 Tag（SECRET=BYTES|0x01 / ALIAS=BYTES|0x02 / ACCESSIBILITY=NUMBER|0x03 /
// RETURN_TYPE=NUMBER|0x40 / CONFLICT_RESOLUTION=NUMBER|0x44），经 NativeValue.FromMap 构造；
// BYTES Tag 值要求 Uint8Array（NativeValue.FromUint8Array），NUMBER Tag 值为 number。
// ACCESSIBILITY=DEVICE_FIRST_UNLOCKED(1)；CONFLICT_RESOLUTION=OVERWRITE(0)；RETURN_TYPE=ALL(0)。
#nullable enable
using Microsoft.Maui.Storage;
using HAsset = HarmonyOS.Bindings.Api.Security.Asset;
using HarmonyOS.Interop;

namespace HarmonyOS.Essentials;

public class HarmonySecureStorage : ISecureStorage
{
    const double TagSecret = 805306369;              // BYTES|0x01
    const double TagAlias = 805306370;               // BYTES|0x02
    const double TagAccessibility = 536870915;       // NUMBER|0x03
    const double TagReturnType = 536870976;          // NUMBER|0x40
    const double TagConflictResolution = 536870980;  // NUMBER|0x44

    const double AccessibilityFirstUnlocked = 1.0; // DEVICE_FIRST_UNLOCKED
    const double ConflictOverwrite = 0.0;          // OVERWRITE
    const double ReturnAll = 0.0;                  // ALL

    public Task SetAsync(string key, string value)
    {
        HAsset.AddSync(BuildAddMap(key, value));
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        // 不走 preQuery/postQuery——那是用户认证流程的 challenge（非存在性检查），
        // 本实现资产无认证要求；querySync 无结果时直接抛 not found（捕获 → null）
        IntPtr[] results;
        try
        {
            results = HAsset.QuerySync(QueryMap((TagAlias, Alias(key)), (TagReturnType, Num(ReturnAll))));
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return Task.FromResult<string?>(null);
        }
        if (results.Length == 0)
            return Task.FromResult<string?>(null);

        var secret = NativeValue.GetMapped(results[0], TagSecret);
        if (secret == IntPtr.Zero)
            return Task.FromResult<string?>(null);
        return Task.FromResult<string?>(System.Text.Encoding.UTF8.GetString(NativeValue.ToByteArray(secret)));
    }

    public bool Remove(string key)
    {
        try
        {
            HAsset.RemoveSync(QueryMap((TagAlias, Alias(key))));
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
        HAsset.RemoveSync(QueryMap());
    }

    internal static IntPtr BuildAddMap(string key, string value) => NativeValue.FromMap(
        (TagAccessibility, Num(AccessibilityFirstUnlocked)),
        (TagConflictResolution, Num(ConflictOverwrite)),
        (TagAlias, Alias(key)),
        (TagSecret, Alias(value)));

    static IntPtr QueryMap(params (double Key, IntPtr Value)[] entries) => NativeValue.FromMap(entries);

    static IntPtr Num(double value) => NativeValue.From(value);

    // BYTES Tag 值要求 Uint8Array（From(byte[]) 的 ArrayBuffer 不被 asset 接受）
    static IntPtr Alias(string key) =>
        NativeValue.FromUint8Array(System.Text.Encoding.UTF8.GetBytes(key));
}
