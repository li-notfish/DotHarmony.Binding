using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// hilog 原生日志（libhilog_ndk.z.so）。
/// OH_LOG_Print 是 C 变参函数；对 %{public}s 单字符串场景，
/// arm64/x64 上以固定签名调用变参函数是安全的（前几个参数均走整型/指针寄存器）。
/// 热路径注意：本类不阻止调用方的字符串插值分配——高频日志请在调用方用常量门控。/// </summary>
public static unsafe partial class HiLog
{
    private const string Lib = "libhilog_ndk.z.so";

    // LOG_APP = 0；LOG_DEBUG = 3；LOG_INFO = 4；LOG_WARN = 5；LOG_ERROR = 6
    [LibraryImport(Lib)]
    private static partial void OH_LOG_Print(int type, int level, uint domain, byte* tag, byte* fmt, byte* value);

    /// <summary>运行时开关（默认开）。关闭后 Debug/Info 直接返回（Warn/Error 始终输出）。</summary>
    public static bool VerboseEnabled { get; set; } = true;

    // 缓存一次的 UTF8 常量（Write 每次调用省两次编码分配）；空串以 NUL 终止符表示
    private static readonly byte[] FmtUtf8 = "%{public}s"u8.ToArray();
    private static readonly byte[] EmptyUtf8 = { 0 };

    public static void Debug(string tag, string message) => Write(3, tag, message);

    public static void Info(string tag, string message) => Write(4, tag, message);

    public static void Warn(string tag, string message) => Write(5, tag, message);

    public static void Error(string tag, string message) => Write(6, tag, message);

    private static void Write(int level, string tag, string message)
    {
        if (level <= 4 && !VerboseEnabled)
            return;
        try
        {
            var msgUtf8 = string.IsNullOrEmpty(message)
                ? EmptyUtf8
                : Encoding.UTF8.GetBytes(message);
            fixed (byte* f = FmtUtf8, m = msgUtf8)
            fixed (byte* t = Encoding.UTF8.GetBytes(tag))
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
