// BrushHelper：Controls 的 Brush 体系 → ArkUI ARGB 取色
// 注意：Controls 的 Brush（SolidColorBrush/GradientBrush/ImageBrush）与 Graphics.SolidPaint
// 是平行类型树（不可互相模式匹配），XAML BackgroundColor 产物为 SolidColorBrush。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public static class BrushHelper
{
    /// <summary>尝试从 Brush 取纯色（当前仅支持 SolidColorBrush；Gradient/Image 记录 gap 后返回 false）</summary>
    public static bool TryGetColor(Brush? brush, out Microsoft.Maui.Graphics.Color color)
    {
        if (brush is SolidColorBrush scb && scb.Color is not null)
        {
            color = scb.Color;
            return true;
        }

        // GradientBrush/ImageBrush 暂不支持，静默透明（记入 gap）
        color = Microsoft.Maui.Graphics.Colors.Transparent;
        return false;
    }

    /// <summary>把 Brush 翻译为节点背景色（不支持的画刷静默跳过）</summary>
    public static void ApplyBackground(ArkUINode node, Brush? brush)
    {
        if (TryGetColor(brush, out var c))
            node.SetBackgroundColor(
                (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }
}
