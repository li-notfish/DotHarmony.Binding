// BrushHelper：Controls 的 Brush 体系 → ArkUI 背景（纯色 / 线性渐变 / 径向渐变）
// 注意：Controls 的 Brush（SolidColorBrush/GradientBrush/ImageBrush）与 Graphics.SolidPaint
// 是平行类型树（不可互相模式匹配），XAML BackgroundColor 产物为 SolidColorBrush。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public static class BrushHelper
{
    /// <summary>尝试从 Brush 取纯色（仅 SolidColorBrush；渐变画刷经 ApplyBackground 走对应通道）</summary>
    public static bool TryGetColor(Brush? brush, out Microsoft.Maui.Graphics.Color color)
    {
        if (brush is SolidColorBrush scb && scb.Color is not null)
        {
            color = scb.Color;
            return true;
        }

        color = Microsoft.Maui.Graphics.Colors.Transparent;
        return false;
    }

    /// <summary>把 Brush 翻译为节点背景：纯色、线性/径向渐变（NODE_*_GRADIENT）、图片（NODE_BACKGROUND_IMAGE）</summary>
    public static void ApplyBackground(ArkUINode node, Brush? brush)
    {
        switch (brush)
        {
            case SolidColorBrush solid when solid.Color is not null:
                node.SetBackgroundColor(
                    (byte)(solid.Color.Red * 255), (byte)(solid.Color.Green * 255),
                    (byte)(solid.Color.Blue * 255), (byte)(solid.Color.Alpha * 255));
                return;

            case LinearGradientBrush linear when HasStops(linear):
                node.SetLinearGradient(AngleOf(linear), false,
                    ToColors(linear.GradientStops), ToStops(linear.GradientStops));
                return;

            case RadialGradientBrush radial when HasStops(radial):
                node.SetRadialGradient((float)radial.Center.X, (float)radial.Center.Y, (float)radial.Radius,
                    false, ToColors(radial.GradientStops), ToStops(radial.GradientStops));
                return;
        }
        // 其余（无色 SolidColorBrush、空 GradientBrush 等）：静默跳过。
        // ImageBrush 不处理——MAUI 10 将其保持为 internal（无法构造/声明，上游 API 限制），
        // 节点层的 SetBackgroundImage 原语已就位，上游公开后即可接线。
    }

    private static bool HasStops(GradientBrush brush) => brush.GradientStops is { Count: > 0 };

    /// <summary>
    /// StartPoint→EndPoint 方向向量转 CSS 角度（0 = 向上，顺时针增大）。
    /// 对齐 ArkUI angle 语义：StartPoint(0,0)→EndPoint(0,1)（上→下）= 180°。
    /// </summary>
    private static float AngleOf(LinearGradientBrush linear)
    {
        var dx = (float)(linear.EndPoint.X - linear.StartPoint.X);
        var dy = (float)(linear.EndPoint.Y - linear.StartPoint.Y);
        var deg = MathF.Atan2(dx, -dy) * 180f / MathF.PI;
        if (deg < 0)
            deg += 360f;
        return deg;
    }

    private static uint[] ToColors(IList<GradientStop> stops)
    {
        var colors = new uint[stops.Count];
        for (int i = 0; i < stops.Count; i++)
        {
            var c = stops[i].Color;
            colors[i] = c is null
                ? 0
                : (uint)((byte)(c.Alpha * 255) << 24 | (byte)(c.Red * 255) << 16
                    | (byte)(c.Green * 255) << 8 | (byte)(c.Blue * 255));
        }
        return colors;
    }

    private static float[] ToStops(IList<GradientStop> stops)
    {
        // Offset 全部为默认值 0 时（XAML 未显式声明）按 MAUI 语义均匀分布
        var allDefault = true;
        foreach (var s in stops)
        {
            if (s.Offset != 0)
            {
                allDefault = false;
                break;
            }
        }

        var result = new float[stops.Count];
        for (int i = 0; i < stops.Count; i++)
            result[i] = allDefault ? (stops.Count == 1 ? 0 : (float)i / (stops.Count - 1)) : stops[i].Offset;
        return result;
    }
}
