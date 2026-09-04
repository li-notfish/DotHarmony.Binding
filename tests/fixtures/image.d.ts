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
 * Represents a parameter object for the **Image** component.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform [since 23]
 * @atomicservice [since 11]
 * @since 10
 */
declare type DrawableDescriptor = import('../api/@ohos.arkui.drawableDescriptor').DrawableDescriptor;
/**
 * Represents a color filter object.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type DrawingColorFilter = import('../api/@ohos.graphics.drawing').default.ColorFilter;
/**
 * Represents a matrix grid object that divides an image into a rectangular grid.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 12
 */
declare type DrawingLattice = import('../api/@ohos.graphics.drawing').default.Lattice;
/**
 * Represents the current matrix object.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 15
 */
declare type ImageMatrix = import('../api/@ohos.matrix4').default.Matrix4Transit;
/**
 * Represents the error information returned when an error occurs during image loading.
 *
 * The table below describes the **ImageError** error codes. The **error** property of **ImageError** contains error
 * details with **code** and **message** fields, representing the error code and error message, respectively.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 20
 */
declare type BusinessError<T = void> = import('../api/@ohos.base').BusinessError<T>;
/**
 * Describes the download information when an online image fails to load or encounters an exception. This object
 * contains resource information, network information, and performance statistics of the download task, which can be
 * used to locate the cause of the loading exception.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 23
 */
declare type RequestDownloadInfo = import('../api/@ohos.request.cacheDownload').default.DownloadInfo;
/**
 * Interpolation effect of the image.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ImageRenderMode {
    /**
     * Render image pixels as they are in the original source image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Original,
    /**
     * Render image pixels to create a monochrome template image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Template
}
/**
 * Defines the image content.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 12
 */
