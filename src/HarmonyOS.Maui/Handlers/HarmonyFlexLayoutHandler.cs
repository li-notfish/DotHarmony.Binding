#nullable enable
using HarmonyOS.Bindings.NativeNode;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// FlexLayout 的专用分派入口。
/// ArkUI Row/Column 承载 Row/Column 两个主方向；Wrap/AlignContent/Position 显式降级。
/// </summary>
public sealed class HarmonyFlexLayoutHandler : HarmonyLayoutHandler
{
    public HarmonyFlexLayoutHandler() { }

    protected override ArkUINode CreatePlatformView()
        => new HarmonyOS.ArkUI.Flex();
}
