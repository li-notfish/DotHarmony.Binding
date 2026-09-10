// MauiHarmonyHost：MAUI 控件树的鸿蒙引导
// 宿主根为一个 100% Stack 容器（挂入 ContentSlot）：Stack 后挂者覆盖先挂者，
// 模态页借此覆盖在页面之上；页面经 HarmonyNavigation 挂入该容器。
using HarmonyOS.Bindings.Hosting;
using ArkStack = HarmonyOS.ArkUI.Stack;
using MPage = Microsoft.Maui.Controls.Page;

namespace HarmonyOS.Maui.Hosting;

public static class MauiHarmonyHost
{
    /// <summary>
    /// 注册 MAUI 根页面工厂。rootFactory 在 UI 主线程（HarmonyBuildUI 时序）被调用，
    /// 返回的首页经 HarmonyHandlerFactory 装配 handler 并挂载上屏。
    /// </summary>
    public static void Run(Func<MPage> rootFactory)
    {
        Host.RootBuilder = contentHandle =>
        {
            // 宿主根容器：页面栈 + 模态层的挂载点（Stack 叠加语义）
            var container = new ArkStack();
            container.SetWidthPercent(1.0f);
            container.SetHeightPercent(1.0f);
            Host.AttachRoot(contentHandle, container);

            HarmonyNavigation.Attach(container);
            HarmonyNavigation.Push(rootFactory());
        };
    }
}
