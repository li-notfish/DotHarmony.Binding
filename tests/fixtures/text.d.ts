/*
 * Copyright (c) 2021-2024 Huawei Device Co., Ltd.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
/**
 * @file
 * @kit ArkUI
 */
/**
 * Defines the configuration object for text overflow behavior.
 *
 * > **NOTE**
 * >
 * > To standardize anonymous object definitions, the element definitions here have been revised in API version 18.
 * > While historical version information is preserved for anonymous objects, there may be cases where the outer element
 * > 's @since version number is higher than inner elements'. This does not affect interface usability.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
declare interface TextOverflowOptions {
    /**
     * Display mode of overflowing text.
     *
     * Default value: **TextOverflow.Clip**
     *
     * @default TextOverflow.Clip [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    overflow: TextOverflow;
}
/**
 * The **Text** component is used to display a piece of textual information.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
interface TextInterface {
    /**
     *
     * Defines the constructor of Text.
     * @param { string | Resource } content - Plain text. This parameter takes effect when the child component
     *     [Span]{@link ./span} is not included and [styled string]{@link ./styled_string} is not set.<br>Default value:
     *     **' '**<br>**NOTE**<br>Priority of displayed content: Styled string > Content of the **Span** component > Text
     *     content of the **Text** component.
     * @param { TextOptions } value - Initialization options of the component. [since 11]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (content?: string | Resource, value?: TextOptions): TextAttribute;
}
/**
 * In addition to the
 * [universal attributes](docroot://reference/apis-arkui/arkui-ts/ts-component-general-attributes.md), the following
 * attributes are supported.
 *
 * In addition to the [universal events](docroot://reference/apis-arkui/arkui-ts/ts-component-general-events.md), the
 * following events are supported.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare class TextAttribute extends CommonMethod<TextAttribute> {
    /**
     * Sets the text style, covering the font size, font width, font family, and font style.
     *
     * @param { Font } value - Text style.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    font(value: Font): TextAttribute;
    /**
     * Sets the font style, with support for font settings.
     *
     * It is only effective for the **Text** component, not for its child components.
     *
     * @param { Font } fontValue - Sets the text style.
     * @param { FontSettingOptions } options - Font settings.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    font(fontValue: Font, options?: FontSettingOptions): TextAttribute;
    /**
     * Sets the font color.
     *
     * @param { ResourceColor } value - Font color.<br>Default value: **'#e6182431'**<br>Default value for wearables:
     *     **'#c5ffffff'**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontColor(value: ResourceColor): TextAttribute;
    /**
     * Sets the text size.
     *
     * @param { number | string | Resource } value - Font size. If **fontSize** is of the number type, the unit fp is
     *     used. This parameter cannot be set in percentage.<br>Default value: **16fp**<br>Default value on wearable
     *     devices: **15fp**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontSize(value: number | string | Resource): TextAttribute;
    /**
     * Sets the minimum font size.
     *
     * For the string type, numeric string values with optional units, for example, **"10"** or **"10fp"**, are supported.
     *
     * For the setting to take effect, this attribute must be used together with
     * [maxFontSize]{@link TextAttribute#maxFontSize} and [maxLines]{@link TextAttribute#maxLines}, or layout constraint
     * settings.
     *
     * When the adaptive font size is used, the **fontSize** settings do not take effect.
     *
     * If the value of **minFontSize** is less than or equal to 0, the adaptive font sizing feature is disabled. In such
     * cases, the [fontSize]{@link TextAttribute#fontSize} attribute is used instead. If **fontSize** is not set, the
     * default value will apply.
     *
     * Since API version 18, adaptive font sizing is supported on child components and styled strings, and text segments
     * without an explicitly defined font size will automatically adjust based on the available space.
     *
     * @param { number | string | Resource } value - Minimum font size.<br>Unit: [fp]{@link common}
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    minFontSize(value: number | string | Resource): TextAttribute;
    /**
     * Sets the maximum font size.
     *
     * For the string type, numeric string values with optional units, for example, **"10"** or **"10fp"**, are supported.
     *
     * For the setting to take effect, this attribute must be used together with
     * [minFontSize]{@link TextAttribute#minFontSize} and [maxLines]{@link TextAttribute#maxLines}, or layout constraint
     * settings.
     *
     * When the adaptive font size is used, the **fontSize** settings do not take effect.
     *
     * If the value of **maxFontSize** is less than or equal to 0 or is less than the value of **minFontSize**, the
     * adaptive font sizing feature is disabled. In such cases, the [fontSize]{@link TextAttribute#fontSize} attribute is
     * used instead. If **fontSize** is not set, the default value will apply.
     *
     * Since API version 18, adaptive font sizing is supported on child components and styled strings, and text segments
     * without an explicitly defined font size will automatically adjust based on the available space.
     *
     * @param { number | string | Resource } value - Maximum font size.<br>Unit: [fp]{@link common}
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    maxFontSize(value: number | string | Resource): TextAttribute;
    /**
     * Sets the minimum font scale factor for text.
     *
     * @param { number | Resource } scale - Minimum font scale factor for text.<br>Value range: [0, 1]<br>**NOTE**<br>
     *     Values less than 0 are treated as 0, and values greater than 1 are treated as 1. Other invalid values do not
     *     take effect by default.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 20]
     * @atomicservice
     * @since 12
     */
    minFontScale(scale: number | Resource): TextAttribute;
    /**
     * Sets the maximum font scale factor for text.
     *
     * @param { number | Resource  } scale - Maximum font scale factor for text.<br>Value range:
     *     [1, +∞)<br>**NOTE**<br>Values less than 1 are treated as **1**. Other invalid values are ineffective by default.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 20]
     * @atomicservice
     * @since 12
     */
    maxFontScale(scale: number | Resource): TextAttribute;
    /**
     * Sets the font style.
     *
     * @param { FontStyle } value - Font style.<br>Default value: **FontStyle.Normal**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontStyle(value: FontStyle): TextAttribute;
    /**
     * Sets the font weight. If the value is too large, the text may be clipped depending on the font.
     *
     * @param { number | FontWeight | string } value - Font weight. For the number type, the value range is [100, 900], at
     *     an interval of 100. The default value is **400**. A larger value indicates a heavier font weight. For the
     *     string type, only strings that represent a number, for example, **400**, and the following enumerated values of
     *     **FontWeight** are supported: **bold**, **bolder**, **lighter**, **regular**, and **medium**.<br>Default value:
     *     **FontWeight.Normal**<br>Default value on wearable devices: **FontWeight.Regular**<br>The
     *     [Resource]{@link Resource} type is supported since API version 20. [since 7 - 19]
     * @param { number | FontWeight | ResourceStr } value - Font weight. For the number type, the value range is
     *     [100, 900], at an interval of 100. The default value is **400**. A larger value indicates a heavier font
     *     weight. For the string type, only strings that represent a number, for example, **400**, and the following
     *     enumerated values of **FontWeight** are supported: **bold**, **bolder**, **lighter**, **regular**, and
     *     **medium**.<br>Default value: **FontWeight.Normal**<br>Default value on wearable devices:
     *     **FontWeight.Regular**<br>The [Resource]{@link Resource} type is supported since API version 20. [since 20]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontWeight(value: number | FontWeight | ResourceStr): TextAttribute;
    /**
     * Sets the text font weight, with support for font settings.
     *
     * It is only effective for the **Text** component, not for its child components.<!--RP4--><!--RP4End-->
     *
     * @param { number | FontWeight | string } weight - Font weight. For the number type, the value ranges from 100 to 90
     *     0, at an interval of 100. A larger value indicates a heavier font weight. The default value is **400**. For the
     *     string type, only strings that represent a number, for example, **400**, and the following enumerated values of
     *     **FontWeight** are supported: **bold**, **bolder**, **lighter**, **regular**, and **medium**.<br>The
     *     [Resource]{@link Resource} type is supported since API version 20. [since 12 - 19]
     * @param { number | FontWeight | ResourceStr } weight - Font weight. For the number type, the value ranges from 100
     *     to 900, at an interval of 100. A larger value indicates a heavier font weight. The default value is **400**.
     *     For the string type, only strings that represent a number, for example, **400**, and the following enumerated
     *     values of **FontWeight** are supported: **bold**, **bolder**, **lighter**, **regular**, and **medium**.<br>The
     *     [Resource]{@link Resource} type is supported since API version 20. [since 20]
     * @param { FontSettingOptions } options - Font setting options.<br>When **enableVariableFontWeight** in **options**
     *     is set to **false**, variable font weight adjustment is disabled. If **weight** is set to a value at intervals
     *     of 100 within [100, 900], the font weight uses the specified value. If **weight** is set to a value that is not
     *     a multiple of 100, the default value **400** is used.<br>When **enableVariableFontWeight** in **options** is
     *     set to **true**, variable font weight adjustment is enabled. If **weight** is set to any integer within
     *     [100, 900], the font weight uses the specified value.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    fontWeight(weight: number | FontWeight | ResourceStr, options?: FontSettingOptions): TextAttribute;
    /**
     * Sets the line spacing of the text. If the value specified is less than or equal to 0, the default value **0** is
     * used.
     *
     * @param { LengthMetrics } value - Line spacing. Default value: **0**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    lineSpacing(value: LengthMetrics): TextAttribute;
    /**
     * Sets the line spacing for text. When **LineSpacingOptions** is not specified, line spacing is applied above the
     * first line and below the last line by default.
     *
     * @param { LengthMetrics } value - Line spacing. Values less than or equal to 0 are treated as the default value
     *     **0**.
     * @param { LineSpacingOptions } options - Line spacing configuration options.<br>Default value:
     *     **{ onlyBetweenLines: false }**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    lineSpacing(value: LengthMetrics, options?: LineSpacingOptions): TextAttribute;
    /**
     * Sets the horizontal alignment of the text.
     *
     * The text takes up the full width of the **Text** component.
     *
     * The vertical position of the text paragraph can be controlled by the
     * [align]{@link CommonMethod#align(value: Alignment)} attribute, but the horizontal position cannot be controlled by
     * **align** in this component. The specific effects are as follows:
     *
     * - **Alignment.TopStart**, **Alignment.Top**, **Alignment.TopEnd**: Content aligns to the top.
     * - **Alignment.Start**, **Alignment.Center**, **Alignment.End**: Content is centered vertically.
     * - **Alignment.BottomStart**, **Alignment.Bottom**, **Alignment.BottomEnd:** Content aligns to the bottom.
     *
     * When **textAlign** is set to **TextAlign.JUSTIFY**, the [wordBreak]{@link TextAttribute#wordBreak} property must be
     * configured according to the text content. The last line of text aligns to the start horizontally and does not
     * participate in justification.
     *
     * > **NOTE**
     * >
     * > **textAlign** only adjusts the overall text layout and does not affect character display order. For character
     * > display order adjustment, see
     * > [Bidirectional Text Layout and Alignment](docroot://ui/arkts-internationalization.md#bidirectional-text-layout-and-alignment).
     *
     * @param { TextAlign } value - Horizontal alignment of the text.<br>Default value: **TextAlign.Start**<br>Default
     *     value on wearable devices: **TextAlign.Center**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    textAlign(value: TextAlign): TextAttribute;
    /**
     * Sets the vertical alignment of the text.
     *
     * > **NOTE**
     * >
     * > - When this API and [halfLeading]{@link TextAttribute#halfLeading} are both set, **halfLeading** does not take
     * > effect.
     * >
     * > - The effect of this attribute is noticeable only when the same font size is used in a paragraph and
     * > [lineHeight]{@link TextAttribute#lineHeight} is set, or when different font sizes are mixed in a paragraph.
     * > Otherwise, the effect is the same regardless of whether this attribute is set or which enum value is used. The
     * > **SuperscriptStyle** in [TextStyle]{@link TextStyle} takes effect only when the value of
     * > [TextVerticalAlign]{@link TextVerticalAlign} is set to **TextVerticalAlign.BASELINE**. In other vertical
     * > alignment modes, the superscript and subscript texts are displayed in the same way as the normal text.
     *
     * @param { Optional<TextVerticalAlign> } textVerticalAlign - Vertical alignment of the text.<br>Default value:
     *     **TextVerticalAlign.BASELINE**
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    textVerticalAlign(textVerticalAlign: Optional<TextVerticalAlign>): TextAttribute;
    /**
     * Sets the vertical alignment of the text content area within the component.
     *
     * This API takes effect only when the height of the text content exceeds the component's height.
     *
     * @param { Optional<TextContentAlign> } textContentAlign - Vertical alignment of the text.<br>If the value is
     *     **undefined** or invalid, alignment defaults to **Center**.
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 21
     */
    textContentAlign(textContentAlign: Optional<TextContentAlign>): TextAttribute;
    /**
     * Sets the text line height.
     *
     * If the value is less than or equal to **0**, the line height is unrestricted and adapts to the font size. When the
     * value is a number, the unit is fp. For the string type, numeric string values with optional units, for example,
     * **"10"** or **"10fp"**, are supported.
     *
     * > **NOTE**
     * >
     * > If certain characters have significantly taller glyphs than others in the same line, layout anomalies such as
     * > clipping, overlapping, or misalignment may occur. In this case, adjust component attributes such as height and
     * > line height to ensure proper layout rendering.
     *
     * @param { number | string | Resource } value - Text line height.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    lineHeight(value: number | string | Resource): TextAttribute;
    /**
     * Sets the minimum line height of text. If the value is less than or equal to 0, the default value **0** is used.
     *
     * @param { LengthMetrics | undefined } value - Minimum line height of text. Percentage values are not supported.<br>
     *     Values less than or equal to 0 are treated as **0**.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    minLineHeight(value: LengthMetrics | undefined): TextAttribute;
    /**
     * Sets the maximum line height of text. If the value is less than or equal to 0, the maximum line height is
     * unrestricted.
     *
     * If **maxLineHeight** is less than **minLineHeight**, **maxLineHeight** takes effect using the value of
     * **minLineHeight**.
     *
     * @param { LengthMetrics | undefined } value - Maximum line height of text. Percentage values are not supported.<br>
     *     Values less than or equal to 0 are treated as **0**. When the value is set to **0**, the maximum line height is
     *     unrestricted.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    maxLineHeight(value: LengthMetrics | undefined): TextAttribute;
    /**
     * Sets the line height of text in multiple mode.
     *
     * The line height equals the input parameter **value** multiplied by **fontHeight**.
     *
     * > **NOTE**
     * >
     * > When both this API and [lineHeight]{@link TextAttribute#lineHeight} are set, only **lineHeightMultiple** takes
     * > effect.
     *
     * @param { number | undefined } value - Multiplier for the line height.<br>Value range: ≥ 0<br>Values ≤ 0 are treated
     *     as **0**. When the value is set to **0**, the default line height is used. Decimal values are supported.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    lineHeightMultiple(value: number | undefined): TextAttribute;
    /**
     * Sets the display mode for overflowing text.
     *
     * When [TextOverflowOptions]{@link TextOverflowOptions} is set to **TextOverflow.None**, **TextOverflow.Clip**, or
     * **TextOverflow.Ellipsis**:
     *
     * - **TextOverflow.None** or **TextOverflow.Clip**: Text is truncated when it exceeds the maximum number of lines.
     * - **TextOverflow.Ellipsis**: Overflowing text is replaced with an ellipsis (...).
     * - This must be used with [maxLines]{@link TextAttribute#maxLines} for the settings to take effect.
     * - Line breaking behavior is controlled by [wordBreak]{@link TextAttribute#wordBreak}. By default, it uses
     * **WordBreak.BREAK_WORD**, which breaks text by word (for example, English text is broken at word boundaries). To
     * break text by character, set **wordBreak** to **WordBreak.BREAK_ALL**.
     * - Line wrapping behavior is governed by [lineBreakStrategy]{@link TextAttribute#lineBreakStrategy} which takes
     * effect only when [wordBreak]{@link TextAttribute#wordBreak} is not **WordBreak.BREAK_ALL**. Hyphens are not
     * supported.
     * - Since API version 11, it is recommended that you configure both [textOverflow]{@link TextAttribute#textOverflow}
     * and [wordBreak]{@link TextAttribute#wordBreak} to control truncation behavior. For details, see
     * [Example 4](docroot://reference/apis-arkui/arkui-ts/ts-basic-components-text.md#example-4-setting-text-wrapping-and-line-breaking)
     * <!--RP1--><!--RP1End-->.
     *
     * When **TextOverflowOptions** is set to **TextOverflow.MARQUEE**:
     *
     * - Text scrolls horizontally within a single line.
     * - [maxLines]{@link TextAttribute#maxLines} and[copyOption]{@link TextAttribute#copyOption} are ignored.
     * - The [clip]{@link CommonMethod#clip(value: boolean)} attribute of the **Text** component defaults to **true**.
     * - [CustomSpan]{@link CustomSpan} is not supported in marquee mode.
     * - Behavior of [textAlign]{@link TextAttribute#textAlign}: If the text does not scroll, **textAlign** applies; if
     * the text scrolls, **textAlign** is ignored.
     * - Since API version 12, **TextOverflow.MARQUEE** is available for the **ImageSpan** component, where the text and
     * images are allowed to scroll within a single line.
     *
     * @param { object } value [since 7 - 17]
     * @param { TextOverflowOptions } options - Display mode when the text is too long. [since 18]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    textOverflow(options: TextOverflowOptions): TextAttribute;
    /**
     * Sets the font family.
     *
     * > **NOTE**
     * >
     * > You can use [loadFontSync]{@link @ohos.graphics.text:text.FontCollection#loadFontSync} to register custom fonts.
     *
     * @param { string | Resource } value - Font family. Default font: **'HarmonyOS Sans'**<br>To specify multiple fonts,
     *     separate them with commas (,), and fonts are applied in priority order. Example: **'Arial, HarmonyOS Sans'**.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontFamily(value: string | Resource): TextAttribute;
    /**
     * Sets the maximum number of lines for text.
     *
     * By default, text is automatically folded. If this attribute is specified, the text will not exceed the specified
     * number of lines. If there is extra text, you can use [textOverflow]{@link TextAttribute#textOverflow} to specify
     * how it is displayed.
     *
     * @param { number } value - Maximum number of lines of the text.<br>**NOTE**<br>Value range: [0, *INT32_MAX*]<br>If
     *     this parameter is set to **0**, no text content is displayed.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    maxLines(value: number): TextAttribute;
    /**
     * Sets the minimum number of lines for text.
     *
     * If the actual text height is less than the height for the minimum number of lines, the component uses the height
     * corresponding to the minimum number of lines.
     *
     * When this API and [maxLines]{@link TextAttribute#maxLines} are both set, the minimum line height cannot exceed the
     * maximum line height.
     *
     * If [constraintSize]{@link CommonMethod#constraintSize} is set for the text, the component height is confined within
     * the [constraintSize]{@link CommonMethod#constraintSize} bounds.
     *
     * @param { Optional<number> } minLines - Minimum number of lines of the text.<br>Value range: [0, *INT32_MAX*]<br>
     *     Values less than 0 are clamped to **0**.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    minLines(minLines: Optional<number>): TextAttribute;
    /**
     * Style and color of the text decorative line.
     *
     * > **NOTE**
     * >
     * > When the bottom contour of a character intersects with the decoration, underline avoidance is triggered, commonly
     * > affecting characters like "g", "j", "y", "q", and "p."
     * >
     * > If the decoration color is set to **Color.Transparent**, it inherits the text color of the first character in
     * > each line. If the decoration color is set to **"#00FFFFFF"**, the line becomes fully transparent.
     *
     * @param { object } value - Style of the text decorative line.<br>Default value:<br>{<br> type:
     *     TextDecorationType.None,<br> color: Color.Black,<br> style: TextDecorationStyle.SOLID <br>}<br>**NOTE**<br>The
     *     **style** parameter cannot be used in widgets. [since 7 - 11]
     * @param { DecorationStyleInterface } value - Style of the text decorative line.<br>Default value:<br>{<br> type:
     *     TextDecorationType.None,<br> color: Color.Black,<br> style: TextDecorationStyle.SOLID <br>}<br>**NOTE**<br>The
     *     **style** parameter cannot be used in widgets. [since 12]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    decoration(value: DecorationStyleInterface): TextAttribute;
    /**
     * Sets the letter spacing for a text style.
     *
     * If the value specified is a percentage or **0**, the default value is used. For the string type, numeric string
     * values with optional units, for example, **"10"** or **"10fp"**, are supported.
     *
     * Negative values compress text. Excessive compression may reduce content area to zero, hiding content.
     *
     * This setting applies to every character, including those at line endings.
     *
     * @param { number | string } value - Letter spacing.<br>Default value: **0**<br>Unit: [fp]{@link common}<br>The
     *     [Resource]{@link Resource} type is supported since API version 20. [since 7 - 19]
     * @param { number | ResourceStr } value - Letter spacing.<br>Default value: **0**<br>Unit: [fp]{@link common}<br>The
     *     [Resource]{@link Resource} type is supported since API version 20. [since 20]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    letterSpacing(value: number | ResourceStr): TextAttribute;
    /**
     * Sets the text case.
     *
     * @param { TextCase } value - Text case.<br>Default value: **TextCase.Normal**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    textCase(value: TextCase): TextAttribute;
    /**
     * Sets the offset of the text baseline.
     *
     * Percentage values follow default display behavior.
     *
     * A positive value moves the content upwards, while a negative value moves it downwards.
     *
     * @param { number | string } value - Offset of the text baseline.<br>Unit: fp. Default value: 0. [since 7 - 19]
     * @param { number | ResourceStr } value - Offset of the text baseline.<br>Unit: fp. Default value: 0. [since 20]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    baselineOffset(value: number | ResourceStr): TextAttribute;
    /**
     * Sets whether copy and paste operations are allowed.
     *
     * Since API version 20, copied text from the **Text** component includes HTML-formatted content in the pasteboard.
     *
     * - When the **Text** component contains child elements, only [Span]{@link ./span} and
     * [ImageSpan]{@link ./image_span} support HTML-formatted pasteboard content.
     * - For styled strings, refer to [toHtml]{@link StyledString#toHtml} for supported HTML conversion scope.
     *
     * When **copyOption** is set to **CopyOptions.InApp** or **CopyOptions.LocalDevice**:
     *
     * - A long press on the text will display a menu that offers the copy and select-all options.
     * - By default, selected text is draggable. To disable dragging, set **draggable** to **false**.
     * - To support **Ctrl+C** copying, also set [textSelectable]{@link TextAttribute#textSelectable} to
     * **TextSelectableMode.SELECTABLE_FOCUSABLE**.
     *
     * The **Text** component listens for **onClick**, which is a non-bubbling event. To allow parent components to
     * respond to clicks within the **Text** area, use [parallelGesture]{@link CommonMethod#parallelGesture} on the
     * parent. For implementation guidance, see
     * [Example 7: Setting Text Recognition](docroot://reference/apis-arkui/arkui-ts/ts-basic-components-text.md#example-7-setting-text-recognition).
     *
     * Because widgets do not have the long press event, the menu will not be displayed when users long press text.
     *
     * @param { CopyOptions } value - Whether copy and paste operations are allowed.<br>Default value:
     *     **CopyOptions.None**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    copyOption(value: CopyOptions): TextAttribute;
    /**
     * Sets the drag effect of the selected text.
     *
     * This attribute cannot be used together with the [onDragStart]{@link CommonMethod#onDragStart} event.
     *
     * If set to **true**, **draggable** must be used in conjunction with [CopyOptions]{@link CopyOptions}. When
     * **copyOptions** is set to **CopyOptions.InApp** or **CopyOptions.LocalDevice**, the selected text becomes draggable
     * and can be copied into a text box.
     *
     * @param { boolean } value - Drag effect of the selected text.<br>**true**: The selected text is draggable.
     *     **false**: The selected text is not draggable.<br>Default value: **false**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 18]
     * @atomicservice [since 11]
     * @since 9
     */
    draggable(value: boolean): TextAttribute;
    /**
     * Sets the text shadow.
     *
     * Intelligent color extraction is not supported for the **type**, **fill**, and **color** fields of the
     * **ShadowOptions** object.
     *
     * Since API version 11, this API supports input parameters in an array to implement multiple text shadows.
     *
     * @param { ShadowOptions } value - Text shadow. [since 10 - 10]
     * @param { ShadowOptions | Array<ShadowOptions> } value - Text shadow. [since 11]
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 11]
     * @since 10
     */
    textShadow(value: ShadowOptions | Array<ShadowOptions>): TextAttribute;
    /**
     * Sets the font size adjustment strategy for adaptive text layout.
     *
     * The available modes are as follows:
     *
     * - **MAX_LINES_FIRST**: prioritizes using the [maxLines]{@link TextAttribute#maxLines} attribute to control text
     * height. If the **maxLines** setting results in a layout beyond the layout constraints, the text will shrink to a
     * font size between [minFontSize]{@link TextAttribute#minFontSize} and [maxFontSize]{@link TextAttribute#maxFontSize}
     * to allow for more content to be shown.
     * - **MIN_FONT_SIZE_FIRST**: prioritizes using the **minFontSize** attribute to control text height. If the text fits
     * on one line at **minFontSize**, the system attempts to increase the font size between **minFontSize** and
     * **maxFontSize** to fill the line with the largest available font size. If the text cannot fit on a single line even
     * at **minFontSize**, it sticks with **minFontSize**.
     * - **LAYOUT_CONSTRAINT_FIRST**: prioritizes using layout constraints to control text height. If the resultant layout
     * is beyond the layout constraints, the text will shrink to a font size between **minFontSize** and **maxFontSize**
     * to respect the layout constraints. If the text still extends beyond the layout constraints after shrinking to
     * **minFontSize**, the lines that exceed the constraints are deleted.
     *
     * @param { TextHeightAdaptivePolicy } value - How the adaptive height is determined for the text.<br>Default value:
     *     **TextHeightAdaptivePolicy.MAX_LINES_FIRST**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    heightAdaptivePolicy(value: TextHeightAdaptivePolicy): TextAttribute;
    /**
     * Sets the indent of the first line text.
     *
     * @param { Length } value - Indent of the first line text.<br>Default value: **0**<br>Unit: [fp]{@link common}
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    textIndent(value: Length): TextAttribute;
    /**
     * Specify the tail indentation for each line in a text block.
     *
     * <p><strong>NOTE</strong>:
     * <br>When a single LengthMetrics value is provided, all lines share the same tail indent.
     * <br>When an array is provided, the i-th element specifies the tail indent for the i-th line.
     * If the number of text lines exceeds the array length, the last element in the array is used
     * for the remaining lines.
     * <br>Negative values are treated as 0.
     * <br>If the value is set to undefined, the default value 0 is used.
     * </p>
     *
     * @param { Optional<LengthMetrics | Array<LengthMetrics>> } value - The tail indent value(s).Default value is 0.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    tailIndents(value: Optional<LengthMetrics | Array<LengthMetrics>>): TextAttribute;
    /**
     * Sets the word break rule.
     *
     * By default, when **wordBreak** is not called or is set to **WordBreak.BREAK_WORD**, text is broken by word. (for
     * example, English text is broken at word boundaries).
     *
     * To break text by character, with the excess part displayed as an ellipsis (...), use **WordBreak.BREAK_ALL** in
     * combination with **{overflow: TextOverflow.Ellipsis}** and **maxLines**.
     *
     * @param { WordBreak } value - Word break rule.<br>Default value: **WordBreak.BREAK_WORD**
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    wordBreak(value: WordBreak): TextAttribute;
    /**
     * Sets the line break rule. This attribute takes effect only when [wordBreak]{@link TextAttribute#wordBreak} is not
     * **WordBreak.BREAK_ALL**. Hyphens are not supported.
     *
     * @param { LineBreakStrategy } strategy - Line break rule.<br>Default value: **LineBreakStrategy.GREEDY**
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    lineBreakStrategy(strategy: LineBreakStrategy): TextAttribute;
    /**
     * Called when data is copied to the pasteboard, which is displayed when the text box is long pressed. Currently, only
     * text can be copied.
     *
     * @param { function } callback - Callback of the listened event.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    onCopy(callback: (value: string) => void): TextAttribute;
    /**
     * Called before the copy operation is performed.
     *
     * **Since**: 26.0.0
     *
     * @param { Callback<string, boolean> } callback - The string type indicates the text to be copied.<br>The boolean
     *     type indicates whether the text can be copied. The value **true** means yes and **false** means no.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    onWillCopy(callback: Callback<string, boolean>): TextAttribute;
    /**
     * Sets text selection.
     *
     * The selected text is highlighted, with selection handles and the text selection menu displayed.
     *
     * If [copyOption]{@link TextAttribute#copyOption} is set to **CopyOptions.None**, the setting of the **selection**
     * attribute does not take effect.
     *
     * If [textOverflow]{@link TextAttribute#textOverflow} is set to **TextOverflow.MARQUEE**, the setting of the
     * **selection** attribute does not take effect.
     *
     * If the value of **selectionStart** is greater than or equal to that of **selectionEnd**, no text will be selected.
     * The value range is [0, textSize], where **textSize** indicates the maximum number of characters in the text
     * content. If the value is less than 0, the value **0** will be used. If the value is greater than **textSize**,
     * **textSize** will be used.
     *
     * If the selection range falls within a truncated or invisible area, selection is ignored. When
     * [clip]{@link CommonMethod#clip(value: boolean)} is set to **false**, the text outside the parent component can be
     * selected.
     *
     * You can obtain the selection range change result through the
     * [onTextSelectionChange]{@link TextAttribute#onTextSelectionChange} API.
     *
     * @param { number } selectionStart - Start position of the selected text.<br>Default value: **-1**
     * @param { number } selectionEnd - End position of the selected text.<br>Default value: **-1**
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    selection(selectionStart: number, selectionEnd: number): TextAttribute;
    /**
     * Sets the color of the text selection handle, also known as the caret, in the text box.
     *
     * @param { ResourceColor } color - Color of the text selection handle.<br>Default value: **'#007DFF'**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 14
     */
    caretColor(color: ResourceColor): TextAttribute;
    /**
     * Sets the background color of the selected text. If the opacity is not set, a 20% opacity will be used.
     *
     * @param { ResourceColor } color - Background color of the selected text.<br>Default value: **'#007DFF'**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 14
     */
    selectedBackgroundColor(color: ResourceColor): TextAttribute;
    /**
     * Applies gradient or solid color effects to text. Supports [RadialGradientStyle]{@link RadialGradientStyle},
     * [LinearGradientStyle]{@link LinearGradientStyle}, and [ColorShaderStyle]{@link ColorShaderStyle}. **shaderStyle**
     * takes precedence over [fontColor]{@link SymbolSpanAttribute#fontColor} and AI-based styling. For solid colors,
     * prefer using [fontColor]{@link SymbolSpanAttribute#fontColor}.
     *
     * @param { ShaderStyle } shader - Shader effect.<br>Based on the input, the system applies a radial gradient (
     *     [RadialGradientStyle]{@link RadialGradientStyle}), linear gradient (
     *     [LinearGradientStyle]{@link LinearGradientStyle}), or solid color ([ColorShaderStyle]{@link ColorShaderStyle}).
     *     <br>**NOTE**<br>If [RadialGradientStyle]{@link RadialGradientStyle} is used and the **center** parameter (from
     *     [RadialGradientOptions]{@link RadialGradientOptions}) is outside the component bounds, setting **repeating** to
     *     **true** enhances the gradient effect.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    shaderStyle(shader: ShaderStyle): TextAttribute;
    /**
     * Sets the ellipsis position.
     *
     * For the settings to work, **overflow** must be set to **TextOverflow.Ellipsis** and **maxLines** must be specified.
     * Setting **ellipsisMode** alone does not take effect.
     *
     * **EllipsisMode.START** and **EllipsisMode.CENTER** take effect only when text overflows in a single line.
     *
     * @param { EllipsisMode } value - Ellipsis position.<br>Default value: **EllipsisMode.END**
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    ellipsisMode(value: EllipsisMode): TextAttribute;
    /**
     * Sets whether to enable special entity detection within the text. Special entities are detected when
     * **enableDataDetector** is set to **true**.
     *
     * The style of detected entities is as follows: the font color is changed to blue, and a blue underline is added.
     *
     * > **NOTE**
     * >
     * > - This API takes effect only when the device has an underlying text detection capability.
     * >
     * > - When [textOverflow]{@link TextAttribute#textOverflow} is set to **TextOverflow.MARQUEE**, text special entity
     * > detection is not performed.
     *
     * <!--RP2--><!--RP2End-->
     *
     * @param { boolean } enable - Whether to enable text recognition.<br>**true**: Enable text recognition. **false**:
     *     Disable text recognition.<br>Default value: **false**
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice [since 12]
     * @since 11
     */
    enableDataDetector(enable: boolean): TextAttribute;
    /**
     * Configures text recognition settings, including entity types to detect, display styles for detected entities, and
     * long-press preview options.
     *
     * This API must be used together with [enableDataDetector]{@link TextAttribute#enableDataDetector}. It takes effect
     * only when **enableDataDetector** is set to **true**.
     *
     * @param { TextDataDetectorConfig } config - Text recognition configuration.
     * @returns { TextAttribute } The attribute of the text.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice [since 12]
     * @since 11
     */
    dataDetectorConfig(config: TextDataDetectorConfig): TextAttribute;
    /**
     * Sets whether to enable entity recognition for selected text. This API only works on devices that provide text
     * recognition.
     *
     * When **enableSelectedDataDetector** is set to **true**, all entity types are recognized by default.
     *
     * This feature is only effective when [CopyOptions]{@link CopyOptions} is set to **CopyOptions.LocalDevice** or
     * **CopyOptions.CrossDevice**.
     *
     * @param { boolean | undefined } enable - Whether to enable entity recognition for selected text.<br>**true**: Entity
     *     recognition is enabled. **false**: Entity recognition is disabled. Default value: **true**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 22
     */
    enableSelectedDataDetector(enable: boolean | undefined): TextAttribute;
    /**
     * Sets the custom selection menu.
     *
     * The long-press response duration of **bindSelectionMenu** is 600 ms while that of
     * [bindContextMenu]{@link CommonMethod#bindContextMenu(content: CustomBuilder, responseType: ResponseType, options?: ContextMenuOptions)}
     * is 800 ms. When both are bound and their triggering methods are set to long press, **bindSelectionMenu** takes
     * precedence.
     *
     * When the custom menu is too long, it is recommended that nest a [Scroll]{@link ./scroll} component inside to prevent
     * the keyboard from being obscured.
     *
     * > **NOTE**
     * >
     * > This API cannot be called within [attributeModifier]{@link CommonMethod#attributeModifier}.
     * >
     * > When [editMenuOptions]{@link TextAttribute#editMenuOptions} is used for configuring the text selection menu, the
     * > system's default style and trigger conditions are preserved.
     * >
     * > In contrast, when [bindSelectionMenu]{@link TextAttribute#bindSelectionMenu} is used, both the menu style and the
     * > trigger conditions are fully customizable.
     *
     * @param { TextSpanType } spanType - Span type of the menu.<br>Default value: **TextSpanType.TEXT**
     * @param { CustomBuilder } content - Content of the menu.
     * @param { TextResponseType } responseType - Response type of the menu.<br>Default value:
     *     **TextResponseType.LONG_PRESS**
     * @param { SelectionMenuOptions } [options] - Options of the menu.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    bindSelectionMenu(spanType: TextSpanType, content: CustomBuilder, responseType: TextResponseType, options?: SelectionMenuOptions): TextAttribute;
    /**
     * Called when the text selection position changes.
     *
     * @param { function } callback - Callback of the listened event.
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    onTextSelectionChange(callback: (selectionStart: number, selectionEnd: number) => void): TextAttribute;
    /**
     * Sets the font feature, for example, monospaced digits.
     *
     * Format: normal \| \<feature-tag-value\>
     *
     * Format of **\<feature-tag-value\>**: \<string\> \[ \<integer\> \| on \| off ]
     *
     * There can be multiple **\<feature-tag-value\>** values, which are separated by commas (,).
     *
     * For example, the input format for monospaced clock fonts is "ss01" on.
     *
     * > **NOTE**
     * >
     * > The **Text** component cannot contain both text and the child component **Span** or **ImageSpan**. If both of
     * > them exist, only the content in **Span** or **ImageSpan** is displayed.
     * >
     * > The typesetting engine rounds down the value of [width]{@link CommonMethod#width(value: Length)} to ensure that
     * > the value is an integer. If the typesetting engine rounds up the value instead, the right side of the text may be
     * > clipped.
     * >
     * > When multiple **Text** components are placed in the [Row]{@link ./row} container with no specific layout or space
     * > allocation settings configured, the components are laid out based on the maximum size of the container. To make
     * > sure the sum of the components' main axis sizes does not exceed the main axis size of the container, you can set
     * > [layoutWeight]{@link CommonMethod#layoutWeight} or use the [flex layout]{@link ./common}.
     * >
     * > The system's default font supports the following ligatures: Th, fb, ff, fb, ffb, ffh, ffi, ffk, ffl, fh, fi, fk,
     * > fl, rf, rt, rv, rx, ry. These ligatures may cause unexpected effects of spans and styled strings. Disabling the
     * > ligature feature can avoid this issue.
     * >
     * > Text rendering behavior is closely tied to the font file in use. For instance, the system's default font supports
     * > 8-punctuation compression only for left-side punctuation marks. Right-side punctuation, including exclamation
     * > marks, enumeration commas, and question marks, is not affected by this feature.
     *
     * @param { string } value - Font feature.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    fontFeature(value: string): TextAttribute;
    /**
     * Sets the marquee effect for text.
     *
     * The **marqueeOptions** settings take effect only when **textOverflow** is set to **TextOverflow.MARQUEE**.
     *
     * @param { Optional<TextMarqueeOptions> } options - Marquee animation properties such as enable/disable, step size,
     *     loop count, and direction.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    marqueeOptions(options: Optional<TextMarqueeOptions>): TextAttribute;
    /**
     * Called when the marquee animation reaches the specified state.
     *
     * @param { Callback<MarqueeState> } callback - Callback that receives a **MarqueeState** enum value, which indicates
     *     the current state of the marquee animation.
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    onMarqueeStateChange(callback: Callback<MarqueeState>): TextAttribute;
    /**
     * Sets whether to enable privacy mode on widgets.
     *
     * @param { boolean } supported - Whether to enable privacy mode on widgets.<br>Default value: **false**. The value
     *     **true** means to enable privacy mode, in which case text is obscured with hyphens (-).<br>**NOTE**<br>The
     *     value **null** means not to enable privacy mode on widgets.<br>Enabling privacy mode requires support from the
     *     widget framework. You can use [obscured]{@link CommonMethod#obscured} to set how the component content is
     *     obscured.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @form
     * @atomicservice
     * @since 12
     */
    privacySensitive(supported: boolean): TextAttribute;
    /**
     * Sets whether the text is selectable and focusable.
     *
     * This attribute must be used in conjunction with [copyOption]{@link TextAttribute#copyOption}.
     *
     * @param { TextSelectableMode } mode - Whether the text is selectable and focusable.<br>Default value:
     *     **TextSelectableMode.SELECTABLE_UNFOCUSABLE**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    textSelectable(mode: TextSelectableMode): TextAttribute;
    /**
     * Sets the extended options for the custom menu, including the text content, icon, and callback.
     *
     * When
     * [disableMenuItems](docroot://reference/apis-arkui/arkts-apis-uicontext-textmenucontroller.md#disablemenuitems20) or
     *
     * [disableSystemServiceMenuItems](docroot://reference/apis-arkui/arkts-apis-uicontext-textmenucontroller.md#disablesystemservicemenuitems20)
     * is used to disable system service menu items in the text selection menu, the disabled menu options will be excluded
     * from the parameter list in the [onCreateMenu]{@link EditMenuOptions.onCreateMenu} callback of **editMenuOptions**.
     *
     * > **NOTE**
     * >
     * > When [editMenuOptions]{@link TextAttribute#editMenuOptions} is used for configuring the text selection menu, the
     * > system's default style and trigger conditions are preserved.
     * >
     * > In contrast, when [bindSelectionMenu]{@link TextAttribute#bindSelectionMenu} is used, both the menu style and the
     * > trigger conditions are fully customizable.
     *
     * @param { EditMenuOptions } editMenu - Extended options of the custom menu.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    editMenuOptions(editMenu: EditMenuOptions): TextAttribute;
    /**
     * Whether half leading is enabled. Half leading refers to splitting the leading in half and applying it equally to
     * the top and bottom of the line.
     *
     * @param { boolean } halfLeading - Whether half leading is enabled. Half leading refers to splitting the leading in
     *     half and applying it equally to the top and bottom of the line.<br>**true**: Half leading is enabled.
     *     **false**: Half leading is not enabled.<br>Default value: **false**
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    halfLeading(halfLeading: boolean): TextAttribute;
    /**
     * Sets whether to enable haptic feedback.
     *
     * To enable haptic feedback, you must declare the **ohos.permission.VIBRATE** permission under **requestPermissions**
     * in the [module.json5](docroot://quick-start/module-configuration-file.md) file of the project.
     *
     * > **NOTE**
     * >
     * > This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 18.
     *
     * @param { boolean } isEnabled - Whether to enable haptic feedback.<br>**true** to enable, **false** otherwise.<br>
     *     Default value: **true**
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 13
     */
    enableHapticFeedback(isEnabled: boolean): TextAttribute;
    /**
     * Sets whether to enable automatic spacing between Chinese and Western characters.
     *
     * @param { Optional<boolean> } enabled - Whether to enable automatic spacing between Chinese and Western characters.<
     *     br>**true** to enable, **false** otherwise.<br>Default value: **false**
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    enableAutoSpacing(enabled: Optional<boolean>): TextAttribute;
    /**
     * Sets whether to optimize trailing spaces at line endings during text layout, resolving alignment display issues
     * caused by trailing spaces.
     *
     * When **Text.optimizeTrailingSpace** is set to **true**:
     *
     * * Trailing space optimization applies to multi-line text, single-line text, and text and image layouts (
     * particularly noticeable with **TextAlign.Center** or **TextAlign.End**).
     * * For text containing only spaces, decoration lines, shadows, and background colors follow the space text display.
     * * Leading spaces are not optimized. When text with trailing spaces wraps, trailing spaces on each line are
     * optimized based on component width.
     *
     * When optimizing pure space text by setting [optimizeTrailingSpace]{@link TextAttribute#optimizeTrailingSpace} to
     * **true**, you cannot simultaneously set
     * [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)},
     * [decoration]{@link TextAttribute#decoration}, and [textAlign]{@link TextAttribute#textAlign} attributes.
     *
     * @param { Optional<boolean> } optimize - Whether to optimize trailing spaces.<br>**true** to optimize, **false**
     *     otherwise.<br>Default value: **false**
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    optimizeTrailingSpace(optimize: Optional<boolean>): TextAttribute;
    /**
     * Applies a transition animation to text content. Supports numeric flip animation via
     * [NumericTextTransition]{@link NumericTextTransition}.
     *
     * @param { Optional<ContentTransition> } transition - Text animation effect.
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 23]
     * @atomicservice
     * @since 20
     */
    contentTransition(transition: Optional<ContentTransition>): TextAttribute;
    /**
     * Applies a transition animation to text content. Supports numeric flip animation via
     * [NumericTextTransition]{@link NumericTextTransition}.
     *
     * @param { SelectedDragPreviewStyle | undefined } value - Drag preview style for selected text.<br>If this parameter
     *     is set to **undefined**, the drag preview follows the theme: white in light mode and black in dark mode.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    selectedDragPreviewStyle(value: SelectedDragPreviewStyle | undefined): TextAttribute;
    /**
     * Specifies the text layout direction. If this attribute is not set, the default text layout direction follows the
     * component layout direction.
     *
     * @param { TextDirection | undefined } direction - Text layout direction.<br>If this parameter is set to
     *     **undefined**, the text layout direction follows the component layout direction as defined by
     *     **TextDirection.DEFAULT**.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    textDirection(direction: TextDirection | undefined): TextAttribute;
    /**
     * Sets whether to add spacing to the first and last lines to avoid text truncation. If this attribute is not set, no
     * spacing is added by default.
     *
     * @param { Optional<boolean> } include - Whether to add spacing to the first and last lines to avoid text truncation.
     *     <br>**true**: Spacing is added to the first and last lines. **false**: Spacing is not added to the first and
     *     last lines.
     * @returns { TextAttribute } - returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    includeFontPadding(include: Optional<boolean>): TextAttribute;
    /**
     * Adapts the line height to the actual text height for overlapped multi-line text. This API takes effect only when
     * the line height is less than the actual text height. If this API is not set, the line height does not adapt to the
     * actual text height by default.
     *
     * @param { Optional<boolean> } enabled - Whether the line height adapts to the actual text height.<br>**true**: Line
     *     height adapts to the actual text height. **false**: Line height does not adapt to the actual text height.
     * @returns { TextAttribute } - returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    fallbackLineSpacing(enabled: Optional<boolean>): TextAttribute;
    /**
     * Sets whether to enable leading punctuation compression.
     *
     * > **NOTE**
     * >
     * > - Leading punctuation is not compressed by default.
     * >
     * > - For the range of punctuation marks that support leading compression, see
     * > [ParagraphStyle]{@link @ohos.graphics.text:text.ParagraphStyle}.
     *
     * @param { Optional<boolean> } enabled - Whether to enable leading punctuation compression.<br>**true**: Leading
     *     punctuation compression is enabled. **false**: Leading punctuation compression is disabled.
     * @returns { TextAttribute } - returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    compressLeadingPunctuation(enabled: Optional<boolean>): TextAttribute;
    /**
     * Sets whether to enable orphan character optimization during text typesetting. If this attribute is not set, orphan
     * character optimization is disabled by default.
     *
     * Orphan character optimization improves the text layout by handling the orphan character (the first Chinese
     * character of the last line of a paragraph) more efficiently. When enabled, it adjusts line breaks to avoid orphan
     * characters as much as possible. This feature takes effect only when [wordBreak]{@link TextAttribute#wordBreak} is
     * not **BREAK_ALL** and [locale]{@link @ohos.graphics.text:text.TextStyle} of the first
     * [TextStyle]{@link @ohos.graphics.text:text.TextStyle} of the text to be typeset is either **"zh-Hans"** or
     * **"zh-Hant"**.
     *
     * **Since**: 26.0.0
     *
     * @param { Optional<boolean> } enabled - Whether to enable orphan character optimization for the last line of the
     *     paragraph.<br>**true**: Orphan character optimization is enabled. **false**: Orphan character optimization is
     *     disabled.<br>When the value is **undefined** or **null**, orphan character optimization is disabled.
     * @returns { TextAttribute } - returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    orphanCharOptimization(enabled: Optional<boolean>): TextAttribute;
    /**
     * Set the font variation.
     *
     * @param { Array<FontVariation> } fontVariations - Indicates the text font variation.
     * @returns { TextAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    fontVariations(fontVariations: Array<FontVariation>): TextAttribute;
    /**
     * Sets the incremental update policy for text rendering.
     *
     * This API takes effect only when Text content contains a StyledString.
     * Default value is IncrementalUpdatePolicy.NONE.
     *
     * @param { IncrementalUpdatePolicy | undefined } policy - Indicates the incremental update policy.
     *     Passing `undefined` resets it to the default value.
     * @returns { TextAttribute } - returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    incrementalUpdatePolicy(policy: IncrementalUpdatePolicy | undefined): TextAttribute;
    /**
     * Whether to enable punctuation overflow at line ends.
     *
     * @param { Optional<boolean> } enabled - Whether to enable the feature, the default value is false.
     * @returns { TextAttribute } returns the instance of the TextAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    punctuationOverflow(enabled: Optional<boolean>): TextAttribute;
}
/**
 * Defines Text Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const TextInstance: TextAttribute;
/**
 * The **Text** component is used to display a piece of textual information.
 *
 * ###### Child Components
 *
 * This component can contain the [Span]{@link ./span}, [ImageSpan]{@link ./image_span},
 * [SymbolSpan]{@link ./symbol_span}, and [ContainerSpan]{@link ./container_span} child components.
 *
 * > **NOTE**
 * >
 * > Use [child components](docroot://reference/apis-arkui/arkui-ts/ts-basic-components-text.md#child-components) to
 * > implement [text and image layout](docroot://ui/arkts-text-image-layout.md) scenarios.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const Text: TextInterface;
/**
 * Provides the [span]{@link ./span} type information.
 *
 * > **NOTE**
 * >
 * > The system follows the priority order below when determining the menu type to display during text interactions:
 * >
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum TextSpanType {
    /**
     * Text span.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    TEXT = 0,
    /**
     * Image span.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    IMAGE = 1,
    /**
     * Mixed span, which contains both text and imagery.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    MIXED = 2,
    /**
     * When this type is registered but **TEXT**, **IMAGE**, or **MIXED** types are not registered, this type will be
     * triggered and displayed for those registered types.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    DEFAULT = 3
}
/**
 * Response type of the menu.
 *
 * > **NOTE**
 * >
 * > The system follows the priority order below when determining the menu type to display during text interactions:
 * >
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum TextResponseType {
    /**
     * The menu is displayed when the component is right-clicked.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    RIGHT_CLICK = 0,
    /**
     * The menu is displayed when the component is long-pressed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    LONG_PRESS = 1,
    /**
     * The menu is displayed when the component is selected.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    SELECT = 2,
    /**
     * When this type is registered but **RIGHT_CLICK**, **LONG_PRESS**, or **SELECT** types are not registered, this type
     * will be triggered and displayed for right-click, long press, mouse selection, and
     * [selection]{@link TextAttribute#selection} API calls.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    DEFAULT = 3
}
/**
 * Enumerates the return values of the marquee state callback.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare enum MarqueeState {
    /**
     * The marquee starts scrolling.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    START = 0,
    /**
     * The marquee completes one scroll movement. If the number of **loops** is not 1, this value will be returned
     * multiple times.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    BOUNCE = 1,
    /**
     * All loops of the marquee are completed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    FINISH = 2
}
/**
 * Enumerates the marquee scrolling modes.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare enum MarqueeStartPolicy {
    /**
     * The marquee scrolls continuously. Default value.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    DEFAULT = 0,
    /**
     * The marquee starts scrolling when it has focus or when the mouse hovers over it.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    ON_FOCUS = 1
}
/**
 * Sets the scrolling policy of the marquee after its attributes are updated.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 23
 */
