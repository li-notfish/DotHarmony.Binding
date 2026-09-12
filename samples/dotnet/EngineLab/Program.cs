// ArkTS 命令总线引擎的最小实验场景（MIGRATION_ARKTS_ENGINE.md §2 / P2 种子）：
// 根 Stack + Text（状态文本）+ Button；点击 → 引擎回流 click → 改 Text 属性 → Flush
// ——一条指令环验证 建树/属性写/事件回流/量测回流 四个核心语义。
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
                _status?.SetText($"clicked {_count} times");
                ArkTsEngine.Flush(); // 点击回流的指令环：属性写 → 单次 napi 批量下发
            });

        // 量测回流验证：根节点 area 快照打日志（影子缓存，非 P/Invoke）
        root.On("area", e => Console.WriteLine($"[EngineLab] root measured {e}"));

        root.AddChild(title);
        root.AddChild(button);
        root.AddChild(_status);
        ArkTsEngine.SetRoot(root);

        ArkTsEngine.Flush(); // 桥未挂接前仅积压；initEngine 握手时统一冲刷
    }
}
