using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PixelMapFormat 枚举
/// </summary>
public enum PixelMapFormat
{
    Unknown = 0,
    Argb8888 = 1,
    Rgb565 = 2,
    Rgba8888 = 3,
    Bgra8888 = 4,
    Rgb888 = 5,
    Alpha8 = 6,
    RgbaF16 = 7,
    Nv21 = 8,
    Nv12 = 9,
    Rgba1010102 = 10,
    YcbcrP010 = 11,
    YcrcbP010 = 12,
    Y8 = 14,
    AlphaU8 = 15,
    AlphaF16 = 16,
    Astc4x4 = 102
}

/// <summary>
/// PropertyKey 枚举
/// </summary>
public enum PropertyKey
{
    [Description("BitsPerSample")]
    BitsPerSample,
    [Description("Orientation")]
    Orientation,
    [Description("ImageLength")]
    ImageLength,
    [Description("ImageWidth")]
    ImageWidth,
    [Description("GPSLatitude")]
    GpsLatitude,
    [Description("GPSLongitude")]
    GpsLongitude,
    [Description("GPSLatitudeRef")]
    GpsLatitudeRef,
    [Description("GPSLongitudeRef")]
    GpsLongitudeRef,
    [Description("DateTimeOriginal")]
    DateTimeOriginal,
    [Description("ExposureTime")]
    ExposureTime,
    [Description("SceneType")]
    SceneType,
    [Description("ISOSpeedRatings")]
    IsoSpeedRatings,
    [Description("FNumber")]
    FNumber,
    [Description("DateTime")]
    DateTime,
    [Description("GPSTimeStamp")]
    GpsTimeStamp,
    [Description("GPSDateStamp")]
    GpsDateStamp,
    [Description("ImageDescription")]
    ImageDescription,
    [Description("Make")]
    Make,
    [Description("Model")]
    Model,
    [Description("PhotoMode")]
    PhotoMode,
    [Description("SensitivityType")]
    SensitivityType,
    [Description("StandardOutputSensitivity")]
    StandardOutputSensitivity,
    [Description("RecommendedExposureIndex")]
    RecommendedExposureIndex,
    [Description("ISOSpeedRatings")]
    IsoSpeed,
    [Description("ApertureValue")]
    ApertureValue,
    [Description("ExposureBiasValue")]
    ExposureBiasValue,
    [Description("MeteringMode")]
    MeteringMode,
    [Description("LightSource")]
    LightSource,
    [Description("Flash")]
    Flash,
    [Description("FocalLength")]
    FocalLength,
    [Description("UserComment")]
    UserComment,
    [Description("PixelXDimension")]
    PixelXDimension,
    [Description("PixelYDimension")]
    PixelYDimension,
    [Description("WhiteBalance")]
    WhiteBalance,
    [Description("FocalLengthIn35mmFilm")]
    FocalLengthIn35MmFilm,
    [Description("HwMnoteCaptureMode")]
    CaptureMode,
    [Description("HwMnotePhysicalAperture")]
    PhysicalAperture,
    [Description("HwMnoteRollAngle")]
    RollAngle,
    [Description("HwMnotePitchAngle")]
    PitchAngle,
    [Description("HwMnoteSceneFoodConf")]
    SceneFoodConf,
    [Description("HwMnoteSceneStageConf")]
    SceneStageConf,
    [Description("HwMnoteSceneBlueSkyConf")]
    SceneBlueSkyConf,
    [Description("HwMnoteSceneGreenPlantConf")]
    SceneGreenPlantConf,
    [Description("HwMnoteSceneBeachConf")]
    SceneBeachConf,
    [Description("HwMnoteSceneSnowConf")]
    SceneSnowConf,
    [Description("HwMnoteSceneSunsetConf")]
    SceneSunsetConf,
    [Description("HwMnoteSceneFlowersConf")]
    SceneFlowersConf,
    [Description("HwMnoteSceneNightConf")]
    SceneNightConf,
    [Description("HwMnoteSceneTextConf")]
    SceneTextConf,
    [Description("HwMnoteFaceCount")]
    FaceCount,
    [Description("HwMnoteFocusMode")]
    FocusMode,
    [Description("Compression")]
    Compression,
    [Description("PhotometricInterpretation")]
    PhotometricInterpretation,
    [Description("StripOffsets")]
    StripOffsets,
    [Description("SamplesPerPixel")]
    SamplesPerPixel,
    [Description("RowsPerStrip")]
    RowsPerStrip,
    [Description("StripByteCounts")]
    StripByteCounts,
    [Description("XResolution")]
    XResolution,
    [Description("YResolution")]
    YResolution,
    [Description("PlanarConfiguration")]
    PlanarConfiguration,
    [Description("ResolutionUnit")]
    ResolutionUnit,
    [Description("TransferFunction")]
    TransferFunction,
    [Description("Software")]
    Software,
    [Description("Artist")]
    Artist,
    [Description("WhitePoint")]
    WhitePoint,
    [Description("PrimaryChromaticities")]
    PrimaryChromaticities,
    [Description("YCbCrCoefficients")]
    YcbcrCoefficients,
    [Description("YCbCrSubSampling")]
    YcbcrSubSampling,
    [Description("YCbCrPositioning")]
    YcbcrPositioning,
    [Description("ReferenceBlackWhite")]
    ReferenceBlackWhite,
    [Description("Copyright")]
    Copyright,
    [Description("JPEGInterchangeFormat")]
    JpegInterchangeFormat,
    [Description("JPEGInterchangeFormatLength")]
    JpegInterchangeFormatLength,
    [Description("ExposureProgram")]
    ExposureProgram,
    [Description("SpectralSensitivity")]
    SpectralSensitivity,
    [Description("OECF")]
    Oecf,
    [Description("ExifVersion")]
    ExifVersion,
    [Description("DateTimeDigitized")]
    DateTimeDigitized,
    [Description("ComponentsConfiguration")]
    ComponentsConfiguration,
    [Description("ShutterSpeedValue")]
    ShutterSpeed,
    [Description("BrightnessValue")]
    BrightnessValue,
    [Description("MaxApertureValue")]
    MaxApertureValue,
    [Description("SubjectDistance")]
    SubjectDistance,
    [Description("SubjectArea")]
    SubjectArea,
    [Description("MakerNote")]
    MakerNote,
    [Description("SubsecTime")]
    SubsecTime,
    [Description("SubsecTimeOriginal")]
    SubsecTimeOriginal,
    [Description("SubsecTimeDigitized")]
    SubsecTimeDigitized,
    [Description("FlashpixVersion")]
    FlashpixVersion,
    [Description("ColorSpace")]
    ColorSpace,
    [Description("RelatedSoundFile")]
    RelatedSoundFile,
    [Description("FlashEnergy")]
    FlashEnergy,
    [Description("SpatialFrequencyResponse")]
    SpatialFrequencyResponse,
    [Description("FocalPlaneXResolution")]
    FocalPlaneXResolution,
    [Description("FocalPlaneYResolution")]
    FocalPlaneYResolution,
    [Description("FocalPlaneResolutionUnit")]
    FocalPlaneResolutionUnit,
    [Description("SubjectLocation")]
    SubjectLocation,
    [Description("ExposureIndex")]
    ExposureIndex,
    [Description("SensingMethod")]
    SensingMethod,
    [Description("FileSource")]
    FileSource,
    [Description("CFAPattern")]
    CfaPattern,
    [Description("CustomRendered")]
    CustomRendered,
    [Description("ExposureMode")]
    ExposureMode,
    [Description("DigitalZoomRatio")]
    DigitalZoomRatio,
    [Description("SceneCaptureType")]
    SceneCaptureType,
    [Description("GainControl")]
    GainControl,
    [Description("Contrast")]
    Contrast,
    [Description("Saturation")]
    Saturation,
    [Description("Sharpness")]
    Sharpness,
    [Description("DeviceSettingDescription")]
    DeviceSettingDescription,
    [Description("SubjectDistanceRange")]
    SubjectDistanceRange,
    [Description("ImageUniqueID")]
    ImageUniqueId,
    [Description("GPSVersionID")]
    GpsVersionId,
    [Description("GPSAltitudeRef")]
    GpsAltitudeRef,
    [Description("GPSAltitude")]
    GpsAltitude,
    [Description("GPSSatellites")]
    GpsSatellites,
    [Description("GPSStatus")]
    GpsStatus,
    [Description("GPSMeasureMode")]
    GpsMeasureMode,
    [Description("GPSDOP")]
    GpsDop,
    [Description("GPSSpeedRef")]
    GpsSpeedRef,
    [Description("GPSSpeed")]
    GpsSpeed,
    [Description("GPSTrackRef")]
    GpsTrackRef,
    [Description("GPSTrack")]
    GpsTrack,
    [Description("GPSImgDirectionRef")]
    GpsImgDirectionRef,
    [Description("GPSImgDirection")]
    GpsImgDirection,
    [Description("GPSMapDatum")]
    GpsMapDatum,
    [Description("GPSDestLatitudeRef")]
    GpsDestLatitudeRef,
    [Description("GPSDestLatitude")]
    GpsDestLatitude,
    [Description("GPSDestLongitudeRef")]
    GpsDestLongitudeRef,
    [Description("GPSDestLongitude")]
    GpsDestLongitude,
    [Description("GPSDestBearingRef")]
    GpsDestBearingRef,
    [Description("GPSDestBearing")]
    GpsDestBearing,
    [Description("GPSDestDistanceRef")]
    GpsDestDistanceRef,
    [Description("GPSDestDistance")]
    GpsDestDistance,
    [Description("GPSProcessingMethod")]
    GpsProcessingMethod,
    [Description("GPSAreaInformation")]
    GpsAreaInformation,
    [Description("GPSDifferential")]
    GpsDifferential,
    [Description("BodySerialNumber")]
    BodySerialNumber,
    [Description("CameraOwnerName")]
    CameraOwnerName,
    [Description("CompositeImage")]
    CompositeImage,
    [Description("CompressedBitsPerPixel")]
    CompressedBitsPerPixel,
    [Description("DNGVersion")]
    DngVersion,
    [Description("DefaultCropSize")]
    DefaultCropSize,
    [Description("Gamma")]
    Gamma,
    [Description("ISOSpeedLatitudeyyy")]
    IsoSpeedLatitudeYyy,
    [Description("ISOSpeedLatitudezzz")]
    IsoSpeedLatitudeZzz,
    [Description("LensMake")]
    LensMake,
    [Description("LensModel")]
    LensModel,
    [Description("LensSerialNumber")]
    LensSerialNumber,
    [Description("LensSpecification")]
    LensSpecification,
    [Description("NewSubfileType")]
    NewSubfileType,
    [Description("OffsetTime")]
    OffsetTime,
    [Description("OffsetTimeDigitized")]
    OffsetTimeDigitized,
    [Description("OffsetTimeOriginal")]
    OffsetTimeOriginal,
    [Description("SourceExposureTimesOfCompositeImage")]
    SourceExposureTimesOfCompositeImage,
    [Description("SourceImageNumberOfCompositeImage")]
    SourceImageNumberOfCompositeImage,
    [Description("SubfileType")]
    SubfileType,
    [Description("GPSHPositioningError")]
    GpsHPositioningError,
    [Description("PhotographicSensitivity")]
    PhotographicSensitivity,
    [Description("HwMnoteBurstNumber")]
    BurstNumber,
    [Description("HwMnoteFaceConf")]
    FaceConf,
    [Description("HwMnoteFaceLeyeCenter")]
    FaceLeyeCenter,
    [Description("HwMnoteFaceMouthCenter")]
    FaceMouthCenter,
    [Description("HwMnoteFacePointer")]
    FacePointer,
    [Description("HwMnoteFaceRect")]
    FaceRect,
    [Description("HwMnoteFaceReyeCenter")]
    FaceReyeCenter,
    [Description("HwMnoteFaceSmileScore")]
    FaceSmileScore,
    [Description("HwMnoteFaceVersion")]
    FaceVersion,
    [Description("HwMnoteFrontCamera")]
    FrontCamera,
    [Description("HwMnoteScenePointer")]
    ScenePointer,
    [Description("HwMnoteSceneVersion")]
    SceneVersion,
    [Description("HwMnoteIsXmageSupported")]
    IsXmageSupported,
    [Description("HwMnoteXmageMode")]
    XmageMode,
    [Description("HwMnoteXmageLeft")]
    XmageLeft,
    [Description("HwMnoteXmageTop")]
    XmageTop,
    [Description("HwMnoteXmageRight")]
    XmageRight,
    [Description("HwMnoteXmageBottom")]
    XmageBottom,
    [Description("HwMnoteCloudEnhancementMode")]
    CloudEnhancementMode,
    [Description("HwMnoteWindSnapshotMode")]
    WindSnapshotMode,
    [Description("GIFLoopCount")]
    GifLoopCount
}

