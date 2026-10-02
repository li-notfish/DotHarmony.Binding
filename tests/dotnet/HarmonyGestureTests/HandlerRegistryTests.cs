using HarmonyOS.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

/// <summary>
/// HarmonyHandlerFactory 开放注册表的解析规则（TryResolveRegistered，纯逻辑）：
/// 精确类型命中、派生类型沿继承链向上找最近注册、未注册返回 null、后注册覆盖、Unregister。
/// </summary>
[Collection("RegistryStatic")] // 静态注册表，避免与其他用例并发污染
public class HandlerRegistryTests
{
    private class CustomButton : Button { }
    private sealed class DerivedCustomButton : CustomButton { }

    private static readonly Func<IElementHandler> Stub = () => new StubHandler();

    private sealed class StubHandler : IElementHandler
    {
        public object? PlatformView => null;
        public IElement? VirtualView => null;
        public IMauiContext? MauiContext => null;
        public void SetMauiContext(IMauiContext mauiContext) { }
        public void SetVirtualView(IElement view) { }
        public void UpdateValue(string property) { }
        public void Invoke(string command, object? args) { }
        public void DisconnectHandler() { }
    }

    [Fact]
    public void Unregistered_Type_Returns_Null()
        => Assert.Null(HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomButton)));

    [Fact]
    public void Exact_Type_Hit()
    {
        HarmonyHandlerFactory.Register<CustomButton>(Stub);
        try
        {
            Assert.Same(Stub, HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomButton)));
        }
        finally { HarmonyHandlerFactory.Unregister<CustomButton>(); }
    }

    [Fact]
    public void Derived_Type_Walks_Up_To_Nearest_Registration()
    {
        HarmonyHandlerFactory.Register<CustomButton>(Stub);
        try
        {
            // DerivedCustomButton 未注册，命中最近的基类 CustomButton 注册
            Assert.Same(Stub, HarmonyHandlerFactory.TryResolveRegistered(typeof(DerivedCustomButton)));
        }
        finally { HarmonyHandlerFactory.Unregister<CustomButton>(); }
    }

    [Fact]
    public void Derived_Registration_Wins_Over_Base()
    {
        Func<IElementHandler> derivedStub = () => new StubHandler();
        HarmonyHandlerFactory.Register<CustomButton>(Stub);
        HarmonyHandlerFactory.Register<DerivedCustomButton>(derivedStub);
        try
        {
            Assert.Same(derivedStub, HarmonyHandlerFactory.TryResolveRegistered(typeof(DerivedCustomButton)));
            Assert.Same(Stub, HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomButton)));
        }
        finally
        {
            HarmonyHandlerFactory.Unregister<DerivedCustomButton>();
            HarmonyHandlerFactory.Unregister<CustomButton>();
        }
    }

    [Fact]
    public void Later_Register_Overrides()
    {
        Func<IElementHandler> second = () => new StubHandler();
        HarmonyHandlerFactory.Register<CustomButton>(Stub);
        HarmonyHandlerFactory.Register<CustomButton>(second);
        try
        {
            Assert.Same(second, HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomButton)));
        }
        finally { HarmonyHandlerFactory.Unregister<CustomButton>(); }
    }

    [Fact]
    public void Unregister_Removes_Registration()
    {
        HarmonyHandlerFactory.Register<CustomButton>(Stub);
        Assert.True(HarmonyHandlerFactory.Unregister<CustomButton>());
        Assert.Null(HarmonyHandlerFactory.TryResolveRegistered(typeof(CustomButton)));
        Assert.False(HarmonyHandlerFactory.Unregister<CustomButton>());
    }
}
