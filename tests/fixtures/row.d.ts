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
 * Sets the spacing between child components of the **Row** component.
 *
 * > **NOTE**
 * >
 * > To standardize anonymous object definitions, the element definitions here have been revised in API version 18.
 * > While starting version information is preserved for historical anonymous objects, there may be cases where the
 * > outer element's @since version number is higher than inner element's. This does not affect interface usability.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
declare interface RowOptions {
    /**
     * Spacing between child components.
     * Since API version 9, this parameter does not take effect when it is set to a negative number or when
     * **justifyContent** is set to **FlexAlign.SpaceBetween**, **FlexAlign.SpaceAround** or **FlexAlign.SpaceEvenly**.
     * Unit: vp. If an invalid value is set, the default value is used instead.
     * > **NOTE**
     * >
     * > The value of **space** can be a number greater than or equal to 0 or a string that can be converted to a number.
     * > Default value: **0**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    space?: string | number;
}
/**
 * Sets the spacing between child components of the **Row** component.
 *
 * > **NOTE**
 * >
 * > To standardize anonymous object definitions, the element definitions here have been revised in API version 18.
 * > While starting version information is preserved for historical anonymous objects, there may be cases where the
 * > outer element's @since version number is higher than inner element's. This does not affect interface usability.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
interface RowOptionsV2 {
    /**
     * Spacing between child components.
     * This parameter does not take effect if the value specified is a negative number, or if **justifyContent** is set to
     * **FlexAlign.SpaceBetween**, **FlexAlign.SpaceAround**, or **FlexAlign.SpaceEvenly**.
     * Unit: vp. Invalid values are treated as the default value.
     * > **NOTE**
     * >
     * > The value of **space** can be a number greater than or equal to 0, a string that can be converted to a number, or a
     * > Resource type that can be converted to a number. Default value: **0**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    space?: SpaceType;
}
/**
 * The components are laid out horizontally
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
interface RowInterface {
    /**
     * Creates a horizontal linear layout container. You can set the spacing between child components.
     *
     * > **NOTE**
     * >
     * > Excessive component nesting (either too deep a hierarchy or too many nested components) incurs significant
     * > performance overhead. For performance purposes, you are advised to remove redundant nodes to simplify the
     * > component tree, use layout boundaries to reduce redundant layout calculations, properly apply rendering control
     * > syntax and layout component methods to minimize unnecessary re-renders and computations. For details about the
     * > best practices, see
     * > [Layout Optimization](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-improve-layout-performance)
     * > .
     *
     * @param { object } value [since 7 - 17]
     * @param { ?RowOptions } options - Spacing between elements in the horizontal layout. The value can be of the number
     *     or string type. [since 18]
     * @returns { RowAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (options?: RowOptions): RowAttribute;
    /**
     * Creates a horizontal linear layout container. You can set the spacing between child components.
     *
     * @param { ?(RowOptions | RowOptionsV2) } options - Spacing between elements in a horizontal layout. The value can be
     *     of the number, string, or Resource type.
     * @returns { RowAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    (options?: RowOptions | RowOptionsV2): RowAttribute;
}
/**
 * In addition to the [universal attributes]{@link CommonMethod}, the following attributes are supported.
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
declare class RowAttribute extends CommonMethod<RowAttribute> {
    /**
     * Sets the alignment mode of child components in the vertical direction.
     *
     * @param { VerticalAlign } value - Alignment mode of child components in the vertical direction.<br>Default value:
     *     **VerticalAlign.Center**
     * @returns { RowAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    alignItems(value: VerticalAlign): RowAttribute;
    /**
     * Sets the alignment mode of the child components in the horizontal direction.
     *
     * @param { FlexAlign } value - Alignment mode of child components in the horizontal direction.<br>Default value:
     *     **FlexAlign.Start**
     * @returns { RowAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    justifyContent(value: FlexAlign): RowAttribute;
    /**
     * Sets whether to reverse the horizontal arrangement of child components.
     *
     * @param { Optional<boolean> } isReversed - Whether to reverse the horizontal arrangement of child components.<br>
     *     Default value: **true**. **true**: Child components are arranged in reverse order horizontally. **false**:
     *     Child components are arranged in normal order horizontally.
     * @returns { RowAttribute } The attribute of the row.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    reverse(isReversed: Optional<boolean>): RowAttribute;
}
/**
 * The **Row** component lays out child components horizontally.
 * > **NOTE**
 * >
 * > If no width or height is set for the **Row** component, the component automatically adapts to the size of its child
 * > components in the main axis and cross axis respectively.
 * >
 * > **Child Components**
 * >
 * > Supported
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const Row: RowInterface;
/**
 * Defines Row Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const RowInstance: RowAttribute;
