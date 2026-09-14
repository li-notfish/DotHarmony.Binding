// ArkTS 命令总线引擎的实验场景（MIGRATION_ARKTS_ENGINE.md §2 / P2 完整版）：
// 根 Stack + Text + Button + Entry + 定位色块；点击/输入 → 引擎回流 → 改属性
// ——指令环验证 建树/属性写/事件回流/量测回流/tick 自动冲刷/增量 diff。
// dlopen 时 ModuleInitializer 即构建场景（纯托管入队，无需 env）；
// ArkTS 侧 initEngine 通道到达后由 ArkTsEngine.InitializeCore 统一冲刷。
using HarmonyOS.Bindings.Experimental;

namespace EngineLab;

public static class Program
{
    private static int _count;
    private static VirtualNode? _status;

    public static void Register()
    {
        var root = new VirtualNode("Stack")
            .SetWidthPercent(100)
            .SetHeightPercent(100)
            .SetBackgroundColor(0xFFFFFFFF);

        var title = new VirtualNode("Text")
            .SetText("Hello from the ArkTS command bus")
            .SetFontSize(18)
            .SetFontColor(0xFF1A1A1A);

        _status = new VirtualNode("Text")
            .SetText("clicked 0 times")
            .SetFontSize(14)
            .SetFontColor(0xFF666666);

        var button = new VirtualNode("Button")
            .SetLabel("Click me")
            .SetFontSize(16)
            .On("click", _ =>
            {
                _count++;
                // 属性写 → 引擎 16ms tick 自动冲刷（P2）：同帧写合并为单次 napi，
                // 无需逐次显式 Flush——hilog 中点击后 ≤16ms 内出现一次 "flushed 1 commands"
                _status?.SetText($"clicked {_count} times");
            });

        // P2 扩展节点：Entry 文本变更回流（textChange，载荷携带文本值）
        var input = new VirtualNode("Entry")
            .SetPlaceholder("type here")
            .SetFontSize(14)
            .On("textChange", e => Console.WriteLine($"[EngineLab] text changed: {e.Text}"));

        // P2 定位属性：绝对定位色块（引擎侧 .position({x,y})，vp）
        var badge = new VirtualNode("Stack")
            .SetWidth(40f)
            .SetHeight(40f)
            .SetBackgroundColor(0xFF336699)
            .SetPosition(16f, 16f);

        // 量测回流验证：根节点 area 快照打日志（影子缓存，非 P/Invoke）
        root.On("area", e => Console.WriteLine($"[EngineLab] root measured {e}"));

        root.AddChild(title);
        root.AddChild(button);
        root.AddChild(_status);
        root.AddChild(input);
        root.AddChild(badge);
        ArkTsEngine.SetRoot(root);

        ArkTsEngine.Flush(); // 桥未挂接前仅积压；initEngine 握手时统一冲刷
    }
}