/// <summary>
/// ImageFormat 枚举
/// </summary>
public enum ImageFormat
{
    Ycbcr422Sp = 1000,
    Jpeg = 2000
}

/// <summary>
/// AlphaType 枚举
/// </summary>
public enum AlphaType
{
    Unknown = 0,
    Opaque = 1,
    Premul = 2,
    Unpremul = 3
}

/// <summary>
/// DecodingDynamicRange 枚举
/// </summary>
public enum DecodingDynamicRange
{
    Auto = 0,
    Sdr = 1,
    Hdr = 2
}

/// <summary>
/// PackingDynamicRange 枚举
/// </summary>
public enum PackingDynamicRange
{
    Auto = 0,
    Sdr = 1
}

/// <summary>
/// AntiAliasingLevel 枚举
/// </summary>
public enum AntiAliasingLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3
}

/// <summary>
/// ScaleMode 枚举
/// </summary>
public enum ScaleMode
{
    FitTargetSize = 0,
    CenterCrop = 1
}

/// <summary>
/// ComponentType 枚举
/// </summary>
public enum ComponentType
{
    YuvY = 1,
    YuvU = 2,
    YuvV = 3,
    Jpeg = 4
}

/// <summary>
/// HdrMetadataKey 枚举
/// </summary>
public enum HdrMetadataKey
{
    HdrMetadataType = 0,
    HdrStaticMetadata = 1,
    HdrDynamicMetadata = 2,
    HdrGainmapMetadata = 3
}

