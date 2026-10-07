// HarmonyControlDefaults：MAUI ↔ ArkUI 默认值差异映射中心。
//
// 设计原则（对齐 ArkUI SDK .d.ts 中的 Default value 标注）：
//   1. ArkUI 已有合理默认的属性 → Handler 不覆写，原生默认生效
//      （Text FontSize=16fp、FontColor=主题色、Button Type=ROUNDED_RECTANGLE 等）
//   2. MAUI 语义与 ArkUI 冲突的属性 → 在这里记录差异值，Handler 显式设置
//   3. MAUI sentinel 值（FontSize=0、BorderWidth=-1、Color=null）→ 不设，让 ArkUI 原生默认接管
//   4. 颜色类属性 → 优先走 ArkUI 主题资源（$r('sys.color.xxx')），不硬编码
//
// 全部 static 可变属性，应用启动前可全局覆写。
// ArkUI 原生默认值来源：DevEco Studio SDK component/*.d.ts 的 Default value 标注。

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyControlDefaults
{
    // ── Button 组合实现（Stack+Text）的视觉默认：NDK 无样式表/主题资源，显式定义 ──

    /// <summary>Button 默认背景色。对齐 ArkUI 胶囊按钮主题蓝（sys.color.brand 浅色 #FF0A59F7）。
    /// NDK C API 无法引用 $r('sys.color.xxx')，此处硬编码浅色主题值；深色适配待主题管线。</summary>
    public static Microsoft.Maui.Graphics.Color ButtonBackground { get; set; } = new Microsoft.Maui.Graphics.Color(0x0A, 0x59, 0xF7);

    /// <summary>Button 默认文字色（ArkUI font_on_primary，浅色主题下为白）。</summary>
    public static Microsoft.Maui.Graphics.Color ButtonTextColor { get; set; } = Microsoft.Maui.Graphics.Colors.White;

    /// <summary>Button 默认圆角（vp）。ArkUI ROUNDED_RECTANGLE 自带 20vp 圆角。</summary>
    public static float ButtonCornerRadius { get; set; } = 20f;

    /// <summary>Button 默认内边距（vp）：横向 24、纵向 8，配合 16fp 字号约 40vp 高。</summary>
    public static Microsoft.Maui.Thickness ButtonPadding { get; set; } = new Microsoft.Maui.Thickness(24, 8);

    // ── MAUI 与 ArkUI 有语义差异的属性 ──

    /// <summary>StackLayout.Spacing MAUI 默认 6.0；ArkUI Column/Row Space 默认 0。
    /// Handler 在 Spacing>0 时叠加到子节点 Margin，0 时不干预（与 ArkUI 一致）。</summary>
    public static double StackLayoutSpacing { get; set; } = 6.0;

    /// <summary>Button 内部 Text 字号兜底。ArkTS 声明式 API 有 16fp 内建默认，
    /// 但 NDK C API 无样式表——不显式设则内部 Text 零尺寸不可见。</summary>
    public static double ButtonFontSize { get; set; } = 16.0;

    /// <summary>通用 Text 字号兜底。ArkUI 默认 16fp；NDK 无样式表时显式设置。</summary>
    public static double TextFontSizeDefault { get; set; } = 16.0;

    /// <summary>次级/详情 Text 字号兜底。</summary>
    public static double DetailTextFontSizeDefault { get; set; } = 12.0;

    /// <summary>状态/列表行 Text 字号兜底。</summary>
    public static double StatusTextFontSizeDefault { get; set; } = 14.0;

    /// <summary>ActivityIndicator 默认描边色；MAUI 未显式设置时使用。</summary>
    public static Microsoft.Maui.Graphics.Color ActivityIndicatorColorDefault { get; set; } =
        Microsoft.Maui.Graphics.Colors.Gray;

    /// <summary>Button.CornerRadius MAUI sentinel -1 = 平台默认；
    /// ArkUI ROUNDED_RECTANGLE 自带 20vp 圆角，sentinel 时不覆写。</summary>
    public static int ButtonCornerRadiusSentinel { get; set; } = -1;

    /// <summary>Button.BorderWidth MAUI sentinel -1 = 平台默认；
    /// ArkUI 按钮默认无边框，sentinel 时不覆写。</summary>
    public static double ButtonBorderWidthSentinel { get; set; } = -1.0;

    /// <summary>Label.MaxLines MAUI 默认 -1 = 不限制；
    /// ArkUI Text 默认不限制，sentinel 时不设 MAX_LINES。</summary>
    public static int LabelMaxLinesSentinel { get; set; } = -1;

    // ── 以下属性 ArkUI 原生默认已与 MAUI 语义一致，Handler 不覆写 ──
    // （记录在此供文档/审计用，实际值由 ArkUI 引擎决定）

    /// <summary>Text FontSize：ArkUI 默认 16fp == MAUI 平台默认。FontSize<=0 时不设。</summary>
    public const double FontSizeSentinel = 0.0;

    /// <summary>Text TextAlign：ArkUI 默认 Start == MAUI TextAlignment.Start。</summary>
    public const string TextAlignmentDefault = "Start";

    /// <summary>Text FontColor：ArkUI 默认主题色 $r('sys.color.font_primary')（深灰 #e6182431）。
    /// Color=null 时不设，让 ArkUI 主题色（含深浅色适配）生效。</summary>
    public const string TextColorDefault = "ArkUI Theme";

    /// <summary>Button FontColor：ArkUI 默认 $r('sys.color.font_on_primary')（主题资源）。
    /// Color=null 时不设，让主题色生效。</summary>
    public const string ButtonTextColorDefault = "ArkUI Theme";

    /// <summary>CheckBox Shape：ArkUI 默认 CIRCLE。MAUI 无对应属性，不覆写。</summary>
    public const string CheckBoxShapeDefault = "Circle";

    /// <summary>Button Type：ArkUI API 18+ 默认 ROUNDED_RECTANGLE。MAUI 无对应属性，不覆写。</summary>
    public const string ButtonTypeDefault = "RoundedRectangle";

    /// <summary>Opacity：ArkUI 默认 1.0 == MAUI 默认 1.0。</summary>
    public const float OpacityDefault = 1.0f;
}
