// 计划承诺的绑定语义单测（headless，纯 MAUI 绑定引擎 + 本库纯协议层）：
//   · TemplateBinding 顺序无关（属性先设/模板后应用、模板先应用/属性后改，两序等价）
//   · MultiBinding 聚合（StringFormat）
//   · FallbackValue 兜底
//   · ContentPresenter.Padding 平台映射存在性（Mapper 键检查，不依赖原生节点）
using System.Reflection;
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace HarmonyGestureTests;

public class BindingComplianceTests
{
    static BindingComplianceTests()
    {
        // TemplateBinding 绑定回填经 BindableObject.Dispatcher；无 UI 线程的宿主
        // （本测试环境）需要探针式 dispatcher 供给
        DispatcherProvider.SetCurrent(new TestDispatcherProvider());
    }

    private sealed class TestDispatcherProvider : IDispatcherProvider
    {
        private static readonly IDispatcher Dispatcher = new TestDispatcher();
        public IDispatcher? GetForCurrentThread() => Dispatcher;

        private sealed class TestDispatcher : IDispatcher
        {
            public bool IsDispatchRequired => false;
            public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
            public bool Dispatch(Action action) { action(); return true; }
            public bool DispatchDelayed(TimeSpan delay, Action action) { action(); return true; }
        }
    }

    [Fact]
    public void ContentPresenter_PaddingMapped()
        => Assert.Contains(
            nameof(ContentPresenter.Padding),
            HarmonyContentPresenterHandler.Mapper.GetKeys());

    [Fact]
    public void FallbackValue_UsedWhenPathMissing()
    {
        var label = new Label();
        // FallbackValue 以属性初始化器给定（该 MAUI 版本的 Binding 构造函数无
        // fallbackValue 命名参数，构造后设 FallbackValue 等价）
        label.SetBinding(Label.TextProperty,
            new Binding("NoSuchProperty") { FallbackValue = "fallback" });
        label.BindingContext = new object();
        Assert.Equal("fallback", label.Text);
    }

    [Fact]
    public void MultiBinding_StringFormatAggregates()
    {
        var label = new Label();
        label.SetBinding(Label.TextProperty, new MultiBinding
        {
            Bindings = { new Binding("A"), new Binding("B") },
            StringFormat = "{0}-{1}",
        });
        label.BindingContext = new { A = "x", B = 42 };
        Assert.Equal("x-42", label.Text);
    }

    [Fact]
    public void TemplateBinding_OrderIndependent()
    {
        // 顺序一：模板先应用，再改属性（TemplateBinding 应跟随）
        var v1 = new ProbeTemplateView();
        v1.ApplyTemplateNow();
        v1.Caption = "after";
        Assert.Equal("after", v1.TemplateLabel!.Text);

        // 顺序二：先设属性，再应用模板（实例化时读当前值）
        var v2 = new ProbeTemplateView();
        v2.Caption = "before";
        v2.ApplyTemplateNow();
        Assert.Equal("before", v2.TemplateLabel!.Text);
    }

    private sealed class ProbeTemplateView : ContentView
    {
        public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
            nameof(Caption), typeof(string), typeof(ProbeTemplateView), "unset");

        public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }
        public Label? TemplateLabel { get; private set; }

        // 分离属性设置与模板实例化（ControlTemplate 赋值即实例化，TemplateUtilities
        // 负责 TemplateBinding 锚定；无头环境不依赖 handler 驱动 OnApplyTemplate）
        public void ApplyTemplateNow()
        {
            ControlTemplate = new ControlTemplate(() =>
            {
                var label = new Label();
                label.SetBinding(Label.TextProperty, new TemplateBinding(nameof(Caption)));
                TemplateLabel = label;
                return new Grid { Children = { label } };
            });
        }
    }

}