/// <summary>
/// HdrMetadataType 枚举
/// </summary>
public enum HdrMetadataType
{
    None = 0,
    Base = 1,
    Gainmap = 2,
    Alternate = 3
}

/// <summary>
/// AllocatorType 枚举
/// </summary>
public enum AllocatorType
{
    Auto = 0,
    Dma = 1,
    ShareMemory = 2
}

/// <summary>
/// CropAndScaleStrategy 枚举
/// </summary>
public enum CropAndScaleStrategy
{
    ScaleFirst = 1,
    CropFirst = 2
}

/// <summary>
/// AuxiliaryPictureType 枚举
/// </summary>
public enum AuxiliaryPictureType
{
    Gainmap = 1,
    DepthMap = 2,
    UnrefocusMap = 3,
    LinearMap = 4,
    FragmentMap = 5,
    LhdrGainmap = 10
}

/// <summary>
/// MetadataType 枚举
/// </summary>
public enum MetadataType
{
    ExifMetadata = 1,
    FragmentMetadata = 2,
    GifMetadata = 5,
    HeifsMetadata = 15,
    DngMetadata = 16,
    WebpMetadata = 17,
    PngMetadata = 19,
    JfifMetadata = 20,
    TiffMetadata = 21,
    XmpMetadata = 22,
    AvisMetadata = 23
}

