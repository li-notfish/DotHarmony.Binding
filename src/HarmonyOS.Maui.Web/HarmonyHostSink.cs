#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

/// <summary>
/// libentry.so C 回写通道：NDK scheme 拦截的响应完成（DidReceiveResponse/Data/Finish）。
/// 与 MauiHarmonyHost 同款寻址约定：NativeLibrary.TryGetExport + 函数指针（不经 LibraryImport，
/// 避免弱宿主导出缺失时静态 P/Invoke 直接崩）。
/// </summary>
internal static unsafe class HarmonyHostSink
{
    private static delegate* unmanaged[Cdecl]<int, int, sbyte*, sbyte*, byte*, long, void> _respond;
    private static delegate* unmanaged[Cdecl]<int, void> _fail;
    private static bool _resolved;

    private static void Resolve()
    {
        if (_resolved) return;
        _resolved = true;
        if (!NativeLibrary.TryLoad("libentry.so", out var lib))
        {
            HiLog.Warn("HarmonyWeb", "[scheme] libentry.so not loaded; native respond unavailable");
            return;
        }
        if (NativeLibrary.TryGetExport(lib, "HarmonyHostWebRespond", out var respond) &&
            NativeLibrary.TryGetExport(lib, "HarmonyHostWebRespondFail", out var fail))
        {
            _respond = (delegate* unmanaged[Cdecl]<int, int, sbyte*, sbyte*, byte*, long, void>)respond;
            _fail = (delegate* unmanaged[Cdecl]<int, void>)fail;
        }
        else
        {
            HiLog.Warn("HarmonyWeb", "[scheme] HarmonyHostWebRespond* export missing (old host)");
        }
    }

    public static void Respond(int requestId, int status, string statusText, string mime, ReadOnlySpan<byte> data)
    {
        Resolve();
        if (_respond == null)
        {
            HiLog.Error("HarmonyWeb", $"[scheme] respond dropped (no sink): id={requestId}");
            return;
        }

        var statusBytes = ToUtf8(statusText, out _);
        var mimeBytes = ToUtf8(mime, out _);
        RespondCore(statusBytes, mimeBytes, data);

        void RespondCore(ReadOnlySpan<byte> st, ReadOnlySpan<byte> mt, ReadOnlySpan<byte> body)
        {
            fixed (byte* pStatus = st, pMime = mt)
            fixed (byte* pData = body)
            {
                _respond(requestId, status, (sbyte*)pStatus, (sbyte*)pMime, pData, body.Length);
            }
        }
    }

    public static void Fail(int requestId)
    {
        Resolve();
        if (_fail is null)
        {
            HiLog.Error("HarmonyWeb", $"[scheme] fail dropped (no sink): id={requestId}");
            return;
        }
        _fail(requestId);
    }

    // UTF8 + NUL 终止：此类字符串只有 statusText/mime，短小字段，直接一次性数组即可
    private static byte[] ToUtf8(string value, out int _)
    {
        _ = 0;
        var buf = new byte[Encoding.UTF8.GetByteCount(value) + 1];
        Encoding.UTF8.GetBytes(value, buf);
        return buf;
    }
}
