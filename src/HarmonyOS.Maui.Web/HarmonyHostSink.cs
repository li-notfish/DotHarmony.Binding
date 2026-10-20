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
    private static delegate* unmanaged[Cdecl]<int, int, sbyte*, sbyte*, void> _begin;
    private static delegate* unmanaged[Cdecl]<int, byte*, long, void> _write;
    private static delegate* unmanaged[Cdecl]<int, void> _finish;
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
            if (NativeLibrary.TryGetExport(lib, "HarmonyHostWebRespondBegin", out var begin) &&
                NativeLibrary.TryGetExport(lib, "HarmonyHostWebRespondWrite", out var write) &&
                NativeLibrary.TryGetExport(lib, "HarmonyHostWebRespondFinish", out var finish))
            {
                _begin = (delegate* unmanaged[Cdecl]<int, int, sbyte*, sbyte*, void>)begin;
                _write = (delegate* unmanaged[Cdecl]<int, byte*, long, void>)write;
                _finish = (delegate* unmanaged[Cdecl]<int, void>)finish;
            }
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

    /// <summary>是否支持流式响应（宿主导出齐全）。</summary>
    public static bool SupportsStreaming
    {
        get { Resolve(); return _begin != null && _write != null && _finish != null; }
    }

    /// <summary>流式写响应：分块读取内容流，避免大文件全量进内存。</summary>
    public static unsafe bool RespondStreaming(int requestId, int status, string statusText, string mime, Stream content)
    {
        Resolve();
        if (_begin == null || _write == null || _finish == null)
            return false;

        var statusBytes = ToUtf8(statusText);
        var mimeBytes = ToUtf8(mime);
        fixed (byte* pStatus = statusBytes, pMime = mimeBytes)
        {
            _begin(requestId, status, (sbyte*)pStatus, (sbyte*)pMime);
        }

        const int ChunkSize = 64 * 1024;
        var buffer = new byte[ChunkSize];
        try
        {
            int read;
            while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
            {
                fixed (byte* p = buffer)
                {
                    _write(requestId, p, read);
                }
            }
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyWeb", $"[scheme] stream write failed: {ex.GetType().Name}: {ex.Message}");
            _fail(requestId);
            return true;
        }

        _finish(requestId);
        return true;
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
    private static byte[] ToUtf8(string value)
    {
        var buf = new byte[Encoding.UTF8.GetByteCount(value) + 1];
        Encoding.UTF8.GetBytes(value, buf);
        return buf;
    }

    private static byte[] ToUtf8(string value, out int _)
    {
        _ = 0;
        var buf = new byte[Encoding.UTF8.GetByteCount(value) + 1];
        Encoding.UTF8.GetBytes(value, buf);
        return buf;
    }
}
