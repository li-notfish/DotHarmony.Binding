// BindingProbePage 的 code-behind：AOT 绑定真值探针。
// Appearing 时把经典 Binding / compiled binding / DynamicResource / AppThemeBinding 的
// 实际取值打进 hilog（tag=VProbe），供自动化日志断言——不允许只看桌面构建通过。
using HarmonyOS.Interop;
using Microsoft.Maui.Controls;

namespace HelloApp;

/// <summary>探针模型：public 成员（HarmonyPreserveXamlBindings 保活根覆盖，AOT 反射可达）。</summary>
public class ProbeItem
{
    public string Title { get; set; } = "ITEM_TITLE_OK";
}

public class ProbeHost
{
    public ProbeItem Item { get; set; } = new();
}

public partial class BindingProbePage : ContentPage
{
    public BindingProbePage()
    {
        InitializeComponent();
        BindingContext = new ProbeHost();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var classic = ClassicLabel.Text;
        var compiled = CompiledLabel.Text;
        var dyn = DynamicLabel.TextColor?.ToHex() ?? "null";
        var theme = ThemeLabel.TextColor?.ToHex() ?? "null";
        HiLog.Info("VProbe", $"[V][BIND] classic='{classic}' compiled='{compiled}' dyn={dyn} theme={theme}");

        var classicOk = classic == "ITEM_TITLE_OK";
        var compiledOk = compiled == "ITEM_TITLE_OK";
        HiLog.Info("VProbe", $"[V][BIND] {(classicOk ? "CLASSIC_OK" : "CLASSIC_FAIL")} {(compiledOk ? "COMPILED_OK" : "COMPILED_FAIL")}");
        Status.Text = $"{(classicOk ? "classic OK" : "classic FAIL")} / {(compiledOk ? "compiled OK" : "compiled FAIL")}";
    }
}
