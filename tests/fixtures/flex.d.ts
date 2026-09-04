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
 * Describes the layout and alignment of child components within the **Flex** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare interface FlexOptions {
    /**
     * Direction in which child components are arranged in the **Flex** component, that is, the direction of the main
     * axis.
     * If an invalid value is passed, the default value will be used. Default value: **FlexDirection.Row**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    direction?: FlexDirection;
    /**
     * Whether the **Flex** component has a single line or multiple lines.
     * If an invalid value is passed, the default value will be used.
     * > **NOTE**
     * >
     * > When wrapped onto multiple lines, the child elements on the new line are stacked in the direction based on the
     * > cross axis direction. Default value: **FlexWrap.NoWrap**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    wrap?: FlexWrap;
    /**
     * Alignment mode of the child components in the **Flex** component along the main axis.
     * If an invalid value is passed, the default value will be used. Default value: **FlexAlign.Start**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    justifyContent?: FlexAlign;
    /**
     * Alignment mode of the child components in the **Flex** component along the cross axis.
     * If an invalid value is passed, the default value will be used. Default value: **ItemAlign.Start**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    alignItems?: ItemAlign;
    /**
     * Alignment mode of multiple lines when there is extra space along the cross axis. This parameter is valid only when
     * **wrap** is set to **Wrap** or **WrapReverse**.
     * If an invalid value is passed, the default value will be used. Default value: **FlexAlign.Start**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    alignContent?: FlexAlign;
    /**
     * Spacing between child components along the main axis or cross axis of the **Flex** component.
     * Invalid values are treated as the default value. This parameter does not take effect if the value specified is a
     * negative number or percentage, or if **justifyContent** is set to **FlexAlign.SpaceBetween**,
     * **FlexAlign.SpaceAround**, or **FlexAlign.SpaceEvenly**.
     * Default value: **{main: LengthMetrics.px(0), cross: LengthMetrics.px(0)}**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    space?: FlexSpaceOptions;
}
/**
 * Sets the spacing between child components along the main axis or cross axis of the **Flex** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface FlexSpaceOptions {
    /**
     * Space on the main axis of the **Flex** component.
     *
     * Default value: **LengthMetrics.px(0)**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    main?: LengthMetrics;
    /**
     * Space on the cross axis of the **Flex** component.
     *
     * Default value: **LengthMetrics.px(0)**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    cross?: LengthMetrics;
}
/**
 * Provides a Flex component that uses the flexible box model for layout.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
interface FlexInterface {
    /**
     * Creates a **Flex** component.
     *
     * @param { FlexOptions } value - Parameters of the child components in the **Flex** component.
     * @returns { FlexAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (value?: FlexOptions): FlexAttribute;
}
/**
 * The [universal attributes]{@link CommonMethod} are supported.
 *
 * The [universal events]{@link CommonMethod} are supported.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare class FlexAttribute extends CommonMethod<FlexAttribute> {
}
/**
 * The **Flex** component is a container that uses the flexible box model for layout. It provides an efficient mechanism
 * for arranging and aligning child elements, as well as distributing available space among them.
 * For details, see [Flex Layout](docroot://ui/arkts-layout-development-flex-layout.md).
 * > **NOTE**
 * >
 * > - This component is supported since API version 7. Updates will be marked with a superscript to indicate their
 * > earliest API version.
 * >
 * > - The **Flex** component adapts the layout of flex items during rendering. This may affect the performance.
 * > Therefore, you are advised to use [Column]{@link Column} or [Row]{@link Row} instead under scenarios where
 * > consistently high performance is required. For best practices, see
 * > [Using Layout Components Properly](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-improve-layout-performance#section12745188175420)
 * >
 * > - If the main axis length of the **Flex** component is unspecified, it follows the size of the parent container by
 * > default. If the **Flex** component contains child components for which
 * > [position]{@link CommonMethod#position} is set, the
 * > **Flex** component does not follow the size of the parent container. If the main axis length of the **Column** or
 * > **Row** component is unspecified, it follows the size of the child nodes by default.
 * >
 * > - If **Flex**, **Column**, or **Row** containers have no child components and no explicit width or height settings,
 * > their default width or height is **-1**.
 * >
 * > - You can set the main axis length of a **Flex** component to **auto** to make it adapt to the layout of its child
 * > components. This way, the **Flex** component's length is subject to the **constraintSize** attribute and the
 * > maximum and minimum length constraints passed from the parent container, with **constraintSize** taking precedence.
 * >
 * > **Child Components**
 * >
 * > This component can contain child components.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const Flex: FlexInterface;
/**
 * Defines Flex Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const FlexInstance: FlexAttribute;