/// <summary>
/// FragmentMapPropertyKey 枚举
/// </summary>
public enum FragmentMapPropertyKey
{
    [Description("XInOriginal")]
    XInOriginal,
    [Description("YInOriginal")]
    YInOriginal,
    [Description("FragmentImageWidth")]
    Width,
    [Description("FragmentImageHeight")]
    Height
}

/// <summary>
/// GifPropertyKey 枚举
/// </summary>
public enum GifPropertyKey
{
    [Description("GifDelayTime")]
    GifDelayTime,
    [Description("GifDisposalType")]
    GifDisposalType,
    [Description("GifHasGlobalColorMap")]
    GifHasGlobalColorMap,
    [Description("GifCanvasWidth")]
    GifCanvasWidth,
    [Description("GifCanvasHeight")]
    GifCanvasHeight,
    [Description("GifLoopCount")]
    GifLoopCount,
    [Description("GifUnclampedDelayTime")]
    GifUnclampedDelayTime
}

/// <summary>
/// HeifsPropertyKey 枚举
/// </summary>
public enum HeifsPropertyKey
{
    [Description("HeifsDelayTime")]
    HeifsDelayTime,
    [Description("HeifsUnclampedDelayTime")]
    HeifsUnclampedDelayTime,
    [Description("HeifsCanvasHeight")]
    HeifsCanvasHeight,
    [Description("HeifsCanvasWidth")]
    HeifsCanvasWidth
}

