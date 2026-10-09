// IPreferences HarmonyOS implementation backed by @ohos.data.preferences.
// Values use a "type-tag:payload" string format to keep long and date precision intact.
// Storing null removes the key, matching the official MAUI Preferences behavior.
#nullable enable
using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Storage;
using HarmonyOS.Interop;
using HStaticPrefs = HarmonyOS.Bindings.Api.Data.Preferences;
using HPrefsObject = HarmonyOS.Bindings.Api.Data.PreferencesObject;
using HOptions = HarmonyOS.Bindings.Api.Data.PreferencesOptions;

namespace HarmonyOS.Essentials;

internal interface IHarmonyPreferencesStore
{
    bool ContainsKey(string key);
    void Remove(string key);
    void Clear();
    void Set(string key, string encodedValue);
    string Get(string key, string defaultValue);
}

public class HarmonyPreferences : IPreferences
{
    private const string FileNamePrefix = "maui_prefs";
    private readonly Func<string?, IHarmonyPreferencesStore> _storeFactory;

    // sharedName(或空串) → 已打开文件；按名懒缓存，避免每次操作重开 preferences
    // 文件并堆积未释放的 napi 句柄（PinnedValue 固定引用）。缓存存续期与进程一致，
    // 与 62379d2 前的按文件懒打开行为对齐。
    private readonly ConcurrentLazyCache<string, IHarmonyPreferencesStore> _stores = new();

    public HarmonyPreferences()
        : this(CreateDefaultStore)
    {
        if (NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8) == IntPtr.Zero)
            throw new InvalidOperationException(
                "host did not export globalThis.abilityContext; Preferences requires an ability context");
    }

    internal HarmonyPreferences(Func<string?, IHarmonyPreferencesStore> storeFactory)
    {
        _storeFactory = storeFactory ?? throw new ArgumentNullException(nameof(storeFactory));
    }

    private IHarmonyPreferencesStore GetStore(string? sharedName)
        => _stores.GetOrAdd(sharedName ?? string.Empty, _ => _storeFactory(sharedName));

    private static IHarmonyPreferencesStore CreateDefaultStore(string? sharedName)
    {
        var name = FileNamePrefix;
        if (sharedName is not null)
        {
            var safe = new string(sharedName.Select(c =>
                char.IsLetterOrDigit(c) || c is '_' or '.' ? c : '_').ToArray());
            name = $"{FileNamePrefix}.{safe}.{StableHash(sharedName):x8}";
        }

        var prefs = HStaticPrefs.GetPreferencesSync(Context, new HOptions(name));
        return new HarmonyPreferencesStore(prefs);
    }

    private sealed class HarmonyPreferencesStore : IHarmonyPreferencesStore
    {
        private readonly HPrefsObject _prefs;

        public HarmonyPreferencesStore(HPrefsObject prefs) => _prefs = prefs;

        public bool ContainsKey(string key) => _prefs.HasSync(key);

        public void Remove(string key)
        {
            _prefs.DeleteSync(key);
            _prefs.FlushSync();
        }

        public void Clear()
        {
            _prefs.ClearSync();
            _prefs.FlushSync();
        }

        public void Set(string key, string encodedValue)
        {
            NodeApi.CallMethodVoid(_prefs.PinnedValue, "putSync"u8, key, encodedValue);
            _prefs.FlushSync();
        }

        public string Get(string key, string defaultValue)
        {
            if (!_prefs.HasSync(key))
                return defaultValue;
            return NodeApi.CallMethod<string>(_prefs.PinnedValue, "getSync"u8, key, string.Empty);
        }
    }

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
        => GetStore(sharedName).ContainsKey(key);

    public void Remove(string key, string? sharedName = null)
        => GetStore(sharedName).Remove(key);

    public void Clear(string? sharedName = null)
        => GetStore(sharedName).Clear();

    public void Set<T>(string key, T value, string? sharedName = null)
    {
        if (value is null)
        {
            Remove(key, sharedName);
            return;
        }

        GetStore(sharedName).Set(key, Encode(value));
    }

    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        var store = GetStore(sharedName);
        if (!store.ContainsKey(key))
            return defaultValue;

        var raw = store.Get(key, string.Empty);
        return Decode(raw, defaultValue);
    }

    internal static uint StableHash(string s)
    {
        unchecked
        {
            uint h = 2166136261;
            foreach (var b in System.Text.Encoding.UTF8.GetBytes(s))
                h = (h ^ b) * 16777619;
            return h;
        }
    }

    internal static string Encode<T>(T value) => value switch
    {
        null => throw new ArgumentNullException(nameof(value), "Preferences cannot encode null"),
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

        if (typeof(T) == typeof(string))
            return tag == "s" ? (T)(object)payload : (T)(object)raw;

        try
        {
            return tag switch
            {
                "b" when typeof(T) == typeof(bool) && payload is "1" or "0" =>
                    (T)(object)(payload == "1"),
                "i" when typeof(T) == typeof(int) &&
                    long.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) &&
                    l is >= int.MinValue and <= int.MaxValue => (T)(object)(int)l,
                "i" when typeof(T) == typeof(long) &&
                    long.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) =>
                    (T)(object)l,
                "f" when typeof(T) == typeof(float) &&
                    float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) =>
                    (T)(object)f,
                "d" when typeof(T) == typeof(double) &&
                    double.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) =>
                    (T)(object)d,
                "t" when typeof(T) == typeof(DateTime) &&
                    long.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ticks) =>
                    (T)(object)DateTime.FromBinary(ticks),
                "o" when typeof(T) == typeof(DateTimeOffset) &&
                    DateTimeOffset.TryParse(payload, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto) =>
                    (T)(object)dto,
                _ => defaultValue,
            };
        }
        catch (FormatException)
        {
            return defaultValue;
        }
        catch (OverflowException)
        {
            return defaultValue;
        }
    }
}
