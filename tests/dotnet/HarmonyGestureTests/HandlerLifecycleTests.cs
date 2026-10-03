using HarmonyOS.Maui.Handlers;
using HarmonyOS.Maui.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// Handler 生命周期契约（离设备，fake handler 注入开放注册表）：
/// P1 修复的回归保障——element.Handler 回写、SetMauiContext 先于 SetVirtualView、
/// element.Handler = null 触发 DisconnectHandler、Unregister 恢复内置分派。
/// </summary>
public class HandlerLifecycleTests
{
    private class ProbeView : View { }
    private sealed class DerivedProbeView : ProbeView { }

    private sealed class FakeHandler : IViewHandler
    {
        public bool ContextSet;
        public bool VirtualViewSet;
        public bool ContextBeforeVirtualView;
        public bool Disconnected;
        public object? PlatformView => null;
        public object? ContainerView => null;
        public bool HasContainer { get; set; }
        public IView? VirtualView { get; private set; }
        public IMauiContext? MauiContext { get; private set; }
        public void SetMauiContext(IMauiContext mauiContext) { MauiContext = mauiContext; ContextSet = true; }
        public void SetVirtualView(IView view)
        {
            ContextBeforeVirtualView = ContextSet;
            VirtualView = view;
            VirtualViewSet = true;
        }
        public void UpdateValue(string property) { }
        public void Invoke(string command, object? args) { }
        public void DisconnectHandler() => Disconnected = true;
        // IElementHandler 显式实现（IViewHandler 继承链要求两套签名）
        IElement? IElementHandler.VirtualView => VirtualView;
        void IElementHandler.SetVirtualView(IElement view) => SetVirtualView((IView)view);
        public Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
            => new(0, 0);
        public void SetFrame(Microsoft.Maui.Graphics.Rect frame) { }
        public void PlatformArrange(Microsoft.Maui.Graphics.Rect frame) { }
    }

    [Fact]
    public void Create_Registered_SetsContextThenVirtualView_AndWritesBackElementHandler()
    {
        var fake = new FakeHandler();
        HarmonyHandlerFactory.Register<ProbeView>(() => fake);
        try
        {
            var view = new ProbeView();
            var handler = HarmonyHandlerFactory.Create((Microsoft.Maui.Controls.Element)view);

            Assert.Same(fake, handler);
            Assert.True(fake.ContextSet);
            Assert.True(fake.ContextBeforeVirtualView); // SetMauiContext 先于 SetVirtualView
            Assert.Same(view, fake.VirtualView);
            Assert.Same(fake, view.Handler); // 回写建立 element→handler 反向链
        }
        finally { HarmonyHandlerFactory.Unregister<ProbeView>(); }
    }

    [Fact]
    public void ElementHandlerCleared_TriggersDisconnect()
    {
        var fake = new FakeHandler();
        HarmonyHandlerFactory.Register<ProbeView>(() => fake);
        try
        {
            var view = new ProbeView();
            HarmonyHandlerFactory.Create((Microsoft.Maui.Controls.Element)view);
            Assert.False(fake.Disconnected);

            view.Handler = null; // 清理路径：必须真正触发 DisconnectHandler
            Assert.True(fake.Disconnected);
        }
        finally { HarmonyHandlerFactory.Unregister<ProbeView>(); }
    }

    [Fact]
    public void Registry_DerivedView_ResolvesNearestBaseRegistration()
    {
        HarmonyHandlerFactory.Register<ProbeView>(() => new FakeHandler());
        try
        {
            var factory = HarmonyHandlerFactory.TryResolveRegistered(typeof(DerivedProbeView));
            Assert.NotNull(factory);
        }
        finally { HarmonyHandlerFactory.Unregister<ProbeView>(); }
    }

    [Fact]
    public void Unregister_RemovesRegistration()
    {
        HarmonyHandlerFactory.Register<ProbeView>(() => new FakeHandler());
        Assert.True(HarmonyHandlerFactory.Unregister<ProbeView>());
        Assert.Null(HarmonyHandlerFactory.TryResolveRegistered(typeof(ProbeView)));
        Assert.False(HarmonyHandlerFactory.Unregister<ProbeView>());
    }
}