declare enum MarqueeUpdatePolicy {
    /**
     * Restarts the marquee from the start position after the attributes of the marquee component are updated.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    DEFAULT = 0,
    /**
     * Resumes the marquee from the current position after the attributes of the marquee component are updated.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    PRESERVE_POSITION = 1
}
/**
 * Describes the initialization options of the **Text** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare interface TextOptions {
    /**
     * Text controller.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    controller: TextController;
}
/**
 * Describes the initialization options of the **Marquee** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare interface TextMarqueeOptions {
    /**
     * Whether to start the marquee.
     *
     * **true**: Start the marquee. **false**: Do not start the marquee.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    start: boolean;
    /**
     * Step length of the scrolling animation text.
     *
     * Default value: **4.0** (in vp)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    step?: number;
    /**
     * Spacing between two marquee rounds. If the unit of **LengthMetrics** is **PERCENT**, the current setting does not
     * take effect and the default value is used.
     *
     * Default value: **48.0vp**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    spacing?: LengthMetrics;
    /**
     * Number of times the marquee will scroll. If the value is less than or equal to **0**, the marquee will scroll
     * continuously.
     *
     * Default value: **-1**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    loop?: number;
    /**
     * Whether the text scrolls from the start.
     *
     * **true** to scroll from the start, **false** to scroll in reverse.
     *
     * Default value: **true**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    fromStart?: boolean;
    /**
     * Time interval between scroll movements.
     *
     * Default value: **0**
     *
     * Unit: millisecond
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    delay?: number;
    /**
     * Whether to apply a fade-out effect when the text is too long.
     *
     * **true** to apply a fade-out effect when the text is too long, **false** otherwise.
     *
     * When this parameter is set to **true**: if the text content exceeds the display range, a fade-out effect is applied
     * to the edges of the partially visible text; if text is partially visible at both ends, the fade-out effect is
     * applied to both ends. The **clip** attribute is automatically locked to **true** and cannot be set to **false**.
     *
     * Default value: **false**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    fadeout?: boolean;
    /**
     * Policy for starting the marquee. This attribute takes effect only when **start** is set to **true**.
     *
     * Default value: **MarqueeStartPolicy.DEFAULT**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    marqueeStartPolicy?: MarqueeStartPolicy;
    /**
     * Scrolling policy of the marquee after its attributes are updated.
     *
     * This attribute takes effect when the marquee is in the playing state and the text width exceeds the width of the
     * marquee component.
     *
     * Default value: **MarqueeUpdatePolicy.DEFAULT**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    marqueeUpdatePolicy?: MarqueeUpdatePolicy;
}
/**
 * Defines the controller of the **Text** component.
 *
 * ###### Objects to Import
 *
 * ```ts
 * controller: TextController = new TextController()
 * ```
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare class TextController {
    /**
     * Closes the custom or default text selection menu.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    closeSelectionMenu(): void;
    /**
     * Binds to or updates the specified styled string.
     *
     * @param { StyledString } value - Styled string.<br>**NOTE**<br>The child class
     *     [MutableStyledString]{@link MutableStyledString} of **StyledString** can also serve as the argument.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    setStyledString(value: StyledString): void;
    /**
     * Obtains the **LayoutManager** object.
     *
     * @returns { LayoutManager } **LayoutManager** object.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    getLayoutManager(): LayoutManager;
    /**
     * Sets the text selection area, which will be highlighted.
     *
     * > **NOTE**
     * >
     * > If [copyOption]{@link TextAttribute#copyOption} is set to **CopyOptions.None**, the setting of
     * > **setTextSelection** does not take effect.
     * >
     * > If [textOverflow]{@link TextAttribute#textOverflow} is set to **TextOverflow.MARQUEE**, the setting of
     * > **setTextSelection** does not take effect.
     * >
     * > If the value of **selectionStart** is greater than or equal to that of **selectionEnd**, no text will be
     * > selected. The value range is [0, textSize], where **textSize** indicates the maximum number of characters in the
     * > text content. If the value is less than 0, the value **0** will be used. If the value is greater than
     * > **textSize**, **textSize** will be used.
     * >
     * > If the selection range falls within a truncated or invisible area, selection is ignored. When truncation is
     * > disabled, selection can extend beyond the parent component's bounds.
     * >
     * > On PC or 2-in-1 devices, calling **setTextSelection** does not show the menu even if **options** is set to
     * > **MenuPolicy.SHOW**.
     * >
     * > When an emoji is truncated by the selection range, the emoji is selected if its start position is within the
     * > specified text selection range.
     *
     * @param { number | undefined } selectionStart - Start position of the text selection range.<br>Value range:
     *     [0, +∞). Negative values and **undefined** are treated as **0**.
     * @param { number | undefined } selectionEnd - End position of the text selection range.<br>Value range:
     *     [0, +∞). Negative values and **undefined** are treated as **0**.
     * @param { SelectionOptions } [options] - Configuration options for text selection.<br>Default value:
     *     **MenuPolicy.DEFAULT** in **SelectionOptions**
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    setTextSelection(selectionStart: number | undefined, selectionEnd: number | undefined, options?: SelectionOptions): void;
}
