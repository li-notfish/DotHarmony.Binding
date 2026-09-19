// IPreferences 鸿蒙实现：@ohos.data.preferences（模块已转正）。
// 每个共享容器对应一个 preferences 文件（懒打开、按 PinnedValue 持有）。
// 值存储采用"类型标签:载荷"字符串编码（b:/i:/f:/d:/s:/t:/o:）：
//   - OHOS number 是 double，MAUI long/DateTime.ToBinary() 超出 2^53 会丢精度——字符串编码规避；
//   - getSync/putSync 生成包装的 ValueType 签名被 distributedData 的同名枚举污染
//     （跨模块同名：真值是 number|string|boolean|... 联合别名），故这两个方法经
//     NodeApi 直调，HasSync/DeleteSync/ClearSync/FlushSync 包装干净直接用；
//   - 文件名：sharedName 为空 → maui_prefs；否则 maui_prefs.<sharedName>（已清洗非法字符）。
// 需要 ability 上下文（宿主 EntryAbility 已导出 globalThis.abilityContext）。
#nullable enable
using System.Globalization;
using Microsoft.Maui.Storage;
using HarmonyOS.Bindings.Runtime;
using HStaticPrefs = HarmonyOS.Bindings.Api.Data.Preferences;
using HPrefsObject = HarmonyOS.Bindings.Api.Data.PreferencesObject;
using HOptions = HarmonyOS.Bindings.Api.Data.PreferencesOptions;

namespace HarmonyOS.Maui.Essentials;

public class HarmonyPreferences : IPreferences
{
    const string FileNamePrefix = "maui_prefs";

    readonly Dictionary<string, HPrefsObject> _files = new(); // sharedName(或空串) → 已打开文件

    public HarmonyPreferences()
    {
        // 不缓存 context 裸句柄：Install（RootBuilder）到首次使用之间句柄会失效
        // （PinnedValue 同款问题，abilityContext 实测 "The context is invalid"）——每次用时重读
        if (NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8) == IntPtr.Zero)
            throw new InvalidOperationException(
                "host did not export globalThis.abilityContext; Preferences requires an ability context");
    }

    // internal：HarmonyAppInfo（RequestedTheme/ShowSettingsUI）复用同一 ability 上下文通道
    internal static IntPtr Context
    {
        get
        {
            var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
            if (context == IntPtr.Zero)
                throw new InvalidOperationException("globalThis.abilityContext became unavailable");
            return context;
        }
    }

    public bool ContainsKey(string key, string? sharedName = null)
    {
        var prefs = GetFile(sharedName);
        return prefs.HasSync(key);
    }

    public void Remove(string key, string? sharedName = null)
    {
        var prefs = GetFile(sharedName);
        prefs.DeleteSync(key);
        prefs.FlushSync();
    }

    public void Clear(string? sharedName = null)
    {
        var prefs = GetFile(sharedName);
        prefs.ClearSync();
        prefs.FlushSync();
    }

    public void Set<T>(string key, T value, string? sharedName = null)
    {
        var prefs = GetFile(sharedName);
        NodeApi.CallMethodVoid(prefs.PinnedValue, "putSync"u8, key, Encode(value));
        prefs.FlushSync();
    }

    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        var prefs = GetFile(sharedName);
        if (!prefs.HasSync(key))
            return defaultValue;
        // 缺失键也会走 getSync 返回空串，HasSync 先行保证空串只来自真实存储的空载荷
        var raw = NodeApi.CallMethod<string>(prefs.PinnedValue, "getSync"u8, key, string.Empty);
        return Decode(raw, defaultValue);
    }

    HPrefsObject GetFile(string? sharedName)
    {
        var fileKey = sharedName ?? string.Empty;
        if (_files.TryGetValue(fileKey, out var cached))
            return cached;
        var name = FileNamePrefix;
        if (sharedName is not null)
        {
            // 文件名约束：仅字母数字、下划线、点（不含 '/' 与控制字符）
            var safe = new string(sharedName.Select(c =>
                char.IsLetterOrDigit(c) || c is '_' or '.' ? c : '_').ToArray());
            name = $"{FileNamePrefix}.{safe}";
        }
        var prefs = HStaticPrefs.GetPreferencesSync(Context, new HOptions(name));
        _files[fileKey] = prefs;
        return prefs;
    }

    // ---- 类型标签编解码（internal 供单测；标签语义见文件头）----

    internal static string Encode<T>(T value) => value switch
    {
        null => throw new ArgumentNullException(nameof(value), "Preferences cannot store null"),
        bool b => "b:" + (b ? "1" : "0"),
        int i => "i:" + i.ToString(CultureInfo.InvariantCulture),
        long l => "i:" + l.ToString(CultureInfo.InvariantCulture),
        float f => "f:" + f.ToString("R", CultureInfo.InvariantCulture),
        double d => "d:" + d.ToString("R", CultureInfo.InvariantCulture),
        string s => "s:" + s,
        DateTime dt => "t:" + dt.ToBinary().ToString(CultureInfo.InvariantCulture),
        DateTimeOffset dto => "o:" + dto.ToString("O", CultureInfo.InvariantCulture),
        _ => throw new NotSupportedException(
            $"Preferences supports bool/int/long/float/double/string/DateTime/DateTimeOffset, got {typeof(T)}"),
    };

    internal static T Decode<T>(string raw, T defaultValue)
    {
        var idx = raw.IndexOf(':');
        var tag = idx < 0 ? string.Empty : raw[..idx];
        var payload = idx < 0 ? raw : raw[(idx + 1)..];

        // string 无标签（外部写入或空载荷）：按原始内容返回
        if (typeof(T) == typeof(string))
            return tag == "s" ? (T)(object)payload : (T)(object)raw;

        return tag switch
        {
            "b" => (T)(object)(payload == "1"),
            "i" => typeof(T) == typeof(int)
                ? (T)(object)(int)long.Parse(payload, CultureInfo.InvariantCulture)
                : (T)(object)long.Parse(payload, CultureInfo.InvariantCulture),
            "f" => (T)(object)float.Parse(payload, CultureInfo.InvariantCulture),
            "d" => (T)(object)double.Parse(payload, CultureInfo.InvariantCulture),
            "t" => (T)(object)DateTime.FromBinary(long.Parse(payload, CultureInfo.InvariantCulture)),
            "o" => (T)(object)DateTimeOffset.Parse(payload, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            _ => defaultValue, // 无标签/未知标签（外部写入的非 MAUI 数据）：按 MAUI 语义返回缺省
        };
    }
}