declare enum ImageContent {
    /**
     * Empty image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    EMPTY = 0
}
/**
 * Describes the dynamic range of the image to be displayed.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @atomicservice
 * @since 12
 */
declare enum DynamicRangeMode {
    /**
     * Unrestricted dynamic range, which allows for the maximum brightening of an image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    HIGH = 0,
    /**
     * Restricted dynamic range, which brightens an image within certain constraints.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    CONSTRAINT = 1,
    /**
     * Standard dynamic range, which does not brighten an image.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    STANDARD = 2
}
/**
 * Interpolation effect of the image.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare enum ImageInterpolation {
    /**
     * Nearest neighbor interpolation.
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
     * Bilinear interpolation.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Low,
    /**
     * MipMap interpolation.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    Medium,
    /**
     * Cubic interpolation. This mode produces scaled images of the highest possible quality, but may require more image
     * rendering time.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    High
}
/**
 * Describes the desired display orientation for image content.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form [since 23]
 * @atomicservice
 * @since 14
 */
declare enum ImageRotateOrientation {
    /**
     * Use EXIF metadata for display orientation, with support for rotation and mirroring.
     *
     * Images of the [PixelMap]{@link @ohos.multimedia.image:image.PixelMap} and
     * [DrawableDescriptor]{@link DrawableDescriptor} types do not contain header information. When this API is called,
     * the image display effect remains unchanged.
     *
     * ![imageRotateOrientation_0](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_0.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    AUTO = 0,
    /**
     * Display original pixel data without transformation.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    UP = 1,
    /**
     * Display the image after rotating it 90 degrees clockwise.
     *
     * ![imageRotateOrientation_2](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_2.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    RIGHT = 2,
    /**
     * Display the image after rotating it 180 degrees clockwise.
     *
     * ![imageRotateOrientation_3](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_3.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    DOWN = 3,
    /**
     * Display the image after rotating it 270 degrees clockwise.
     *
     * ![imageRotateOrientation_4](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_4.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    LEFT = 4,
    /**
     * Display the image after flipping it horizontally.
     *
     * ![imageRotateOrientation_5](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_5.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 20
     */
    UP_MIRRORED = 5,
    /**
     * Display the image after flipping it horizontally and then rotating it 90 degrees clockwise.
     *
     * ![imageRotateOrientation_6](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_6.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 20
     */
    RIGHT_MIRRORED = 6,
    /**
     * Display the image after flipping it vertically.
     *
     * ![imageRotateOrientation_7](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_7.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 20
     */
    DOWN_MIRRORED = 7,
    /**
     * Display the image after flipping it horizontally and then rotating it 270 degrees clockwise.
     *
     * ![imageRotateOrientation_8](docroot://reference/apis-arkui/arkui-ts/figures/imageRotateOrientation_8.png)
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 20
     */
    LEFT_MIRRORED = 8
}
/**
 * The **Image** component is usually used to display images in applications. It supports data sources of the following
 * types: [PixelMap]{@link @ohos.multimedia.image:image.PixelMap}, [ResourceStr]{@link ResourceStr}, and
 * [DrawableDescriptor]{@link DrawableDescriptor}. Supported image formats include PNG, JPG, JPEG, BMP, SVG, WEBP, GIF,
 * HEIF, and TIFF. Note that the APNG and SVGA formats are not supported.
 *
 * > **NOTE**
 *
 * > - This component supports the TIFF image format since API version 23.
 * >
 * > - When keyboard shortcuts are used to copy an **Image** component, the **Image** component must be in a focused
 * > state. For instructions on how to set focus, see
 * > [Setting Whether a Component Is Focusable]
 * > (docroot://ui/arkts-common-events-focus-event.md#setting-whether-a-component-is-focusable).
 * > By default, the **Image** component is not focusable. To enable it to gain focus, set both the
 * > [focusable]{@link CommonMethod#focusable} and [focusOnTouch]{@link CommonMethod#focusOnTouch} attributes to
 * > **true**.
 * >
 * > - The **Image** component supports SVG image sources. For details about SVG tags, see [SVG Tags]{@link ./common}.
 * >
 * > - For animated images, animation playback is disabled by default and depends on the visibility of the **Image**
 * > component. When the component is visible, the animation is started through the callback. When the component is
 * > invisible, the animation is stopped. The visibility status of the **Image** component can be identified through the
 * >
 * > [onVisibleAreaChange]
 * > {@link CommonMethod#onVisibleAreaChange(ratios: Array<number>, event: VisibleAreaChangeCallback)}
 * > event. If the value of **ratios** is greater than 0, the component is visible.
 * >
 * > - For details about how to resolve white block issues during image loading, see
 * > [Solution to White Image Blocks]
 * > (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-image-white-lump-solution).
 * > For details about how to address slow image loading, see
 * > [Optimizing Preset Image Loading]
 * > (https://developer.huawei.com/consumer/en/doc/best-practices/
 * >  bpta-texture-compression-improve-performance#section91526132216).
 * >
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
interface ImageInterface {
    /**
     * Obtains an image from the specified source for subsequent rendering and display.
     *
     * If the **Image** component fails to obtain the image or the obtained image size is 0, the **Image** component is
     * automatically resized to 0 and does not follow the layout constraints of its parent component.
     *
     * By default, the **Image** component crops images to keep their center. For example, if the component has the same
     * width and height, it crops any image whose width and height are different, so as to keep its center.
     *
     * If the **Image** component does not have its width and height set, its size adapts to that of its parent component
     * once the image is successfully loaded.
     *
     * > **NOTE**
     * >
     * > - Passing a URL directly to an **Image** component may lead to potential performance issues, such as: (1) Large
     * > images cannot be downloaded in advance during loading, resulting in a long display time of white blocks; (2)
     * > Small images set to load synchronously may block the UI thread in a weak network environment, causing screen
     * > freezes; (3) In a rapidly scrolling waterfall flow, images that are about to be displayed cannot be downloaded in
     * > advance, resulting in many white blocks during scrolling. Performance issues may manifest differently in
     * > different scenarios. To minimize these issues, separate the network download part from the display of the
     * > **Image** component, and download in advance or asynchronously. For details about how to resolve white block
     * > issues during image loading, see
     * > [Solution to White Image Blocks]
     * > (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-image-white-lump-solution).
     * > For details about how to address slow image loading, see
     * > [Optimizing Preset Image Loading]
     * > (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-texture-compression-improve-performance).
     * >
     * >
     * > - When **src** is switched from a valid value (an image resource that can be parsed and loaded correctly) to an
     * > invalid value (an image path that cannot be parsed or loaded), the component retains the previously successfully
     * > loaded image content without clearing or resetting it.
     * >
     * > - If the input parameter is of the [PixelMap]{@link @ohos.multimedia.image:image.PixelMap} type, the **Image**
     * > component can detect data changes only when the **PixelMap** object is updated to point to a new instance. If
     * > modifications are made to the content of the **PixelMap** object, such as pixel values, but the reference to the
     * > object remains the same, the **Image** component will not recognize these modifications as a data change.
     * >
     * > - If the input parameter of the **Image** component is a Base64 string, the standard format of the Base64 string
     * > is **data:image/subtype;base64,Base64EncodedData**. In this format, **subtype** indicates the type declaration,
     * > **Base64EncodedData** indicates the Base64-encoded data, and other values are fixed strings. For example, the
     * > input parameter of a PNG image is **data:image/png;base64,iVBORw0KGgo...**.
     * > >
     * > >   1. **image/subType** declares the data type. The **Image** component does not enforce that the declared type
     * > exactly matches the actual image format decoded from Base64. In some scenarios, the image may still display
     * > correctly even if the declared type does not match the actual format. To prevent future behavior changes or
     * > unknown issues, it is recommended that the declared type always match the actual image format.
     * > >
     * > >   2. The **Image** component does not support the wildcard syntax: **data:image/*;base64,Base64EncodedData**.
     * > The **subType** must explicitly declare the specific image type.
     * > >
     * > >   3. The **Image** component does not support loading SVG images in Base64 string format.
     *
     * @param { PixelMap | ResourceStr | DrawableDescriptor } src - Data source of the image. Local and online sources are
     *     supported. For details about how to reference an image, see
     *     [Loading Image Resources](docroot://ui/arkts-graphics-display.md#loading-image-resources).<br>1. **PixelMap**:
     *     a pixel map storing graphical information, commonly used for image editing scenarios.<br>2. **ResourceStr**: a
     *     string or a Resource object.<br>The string type can be used to load local images and, more frequently, online
     *     images. When
     *     [using a local image referenced using a relative path](docroot://reference/apis-arkui/arkui-ts/ts-basic-compon
     *      ents-image.md#example-25-displaying-an-image-using-a-relative-path),
     *     the **Image** component cannot be called across bundles or modules. If an image needs to be used globally, you
     *     are advised to use the Resource format.<br>Since DevEco Studio 6.0.0 Beta2, resources in non-**resource**
     *     directories are not packaged by default for new projects or modules. To enable packaging, go to **buildOption**
     *     > **resOptions** > **copyCodeResource** to set **enable** to **true** in the module's **build-profile.json5**
     *     file. For details, see
     *     [resOptions](https://developer.huawei.com/consumer/en/doc/harmonyos-guides/ide-hvigor-build-
     *      profile#table1476161719356).
     *     <br>- Base64 strings are supported.<br>- When providing an HTTPS network image URL, refer to
     *     [Example 2: Downloading and Displaying Static Online Images](docroot://reference/apis-arkui/arkui-ts/ts-basic-
     *      components-image.md#example-2-downloading-and-displaying-static-online-images)
     *     for implementation guidance.<br>- Strings prefixed with the **file://** path are supported (application sandbox
     *     URI: **file://<bundleName>/<sandboxPath>**). For details about how to construct the application sandbox path
     *     URI, see [constructor]{@link @ohos.file.fileuri:fileUri.FileUri#constructor}. The sandbox path must be
     *     converted to an application sandbox URI using the
     *     [fileUri.getUriFromPath(path)]{@link @ohos.file.fileuri:fileUri.getUriFromPath} API before being passed in for
     *     display. In addition, ensure that the application has the read permission to the files in the specified path.<
     *     br>The Resource format allows for access across bundles and modules. It is recommended for accessing local
     *     images. For details, see
     *     [Cross-HAP/HSP Resources](docroot://quick-start/resource-categories-and-access.md#cross-haphsp-resources).<br>
     *     3. **DrawableDescriptor**: an object created when the passed resource ID or name belongs to a common image. The
     *     [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor} type can be
     *     passed to play animations from a **PixelMap** array.<br>**NOTE**<br>- ArkTS widgets support GIF animations, but
     *     the animations only play once on display.<br>- ArkTS widgets do not support the strings with the **http://** or
     *     **file://** prefix.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    (src: PixelMap | ResourceStr | DrawableDescriptor): ImageAttribute;
    /**
     * Obtains an image. The [ImageContent]{@link ImageContent} type allows you to specify the image content.
     *
     * @param { PixelMap | ResourceStr | DrawableDescriptor | ImageContent } src - Data source of the image. Local and
     *     online sources are supported. For details about how to reference an image, see
     *     [Loading Image Resources](docroot://ui/arkts-graphics-display.md#loading-image-resources).<br>For details about
     *     how to use **PixelMap**, **ResourceStr**, and **DrawableDescriptor**, see the **src** parameter description of
     *     [Image](docroot://reference/apis-arkui/arkui-ts/ts-basic-components-image.md#image-1).<br>
     *     [ImageContent]{@link ImageContent}: image content.<br>**NOTE**<br>- ArkTS widgets support GIF animations, but
     *     the animations only play once on display.<br>- ArkTS widgets do not support the strings with the **http://** or
     *     **file://** prefix.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 12
     */
    (src: PixelMap | ResourceStr | DrawableDescriptor | ImageContent): ImageAttribute;
    /**
     * Set src to obtain images
     *
     * @param { PixelMap | ResourceStr | DrawableDescriptor | ImageContent } src
     * @param { string } [reloadKey]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 26.0.0
     */
    (src: PixelMap | ResourceStr | DrawableDescriptor | ImageContent, reloadKey?: string): ImageAttribute;
    /**
     * Obtains an image. The [imageAIOptions]{@link ImageAIOptions} parameter allows you to set AI image analysis options.
     *
     * @param { PixelMap | ResourceStr | DrawableDescriptor } src - Data source of the image. Local and online sources are
     *     supported. For details about how to reference an image, see
     *     [Loading Image Resources](docroot://ui/arkts-graphics-display.md#loading-image-resources).<br>For details about
     *     how to use **PixelMap**, **ResourceStr**, and **DrawableDescriptor**, see the **src** parameter description of
     *     [Image](docroot://reference/apis-arkui/arkui-ts/ts-basic-components-image.md#image-1).<br>**NOTE**<br>- ArkTS
     *     widgets support GIF animations, but the animations only play once on display.<br>- ArkTS widgets do not support
     *     the strings with the **http://** or **file://** prefix.
     * @param { ImageAIOptions } imageAIOptions - AI image analysis options. You can configure the analysis type or bind
     *     an analyzer controller through this parameter.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    (src: PixelMap | ResourceStr | DrawableDescriptor, imageAIOptions: ImageAIOptions): ImageAttribute;
    /**
     * Set src and ai options to obtain images
     *
     * @param { PixelMap | ResourceStr | DrawableDescriptor } src
     * @param { ImageAIOptions } [imageAIOptions]
     * @param { string } [reloadKey]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 26.0.0
     */
    (src: PixelMap | ResourceStr | DrawableDescriptor, imageAIOptions?: ImageAIOptions, reloadKey?: string): ImageAttribute;
}
/**
 * Defines source size of image.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 18
 */
interface ImageSourceSize {
    /**
     * Image decode width. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     */
    /**
     * Image decode width. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @form
     * @since 9
     */
    /**
     * Image decode width. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @since 10
     */
    /**
     * Image decode width. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    /**
     * Image decode width. The unit of the number type is vp.
     * Anonymous Object Rectification.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    width: number;
    /**
     * Image decode height. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     */
    /**
     * Image decode height. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @form
     * @since 9
     */
    /**
     * Image decode height. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @since 10
     */
    /**
     * Image decode height. The unit of the number type is vp.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    /**
     * Image decode height. The unit of the number type is vp.
     * Anonymous Object Rectification.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @atomicservice
     * @since 18
     */
    height: number;
}
/**
 * Defines the content for color filling.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice
 * @since 15
 */
declare class ColorContent {
    /**
     * Resets the [fillColor]{@link ImageAttribute#fillColor(value: ResourceColor)} API, effectively the same as not
     * setting [fillColor]{@link ImageAttribute#fillColor(value: ResourceColor)}.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    static readonly ORIGIN: ColorContent;
}
/**
 * The **Image** component is usually used to display images in applications. It supports data sources of the following
 * types: [PixelMap]{@link @ohos.multimedia.image:image.PixelMap}, [ResourceStr]{@link ResourceStr}, and
 * [DrawableDescriptor]{@link DrawableDescriptor}. Supported image formats include PNG, JPG, JPEG, BMP, SVG, WEBP, GIF,
 * HEIF, and TIFF. Note that the APNG and SVGA formats are not supported.
 *
 * > **NOTE**
 *
 * > - This component supports the TIFF image format since API version 23.
 * >
 * > - When keyboard shortcuts are used to copy an **Image** component, the **Image** component must be in a focused
 * > state. For instructions on how to set focus, see
 * > [Setting Whether a Component Is Focusable](docroot://ui/arkts-common-events-focus-event.md#setting-whether-a-
 *    component-is-focusable).
 * > By default, the **Image** component is not focusable. To enable it to gain focus, set both the
 * > [focusable]{@link CommonMethod#focusable} and [focusOnTouch]{@link CommonMethod#focusOnTouch} attributes to
 * > **true**.
 * >
 * > - The **Image** component supports SVG image sources. For details about SVG tags, see [SVG Tags]{@link ./common}.
 * >
 * > - For animated images, animation playback is disabled by default and depends on the visibility of the **Image**
 * > component. When the component is visible, the animation is started through the callback. When the component is
 * > invisible, the animation is stopped. The visibility status of the **Image** component can be identified through the
 * >
 * > [onVisibleAreaChange]
 * > {@link CommonMethod#onVisibleAreaChange(ratios: Array<number>, event: VisibleAreaChangeCallback)}
 * > event. If the value of **ratios** is greater than 0, the component is visible.
 * >
 * > - For details about how to resolve white block issues during image loading, see
 * > [Solution to White Image Blocks]
 * > (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-image-white-lump-solution).
 * > For details about how to address slow image loading, see
 * > [Optimizing Preset Image Loading](https://developer.huawei.com/consumer/en/doc/best-practices/bpta-texture-
 * >  compression-improve-performance#section91526132216).
 * >
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare class ImageAttribute extends CommonMethod<ImageAttribute> {
    /**
     * Sets the placeholder image displayed during image loading.
     *
     * The placeholder image supports configuration of [objectFit]{@link ImageAttribute#objectFit} for setting the fill
     * effect, which is consistent with the fill effect of the image.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { string | Resource } value - Placeholder image displayed during loading. Local images (in PNG, JPG, BMP,
     *     SVG, GIF, or HEIF format) and [PixelMap]{@link @ohos.multimedia.image:image.PixelMap} objects are supported,
     *     but online images are not.<br>- Base64 strings are supported.<br>- Strings prefixed with the **file://** path
     *     are supported (application sandbox URI: **file://<bundleName>/<sandboxPath>**). For details about how to
     *     construct the application sandbox path URI, see
     *     [constructor]{@link @ohos.file.fileuri:fileUri.FileUri#constructor}. The sandbox path must be converted to an
     *     application sandbox URI using the
     *     [fileUri.getUriFromPath(path)]{@link @ohos.file.fileuri:fileUri.getUriFromPath} API before being passed in for
     *     display. In addition, ensure that the application has the read permission to the files in the specified path.<
     *     br>Default value: **null**<br>When the value is switched from a valid one (an image resource that can be parsed
     *     and loaded correctly) to an invalid one (an image path that cannot be parsed or loaded), the component retains
     *     the previously successfully loaded image content without clearing or resetting it. [since 7 - 11]
     * @param { string | Resource | PixelMap } value - Placeholder image displayed during loading. Local images (in PNG,
     *     JPG, BMP, SVG, GIF, or HEIF format) and [PixelMap]{@link @ohos.multimedia.image:image.PixelMap} objects are
     *     supported, but online images are not.<br>- Base64 strings are supported.<br>- Strings prefixed with the
     *     **file://** path are supported (application sandbox URI: **file://<bundleName>/<sandboxPath>**). For details
     *     about how to construct the application sandbox path URI, see
     *     [constructor]{@link @ohos.file.fileuri:fileUri.FileUri#constructor}. The sandbox path must be converted to an
     *     application sandbox URI using the
     *     [fileUri.getUriFromPath(path)]{@link @ohos.file.fileuri:fileUri.getUriFromPath} API before being passed in for
     *     display. In addition, ensure that the application has the read permission to the files in the specified path.<
     *     br>Default value: **null**<br>When the value is switched from a valid one (an image resource that can be parsed
     *     and loaded correctly) to an invalid one (an image path that cannot be parsed or loaded), the component retains
     *     the previously successfully loaded image content without clearing or resetting it. [since 12]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    alt(value: string | Resource | PixelMap): ImageAttribute;
    /**
     * Sets the placeholder image displayed during image loading and when image loading fails.
     *
     * > **NOTE**
     * >
     * > When a placeholder image is configured via [ImageAlt]{@link ImageAlt}, **Image** takes effect based on the
     * > placeholder image sources configured for the loading and load-failure states. If no placeholder image is
     * > configured, it is not displayed by default.
     *
     * The placeholder image supports configuration of [objectFit]{@link ImageAttribute#objectFit} for setting the fill
     * effect, which is consistent with the fill effect of the image.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ResourceStr | PixelMap | ImageAlt } src - Placeholder image displayed during loading or in case of loading
     *     failure. Local images (in PNG, JPG, BMP, SVG, GIF, or HEIF format) and
     *     [PixelMap]{@link @ohos.multimedia.image:image.PixelMap} objects are supported, but online images are not.<br>-
     *     Base64 strings are supported.<br>- Strings prefixed with the **file://** path are supported (application
     *     sandbox URI: **file://<bundleName>/<sandboxPath>**). For details about how to construct the application sandbox
     *     path URI, see [constructor]{@link @ohos.file.fileuri:fileUri.FileUri#constructor}. The sandbox path must be
     *     converted to an application sandbox URI using the
     *     [fileUri.getUriFromPath(path)]{@link @ohos.file.fileuri:fileUri.getUriFromPath} API before being passed in for
     *     display. In addition, ensure that the application has the read permission to the files in the specified path.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    alt(src: ResourceStr | PixelMap | ImageAlt): ImageAttribute;
    /**
     * Specifies whether the image follows the system language direction, displaying a mirrored effect in a right-to-left
     * (RTL) language environments.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { boolean } value - Whether the image follows the system language direction, displaying a mirrored effect in
     *     an RTL language environment.<br>Default value: **false**<br>The value **true** means that the image follows the
     *     system language direction, displaying a mirrored effect in an RTL language environment, and **false** means the
     *     opposite.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    matchTextDirection(value: boolean): ImageAttribute;
    /**
     * Specifies whether the image display size follows the size of the image source.
     *
     * This attribute does not take effect when the component has the **width** and **height** attributes set.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { boolean } value - Whether the image display size follows the size of the image source.<br>Default value:
     *     **false**<br>**NOTE**<br>**false** or not set: The image display size does not follow the size of the image
     *     source.<br> **true**: The image display size follows the size of the image source.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fitOriginalSize(value: boolean): ImageAttribute;
    /**
     * Fill color to be superimposed on the image. This attribute applies only to SVG images. Once set, the fill color
     * will replace the fill colors of all drawable elements within the SVG image. To set the fill color for a PNG image,
     * use [colorFilter]{@link ImageAttribute#colorFilter(value: ColorFilter | DrawingColorFilter)}.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ResourceColor } value - Fill color to be superimposed on the image.<br>**NOTE**<br> By default, no fill
     *     color is applied. If an invalid value is passed, the system uses the default theme color: black in light mode
     *     and white in dark mode.<br>Since API version 21, when [supportSvg2]{@link ImageAttribute#supportSvg2} is set to
     *     **true**, **fillColor** depends on the **fill** attribute configuration in the SVG image source. If the
     *     **fill** attribute in the SVG image source is set to **'none'**, **fillColor** does not take effect. When
     *     **supportSvg2** is set to **false**, **fillColor** takes effect and replaces the fill colors of all drawable
     *     elements in the SVG image.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    fillColor(value: ResourceColor): ImageAttribute;
    /**
     * Fill color to be superimposed on the image. This attribute applies only to SVG images. Once set, the fill color
     * will replace the fill colors of all drawable elements within the SVG image. To set the fill color for a PNG image,
     * use [colorFilter]{@link ImageAttribute#colorFilter(value: ColorFilter | DrawingColorFilter)}. To reset the fill
     * color, pass a value of the [ColorContent]{@link ColorContent} type.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ResourceColor | ColorContent } color - Fill color to be superimposed on the image.<br>**NOTE**<br> By
     *     default, no fill color is applied. If an invalid value is passed, the system uses the default theme color:
     *     black in light mode and white in dark mode.<br>Since API version 21, when
     *     [supportSvg2]{@link ImageAttribute#supportSvg2} is set to **true**, **fillColor** depends on the **fill**
     *     attribute configuration in the SVG image source. If the **fill** attribute in the SVG image source is set to
     *     **'none'**, **fillColor** does not take effect.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    fillColor(color: ResourceColor | ColorContent): ImageAttribute;
    /**
     * Fill color to be superimposed on the image. This attribute applies only to SVG images. Once set, the fill color
     * will replace the fill colors of all drawable elements within the SVG image. To set the fill color for a PNG image,
     * use [colorFilter]{@link ImageAttribute#colorFilter(value: ColorFilter | DrawingColorFilter)}. To reset the fill
     * color, pass a value of the [ColorContent]{@link ColorContent} type. You can set P3 color gamut values by passing in
     * the [ColorMetrics]{@link ../../../arkui/Graphics:ColorMetrics} type, which can achieve richer color performance on
     * devices that support high color gamut.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ResourceColor | ColorContent | ColorMetrics } color - Fill color to be superimposed on the image.<br>
     *     **NOTE**<br> By default, no fill color is applied. If an invalid value is passed, the system uses the default
     *     theme color: black in light mode and white in dark mode.<br>Since API version 21, when
     *     [supportSvg2]{@link ImageAttribute#supportSvg2} is set to **true**, **fillColor** depends on the **fill**
     *     attribute configuration in the SVG image source. If the **fill** attribute in the SVG image source is set to
     *     **'none'**, **fillColor** does not take effect.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 20
     */
    fillColor(color: ResourceColor | ColorContent | ColorMetrics): ImageAttribute;
    /**
     * Sets how the image is resized to fit its container. If the attribute is not set, the default value is
     * **ImageFit.Cover**, which scales the image up or down while maintaining its aspect ratio.
     *
     * @param { ImageFit } value - How the image is resized to fit its container.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    objectFit(value: ImageFit): ImageAttribute;
    /**
     * Sets the transformation matrix of the image. This API allows you to use the functions provided by the
     * [ImageMatrix]{@link ImageAttribute#imageMatrix} object, such as translate, rotate, and scale, to achieve the
     * optimal display of grid thumbnails. This attribute is not applicable to SVG images.
     *
     * This attribute does not take effect when [resizable]{@link ImageAttribute#resizable} and
     * [objectRepeat]{@link ImageAttribute#objectRepeat} are set. This attribute only processes the image source and does
     * not trigger any callback events of the **Image** component.
     *
     * This attribute is strongly associated with [objectFit]{@link ImageAttribute#objectFit} and takes effect only when
     * [objectFit]{@link ImageAttribute#objectFit} is set to **ImageFit.MATRIX**.
     *
     * @param { ImageMatrix } matrix - Transformation matrix of the image.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 15
     */
    imageMatrix(matrix: ImageMatrix): ImageAttribute;
    /**
     * Sets how the image is repeated. When set to repeat, the image is repeated from the center to edges. The last image
     * will be clipped if it does not fit in the component. This attribute is not applicable to SVG images.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ImageRepeat } value - How the image is repeated.<br>Default value: **ImageRepeat.NoRepeat**
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    objectRepeat(value: ImageRepeat): ImageAttribute;
    /**
     * Specifies whether to resize the image source based on the size of the display area during image decoding. As
     * downsampling images results in some loss of information, it may reduce the image quality, causing issues such as
     * aliasing. To retain the original image quality, set **autoResize** to **false**, but this may increase the memory
     * usage.
     *
     * If the original image size does not match the display size, the image may be distorted or blurred. Recommended
     * configuration for the optimal definition:
     *
     * When the image is scaled down: .autoResize(false) + .interpolation(.Medium)
     *
     * When the image is scaled up: .interpolation(.High)
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor} or the image format
     * is SVG.
     *
     * @param { boolean } value - Whether to resize the image source based on the size of the display area during image
     *     decoding. This resizing can help reduce the memory usage. For example, if the original image size is 800 x 1200
     *     and the display area size is 200 x 200, the image will be decoded to 200 x 300 at a downsampled resolution (the
     *     actual result may vary depending on the scaling and fill type configurations used in the calculation), greatly
     *     reducing the memory occupied by the image.<br>Default value: **false**<br>**true**: Enable resizing.<br>
     *     **false**: Disable resizing.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    autoResize(value: boolean): ImageAttribute;
    /**
     * Sets the rendering mode of the image. This attribute is not applicable to SVG images.
     *
     * This attribute does not take effect when
     * [ColorFilter]{@link ImageAttribute#colorFilter(value: ColorFilter | DrawingColorFilter)} is set.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ImageRenderMode } value - Rendering mode of the image, which can be **Original** or **Template** (
     *     monochrome).<br>Default value: **ImageRenderMode.Original**
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    renderMode(value: ImageRenderMode): ImageAttribute;
    /**
     * Sets the dynamic range of the image to be displayed. This attribute is not applicable to SVG images.
     *
     * **Device behavior difference**: This API takes effect on mobile phones, PCs, 2-in-1 devices, and tablets, but not
     * on other device types.
     *
     * @param { DynamicRangeMode } value - Dynamic range of the image.<br>Default value: **DynamicRangeMode.STANDARD**
     * @returns { ImageAttribute } Returns the instance of the ImageAttribute, Default value is
     *     dynamicRangeMode.Standard.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 12
     */
    dynamicRangeMode(value: DynamicRangeMode): ImageAttribute;
    /**
     * Sets the brightness of HDR images displayed by the component.
     *
     * This attribute is not applicable to SVG images.
     *
     * If this attribute and the [dynamicRangeMode]{@link ImageAttribute#dynamicRangeMode} attribute are both set,
     * [dynamicRangeMode]{@link ImageAttribute#dynamicRangeMode} does not take effect.
     *
     * @param { number } brightness - Brightness of HDR images displayed by the component. This API only takes effect for
     *     HDR image sources.<br>Default value: **1.0**<br>Value range: [0.0, 1.0]. Values less than 0 or greater than 1.0
     *     are clamped to **1.0**. **0**: The image is displayed at SDR brightness.<br>**1.0**: The image is displayed at
     *     the highest allowed HDR brightness.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice
     * @since 19
     */
    hdrBrightness(brightness: number): ImageAttribute;
    /**
     * Defines the image interpolation effect. This attribute mitigates aliasing during image scaling. This attribute is
     * not applicable to SVG images.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { ImageInterpolation } value - Interpolation effect of the image.<br>Default value:
     *     **ImageInterpolation.Low**<br>When set to **undefined**, the value is treated as **ImageInterpolation.None**.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    interpolation(value: ImageInterpolation): ImageAttribute;
    /**
     * Sets the decoding size of the image. This attribute works only when the target size is smaller than the source
     * size. This attribute is not applicable to SVG images or **PixelMap** objects.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { object } value - Decoding size of the image. This parameter can be used to reduce the image resolution
     *     when the image display size needs to be smaller than the component size. When this parameter is used with
     *     **ImageFit.None** of the [objectFit]{@link ImageAttribute#objectFit} API, a small image can be displayed in the
     *     component. [since 7 - 17]
     * @param { ImageSourceSize } value - Decoding size of the image. This parameter can be used to reduce the image
     *     resolution when the image display size needs to be smaller than the component size. When this parameter is used
     *     with **ImageFit.None** of the [objectFit]{@link ImageAttribute#objectFit} API, a small image can be displayed
     *     in the component. [since 18]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    sourceSize(value: ImageSourceSize): ImageAttribute;
    /**
     * Specifies whether to load the image synchronously. When loading a small local image, you are advised to set
     * **syncLoad** to **true** so that the image loading can be quickly completed on the main thread.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * If image flickering occurs during loading, set **syncLoad** to **true**. For details, see
     * [Optimizing Concurrent Tasks]
     * (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-click-to-click-response-
     *  optimization#section715115119192).
     *
     * @param { boolean } value - Whether to load the image synchronously. By default, the image is loaded asynchronously.
     *     During synchronous loading, the UI thread is blocked and the placeholder image is not displayed.<br>Default
     *     value: **false**<br>**true**: Load the image synchronously.<br>**false**: Load the image asynchronously.<br>If
     *     the main thread is blocked for more than 6s, AppFreeze will occur. For details, see
     *     [Application Freeze Detection](docroot://dfx/appfreeze-guidelines.md).
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 8
     */
    syncLoad(value: boolean): ImageAttribute;
    /**
     * Sets the color filter for the image.
     *
     * When this attribute is set, [renderMode]{@link ImageAttribute#renderMode} is not effective.
     *
     * @param { ColorFilter } value - 1. Color filter of the image. The input parameter is a 4 x 5 RGBA transformation
     *     matrix.<br>2. The ColorFilter type of **@ohos.graphics.drawing** can be used as an input parameter since API
     *     version 12.<br>**NOTE**<br>This parameter is not available for SVG images in API version 11 and earlier
     *     versions.<br>The DrawingColorfilter type can be used in atomic services since API version 12. For SVG sources,
     *     the effect only applies when the **stroke** property is set (regardless of the value).<br>Since API version 21,
     *     when [supportSvg2]{@link ImageAttribute#supportSvg2} is set to **true**, **colorFilter** takes effect on the
     *     entire SVG image source. [since 9 - 11]
     * @param { ColorFilter | DrawingColorFilter } value - 1. Color filter of the image. The input parameter is a 4 x 5
     *     RGBA transformation matrix.<br>2. The ColorFilter type of **@ohos.graphics.drawing** can be used as an input
     *     parameter since API version 12.<br>**NOTE**<br>This parameter is not available for SVG images in API version 11
     *     and earlier versions.<br>The DrawingColorfilter type can be used in atomic services since API version 12. For
     *     SVG sources, the effect only applies when the **stroke** property is set (regardless of the value).<br>Since
     *     API version 21, when [supportSvg2]{@link ImageAttribute#supportSvg2} is set to **true**, **colorFilter** takes
     *     effect on the entire SVG image source. [since 12]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    colorFilter(value: ColorFilter | DrawingColorFilter): ImageAttribute;
    /**
     * Sets the color filter for the image.
     *
     * <p><strong>NOTE</strong>:
     * <br>When this attribute is set, renderMode is not effective.
     * <br>When value is ResourceColor type, it will be converted to ColorFilter with blend mode.
     * </p>
     *
     * @param { ColorFilter | DrawingColorFilter | ResourceColor } value - Color filter of image.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 26.0.0
     */
    colorFilter(value: ColorFilter | DrawingColorFilter | ResourceColor): ImageAttribute;
    /**
     * Specifies whether the image can be copied. When **copyOption** is set to a value other than **CopyOptions.None**,
     * the image can be copied through multiple interactions, such as long press, right-click, or Ctrl+C. SVG images
     * cannot be copied.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { CopyOptions } value - Specifies whether the image can be copied.<br>Default value: **CopyOptions.None**
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    copyOption(value: CopyOptions): ImageAttribute;
    /**
     * Specifies whether the image is draggable.
     *
     * @param { boolean } value - Whether the image is draggable. The value **true** means that the image is draggable, in
     *     which case the bound long press gesture will not take effect.<br>Default value:<br>API version 9 and earlier:
     *     **false**<br> Since API version 10: **true**<br> To bind custom gestures to the component, set **draggable** to
     *     **false**. With the value **false**, drag-related events are not triggered.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 18]
     * @atomicservice [since 11]
     * @since 9
     */
    draggable(value: boolean): ImageAttribute;
    /**
     * This callback is triggered when an image is successfully loaded.
     * The size of the image source that is successfully loaded is returned, in pixels.
     *
     * @param { function } callback
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @since 7
     */
    /**
     * This callback is triggered when an image is successfully loaded.
     * The size of the image source that is successfully loaded is returned, in pixels.
     *
     * @param { function } callback
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @form
     * @since 9
     */
    /**
     * This callback is triggered when an image is successfully loaded.
     * The size of the image source that is successfully loaded is returned, in pixels.
     *
     * @param { function } callback
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @since 10
     */
    /**
     * Triggered when an image is successfully loaded or decoded.
     * The size of the image source that is successfully loaded is returned, in pixels.
     *
     * <p><strong>NOTE</strong>:
     * <br>This event is not triggered if the parameter type of the component is AnimatedDrawableDescriptor.
     * </p>
     *
     * @param { function } callback
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform
     * @form
     * @atomicservice
     * @since 11
     */
    onComplete(callback: (event?: {
        /**
         * The width of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 7
         */
        /**
         * The width of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 9
         * @form
         */
        /**
         * The width of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The width of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        width: number;
        /**
         * The height of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 7
         */
        /**
         * The height of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 9
         * @form
         */
        /**
         * The height of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The height of the image source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        height: number;
        /**
         * The width of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 7
         */
        /**
         * The width of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 9
         * @form
         */
        /**
         * The width of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The width of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        componentWidth: number;
        /**
         * The height of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 7
         */
        /**
         * The height of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 9
         * @form
         */
        /**
         * The height of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The height of the component source.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        componentHeight: number;
        /**
         * The value of the status of the image being loaded successfully.
         * If the returned status value is 0, the image data is successfully loaded.
         * If the returned status value is 1, the image is successfully decoded.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 7
         */
        /**
         * The value of the status of the image being loaded successfully.
         * If the returned status value is 0, the image data is successfully loaded.
         * If the returned status value is 1, the image is successfully decoded.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @since 9
         * @form
         */
        /**
         * The value of the status of the image being loaded successfully.
         * If the returned status value is 0, the image data is successfully loaded.
         * If the returned status value is 1, the image is successfully decoded.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The value of the status of the image being loaded successfully.
         * If the returned status value is 0, the image data is successfully loaded.
         * If the returned status value is 1, the image is successfully decoded.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        loadingStatus: number;
        /**
         * The width of the picture that is actually drawn.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The width of the picture that is actually drawn.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        contentWidth: number;
        /**
         * The height of the picture that is actually drawn.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The height of the picture that is actually drawn.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        contentHeight: number;
        /**
         * The actual draw is offset from the x-axis of the component itself.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The actual draw is offset from the x-axis of the component itself.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        contentOffsetX: number;
        /**
         * The actual draw is offset from the y-axis of the component itself.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @since 10
         * @form
         */
        /**
         * The actual draw is offset from the y-axis of the component itself.
         *
         * @type { number }
         * @syscap SystemCapability.ArkUI.ArkUI.Full
         * @stagemodelonly
         * @crossplatform
         * @atomicservice
         * @since 11
         * @form
         */
        contentOffsetY: number;
    }) => void): ImageAttribute;
    /**
     * Triggered when an error occurs during image loading.
     *
     * This event is not triggered if the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { function } callback - Callback triggered when an error occurs during image loading.<br>**NOTE**<br>You are
     *     advised to use this callback to quickly identify the cause of image loading failures. For details, see the
     *     [ImageError]{@link ImageError} error codes. [since 9 - 10]
     * @param { ImageErrorCallback } callback - Callback triggered when an error occurs during image loading.
     *     **NOTE**<
     *     You are advised to use this callback to quickly identify the cause of image loading failures. For details,
     *     see the [ImageError]{@link ImageError} error codes. [since 11]
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    onError(callback: ImageErrorCallback): ImageAttribute;
    /**
     * Triggered when the animation playback in the loaded SVG image is complete. If the animation is an infinite loop,
     * this callback is not triggered.
     *
     * Only images in SVG format are supported. This event is not triggered if the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * @param { function } event - Triggered when the animation playback in the loaded SVG image is complete. If the
     *     animation is an infinite loop, this callback is not triggered.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form [since 9]
     * @atomicservice [since 11]
     * @since 7
     */
    onFinish(event: () => void): ImageAttribute;
    /**
     * Sets whether to enable the AI image analyzer, which supports subject recognition, text recognition,
     * and object lookup.
     *
     * This attribute cannot be used together with the
     * [overlay](docroot://reference/apis-arkui/arkui-ts/ts-universal-attributes-overlay.md#overlay) attribute. If they
     * are set at the same time, the [CustomBuilder](docroot://reference/apis-arkui/arkui-ts/ts-types.md#custombuilder8)
     * attribute in **overlay** has no effect. This feature also depends on device capabilities.
     *
     * Images to be analyzed must be static, non-vector images. That is, SVG and GIF images cannot be analyzed.
     * [Pixel maps]{@link @ohos.multimedia.image:image.PixelMap} in
     * [RGBA_8888]{@link @ohos.multimedia.image:image.PixelMapFormat} format can be passed in for analysis. For details,
     * see
     * [Example 5: Enabling the AI Image Analyzer]
     *  (docroot://reference/apis-arkui/arkui-ts/ts-basic-components-image.md#example-5-enabling-the-ai-image-analyzer).
     *
     *
     * The [alt]{@link ImageAttribute#alt(value: string | Resource | PixelMap)} placeholder image does not support
     * analysis. The [objectRepeat]{@link ImageAttribute#objectRepeat} attribute supports analysis only when it is set to
     * **ImageRepeat.NoRepeat**. Analysis is not supported when the [obscured]{@link CommonMethod#obscured} attribute is
     * enabled.
     *
     * Analysis is performed based on the complete original image. Even if the settings of the
     * [clip]{@link CommonMethod#clip(value: boolean)}, [margin]{@link CommonMethod#margin},
     * [borderRadius]{@link CommonMethod#borderRadius(value: Length | BorderRadiuses | LocalizedBorderRadiuses)},
     * [position]{@link CommonMethod#position}, and [objectFit]{@link ImageAttribute#objectFit} attributes cause
     * incomplete image display, or if a mask layer is set via [renderMode]{@link ImageAttribute#renderMode}, analysis
     * will still be conducted on the complete original image. The [copyOption]{@link ImageAttribute#copyOption} attribute
     * does not affect the AI image analyzer functionality.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
     *
     * > **NOTE**
     * >
     * > - The **ohos.permission.INTERNET** permission is required.
     * >
     * > - This API can be called within [attributeModifier]{@link CommonMethod#attributeModifier} since API version 12.
     *
     * @param { boolean} enable - Whether the **Image** component supports AI analysis.<br>When this parameter is set to
     *     **true**, the **Image** component supports AI analysis. When this parameter is set to **false**, the **Image**
     *     component does not support AI analysis.<br>Default value: **false**
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @atomicservice [since 12]
     * @since 11
     */
    enableAnalyzer(enable: boolean): ImageAttribute;
    /**
     * Sets the resizable image options. Resizing is effective for drag previews and placeholder images.
     *
     * When a valid [ResizableOptions]{@link ResizableOptions} is set, the **objectRepeat**, **antialiased**, and
     * **orientation** attributes do not take effect.
     *
     * When the sum of the values of **top** and **bottom** is greater than the source image height, or the sum of the
     * values of **left** and **right** is greater than the source image width, the
     * [ResizableOptions]{@link ResizableOptions} attribute does not take effect.
     *
     * This attribute does not take effect when the parameter type of the component is
     * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor} or the image format
     * is SVG.
     *
     * > **NOTE**
     * >
     * > This API can be called in [attributeModifier]{@link CommonMethod#attributeModifier} since API version 20.
     *
     * @param { ResizableOptions } value - Resizable image options.
     * @returns { ImageAttribute } Returns the instance of the ImageAttribute.
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    resizable(value: ResizableOptions): ImageAttribute;
    /**
     * Sets whether to secure sensitive information on widgets.
     *
     * @param { boolean } supported - Whether to secure sensitive information on widgets.<br>**false** (default): Do not
     *     secure sensitive information on widgets.<br>**true**: Secure sensitive information on widgets, obscuring the
     *     image with a semi-transparent background style in privacy mode.<br>**NOTE**<br>If this parameter is set to
     *     **null**, the image is not obscured.<br>Privacy mode requires support from the widget framework.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @form
     * @atomicservice
     * @since 12
     */
    privacySensitive(supported: boolean): ImageAttribute;
    /**
     * Sets the display orientation of the image content.
     *
     * This attribute does not apply to placeholder images specified by
     * [alt]{@link ImageAttribute#alt(value: string | Resource | PixelMap)}.
     *
     * @param { ImageRotateOrientation } orientation - Display orientation of the image content.<br>Only static pixel map
     *     display is supported.<br>For images containing rotation or flip information, use
     *     **ImageRotateOrientation.AUTO**.<br>Default value: **ImageRotateOrientation.UP**<br>When this parameter is set
     *     to **undefined** or **null**, the value is **ImageRotateOrientation.AUTO**.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form [since 23]
     * @atomicservice
     * @since 14
     */
    orientation(orientation: ImageRotateOrientation): ImageAttribute;
    /**
     * Sets whether to enable
     * [enhanced SVG tag parsing](docroot://reference/apis-arkui/arkui-ts/ts-image-svg2-capabilities.md). When this
     * feature is enabled, SVG image rendering behavior changes accordingly.
     *
     * After the **Image** component is created, the value of this attribute cannot be dynamically changed.
     *
     * @param { boolean } enable - Determines whether to enable the enhanced SVG tag parsing feature.<br>Default value:
     *     **false**<br>**true**: Enable enhanced SVG parsing.<br>**false**: Use original SVG parsing.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 21
     */
    supportSvg2(enable: boolean): ImageAttribute;
    /**
     * Triggers transition animations when the image content changes.
     *
     * @param { ContentTransitionEffect } transition - Type of transition animation.<br>The value
     *     **ContentTransitionEffect.OPACITY** indicates the fade-in/fade-out effect, and the value
     *     **ContentTransitionEffect.IDENTITY** indicates no animation effect.<br>Default value:
     *     **ContentTransitionEffect.IDENTITY**<br>When this parameter is set to **undefined** or **null**, the value
     *     defaults to **ContentTransitionEffect.IDENTITY**.<br>Note: This parameter does not take effect for dynamic
     *     image resources.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 21
     */
    contentTransition(transition: ContentTransitionEffect): ImageAttribute;
    /**
     * Sets whether to enable anti-aliasing for the edges of a pixel map image. If the attribute is not set, anti-aliasing
     * is disabled by default. This attribute is not applicable to SVG images.
     *
     * > **NOTE**
     * >
     * > If the [backgroundColor]{@link CommonMethod#backgroundColor(value: ResourceColor)} attribute is set for an image,
     * > setting the **antialiased** attribute of the image to **true** does not affect the aliasing effect of the
     * > background color.
     * >
     * > This attribute does not take effect when used together with [resizable]{@link ImageAttribute#resizable}.
     *
     * @param { Optional<boolean> } isAntialiased - Sets whether to enable anti-aliasing for the edges of a pixel map
     *     image.<br> **true**: Enable edge anti-aliasing.<br>**false**: Disable edge anti-aliasing.<br>When this
     *     parameter is set to **undefined**, edge anti-aliasing is disabled.
     * @returns { ImageAttribute }
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 23
     */
    antialiased(isAntialiased: Optional<boolean>): ImageAttribute;
}
/**
 * The **Image** component is usually used to display images in applications. It supports data sources of the following
 * types: [PixelMap]{@link @ohos.multimedia.image:image.PixelMap}, [ResourceStr]{@link ResourceStr}, and
 * [DrawableDescriptor]{@link DrawableDescriptor}. Supported image formats include PNG, JPG, JPEG, BMP, SVG, WEBP, GIF,
 * HEIF, and TIFF. Note that the APNG and SVGA formats are not supported.
 *
 * > **NOTE**
 *
 * > - This component supports the TIFF image format since API version 23.
 * >
 * > - When keyboard shortcuts are used to copy an **Image** component, the **Image** component must be in a focused
 * > state. For instructions on how to set focus, see
 * > [Setting Whether a Component Is Focusable]
 * >  (docroot://ui/arkts-common-events-focus-event.md#setting-whether-a-component-is-focusable).
 * > By default, the **Image** component is not focusable. To enable it to gain focus, set both the
 * > [focusable]{@link CommonMethod#focusable} and [focusOnTouch]{@link CommonMethod#focusOnTouch} attributes to
 * > **true**.
 * >
 * > - The **Image** component supports SVG image sources. For details about SVG tags, see [SVG Tags]{@link ./common}.
 * >
 * > - For animated images, animation playback is disabled by default and depends on the visibility of the **Image**
 * > component. When the component is visible, the animation is started through the callback. When the component is
 * > invisible, the animation is stopped. The visibility status of the **Image** component can be identified through the
 * >
 * > [onVisibleAreaChange]
 * >  {@link CommonMethod#onVisibleAreaChange(ratios: Array<number>, event: VisibleAreaChangeCallback)}
 * > event. If the value of **ratios** is greater than 0, the component is visible.
 * >
 * > - For details about how to resolve white block issues during image loading, see
 * > [Solution to White Image Blocks]
 * >  (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-image-white-lump-solution).
 * > For details about how to address slow image loading, see
 * > [Optimizing Preset Image Loading]
 * >  (https://developer.huawei.com/consumer/en/doc/best-practices/bpta-texture-compression-improve-
 * >   performance#section91526132216).
 * >
 *
 * Required Permissions
 *
 * The **ohos.permission.INTERNET** permission is required for using online images. For details about how to apply for a
 * permission, see [Declaring Permissions](docroot://security/AccessToken/declare-permissions.md).
 *
 * Child Components
 *
 * Not supported
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 */
declare const Image: ImageInterface;
/**
 * Defines Image Component instance.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form [since 9]
 * @atomicservice [since 11]
 * @since 7
 * @noninterop [since 11]
 */
declare const ImageInstance: ImageAttribute;
/**
 * Triggered when an error occurs during image loading.
 *
 * This event is not triggered if the parameter type of the component is
 * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
 *
 * @type ImageErrorCallback [since 9 - 10]
 * @param { ImageError } error - Object returned by the callback triggered when an exception occurs during image
 *     loading. [since 11]
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form
 * @atomicservice [since 11]
 * @since 9
 */
type ImageErrorCallback = (error: ImageError) => void;
/**
 * Describes the object returned by the image loading error callback.
 *
 * This event is not triggered if the parameter type of the component is
 * [AnimatedDrawableDescriptor]{@link @ohos.arkui.drawableDescriptor:AnimatedDrawableDescriptor}.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @FaAndStageModel
 * @crossplatform [since 10]
 * @form
 * @atomicservice [since 11]
 * @since 9
 */
declare interface ImageError {
    /**
     * Width of the component.
     *
     * Unit: px
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    componentWidth: number;
    /**
     * Height of the component.
     *
     * Unit: px
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @FaAndStageModel
     * @crossplatform [since 10]
     * @form
     * @atomicservice [since 11]
     * @since 9
     */
    componentHeight: number;
    /**
     * Error information.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice [since 11]
     * @since 10
     */
    message: string;
    /**
     * Error information returned when an error occurs during image loading, where **code** represents the error code and
     * **message** indicates the error message. For details, see the error code reference below.
     *
     * Default value: **{ code : -1, message : "" }**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 20
     */
    error?: BusinessError<void>;
    /**
     * Detailed information about online image download, including download resources, network, and performance. This
     * field is carried when the image source is an online image and the download fails.
     *
     * Default value: **null**
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 23
     */
    downloadInfo?: RequestDownloadInfo;
}
/**
 * Defines the resizable image options.
 *
 * **Figure 1** Effect of Setting EdgeWidths
 * ![edgewidths](docroot://reference/apis-arkui/arkui-ts/figures/edgewidths.png)
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @atomicservice [since 12]
 * @since 11
 */
declare interface ResizableOptions {
    /**
     * Edge widths in different directions of a component.
     *
     * **NOTE**
     *
     * This attribute takes effect only when both **bottom** and **right** are greater than 0.
     *
     * When **top** is set, the top part of the image is stretched while the pixel values of the image remain unchanged.
     *
     * When **right** is set, the right part of the image is stretched while the pixel values of the image remain
     * unchanged.
     *
     * When **bottom** is set, the bottom part of the image is stretched while the pixel values of the image remain
     * unchanged.
     *
     * When **left** is set, the left part of the image is stretched while the pixel values of the image remain unchanged.
     *
     * The default width of each direction is **0**. The default unit is vp.
     *
     * The effect of setting **EdgeWidths** is shown in Figure 1 (Effect of Setting EdgeWidths).
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice [since 12]
     * @since 11
     */
    slice?: EdgeWidths;
    /**
     * Lattice object, which is used to divide the image by lattice.
     *
     * **NOTE**
     *
     * Use the
     * [createImageLattice]{@link @ohos.graphics.drawing:drawing.Lattice.createImageLattice(xDivs: Array<number>,
     *  yDivs: Array<number>, fXCount: number, fYCount: number, fBounds?: common2D.Rect | null, fRectTypes?:
     *  Array<RectType> | null, fColors?: Array<common2D.Color> | null)}
     * API of **@ohos.graphics.drawing** to create a **Lattice** type as the input parameter. Lattices located at both
     *  even columns and even rows are fixed; those at other positions are stretched according to **slice**.
     *
     * This parameter does not take effect for the [backgroundImageResizable]{@link CommonMethod#backgroundImageResizable}
     * API.
     *
     * When a number is passed, the default unit is px.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @atomicservice
     * @since 12
     */
    lattice?: DrawingLattice;
}
/**
 * Sets the placeholder image.
 *
 * @syscap SystemCapability.ArkUI.ArkUI.Full
 * @stagemodelonly
 * @crossplatform
 * @form
 * @atomicservice
 * @since 22
 */
declare interface ImageAlt {
    /**
     * Placeholder image displayed during loading.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    placeholder?: ResourceStr | PixelMap;
    /**
     * Placeholder image displayed when loading fails.
     *
     * @syscap SystemCapability.ArkUI.ArkUI.Full
     * @stagemodelonly
     * @crossplatform
     * @form
     * @atomicservice
     * @since 22
     */
    error?: ResourceStr | PixelMap;
}
