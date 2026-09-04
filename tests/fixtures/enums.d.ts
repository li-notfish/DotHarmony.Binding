/*
 * Copyright (c) 2021-2023 Huawei Device Co., Ltd.
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
 * common enum of the checkbox shape
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice [since 12]
 * @since 11
 */
declare enum CheckBoxShape {
    /**
     * Circle.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    CIRCLE = 0,
    /**
     * Rounded Square.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 12]
     * @since 11
     */
    ROUNDED_SQUARE = 1
}
/**
 * common enum of color
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Color {
    /**
     * White.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    White,
    /**
     * Black.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Black,
    /**
     * Blue.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Blue,
    /**
     * Brown.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Brown,
    /**
     * Gray.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Gray,
    /**
     * Green.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Green,
    /**
     * Grey.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Grey,
    /**
     * Orange.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Orange,
    /**
     * Pink.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Pink,
    /**
     * Red.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Red,
    /**
     * Yellow.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Yellow,
    /**
     * Transparent.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    Transparent
}
/**
 * Enumerates the coloring strategies.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ColoringStrategy {
    /**
     * The foreground colors are the inverse of the component background colors. This strategy is only applicable when set
     * within the [foregroundColor]{@link CommonMethod#foregroundColor(value: ResourceColor | ColoringStrategy)}
     * attribute.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    INVERT = 'invert',
    /**
     * The shadow colors of the component are the average color obtained from the component background shadow area. This
     * strategy is only applicable when set within the
     * [shadow]{@link CommonMethod#shadow(value: ShadowOptions | ShadowStyle)} attribute whose input parameter type is
     * ShadowOptions.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    AVERAGE = 'average',
    /**
     * The shadow colors of the component are the primary color obtained from the component background shadow area. This
     * strategy is only applicable when set within the
     * [shadow]{@link CommonMethod#shadow(value: ShadowOptions | ShadowStyle)} attribute whose input parameter type is
     * ShadowOptions.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    PRIMARY = 'primary'
}
/**
 * Image display mode.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ImageFit {
    /**
     * Keep the aspect ratio to zoom out or zoom in so that the image is completely displayed within the display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Contain,
    /**
     * Keep the aspect ratio to zoom out or zoom in so that both sides of the image are greater than or equal to the
     * display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Cover,
    /**
     * Adaptive display
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Auto,
    /**
     * Zoom in or out without maintaining the aspect ratio so that the image fills the display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Fill,
    /**
     * Keep the aspect ratio displayed, and the image zooms out or remains unchanged.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    ScaleDown,
    /**
     * Keep the original size and display it in the center.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None,
    /**
     * Top Start.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    TOP_START = 7,
    /**
     * The top is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    TOP = 8,
    /**
     * Top tail end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    TOP_END = 9,
    /**
     * The starting end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    START = 10,
    /**
     * Center horizontal and vertical.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    CENTER = 11,
    /**
     * The tail end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    END = 12,
    /**
     * Bottom starting end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    BOTTOM_START = 13,
    /**
     * The bottom is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    BOTTOM = 14,
    /**
     * Bottom end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    BOTTOM_END = 15,
    /**
     * Matrix of Image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    MATRIX = 16
}
/**
 * Border Style
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum BorderStyle {
    /**
     * Displays as a series of dots with a radius of half the borderWidth.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Dotted,
    /**
     * Shows as a series of short square dashed lines.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Dashed,
    /**
     * Shows as a solid line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Solid
}
/**
 * Enumerates rendering strategies for drawing rounded corners.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 22
 */