/// <summary>
/// DngPropertyKey 枚举
/// </summary>
public enum DngPropertyKey
{
    [Description("DNGVersion")]
    DngVersion,
    [Description("DNGBackwardVersion")]
    DngBackwardVersion,
    [Description("UniqueCameraModel")]
    UniqueCameraModel,
    [Description("LocalizedCameraModel")]
    LocalizedCameraModel,
    [Description("CFAPlaneColor")]
    CfaPlaneColor,
    [Description("CFALayout")]
    CfaLayout,
    [Description("LinearizationTable")]
    LinearizationTable,
    [Description("BlackLevelRepeatDim")]
    BlackLevelRepeatDim,
    [Description("BlackLevel")]
    BlackLevel,
    [Description("BlackLevelDeltaH")]
    BlackLevelDeltaH,
    [Description("BlackLevelDeltaV")]
    BlackLevelDeltaV,
    [Description("WhiteLevel")]
    WhiteLevel,
    [Description("DefaultScale")]
    DefaultScale,
    [Description("DefaultCropOrigin")]
    DefaultCropOrigin,
    [Description("DefaultCropSize")]
    DefaultCropSize,
    [Description("ColorMatrix1")]
    ColorMatrix1,
    [Description("ColorMatrix2")]
    ColorMatrix2,
    [Description("CameraCalibration1")]
    CameraCalibration1,
    [Description("CameraCalibration2")]
    CameraCalibration2,
    [Description("ReductionMatrix1")]
    ReductionMatrix1,
    [Description("ReductionMatrix2")]
    ReductionMatrix2,
    [Description("AnalogBalance")]
    AnalogBalance,
    [Description("AsShotNeutral")]
    AsShotNeutral,
    [Description("AsShotWhiteXY")]
    AsShotWhitexy,
    [Description("BaselineExposure")]
    BaselineExposure,
    [Description("BaselineNoise")]
    BaselineNoise,
    [Description("BaselineSharpness")]
    BaselineSharpness,
    [Description("BayerGreenSplit")]
    BayerGreenSplit,
    [Description("LinearResponseLimit")]
    LinearResponseLimit,
    [Description("CameraSerialNumber")]
    CameraSerialNumber,
    [Description("LensInfo")]
    LensInfo,
    [Description("ChromaBlurRadius")]
    ChromaBlurRadius,
    [Description("AntiAliasStrength")]
    AntiAliasStrength,
    [Description("ShadowScale")]
    ShadowScale,
    [Description("DNGPrivateData")]
    DngPrivateData,
    [Description("MakerNoteSafety")]
    MakerNoteSafety,
    [Description("CalibrationIlluminant1")]
    CalibrationIlluminant1,
    [Description("CalibrationIlluminant2")]
    CalibrationIlluminant2,
    [Description("BestQualityScale")]
    BestQualityScale,
    [Description("RawDataUniqueID")]
    RawDataUniqueId,
    [Description("OriginalRawFileName")]
    OriginalRawFileName,
    [Description("OriginalRawFileData")]
    OriginalRawFileData,
    [Description("ActiveArea")]
    ActiveArea,
    [Description("MaskedAreas")]
    MaskedAreas,
    [Description("AsShotICCProfile")]
    AsShotIccProfile,
    [Description("AsShotPreProfileMatrix")]
    AsShotPreProfileMatrix,
    [Description("CurrentICCProfile")]
    CurrentIccProfile,
    [Description("CurrentPreProfileMatrix")]
    CurrentPreProfileMatrix,
    [Description("ColorimetricReference")]
    ColorimetricReference,
    [Description("CameraCalibrationSignature")]
    CameraCalibrationSignature,
    [Description("ProfileCalibrationSignature")]
    ProfileCalibrationSignature,
    [Description("ExtraCameraProfiles")]
    ExtraCameraProfiles,
    [Description("AsShotProfileName")]
    AsShotProfileName,
    [Description("NoiseReductionApplied")]
    NoiseReductionApplied,
    [Description("ProfileName")]
    ProfileName,
    [Description("ProfileHueSatMapDims")]
    ProfileHueSatMapDims,
    [Description("ProfileHueSatMapData1")]
    ProfileHueSatMapData1,
    [Description("ProfileHueSatMapData2")]
    ProfileHueSatMapData2,
    [Description("ProfileToneCurve")]
    ProfileToneCurve,
    [Description("ProfileEmbedPolicy")]
    ProfileEmbedPolicy,
    [Description("ProfileCopyright")]
    ProfileCopyright,
    [Description("ForwardMatrix1")]
    ForwardMatrix1,
    [Description("ForwardMatrix2")]
    ForwardMatrix2,
    [Description("PreviewApplicationName")]
    PreviewApplicationName,
    [Description("PreviewApplicationVersion")]
    PreviewApplicationVersion,
    [Description("PreviewSettingsName")]
    PreviewSettingsName,
    [Description("PreviewSettingsDigest")]
    PreviewSettingsDigest,
    [Description("PreviewColorSpace")]
    PreviewColorSpace,
    [Description("PreviewDateTime")]
    PreviewDateTime,
    [Description("RawImageDigest")]
    RawImageDigest,
    [Description("OriginalRawFileDigest")]
    OriginalRawFileDigest,
    [Description("SubTileBlockSize")]
    SubTileBlockSize,
    [Description("RowInterleaveFactor")]
    RowInterleaveFactor,
    [Description("ProfileLookTableDims")]
    ProfileLookTableDims,
    [Description("ProfileLookTableData")]
    ProfileLookTableData,
    [Description("OpcodeList1")]
    OpcodeList1,
    [Description("OpcodeList2")]
    OpcodeList2,
    [Description("OpcodeList3")]
    OpcodeList3,
    [Description("NoiseProfile")]
    NoiseProfile,
    [Description("OriginalDefaultFinalSize")]
    OriginalDefaultFinalSize,
    [Description("OriginalBestQualityFinalSize")]
    OriginalBestQualityFinalSize,
    [Description("OriginalDefaultCropSize")]
    OriginalDefaultCropSize,
    [Description("ProfileHueSatMapEncoding")]
    ProfileHueSatMapEncoding,
    [Description("ProfileLookTableEncoding")]
    ProfileLookTableEncoding,
    [Description("BaselineExposureOffset")]
    BaselineExposureOffset,
    [Description("DefaultBlackRender")]
    DefaultBlackRender,
    [Description("NewRawImageDigest")]
    NewRawImageDigest,
    [Description("RawToPreviewGain")]
    RawToPreviewGain,
    [Description("DefaultUserCrop")]
    DefaultUserCrop
}

