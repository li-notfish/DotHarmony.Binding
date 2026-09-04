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
 * Enumerates the scrolling directions.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ScrollDirection {
    /**
     * Only vertical scrolling is supported.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Vertical,
    /**
     * Only horizontal scrolling is supported.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    Horizontal,
    /**
     * Vertical or horizontal scrolling is supported.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     * @deprecated since 9
     * @useinstead ScrollDirection#FREE
     */
    Free,
    /**
     * Scrolling is disabled.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    None,
    /**
     * Free scrolling is supported.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    FREE = 4
}
/**
 * Enumerates alignment modes.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 11]
 * @since 10
 */
declare enum ScrollAlign {
    /**
     * The start edge of the list item is flush with the start edge of the list.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    START,
    /**
     * The list item is centered along the main axis of the list.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    CENTER,
    /**
     * The end edge of the list item is flush with the end edge of the list.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    END,
    /**
     * The list item is automatically aligned.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    AUTO
}
/**
 * Represents the offset values resulting from a scroll operation.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 11
 */
declare interface OffsetResult {
    /**
     * Horizontal scrolling offset.
     * <br>The unit of the return value is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    xOffset: number;
    /**
     * Vertical scrolling offset.
     * <br>The unit of the return value is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    yOffset: number;
}
/**
 * Provides parameters for scrolling to the edge of a scrollable container.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface ScrollEdgeOptions {
    /**
     * Fixed velocity for scrolling to the edge of the container.
     * If the value specified is less than or equal to 0, the parameter will not take effect.
     *
     * @default 0vp/s
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    velocity?: number;
}
/**
 * Provides parameters for scrolling to a specific index.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface ScrollToIndexOptions {
    /**
     * Extra offset for scrolling to a specified index.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    extraOffset?: LengthMetrics;
}
/**
 * Provides parameters for customizing scroll animations.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface ScrollAnimationOptions {
    /**
     * Scrolling duration.
     *
     * <p><strong>NOTE</strong>
     * <br>Scrolling duration.<br>Default value: **1000**<br>Unit: ms
     * <br>**NOTE**<br>A value less than 0 evaluates to the default value.
     * </p>
     *
     * @default 1000
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    duration?: number;
    /**
     * Scrolling curve.
     *
     * @default Curve.Ease
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    curve?: Curve | ICurve;
    /**
     * Whether to enable overscroll.
     *
     * <p><strong>NOTE</strong>
     * <br> Scrolling can exceed the boundary and initiate a bounce animation when this parameter is set to <em>true</em>,
     * and the component's <em>edgeEffect</em> attribute is set to EdgeEffect.Spring.
     * </p>
     *
     * @default false
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    canOverScroll?: boolean;
}
/**
 * Provides parameters for setting the initial scrolling offset.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare interface OffsetOptions {
    /**
     * Horizontal scrolling offset.
     *
     * @default 0
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    xOffset?: Dimension;
    /**
     * Vertical scrolling offset.
     *
     * @default 0
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    yOffset?: Dimension;
}
/**
 * Defines a UIScrollableCommonEvent which is used to set different common event to target component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 19
 */
declare interface UIScrollEvent extends UIScrollableCommonEvent {
    /**
     * Set or reset the callback which is triggered when the Scroll will scroll.
     *
     * @param { ScrollOnWillScrollCallback | undefined } callback - callback function, triggered when
     *     the Scroll will scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnWillScroll(callback: ScrollOnWillScrollCallback | undefined): void;
    /**
     * Set or reset the callback which is triggered when the Scroll did scroll.
     *
     * @param { ScrollOnScrollCallback | undefined } callback - callback function, triggered when the Scroll did scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 19
     */
    setOnDidScroll(callback: ScrollOnScrollCallback | undefined): void;
}
/**
 * Defines a controller for scrollable container components.
 *
 * <p><strong>NOTE</strong>
 * <br>1. The binding of a <em>Scroller</em> instance to a scrollable container component occurs during the component
 * creation phase.
 * <br>2. <em>Scroller</em> APIs can only be effectively called
 * after the <em>Scroller</em> instance is bound to a scrollable container component.
 * Otherwise, depending on the API called, it may have no effect or throw an exception.
 * <br>3. For example, with aboutToAppear, this callback is executed after a new instance of a custom component is
 * created and before its <em>build()</em> method is called.
 * Therefore, if a scrollable component is defined within the <em>build</em> method of a custom component,
 * the internal scrollable component has not yet been created during the <em>aboutToAppear</em> callback
 * of that custom component, and therefore the <em>Scroller</em> APIs cannot be called effectively.
 * </p>
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare class Scroller {
    /**
     * A constructor used to create a <em>Scroller</em> object.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    constructor();
    /**
     * Scrolls to the specified position.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>If the scrolling speed of the <em>scrollTo</em> animation exceeds 200 vp/s, the components within
     * the scrollable area will not respond to click events.
     * </p>
     *
     * @param { object } value [since 7 - 17]
     * @param { ScrollOptions } options - Parameters for scrolling to the specified position. [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollTo(options: ScrollOptions);
    /**
     * Scrolls to the edge of the container, regardless of the scroll axis direction.
     * By default, the <em>Scroll</em> component comes with an animation, while the <em>Grid</em>, <em>List</em>,
     * and <em>WaterFlow</em> components do not.
     *
     * @param { Edge } value - Edge position to scroll to.
     *     <br><em>Atomic service API</em>: This API can be used in atomic services since API version 11.
     * @param { ScrollEdgeOptions } [options] - Mode of scrolling to the edge position.
     *     <br><em>Atomic service API</em>: This API can be used in atomic services since API version 12. [since 12]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollEdge(value: Edge, options?: ScrollEdgeOptions);
    /**
     * Performs inertial scrolling based on the initial velocity passed in.
     *
     * @param { number } velocity - Initial velocity of inertial scrolling. Unit: vp/s
     *     <br><em>NOTE</em>
     *     <br>If the value specified is 0, it is considered as invalid, and the scrolling for this instance will not take
     *     effect.
     *     A positive value indicates scrolling towards the top, while a negative value indicates scrolling towards the
     *     bottom.
     * @throws { BusinessError } 401 - Parameter error. Possible causes:
     *     <br> 1. Mandatory parameters are left unspecified.
     *     <br> 2. Incorrect parameters types.
     *     <br> 3. Parameter verification failed.
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    fling(velocity: number): void;
    /**
     * Scrolls to the next or previous page.
     *
     * @param { object } value [since 9 - 13]
     * @param { ScrollPageOptions } value - Page turning mode. [since 14]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    scrollPage(value: ScrollPageOptions);
    /**
     * Scrolls to the next or previous page.
     *
     * @param { object } value - next: Whether to turn to the next page.
     *     The value <em>true</em> means to scroll to the next page, and <em>false</em> means to scroll to the previous
     *     page.
     *     direction: Scrolling direction: horizontal or vertical.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     * @deprecated since 9
     * @useinstead Scroller#scrollPage
     */
    scrollPage(value: {
        next: boolean;
        direction?: Axis;
    });
    /**
     * Obtains the current scrolling offset.
     *
     * <p><strong>NOTE</strong>
     * <br>1. If <em>Scroller</em> is not bound to a component, this API returns <em>undefined</em>,
     * which is not declared in the API. You are advised to use the <em>offset</em> function.
     * <br>2. The <em>Grid</em>, <em>List</em>, and <em>WaterFlow</em> components use a lazy loading mechanism.
     * Before all content is fully loaded and laid out, the total content offset is estimated, and this estimation
     * may be inaccurate. For the <em>List</em> component, the <em>childrenMainSize</em> attribute can be used
     * to mitigate such inaccuracies. Currently, there is no solution to inaccurate estimation of the
     * <em>Grid</em> and <em>WaterFlow</em> components.
     * </p>
     *
     * @returns { OffsetResult } Returns the current scrolling offset. If the scroller not bound to a component, the
     *     return value is void. [since 11]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    currentOffset(): OffsetResult;
    /**
     * Obtains the current scrolling offset.
     *
     * @returns { OffsetResult | undefined } Returns the current scrolling offset.
     *     If the scroller not bound to a component, the return value is undefined.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    offset(): OffsetResult | undefined;
    /**
     * Scrolls to a specified index, with support for setting an extra offset for the scroll.
     * When smooth scrolling is enabled, all items encountered during the scroll are loaded and their layout
     * is calculated. Loading a large number of items may cause performance issues. It is recommended that you
     * first call <em>scrollToIndex</em> without animation to jump to a position near the target, then call it
     * again with animation to smoothly scroll to the final target position.
     *
     * <p><strong>NOTE</strong>
     * <br>This API only works for the <em>ArcList</em>, <em>Grid</em>, <em>List</em>, and <em>WaterFlow</em> components.
     * <br>When refreshing the data source using <em>LazyForEach</em>, <em>ForEach</em>, or <em>Repeat</em>,
     * ensure this API is called after the data refresh is complete.
     * <br>Starting from API version 11, the <em>List</em> component supports <em>contentStartOffset</em>
     * and <em>contentEndOffset</em>. Starting from API version 22, the <em>Grid</em> and <em>WaterFlow</em>
     * components also support setting <em>contentStartOffset</em> and <em>contentEndOffset</em>.
     * <br>- If the scrollable container has <em>contentStartOffset</em> set and <em>ScrollAlign</em> is
     * <em>START</em>, after scrolling, the start of the specified item will align with the
     * <em>contentStartOffset</em> of the container.
     * <br>- If the scrollable container has <em>contentEndOffset</em> set and <em>ScrollAlign</em> is
     * <em>END</em>, after scrolling, the end of the specified item will align with the
     * <em>contentEndOffset</em> of the container.
     * <br>- If the scrollable container has <em>contentStartOffset</em> or <em>contentEndOffset</em> set
     * and <em>ScrollAlign</em> is <em>AUTO</em>: When the specified item is completely within the visible area,
     * no adjustment is made. Otherwise, following the shortest-scroll-distance principle, the start of the item
     * will align with the container's <em>contentStartOffset</em>, or the end will align with the container's
     * <em>contentEndOffset</em>, ensuring the item is fully displayed.
     * </p>
     *
     * @param { number } value - Index of the item to be scrolled to in the container.
     *     <br><em>NOTE</em>
     *     <br>If the value set is a negative value or greater than the maximum index of the items in the container,
     *     the value is deemed abnormal, and no scrolling will be performed.
     * @param { boolean } smooth - If true, scroll to index item with animation. If false, scroll to index item without
     *     animation. [since 7 - 11]
     * @param { ScrollAlign } align - Sets the alignment mode of a specified index. [since 7 - 11]
     * @param { boolean } [smooth] - Whether to enable the smooth animation for scrolling to the item with the specified
     *     index.
     *     The value <em>true</em> means to enable that the smooth animation, and <em>false</em> means the opposite.<br>
     *     Default value: <em>false</em> [since 12]
     * @param { ScrollAlign } [align] - How the list item to scroll to is aligned with the container.
     *     <br> Default value when the container is <em>List</em>: <em>ScrollAlign.START</em>
     *     <br> Default value when the container is <em>Grid</em>: <em>ScrollAlign.AUTO</em>
     *     <br> Default value when the container is <em>WaterFlow</em>: <em>ScrollAlign.START</em>
     *     <br><em>NOTE</em>
     *     <br>This parameter is only available for the <em>List</em>, <em>Grid</em>, and <em>WaterFlow</em>
     *     components. [since 12]
     * @param { ScrollToIndexOptions } [options] - Options for scrolling to a specified index,
     *     for example, an extra offset for the scroll.<br>Default value: <em>0</em>, in vp [since 12]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollToIndex(value: number, smooth?: boolean, align?: ScrollAlign, options?: ScrollToIndexOptions);
    /**
     * Scrolls by the specified amount.
     *
     * <p><strong>NOTE</strong>
     * <br>This API is available for the <em>ArcList</em>, <em>Scroll</em>, <em>List</em>, <em>Grid</em>,
     * and <em>WaterFlow</em> components.
     * </p>
     *
     * @param { Length } dx - Amount to scroll by in the horizontal direction. The percentage format is not supported.
     * @param { Length } dy - Amount to scroll by in the vertical direction. The percentage format is not supported.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    scrollBy(dx: Length, dy: Length);
    /**
     * Checks whether the component has scrolled to the bottom.
     *
     * <p><strong>NOTE</strong>
     * <br>This API is available for the <em>ArcList</em>, <em>Scroll</em>, <em>List</em>, <em>Grid</em>,
     * and <em>WaterFlow</em> components.
     * </p>
     *
     * @returns { boolean } Returns whether the component scrolls to the end position.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    isAtEnd(): boolean;
    /**
     * Obtains the size and position of a child component relative to its container.
     *
     * <p><strong>NOTE</strong>
     * <br>- The value of <em>index</em> must be the index of a child component visible in the display area.
     * Otherwise, the value is considered invalid.
     * <br>- The value of <em>index</em> must be the index of a child component visible in the display area. Otherwise,
     * the value is considered invalid.
     * </p>
     *
     * @param { number } index - Index of the target child component.
     * @returns { RectResult } Size and position of the child component relative to the component.<br>Unit: vp
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
    getItemRect(index: number): RectResult;
    /**
     * Obtains the index of a child component based on coordinates.
     *
     * <p><strong>NOTE</strong>
     * <br>The returned index is <em>-1</em> for invalid coordinates.
     * </p>
     *
     * @param { number } x - X-coordinate, in vp.
     * @param { number } y - Y-coordinate, in vp.
     * @returns { number } Index of the item.
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
    getItemIndex(x: number, y: number): number;
    /**
     * Obtains the content size.
     *
     * @returns { SizeResult } Total size of the scrollable component's content, including the content width and height.
     *     <br>Unit: vp
     * @throws { BusinessError } 100004 - Controller not bound to a component.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    contentSize(): SizeResult;
    /**
     * Obtains the FrameNode corresponding to this scroller.
     *
     * @returns { FrameNode | undefined } Returns the FrameNode bound to this scroller.
     *     If the scroller is not bound to a component, the return value is undefined.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    getFrameNode(): FrameNode | undefined;
}
/**
 * Provides parameters for scrolling to a specific position in a scrollable container.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare interface ScrollOptions {
    /**
     * Horizontal scrolling offset.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>This parameter cannot be set in percentage.
     * <br>This parameter takes effect only when the scroll axis is the x-axis.
     * <br>Value range: Values less than 0 are treated as 0, and scrolling occurs without animation.
     * Animated scrolling stops at the starting position by default.
     * By setting the **animation** parameter, you can enable a bounce effect when the scrolling
     * goes beyond the boundary.
     * <br>If the parameter type is number, the unit is vp.
     * </p>
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    xOffset: number | string;
    /**
     * Vertical scrolling offset.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>This parameter cannot be set in percentage.
     * <br>This parameter takes effect only when the scroll axis is the y-axis.
     * <br>Value range: Values less than 0 are treated as 0, and scrolling occurs without animation.
     * Animated scrolling stops at the starting position by default.
     * By setting the **animation** parameter, you can enable a bounce effect when the scrolling
     * goes beyond the boundary.<br>If the parameter type is number, the unit is vp.
     * </p>
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    yOffset: number | string;
    /**
     * Animation configuration
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>Currently, the <em>List</em>, <em>Scroll</em>, <em>Grid</em>, and <em>WaterFlow</em>
     * support the <em>Boolean</em> type and <em>ICurve</em>.
     * </p>
     *
     *     parameters
     *     and the boolean type enables default spring animation. [since 10 - 11]
     *     and the boolean type enables default spring animation. [since 12]
     * @default ScrollAnimationOptions: { duration: 1000, curve: Curve.Ease, canOverScroll: false } [since 18]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    animation?: ScrollAnimationOptions | boolean;
    /**
     * Set whether the scroll target position can over the boundary.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    canOverScroll?: boolean;
}
/**
 * Provides parameters for page scrolling behavior.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 14
 */
declare interface ScrollPageOptions {
    /**
     * Whether to turn to the next page.The value true means to scroll to the next page,
     * and false means to scroll to the previous page.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 14
     */
    next: boolean;
    /**
     * Whether to enable the page-turning animation.The value true means to enable the page-turning animation,
     * and false means the opposite.
     *
     * @default false
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 14
     */
    animation?: boolean;
}
/**
 * Defines a scroll snapping mode object.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 11]
 * @atomicservice [since 11]
 * @since 10
 */
declare interface ScrollSnapOptions {
    /**
     * Alignment mode for the scroll snap position.
     *
     * @default ScrollSnapAlign.NONE [since 11]
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    snapAlign: ScrollSnapAlign;
    /**
     * Pagination points for scroll snapping.
     *
     * <p><strong>NOTE</strong>
     * <br>1. If the value is of the Dimension type, it indicates the size of each page,
     * and the system will paginate based on this size.
     * <br>2. If the value is of the Array\<Dimension\> type, each <em>Dimension</em> represents a pagination point,
     * and the system will paginate accordingly.
     * Each <em>Dimension</em> value must be within the [0, scrollable distance] range.
     * <br>3. If this parameter is not set or <em>Dimension</em> is set to a value less than or equal to 0,
     * the value is regarded as an invalid value.
     * In this case, there is no scroll snapping. When the value is of the Array\<Dimension\> type,
     * the items in the array must be monotonically increasing.
     * <br>4. When the value is a percentage,
     * the actual size is the product of the viewport of the <em>Scroll</em> component and the percentage value.
     * </p>
     *
     * @default 100%
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    snapPagination?: Dimension | Array<Dimension>;
    /**
     * Whether to enable the snap to start feature. When scroll snapping is defined for the <em>Scroll</em> component,
     * setting this parameter to <em>false</em> enables the component to scroll between the start and the first page.
     *
     * <p><strong>NOTE</strong>
     * <br>1. Default value: <em>true</em>
     * <br>2. This attribute takes effect only when <em>snapPagination</em>
     * is set to a value of the <em>Array\<Dimension\></em> type;
     * it does not work with values of the <em>Dimension</em> type.
     * </p>
     *
     * @default true
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    enableSnapToStart?: boolean;
    /**
     * Whether to enable the snap to end feature. When scroll snapping is defined for the <em>Scroll</em> component,
     * setting this parameter to <em>false</em> enables the component to scroll between the end and the last page.
     *
     * <p><strong>NOTE</strong>
     * <br>1. Default value: <em>true</em>
     * <br>2. This attribute takes effect only when <em>snapPagination</em>
     * is set to a value of the <em>Array\<Dimension\></em> type;
     * it does not work with values of the <em>Dimension</em> type.
     * </p>
     *
     * @default true
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    enableSnapToEnd?: boolean;
}
/**
 * Provides interfaces for scrollable containers.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
interface ScrollInterface {
    /**
     * Called when a scrollable container is set.
     *
     * @param { Scroller } scroller
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    (scroller?: Scroller): ScrollAttribute;
}
/**
 * Represents the callback triggered when scrolling reaches an edge.
 *
 * @param { Edge } side - Edge position to scroll to.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare type OnScrollEdgeCallback = (side: Edge) => void;
/**
 * The data returned by the event handler when onScrollFrameBegin.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
interface OnScrollFrameBeginHandlerResult {
    /**
     * Actual scroll offset relative to the previous frame.<br>Unit: vp
     * Anonymous Object Rectification.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    offsetRemain: number;
}
/**
 * Represents the callback triggered before each frame scrolling starts.
 *
 * @param { number } offset - Amount to scroll by, in vp.
 * @param { ScrollState } state - Current scroll state.
 * @returns { OnScrollFrameBeginHandlerResult } data - the scroll data return by handler
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 18
 */
declare type OnScrollFrameBeginCallback = (offset: number, state: ScrollState) => OnScrollFrameBeginHandlerResult;
/**
 * Defines the scroll attribute functions.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare class ScrollAttribute extends ScrollableCommonMethod<ScrollAttribute> {
    /**
     * Sets the scrolling direction. The scroll offset is reset when this value is changed.
     *
     * @param { ScrollDirection } value - Scrolling direction.<br>Default value: <em>ScrollDirection.Vertical</em>
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollable(value: ScrollDirection): ScrollAttribute;
    /**
     * Set maximum zoom scale.
     *
     * @param { number } scale - Set maximum zoom scale.
     *     <br>Default value: 1.
     *     <br>Value range: (0, +∞). If this parameter is set to a value less than or equal to 0, the default value is
     *     used.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    maxZoomScale(scale: number): ScrollAttribute;
    /**
     * Set minimum zoom scale.
     * Setting either <em>maxZoomScale</em> and <em>minZoomScale</em> to a value other than 1
     * automatically enables the zoom gesture.
     *
     * @param { number } scale - Set minimum zoom scale.
     *     <br>Default value: 1.
     *     <br>Value range: (0, maxZoomScale]. If the value is greater than <em>maxZoomScale</em>,
     *     <em>maxZoomScale</em> is used.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    minZoomScale(scale: number): ScrollAttribute;
    /**
     * Current zoom scale.
     * This parameter supports !! for two-way binding of variables.
     *
     * @param { number } scale - Current zoom scale.
     *     <br>Default value: 1.
     *     <br>Value range: (0, +∞).
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    zoomScale(scale: number): ScrollAttribute;
    /**
     * Enable bounces zoom scale.
     *
     * @param { boolean } enable - Enable bounces zoom scale.
     *     <br>Default value: true.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    enableBouncesZoom(enable: boolean): ScrollAttribute;
    /**
     * Triggered to return the horizontal and vertical offsets, in vp, during scrolling when the specified scroll event
     * occurs.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is started by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called.
     * <br>3. This event supports the out-of-bounds bounce effect.
     * </p>
     *
     * @param { function } event - callback when scroll,
     *     xOffset: Actual scroll offset relative to the previous frame.<br>Unit: vp
     *     yOffset: Vertical offset relative to the previous frame.
     *     A positive offset indicates scrolling upward, and a negative offset indicates scrolling downward.<br>Unit: vp
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     * @deprecated since 12
     * @useinstead scroll/Scroll#onWillScroll
     */
    onScroll(event: (xOffset: number, yOffset: number) => void): ScrollAttribute;
    /**
     * Triggered before scrolling.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is started by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called.
     * <br>3. This event supports the out-of-bounds bounce effect.
     * </p>
     *
     * @param { ScrollOnWillScrollCallback } handler - Callback triggered before scrolling.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    onWillScroll(handler: ScrollOnWillScrollCallback): ScrollAttribute;
    /**
     * Triggered when the Scroll component scrolls.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is started by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called.
     * <br>3. This event supports the out-of-bounds bounce effect.
     * </p>
     *
     * @param { ScrollOnScrollCallback } handler - Callback triggered when the <em>Scroll</em> component scrolls.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    onDidScroll(handler: ScrollOnScrollCallback): ScrollAttribute;
    /**
     * Triggered when scrolling reaches the edge.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling reaches the edge after being started by the <em>Scroll</em> component
     * or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called.
     * <br>3. This event supports the out-of-bounds bounce effect.
     * </p>
     *
     * @param { function } event [since 7 - 17]
     * @param { OnScrollEdgeCallback } event - Edge position to scroll to. [since 18]
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    onScrollEdge(event: OnScrollEdgeCallback): ScrollAttribute;
    /**
     * Called when scrolling start.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is started by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called, accompanied by a transition animation.
     * </p>
     *
     * @param { function } event [since 9 - 17]
     * @param { VoidCallback } event - Callback triggered when scrolling starts. [since 18]
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    onScrollStart(event: VoidCallback): ScrollAttribute;
    /**
     * Triggered when scrolling stops.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is stopped by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called, accompanied by a transition animation.
     * </p>
     *
     * @param { function } event
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     * @deprecated since 9
     * @useinstead scroll/Scroll#onScrollStop
     */
    onScrollEnd(event: () => void): ScrollAttribute;
    /**
     * Called when scrolling has stopped.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>1. This event is triggered when scrolling is stopped by the <em>Scroll</em> component or other input settings,
     * such as keyboard and mouse operations.
     * <br>2. This event is triggered when the controller API is called, accompanied by a transition animation.
     * </p>
     *
     * @param { function } event [since 9 - 17]
     * @param { VoidCallback } event - Callback triggered when scrolling stops. [since 18]
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    onScrollStop(event: VoidCallback): ScrollAttribute;
    /**
     * Called when the Scroll did zoom.
     *
     * @param { ScrollOnDidZoomCallback } event - callback of zoom.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    onDidZoom(event: ScrollOnDidZoomCallback): ScrollAttribute;
    /**
     * Called when zooming has stated.
     *
     * @param { VoidCallback } event - Zoom start callback.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    onZoomStart(event: VoidCallback): ScrollAttribute;
    /**
     * Called when zooming has stopped.
     *
     * @param { VoidCallback } event - Zoom stop callback.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    onZoomStop(event: VoidCallback): ScrollAttribute;
    /**
     * Sets the scrollbar state. If the container component cannot be scrolled, the scrollbar is not displayed.
     * If the size of a child component of a container component is infinite, the scrollbar cannot be dragged
     * or scrolled with the child component.
     * Since API version 10, when the scrollable component has rounded corners, to prevent the scrollbar from
     * being cut off by the corners, the scrollbar will automatically calculate the clearance distance from
     * the top and bottom.
     *
     * @param { BarState } barState - Scrollbar state.<br>Default value: <em>BarState.Auto</em>
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollBar(barState: BarState): ScrollAttribute;
    /**
     * Sets the scrollbar color.
     *
     * @param { Color | number | string } color - Scrollbar color.<br>Default value: <em>'\#182431'</em> (40% opacity)
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollBarColor(color: Color | number | string): ScrollAttribute;
    /**
     * Sets the scrollbar color.
     *
     * @param { Color | number | string | Resource } color - Scrollbar color.
     *     <br>Default value: <em>'\#182431'</em> (40% opacity)
     *     <br>A number value indicates a HEX color in RGB or ARGB format, for example, <em>0xffffff</em>.
     *     A string value indicates a color in RGB or ARGB format, for example, <em>'#ffffff'</em>.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 22
     */
    scrollBarColor(color: Color | number | string | Resource): ScrollAttribute;
    /**
     * Sets the scrollbar width.
     *
     * @param { number | string } value - Scrollbar width.<br>Default value: <em>4</em> <br>Unit: vp
     *     <br>Values less than 0 are treated as the default value. The value <em>0</em> means not to show
     *     the scrollbar.
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    scrollBarWidth(value: number | string): ScrollAttribute;
    /**
     * Sets the scrollbar width.
     *
     * @param { number | string | Resource } value  - Scrollbar width.
     *     <br>Unit: vp
     *     <br>Default value: <em>4</em>
     *     <br>If this parameter is set to a value less than or equal to 0, the default value is used.
     *     The value <em>0</em> means not to show the scrollbar.
     * @returns { ScrollAttribute  }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 26.0.0
     */
    scrollBarWidth(value: number | string | Resource): ScrollAttribute;
    /**
     * Sets the effect used when the scroll boundary is reached.
     *
     * @param { EdgeEffect } edgeEffect - Effect used when the scroll boundary is reached. The spring and shadow effects
     *     are supported.
     *     <br>Default value: <em>EdgeEffect.None</em>
     * @param { EdgeEffectOptions } options - Whether to enable the scroll effect when the component content is smaller
     *     than the component itself.
     *     The value <em>{ alwaysEnabled: true }</em> means to enable the scroll effect, and <em>{ alwaysEnabled: false }
     *     </em> means the opposite.
     *     <br>Default value: <em>{ alwaysEnabled: true }</em> [since 11]
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 7
     */
    edgeEffect(edgeEffect: EdgeEffect, options?: EdgeEffectOptions): ScrollAttribute;
    /**
     * Triggered when each frame scrolling starts.
     * Anonymous Object Rectification.
     *
     * <p><strong>NOTE</strong>
     * <br>This event is triggered when any of the following conditions is met:
     * <br>1. Scrolling is initiated by user interaction (for example, finger swipe, keyboard, or mouse operation).
     * <br>2. The <em>Scroll</em> component scrolls by inertia.
     * <br>3. Scrolling is triggered by calling the <em>fling</em> API.
     * <br>This event is not triggered when any of the following conditions is met:
     * <br>1. A scroll control API other than <em>fling</em> is called.
     * <br>2. The out-of-bounds bounce effect is active.
     * <br>3. The scrollbar is dragged.
     * </p>
     *
     * @param { function } event [since 9 - 17]
     * @param { OnScrollFrameBeginCallback } event - Callback triggered when each frame scrolling starts. [since 18]
     * @returns { ScrollAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @atomicservice [since 11]
     * @since 9
     */
    onScrollFrameBegin(event: OnScrollFrameBeginCallback): ScrollAttribute;
    /**
     * Sets the nested scrolling options. You can set the nested scrolling mode in the forward and backward directions
     * to implement scrolling linkage with the parent component.
     * Nested scrolling will not take effect when both of the following conditions are met: (1) The child
     * <em>Scroll</em> component has <em>enablePaging</em> or <em>scrollSnap</em> enabled. (2) The parent component
     * is configured to have scrolling priority.
     *
     * @param { NestedScrollOptions } value - Nested scrolling options.
     *     <br>Default value: <em>{ scrollForward: NestedScrollMode.SELF_ONLY, scrollBackward: NestedScrollMode.SELF_ONLY
     *     }</em>
     * @returns { ScrollAttribute } the attribute of the scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    nestedScroll(value: NestedScrollOptions): ScrollAttribute;
    /**
     * Sets whether to support scroll gestures. When this attribute is set to <em>false</em>,
     * scrolling by finger or mouse is not supported, but the scroll controller API is not affected.
     * The component cannot be scrolled by dragging the mouse.
     *
     * @param { boolean } value - Whether to support scroll gestures.<br>Default value: <em>true</em>
     * @returns { ScrollAttribute } The attribute of the scroll
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    enableScrollInteraction(value: boolean): ScrollAttribute;
    /**
     * Sets the friction coefficient. It applies only to gestures in the scrolling area, and
     * it affects only indirectly the scroll chaining during the inertial scrolling process.
     * If this attribute is set to a value less than or equal to 0, the default value is used.
     *
     * @param { number | Resource } value - Friction coefficient.
     *     <br>Default value: <em>0.9</em> for wearable devices and <em>0.6</em> for non-wearable devices
     *     <br>Since API version 11, the default value for non-wearable devices is <em>0.7</em>.
     *     <br>Since API version 12, the default value for non-wearable devices is <em>0.75</em>.
     * @returns { ScrollAttribute } the attribute of the scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 11]
     * @since 10
     */
    friction(value: number | Resource): ScrollAttribute;
    /**
     * Sets the scroll snapping mode.
     * During the snap animation, the scroll operation source type reported by the <em>onWillScroll</em> event
     * is <em>ScrollSource.FLING</em>.
     *
     * @param { ScrollSnapOptions } value - Scroll snapping mode.
     * @returns { ScrollAttribute } the attribute of the scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform [since 11]
     * @atomicservice [since 11]
     * @since 10
     */
    scrollSnap(value: ScrollSnapOptions): ScrollAttribute;
    /**
     * Sets whether to enable the swipe-to-turn-pages feature.
     * If both <em>enablePaging</em> and <em>scrollSnap</em> are set, <em>scrollSnap</em> takes effect,
     * but <em>enablePaging</em> does not.
     *
     * @param { boolean } value - Whether to enable the swipe-to-turn-pages feature. Default value: false.
     *     The value <em>true</em> means to enable the swipe-to-turn-pages feature, and <em>false</em> means the opposite.
     * @returns { ScrollAttribute } the attribute of the scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    enablePaging(value: boolean): ScrollAttribute;
    /**
     * Sets the initial scrolling offset. This attribute takes effect only during the initial layout
     * of the component. After the initial layout, dynamically changing the value of this attribute
     * does not have any effect.
     *
     * @param { OffsetOptions } value - Initial scrolling offset. When the value specified is a percentage,
     *     the initial scrolling offset is calculated as the product of the <em>Scroll</em> component's size
     *     in the main axis direction and the percentage value.
     * @returns { ScrollAttribute } the attribute of the scroll.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    initialOffset(value: OffsetOptions): ScrollAttribute;
}
/**
 * Represents the callback triggered when the <em>Scroll</em> component scrolls.
 *
 * <p><strong>NOTE</strong>
 * <br>If the <em>onScrollFrameBegin</em> event and <em>scrollBy</em> method are used to implement nested scrolling,
 * set the <em>edgeEffect</em> attribute of the scrollable child component to <em>None</em>. For example,
 * if a <em>List</em> is nested in the <em>Scroll</em> component, <em>edgeEffect</em> of the <em>List</em> must be
 * set to <em>EdgeEffect.None</em>.
 * </p>
 *
 * @param { number } xOffset - Horizontal offset per frame during scrolling. A positive offset indicates scrolling to
 *     the left,
 *     and a negative offset indicates scrolling to the right.
 *     <br>Unit: vp
 * @param { number } yOffset - Vertical offset per frame during scrolling.
 *     A positive offset indicates scrolling upward, and a negative offset indicates scrolling downward.
 *     <br>Unit: vp
 * @param { ScrollState } scrollState - Current scrolling state.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type ScrollOnScrollCallback = (xOffset: number, yOffset: number, scrollState: ScrollState) => void;
/**
 * Called before scroll to allow developer to control real offset the Scroll can scroll.
 *
 * @param { number } xOffset - Horizontal offset per frame during scrolling.
 *     A positive offset indicates scrolling to the left, and a negative offset indicates scrolling to the right.
 *     <br>Unit: vp
 * @param { number } yOffset - offset per frame during scrolling.
 *     A positive offset indicates scrolling upward, and a negative offset indicates scrolling downward.
 *     <br>Unit: vp
 * @param { ScrollState } scrollState - Current scrolling state.
 * @param { ScrollSource } scrollSource - Source of the current scrolling operation.
 * @returns { void | OffsetResult } the remain offset for the Scroll,
 *     same as (xOffset, yOffset) when no OffsetResult is returned.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type ScrollOnWillScrollCallback = (xOffset: number, yOffset: number, scrollState: ScrollState, scrollSource: ScrollSource) => void | OffsetResult;
/**
 * callback of Scroll, using in onDidZoom.
 *
 * @param { number } scale - current zoom scale.
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 20
 */
declare type ScrollOnDidZoomCallback = (scale: number) => void;
/**
 * Defines Scroll Component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare const Scroll: ScrollInterface;
/**
 * Defines Scroll Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @atomicservice [since 11]
 * @since 7
 */
declare const ScrollInstance: ScrollAttribute;
