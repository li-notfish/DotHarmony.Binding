#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using HarmonyOS.Interop;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace HarmonyOS.Maui.Blazor;

// The official IpcReceiver deserializes every incoming payload as JsonElement[].
// NativeAOT does not retain that generic array converter, so this adapter parses
// the same __bwv wire format without generic JSON serializer reflection.
internal static class HarmonyIpcReceiver
{
    private const string Prefix = "__bwv:";
    private const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static readonly FieldInfo CurrentPageContextField =
        typeof(WebViewManager).GetField("_currentPageContext", MemberFlags)
        ?? throw new MissingFieldException(typeof(WebViewManager).FullName, "_currentPageContext");

    private static readonly MethodInfo AttachToPageMethod =
        typeof(WebViewManager).GetMethod("AttachToPageAsync", MemberFlags, null, new[] { typeof(string), typeof(string) }, null)
        ?? throw new MissingMethodException(typeof(WebViewManager).FullName, "AttachToPageAsync");

    public static void Dispatch(HarmonyBlazorWebViewManager manager, string message)
    {
        if (!TryParseMessage(message, out var messageType, out var document, out var arguments))
            return;

        // 任何 __bwv 消息即证明 blazor.webview.js 已完成初始化——供启动重试的自确认
        manager.NoteBlazorIpc();

        _ = manager.Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                await HandleMessageAsync(manager, messageType, document.RootElement, arguments).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HiLog.Error("HarmonyWeb",
                    $"[harmony-ipc] {messageType} failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                document.Dispose();
            }
        });
    }

    private static bool TryParseMessage(
        string message,
        out string messageType,
        out JsonDocument document,
        out JsonElement arguments)
    {
        messageType = string.Empty;
        document = null!;
        arguments = default;

        if (string.IsNullOrEmpty(message) || !message.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        var payload = System.Text.Encoding.UTF8.GetBytes(message[Prefix.Length..]);
        var reader = new Utf8JsonReader(payload);
        var parsed = JsonDocument.ParseValue(ref reader);
        if (parsed.RootElement.ValueKind != JsonValueKind.Array || parsed.RootElement.GetArrayLength() == 0)
        {
            parsed.Dispose();
            return false;
        }

        messageType = parsed.RootElement[0].GetString() ?? string.Empty;
        document = parsed;
        arguments = parsed.RootElement;
        return true;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075")]
    private static async Task HandleMessageAsync(
        HarmonyBlazorWebViewManager manager,
        string messageType,
        JsonElement root,
        JsonElement arguments)
    {
        switch (messageType)
        {
            case "AttachPage":
                if (arguments.GetArrayLength() < 2)
                    throw new InvalidOperationException("AttachPage requires baseUrl and startUrl.");
                await InvokeAttachToPageAsync(manager, arguments[1].GetString() ?? string.Empty, arguments[2].GetString() ?? string.Empty);
                return;
            case "BeginInvokeDotNet":
                if (arguments.GetArrayLength() < 6)
                    throw new InvalidOperationException("BeginInvokeDotNet requires five arguments.");
                var jsRuntime = GetPageService(manager, "JSRuntime");
                var invocationInfo = new DotNetInvocationInfo(
                    arguments[2].GetString(),
                    arguments[3].GetString()
                        ?? throw new InvalidOperationException("BeginInvokeDotNet missing methodIdentifier."),
                    arguments[4].GetInt64(),
                    arguments[1].GetString());
                DotNetDispatcher.BeginInvokeDotNet(
                    (JSRuntime)jsRuntime,
                    invocationInfo,
                    arguments[5].GetString() ?? "null");
                return;
            case "EndInvokeJS":
                if (arguments.GetArrayLength() < 4)
                    throw new InvalidOperationException("EndInvokeJS requires three arguments.");
                DotNetDispatcher.EndInvokeJS(
                    (JSRuntime)GetPageService(manager, "JSRuntime"),
                    arguments[3].GetString() ?? string.Empty);
                return;
            case "ReceiveByteArrayFromJS":
                if (arguments.GetArrayLength() < 3)
                    throw new InvalidOperationException("ReceiveByteArrayFromJS requires two arguments.");
                DotNetDispatcher.ReceiveByteArray(
                    (JSRuntime)GetPageService(manager, "JSRuntime"),
                    arguments[1].GetInt32(),
                    arguments[2].GetBytesFromBase64());
                return;
            case "OnRenderCompleted":
                if (arguments.GetArrayLength() < 3)
                    throw new InvalidOperationException("OnRenderCompleted requires two arguments.");
                if (arguments[2].ValueKind == JsonValueKind.String)
                    throw new InvalidOperationException(arguments[2].GetString());
                InvokePageMember(manager, "Renderer", "NotifyRenderCompleted", arguments[1].GetInt64());
                return;
            case "OnLocationChanged":
                if (arguments.GetArrayLength() < 4)
                    throw new InvalidOperationException("OnLocationChanged requires three arguments.");
                InvokePageMember(
                    manager,
                    "NavigationManager",
                    "LocationUpdated",
                    arguments[1].GetString() ?? string.Empty,
                    arguments[2].ValueKind == JsonValueKind.Null ? null : arguments[2].GetString(),
                    arguments[3].GetBoolean());
                return;
            case "OnLocationChanging":
                if (arguments.GetArrayLength() < 5)
                    throw new InvalidOperationException("OnLocationChanging requires four arguments.");
                InvokePageMember(
                    manager,
                    "NavigationManager",
                    "HandleLocationChanging",
                    arguments[1].GetInt32(),
                    arguments[2].GetString() ?? string.Empty,
                    arguments[3].ValueKind == JsonValueKind.Null ? null : arguments[3].GetString(),
                    arguments[4].GetBoolean());
                return;
            default:
                return;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075")]
    private static Task InvokeAttachToPageAsync(HarmonyBlazorWebViewManager manager, string baseUrl, string startUrl)
    {
        var task = AttachToPageMethod.Invoke(manager, new object?[] { baseUrl, startUrl });
        return task switch
        {
            Task typedTask => typedTask,
            _ => Task.CompletedTask,
        };
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075")]
    private static object GetPageService(HarmonyBlazorWebViewManager manager, string propertyName)
    {
        var pageContext = CurrentPageContextField.GetValue(manager)
            ?? throw new InvalidOperationException("Cannot receive IPC messages when no page is attached.");
        var property = pageContext.GetType().GetProperty(propertyName, MemberFlags)
            ?? throw new MissingMemberException(pageContext.GetType().FullName, propertyName);
        return property.GetValue(pageContext)
            ?? throw new InvalidOperationException($"Blazor page property {propertyName} is not available.");
    }

    [UnconditionalSuppressMessage("Trimming", "IL2075")]
    private static void InvokePageMember(HarmonyBlazorWebViewManager manager, string propertyName, string methodName, params object?[] arguments)
    {
        var pageContext = CurrentPageContextField.GetValue(manager)
            ?? throw new InvalidOperationException("Cannot receive IPC messages when no page is attached.");
        var property = pageContext.GetType().GetProperty(propertyName, MemberFlags)
            ?? throw new MissingMemberException(pageContext.GetType().FullName, propertyName);
        var target = property.GetValue(pageContext)
            ?? throw new InvalidOperationException($"Blazor page property {propertyName} is not available.");
        var method = target.GetType().GetMethod(methodName, MemberFlags)
            ?? throw new MissingMethodException(target.GetType().FullName, methodName);
        method.Invoke(target, arguments);
    }
}
