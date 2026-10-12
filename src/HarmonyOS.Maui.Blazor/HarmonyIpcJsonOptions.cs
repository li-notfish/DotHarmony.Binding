#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using HarmonyOS.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.JSInterop;

namespace HarmonyOS.Maui.Blazor;

internal static class HarmonyIpcJsonOptions
{
    private static bool _configured;
    private static readonly object Sync = new();

    public static void Configure()
    {
        if (_configured)
            return;

        lock (Sync)
        {
            if (_configured)
                return;
            try
            {
                var options = GetProviderOptions();
                if (options.IsReadOnly)
                {
                    // options 已被首次序列化锁定：converter 插入与 TypeInfoResolver 赋值均会抛/无效。
                    // 必须在首次 Blazor IPC 之前完成配置——记录硬错误而不是静默降级。
                    HiLog.Error("HarmonyWeb",
                        "[blazor-ipc] JsonSerializerOptionsProvider.Options is already read-only; "
                        + "call UseHarmonyBlazorWebView() before any Blazor serialization occurs.");
                    return;
                }
                if (!options.Converters.OfType<HarmonyJsonElementArrayConverter>().Any())
                    options.Converters.Insert(0, new HarmonyJsonElementArrayConverter());
                if (!options.Converters.OfType<HarmonyJSCallResultTypeConverter>().Any())
                    options.Converters.Insert(0, new HarmonyJSCallResultTypeConverter());
                if (!options.Converters.OfType<HarmonyNavigationOptionsConverter>().Any())
                    options.Converters.Insert(0, new HarmonyNavigationOptionsConverter());
                options.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                    HarmonyIpcJsonContext.Default,
                    options.TypeInfoResolver);
                _configured = true;
            }
            catch (Exception ex)
            {
                // 不置位 _configured：下次调用重试并再次上报，避免一次性吞掉配置失败
                HiLog.Error("HarmonyWeb", $"[blazor-ipc] configure failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    private static JsonSerializerOptions GetProviderOptions()
    {
        var providerType = Type.GetType(
            "Microsoft.AspNetCore.Components.JsonSerializerOptionsProvider, Microsoft.AspNetCore.Components",
            throwOnError: true);
        var field = providerType!.GetField("Options", BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingFieldException(providerType.FullName, "Options");
        return (JsonSerializerOptions)field.GetValue(null)!;
    }
}

// IpcCommon.Serialize passes these values as runtime-typed object arguments. NativeAOT
// cannot reliably synthesize their reflection-backed converters, so register them
// explicitly on the WebView's shared JsonSerializerOptions.
internal sealed class HarmonyJSCallResultTypeConverter : JsonConverter<JSCallResultType>
{
    public override JSCallResultType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => (JSCallResultType)reader.GetInt32();

    public override void Write(
        Utf8JsonWriter writer,
        JSCallResultType value,
        JsonSerializerOptions options)
        => writer.WriteNumberValue((int)value);
}

internal sealed class HarmonyNavigationOptionsConverter : JsonConverter<NavigationOptions>
{
    public override NavigationOptions Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => throw new NotSupportedException(
            "NavigationOptions is only serialized in outgoing WebView IPC messages.");

    public override void Write(
        Utf8JsonWriter writer,
        NavigationOptions value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("forceLoad", value.ForceLoad);
        writer.WriteBoolean("replaceHistoryEntry", value.ReplaceHistoryEntry);
        if (value.HistoryEntryState is null)
            writer.WriteNull("historyEntryState");
        else
            writer.WriteString("historyEntryState", value.HistoryEntryState);
        writer.WriteEndObject();
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonElement[]))]
[JsonSerializable(typeof(JSCallResultType))]
[JsonSerializable(typeof(NavigationOptions))]
internal sealed partial class HarmonyIpcJsonContext : JsonSerializerContext
{
}

internal sealed class HarmonyJsonElementArrayConverter : JsonConverter<JsonElement[]>
{
    public override JsonElement[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Blazor IPC payload must be an array.");

        return document.RootElement.EnumerateArray().Select(element => element.Clone()).ToArray();
    }

    public override void Write(Utf8JsonWriter writer, JsonElement[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var element in value)
            element.WriteTo(writer);
        writer.WriteEndArray();
    }
}
