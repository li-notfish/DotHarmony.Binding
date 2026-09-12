// P0 漏斗纪律的机械闸（MIGRATION_ARKTS_ENGINE.md §1）：
// Handler/Hosting 层（src/HarmonyOS.Maui）只允许经包装类型触碰 ArkUI——
// ArkUINodeBase/Nodes/* 包装类、ArkUINodeEvent/ArkUIPointerEvent 载荷包装、手势包装类、事件枚举。
// 禁止直接出现原生函数表类、P/Invoke 入口、原始事件/属性结构体——保证未来换渲染后端
// （ArkTS 命令总线引擎）时，唯一替换点是 Bindings 包装层。
using Xunit;

namespace HarmonyGestureTests;

public class FunnelDisciplineTests
{
    /// <summary>禁止在 Handler/Hosting 层出现的符号（NativeAOT 互换边界）</summary>
    private static readonly (string Token, string Reason)[] Forbidden =
    [
        ("ArkUINativeApi", "节点函数表类——经 ArkUINodeBase 包装方法"),
        ("ArkUIGestureApi", "手势函数表类——经 Nodes/Gestures 包装类"),
        ("ArkUIAnimateApi", "动画函数表类——经 ArkUINodeBase.Animate/AnimateAsync"),
        ("NativeNodeApi", "napi P/Invoke 类——属 Bindings/Runtime 层"),
        ("OH_ArkUI_", "NDK C 入口——一律加包装后使用"),
        ("napi_", "Node-API C 入口——属 Bindings/Runtime 层"),
        ("GetNodeComponentEvent", "原始事件结构体直读——经 ArkUINodeEvent 载荷包装"),
        ("ArkUI_NodeComponentEvent", "原始事件结构体——经 ArkUINodeEvent 载荷包装"),
        ("ArkUI_AttributeItem", "原始属性结构体——经 SetXxxAttribute 包装"),
        ("DllImport", "Handler 层不得自行 P/Invoke"),
        ("LibraryImport", "Handler 层不得自行 P/Invoke"),
    ];

    [Fact]
    public void HandlerLayer_NeverTouchesNativeFunctionTables()
    {
        var root = FindRepoRoot();
        var handlerRoot = Path.Combine(root, "src", "HarmonyOS.Maui");
        Assert.True(Directory.Exists(handlerRoot), $"找不到 {handlerRoot}");

        var violations = new List<string>();
        foreach (var file in Directory.EnumerateFiles(handlerRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (var (token, reason) in Forbidden)
                {
                    if (lines[i].Contains(token, StringComparison.Ordinal))
                        violations.Add($"{Path.GetRelativePath(root, file)}:{i + 1}: '{token}' — {reason}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "漏斗纪律违规（原生 API 只能进 Bindings 包装层，见 MIGRATION_ARKTS_ENGINE.md §1）：\n"
            + string.Join("\n", violations));
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir is not null; i++, dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "HarmonyOS.Maui")))
                return dir.FullName;
        }
        throw new InvalidOperationException("无法从测试目录定位仓库根（src/HarmonyOS.Maui）");
    }
}
