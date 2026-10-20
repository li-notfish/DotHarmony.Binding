#nullable enable
using System;
using System.Runtime.InteropServices;
using HarmonyOS.Interop;

namespace HarmonyOS.Maui.Web;

/// <summary>
/// libentry.so 的「按需创建 Web 实例」通道：surface 缺失时请求宿主新建一个 BuilderNode。
/// 与 <see cref="HarmonyHostSink"/> 同款寻址约定（TryGetExport + 函数指针），
/// 避免弱宿主导出缺失时静态 P/Invoke 直接崩。
/// </summary>
internal static unsafe class HarmonyWebViewFactorySink
{
    private static delegate* unmanaged[Cdecl]<sbyte*, int> _createView;
    private static bool _resolved;

    private static void Resolve()
    {
        if (_resolved) return;
        _resolved = true;
        if (!NativeLibrary.TryLoad("libentry.so", out var lib))
        {
            HiLog.Warn("HarmonyWeb", "[factory] libentry.so not loaded; cannot request new web views");
            return;
        }
        if (NativeLibrary.TryGetExport(lib, "HarmonyHostWebCreateView", out var create))
            _createView = (delegate* unmanaged[Cdecl]<sbyte*, int>)create;
        else
            HiLog.Warn("HarmonyWeb", "[factory] HarmonyHostWebCreateView export missing (old host)");
    }

    /// <summary>是否可用（宿主提供了按需创建能力）。</summary>
    public static bool IsAvailable
    {
        get { Resolve(); return _createView != null; }
    }

    /// <summary>请求宿主为一个 webTag 创建 Web 实例。成功返回 true。</summary>
    public static bool TryCreate(string webTag)
    {
        if (string.IsNullOrEmpty(webTag))
            return false;
        Resolve();
        if (_createView == null)
            return false;

        var utf8 = System.Text.Encoding.UTF8.GetBytes(webTag + "\0");
        fixed (byte* p = utf8)
        {
            var status = _createView((sbyte*)p);
            if (status == 0)
            {
                HiLog.Info("HarmonyWeb", $"[factory] host created web view for tag '{webTag}'");
                return true;
            }
            HiLog.Error("HarmonyWeb", $"[factory] host failed to create web view for tag '{webTag}': {status}");
            return false;
        }
    }
}