declare enum RenderStrategy {
    /**
     * Online rendering mode. The content to be rendered is clipped with rounded corners and directly rendered to the main
     * canvas.
     *
     * Note: Online rendering may cause display anomalies in certain scenarios. For example, when blur effects are applied
     * within rounded corner components, background colors may interact and create gradient overlay effects. For detailed
     * behavior, see
     * [Example 3: Configuring Offscreen Rounded Corners](docroot://reference/apis-arkui/arkui-ts/ts-universal-attributes-border.md#example-3-configuring-offscreen-rounded-corners).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    FAST = 0,
    /**
     * Offscreen rendering mode. The content to be rendered is first rendered to the offscreen canvas without rounded
     * corners, and then clipped with rounded corners and rendered to the main canvas.
     *
     * **NOTE**
     *
     * 1. Compared with online rendering, offscreen rendering requires additional performance overhead.
     * 2. In offscreen rendering, the content is first rendered on an additional canvas, and then rendered on the main
     * canvas.
     * 3. Use offscreen rendering primarily for multi-layer components requiring rounded corners. For single components,
     * it has effect only when the [clip]{@link CommonMethod#clip(value: boolean)} attribute, [background]{@link common},
     * or [foreground color]{@link common} is configured.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    OFFSCREEN = 1
}
/**
 * Line Join Style
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum LineJoinStyle {
    /**
     * Connect path segments using bevels.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Miter,
    /**
     * Connect path segments using sharp corners.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Round,
    /**
     * Connect path segments using fillets.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bevel
}
/**
 * Sets the trigger status type of a touch operation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TouchType {
    /**
     * A finger is pressed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Down,
    /**
     * A finger is lifted.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Up,
    /**
     * A finger moves on the screen in pressed state.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Move,
    /**
     * A touch event is canceled. Examples: 1. touching the home button to return to the home screen while keeping a
     * finger on the screen; 2. folding a foldable phone to switch to the external screen while keeping a finger on the
     * screen.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Cancel,
    /**
     * A finger is pressed in accessibility mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    HOVER_ENTER = 9,
    /**
     * The mouse pointer moves in accessibility mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    HOVER_MOVE = 10,
    /**
     * The mouse pointer exits the component in accessibility mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    HOVER_EXIT = 11,
    /**
     * The triggered event is canceled in accessibility mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    HOVER_CANCEL = 12
}
/**
 * Enumerates how an input event is triggered.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 15
 */
declare enum InteractionHand {
    /**
     * Unspecified.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    NONE = 0,
    /**
     * Left-hand interaction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    LEFT = 1,
    /**
     * Right-hand interaction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    RIGHT = 2
}
/**
 * Enumerates the mouse button types.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 8
 */
declare enum MouseButton {
    /**
     * Left button on the mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Left,
    /**
     * Right button on the mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Right,
    /**
     * Middle button on the mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Middle,
    /**
     * Back button on the left of the mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Back,
    /**
     * Forward button on the left of the mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Forward,
    /**
     * No button.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    None
}
/**
 * Sets the action type of a mouse operation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 8
 */
declare enum MouseAction {
    /**
     * The mouse button is pressed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Press,
    /**
     * The mouse button is released.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Release,
    /**
     * The mouse cursor moves.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Move,
    /**
     * The mouse pointer is hovered on an element.
     *
     * Note: This value has no effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Hover,
    /**
     * The mouse pointer moves into the window.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    ENTER_WINDOW = 4,
    /**
     * The mouse pointer moves out of the window.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    LEAVE_WINDOW = 5,
    /**
     * The mouse button action is canceled. It is triggered in the following scenarios:
     *
     * 1. Component focus loss: This action is triggered when a currently focused component loses focus due to a system
     * event (such as pop-up interruption or app switching).
     * 2. Event interruption: During a mouse operation, if a higher-priority event occurs (such as a system-level gesture
     * or forced event stream recycling), causing the current mouse operation to be forcibly terminated.
     * 3. Abnormal state exit: In scenarios such as component destruction or abnormal rendering environment, unfinished
     * mouse events are marked as canceled.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    CANCEL = 13
}
/**
 * Animation status.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum AnimationStatus {
    /**
     * Initial state of the animation.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Initial = 0,
    /**
     * The animation is playing.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Running = 1,
    /**
     * The animation is paused.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Paused = 2,
    /**
     * The animation is stopped.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Stopped = 3
}
/**
 * Enumerates the interpolation curves. For details about the animation, see <!--RP1-->
 * [Bezier Curve](docroot://../design/ux-design/animation-attributes.md)<!--RP1End-->.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Curve {
    /**
     * The animation maintains a constant speed throughout the process.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Linear,
    /**
     * The animation starts slowly, accelerates, and then decelerates before ending. The curve is cubic-bezier(0.25, 0.1,
     * 0.25, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Ease,
    /**
     * The animation starts at a low speed and then picks up speed until the end. The cubic-bezier(0.42, 0.0, 1.0, 1.0)
     * is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    EaseIn,
    /**
     * The animation ends at a low speed. The cubic-bezier(0.0, 0.0, 0.58, 1.0) is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    EaseOut,
    /**
     * The animation starts and ends at a low speed. The cubic-bezier curve(0.42, 0.0, 0.58, 1.0) is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    EaseInOut,
    /**
     * The animation uses the standard cubic-bezier curve(0.4, 0.0, 0.2, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    FastOutSlowIn,
    /**
     * The animation uses the deceleration cubic-bezier curve(0.0, 0.0, 0.2, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    LinearOutSlowIn,
    /**
     * The animation uses the acceleration cubic-bezier curve(0.4, 0.0, 1.0, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    FastOutLinearIn,
    /**
     * The animation uses the extreme deceleration cubic-bezier curve(0.0, 0.0, 0.0, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    ExtremeDeceleration,
    /**
     * The animation uses the sharp cubic-bezier curve(0.33, 0.0, 0.67, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Sharp,
    /**
     * The animation uses the rhythm cubic-bezier curve(0.7, 0.0, 0.2, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Rhythm,
    /**
     * The animation uses the smooth cubic-bezier curve(0.4, 0.0, 0.4, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Smooth,
    /**
     * The animation uses the friction cubic-bezier curve(0.2, 0.0, 0.2, 1.0).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Friction
}
/**
 * Sets the status before and after execution of the animation in the current playback direction.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FillMode {
    /**
     * If the animation is not executed, no style is applied to the target. After the animation is played, the initial
     * default state is restored.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    None = 0,
    /**
     * The target component retains the state set by the last keyframe encountered during execution of the animation.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Forwards = 1,
    /**
     * The animation applies the values defined in the first relevant keyframe once it is applied to the target component,
     * and retains the values during the period set by **delay**. The first relevant keyframe depends on the value of
     * **playMode**. If **playMode** is **Normal** or **Alternate**, the first relevant keyframe is in the **from** state.
     * If **playMode** is **Reverse** or **AlternateReverse**, the first relevant keyframe is in the **to** state.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Backwards = 2,
    /**
     * The animation follows the rules for both **Forwards** and **Backwards**, extending the animation attributes in both
     * directions.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Both = 3
}
/**
 * Animation playback mode.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum PlayMode {
    /**
     * The animation is played forwards.
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
     * The animation is played backwards.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Reverse,
    /**
     * The animation is played forwards for an odd number of times (1, 3, 5...) and backwards for an even number of times
     * (2, 4, 6...).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Alternate,
    /**
     * The animation is played backwards for an odd number of times (1, 3, 5...) and forwards for an even number of times
     * (2, 4, 6...).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    AlternateReverse
}
/**
 * Sets the status type of a button operation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum KeyType {
    /**
     * The key is pressed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Down,
    /**
     * The key is released.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Up,
    /**
     * The key event is canceled.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    CANCEL = 3
}
/**
 * Sets the device type that triggers the button event.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum KeySource {
    /**
     * Unknown input device.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Unknown,
    /**
     * The input device is a keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Keyboard,
    /**
     * The input device is a joystick.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    JOYSTICK
}
/**
 * Edge.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Edge {
    /**
     * The top is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Top,
    /**
     * Center horizontal and vertical.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @since 7
     * @deprecated since 9
     */
    Center,
    /**
     * The bottom is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Bottom,
    /**
     * Cross axis direction text baseline alignment.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @since 7
     * @deprecated since 9
     */
    Baseline,
    /**
     * Align the head of the cross axis direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * Middle
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @since 7
     * @deprecated since 9
     */
    Middle,
    /**
     * Align the head of the cross axis direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    End
}
/**
 * Week Enum value
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Week {
    /**
     * Monday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Mon,
    /**
     * Tuesday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Tue,
    /**
     * Wednesday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Wed,
    /**
     * Thursday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Thur,
    /**
     * Friday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Fri,
    /**
     * Saturday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Sat,
    /**
     * Sunday.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Sun
}
/**
 * Sets the horizontal layout of elements.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Direction {
    /**
     * Elements are laid out from left to right.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Ltr,
    /**
     * Elements are laid out from right to left.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Rtl,
    /**
     * Use the default layout direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Auto
}
/**
 * Used to set the status of the scroll bar.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum BarState {
    /**
     * Not displayed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Off,
    /**
     * On-demand display (displayed when you touch it and disappears after 2 seconds).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Auto,
    /**
     * Resident display.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    On
}
/**
 * Sliding effect
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum EdgeEffect {
    /**
     * Elastic physical action, sliding to the edge can continue to slide for a distance based on the initial speed or
     * touch event, and spring back when released.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Spring,
    /**
     * Fade.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Fade,
    /**
     * Sliding to the edge has no effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None
}
/**
 * Alignment enumeration description.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Alignment {
    /**
     * Top Start.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    TopStart,
    /**
     * The top is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Top,
    /**
     * Top tail end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    TopEnd,
    /**
     * The starting end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * Center horizontal and vertical.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * The tail end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    End,
    /**
     * Bottom starting end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    BottomStart,
    /**
     * The bottom is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bottom,
    /**
     * Bottom end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    BottomEnd
}
/**
 * LocalizedAlignment enumeration description.
 *
 * @enum { string }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 20
 */
declare enum LocalizedAlignment {
    /**
     * Top Start.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    TOP_START = "top_start",
    /**
     * The top is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    TOP = "top",
    /**
     * Top tail end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    TOP_END = "top_end",
    /**
     * The starting end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    START = "start",
    /**
     * Center horizontal and vertical.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    CENTER = "center",
    /**
     * The tail end is centered longitudinally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    END = "end",
    /**
     * Bottom starting end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    BOTTOM_START = "bottom_start",
    /**
     * The bottom is centered horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    BOTTOM = "bottom",
    /**
     * Bottom end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    BOTTOM_END = "bottom_end"
}
/**
 * TransitionType enumeration description.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TransitionType {
    /**
     * The transition takes effect in all scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    All,
    /**
     * The transition takes effect when a component is inserted or displayed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Insert,
    /**
     * The transition takes effect when a component is deleted or hidden.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Delete
}
/**
 * RelateType enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum RelateType {
    /**
     * Scales the current component to fill the parent component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    FILL,
    /**
     * Scales the current component to fit the parent component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    FIT
}
/**
 * Defines the visibility and layout placeholder status of the component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Visibility {
    /**
     * The component is visible.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Visible,
    /**
     * The component is hidden, and a placeholder is used for it in the layout.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Hidden,
    /**
     * The component is hidden. It is not involved in the layout, and no placeholder is used for it.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None
}
/**
 * LineCapStyle enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum LineCapStyle {
    /**
     * The two ends of the dividing line are parallel lines.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Butt,
    /**
     * The two ends of the dividing line are semicircles.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Round,
    /**
     * Extends half a circle at the end of the path with a width equal to half the line width and a height equal to the
     * line width.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Square
}
/**
 * Defines the axis direction.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum Axis {
    /**
     * Vertical direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Vertical,
    /**
     * Horizontal direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Horizontal
}
/**
 * HorizontalAlign enumeration description.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum HorizontalAlign {
    /**
     * Aligns the start end in the language direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * Center alignment. The default alignment mode is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * Aligns the ends in the language direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    End
}
/**
 * FlexAlign enumeration description.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FlexAlign {
    /**
     * The element is aligned at the head of the principal axis,
     * the first element is aligned with the head of the row, and subsequent elements are aligned with the previous one.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * The elements are centered in the direction of the principal axis,
     * and the first element is the same distance from the beginning of the row as the last element is from the end of the
     * row.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * The element is aligned at the tail of the principal axis,
     * the last element is aligned at the end of the row, and the other elements are aligned with the next.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    End,
    /**
     * Elastic elements are evenly distributed in the direction of the Flex principal axis,
     * with the same distance between adjacent elements.
     * The first element aligns with the beginning of the line, and the last element aligns with the end of the line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    SpaceBetween,
    /**
     * Elastic elements are evenly distributed in the direction of the Flex principal axis,
     * with the same distance between adjacent elements. Half the distance between adjacent elements as the distance
     * between
     * the first element and the distance between the last element and the end of the row.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    SpaceAround,
    /**
     * Elements in the Flex axis direction are evenly spaced.
     * The spacing between adjacent elements, the spacing between the first element and the beginning of the row,
     * and the spacing between the last element and the end of the row are the same.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    SpaceEvenly
}
/**
 * ItemAlign enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ItemAlign {
    /**
     * Use the default configuration in the Flex container.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Auto,
    /**
     * The element is in the Flex container with the cross-axis direction head aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * The element is centered in the Flex container with the cross axis direction aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * The element is bottom aligned in the Flex container with the cross axis direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    End,
    /**
     * Element In the Flex container, the cross-axis direction text baseline is aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Baseline,
    /**
     * Element In a Flex container, the fill is stretched across the axis and, when no dimension is set, to the container
     * size.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Stretch
}
/**
 * FlexDirection enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FlexDirection {
    /**
     * The main axis is consistent with the row direction as the layout mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Row,
    /**
     * The main axis is consistent with the column direction as the layout mode.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Column,
    /**
     * The layout is in the opposite direction to the Row direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    RowReverse,
    /**
     * Layout in the opposite direction to the column.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    ColumnReverse
}
/**
 * Pixel Round Direction
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 11
 */
declare enum PixelRoundCalcPolicy {
    /**
     * No Force round the component boundary coordinates to integer pixel.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    NO_FORCE_ROUND = 0,
    /**
     * Force ceil the component boundary coordinates to integer pixel.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    FORCE_CEIL = 1,
    /**
     * Force floor the component boundary coordinates to integer pixel.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    FORCE_FLOOR = 2
}
/**
 * Pixel Round Mode
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
declare enum PixelRoundMode {
    /**
     * Do pixel round on layout finish.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    PIXEL_ROUND_ON_LAYOUT_FINISH = 0,
    /**
     * Do pixel round after measure.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    PIXEL_ROUND_AFTER_MEASURE = 1
}
/**
 * FlexWrap enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FlexWrap {
    /**
     * The Flex container has a single row/column layout of elements, and children are allowed to go beyond the container.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    NoWrap,
    /**
     * The elements of the Flex container are arranged in multiple rows or columns, and the sub-items are allowed to
     * exceed the container.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Wrap,
    /**
     * The elements of the Flex container are arranged in reverse multiple rows/columns, and children are allowed to
     * exceed the container.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    WrapReverse
}
/**
 * VerticalAlign enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum VerticalAlign {
    /**
     * Top alignment.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Top,
    /**
     * Center alignment. The default alignment mode is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * Bottom alignment.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bottom
}
/**
 * ImageRepeat enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ImageRepeat {
    /**
     * Do not draw the picture again.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    NoRepeat,
    /**
     * Repeat the drawing only on the horizontal axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    X,
    /**
     * Repeat the drawing only on the vertical axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Y,
    /**
     * Draw the picture repeatedly on both axes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    XY
}
/**
 * ImageSize enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ImageSize {
    /**
     * Keep the scale of the original image unchanged.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Auto,
    /**
     * Keep the aspect ratio to zoom in or out the image so that both sides of the image are greater than or equal to the display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Cover,
    /**
     * Keep the aspect ratio to zoom out or zoom in so that the image is completely displayed within the display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Contain,
    /**
     * Zoom in or out without maintaining the aspect ratio so that the image fills the display boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    FILL = 3
}
/**
 * GradientDirection enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum GradientDirection {
    /**
     * From right to left.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Left = 0,
    /**
     * From bottom to top.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Top = 1,
    /**
     * From left to right.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Right = 2,
    /**
     * From top to bottom.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bottom = 3,
    /**
     * From upper left to lower right.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    LeftTop = 4,
    /**
     * From lower left to upper right.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    LeftBottom = 5,
    /**
     * From upper right to lower left.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    RightTop = 6,
    /**
     * From lower right to upper left.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    RightBottom = 7,
    /**
     * None.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None = 8
}
/**
 * SharedTransitionEffectType enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum SharedTransitionEffectType {
    /**
     * The target page element remains in a fixed position, with configurable opacity animation.
     *
     * Currently, this effect only takes effect when configured for redirection to the target page.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Static,
    /**
     * The source page element moves to the position of the target page element and scales accordingly.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Exchange
}
/**
 * Font style.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FontStyle {
    /**
     * Standard font style.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Normal,
    /**
     * Italic font style.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Italic
}
/**
 * The font weight of the text
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum FontWeight {
    /**
     * 100 font weight (thin).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Lighter,
    /**
     * 400 font weight (normal).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Normal,
    /**
     * 400 font weight (normal), which is the same as the Normal effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Regular,
    /**
     * 500 font weight (medium).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Medium,
    /**
     * 700 font weight (bold).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bold,
    /**
     * 900 font weight (extra bold).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Bolder
}
/**
 * The horizontal alignment of the text paragraph.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TextAlign {
    /**
     * Horizontally centered.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Center,
    /**
     * Aligned with the start.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Start,
    /**
     * Aligned with the end.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    End,
    /**
     * Aligned with both margins.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 11]
     * @since 10
     */
    JUSTIFY,
    /**
     * The text is aligned in the left direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 23
     */
    LEFT = 4,
    /**
     * The text is aligned in the right direction.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 23
     */
    RIGHT = 5
}
/**
 * Display mode when the text is too long.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TextOverflow {
    /**
     * Overflowing content is clipped at the limit of the maximum line width.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None,
    /**
     * Overflowing content is clipped at the limit of the maximum line width. Same effect as **None**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Clip,
    /**
     * An ellipsis (...) is used to represent text overflow.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Ellipsis,
    /**
     * Text continuously scrolls when text overflow occurs.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    MARQUEE
}
/**
 * Text decoration type.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TextDecorationType {
    /**
     * No text decorations.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    None,
    /**
     * Line below the text.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Underline,
    /**
     * Line above the text.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Overline,
    /**
     * Line through the text.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    LineThrough
}
/**
 * The style of the text case.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum TextCase {
    /**
     * The original case of the text is retained.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Normal,
    /**
     * All letters in the text are in lowercase.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    LowerCase,
    /**
     * All letters in the text are in uppercase.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    UpperCase
}
/**
 * The mode of adjusting the text font size to adapt to the layout.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum TextHeightAdaptivePolicy {
    /**
     * the text height adaptation mode to [maxLines]{@link TextAreaAttribute#maxLines(value: number)} first.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    MAX_LINES_FIRST,
    /**
     * Prioritize the **minFontSize** settings.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    MIN_FONT_SIZE_FIRST,
    /**
     * Prioritize the layout constraint settings in terms of height.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    LAYOUT_CONSTRAINT_FIRST
}
/**
 * How menu display is triggered.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 8
 */
declare enum ResponseType {
    /**
     * Shows the shortcut menu by right-clicking the text.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    RightClick,
    /**
     * The menu is displayed when the component is long-pressed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    LongPress
}
/**
 * Sets the hover effect of the component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 8
 */
declare enum HoverEffect {
    /**
     * Default hover effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Auto,
    /**
     * Zoom-in and zoom-out effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Scale,
    /**
     * Background fade-in and fade-out effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Highlight,
    /**
     * No effect.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    None
}
/**
 * Placement enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 8
 */
declare enum Placement {
    /**
     * Left placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Left,
    /**
     * Right placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Right,
    /**
     * Top placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Top,
    /**
     * Bottom placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    Bottom,
    /**
     * TopLeft placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    TopLeft,
    /**
     * TopRight placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    TopRight,
    /**
     * BottomLeft placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    BottomLeft,
    /**
     * BottomRight placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    BottomRight,
    /**
     * LeftTop placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    LeftTop,
    /**
     * LeftBottom placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    LeftBottom,
    /**
     * RightTop placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    RightTop,
    /**
     * RightBottom placement
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    RightBottom
}
/**
 * ArrowPointPosition enumeration description
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum ArrowPointPosition {
    /**
     * Target start position
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    START = 'Start',
    /**
     * Target center position
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    CENTER = 'Center',
    /**
     * Target end position
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    END = 'End'
}
/**
 * Copy options.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form
 * @atomicservice [since 11]
 * @since 9
 */
declare enum CopyOptions {
    /**
     * Copy disabled.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    None = 0,
    /**
     * Copy and paste within the current application only.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    InApp = 1,
    /**
     * Copy and paste across all applications on the device.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    LocalDevice = 2,
    /**
     * Cross-device copy.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @since 11
     * @deprecated since 12
     */
    CROSS_DEVICE = 3
}
/**
 * Sets the response logic and node blocking rules for the hit test.
 *
 * > **NOTE**
 * >
 * > When multiple nodes in a **Stack** component have overlapping touch areas, if the touch point hits a child
 * > component of the topmost node, only the topmost node will undergo hit testing by default. In this case, touch
 * > testing for lower-layer nodes can only be triggered by setting the
 * > [hitTestBehavior]{@link CommonMethod#hitTestBehavior} of the topmost node to **HitTestMode.Transparent**.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 26.0.0]
 * @atomicservice [since 11]
 * @since 9
 */
declare enum HitTestMode {
    /**
     * Default hit test mode. The node itself and its child nodes respond to the hit test, but block the hit test of
     * sibling nodes. It does not affect the hit test of ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 26.0.0]
     * @atomicservice [since 11]
     * @since 9
     */
    Default,
    /**
     * The node itself responds to the hit test and blocks the hit test of child nodes, sibling nodes, and ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 26.0.0]
     * @atomicservice [since 11]
     * @since 9
     */
    Block,
    /**
     * Both the node itself and its child nodes respond to the hit test and do not block the hit test of sibling nodes and
     * ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 26.0.0]
     * @atomicservice [since 11]
     * @since 9
     */
    Transparent,
    /**
     * The node itself does not respond to the hit test and does not block the hit test of child nodes, sibling nodes, and
     * ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 26.0.0]
     * @atomicservice [since 11]
     * @since 9
     */
    None,
    /**
     * The node itself and its child nodes respond to the hit test, preventing all sibling nodes and parent nodes with
     * lower priority from participating in the hit test.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 26.0.0]
     * @atomicservice
     * @since 20
     */
    BLOCK_HIERARCHY,
    /**
     * The node itself does not respond to the hit test, and all its descendants (children, grandchildren, and more) also
     * do not respond to the hit test. It does not affect the hit test of ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 26.0.0]
     * @atomicservice
     * @since 20
     */
    BLOCK_DESCENDANTS
}
/**
 * Title height.
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 9
 */
declare enum TitleHeight {
    /**
     * Title height when only main title is available.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    MainOnly,
    /**
     * Title height when main title and subtitle are both available.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    MainWithSub
}
/**
 * Enumerates the input method modifier keys.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ModifierKey {
    /**
     * Ctrl key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    CTRL,
    /**
     * Shift key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    SHIFT,
    /**
     * Alt key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    ALT
}
/**
 * Enumerates the input method function keys.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum FunctionKey {
    /**
     * Esc key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    ESC,
    /**
     * F1 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F1,
    /**
     * F2 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F2,
    /**
     * F3 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F3,
    /**
     * F4 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F4,
    /**
     * F5 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F5,
    /**
     * F6 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F6,
    /**
     * F7 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F7,
    /**
     * F8 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F8,
    /**
     * F9 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F9,
    /**
     * F10 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F10,
    /**
     * F11 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F11,
    /**
     * F12 key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    F12,
    /**
     * Tab key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    TAB,
    /**
     * Up arrow key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DPAD_UP,
    /**
     * Down arrow key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DPAD_DOWN,
    /**
     * Left arrow key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DPAD_LEFT,
    /**
     * Right arrow key on the keyboard.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DPAD_RIGHT
}
/**
 * The alignment mode of the image relative to the line height.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ImageSpanAlignment {
    /**
     * The image is bottom aligned with the text baseline.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    BASELINE,
    /**
     * The image is bottom aligned with the line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    BOTTOM,
    /**
     * The image is centered aligned with the line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    CENTER,
    /**
     * The image is top aligned with the line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    TOP,
    /**
     * The alignment mode follows the parent component of the **Text** component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    FOLLOW_PARAGRAPH
}
/**
 * Sets how the component content is obscured.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 11]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ObscuredReasons {
    /**
     * The content is replaced by a placeholder.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    PLACEHOLDER = 0
}
/**
 * The polymorphic style of the text box.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 11]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum TextContentStyle {
    /**
     * Default style. The caret width is fixed at 1.5 vp, and the caret height is subject to the background height and
     * font size of the selected text.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    DEFAULT,
    /**
     * Inline input style. The background height of the selected text is the same as the height of the text box.
     *
     * This style is used in scenarios where editing and non-editing states are obvious, for example, renaming in the file
     * list view.
     *
     * The **showError** attribute is not supported for this style.
     *
     * This style does not allow for text dragging and dropping.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    INLINE
}
/**
 * Enum of click effect level.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ClickEffectLevel {
    /**
     * Small area (light)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    LIGHT = 0,
    /**
     * Medium area (stable)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    MIDDLE = 1,
    /**
     * Large area (heavy)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    HEAVY = 2
}
/**
 * The type of XComponent
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 12]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum XComponentType {
    /**
     * Surface type. The default type is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 12]
     * @atomicservice [since 11]
     * @since 10
     */
    SURFACE = 0,
    /**
     * Component type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice [since 11]
     * @since 10
     * @deprecated since 12
     * @useinstead Column
     */
    COMPONENT,
    /**
     * Texture type.
     * Supports EGL/OpenGLES and media data rendering. Custom drawing content is
     * composited with XComponent’s native content before display.
     * Key features:
     *
     * 1. Maintains frame synchronization between GPU textures and ArkUI drawing commands.
     * 2. Supports unified animation with built-in components.
     * 3. Utilizes GPU composition, which may have higher power consumption than
     * the SURFACE type using the display subsystem (DSS).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 23]
     * @atomicservice [since 11]
     * @since 10
     */
    TEXTURE = 2,
    /**
     * Node type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     * @deprecated since 20
     * @useinstead ContentSlot
     */
    NODE = 3
}
/**
 * Nested scroll nested mode
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 11]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum NestedScrollMode {
    /**
     * Only Self response scrolling.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    SELF_ONLY,
    /**
     * Self priority response scrolling.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    SELF_FIRST,
    /**
     * Parent scrollable component priority response scrolling.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    PARENT_FIRST,
    /**
     * Both self and parent scrollable component response scrolling.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    PARALLEL
}
/**
 * The possible source of scroll event
 * @enum { number } ScrollSource
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum ScrollSource {
    /**
     * Drag events.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DRAG = 0,
    /**
     * Fling after the drag has ended with velocity.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    FLING,
    /**
     * Over scroll with EdgeEffect.Spring.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    EDGE_EFFECT,
    /**
     * Other user input except drag, such as mouse wheel, key event.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    OTHER_USER_INPUT,
    /**
     * Drag events of scroll bar.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SCROLL_BAR,
    /**
     * Fling after the drag on scroll bar has ended with velocity.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SCROLL_BAR_FLING,
    /**
     * Member methods of Scroller without animation.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SCROLLER,
    /**
     * Member methods of Scroller with animation.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SCROLLER_ANIMATION
}
/**
 * Enumerates the modes in which the final state of the component's content is rendered during its width and height
 * animation process.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form [since 18]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum RenderFit {
    /**
     * The component's content stays at the final size and always aligned with the center of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    CENTER = 0,
    /**
     * The component's content stays at the final size and always aligned with the top center of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    TOP = 1,
    /**
     * The component's content stays at the final size and always aligned with the bottom center of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    BOTTOM = 2,
    /**
     * The component's content stays at the final size and always aligned with the left of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    LEFT = 3,
    /**
     * The component's content stays at the final size and always aligned with the right of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RIGHT = 4,
    /**
     * The component's content stays at the final size and always aligned with the upper left corner of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    TOP_LEFT = 5,
    /**
     * The component's content stays at the final size and always aligned with the upper right corner of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    TOP_RIGHT = 6,
    /**
     * The component's content stays at the final size and always aligned with the lower left corner of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    BOTTOM_LEFT = 7,
    /**
     * The component's content stays at the final size and always aligned with the lower right corner of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    BOTTOM_RIGHT = 8,
    /**
     * The component's content is always resized to fill the component's content box, without considering its aspect ratio
     * in the final state.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_FILL = 9,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to fit within the
     * component's content box. It is always aligned with the center of the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_CONTAIN = 10,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to fit within the
     * component's content box. When there is remaining space in the width direction of the component, the content is left
     * -aligned with the component. When there is remaining space in the height direction of the component, the content is
     * top-aligned with the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_CONTAIN_TOP_LEFT = 11,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to fit within the
     * component's content box. When there is remaining space in the width direction of the component, the content is
     * right-aligned with the component. When there is remaining space in the height direction of the component, the
     * content is bottom-aligned with the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_CONTAIN_BOTTOM_RIGHT = 12,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to cover the component's
     * entire content box. It is always aligned with the center of the component, so that its middle part is displayed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_COVER = 13,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to cover the component's
     * entire content box. When there is remaining space in the width direction, the content is left-aligned with the
     * component, so that its left part is displayed. When there is remaining space in the height direction, the content
     * is top-aligned with the component, so that its top part is displayed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_COVER_TOP_LEFT = 14,
    /**
     * While maintaining its aspect ratio in the final state, the component's content is scaled to cover the component's
     * entire content box. When there is remaining space in the width direction, the content is right-aligned with the
     * component, so that its right part is displayed. When there is remaining space in the height direction, the content
     * is bottom-aligned with the component, so that its bottom part is displayed.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 18]
     * @atomicservice [since 11]
     * @since 10
     */
    RESIZE_COVER_BOTTOM_RIGHT = 15
}
/**
 * The Button Style of dialog,
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum DialogButtonStyle {
    /**
     * Default Style.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    DEFAULT = 0,
    /**
     * Highlight Style.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    HIGHLIGHT = 1
}
/**
 * The word break rule.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 11
 */
declare enum WordBreak {
    /**
     * Word breaks can occur between any two characters for Chinese, Japanese, and Korean (CJK) text, but can occur only
     * at a space character for non-CJK text (such as English).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    NORMAL = 0,
    /**
     * Line breaks can occur between any two characters for non-CJK text. For CJK text, the effect is the same as that of
     * **NORMAL**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    BREAK_ALL = 1,
    /**
     * This option has the same effect as **BREAK_ALL** for non-CJK text, except that it preferentially wraps lines at
     * appropriate characters (for example, spaces). If no breakpoints are found, it breaks between any two characters.
     * For CJK text, the effect is the same as that of **NORMAL**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    BREAK_WORD = 2,
    /**
     * This option has the same effect as **BREAK_ALL** for non-CJK text, except that it preferentially wraps lines at
     * appropriate characters (for example, spaces). If no breakpoints are found, it breaks between any two characters.
     * For CJK text, the effect is the same as that of **NORMAL**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 18
     */
    HYPHENATION = 3
}
/**
 * The line break rule.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum LineBreakStrategy {
    /**
     * Places as many words on a line as possible and moves to the next line only if no more words can fit into the same
     * line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    GREEDY = 0,
    /**
     * Fills in lines as much as possible on the basis of **BALANCED**, which may results in a large blank area on the
     * last line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    HIGH_QUALITY = 1,
    /**
     * Without splitting words, the width of each line in a paragraph is the same as much as possible.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    BALANCED = 2
}
/**
 * The position of ellipsis.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum EllipsisMode {
    /**
     * An ellipsis is used at the start of the line of text. This applies to single-line text scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    START = 0,
    /**
     * An ellipsis is used at the center of the line of text. This applies to single-line text scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    CENTER = 1,
    /**
     * An ellipsis is used at the end of the line of text. This applies to single-line and multi-line text scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    END = 2,
    /**
     * An ellipsis is used at the start of the line of text. This applies to single-line and multi-line text scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 24
     */
    MULTILINE_START = 3,
    /**
     * An ellipsis is used at the center of the line of text. This applies to single-line and multi-line text scenarios.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 24
     */
    MULTILINE_CENTER = 4
}
/**
 * This type allows for an object of a custom type or **undefined**.
 *
 * @unionmember { T } The object can be of any custom type.
 * @unionmember { undefined } The object can be **undefined**.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare type Nullable<T> = T | undefined;
/**
 * Decide whether the width of select menu fit the trigger or content
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum OptionWidthMode {
    /**
     * The menu width fit the content.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FIT_CONTENT = 'fit_content',
    /**
     * The menu width fit the trigger.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FIT_TRIGGER = 'fit_trigger'
}
/**
 * Enumerates the fold status.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare enum FoldStatus {
    /**
     * Fold Status Unknown.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FOLD_STATUS_UNKNOWN = 0,
    /**
     * Fold Status Expanded.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FOLD_STATUS_EXPANDED = 1,
    /**
     * Fold Status Folded.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FOLD_STATUS_FOLDED = 2,
    /**
     * Fold Status Half Folded.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    FOLD_STATUS_HALF_FOLDED = 3
}
/**
 * Enumerates the app rotation.
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 12
 */
declare enum AppRotation {
    /**
     * App does not rotate to display vertically.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    ROTATION_0 = 0,
    /**
     * App rotates 90 degrees clockwise to display horizontally.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    ROTATION_90 = 1,
    /**
     * App rotates 180 degrees clockwise to display vertically in reverse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    ROTATION_180 = 2,
    /**
     * App rotates 270 degrees clockwise to display horizontally in reverse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    ROTATION_270 = 3
}
/**
 * Enum of EmbeddedType
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 12
 */
declare enum EmbeddedType {
    /**
     * The EmbeddedComponent show the UI in EmbeddedUIExtensionAbility.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    EMBEDDED_UI_EXTENSION = 0
}
/**
 * The scrolling strategy for the marquee after its attributes are updated.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum MarqueeUpdateStrategy {
    /**
     * After the marquee attributes are updated, the marquee scrolls from the start position.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DEFAULT = 0,
    /**
     * After the marquee attributes are updated, the marquee scrolls from the current position.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    PRESERVE_POSITION = 1
}
/**
 * The style of the text decoration.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum TextDecorationStyle {
    /**
     * Single solid line (default value).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SOLID = 0,
    /**
     * Double solid line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DOUBLE = 1,
    /**
     * Dotted line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DOTTED = 2,
    /**
     * Dashed line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    DASHED = 3,
    /**
     * Wavy line.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    WAVY = 4
}
/**
 * Sets whether text can be selected and focused on.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum TextSelectableMode {
    /**
     * The text is selectable, but not focusable. Setting the **selection**, **bindSelectionMenu**, or **copyOption**
     * attribute does not affect the behavior.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SELECTABLE_UNFOCUSABLE = 0,
    /**
     * The text is selectable and focusable. It obtains focus when touched.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    SELECTABLE_FOCUSABLE = 1,
    /**
     * The text is not selectable nor focusable. The **selection**, **bindSelectionMenu**, and **copyOption** attributes
     * do not work in this case.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    UNSELECTABLE = 2
}
/**
 * Type of accessibility hover event.
 *
 * @enum {number}
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum AccessibilityHoverType {
    /**
     * Hover enter type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    HOVER_ENTER = 0,
    /**
     * Hover move type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    HOVER_MOVE = 1,
    /**
     * Hover exit type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    HOVER_EXIT = 2,
    /**
     * Hover cancel type.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    HOVER_CANCEL = 3
}
/**
 * Enumerates the width breakpoint values corresponding to different window width thresholds. The values are returned
 * through [getWindowWidthBreakpoint]{@link @ohos.arkui.UIContext:UIContext#getWindowWidthBreakpoint}.
 *
 * The following table lists default width breakpoint thresholds for typical devices, serving as a reference for
 * responsive layout design based on window width breakpoints. Device manufacturers may customize these thresholds
 * through product-specific configurations when needed.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 22]
 * @atomicservice
 * @since 13
 */
declare enum WidthBreakpoint {
    /**
     * The window width is less than 320 vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    WIDTH_XS = 0,
    /**
     * The window width is greater than or equal to 320 vp and less than 600 vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    WIDTH_SM = 1,
    /**
     * The window width is greater than or equal to 600 vp and less than 840 vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    WIDTH_MD = 2,
    /**
     * The window width is greater than or equal to 840 vp and less than 1440 vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    WIDTH_LG = 3,
    /**
     * The window width is greater than or equal to 1440 vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    WIDTH_XL = 4
}
/**
 * Enumerates the height breakpoint values corresponding to different window aspect ratio thresholds. The values are
 * returned through [getWindowHeightBreakpoint]{@link @ohos.arkui.UIContext:UIContext#getWindowHeightBreakpoint}.
 *
 * The following table lists default aspect ratio breakpoint thresholds for typical devices, serving as a reference for
 * responsive layout design based on window aspect ratios. Device manufacturers may customize these thresholds through
 * product-specific configurations when needed.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 22]
 * @atomicservice
 * @since 13
 */
declare enum HeightBreakpoint {
    /**
     * The window aspect ratio is less than 0.8.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    HEIGHT_SM = 0,
    /**
     * The window aspect ratio is greater than or equal to 0.8 and less than 1.2.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    HEIGHT_MD = 1,
    /**
     * The window aspect ratio is greater than or equal to 1.2.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 22]
     * @atomicservice
     * @since 13
     */
    HEIGHT_LG = 2
}
/**
 * Enumerates the axis types for focus axis events.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 15
 */
declare enum AxisModel {
    /**
     * Game controller x-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_X = 0,
    /**
     * Game controller y-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_Y = 1,
    /**
     * Game controller z-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_Z = 2,
    /**
     * Game controller rz-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_RZ = 3,
    /**
     * Game controller GAS-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_GAS = 4,
    /**
     * Game controller BRAKE-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_BRAKE = 5,
    /**
     * Game controller HAT0X-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_HAT0X = 6,
    /**
     * Game controller HAT0Y-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 15
     */
    ABS_HAT0Y = 7,
    /**
     * Game controller RX-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_RX = 8,
    /**
     * Game controller RY-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_RY = 9,
    /**
     * Game controller THROTTLE-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_THROTTLE = 10,
    /**
     * Game controller RUDDER-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_RUDDER = 11,
    /**
     * Game controller WHEEL-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_WHEEL = 12,
    /**
     * Game controller HAT1X-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT1X = 13,
    /**
     * Game controller HAT1Y-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT1Y = 14,
    /**
     * Game controller HAT2X-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT2X = 15,
    /**
     * Game controller HAT2Y-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT2Y = 16,
    /**
     * Game controller HAT3X-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT3X = 17,
    /**
     * Game controller HAT3Y-axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 23
     */
    ABS_HAT3Y = 18
}
/**
 * Enumerates focus wrapping modes for cross-axis directional navigation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 20
 */
declare enum FocusWrapMode {
    /**
     * Cross-axis directional navigation does not wrap focus.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    DEFAULT = 0,
    /**
     * Cross-axis directional navigation wraps focus.
     *
     * In irregular grid layouts, when moving focus along the cross axis, the system prioritizes focusable items within
     * the same row.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    WRAP_WITH_ARROW = 1
}
/**
 * Specify the number of columns for different responsive breakpoints.
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 22
 */
declare enum PresetFillType {
    /**
     * For Lists and Swipers: One column is displayed on SM (and smaller),two on MD, and three on LG (and larger)
     * devices. For Grid and WaterFlow: two column is displayed on SM (and smaller), three on MD, and five on LG
     * (and larger) devices.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    BREAKPOINT_DEFAULT = 0,
    /**
     *For SM (and smaller),MD, and LG (and larger) devices, one,two,and three columns are displayed respectively.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    BREAKPOINT_SM1MD2LG3 = 1,
    /**
     *For SM (and smaller),MD, and LG (and larger) devices,two,three,and five columns are displayed respectively.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    BREAKPOINT_SM2MD3LG5 = 2
}
/**
 * Enumerates the crown actions.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 18
 */
declare enum CrownAction {
    /**
     * Rotation crown behavior begins.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     * @deprecated since 24
     */
    BEGIN = 0,
    /**
     * Rotation crown behavior update.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    UPDATE = 1,
    /**
     * The rotation crown behavior ends.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    END = 2
}
/**
 * Enumerates the sensitivity levels for crown rotation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 18
 */
declare enum CrownSensitivity {
    /**
     * Low sensitivity.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    LOW = 0,
    /**
     * Medium sensitivity.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    MEDIUM = 1,
    /**
     * High sensitivity.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 18
     */
    HIGH = 2
}
/**
 * Page flip mode of Swiper and Tabs on mouse wheel event.

 * @enum { number } PageFlipMode
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 15
 */
declare enum PageFlipMode {
    /**
     * Turn pages continuously according to the number of mouse wheel events.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 15
     */
    CONTINUOUS = 0,
    /**
     * The page-turning animation remains unresponsive to other mouse wheel events until it completes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 15
     */
    SINGLE = 1
}
/**
 * Enumerates the types of axis actions for axis events.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 17
 */
declare enum AxisAction {
    /**
     * No axis event.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 17
     */
    NONE = 0,
    /**
     * The axis event begins.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 17
     */
    BEGIN = 1,
    /**
     * The axis event is in progress.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 17
     */
    UPDATE = 2,
    /**
     * The axis event ends.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 17
     */
    END = 3,
    /**
     * The axis event is canceled.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 17
     */
    CANCEL = 4
}
/**
 * Type of focus draw level.
 *
 * @enum {number}
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 19
 */
declare enum FocusDrawLevel {
    /**
     * Draw focus on node self level.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 19
     */
    SELF = 0,
    /**
     * Draw focus on top level.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 19
     */
    TOP = 1
}
/**
 * Menu divider mode.
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 19
 */
declare enum DividerMode {
    /**
     * Menu divider mode floating above menu.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    FLOATING_ABOVE_MENU = 0,
    /**
     * Menu divider mode embedded in menu.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    EMBEDDED_IN_MENU = 1
}
/**
 * Enumerates interaction event types that can be queried.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 19
 */
declare enum EventQueryType {
    /**
     * Click event.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    ON_CLICK = 0
}
/**
 * Follow position type.
 *
 * @enum { number }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 20
 */
declare enum TipsAnchorType {
    /**
     * Follow the component.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    TARGET,
    /**
     * Follow the cursor.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    CURSOR
}
/**
 * Dialog display mode.
 *
 * @enum { DialogDisplayMode }
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare enum DialogDisplayMode {
    /**
     * The dialog displayed based on screen.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    SCREEN_BASED = 0,
    /**
     * The dialog displayed based on application window.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    WINDOW_BASED = 1
}
/**
 * Enumerates color space types for specifying color rendering modes.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 20
 */
declare enum ColorSpace {
    /**
     * Standard RGB color space, suitable for most display devices.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    SRGB = 0,
    /**
     * Display P3 color space with wider gamut, designed for high-end display devices.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    DISPLAY_P3 = 1
}
/**
 * Enumerates animatable property types for component animations.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 20
 */
declare enum AnimationPropertyType {
    /**
     * Rotation angles for the x, y, and z axes. Parameters: 3. Unit: degrees (°).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    ROTATION = 0,
    /**
     * Translation offsets for the x and y axes. Parameters: 2. Unit: px.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    TRANSLATION = 1,
    /**
     * Scale factors for the x and y axes. Parameters: 2. Value range: (-∞, +∞).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    SCALE = 2,
    /**
     * Opacity value. Parameters: 1. Value range: [0, 1].
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    OPACITY = 3
}
/**
 * Sets the type of the input tool applicable to the touch target.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 22
 */
declare enum ResponseRegionSupportedTool {
    /**
     * All input tool types.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    ALL = 0,
    /**
     * Finger.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    FINGER = 1,
    /**
     * Stylus.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    PEN = 2,
    /**
     * Mouse.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    MOUSE = 3
}
/**
 * Enumerates the axis types for axis events.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 22
 */
declare enum AxisType {
    /**
     * Vertical scroll axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 22
     */
    VERTICAL_AXIS = 0,
    /**
     * Horizontal scroll axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 22
     */
    HORIZONTAL_AXIS = 1,
    /**
     * Pinch axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 22
     */
    PINCH_AXIS = 2
}
/**
 * Defines the input event subtype mask for monitoring.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare enum InputEventSubTypeMask {
    /**
     * Mouse left button down.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    LEFT_MOUSE_DOWN = 1 << 0,
    /**
     * Mouse left button up.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    LEFT_MOUSE_UP = 1 << 1,
    /**
     * Mouse right button down.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    RIGHT_MOUSE_DOWN = 1 << 2,
    /**
     * Mouse right button up.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    RIGHT_MOUSE_UP = 1 << 3,
    /**
     * Mouse middle button down.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    MIDDLE_MOUSE_DOWN = 1 << 4,
    /**
     * Mouse middle button up.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    MIDDLE_MOUSE_UP = 1 << 5,
    /**
     * Mouse left button dragging.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    LEFT_MOUSE_DRAGGING = 1 << 6,
    /**
     * Mouse right button dragging.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    RIGHT_MOUSE_DRAGGING = 1 << 7,
    /**
     * Mouse middle button dragging.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    MIDDLE_MOUSE_DRAGGING = 1 << 8,
    /**
     * Touch down.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    TOUCH_DOWN = 1 << 9,
    /**
     * Touch up.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    TOUCH_UP = 1 << 10,
    /**
     * Physical keyboard key down.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    KEY_DOWN = 1 << 11,
    /**
     * Physical keyboard key up.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    KEY_UP = 1 << 12
}
/**
 * Defines the input event intercept action.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare enum InputEventInterceptAction {
    /**
     * Continue to deliver the event to the UI framework.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    CONTINUE = 0,
    /**
     * Block the event from being delivered to the UI framework.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    BLOCK = 1
}
/**
 * Defines whether the dispatched event is a competitive gesture. In the competitive scenario, only one of the original
 * node and target node of the gesture responds. In the non‑competitive scenario, both nodes can respond simultaneously.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 24
 */
declare enum CompetitionStrategy {
    /**
     * Indicates that the dispatched event is a non‑competitive gesture.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 24
     */
    DEFAULT = 0,
    /**
     * Indicates that the dispatched event is a competitive gesture.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 24
     */
    COMPETITION = 1
}
/**
 * Define the gesture and events collection intervention operations.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare enum GestureCollectIntervention {
    /**
     * Continue the normal collection process. No intervention will be applied.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    CONTINUE = 0,
    /**
     * Discard all pending lower-priority gestures and events.
     * This includes gestures from left sibling nodes and ancestor nodes (parent and above).
     * Only the already collected gestures from the current node and higher-priority nodes will be retained.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    DISCARD_LOWER = 1,
    /**
     * Discard already collected higher-priority gestures and events.
     * This removes gestures from right sibling nodes that have been collected.
     * The collection will continue with lower-priority gestures (left siblings and ancestors).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    DISCARD_HIGHER = 2,
    /**
     * Discard gestures and events from the current node itself.
     * The current node's gestures and events will be excluded from the gesture tree.
     * Gestures from sibling nodes (both left and right) and ancestor nodes will still be collected.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    DISCARD_SELF = 3,
    /**
     * Discard gestures and events from left sibling nodes that are pending collection.
     * Gestures and events from the current node and already collected right sibling nodes will be retained.
     * The collection will continue with ancestor nodes.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    DISCARD_LOWER_PRIORITY_SIBLINGS = 4
}
/**
 * Enumerates the raw input event types.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare enum RawInputEventType {
    /**
     * Touch event.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    TOUCH = 0,
    /**
     * Mouse event.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    MOUSE = 1
}
/**
 * Define the smart gesture shortcut priority enumeration.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 26.0.0
 */
declare enum GestureShortcut {
    /**
     * Primary priority.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    PRIMARY = 0
}
/**
 * Define the actions for smart gesture shortcut.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 26.0.0
 */
declare enum SmartGestureAction {
    /**
     * No action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    NONE = 0,
    /**
     * Page forward action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    PAGE_FORWARD = 1,
    /**
     * Scroll forward action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    SCROLL_FORWARD = 2,
    /**
     * Select action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    SELECT = 3,
    /**
     * Click action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    CLICK = 4,
    /**
     * Back press action.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    BACK_PRESS = 5
}
/**
 * Define the smart gesture's operation intention.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 26.0.0
 */
declare enum OperateIntention {
    /**
     * Tap intention.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    TAP = 0,
    /**
     * Slide forward intention.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    SLIDE_FORWARD = 1,
    /**
     * Back press intention.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    BACK_PRESS = 2
}
