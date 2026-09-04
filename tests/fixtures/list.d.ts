/*
 * Copyright (c) 2021-2025 Huawei Device Co., Ltd.
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
 * Enumerates the scrolling states.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ScrollState {
    /**
     * Idle state. Triggered when the scroll state returns to idle, and when the controller's non-animated methods are
     * used to control the scroll.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Idle,
    /**
     * Scrolling state. Triggered when the list is dragged with the finger, when the scrollbar is dragged, or when the
     * mouse scroll wheel is used.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Scroll,
    /**
     * Inertial scrolling state. Triggered by all animated scroll actions. This includes: Inertial scrolling that occurs
     * after a fling;
     *
     * Bounce-back scrolling when the swipe reaches the edge; Inertial scrolling after quickly dragging the built-in
     * scrollbar and releasing;
     *
     * Scrolling controlled by the animated methods provided by the scroller.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Fling
}
/**
 * Sets the alignment mode of child components in the cross-axis direction of the list.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form
 * @atomicservice [since 11]
 * @since 9
 */
declare enum ListItemAlign {
    /**
     * The list items are packed toward the start edge of the **List** component along the cross axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    Start,
    /**
     * The list items are centered in the **List** component along the cross axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    Center,
    /**
     * The list items are packed toward the end edge of the **List** component along the cross axis.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    End
}
/**
 * Enumerates the areas of **ListItemGroup**.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare enum ListItemGroupArea {
    /**
     * Area other than the **ListItem**, header, and footer areas in **ListItemGroup**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    NONE = 0,
    /**
     * **ListItem** area in **ListItemGroup**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    IN_LIST_ITEM_AREA = 1,
    /**
     * Header area in **ListItemGroup**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    IN_HEADER_AREA = 2,
    /**
     * Footer area in **ListItemGroup**.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    IN_FOOTER_AREA = 3
}
/**
 * Enumerates the sticky styles.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form
 * @atomicservice [since 11]
 * @since 9
 */
declare enum StickyStyle {
    /**
     * In the **ListItemGroup** component, the header is not pinned to the top, and the footer is not pinned to the
     * bottom.
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
     * In the **ListItemGroup** component, the header is pinned to the top, and the footer is not pinned to the bottom.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    Header = 1,
    /**
     * In the **ListItemGroup** component, the footer is pinned to the bottom, and the header is not pinned to the top.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    Footer = 2,
    /**
     * In the **ListItemGroup** component, the header is pinned to the top, and the footer is pinned to the bottom.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    BOTH = 3
}
/**
 * Enumerates the alignment modes of list items when scrolling ends.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 11]
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ScrollSnapAlign {
    /**
     * No alignment. This is the default value.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    NONE = 0,
    /**
     * The first item in the view is aligned at the start of the list.
     *
     * **NOTE**
     *
     * When the list hits the end, the items at the end must be completely displayed. In this case, the items at the start
     * may not be aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    START = 1,
    /**
     * The middle items in the view are aligned in the center of the list.
     *
     * **NOTE**
     *
     * The top and end items can be aligned to the center of the list. In this case, which may cause empty space to be
     * visible in the list display.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    CENTER = 2,
    /**
     * The last item in the view is aligned at the end of the list.
     *
     * **NOTE**
     *
     * When the list hits the start, the items at the start must be completely displayed. In this case, the items at the
     * end may not be aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    END = 3
}
/**
 * Represents the return value of the
 * [getEvent('List')]{@link FrameNode:typeNode.getEvent(node: FrameNode, nodeType: 'List')} method in **frameNode**,
 * which can be used to set scroll events for a **List** node.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 19
 */
declare interface UIListEvent extends UIScrollableCommonEvent {
    /**
     * Sets the callback for the
     * [onWillScroll](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#onwillscroll12) event.
     *
     * If the input parameter is **undefined**, the event callback is reset.
     *
     * @param { OnWillScrollCallback | undefined } callback - Callback for the **onWillScroll** event.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnWillScroll(callback: OnWillScrollCallback | undefined): void;
    /**
     * Sets the callback for the
     * [onDidScroll](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#ondidscroll12) event.
     *
     * If the input parameter is **undefined**, the event callback is reset.
     *
     * @param { OnScrollCallback | undefined } callback - Callback for the **onDidScroll** event.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnDidScroll(callback: OnScrollCallback | undefined): void;
    /**
     * Sets the callback of the
     * [onScrollIndex](docroot://reference/apis-arkui/arkui-ts/ts-container-list.md#onscrollindex) event.
     *
     * If the input parameter is **undefined**, the event callback is reset.
     *
     * @param { OnListScrollIndexCallback | undefined } callback - Callback for the **onScrollIndex** event.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnScrollIndex(callback: OnListScrollIndexCallback | undefined): void;
    /**
     * Sets the callback of the [onScrollVisibleContentChange]{@link ListAttribute#onScrollVisibleContentChange} event.
     *
     * If the input parameter is **undefined**, the event callback is reset.
     *
     * @param { OnScrollVisibleContentChangeCallback | undefined } callback - Callback for the
     *     **onScrollVisibleContentChange** event.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnScrollVisibleContentChange(callback: OnScrollVisibleContentChangeCallback | undefined): void;
}
/**
 * Implements the callbacks and events for the [ListItem]{@link list_item} in the [expanded]{@link SwipeActionState}
 * state.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare interface CloseSwipeActionOptions {
    /**
     * Triggered after the collapse animation is complete.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    onFinish?: () => void;
}
/**
 * Describes the details of the child components in the visible area of a list.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface VisibleListContentInfo {
    /**
     * Index of the list item or list item group in the list display area.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    index: number;
    /**
     * Position of the top or bottom edge of the viewport in the
     * list item group to which the edge is located, if applicable.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    itemGroupArea?: ListItemGroupArea;
    /**
     * Index of the starting or ending list item in the list
     * item group to which the top or bottom edge of the viewport is located.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    itemIndexInGroup?: number;
}
/**
 * Defines the system back button behavior of the **List** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 26.0.0
 */
declare interface ListBackPressBehavior {
    /**
     * Whether to close the swipe menu when back key is pressed.
     *
     * @default true
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    closeSwipeAction?: boolean;
}
/**
 * Triggered when a child component enters or leaves the list display area.
 *
 * When the **List** component changes from having child components to being empty, the values of the reported **start**
 * and **end** parameters remain the same as those when the component had child components last time.
 *
 * If the values of **start** and **end** are both **0**, the **List** component contains only one child component.
 *
 * > **NOTE**
 * >
 * > This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 14.
 *
 * @param {VisibleListContentInfo} start - 1. Index of the first child component in the list display area.<br>2. If the
 *     first child component in the list display area is **ListItemGroup**, you can obtain the area where the first
 *     child component belongs.<br>3. If the first child component in the list display area is **ListItem** in
 *     **ListItemGroup**, you can obtain the index of **ListItem** in **ListItemGroup**.
 * @param {VisibleListContentInfo} end - 1. Index of the last child component in the list display area.<br>2. If the
 *     last child component in the list display area is **ListItemGroup**, you can obtain the area where the last child
 *     component belongs.<br>3. If the last child component in the list display area is **ListItem** in
 *     **ListItemGroup**, you can obtain the index of **ListItem** in **ListItemGroup**.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type OnScrollVisibleContentChangeCallback = (start: VisibleListContentInfo, end: VisibleListContentInfo) => void;
/**
 * Represents a callback for item changes in the visible area of the **List** component.
 *
 * @param {number} start - Index of the first child component in the list display area.
 * @param {number} end - Index of the last child component in the list display area.
 * @param {number} center - Index of the center child component in the list display area.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 19
 */
declare type OnListScrollIndexCallback = (start: number, end: number, center: number) => void;
/**
 * Implements the scroll controller of the **List** component. A **List** component is bound to a **ListScroller** on a
 * one-to-one basis.
 *
 * > **NOTE**
 * >
 * > **ListScroller** inherits from [Scroller]{@link Scroller} and has all methods of
 * > [Scroller]{@link Scroller}.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare class ListScroller extends Scroller {
    /**
     * Obtains the size of a [list item]{@link list_item} in a [list item group]{@link list_item_group} and its position
     * relative to the list.
     *
     * > **NOTE**
     * >
     * > - The value of **index** must be the index of a child component visible in the display area.
     *     Otherwise, the value is considered invalid.
     * > - The child component for which **index** is set must be a list item group. Otherwise,
     *     the **index** value is considered invalid.
     * > - The value of **indexInGroup** must be the index of a list item in the list item group visible
     *     in the display area. Otherwise, the value is considered invalid.
     * > - When **index** or **indexInGroup** is set to an invalid value, the returned size and position are both **0**.
     *
     * @param { number } index - Index of the list item group in the list.
     * @param { number } indexInGroup - Index of the list item in the list item group.
     * @returns { RectResult } Size of the list item in the list item group and its position relative to the list.
     *     <br>Unit: vp
     * @throws { BusinessError } 401 - Parameter error. Possible causes:
     *     <br> 1. Mandatory parameters are left unspecified.
     *     <br> 2. Incorrect parameters types.
     *     <br> 3. Parameter verification failed.
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    getItemRectInGroup(index: number, indexInGroup: number): RectResult;
    /**
     * Scrolls to the specified list item in the specified list item group.
     *
     * @param { number } index - Index of the target list item group in the current container.<br>**NOTE**<br>If the value
     *     set is a negative value or greater than the maximum index of the items in the container, the value is deemed
     *     abnormal, and no scrolling will be performed.
     * @param { number } indexInGroup - Index of the target list item in the list item group specified by **index**.<br>
     *     **NOTE**<br>If the value set is a negative value or greater than the maximum index of the items in the list
     *     item group, the value is deemed abnormal, and no scrolling will be performed.
     * @param { boolean } smooth - Whether the scroll animation is enabled. The options are **true** (enabled) and
     *     **false** (disabled).<br>Default value: **false**<br>**NOTE**<br>When **smooth** is set to **true**, all passed
     *     items are loaded and counted in layout calculation. This may result in performance issues if a large number of
     *     items are involved.
     * @param { ScrollAlign } align - How the list item to scroll to is aligned with the container.<br>Default value:
     *     **ScrollAlign.START**
     * @throws { BusinessError } 401 - Parameter error. Possible causes:
     *     <br> 1. Mandatory parameters are left unspecified.
     *     <br> 2. Incorrect parameters types.
     *     <br> 3. Parameter verification failed.
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    scrollToItemInGroup(index: number, indexInGroup: number, smooth?: boolean, align?: ScrollAlign): void;
    /**
     * Collapses the [list items]{@link list_item} in the [EXPANDED]{@link SwipeActionState} state and sets callback
     * events.
     *
     * @param { CloseSwipeActionOptions } options - Callback events for collapsing [list items]{@link list_item} in the
     *     [EXPANDED]{@link SwipeActionState} state.
     * @throws { BusinessError } 401 - Parameter error. Possible causes:
     *     <br> 1. Mandatory parameters are left unspecified.
     *     <br> 2. Incorrect parameters types.
     *     <br> 3. Parameter verification failed.
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    closeAllSwipeActions(options?: CloseSwipeActionOptions): void;
    /**
     * Obtains the index information of the child component at the specified coordinates.
     *
     * @param { number } x - X-coordinate, in vp.
     * @param { number } y - Y-coordinate, in vp.
     * @returns { VisibleListContentInfo } Index information of a child component at the specified coordinates.
     * @throws { BusinessError } 401 - Parameter error. Possible causes:
     *     <br> 1. Mandatory parameters are left unspecified.
     *     <br> 2. Incorrect parameters types.
     *     <br> 3. Parameter verification failed.
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 14
     */
    getVisibleListContentInfo(x: number, y: number): VisibleListContentInfo;
}
/**
 * Defines the options of the **List** component.
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
interface ListOptions {
    /**
     * Index of the item to be displayed at the start when the list is initially loaded.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>If the set value is a negative number or is greater than the index of the last item in the list,
     * the value is invalid. In this case, the default value will be used.
     * </p>
     *
     * @default 0 [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    initialIndex?: number;
    /**
     * Spacing between list items along the main axis.
     * <br>Default value: **0**
     * <br>If the parameter type is number, the unit is vp.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>If this parameter is set to a negative number or a value greater than or equal to the length of the list
     * content area, the default value is used.
     * <br>If this parameter is set to a value less than the width of the list divider, the width of the list divider
     * is used as the spacing.
     * <br> Child components of <em>List</em> whose <em>visibility</em> attribute is set to <em>None</em> are not
     * displayed, but the spacing above and below them still takes effect.
     * </p>
     *
     * @default 0 [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    space?: number | string;
    /**
     * Spacing between list items along the main axis.
     *
     * <p><strong>NOTE</strong>
     * <br>If this parameter is set to a negative number or a value greater than or equal to the length of the list
     * content area, the default value is used.
     * <br>If this parameter is set to a value less than the width of the list divider, the width of the list divider
     * is used as the spacing.
     * <br> Child components of <em>ListItemGroup</em> whose <em>visibility</em> attribute is set to <em>None</em>
     * are not displayed, but the spacing above and below them still takes effect.
     * <br> If both spaceWidth and space are set, spaceWidth will take precedence.
     * </p>
     *
     * @type { ?Dimension }
     * @default 0
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 26.0.0
     */
    spaceWidth?: Dimension;
    /**
     * Scroller, which can be bound to scrollable components.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>The scroller cannot be bound to other scrollable components.
     * </p>
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    scroller?: Scroller;
}
/**
 * Enumerates the speeds of the snap animation for list scrolling.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 22
 */
declare enum ScrollSnapAnimationSpeed {
    /**
     * Default snap animation speed for the list, typically used when list items are large and scrolling moves one item
     * per swipe.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    NORMAL = 0,
    /**
     * Slower snap animation speed, typically used when list items are small and scrolling moves multiple items per swipe.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    SLOW = 1
}
/**
 * The **List** component provides a list container that presents a series of list items arranged in a column with the
 * same width. It supports presentations of the same type of data in a multiple and coherent row style, for example,
 * images or text.
 *
 * Lazy loading of **List** loads the child components in the visible area as required. Compared with full loading, lazy
 * loading can improve the app startup speed and reduce the memory usage. The lazy loading capabilities vary when the
 * **List** component is used together with
 * [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), or
 * [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md).
 *
 * - When **List** is used together with **ForEach**, all child nodes are created at a time. The nodes within the screen
 * range are laid out and rendered when needed. When a user swipes, the nodes that are out of the screen range are not
 * removed from the tree, and the nodes that are within the screen range are laid out and rendered.
 * - When **List** is used together with **LazyForEach**, all nodes within the screen range are created, laid out, and
 * rendered at a time. When a user swipes, the nodes that are out of the screen range are removed from the tree, and the
 * nodes that are within the screen range are created, laid out, and rendered.
 * - When the **List** component is used together with **Repeat** with
 * [virtualScroll]{@link RepeatAttribute#virtualScroll}, the lazy loading behavior is the same as that of
 * **LazyForEach**. When the **List** component is used together with **Repeat** without **virtualScroll**, the lazy
 * loading behavior is the same as that of **ForEach**.
 *
 * If a scrollable component is nested in a **List** component, their scrolling directions are the same, and the main
 * axis size is not set for the **List** component, the **List** component loads all child components. As a result, lazy
 * loading does not take effect. In this scenario, you are advised to use the [ListItemGroup]{@link list_item_group}
 * component to optimize the performance.
 *
 * Preloading in **List** refers to loading not only the visible child components within the display area but also some
 * invisible child components outside the display area during idle time. Preloading can reduce frame loss during
 * scrolling and improve smoothness. Preloading takes effect only when lazy loading is used. You can set the number of
 * components to be preloaded for the **List** component using
 * [cachedCount]{@link ListAttribute#cachedCount(value: number)}. By default, child components equivalent to one screen
 * above and below the visible area are preloaded (up to a maximum of 16 rows). The preloading capabilities vary when
 * the **List** component is used together with
 * [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), or
 * [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md).
 *
 * - When the **List** component is used together with **ForEach** and **cachedCount** is set, in addition to laying out
 * child components within the visible area, child components within the range of **cachedCount** outside the visible
 * area are pre-laid out during idle time.
 * - When the **List** component is used together with **LazyForEach** and **cachedCount** is set, in addition to
 * creating and laying out child components within the display area, child components within the range of
 * **cachedCount** outside the display area are pre-created and pre-laid out during idle time.
 * - When the **List** component is used together with **Repeat** with
 * [virtualScroll]{@link RepeatAttribute#virtualScroll}, the preloading behavior is the same as that of **LazyForEach**.
 * When the **List** component is used together with **Repeat** without **virtualScroll**, the preloading behavior is
 * the same as that of **ForEach**.
 *
 * > **NOTE**
 *
 * > The component has been bound with gestures to implement functions such as follow-up scrolling. If you need to add
 * > custom gestures, refer to [Gesture Blocking Enhancement]{@link common}.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
interface ListInterface {
    /**
     * Creates a list container.
     *
     * @param { object } value [since 7 - 17]
     * @param { ListOptions } [options] - Options of the **List** component. [since 18]
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (options?: ListOptions): ListAttribute;
}
/**
 * Defines the divider style of the list or list item group.
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
declare interface ListDividerOptions {
    /**
     * Width of the divider.
     * <br>Unit: vp
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>If this parameter is set to a negative number, a percentage, or a value greater than or equal to the length
     * of the list content area, the value <strong>0</strong> will be used.
     * </p>
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    strokeWidth: Length;
    /**
     * Color of the divider.
     * Anonymous Object Rectification.
     *
     * <p><strong>Default value</strong>: 0x08000000
     * </p>
     *
     * @default 0x08000000 [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    color?: ResourceColor;
    /**
     * Distance between the divider and the start edge of the list.
     * Anonymous Object Rectification.
     *
     * <p><strong>Default value</strong>: **0**<br>Unit: vp
     * <br><strong>NOTE</strong>
     * <br>If this parameter is set to a negative number or a percentage, the default value will be used.
     * <br>If <strong>endMargin</strong> and <strong>startMargin</strong> add up to a value that exceeds the column
     * width, they will be set to <strong>0</strong>.
     * </p>
     *
     * @default 0vp [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    startMargin?: Length;
    /**
     * Distance between the divider and the end edge of the list.
     * Anonymous Object Rectification.
     *
     * <p><strong>Default value</strong>: **0**<br>Unit: vp
     * <br><strong>NOTE</strong>
     * <br>If this parameter is set to a negative number or a percentage, the default value will be used.
     * <br>If <strong>endMargin</strong> and <strong>startMargin</strong> add up to a value that exceeds the column
     * width, they will be set to <strong>0</strong>.
     * </p>
     *
     * @default 0vp [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    endMargin?: Length;
}
/**
 * In addition to [universal attributes]{@link common} and
 * [scrollable component common attributes](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#attributes)
 * , the following attributes are also supported.
 *
 * In addition to [universal events]{@link common} and
 * [scrollable component common events](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#events)
 * , the following events are also supported.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare class ListAttribute extends ScrollableCommonMethod<ListAttribute> {
    /**
     * Sets the number of columns or rows in the **List** component. (When the **List** is scrolled vertically, the number
     * of columns is displayed. When the **List** is scrolled horizontally, the number of rows is displayed.)
     *
     * The following example describes how to set the number of columns:
     *
     * - If **value** is a number, the number of columns is specified based on the number.
     * - If **value** is of the **LengthConstrain** type, **minLength** in **LengthConstrain** indicates the minimum
     * column width. The **List** component calculates the maximum number of columns based on its minimum column width. In
     * addition, **LengthConstrain** is passed to the child components of the **List** component as the maximum and
     * minimum layout width constraints. These constraints take effect when the child components do not have a specified
     * width.
     * - Each list item group occupies one row in multi-column mode. Its child list items are arranged based on the
     * **lanes** attribute of the list.
     * - If **value** is of the **LengthConstrain** type, the number of columns in **ListItemGroup** is calculated based
     * on the width of **ListItemGroup**. Therefore, when the width of **ListItemGroup** is different from that of the
     * **List** component, the number of columns in **ListItemGroup** may be different from that in the **List**
     * component.
     *
     * @param { number | LengthConstrain } value - Number of columns or rows in the list.<br>Default value: **1**<br>Value
     *     range: [1, +∞)
     * @param { Dimension } gutter - Column gap or row gap.<br>Default value: **0**
     *     <br>Value range: [0, +∞)
     *     <br>**NOTE**<br>This parameter takes effect when the number of columns or rows is greater than 1. [since 10]
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    lanes(value: number | LengthConstrain, gutter?: Dimension): ListAttribute;
    /**
     * Sets the number of columns and the column spacing of the **List** component. By default, the **List** component is
     * displayed in one column.
     *
     * @param { number | LengthConstrain | ItemFillPolicy } value - Number of columns in the layout of the **List**
     *     component.<br> If this parameter is set to a number, the number of columns is determined by this value. The
     *     value range of the number type is
     *     [1, +∞).<br>If this parameter is set to a value of the **LengthConstrain** type,
     *     the number of columns is determined based on the maximum and minimum values specified in **LengthConstrain**.
     *     <br>If this parameter is set to a value of the **ItemFillPolicy** type,
     *     the number of columns is determined
     *     based on the [breakpoint type](docroot://ui/arkts-layout-development-grid-layout.md#breakpoints)
     *     corresponding to the width of the **List** component.
     *     This type takes effect only when the scrolling direction of the list is vertical.
     * @param { Dimension } [gutter] - Gap between columns.<br>Default value: **0**<br>Value range: [0, +∞)
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    lanes(value: number | LengthConstrain | ItemFillPolicy, gutter?: Dimension): ListAttribute;
    /**
     * Sets the layout mode of list items along the cross axis when the cross-axis width of the list is greater than the
     * value calculated by the following formula: cross-axis width of list items × lanes + (lanes – 1) × gutter.
     *
     * @param { ListItemAlign } value - Alignment mode of list items along the cross axis.<br>Default value:
     *     **ListItemAlign.Start**
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    alignListItem(value: ListItemAlign): ListAttribute;
    /**
     * Sets the direction in which the list items are arranged.
     *
     * @param { Axis } value - Direction in which the list items are arranged.<br>Default value: **Axis.Vertical**
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    listDirection(value: Axis): ListAttribute;
    /**
     * Sets the scrollbar state.
     *
     * @param { BarState } value - Scrollbar state.<br>In API version 9 and earlier versions, the default value is
     *     **BarState.Off**. Since API version 10, the default value is **BarState.Auto**.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollBar(value: BarState): ListAttribute;
    /**
     * Sets the effect used when the scroll boundary is reached.
     *
     * > **NOTE**
     * >
     * > By default, this component can produce a bounce effect only when there is more than one screen of content. To
     * > produce a bounce effect when there is less than one screen of content, set the **options** parameter of the
     * > **edgeEffect** attribute to **{ alwaysEnabled: true }**.
     *
     * @param { EdgeEffect } value - Effect used when the scroll boundary is reached. The spring and shadow effects are
     *     supported.<br>Default value: **EdgeEffect.Spring**
     * @param { EdgeEffectOptions } options - Whether to enable the scroll effect when the component content is smaller
     *     than the component itself. The value **{ alwaysEnabled: true }** means to enable the scroll effect, and
     *     **{ alwaysEnabled: false }** means the opposite.<br>Default value: **{ alwaysEnabled: false }** [since 11]
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    edgeEffect(value: EdgeEffect, options?: EdgeEffectOptions): ListAttribute;
    /**
     * Sets the offset from the start of the list content to the boundary of the list display area.
     *
     * If the sum of **contentStartOffset** and **contentEndOffset** exceeds the length of the list content area, both
     * offsets are reset to **0**.
     *
     * @param { number } value - Offset from the start of the list content to the boundary of the list display area.<br>
     *     Default value: **0**<br>Unit: vp<br>**NOTE**<br>If the set value is a negative number, the default value will
     *     be used.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    contentStartOffset(value: number): ListAttribute;
    /**
     * Sets the offset from the start of the list content to the boundary of the list display area. Compared with
     * [contentStartOffset<sup>11+</sup>]{@link ListAttribute#contentStartOffset(value: number)}, the parameter name is
     * changed to **offset** and the Resource type is supported.
     *
     * If the sum of **contentStartOffset** and **contentEndOffset** exceeds the length of the list content area, both
     * offsets are reset to **0**.
     *
     * @param { number | Resource } offset - Offset from the start of the list content to the boundary of the list display
     *     area.<br>Default value: **0**<br>If the parameter type is number, the unit is vp.<br>Invalid values (negative
     *     numbers or non-numeric Resource values) are treated as the default value.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    contentStartOffset(offset: number | Resource): ListAttribute;
    /**
     * Sets the offset from the end of the list content to the boundary of the list display area.
     *
     * If the sum of **contentStartOffset** and **contentEndOffset** exceeds the length of the list content area, both
     * offsets are reset to **0**.
     *
     * @param { number } value - Offset from the end of the list content to the boundary of the list display area.<br>
     *     Default value: **0**<br>Unit: vp<br>**NOTE**<br>If the set value is a negative number, the default value will
     *     be used.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    contentEndOffset(value: number): ListAttribute;
    /**
     * Sets the offset from the end of the list content to the boundary of the list display area. Compared with
     * [contentEndOffset<sup>11+</sup>]{@link ListAttribute#contentEndOffset(value: number)}, the parameter name is
     * changed to **offset** and the Resource type is supported.
     *
     * If the sum of **contentStartOffset** and **contentEndOffset** exceeds the length of the list content area, both
     * offsets are reset to **0**.
     *
     * @param { number | Resource } offset - Offset from the end of the list content to the boundary of the list display
     *     area.<br>Default value: **0**<br>If the parameter type is number, the unit is vp.<br>Invalid values (negative
     *     numbers or non-numeric Resource values) are treated as the default value.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    contentEndOffset(offset: number | Resource): ListAttribute;
    /**
     * Sets the style of the divider for the list items. By default, there is no divider.
     *
     * The divider is drawn between list items along the main axis, and not above the first list item and below the last
     * list item.
     *
     * In multi-column mode, the value of **startMargin** is calculated from the start edge of the cross axis of each
     * column. In single-column mode, it is calculated from the start edge of the cross axis of the list.
     *
     * When a list item has [polymorphic styles]{@link common} applied, the dividers above and below the pressed child
     * component are not rendered.
     *
     * @param { object | null } value - Style of the divider for the list items.<br>Default value: **null** [since 7 - 8]
     * @param { {strokeWidth: Length;color?: ResourceColor;startMargin?: Length;endMargin?: Length;} | null }
     *     value - Style of the divider for the list items.<br>Default value: **null** [since 9 - 17]
     * @param { ListDividerOptions | null } value - Style of the divider for the list items.<br>Default value:
     *     **null** [since 18]
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    divider(value: ListDividerOptions | null): ListAttribute;
    /**
     * Sets whether to enable edit mode. For details about how to delete selected list items, see
     * [Example 3](docroot://reference/apis-arkui/arkui-ts/ts-container-list.md#example-3-setting-the-edit-mode).
     *
     * > **NOTE**
     * >
     * > This API is supported since API version 7 and deprecated since API version 9. No substitute is provided.
     *
     * @param { boolean } value - Whether to enable edit mode.<br>Default value: **false** (the edit mode is disabled).
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     * @deprecated since 9
     */
    editMode(value: boolean): ListAttribute;
    /**
     * Sets whether to enable multiselect.
     *
     * @param { boolean } value - Whether to enable multiselect.<br>**false** (default): Multiselect is disabled. **true**
     *     : Multiselect is enabled.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    multiSelectable(value: boolean): ListAttribute;
    /**
     * Sets the number of **ListItem** or **ListItemGroup** components to be preloaded (cached). In a lazy loading
     * scenario, only the **cachedCount** rows of **ListItem** components above and below the visible area of the **List**
     * component is preloaded. In a non-lazy loading scenario, all items are loaded at once. For both lazy and non-lazy
     * loading, only the content within the list display area plus the content equivalent to **cachedCount** outside the
     * display area is laid out. <!--Del-->For details, see
     * [Minimizing White Blocks During Swiping](docroot://performance/arkts-performance-improvement-recommendation.md#minimizing-white-blocks-during-swiping)
     * .<!--DelEnd-->
     *
     * When **cachedCount** is set for the list, the system preloads and lays out the **cachedCount**-specified number of
     * rows of list items both above and below the currently visible area of the list. When calculating the number of rows
     * for list items, the system takes into account the number of rows from the list items within a list item group. If a
     * list item group does not contain any list items, then the entire list item group is counted as one row.
     *
     * When a list is nested with **LazyForEach**, and within **LazyForEach** there is a list item group, **LazyForEach**
     * will create **cachedCount**-specified number of list item groups both above and below the currently visible area of
     * the list.
     *
     * @param { number } value - Number of list items or list item groups to be preloaded (cached).<br>Default value:
     *     number of nodes visible on the screen, with the maximum value of 16<br>Value range:
     *     [0, +∞).<br>Values less than 0 are treated as **1**.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    cachedCount(value: number): ListAttribute;
    /**
     * Sets the number of list items or list item groups to be cached (preloaded) and specifies whether to display the
     * preloaded nodes.
     *
     * When **cachedCount** is set for the list, the system preloads and lays out the **cachedCount**-specified number of
     * rows of list items both above and below the currently visible area of the list. When calculating the number of rows
     * for list items, the system takes into account the number of rows from the list items within a list item group. If a
     * list item group does not contain any list items, then the entire list item group is counted as one row. This
     * attribute can be combined with the [clip]{@link CommonMethod#clip(value: boolean)} or
     * [clipContent](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#clipcontent14) attributes
     * to display the preloaded nodes.
     *
     * > **NOTE**
     * >
     * > You are advised to set cachedCount to n/2 (n indicates the number of list items displayed on one screen). You
     * > also need to consider other factors to balance the experience and memory usage. For best practices, see
     * > [Cache List Items](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-best-practices-long-list#section11667144010222).
     *
     * @param { number } count - Number of list items to be preloaded.<br>Default value: number of nodes visible on the
     *     screen, with the maximum value of 16<br>Value range: [0, +∞).<br>Values less than 0 are treated as **1**.
     * @param { boolean } show - Whether to display the preloaded list items. If this parameter is set to **true**, the
     *     preloaded list items are displayed. If this parameter is set to **false**, the preloaded list items are not
     *     displayed.<br> Default value: **false**
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 14
     */
    cachedCount(count: number, show: boolean): ListAttribute;
    /**
     * Sets the number of list items or list item groups to be cached (preloaded) and specifies whether to display the
     * preloaded nodes.
     *
     * If the first parameter of the **cachedCount** attribute is of the **number** type, a specified number (specified by
     * **count**) of rows of list items will be preloaded and laid out above and below the visible area during idle
     * frames.
     *
     * If the first parameter of the **cachedCount** attribute is of the **CacheCountInfo** type, preloading and layout
     * will occur during idle frames when the number of cached rows is less than **CacheCountInfo.minCount**. When the
     * number of cached rows is greater than **CacheCountInfo.maxCount**, the nodes outside the specified range will be
     * destroyed or reused. When the UI is idle (no animation or user operation), a specified number (specified by
     * **CacheCountInfo.maxCount**) of rows of list items will be preloaded above and below the visible area.
     *
     * When calculating the number of rows for list items, the system takes into account the number of rows from the list
     * items within a list item group. If a list item group does not contain any list items, then the entire list item
     * group is counted as one row. This attribute can be combined with the
     * [clip]{@link CommonMethod#clip(value: boolean)} or
     * [clipContent](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#clipcontent14) attributes
     * to display the preloaded nodes.
     *
     * Default behavior: The **count** parameter is of the **number** type by default, with its value set based on the
     * number of nodes displayed on the screen, up to a maximum of 16. Preloaded **ListItem** components are not involved
     * in drawing by default.
     *
     * > **NOTE**
     * >
     * > You are advised to set cachedCount to n/2 (n indicates the number of list items displayed on one screen). You
     * > also need to consider other factors to balance the experience and memory usage. Starting from API version 22,
     * > setting both minimum and maximum cache counts is supported. The maximum cache count can be set to a moderately
     * > higher value, such as twice the minimum cache count, to utilize the UI thread's idle time for node creation. This
     * > reduces the need to create nodes during scrolling for preloading and enhances scrolling smoothness. For best
     * > practices, see
     * > [Cache List Items](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-best-practices-long-list#section11667144010222).
     *
     * @param { number | CacheCountInfo } count - Number of preloaded **ListItem** components if the parameter is of the
     *     **number** type.<br>Value range:
     *     [0, +∞).<br>Values less than 0 are treated as **1**.
     *     <br>If the parameter type is CacheCountInfo, the parameter indicates the maximum and minimum preloading range.
     * @param { boolean } show - Whether to display the preloaded list items.<br>**true**: yes<br>**false**: no
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    cachedCount(count: number | CacheCountInfo, show: boolean): ListAttribute;
    /**
     * Sets whether to enable the chain linkage effect for the current **List** component.
     *
     * > **NOTE**
     * >
     * > - The chain linkage effect refers to the interaction where, during finger swiping, the dragged **ListItem** acts
     * > as the driving object, while adjacent items are driven objects. The driving object drives the linkage of the
     * > driven objects, following a physics-based spring animation.
     * >
     * > - The driving effect of the chain linkage effect is reflected in the spacing between **ListItem**s. The spacing
     * > in the static state can be set by using the **space** parameter of the **List** component. If the **space**
     * > parameter is not set and the chain linkage effect is enabled, the spacing is 20 vp by default.
     * >
     * > - After the chain linkage effect is enabled, the divider of the **List** component is not displayed.
     * >
     * > - The chain linkage effect takes effect only when the **List** component is in single-column mode and the edge
     * > effect is of the **EdgeEffect.Spring** type.
     *
     * @param { boolean } value - Whether to enable chained animations.<br>**false** (default): Chained animations are
     *     disabled. **true**: Chained animations are enabled.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    chainAnimation(value: boolean): ListAttribute;
    /**
     * Sets whether to pin the header to the top or the footer to the bottom in the
     * [list item group]{@link list_item_group}, if set. To support both the pin-to-top and pin-to-bottom features, set
     * **sticky** to **StickyStyle.Header \| StickyStyle.Footer**. From API version 20, the **sticky** attribute can also
     * be set to **StickyStyle.BOTH** to enable both sticky header and sticky footer at the same time.
     *
     * > **NOTE**
     * >
     * > Occasionally, after **sticky** is set, floating-point calculation precision may result in small gaps appearing
     * > during scrolling. To address this issue, you can apply the [pixelRound]{@link CommonMethod#pixelRound} attribute
     * > to the current component, which rounds down the pixel values and help eliminate the gaps.
     *
     * @param { StickyStyle } value - Whether to pin the header to the top or the footer to the bottom in the list item
     *     group.<br>Default value: **StickyStyle.None**
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    sticky(value: StickyStyle): ListAttribute;
    /**
     * Sets the scroll snap alignment effect for list items when scrolling ends.
     *
     * This API is available only when the heights of list items are the same. During the alignment animation, the scroll
     * operation source type reported by the
     * [onWillScroll](docroot://reference/apis-arkui/arkui-ts/ts-container-scrollable-common.md#onwillscroll12) event is
     * **ScrollSource.FLING**.
     *
     * @param { ScrollSnapAlign } value - Alignment mode of the scroll snap position.<br>Default value:
     *     **ScrollSnapAlign.NONE**
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    scrollSnapAlign(value: ScrollSnapAlign): ListAttribute;
    /**
     * Sets the nested scrolling mode in the forward and backward directions to implement scrolling linkage with the
     * parent component.
     *
     * @param { NestedScrollOptions } value - Nested scrolling options.<br>Default value:
     *     **{ scrollForward: NestedScrollMode.SELF_ONLY, scrollBackward: NestedScrollMode.SELF_ONLY }**
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    nestedScroll(value: NestedScrollOptions): ListAttribute;
    /**
     * Sets whether to support the scroll gesture.
     *
     * @param { boolean } value - Whether to support the scroll gesture. With the value **true**, scrolling via finger or
     *     mouse is enabled. With the value **false**, scrolling via finger or mouse is disabled, but this does not affect
     *     the scrolling APIs of the [Scroller]{@link Scroller}.<br>Default value: **true**
     * @returns { ListAttribute } The attribute of the list
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    enableScrollInteraction(value: boolean): ListAttribute;
    /**
     * Sets the friction coefficient. It applies only to gestures in the scrolling area, and it affects only the inertial
     * scrolling process. A value less than or equal to 0 evaluates to the default value.
     *
     * @param { number | Resource } value - Friction coefficient.<br>Default value: **0.6** for non-wearable devices and
     *     **0.9** for wearable devices.<br>Since API version 11, the default value for non-wearable devices is **0.7**.<
     *     br>Since API version 12, the default value for non-wearable devices is **0.75**.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    friction(value: number | Resource): ListAttribute;
    /**
     * Sets the size information of the child components of a **List** component along the main axis.
     *
     * > **NOTE**
     * >
     * > - This attribute provides the **List** component with the size of all child components in the main-axis
     * > direction. This ensures that the **List** component can maintain the accuracy of the scrolling position in
     * > scenarios such as varying main-axis sizes among child components, adding or removing child components, or using
     * > [scrollToIndex]{@link Scroller#scrollToIndex}. In this way, [scrollTo]{@link Scroller#scrollTo} can accurately
     * > jump to the specified position, [currentOffset]{@link Scroller#currentOffset} can obtain the accurate scroll
     * > position, and the built-in scroll bar can be smoothly moved without jumps.
     * >
     * > - If a child component is **ListItemGroup**, the overall size of **ListItemGroup** in the main-axis direction
     * > needs to be accurately calculated based on the column count of **ListItemGroup**, the spacing between list items
     * > in **ListItemGroup** in the main-axis direction, and the size of the header, footer, and **ListItem** components
     * > in **ListItemGroup**. This calculated size must then be passed to the **List** component.
     * >
     * > - If a child component contains **ListItemGroup** components, the
     * > [childrenMainSize]{@link ListItemGroupAttribute#childrenMainSize} attribute must be set for each
     * > **ListItemGroup** component. The **List** component and each **ListItemGroup** component must be bound to a
     * > **ChildrenMainSize** object through the **childrenMainSize** attribute in one-to-one mode.
     * >
     * > - For a multi-column list where child components are generated using **LazyForEach**, ensure that **LazyForEach**
     * > generates either all **ListItemGroup** components or all **ListItem** components.
     *
     * @param { ChildrenMainSize } value - Size information of child components in the main axis direction.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    childrenMainSize(value: ChildrenMainSize): ListAttribute;
    /**
     * Sets whether to maintain the visible content's position when data is inserted or deleted outside the display area
     * of the component.
     *
     * @param { boolean } enabled - Whether to maintain the visible content's position when data is inserted or deleted
     *     outside the visible area of the component.<br>Default value: **false**<br>**false**: The visible content
     *     position will change when data is inserted or deleted. **true**: The visible content position remains unchanged
     *     when data is inserted or deleted.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    maintainVisibleContentPosition(enabled: boolean): ListAttribute;
    /**
     * Whether the list's layout starts from the bottom (end) rather than the top (beginning).
     *
     * @param { boolean } enabled - Whether the list's layout starts from the bottom (end) rather than the top (beginning)
     *     .<br>**false** (default): The layout starts from the top. **true**: The layout starts from the bottom.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    stackFromEnd(enabled: boolean): ListAttribute;
    /**
     * Sets the focus wrap mode for arrow keys.
     *
     * @param { Optional<FocusWrapMode> } mode - Focus wrap mode for cross-axis arrow keys.<br>Default value:
     *     **FocusWrapMode.DEFAULT**<br>**NOTE**<br>Abnormal values are treated as the default value, meaning that cross-
     *     axis arrow keys cannot wrap.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    focusWrapMode(mode: Optional<FocusWrapMode>): ListAttribute;
    /**
     * Sets whether to synchronously load all child components in the list.
     *
     * @param { boolean } enable - Whether to synchronously load all child components in the list.<br>**true**: yes;
     *     **false**: no Default value: **true**<br>**NOTE**<br>When this parameter is set to **false**, in the first
     *     display or **scrollToIndex** jumps without animation, if the time consumed by the frame layout exceeds 50 ms,
     *     the child components that have not been laid out in the list are delayed to the next frame for layout.
     * @returns { ListAttribute } The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    syncLoad(enable: boolean): ListAttribute;
    /**
     * Sets the speed of the snap animation for list item scrolling. This parameter takes effect only when the scroll
     * alignment effect is set.
     *
     * @param { ScrollSnapAnimationSpeed } speed - Speed of the snap animation for listing scrolling.<br>Default value:
     *     **ScrollSnapAnimationSpeed.NORMAL**
     * @returns { ListAttribute } The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    scrollSnapAnimationSpeed(speed: ScrollSnapAnimationSpeed): ListAttribute;
    /**
     * Configures the options of the edit mode.
     *
     * @param { EditModeOptions } [options] - Edit mode options.
     * @returns { ListAttribute } - The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    editModeOptions(options?: EditModeOptions): ListAttribute;
    /**
     * Sets whether to enable the edit mode for the **List** component. After the edit mode is enabled, you can swipe to
     * select multiple [ListItem]{@link list_item} components in the **List** component. If this API is not called, the
     * edit mode is not enabled.
     *
     * @param { boolean | undefined } enabled - Whether to enable the edit mode.<br>**true** means to enable the edit mode
     *     and swiping to select multiple items is supported; **false** or **undefined** means to disable the edit mode
     *     and swiping to select multiple items is not supported.
     * @returns { ListAttribute } The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    enableEditMode(enabled: boolean | undefined): ListAttribute;
    /**
     * Triggered when the editing mode status changes.
     *
     * @param { Callback<boolean> | undefined } callback - Callback triggered when editing mode status changes.
     *     <br>Passing undefined will unregister the callback.
     * @returns { ListAttribute } The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    onEditModeChange(callback: Callback<boolean> | undefined): ListAttribute;
    /**
     * Defines whether the **List** component supports the generation of empty branch nodes that do not contain any child
     * components using the **if/else** rendering control syntax in **LazyForEach** or **Repeat**. If this attribute is
     * not set, empty branch nodes are not supported. This attribute cannot be updated after being set. Therefore, you
     * cannot switch between the behavior of supporting empty branches and the behavior of not supporting empty branches
     * after setting this attribute.
     *
     * @param { boolean | undefined } supported - Whether the current **List** component supports the use of the
     *     [if/else](docroot://ui/rendering-control/arkts-rendering-control-ifelse.md) rendering syntax in
     *     [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md) or
     *     [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md) to generate an empty branch node
     *     that contains no child component.<br>**true**: yes; **false**: no<br>If the value is **undefined**, it is
     *     processed as **false**.
     * @returns { ListAttribute } the attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    supportEmptyBranchInLazyLoading(supported: boolean | undefined): ListAttribute;
    /**
     * Sets the system back button behavior of the **List** component.
     *
     * @param { ListBackPressBehavior | undefined } behavior - System back button behavior of the **List** component.
     *     Currently, you can use the [ListBackPressBehavior]{@link ListBackPressBehavior} parameter to configure whether
     *     to collapse the expanded swipe-out component of a **ListItem** when the system back button takes effect.<br>If
     *     this parameter is set to **undefined**, the default behavior is restored. That is, when the system back button
     *     takes effect, the expanded swipe-out component of the **ListItem** is collapsed.
     * @returns { ListAttribute } The attribute of the list.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    backPressBehavior(behavior: ListBackPressBehavior | undefined): ListAttribute;
    /**
     * Triggered when the list scrolls.
     *
     * @param { function } event - Callback when scroll,
     * scrollOffset: Offset relative to the previous frame.
     * The offset is positive when the list content scrolls up and negative when the list content scrolls down.
     * <br>Unit: vp
     * scrollState: Current scroll state.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     * @deprecated since 12
     * @useinstead common.ScrollableCommonMethod#onDidScroll
     */
    onScroll(event: (scrollOffset: number, scrollState: ScrollState) => void): ListAttribute;
    /**
     * Triggered when a child component enters or leaves the list display area.
     *
     * @param { function } event
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    onScrollIndex(event: (start: number, end: number, center: number) => void): ListAttribute;
    /**
     * Triggered when a child component enters or leaves the list display area. During index calculation, the list item,
     * header of the list item group, and footer of the list item group each are counted as a child component.
     *
     * When the list edge scrolling effect is the spring effect, the **onScrollVisibleContentChange** event is not
     * triggered when the user scrolls the list to the edge or releases the list to rebound.
     *
     * This event is triggered once when the list is initialized and when the index of the first child component or the
     * next child component in the list display area changes.
     *
     * @param { OnScrollVisibleContentChangeCallback } handler - Callback invoked when the displayed content changes.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    onScrollVisibleContentChange(handler: OnScrollVisibleContentChangeCallback): ListAttribute;
    /**
     * Triggered when the list reaches the start position.
     *
     * This event is triggered once when **initialIndex** is **0** during list initialization and once when the list
     * scrolls to the start position. When the list edge scrolling effect is the spring effect, this event is triggered
     * once when the list passes the start position and is triggered again when the list returns to the start position.
     *
     * @param { function } event - Callback triggered when the list reaches the start position.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    onReachStart(event: () => void): ListAttribute;
    /**
     * Called when the list reaches the end position. This callback is triggered when the last child component appears in
     * the list view due to scrolling or content/layout changes.
     *
     * If the child component does not fill the list and can be completely displayed in the list without scrolling, this
     * event is triggered during the first loading.
     *
     * When the list edge scrolling effect is the spring effect, this event is triggered once when the list passes the end
     * position and is triggered again when the list returns to the end position.
     *
     * @param { function } event - Callback triggered when the list reaches the end position.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    onReachEnd(event: () => void): ListAttribute;
    /**
     * Triggered when the list starts scrolling initiated by the user's finger dragging the list or its scrollbar. This
     * event is also triggered when the animation contained in the scrolling triggered by [Scroller]{@link Scroller}
     * starts.
     *
     * @param { function } event - Callback invoked when the list starts scrolling.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    onScrollStart(event: () => void): ListAttribute;
    /**
     * Triggered when the list stops scrolling after the user's finger leaves the screen. This event is also triggered
     * when the animation contained in the scrolling triggered by [Scroller]{@link Scroller} stops.
     *
     * @param { function } event - Callback triggered when the list stops sliding.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    onScrollStop(event: () => void): ListAttribute;
    /**
     * Triggered when a list item is deleted.
     *
     * @param { function } event
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     * @deprecated since 9
     */
    onItemDelete(event: (index: number) => boolean): ListAttribute;
    /**
     * Triggered when a list item moves.
     *
     * @param { function } event
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    onItemMove(event: (from: number, to: number) => boolean): ListAttribute;
    /**
     * Triggered when a list item starts to be dragged.
     *
     * Automatic scrolling of the list cannot be triggered when a list item is dragged to the edge of the list. You can
     * use the [onMove](docroot://reference/apis-arkui/arkui-ts/ts-universal-attributes-drag-sorting.md#onmove) API of
     * **ForEach**, **LazyForEach**, or **Repeat** to implement this effect. For details, see
     * [Example 12: Implementing Dragging with OnMove](docroot://reference/apis-arkui/arkui-ts/ts-container-list.md#example-12-implementing-dragging-with-onmove).
     * However, note that the
     * [onMove](docroot://reference/apis-arkui/arkui-ts/ts-universal-attributes-drag-sorting.md#onmove) API does not
     * support cross-**ListItemGroup** dragging.
     *
     * > **NOTE**
     * >
     * > This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 14.
     *
     * @param { function } event - Callback triggered when the dragging of a list item starts.<br> In API version 22 and
     *     earlier versions, the parameter type is **(event: ItemDragInfo, itemIndex: number) => (() => any) | void**. For
     *     details about the **event** and **itemIndex** parameters, see
     *     [OnItemDragStartCallback]{@link OnItemDragStartCallback}. [since 8 - 22]
     * @param { OnItemDragStartCallback } event - Callback triggered when the dragging of a list item starts.<br> In API
     *     version 22 and earlier versions, the parameter type is
     *     **(event: ItemDragInfo, itemIndex: number) => (() => any) | void**. For details about the **event** and
     *     **itemIndex** parameters, see [OnItemDragStartCallback]{@link OnItemDragStartCallback}. [since 23]
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    onItemDragStart(event: OnItemDragStartCallback): ListAttribute;
    /**
     * Called when a dragged list item enters the list.
     *
     * @param { function } event - Information about the drag point.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    onItemDragEnter(event: (event: ItemDragInfo) => void): ListAttribute;
    /**
     * Triggered when the dragged item moves over the drop target of the list.
     *
     * @param { function } event
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    onItemDragMove(event: (event: ItemDragInfo, itemIndex: number, insertIndex: number) => void): ListAttribute;
    /**
     * Triggered when the dragged item leaves the drop target of the list.
     *
     * @param { function } event
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    onItemDragLeave(event: (event: ItemDragInfo, itemIndex: number) => void): ListAttribute;
    /**
     * Triggered when the dragged item is dropped on the drop target of the list.
     * During dragging across lists, **isSuccess** is set to **true** if the drop target is bound to **onItemDrop**.
     * Otherwise, **isSuccess** is set to **false**. During dragging within a list, **isSuccess** is the return value of
     * the **onItemMove** event.
     *
     * @param { function } event - Callback triggered when dragging is stopped within the scope of the list.
     *     <br/>event: Information about the drag point.
     *     <br/>itemIndex: Initial position of the dragged item.
     *     <br/>insertIndex: Index of the position to which the dragged item is dropped. <br/>
     *     isSuccess: Whether the dragged item is successfully dropped. If the return value is **true**, the list item is
     *     successfully dropped. If the return value is **false**, the list item is not successfully dropped.
     * @returns { ListAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 8
     */
    onItemDrop(event: (event: ItemDragInfo, itemIndex: number, insertIndex: number, isSuccess: boolean) => void): ListAttribute;
    /**
     * When this API is called back, the event parameter passes the scroll offset that is about to occur. The event
     * processing function can calculate the actually required scroll offset based on the application scenario and return
     * it as the return value. The list will then scroll according to this returned actual scroll offset.
     *
     * If **listDirection** is set to **Axis.Vertical**, the return value is the amount by which the list needs to scroll
     * in the vertical direction. If **listDirection** is set to **Axis.Horizontal**, the return value is the amount by
     * which the list needs to scroll in the horizontal direction.
     *
     * This event is triggered when either of the following conditions is met:
     *
     * 1. Scrolling is initiated by user interaction (for example, finger swipe, keyboard, or mouse operation).
     * 2. The **List** component scrolls by inertia.
     * 3. Call the [fling]{@link Scroller#fling} API to trigger scrolling.
     *
     * This event is not triggered in the following scenarios:
     *
     * 1. A scroll control API other than [fling]{@link Scroller#fling} is called.
     * 2. The out-of-bounds bounce effect is active.
     * 3. The scrollbar is dragged.
     *
     * @param { function } event - Callback triggered when each frame scrolling starts. [since 9 - 19]
     * @param { OnScrollFrameBeginCallback } event - Callback triggered when each frame scrolling starts. [since 20]
     * @returns { ListAttribute } Returns the instance of the ListAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    onScrollFrameBegin(event: OnScrollFrameBeginCallback): ListAttribute;
}
/**
 * The **List** component provides a list container that presents a series of list items arranged in a column with the
 * same width. It supports presentations of the same type of data in a multiple and coherent row style, for example,
 * images or text.
 *
 * Lazy loading of **List** loads the child components in the visible area as required. Compared with full loading, lazy
 * loading can improve the app startup speed and reduce the memory usage. The lazy loading capabilities vary when the
 * **List** component is used together with
 * [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), or
 * [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md).
 *
 * - When **List** is used together with **ForEach**, all child nodes are created at a time. The nodes within the screen
 * range are laid out and rendered when needed. When a user swipes, the nodes that are out of the screen range are not
 * removed from the tree, and the nodes that are within the screen range are laid out and rendered.
 * - When **List** is used together with **LazyForEach**, all nodes within the screen range are created, laid out, and
 * rendered at a time. When a user swipes, the nodes that are out of the screen range are removed from the tree, and the
 * nodes that are within the screen range are created, laid out, and rendered.
 * - When the **List** component is used together with **Repeat** with
 * [virtualScroll]{@link RepeatAttribute#virtualScroll}, the lazy loading behavior is the same as that of
 * **LazyForEach**. When the **List** component is used together with **Repeat** without **virtualScroll**, the lazy
 * loading behavior is the same as that of **ForEach**.
 *
 * If a scrollable component is nested in a **List** component, their scrolling directions are the same, and the main
 * axis size is not set for the **List** component, the **List** component loads all child components. As a result, lazy
 * loading does not take effect. In this scenario, you are advised to use the [ListItemGroup]{@link list_item_group}
 * component to optimize the performance.
 *
 * Preloading in **List** refers to loading not only the visible child components within the display area but also some
 * invisible child components outside the display area during idle time. Preloading can reduce frame loss during
 * scrolling and improve smoothness. Preloading takes effect only when lazy loading is used. You can set the number of
 * components to be preloaded for the **List** component using
 * [cachedCount]{@link ListAttribute#cachedCount(value: number)}. By default, child components equivalent to one screen
 * above and below the visible area are preloaded (up to a maximum of 16 rows). The preloading capabilities vary when
 * the **List** component is used together with
 * [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), or
 * [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md).
 *
 * - When the **List** component is used together with **ForEach** and **cachedCount** is set, in addition to laying out
 * child components within the visible area, child components within the range of **cachedCount** outside the visible
 * area are pre-laid out during idle time.
 * - When the **List** component is used together with **LazyForEach** and **cachedCount** is set, in addition to
 * creating and laying out child components within the display area, child components within the range of
 * **cachedCount** outside the display area are pre-created and pre-laid out during idle time.
 * - When the **List** component is used together with **Repeat** with
 * [virtualScroll]{@link RepeatAttribute#virtualScroll}, the preloading behavior is the same as that of **LazyForEach**.
 * When the **List** component is used together with **Repeat** without **virtualScroll**, the preloading behavior is
 * the same as that of **ForEach**.
 *
 * > **NOTE**
 *
 * > The component has been bound with gestures to implement functions such as follow-up scrolling. If you need to add
 * > custom gestures, refer to [Gesture Blocking Enhancement]{@link common}.
 *
 * ###### Child Components
 *
 * Only the [ListItem]{@link list_item} and [ListItemGroup]{@link list_item_group} child components and custom
 * components are supported. When using custom components inside **List**, you are advised to wrap the custom component
 * with a **ListItem** or **ListItemGroup** as the top-level container. Setting attributes or event methods directly on
 * custom components is not recommended.
 *
 * Child components can be dynamically generated using rendering control types
 * [if/else](docroot://ui/rendering-control/arkts-rendering-control-ifelse.md),
 * [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), and
 * [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md). **LazyForEach** or **Repeat** is
 * recommended to optimize performance.
 *
 * > **NOTE**
 * >
 * > If performance lag occurs when you process a large number of child components, consider using lazy loading, list
 * > item caching, dynamic preloading, component reuse, and layout optimization. For best practices, see
 * > [Optimizing Frame Loss for Long List Loading](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-best-practices-long-list).
 * >
 * > Starting from API version 21, the maximum width or height for a single child component inside a **List** container
 * > is 16,777,216 px. In API version 20 and earlier versions, the limit was 1,000,000 px. If a child component exceeds
 * > the applicable size limit, scrolling or display behavior may become abnormal.
 * >
 * > Below are the rules for calculating the indexes of the child components of **List**:
 * >
 * > - The index increases in ascending order of child components.
 * >
 * > - In the **if/else** statement, only the child components for which the condition evaluates to true participate in
 * > the index calculation.
 * >
 * > - In the **ForEach**, **LazyForEach**, or **Repeat** statement, the indexes of all expanded subnodes are
 * > calculated.
 * >
 * > - After changes occur in [if/else](docroot://ui/rendering-control/arkts-rendering-control-ifelse.md),
 * > [ForEach](docroot://ui/rendering-control/arkts-rendering-control-foreach.md),
 * > [LazyForEach](docroot://ui/rendering-control/arkts-rendering-control-lazyforeach.md), and
 * > [Repeat](docroot://ui/rendering-control/arkts-new-rendering-control-repeat.md), index values are updated
 * > accordingly for child components.
 * >
 * > - Each **ListItemGroup** component is taken as a whole and assigned an index, and the indexes of the list items
 * > within are not included in the index calculation.
 * >
 * > - Child components of **List** whose **visibility** attribute is set to **Hidden** or **None** are included in the
 * > index calculation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const List: ListInterface;
/**
 * Defines List Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const ListInstance: ListAttribute;
