// IMainThread 鸿蒙实现：宿主把 .NET 运行时嵌在 JS 线程内联驱动（ArkTS 回调 → NativeAOT），
// JS 线程 == UI 线程 == Install 线程。IsMainThread 捕获 Install 线程比对；
// BeginInvokeOnMainThread 无跨线程投递通道（TSFN 仅 native→.NET 方向），直接内联执行——
// 宿主单线程驱动下语义等价，线程外调用方仅罕见的异步续体。
#nullable enable

namespace HarmonyOS.Maui.Essentials;

public class HarmonyMainThread
{
    static readonly Thread? _uiThread = Thread.CurrentThread;

    public bool IsMainThread() => Thread.CurrentThread == _uiThread;

    public void BeginInvokeOnMainThread(Action action)
    {
        if (Thread.CurrentThread == _uiThread || _uiThread is null)
        {
            action();
            return;
        }
        // 无 .NET→JS 线程投递通道（ThreadSafeFunction 是 native→.NET 方向）：内联兜底
        action();
    }
}