/// <summary>
/// TiffPropertyKey 枚举
/// </summary>
public enum TiffPropertyKey
{
    [Description("TiffCompression")]
    Compression,
    [Description("TiffPhotometricInterpretation")]
    PhotometricInterpretation,
    [Description("TiffTransferFunction")]
    TransferFunction,
    [Description("TiffOrientation")]
    Orientation,
    [Description("TiffXResolution")]
    XResolution,
    [Description("TiffYResolution")]
    YResolution,
    [Description("TiffResolutionUnit")]
    ResolutionUnit,
    [Description("TiffWhitePoint")]
    WhitePoint,
    [Description("TiffPrimaryChromaticities")]
    PrimaryChromaticities,
    [Description("TiffTileLength")]
    TileLength,
    [Description("TiffTileWidth")]
    TileWidth,
    [Description("TiffDocumentName")]
    DocumentName,
    [Description("TiffImageDescription")]
    ImageDescription,
    [Description("TiffArtist")]
    Artist,
    [Description("TiffCopyright")]
    Copyright,
    [Description("TiffDateTime")]
    DateTime,
    [Description("TiffMake")]
    Make,
    [Description("TiffModel")]
    Model,
    [Description("TiffSoftware")]
    Software,
    [Description("TiffHostComputer")]
    HostComputer
}

