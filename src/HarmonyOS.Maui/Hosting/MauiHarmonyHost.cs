// MauiHarmonyHost：MAUI 控件树的鸿蒙引导
// 把 Host.RootBuilder（HarmonyOS.Bindings.Hosting）与 MAUI handler 树装配衔接：
// rootFactory 返回 MAUI 控件树根 → 装配 handler 树 → platform view 挂入 ContentSlot
using HarmonyOS.Bindings.Hosting;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Maui.Handlers;

namespace HarmonyOS.Maui.Hosting;

public static class MauiHarmonyHost
{
    /// <summary>
    /// 注册 MAUI 根视图工厂。rootFactory 在 UI 主线程（HarmonyBuildUI 时序）被调用，
    /// 返回的控件树由 HarmonyHandlerFactory 装配 handler 并挂载上屏。
    /// </summary>
    public static void Run(Func<Microsoft.Maui.Controls.Page> rootFactory)
    {
        Host.RootBuilder = contentHandle =>
        {
            var root = rootFactory();
            var handler = HarmonyHandlerFactory.Create((Microsoft.Maui.Controls.Element)root);
            handler.SetVirtualView(root);
            if (handler.PlatformView is ArkUINodeBase platformRoot)
            {
                // MAUI 根布局 Fill 语义：撑满宿主可用区域（ContentSlot 的父容器）
                platformRoot.SetWidthPercent(1.0f);
                platformRoot.SetHeightPercent(1.0f);
                Host.AttachRoot(contentHandle, platformRoot);
            }
            else
            {
                throw new InvalidOperationException(
                    $"root handler PlatformView is not an ArkUINodeBase: {handler.PlatformView?.GetType().Name}");
            }
        };
    }
}
