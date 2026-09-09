using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// hilog 原生日志（libhilog_ndk.z.so）。
/// OH_LOG_Print 是 C 变参函数；对 %{public}s 单字符串场景，
/// arm64/x64 上以固定签名调用变参函数是安全的（前几个参数均走整型/指针寄存器）。
/// </summary>
public static unsafe partial class HiLog
{
    private const string Lib = "libhilog_ndk.z.so";

    // LOG_APP = 0；LOG_DEBUG = 3；LOG_INFO = 4；LOG_WARN = 5；LOG_ERROR = 6
    [LibraryImport(Lib)]
    private static partial void OH_LOG_Print(int type, int level, uint domain, byte* tag, byte* fmt, byte* value);

    public static void Debug(string tag, string message) => Write(3, tag, message);

    public static void Info(string tag, string message) => Write(4, tag, message);

    public static void Warn(string tag, string message) => Write(5, tag, message);

    public static void Error(string tag, string message) => Write(6, tag, message);

    private static void Write(int level, string tag, string message)
    {
        try
        {
            var tagUtf8 = Encoding.UTF8.GetBytes(tag);
            var fmtUtf8 = Encoding.UTF8.GetBytes("%{public}s");
            var msgUtf8 = Encoding.UTF8.GetBytes(message ?? string.Empty);
            fixed (byte* t = tagUtf8, f = fmtUtf8, m = msgUtf8)
            {
                OH_LOG_Print(0, level, 0x0000, t, f, m);
            }
        }
        catch
        {
            // 日志失败不参与控制流
        }
    }
}
