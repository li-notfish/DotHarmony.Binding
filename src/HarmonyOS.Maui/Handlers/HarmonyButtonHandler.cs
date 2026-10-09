using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.ArkUI;
using Button = Microsoft.Maui.Controls.Button;
using ArkFlex = HarmonyOS.ArkUI.Flex;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Button 的 HarmonyOS Handler。
/// 实测当前 NDK 的 Button 节点两条文字路径均不可用：内建 NODE_BUTTON_LABEL 参与测量但不绘制
/// （胶囊有形无字），AddChild 子 Text 不布局不渲染。因此用 Flex + 子 Text 组合实现，
/// 视觉默认值（胶囊圆角/主题蓝/白字/内边距）由 HarmonyControlDefaults 集中定义。</summary>
public class HarmonyButtonHandler : HarmonyViewHandler<Button, ArkFlex>, IButtonHandler
{
    /// <summary>点击事件日志开关（插值分配走在调用点，仅在排障时打开）</summary>
    private static readonly bool LogClick = false;

    public static PropertyMapper<Button, HarmonyButtonHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(Button.Text)] = MapText,
        [nameof(Button.TextColor)] = MapTextColor,
        [nameof(Button.FontSize)] = MapFontSize,
        [nameof(Button.FontFamily)] = MapFontFamily,
        [nameof(Button.BackgroundColor)] = MapBackgroundColor,
        [nameof(Button.BorderColor)] = MapBorderColor,
        [nameof(Button.BorderWidth)] = MapBorderWidth,
        [nameof(Button.CornerRadius)] = MapCornerRadius,
        [nameof(Button.Padding)] = MapPadding,
        [nameof(Button.ImageSource)] = MapImageSource,
        [nameof(Button.ContentLayout)] = MapContentLayout,
        [nameof(Button.TextTransform)] = MapTextTransform,
        [nameof(Button.LineBreakMode)] = MapLineBreakMode,
        [nameof(Button.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(Button.FontAttributes)] = MapFontAttributes,
        // 覆盖基座：NDK Button 的 Auto 测量不含子节点（内建 label 路径不入排版），
        // 基座对 WidthRequest/HeightRequest=-1 复位 Auto 会把按钮压成零尺寸（实测回归：
        // HelloApp 全部按钮消失）。Button 仅在显式请求时设固定尺寸，-1 时保持节点默认。
        [nameof(VisualElement.WidthRequest)] = MapWidthRequest,
        [nameof(VisualElement.HeightRequest)] = MapHeightRequest,
    };

    private ArkText? _label;
    private HarmonyOS.ArkUI.Image? _image;
    private NativeChildTracker<HarmonyOS.Bindings.NativeNode.ArkUINodeBase>? _childTracker;
    private ImageSourcePartLoader? _imageSourceLoader;

    public HarmonyButtonHandler() : base(Mapper) { }

    IButton IButtonHandler.VirtualView => VirtualView;

    object IButtonHandler.PlatformView => PlatformView;

    ImageSourcePartLoader IButtonHandler.ImageSourceLoader =>
        _imageSourceLoader ??= new ImageSourcePartLoader(
            new HarmonyImageSourcePartSetter(this, _ => MapImageSource(this, VirtualView)));

    protected override ArkFlex CreatePlatformView()
    {
        var flex = new ArkFlex();
        flex.Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW;
        flex.AlignItems = ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_CENTER;
        flex.JustifyContent = ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_CENTER;
        // 默认按钮视觉：胶囊圆角 + 主题蓝底 + 默认内边距（NDK 无样式表，显式补齐）
        flex.SetBackgroundColor(HarmonyControlDefaults.ButtonBackground.ToUint());
        var r = HarmonyControlDefaults.ButtonCornerRadius;
        flex.SetBorderRadius(r, r, r, r);
        var p = HarmonyControlDefaults.ButtonPadding;
        flex.SetPaddingEdges((float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);

        _label = new ArkText();
        _label.FontSize = (float)HarmonyControlDefaults.ButtonFontSize;
        var tc = HarmonyControlDefaults.ButtonTextColor;
        _label.SetFontColor((byte)(tc.Red * 255), (byte)(tc.Green * 255), (byte)(tc.Blue * 255), (byte)(tc.Alpha * 255));
        // 纯展示节点必须透传命中：否则子 Text 挡住点击，Flex 的 CLICK 永远不触发
        _label.SetHitTestBehavior(ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_TRANSPARENT);
        _childTracker = new NativeChildTracker<HarmonyOS.Bindings.NativeNode.ArkUINodeBase>(
            flex.AddChild, flex.RemoveAllChildren);
        _childTracker.Add(_label);
        return flex;
    }

    protected override void ConnectHandler(ArkFlex platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
        platformView.SubscribeEvent(ArkUI_NodeEventType.NODE_TOUCH_EVENT, OnTouch);
    }

    protected override void DisconnectHandler(ArkFlex platformView)
    {
        platformView.UnsubscribeEvent(ArkUI_NodeEventType.NODE_ON_CLICK, OnClick);
        platformView.UnsubscribeEvent(ArkUI_NodeEventType.NODE_TOUCH_EVENT, OnTouch);
        base.DisconnectHandler(platformView);
    }

    public static void MapText(HarmonyButtonHandler h, Button v)
    {
        if (h._label is not null)
            h._label.Content = TransformText(v);
    }

    public static void MapFontSize(HarmonyButtonHandler h, Button v)
    {
        // MAUI FontSize=0(Default) 语义=平台默认；NDK 无样式表，缺省落 HarmonyControlDefaults.ButtonFontSize
        if (h._label is not null)
            h._label.FontSize = v.FontSize > 0 ? (float)v.FontSize : (float)HarmonyControlDefaults.ButtonFontSize;
    }

    public static void MapFontFamily(HarmonyButtonHandler h, Button v)
    {
        if (!string.IsNullOrEmpty(v.FontFamily))
            h._label?.SetFontFamily(v.FontFamily);
    }

    public static void MapTextColor(HarmonyButtonHandler h, Button v)
    {
        if (v.TextColor is { } c && h._label is not null)
            h._label.SetFontColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
    }

    public static void MapImageSource(HarmonyButtonHandler h, Button v)
    {
        var src = ImageSourceResolver.Resolve(v.ImageSource);
        if (src is null)
        {
            h._image?.SetVisibility(ArkUI_Visibility.ARKUI_VISIBILITY_NONE);
            return;
        }

        h._image ??= new HarmonyOS.ArkUI.Image();
        h._image.Src = src;
        h._image.SetHitTestBehavior(ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_TRANSPARENT);
        MapContentLayout(h, v);
    }

    public static void MapContentLayout(HarmonyButtonHandler h, Button v)
    {
        // Flex 通过子节点顺序表达内容先后；左右/上下布局由 ArkUI 自身排版。
        if (h._image is null || h._label is null)
            return;

        var spacing = (float)Math.Max(v.ContentLayout.Spacing, 0);
        var position = v.ContentLayout.Position;
        var children = new List<HarmonyOS.Bindings.NativeNode.ArkUINodeBase>();
        if (position == Button.ButtonContentLayout.ImagePosition.Left)
        {
            h._image.SetMarginEdges(0, spacing, 0, 0);
            children.Add(h._image);
            children.Add(h._label);
            h.PlatformView.Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW;
        }
        else if (position == Button.ButtonContentLayout.ImagePosition.Right)
        {
            h._label.SetMarginEdges(0, spacing, 0, 0);
            children.Add(h._label);
            children.Add(h._image);
            h.PlatformView.Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW;
        }
        else if (position == Button.ButtonContentLayout.ImagePosition.Top)
        {
            h._image.SetMarginEdges(0, 0, spacing, 0);
            children.Add(h._image);
            children.Add(h._label);
            h.PlatformView.Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_COLUMN;
        }
        else
        {
            h._label.SetMarginEdges(0, 0, spacing, 0);
            children.Add(h._label);
            children.Add(h._image);
            h.PlatformView.Direction = ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_COLUMN;
        }

        h._childTracker?.Replace(children);
    }

    public static void MapTextTransform(HarmonyButtonHandler h, Button v)
    {
        if (h._label is not null)
            h._label.Content = TransformText(v);
    }

    public static void MapLineBreakMode(HarmonyButtonHandler h, Button v)
    {
        if (h._label is null)
            return;

        h._label.SetMaxLines(v.LineBreakMode switch
        {
            LineBreakMode.NoWrap => 1,
            LineBreakMode.HeadTruncation => 1,
            LineBreakMode.MiddleTruncation => 1,
            LineBreakMode.TailTruncation => 1,
            _ => 0,
        });
        h._label.SetEllipsisMode(v.LineBreakMode switch
        {
            LineBreakMode.HeadTruncation => ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_START,
            LineBreakMode.MiddleTruncation => ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_CENTER,
            LineBreakMode.TailTruncation => ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_END,
            _ => ArkUI_EllipsisMode.ARKUI_ELLIPSIS_MODE_END,
        });
    }

    public static void MapCharacterSpacing(HarmonyButtonHandler h, Button v)
    {
        if (h._label is not null)
            h._label.SetLetterSpacing((float)v.CharacterSpacing);
    }

    public static void MapFontAttributes(HarmonyButtonHandler h, Button v)
    {
        if (h._label is null)
            return;

        h._label.SetFontWeight(
            v.FontAttributes.HasFlag(FontAttributes.Bold)
                ? ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W700
                : ArkUI_FontWeight.ARKUI_FONT_WEIGHT_W400);
        h._label.SetFontStyle(
            v.FontAttributes.HasFlag(FontAttributes.Italic)
                ? ArkUI_FontStyle.ARKUI_FONT_STYLE_ITALIC
                : ArkUI_FontStyle.ARKUI_FONT_STYLE_NORMAL);
    }

    public static void MapBackgroundColor(HarmonyButtonHandler h, Button v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapBorderColor(HarmonyButtonHandler h, Button v)
    {
        if (v.BorderColor is { } c)
            h.PlatformView.SetBorderColor(c.ToUint());
    }

    public static void MapBorderWidth(HarmonyButtonHandler h, Button v)
    {
        // MAUI 默认 -1 = 未设置；ArkUI 边框宽无"未设置"值，仅在显式非负时下发
        if (v.BorderWidth >= 0)
            h.PlatformView.SetBorderWidth((float)v.BorderWidth);
    }

    public static void MapCornerRadius(HarmonyButtonHandler h, Button v)
    {
        if (v.CornerRadius >= 0)
            h.PlatformView.SetBorderRadius(v.CornerRadius, v.CornerRadius, v.CornerRadius, v.CornerRadius);
    }

    public static void MapPadding(HarmonyButtonHandler h, Button v)
    {
        // MAUI Button.Padding 默认 0 语义=平台默认内边距；显式非零时才覆写 CreatePlatformView 的默认值。
        // 已知取舍：显式 Padding(0) 无法回到零内边距（0 被当作 sentinel），NDK 无样式表可查询区分。
        var p = v.Padding;
        if (p.Top > 0 || p.Right > 0 || p.Bottom > 0 || p.Left > 0)
            h.PlatformView.SetPaddingEdges((float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
    }

    public static void MapWidthRequest(HarmonyButtonHandler h, Button v)
    {
        if (v.WidthRequest >= 0)
            h.PlatformView.SetWidth((float)v.WidthRequest);
    }

    public static void MapHeightRequest(HarmonyButtonHandler h, Button v)
    {
        if (v.HeightRequest >= 0)
            h.PlatformView.SetHeight((float)v.HeightRequest);
    }

    private void OnClick(HarmonyOS.Bindings.NativeNode.ArkUINodeEvent _)
    {
        if (LogClick) HarmonyOS.Interop.HiLog.Debug("HarmonyHost", $"[Click] {VirtualView?.Text}");
        VirtualView?.SendClicked();
    }

    private void OnTouch(ArkUINodeEvent e)
    {
        var pointer = ArkUIPointerEvent.From(e.InputEvent);
        if (pointer.IsNull)
            return;

        if (VirtualView is { } button)
            ButtonTouchRouter.Route(button, pointer.TouchAction);
    }

    private static string TransformText(Button v)
    {
        var text = v.Text ?? string.Empty;
        return v.TextTransform switch
        {
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.Lowercase => text.ToLowerInvariant(),
            _ => text,
        };
    }
}