/// <summary>
/// JfifPropertyKey 枚举
/// </summary>
public enum JfifPropertyKey
{
    [Description("JfifXDensity")]
    XDensity,
    [Description("JfifYDensity")]
    YDensity,
    [Description("JfifDensityUnit")]
    DensityUnit,
    [Description("JfifVersion")]
    Version,
    [Description("JfifIsProgressive")]
    IsProgressive
}

/// <summary>
/// PngPropertyKey 枚举
/// </summary>
public enum PngPropertyKey
{
    [Description("PngXPixelsPerMeter")]
    XPixelsPerMeter,
    [Description("PngYPixelsPerMeter")]
    YPixelsPerMeter,
    [Description("PngGamma")]
    Gamma,
    [Description("PngInterlaceType")]
    InterlaceType,
    [Description("PngSRGBIntent")]
    SrgbIntent,
    [Description("PngChromaticities")]
    Chromaticities,
    [Description("PngTitle")]
    Title,
    [Description("PngDescription")]
    Description,
    [Description("PngComment")]
    Comment,
    [Description("PngDisclaimer")]
    Disclaimer,
    [Description("PngWarning")]
    Warning,
    [Description("PngAuthor")]
    Author,
    [Description("PngCopyright")]
    Copyright,
    [Description("PngCreationTime")]
    CreationTime,
    [Description("PngModificationTime")]
    ModificationTime,
    [Description("PngSoftware")]
    Software
}

/// <summary>
/// ImageOrientation 枚举
/// </summary>
public enum ImageOrientation
{
    TopLeft = 1,
    TopRight = 2,
    BottomRight = 3,
    BottomLeft = 4,
    LeftTop = 5,
    RightTop = 6,
    RightBottom = 7,
    LeftBottom = 8
}

/// <summary>
/// ImageFocusMode 枚举
/// </summary>
public enum ImageFocusMode
{
    AfA = 0,
    AfS = 1,
    AfC = 2,
    Mf = 3
}

/// <summary>
/// XmageColorMode 枚举
/// </summary>
public enum XmageColorMode
{
    Normal = 0,
    Bright = 1,
    Soft = 2,
    Mono = 3
}

/// <summary>
/// WebPPropertyKey 枚举
/// </summary>
public enum WebPPropertyKey
{
    [Description("WebPCanvasWidth")]
    CanvasWidth,
    [Description("WebPCanvasHeight")]
    CanvasHeight,
    [Description("WebPDelayTime")]
    DelayTime,
    [Description("WebPUnclampedDelayTime")]
    UnclampedDelayTime,
    [Description("WebPLoopCount")]
    LoopCount
}

/// <summary>
/// XMPTagType 枚举
/// </summary>
public enum XMPTagType
{
    Unknown = 0,
    String = 1,
    UnorderedArray = 2,
    OrderedArray = 3,
    AlternateArray = 4,
    AlternateText = 5,
    Structure = 6
}

/// <summary>
/// AvisPropertyKey 枚举
/// </summary>
public enum AvisPropertyKey
{
    [Description("AvisDelayTime")]
    DelayTime
}