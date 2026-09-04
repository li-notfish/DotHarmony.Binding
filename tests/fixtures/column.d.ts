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
 * Describes the supported data types for the **space** parameter in the constructors of the **Column** component. The
 * type is a union of the following types.
 *
 * @unionmember { string } Represents a string value. It can take any string value.
 * @unionmember { number } Represents a numeric value. It can take any numerical value.
 * @unionmember { Resource } Represents a resource reference type. It can take values from system resources or
 *      application resources.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
declare type SpaceType = string | number | Resource;
/**
 * Sets the spacing between child components of the **Column** component.
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
interface ColumnOptions {
    /**
     * Vertical spacing between two adjacent child components.
     * This parameter has no effect if the value specified is a negative number, or if
     * [justifyContent](@ColumnAttribute#justifyContent) is set to
     * **FlexAlign.SpaceBetween**, **FlexAlign.SpaceAround**, or **FlexAlign.SpaceEvenly**
     * Unit: vp, Invalid values are treated as the default value.
     * **NOTE**
     * The value of **space** can be a number greater than or equal to 0 or a string that can be converted to a number.
     * Default value: **0**.
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
 * Sets the spacing between child components of the **Column** component.
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
interface ColumnOptionsV2 {
    /**
     * Vertical spacing between two adjacent child components.
     * This parameter has no effect if the value specified is a negative number, or if **justifyContent** is set to
     * **FlexAlign.SpaceBetween**, **FlexAlign.SpaceAround**, or **FlexAlign.SpaceEvenly**.
     * Unit: vp, Invalid values are treated as the default value.
     * **NOTE**
     * The value of **space** can be a number greater than or equal to 0, a string that can be converted to a number, or a
     * Resource type that can be converted to a number. Default value: **0**.
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
 * Defines the Column Component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
interface ColumnInterface {
    /**
     * Creates a vertical linear layout container. You can set the spacing between child components.
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
     * @param { ColumnOptions } [options] - Vertical spacing between two adjacent child components. The value can be of
     *     the number or string type. [since 18]
     * @returns { ColumnAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (options?: ColumnOptions): ColumnAttribute;
    /**
     * Creates a vertical linear layout container. You can set the spacing between child components.
     *
     * @param { ColumnOptions | ColumnOptionsV2 } [options] - Vertical spacing between two adjacent child components. The
     *     value can be of the number, string, or Resource type.
     * @returns { ColumnAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    (options?: ColumnOptions | ColumnOptionsV2): ColumnAttribute;
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
declare class ColumnAttribute extends CommonMethod<ColumnAttribute> {
    /**
     * Alignment mode of the child components in the horizontal direction.
     *
     * @param { HorizontalAlign } value - Alignment mode of child components in the horizontal direction.<br>Default value
     *     : **HorizontalAlign.Center**
     * @returns { ColumnAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    alignItems(value: HorizontalAlign): ColumnAttribute;
    /**
     * Alignment mode of the child components in the vertical direction.
     *
     * @param { FlexAlign } value - Alignment mode of child components in the vertical direction.<br>Default value:
     *     **FlexAlign.Start**
     * @returns { ColumnAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    justifyContent(value: FlexAlign): ColumnAttribute;
    /**
     * Sets whether to reverse the vertical arrangement of child components.
     *
     * @param { Optional<boolean> } isReversed - Whether to reverse the vertical arrangement of child components.<br>
     *     Default value: **true**. **true**: Child components are arranged in reverse order vertically. **false**: Child
     *     components are arranged in normal order vertically.
     * @returns { ColumnAttribute } The attribute of the column.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    reverse(isReversed: Optional<boolean>): ColumnAttribute;
}
/**
 * The **Column** component lays out child components vertically.
 * > **NOTE**
 * >
 * > If no height or width is set for the **Column** component, the component automatically adapts to the size of its
 * > child components in the main axis and cross axis respectively.
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
declare const Column: ColumnInterface;
/**
 * Defines Column Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const ColumnInstance: ColumnAttribute;
