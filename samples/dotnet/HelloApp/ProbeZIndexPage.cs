// 原生 z 平局仲裁探针：三个原生色块两两/三重重叠，按受控顺序写 NODE_Z_INDEX。
// 原生节点操作必须走 UI 帧（后台线程 AddChild 曾静默失败；MainThread 调度在宿主下未实现），
// 因此改为按钮驱动：每次点击 "PROBE NEXT" 应用下一步并输出 "PROBE STEP <名> APPLIED"，
// 外部脚本（scripts/verify-zindex-probe.ps1）逐步点击+截屏，采样重叠区像素判定仲裁规则。
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkStack = HarmonyOS.ArkUI.Stack;

namespace HelloApp;

public class ProbeZIndexPage : ContentPage
{
    // 几何（vp）：R/G/B 错开 45vp、边长 170，保证三块都收在首行 360vp 高度内
    // （Stack 默认不裁剪子节点，越界块会盖住下方的 NEXT 按钮吞掉点击——实测踩过）
    private const int Size = 170;
    private static readonly (string Name, byte R, byte G, byte B, int X, int Y)[] Blocks =
    {
        ("red",   255, 0,   0,   30, 30),
        ("green", 0,   200, 0,   75, 75),
        ("blue",  0,   0,   255, 120, 120),
    };

    private readonly Label _status = new() { Text = "probe ready", TextColor = Colors.White };
    private ArkUINodeBase[]? _nodes;
    private int _stepIndex = -1;

    public ProbeZIndexPage()
    {
        Title = "zprobe";
        BackgroundColor = Colors.Black;
        var next = new Button { Text = "PROBE NEXT", BackgroundColor = Colors.DarkSlateGray, TextColor = Colors.White };
        Grid.SetRow(next, 1);
        next.Clicked += (_, _) => Advance();
        Content = new Grid
        {
            RowDefinitions = [new RowDefinition(360), new RowDefinition(GridLength.Auto)],
            Children = { _status, next },
        };
    }

    private void Advance() // 点击回调已在 UI 帧内
    {
        try
        {
            _nodes ??= Setup();
            var (r, g, bl) = (_nodes[0], _nodes[1], _nodes[2]);
            _stepIndex++;
            string name;
            switch (_stepIndex)
            {
                case 0: name = "s0_decl_000";            r.SetZIndex(0); g.SetZIndex(0); bl.SetZIndex(0); break;
                case 1: name = "s1_rev_000";             bl.SetZIndex(0); g.SetZIndex(0); r.SetZIndex(0); break;
                case 2: name = "s2_g_raise10";           g.SetZIndex(10); break;
                case 3: name = "s3_restore_rewall";      g.SetZIndex(0); r.SetZIndex(0); g.SetZIndex(0); bl.SetZIndex(0); break;
                case 4: name = "s4_restore_onlyothers";  g.SetZIndex(10); g.SetZIndex(0); r.SetZIndex(0); bl.SetZIndex(0); break;
                case 5: name = "s5_distinct_012";        r.SetZIndex(0); g.SetZIndex(1); bl.SetZIndex(2); break;
                case 6: name = "s6_distinct_210";        r.SetZIndex(2); g.SetZIndex(1); bl.SetZIndex(0); break;
                default: HiLog.Info("HarmonyHost", "PROBE DONE"); _status.Text = "done"; return;
            }
            _status.Text = "step: " + name;
            HiLog.Info("HarmonyHost", $"PROBE STEP {name} APPLIED");
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"PROBE ERROR: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private ArkUINodeBase[] Setup()
    {
        if (Content?.Handler?.PlatformView is not ArkUINodeBase host)
            throw new InvalidOperationException("no host platform view");
        var container = new ArkStack();
        container.SetPosition(0, 0);
        container.SetWidth(320);
        container.SetHeight(300);
        container.SetBackgroundColor(20, 20, 20);
        host.AddChild(container);

        var nodes = new ArkUINodeBase[Blocks.Length];
        for (int i = 0; i < Blocks.Length; i++)
        {
            var b = Blocks[i];
            var n = new ArkColumn();
            n.SetPosition(b.X, b.Y);
            n.SetWidth(Size);
            n.SetHeight(Size);
            n.SetBackgroundColor(b.R, b.G, b.B);
            container.AddChild(n);
            nodes[i] = n;
        }
        HiLog.Info("HarmonyHost", "PROBE SETUP OK");
        return nodes;
    }
}
