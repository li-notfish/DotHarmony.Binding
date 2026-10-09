using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Label 的 HarmonyOS Handler（ArkUI Text 节点）。</summary>
public class HarmonyLabelHandler : HarmonyViewHandler<Label, ArkText>, ILabelHandler
{
    public static PropertyMapper<Label, HarmonyLabelHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Label.Text)] = MapText,
        [nameof(Label.TextColor)] = MapTextColor,
        [nameof(Label.FontSize)] = MapFontSize,
        [nameof(Label.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(Label.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(Label.Background)] = MapBackground,
        [nameof(Label.LineBreakMode)] = MapLineBreakMode,
        [nameof(Label.MaxLines)] = MapMaxLines,
        [nameof(Label.FontFamily)] = MapFontFamily,
        [nameof(Label.FontAttributes)] = MapFontAttributes,
        [nameof(Label.LineHeight)] = MapLineHeight,
        [nameof(Label.Padding)] = MapPadding,
    };

    public HarmonyLabelHandler() : base(Mapper) { }

    ILabel ILabelHandler.VirtualView => VirtualView;

    object ILabelHandler.PlatformView => PlatformView;

    protected override ArkText CreatePlatformView() => new();

    public static void MapText(HarmonyLabelHandler h, Label v)
    {
        h.PlatformView.Content = v.Text ?? string.Empty;
    }

    public static void MapTextColor(HarmonyLabelHandler h, Label v)
    {
        if (v.TextColor is { } c)
            h.PlatformView.SetFontColor(c);
    }

    public static void MapFontSize(HarmonyLabelHandler h, Label v)
    {
        if (v.FontSize > 0)
            h.PlatformView.FontSize = (float)v.FontSize;
    }

    public static void MapHorizontalTextAlignment(HarmonyLabelHandler h, Label v)
    {
        h.PlatformView.TextAlign = v.HorizontalTextAlignment switch
        {
            TextAlignment.Start => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
            TextAlignment.Center => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_CENTER,
            TextAlignment.End => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_END,
            _ => ArkUI_TextAlignment.ARKUI_TEXT_ALIGNMENT_START,
        };
    }

    public static void MapVerticalTextAlignment(HarmonyLabelHandler h, Label v)
    {
    }

    public static void MapBackground(HarmonyLabelHandler h, Label v)
    {
        BrushHelper.ApplyBackground(h.PlatformView, v.Background);
    }

    public static void MapMaxLines(HarmonyLabelHandler h, Label v)
    {
        // MAUI 默认 -1 = 不限行；ArkUI MAX_LINES 无"无限"值，仅在显式设置时下发
        if (v.MaxLines >= 0)
            h.PlatformView.SetMaxLines(v.MaxLines);
    }

    public static void MapLineBreakMode(HarmonyLabelHandler h, Label v)
    {
        // ArkUI 约束：ELLIPSIS_MODE 仅对设置了 MAX_LINES 的文本生效，
        // 省略号截断模式同时补 MAX_LINES=1（WordWrap/NoWrap 由布局宽度自然表达）
        switch (v.LineBreakMode)
        {
            case LineBreakMode.HeadTruncation:
                h.PlatformView.SetMaxLines(v.MaxLines >= 0 ? v.MaxLines : 1);
                h.PlatformView.SetEllipsisMode(ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_START);
                break;
            case LineBreakMode.MiddleTruncation:
                h.PlatformView.SetMaxLines(v.MaxLines >= 0 ? v.MaxLines : 1);
                h.PlatformView.SetEllipsisMode(ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_CENTER);
                break;
            case LineBreakMode.TailTruncation:
                h.PlatformView.SetMaxLines(v.MaxLines >= 0 ? v.MaxLines : 1);
                h.PlatformView.SetEllipsisMode(ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_END);
                break;
            case LineBreakMode.NoWrap:
                h.PlatformView.SetMaxLines(1);
                break;
        }
    }

    public static void MapFontFamily(HarmonyLabelHandler h, Label v)
    {
        if (!string.IsNullOrEmpty(v.FontFamily))
            h.PlatformView.SetFontFamily(v.FontFamily);
    }

    public static void MapFontAttributes(HarmonyLabelHandler h, Label v)
    {
        // NODE_FONT_WEIGHT/STYLE 是通用属性；MAUI FontAttributes 默认 None = 400/NORMAL
        var attrs = v.FontAttributes;
        h.PlatformView.SetFontWeight(attrs.HasFlag(FontAttributes.Bold)
            ? ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W700 : ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W400);
        h.PlatformView.SetFontStyle(attrs.HasFlag(FontAttributes.Italic)
            ? ArkUI_FontStyle.ARKUI_FONT_STYLE_ITALIC : ArkUI_FontStyle.ARKUI_FONT_STYLE_NORMAL);
    }

    public static void MapLineHeight(HarmonyLabelHandler h, Label v)
    {
        // MAUI 默认 1.0（LineHeight 默认值 = -1? 实际默认 1.0）；倍数语义与 NODE_TEXT_LINE_HEIGHT_MULTIPLE 一致
        if (v.LineHeight >= 0)
            h.PlatformView.SetLineHeightMultiple((float)v.LineHeight);
    }

    public static void MapPadding(HarmonyLabelHandler h, Label v)
    {
        var p = v.Padding;
        h.PlatformView.SetPaddingEdges((float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
    }
}
