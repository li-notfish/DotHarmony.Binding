// 所有 HarmonyOS View Handler 的统一基类：在 Connect/Disconnect 生命周期挂载
// 手势识别管线（HarmonyGestureManager：View.GestureRecognizers → ArkUI 原生手势）。
// Page 类 Handler（ContentPage/NavigationPage）无手势语义，保持直接继承 ViewHandler。
#nullable enable
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public abstract class HarmonyViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
    where TVirtualView : class, Microsoft.Maui.IView
    where TPlatformView : ArkUINodeBase
{
    private HarmonyGestureManager? _gestures;

    protected HarmonyViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper) { }

    protected override void ConnectHandler(TPlatformView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is Microsoft.Maui.Controls.View controlsView)
            _gestures = new HarmonyGestureManager(controlsView, platformView);
    }

    protected override void DisconnectHandler(TPlatformView platformView)
    {
        _gestures?.Dispose();
        _gestures = null;
        base.DisconnectHandler(platformView);
    }
}
