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
 * Enumerates the button types.
 *
 * > **NOTE**
 * >
 * > - The corner radius of the rounded rectangle button is set using the universal attribute
 * > [borderRadius]{@link CommonMethod#borderRadius(value: Length | BorderRadiuses | LocalizedBorderRadiuses)}.
 * >
 * > - For a button of the **Capsule** type, the **borderRadius** settings do not take effect, and the radius of its
 * > rounded corner is always half of the button height or width, whichever is smaller.
 * >
 * > - For a button of the **Circle** type: (1) If both its width and height are set, **borderRadius** does not take
 * > effect, and the button radius is half of the width or height (whichever is smaller). (2) If either its width or
 * > height is set, **borderRadius** does not take effect, and the button radius is half of the set width or height. (3)
 * > If neither its width nor height is set, the button radius is as specified by **borderRadius**; if **borderRadius**
 * > is set to a negative value, the value **0** will be used.
 * >
 * > - The button text is set using [fontSize]{@link ButtonAttribute#fontSize},
 * > [fontColor]{@link ButtonAttribute#fontColor}, [fontStyle]{@link ButtonAttribute#fontStyle},
 * > [fontFamily]{@link ButtonAttribute#fontFamily}, and [fontWeight]{@link ButtonAttribute#fontWeight}.
 * >
 * > - Before setting the [gradient color]{@link common}, you need to set
 * > [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)} to transparent.
 * >
 * > - When **borderRadius** is not set, the corner radius of the rounded rectangle button remains at the default value.
 * > In this case, the corner radius does not change with the button height and is subject to the **controlSize**
 * > property. When **controlSize** is **NORMAL**, the corner radius is 20 vp; when **controlSize** is **SMALL**, the
 * > corner radius is 14 vp.
 * >
 * > - When [border]{@link CommonMethod#border(value: BorderOptions)} is set for the
 * > button, a default
 * > [borderRadius]{@link CommonMethod#borderRadius(value: Length | BorderRadiuses | LocalizedBorderRadiuses)} value is
 * > automatically applied. When both **border** and **borderRadius** attributes are used, **borderRadius** must be
 * > specified after **border** to prevent the border radius from being overridden by the default radius value in the
 * > border style.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ButtonType {
    /**
     * Capsule-type button (the round corner is half of the height by default).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Capsule,
    /**
     * Circular button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Circle,
    /**
     * Normal button, with no rounded corners by default.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Normal,
    /**
     * Rounded rectangle button (default value: when **controlSize** is **NORMAL**, the corner radius is 20 vp; when
     * controlSize is **SMALL**, the corner radius is 14 vp).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 15
     */
    ROUNDED_RECTANGLE = 3
}
/**
 * Enumerates the button importance levels.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice [since 12]
 * @since 11
 */
declare enum ButtonStyleMode {
    /**
     * Normal button (used to direct the user to a common task).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    NORMAL = 0,
    /**
     * Emphasized button (used to direct the user to the most important task).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    EMPHASIZED = 1,
    /**
     * Text button (displayed as simple text without any background color).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    TEXTUAL = 2
}
/**
 * Role of the button.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 12
 */
declare enum ButtonRole {
    /**
     * Normal button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    NORMAL = 0,
    /**
     * Warning button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    ERROR = 1
}
/**
 * Defines the callback type used in **ButtonConfiguration**.
 *
 * @param { number } xPos - X-coordinate of the click point.<br>Unit: vp
 * @param { number } yPos - Y-coordinate of the click point.<br>Unit: vp
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type ButtonTriggerClickCallback = (xPos: number, yPos: number) => void;
/**
 * You need a custom class to implement the **ContentModifier** API. Inherits from
 * [CommonConfiguration]{@link CommonConfiguration}.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface ButtonConfiguration extends CommonConfiguration<ButtonConfiguration> {
    /**
     * Text label of the button.
     *
     * Note: If the text is longer than the width of the button, it is truncated.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    label: string;
    /**
     * Whether the button is pressed.
     *
     * **true**: pressed; **false**: not pressed.
     *
     * Default value: **false**
     *
     * **NOTE**
     *
     * This setting applies to the original button size, not to any new component constructed using the builder.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    pressed: boolean;
    /**
     * Click event of the new component constructed using the builder.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    triggerClick: ButtonTriggerClickCallback;
}
/**
 * Button size.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice [since 12]
 * @since 11
 */
declare enum ControlSize {
    /**
     * Small button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    SMALL = 'small',
    /**
     * Normal button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    NORMAL = 'normal'
}
/**
 * Describes the button style.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare interface ButtonOptions {
    /**
     * Button display style.
     *
     * Default value: **ButtonType.ROUNDED_RECTANGLE**
     *
     * API version 18 and later: The default value is **ButtonType.ROUNDED_RECTANGLE**. Versions earlier than API
     * version 18: The default value is **ButtonType.Capsule**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    type?: ButtonType;
    /**
     * Whether to enable the pressed state effect when the button is clicked.
     *
     * **true**: The pressed state effect is enabled. **false**: The pressed state effect is disabled.
     *
     * Default value: **true**
     *
     * **NOTE**
     *
     * When the pressed state effect is enabled and a custom pressed state style is configured, the resulting color
     * displayed after pressing is a composite blend of the original background color and the newly defined pressed
     * state color.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    stateEffect?: boolean;
    /**
     * Style and importance of the button. The system automatically adjusts the button background color and text color
     * based on the enumerated value. You can also use the
     * [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)},
     * [fontColor]{@link ButtonAttribute#fontColor}, and [role]{@link ButtonAttribute#role} APIs to set the background
     * color and text color. The actual displayed effect will be determined by the last setting.
     *
     * Default value: **ButtonStyleMode.EMPHASIZED**
     *
     * **NOTE**
     *
     * The button primacy is as follows, from high to low: emphasized button, normal button, text button.
     *
     * @default ButtonStyleMode.EMPHASIZED
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    buttonStyle?: ButtonStyleMode;
    /**
     * Button size.
     *
     * Default value: **ControlSize.NORMAL**
     *
     * @default ControlSize.NORMAL
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    controlSize?: ControlSize;
    /**
     * Role of the button. The system automatically adjusts the button background color and text color based on the
     * enumerated value. You can also use the
     * [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)},
     * [fontColor]{@link ButtonAttribute#fontColor}, and [buttonStyle]{@link ButtonAttribute#buttonStyle} APIs to set
     * the background color and text color. The actual displayed effect will be determined by the last setting.
     *
     * Default value: **ButtonRole.NORMAL**
     *
     * @default ButtonRole.NORMAL
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    role?: ButtonRole;
}
/**
 * The **Button** component can be used to create different types of buttons.
 *
 * > **NOTE**
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
interface ButtonInterface {
    /**
     * Creates an empty button.
     *
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (): ButtonAttribute;
    /**
     * Creates a button that can contain a single child component.
     *
     * @param { ButtonOptions } options - Button settings.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (options: ButtonOptions): ButtonAttribute;
    /**
     * Creates a button based on text content. In this case, the component cannot contain child components.
     *
     * By default, the text content is displayed in a one line.
     *
     * @param { ResourceStr } label - Button text.<br>Note: If the text is longer than the width of the button, it is
     *     truncated.
     * @param { ButtonOptions } options - Button settings.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (label: ResourceStr, options?: ButtonOptions): ButtonAttribute;
}
/**
 * Label text and font style of the button.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare interface LabelStyle {
    /**
     * Display mode when the label text is too long. Text is clipped at the transition between words. To clip text in
     * the middle of a word, add zero-width spaces between characters.
     *
     * Default value: **TextOverflow.Ellipsis**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    overflow?: TextOverflow;
    /**
     * Maximum number of lines in the label text. If this attribute is specified, the text will not exceed the specified
     * number of lines. If there is extra text, you can use **overflow** to specify how it is displayed.
     *
     * Default value: **1**
     *
     * **NOTE**
     *
     * If this parameter is set to a value less than or equal to 0, the default value is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    maxLines?: number;
    /**
     * Minimum font size of the label text. For the setting to take effect, this attribute must be used together with
     * **maxFontSize**, **maxLines**, or layout constraint settings.
     *
     * **NOTE**
     *
     * If the value of **minFontSize** is less than or equal to 0, the adaptive font size does not take effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    minFontSize?: number | ResourceStr;
    /**
     * Maximum font size of the label text. For the setting to take effect, this attribute must be used together with
     * **minFontSize**, **maxLines**, or layout constraint settings.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    maxFontSize?: number | ResourceStr;
    /**
     * How the adaptive height is determined for the label text.
     *
     * Default value: **TextHeightAdaptivePolicy.MAX_LINES_FIRST**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    heightAdaptivePolicy?: TextHeightAdaptivePolicy;
    /**
     * Font of the label text.
     *
     * Default value:
     *
     * {
     *
     * size:'16.0fp',
     *
     * weight:FontWeight.Medium,
     *
     * style:FontStyle.Normal,
     *
     * family:'HarmonyOS Sans'
     *
     * }
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    font?: Font;
    /**
     * Horizontal alignment of the label text. This attribute does not take effect when the **Text** component of the
     * child node is used to set the label. The actual text alignment mode is determined by the **textAlign** attribute
     * of the **Text** component of the child node.
     *
     * The default value is **TextAlign.Center** for wearables and **TextAlign.Start** for other devices.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    textAlign?: TextAlign;
}
/**
 * In addition to the [universal attributes]{@link common}, the following attributes are supported.
 *
 * The [universal events]{@link common} are supported.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare class ButtonAttribute extends CommonMethod<ButtonAttribute> {
    /**
     * Sets the button type.
     *
     * @param { ButtonType } value - Button type.<br>API version 18 and later: The default value is
     *     **ButtonType.ROUNDED_RECTANGLE**.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    type(value: ButtonType): ButtonAttribute;
    /**
     * Specifies whether to enable the pressed state effect when the button is clicked.
     *
     * @param { boolean } value - Whether to enable the pressed state effect when the button is clicked.<br>**true**: The
     *     pressed state effect is enabled. **false**: The pressed state effect is disabled.<br>Default value: **true**
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    stateEffect(value: boolean): ButtonAttribute;
    /**
     * Sets the style and primacy for the button. The system automatically adjusts the button background color and text
     * color based on the enumerated value. You can also use the
     * [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)},
     * [fontColor]{@link ButtonAttribute#fontColor}, and [role]{@link ButtonAttribute#role} APIs to set the background
     * color and text color. The actual displayed effect will be determined by the last setting.
     *
     * > **NOTE**
     * >
     * > This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 12.
     *
     * @param { ButtonStyleMode } value - Style and primacy of the button<br>Default value: **ButtonStyleMode.EMPHASIZED**
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    buttonStyle(value: ButtonStyleMode): ButtonAttribute;
    /**
     * Sets the size for the button.
     *
     * > **NOTE**
     * >
     * > This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 12.
     *
     * @param { ControlSize } value - Size of the button.<br>Default value: **ControlSize.NORMAL**
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    controlSize(value: ControlSize): ButtonAttribute;
    /**
     * Sets the role of the button. The system automatically adjusts the button background color and text color based on
     * the enumerated value. You can also use the
     * [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)},
     * [fontColor]{@link ButtonAttribute#fontColor}, and [buttonStyle]{@link ButtonAttribute#buttonStyle} APIs to set the
     * background color and text color. The actual displayed effect will be determined by the last setting.
     *
     * @param { ButtonRole } value - Role of the button.<br>Default value: **ButtonRole.NORMAL**
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    role(value: ButtonRole): ButtonAttribute;
    /**
     * Sets the font color for the button.
     *
     * @param { ResourceColor } value - Font color of the button.<br>Default value: **$r('sys.color.font_on_primary')**,
     *     which means white
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontColor(value: ResourceColor): ButtonAttribute;
    /**
     * Sets the font size for the button.
     *
     * @param { Length } value - Font size of the button.<br>Default value:<br>**$r('sys.float.Body_L')** when
     *     **controlSize** is set to **ControlSize.NORMAL**<br>**$r('sys.float.Body_S')** when **controlSize** is set to
     *     **ControlSize.SMALL**<br>Note: For the string type, percentage values are not supported.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontSize(value: Length): ButtonAttribute;
    /**
     * Sets the font weight for the button.
     *
     * @param { number | FontWeight | string } value - Font weight of the button. For the number type, the value ranges
     *     from 100 to 900, at an interval of 100. A larger value indicates a thicker font.<br>Default value: **500**<br>
     *     For the string type, only strings that represent a number, for example, **'400'**, and the following enumerated
     *     values of **FontWeight** are supported: **'bold'**, **'bolder'**, **'lighter'**, **'regular'**, and
     *     **'medium'**.<br>If the value is abnormal or invalid, the font weight defaults to 400.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fontWeight(value: number | FontWeight | string): ButtonAttribute;
    /**
     * Sets the font style for the button.
     *
     * @param { FontStyle } value - Font style of the button.<br>Default value: **FontStyle.Normal**
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    fontStyle(value: FontStyle): ButtonAttribute;
    /**
     * Sets the font family.
     *
     * @param { string | Resource } value - Font family. The 'HarmonyOS Sans' font and
     *     [registered custom fonts]{@link @ohos.font:font} are supported.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    fontFamily(value: string | Resource): ButtonAttribute;
    /**
     * Creates a content modifier.
     *
     * @param { ContentModifier<ButtonConfiguration> } modifier - Content modifier to apply to the button.<br>
     *     **modifier**: content modifier. You need a custom class to implement the **ContentModifier** API.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    contentModifier(modifier: ContentModifier<ButtonConfiguration>): ButtonAttribute;
    /**
     * Sets the label style for the button.
     *
     * @param { LabelStyle } value - Label style of the button.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    labelStyle(value: LabelStyle): ButtonAttribute;
    /**
     * Sets the minimum font scale factor for text.
     *
     * @param { number | Resource } scale - Minimum font scale factor for text.<br>Value range: [0, 1]<br>**NOTE**<br>A
     *     value less than 0 is handled as **0**. A value greater than 1 is handled as **1**. Abnormal values are
     *     ineffective by default.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 20]
     * @atomicservice
     * @since 18
     */
    minFontScale(scale: number | Resource): ButtonAttribute;
    /**
     * Sets the maximum font scale factor for text.
     *
     * @param { number | Resource  } scale - Maximum font scale factor for text.<br>Value range:
     *     [1, +∞)<br>**NOTE**<br>A value less than 1 is handled as **1**. Abnormal values are ineffective by default.<br>If this parameter is not configured, the maximum scale for a circular button is 1x, while the maximum scale for capsule-type buttons, standard buttons, and rounded rectangle buttons defaults to the system-defined value.
     * @returns { ButtonAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 20]
     * @atomicservice
     * @since 18
     */
    maxFontScale(scale: number | Resource): ButtonAttribute;
}
/**
 * The **Button** component can be used to create different types of buttons.
 *
 * > **NOTE**
 *
 * ###### Child Components
 *
 * This component can contain only one child component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const Button: ButtonInterface;
/**
 * Defines Button Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const ButtonInstance: ButtonAttribute;
