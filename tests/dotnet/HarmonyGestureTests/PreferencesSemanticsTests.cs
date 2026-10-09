#nullable enable
using System;
using System.Collections.Generic;
using HarmonyOS.Essentials;
using Xunit;

namespace HarmonyGestureTests;

public class PreferencesSemanticsTests
{
    [Fact]
    public void Set_Null_Removes_Key()
    {
        var store = new FakePreferencesStore();
        store.Values["a"] = "s:value";
        store.Values["b"] = "s:other";

        var prefs = new HarmonyPreferences(_ => store);
        prefs.Set<string?>("a", null);

        Assert.False(prefs.ContainsKey("a"));
        Assert.True(prefs.ContainsKey("b"));
        Assert.Equal("other", prefs.Get("b", "missing"));
    }

    [Fact]
    public void Set_Null_Does_Not_Remove_Other_Keys()
    {
        var store = new FakePreferencesStore();
        store.Values["a"] = "s:one";
        store.Values["b"] = "s:two";
        store.Values["c"] = "s:three";

        var prefs = new HarmonyPreferences(_ => store);
        prefs.Set<string?>("b", null);

        Assert.Equal("one", prefs.Get("a", "missing"));
        Assert.Equal("three", prefs.Get("c", "missing"));
    }

    [Fact]
    public void Store_Is_Cached_Per_SharedName()
    {
        var created = 0;
        var store = new FakePreferencesStore();
        var prefs = new HarmonyPreferences(_ => { created++; return store; });

        prefs.Set("k", 1);
        prefs.Get("k", 0);
        prefs.ContainsKey("k");

        Assert.Equal(1, created);

        prefs.Remove("k", "other");

        Assert.Equal(2, created);
    }

    private sealed class FakePreferencesStore : IHarmonyPreferencesStore
    {
        public Dictionary<string, string> Values { get; } = new();

        public bool ContainsKey(string key) => Values.ContainsKey(key);

        public void Remove(string key) => Values.Remove(key);

        public void Clear() => Values.Clear();

        public void Set(string key, string encodedValue) => Values[key] = encodedValue;

        public string Get(string key, string defaultValue) =>
            Values.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
