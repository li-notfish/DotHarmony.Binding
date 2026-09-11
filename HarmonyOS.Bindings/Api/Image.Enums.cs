using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PixelMapFormat 枚举
/// </summary>
public enum PixelMapFormat
{
    UNKNOWN = 0,
    ARGB_8888 = 1,
    RGB_565 = 2,
    RGBA_8888 = 3,
    BGRA_8888 = 4,
    RGB_888 = 5,
    ALPHA_8 = 6,
    RGBA_F16 = 7,
    NV21 = 8,
    NV12 = 9,
    RGBA_1010102 = 10,
    YCBCR_P010 = 11,
    YCRCB_P010 = 12,
    Y8 = 14,
    ALPHA_U8 = 15,
    ALPHA_F16 = 16,
    ASTC_4x4 = 102
}

/// <summary>
/// PropertyKey 枚举
/// </summary>
public enum PropertyKey
{
    [Description("BitsPerSample")]
    BITS_PER_SAMPLE,
    [Description("Orientation")]
    ORIENTATION,
    [Description("ImageLength")]
    IMAGE_LENGTH,
    [Description("ImageWidth")]
    IMAGE_WIDTH,
    [Description("GPSLatitude")]
    GPS_LATITUDE,
    [Description("GPSLongitude")]
    GPS_LONGITUDE,
    [Description("GPSLatitudeRef")]
    GPS_LATITUDE_REF,
    [Description("GPSLongitudeRef")]
    GPS_LONGITUDE_REF,
    [Description("DateTimeOriginal")]
    DATE_TIME_ORIGINAL,
    [Description("ExposureTime")]
    EXPOSURE_TIME,
    [Description("SceneType")]
    SCENE_TYPE,
    [Description("ISOSpeedRatings")]
    ISO_SPEED_RATINGS,
    [Description("FNumber")]
    F_NUMBER,
    [Description("DateTime")]
    DATE_TIME,
    [Description("GPSTimeStamp")]
    GPS_TIME_STAMP,
    [Description("GPSDateStamp")]
    GPS_DATE_STAMP,
    [Description("ImageDescription")]
    IMAGE_DESCRIPTION,
    [Description("Make")]
    MAKE,
    [Description("Model")]
    MODEL,
    [Description("PhotoMode")]
    PHOTO_MODE,
    [Description("SensitivityType")]
    SENSITIVITY_TYPE,
    [Description("StandardOutputSensitivity")]
    STANDARD_OUTPUT_SENSITIVITY,
    [Description("RecommendedExposureIndex")]
    RECOMMENDED_EXPOSURE_INDEX,
    [Description("ISOSpeedRatings")]
    ISO_SPEED,
    [Description("ApertureValue")]
    APERTURE_VALUE,
    [Description("ExposureBiasValue")]
    EXPOSURE_BIAS_VALUE,
    [Description("MeteringMode")]
    METERING_MODE,
    [Description("LightSource")]
    LIGHT_SOURCE,
    [Description("Flash")]
    FLASH,
    [Description("FocalLength")]
    FOCAL_LENGTH,
    [Description("UserComment")]
    USER_COMMENT,
    [Description("PixelXDimension")]
    PIXEL_X_DIMENSION,
    [Description("PixelYDimension")]
    PIXEL_Y_DIMENSION,
    [Description("WhiteBalance")]
    WHITE_BALANCE,
    [Description("FocalLengthIn35mmFilm")]
    FOCAL_LENGTH_IN_35_MM_FILM,
    [Description("HwMnoteCaptureMode")]
    CAPTURE_MODE,
    [Description("HwMnotePhysicalAperture")]
    PHYSICAL_APERTURE,
    [Description("HwMnoteRollAngle")]
    ROLL_ANGLE,
    [Description("HwMnotePitchAngle")]
    PITCH_ANGLE,
    [Description("HwMnoteSceneFoodConf")]
    SCENE_FOOD_CONF,
    [Description("HwMnoteSceneStageConf")]
    SCENE_STAGE_CONF,
    [Description("HwMnoteSceneBlueSkyConf")]
    SCENE_BLUE_SKY_CONF,
    [Description("HwMnoteSceneGreenPlantConf")]
    SCENE_GREEN_PLANT_CONF,
    [Description("HwMnoteSceneBeachConf")]
    SCENE_BEACH_CONF,
    [Description("HwMnoteSceneSnowConf")]
    SCENE_SNOW_CONF,
    [Description("HwMnoteSceneSunsetConf")]
    SCENE_SUNSET_CONF,
    [Description("HwMnoteSceneFlowersConf")]
    SCENE_FLOWERS_CONF,
    [Description("HwMnoteSceneNightConf")]
    SCENE_NIGHT_CONF,
    [Description("HwMnoteSceneTextConf")]
    SCENE_TEXT_CONF,
    [Description("HwMnoteFaceCount")]
    FACE_COUNT,
    [Description("HwMnoteFocusMode")]
    FOCUS_MODE,
    [Description("Compression")]
    COMPRESSION,
    [Description("PhotometricInterpretation")]
    PHOTOMETRIC_INTERPRETATION,
    [Description("StripOffsets")]
    STRIP_OFFSETS,
    [Description("SamplesPerPixel")]
    SAMPLES_PER_PIXEL,
    [Description("RowsPerStrip")]
    ROWS_PER_STRIP,
    [Description("StripByteCounts")]
    STRIP_BYTE_COUNTS,
    [Description("XResolution")]
    X_RESOLUTION,
    [Description("YResolution")]
    Y_RESOLUTION,
    [Description("PlanarConfiguration")]
    PLANAR_CONFIGURATION,
    [Description("ResolutionUnit")]
    RESOLUTION_UNIT,
    [Description("TransferFunction")]
    TRANSFER_FUNCTION,
    [Description("Software")]
    SOFTWARE,
    [Description("Artist")]
    ARTIST,
    [Description("WhitePoint")]
    WHITE_POINT,
    [Description("PrimaryChromaticities")]
    PRIMARY_CHROMATICITIES,
    [Description("YCbCrCoefficients")]
    YCBCR_COEFFICIENTS,
    [Description("YCbCrSubSampling")]
    YCBCR_SUB_SAMPLING,
    [Description("YCbCrPositioning")]
    YCBCR_POSITIONING,
    [Description("ReferenceBlackWhite")]
    REFERENCE_BLACK_WHITE,
    [Description("Copyright")]
    COPYRIGHT,
    [Description("JPEGInterchangeFormat")]
    JPEG_INTERCHANGE_FORMAT,
    [Description("JPEGInterchangeFormatLength")]
    JPEG_INTERCHANGE_FORMAT_LENGTH,
    [Description("ExposureProgram")]
    EXPOSURE_PROGRAM,
    [Description("SpectralSensitivity")]
    SPECTRAL_SENSITIVITY,
    [Description("OECF")]
    OECF,
    [Description("ExifVersion")]
    EXIF_VERSION,
    [Description("DateTimeDigitized")]
    DATE_TIME_DIGITIZED,
    [Description("ComponentsConfiguration")]
    COMPONENTS_CONFIGURATION,
    [Description("ShutterSpeedValue")]
    SHUTTER_SPEED,
    [Description("BrightnessValue")]
    BRIGHTNESS_VALUE,
    [Description("MaxApertureValue")]
    MAX_APERTURE_VALUE,
    [Description("SubjectDistance")]
    SUBJECT_DISTANCE,
    [Description("SubjectArea")]
    SUBJECT_AREA,
    [Description("MakerNote")]
    MAKER_NOTE,
    [Description("SubsecTime")]
    SUBSEC_TIME,
    [Description("SubsecTimeOriginal")]
    SUBSEC_TIME_ORIGINAL,
    [Description("SubsecTimeDigitized")]
    SUBSEC_TIME_DIGITIZED,
    [Description("FlashpixVersion")]
    FLASHPIX_VERSION,
    [Description("ColorSpace")]
    COLOR_SPACE,
    [Description("RelatedSoundFile")]
    RELATED_SOUND_FILE,
    [Description("FlashEnergy")]
    FLASH_ENERGY,
    [Description("SpatialFrequencyResponse")]
    SPATIAL_FREQUENCY_RESPONSE,
    [Description("FocalPlaneXResolution")]
    FOCAL_PLANE_X_RESOLUTION,
    [Description("FocalPlaneYResolution")]
    FOCAL_PLANE_Y_RESOLUTION,
    [Description("FocalPlaneResolutionUnit")]
    FOCAL_PLANE_RESOLUTION_UNIT,
    [Description("SubjectLocation")]
    SUBJECT_LOCATION,
    [Description("ExposureIndex")]
    EXPOSURE_INDEX,
    [Description("SensingMethod")]
    SENSING_METHOD,
    [Description("FileSource")]
    FILE_SOURCE,
    [Description("CFAPattern")]
    CFA_PATTERN,
    [Description("CustomRendered")]
    CUSTOM_RENDERED,
    [Description("ExposureMode")]
    EXPOSURE_MODE,
    [Description("DigitalZoomRatio")]
    DIGITAL_ZOOM_RATIO,
    [Description("SceneCaptureType")]
    SCENE_CAPTURE_TYPE,
    [Description("GainControl")]
    GAIN_CONTROL,
    [Description("Contrast")]
    CONTRAST,
    [Description("Saturation")]
    SATURATION,
    [Description("Sharpness")]
    SHARPNESS,
    [Description("DeviceSettingDescription")]
    DEVICE_SETTING_DESCRIPTION,
    [Description("SubjectDistanceRange")]
    SUBJECT_DISTANCE_RANGE,
    [Description("ImageUniqueID")]
    IMAGE_UNIQUE_ID,
    [Description("GPSVersionID")]
    GPS_VERSION_ID,
    [Description("GPSAltitudeRef")]
    GPS_ALTITUDE_REF,
    [Description("GPSAltitude")]
    GPS_ALTITUDE,
    [Description("GPSSatellites")]
    GPS_SATELLITES,
    [Description("GPSStatus")]
    GPS_STATUS,
    [Description("GPSMeasureMode")]
    GPS_MEASURE_MODE,
    [Description("GPSDOP")]
    GPS_DOP,
    [Description("GPSSpeedRef")]
    GPS_SPEED_REF,
    [Description("GPSSpeed")]
    GPS_SPEED,
    [Description("GPSTrackRef")]
    GPS_TRACK_REF,
    [Description("GPSTrack")]
    GPS_TRACK,
    [Description("GPSImgDirectionRef")]
    GPS_IMG_DIRECTION_REF,
    [Description("GPSImgDirection")]
    GPS_IMG_DIRECTION,
    [Description("GPSMapDatum")]
    GPS_MAP_DATUM,
    [Description("GPSDestLatitudeRef")]
    GPS_DEST_LATITUDE_REF,
    [Description("GPSDestLatitude")]
    GPS_DEST_LATITUDE,
    [Description("GPSDestLongitudeRef")]
    GPS_DEST_LONGITUDE_REF,
    [Description("GPSDestLongitude")]
    GPS_DEST_LONGITUDE,
    [Description("GPSDestBearingRef")]
    GPS_DEST_BEARING_REF,
    [Description("GPSDestBearing")]
    GPS_DEST_BEARING,
    [Description("GPSDestDistanceRef")]
    GPS_DEST_DISTANCE_REF,
    [Description("GPSDestDistance")]
    GPS_DEST_DISTANCE,
    [Description("GPSProcessingMethod")]
    GPS_PROCESSING_METHOD,
    [Description("GPSAreaInformation")]
    GPS_AREA_INFORMATION,
    [Description("GPSDifferential")]
    GPS_DIFFERENTIAL,
    [Description("BodySerialNumber")]
    BODY_SERIAL_NUMBER,
    [Description("CameraOwnerName")]
    CAMERA_OWNER_NAME,
    [Description("CompositeImage")]
    COMPOSITE_IMAGE,
    [Description("CompressedBitsPerPixel")]
    COMPRESSED_BITS_PER_PIXEL,
    [Description("DNGVersion")]
    DNG_VERSION,
    [Description("DefaultCropSize")]
    DEFAULT_CROP_SIZE,
    [Description("Gamma")]
    GAMMA,
    [Description("ISOSpeedLatitudeyyy")]
    ISO_SPEED_LATITUDE_YYY,
    [Description("ISOSpeedLatitudezzz")]
    ISO_SPEED_LATITUDE_ZZZ,
    [Description("LensMake")]
    LENS_MAKE,
    [Description("LensModel")]
    LENS_MODEL,
    [Description("LensSerialNumber")]
    LENS_SERIAL_NUMBER,
    [Description("LensSpecification")]
    LENS_SPECIFICATION,
    [Description("NewSubfileType")]
    NEW_SUBFILE_TYPE,
    [Description("OffsetTime")]
    OFFSET_TIME,
    [Description("OffsetTimeDigitized")]
    OFFSET_TIME_DIGITIZED,
    [Description("OffsetTimeOriginal")]
    OFFSET_TIME_ORIGINAL,
    [Description("SourceExposureTimesOfCompositeImage")]
    SOURCE_EXPOSURE_TIMES_OF_COMPOSITE_IMAGE,
    [Description("SourceImageNumberOfCompositeImage")]
    SOURCE_IMAGE_NUMBER_OF_COMPOSITE_IMAGE,
    [Description("SubfileType")]
    SUBFILE_TYPE,
    [Description("GPSHPositioningError")]
    GPS_H_POSITIONING_ERROR,
    [Description("PhotographicSensitivity")]
    PHOTOGRAPHIC_SENSITIVITY,
    [Description("HwMnoteBurstNumber")]
    BURST_NUMBER,
    [Description("HwMnoteFaceConf")]
    FACE_CONF,
    [Description("HwMnoteFaceLeyeCenter")]
    FACE_LEYE_CENTER,
    [Description("HwMnoteFaceMouthCenter")]
    FACE_MOUTH_CENTER,
    [Description("HwMnoteFacePointer")]
    FACE_POINTER,
    [Description("HwMnoteFaceRect")]
    FACE_RECT,
    [Description("HwMnoteFaceReyeCenter")]
    FACE_REYE_CENTER,
    [Description("HwMnoteFaceSmileScore")]
    FACE_SMILE_SCORE,
    [Description("HwMnoteFaceVersion")]
    FACE_VERSION,
    [Description("HwMnoteFrontCamera")]
    FRONT_CAMERA,
    [Description("HwMnoteScenePointer")]
    SCENE_POINTER,
    [Description("HwMnoteSceneVersion")]
    SCENE_VERSION,
    [Description("HwMnoteIsXmageSupported")]
    IS_XMAGE_SUPPORTED,
    [Description("HwMnoteXmageMode")]
    XMAGE_MODE,
    [Description("HwMnoteXmageLeft")]
    XMAGE_LEFT,
    [Description("HwMnoteXmageTop")]
    XMAGE_TOP,
    [Description("HwMnoteXmageRight")]
    XMAGE_RIGHT,
    [Description("HwMnoteXmageBottom")]
    XMAGE_BOTTOM,
    [Description("HwMnoteCloudEnhancementMode")]
    CLOUD_ENHANCEMENT_MODE,
    [Description("HwMnoteWindSnapshotMode")]
    WIND_SNAPSHOT_MODE,
    [Description("GIFLoopCount")]
    GIF_LOOP_COUNT
}

/// <summary>
/// ImageFormat 枚举
/// </summary>
public enum ImageFormat
{
    YCBCR_422_SP = 1000,
    JPEG = 2000
}

/// <summary>
/// AlphaType 枚举
/// </summary>
public enum AlphaType
{
    UNKNOWN = 0,
    OPAQUE = 1,
    PREMUL = 2,
    UNPREMUL = 3
}

/// <summary>
/// DecodingDynamicRange 枚举
/// </summary>
public enum DecodingDynamicRange
{
    AUTO = 0,
    SDR = 1,
    HDR = 2
}

/// <summary>
/// PackingDynamicRange 枚举
/// </summary>
public enum PackingDynamicRange
{
    AUTO = 0,
    SDR = 1
}

/// <summary>
/// AntiAliasingLevel 枚举
/// </summary>
public enum AntiAliasingLevel
{
    NONE = 0,
    LOW = 1,
    MEDIUM = 2,
    HIGH = 3
}

/// <summary>
/// ScaleMode 枚举
/// </summary>
public enum ScaleMode
{
    FIT_TARGET_SIZE = 0,
    CENTER_CROP = 1
}

/// <summary>
/// ComponentType 枚举
/// </summary>
public enum ComponentType
{
    YUV_Y = 1,
    YUV_U = 2,
    YUV_V = 3,
    JPEG = 4
}

/// <summary>
/// HdrMetadataKey 枚举
/// </summary>
public enum HdrMetadataKey
{
    HDR_METADATA_TYPE = 0,
    HDR_STATIC_METADATA = 1,
    HDR_DYNAMIC_METADATA = 2,
    HDR_GAINMAP_METADATA = 3
}

/// <summary>
/// HdrMetadataType 枚举
/// </summary>
public enum HdrMetadataType
{
    NONE = 0,
    BASE = 1,
    GAINMAP = 2,
    ALTERNATE = 3
}

/// <summary>
/// AllocatorType 枚举
/// </summary>
public enum AllocatorType
{
    AUTO = 0,
    DMA = 1,
    SHARE_MEMORY = 2
}

/// <summary>
/// CropAndScaleStrategy 枚举
/// </summary>
public enum CropAndScaleStrategy
{
    SCALE_FIRST = 1,
    CROP_FIRST = 2
}

/// <summary>
/// AuxiliaryPictureType 枚举
/// </summary>
public enum AuxiliaryPictureType
{
    GAINMAP = 1,
    DEPTH_MAP = 2,
    UNREFOCUS_MAP = 3,
    LINEAR_MAP = 4,
    FRAGMENT_MAP = 5,
    LHDR_GAINMAP = 10
}

/// <summary>
/// MetadataType 枚举
/// </summary>
public enum MetadataType
{
    EXIF_METADATA = 1,
    FRAGMENT_METADATA = 2,
    GIF_METADATA = 5,
    HEIFS_METADATA = 15,
    DNG_METADATA = 16,
    WEBP_METADATA = 17,
    PNG_METADATA = 19,
    JFIF_METADATA = 20,
    TIFF_METADATA = 21,
    XMP_METADATA = 22,
    AVIS_METADATA = 23
}

/// <summary>
/// FragmentMapPropertyKey 枚举
/// </summary>
public enum FragmentMapPropertyKey
{
    [Description("XInOriginal")]
    X_IN_ORIGINAL,
    [Description("YInOriginal")]
    Y_IN_ORIGINAL,
    [Description("FragmentImageWidth")]
    WIDTH,
    [Description("FragmentImageHeight")]
    HEIGHT
}

/// <summary>
/// GifPropertyKey 枚举
/// </summary>
public enum GifPropertyKey
{
    [Description("GifDelayTime")]
    GIF_DELAY_TIME,
    [Description("GifDisposalType")]
    GIF_DISPOSAL_TYPE,
    [Description("GifHasGlobalColorMap")]
    GIF_HAS_GLOBAL_COLOR_MAP,
    [Description("GifCanvasWidth")]
    GIF_CANVAS_WIDTH,
    [Description("GifCanvasHeight")]
    GIF_CANVAS_HEIGHT,
    [Description("GifLoopCount")]
    GIF_LOOP_COUNT,
    [Description("GifUnclampedDelayTime")]
    GIF_UNCLAMPED_DELAY_TIME
}

/// <summary>
/// HeifsPropertyKey 枚举
/// </summary>
public enum HeifsPropertyKey
{
    [Description("HeifsDelayTime")]
    HEIFS_DELAY_TIME,
    [Description("HeifsUnclampedDelayTime")]
    HEIFS_UNCLAMPED_DELAY_TIME,
    [Description("HeifsCanvasHeight")]
    HEIFS_CANVAS_HEIGHT,
    [Description("HeifsCanvasWidth")]
    HEIFS_CANVAS_WIDTH
}

/// <summary>
/// DngPropertyKey 枚举
/// </summary>
public enum DngPropertyKey
{
    [Description("DNGVersion")]
    DNG_VERSION,
    [Description("DNGBackwardVersion")]
    DNG_BACKWARD_VERSION,
    [Description("UniqueCameraModel")]
    UNIQUE_CAMERA_MODEL,
    [Description("LocalizedCameraModel")]
    LOCALIZED_CAMERA_MODEL,
    [Description("CFAPlaneColor")]
    CFA_PLANE_COLOR,
    [Description("CFALayout")]
    CFA_LAYOUT,
    [Description("LinearizationTable")]
    LINEARIZATION_TABLE,
    [Description("BlackLevelRepeatDim")]
    BLACK_LEVEL_REPEAT_DIM,
    [Description("BlackLevel")]
    BLACK_LEVEL,
    [Description("BlackLevelDeltaH")]
    BLACK_LEVEL_DELTA_H,
    [Description("BlackLevelDeltaV")]
    BLACK_LEVEL_DELTA_V,
    [Description("WhiteLevel")]
    WHITE_LEVEL,
    [Description("DefaultScale")]
    DEFAULT_SCALE,
    [Description("DefaultCropOrigin")]
    DEFAULT_CROP_ORIGIN,
    [Description("DefaultCropSize")]
    DEFAULT_CROP_SIZE,
    [Description("ColorMatrix1")]
    COLOR_MATRIX1,
    [Description("ColorMatrix2")]
    COLOR_MATRIX2,
    [Description("CameraCalibration1")]
    CAMERA_CALIBRATION1,
    [Description("CameraCalibration2")]
    CAMERA_CALIBRATION2,
    [Description("ReductionMatrix1")]
    REDUCTION_MATRIX1,
    [Description("ReductionMatrix2")]
    REDUCTION_MATRIX2,
    [Description("AnalogBalance")]
    ANALOG_BALANCE,
    [Description("AsShotNeutral")]
    AS_SHOT_NEUTRAL,
    [Description("AsShotWhiteXY")]
    AS_SHOT_WHITEXY,
    [Description("BaselineExposure")]
    BASELINE_EXPOSURE,
    [Description("BaselineNoise")]
    BASELINE_NOISE,
    [Description("BaselineSharpness")]
    BASELINE_SHARPNESS,
    [Description("BayerGreenSplit")]
    BAYER_GREEN_SPLIT,
    [Description("LinearResponseLimit")]
    LINEAR_RESPONSE_LIMIT,
    [Description("CameraSerialNumber")]
    CAMERA_SERIAL_NUMBER,
    [Description("LensInfo")]
    LENS_INFO,
    [Description("ChromaBlurRadius")]
    CHROMA_BLUR_RADIUS,
    [Description("AntiAliasStrength")]
    ANTI_ALIAS_STRENGTH,
    [Description("ShadowScale")]
    SHADOW_SCALE,
    [Description("DNGPrivateData")]
    DNG_PRIVATE_DATA,
    [Description("MakerNoteSafety")]
    MAKER_NOTE_SAFETY,
    [Description("CalibrationIlluminant1")]
    CALIBRATION_ILLUMINANT1,
    [Description("CalibrationIlluminant2")]
    CALIBRATION_ILLUMINANT2,
    [Description("BestQualityScale")]
    BEST_QUALITY_SCALE,
    [Description("RawDataUniqueID")]
    RAW_DATA_UNIQUE_ID,
    [Description("OriginalRawFileName")]
    ORIGINAL_RAW_FILE_NAME,
    [Description("OriginalRawFileData")]
    ORIGINAL_RAW_FILE_DATA,
    [Description("ActiveArea")]
    ACTIVE_AREA,
    [Description("MaskedAreas")]
    MASKED_AREAS,
    [Description("AsShotICCProfile")]
    AS_SHOT_ICC_PROFILE,
    [Description("AsShotPreProfileMatrix")]
    AS_SHOT_PRE_PROFILE_MATRIX,
    [Description("CurrentICCProfile")]
    CURRENT_ICC_PROFILE,
    [Description("CurrentPreProfileMatrix")]
    CURRENT_PRE_PROFILE_MATRIX,
    [Description("ColorimetricReference")]
    COLORIMETRIC_REFERENCE,
    [Description("CameraCalibrationSignature")]
    CAMERA_CALIBRATION_SIGNATURE,
    [Description("ProfileCalibrationSignature")]
    PROFILE_CALIBRATION_SIGNATURE,
    [Description("ExtraCameraProfiles")]
    EXTRA_CAMERA_PROFILES,
    [Description("AsShotProfileName")]
    AS_SHOT_PROFILE_NAME,
    [Description("NoiseReductionApplied")]
    NOISE_REDUCTION_APPLIED,
    [Description("ProfileName")]
    PROFILE_NAME,
    [Description("ProfileHueSatMapDims")]
    PROFILE_HUE_SAT_MAP_DIMS,
    [Description("ProfileHueSatMapData1")]
    PROFILE_HUE_SAT_MAP_DATA1,
    [Description("ProfileHueSatMapData2")]
    PROFILE_HUE_SAT_MAP_DATA2,
    [Description("ProfileToneCurve")]
    PROFILE_TONE_CURVE,
    [Description("ProfileEmbedPolicy")]
    PROFILE_EMBED_POLICY,
    [Description("ProfileCopyright")]
    PROFILE_COPYRIGHT,
    [Description("ForwardMatrix1")]
    FORWARD_MATRIX1,
    [Description("ForwardMatrix2")]
    FORWARD_MATRIX2,
    [Description("PreviewApplicationName")]
    PREVIEW_APPLICATION_NAME,
    [Description("PreviewApplicationVersion")]
    PREVIEW_APPLICATION_VERSION,
    [Description("PreviewSettingsName")]
    PREVIEW_SETTINGS_NAME,
    [Description("PreviewSettingsDigest")]
    PREVIEW_SETTINGS_DIGEST,
    [Description("PreviewColorSpace")]
    PREVIEW_COLOR_SPACE,
    [Description("PreviewDateTime")]
    PREVIEW_DATE_TIME,
    [Description("RawImageDigest")]
    RAW_IMAGE_DIGEST,
    [Description("OriginalRawFileDigest")]
    ORIGINAL_RAW_FILE_DIGEST,
    [Description("SubTileBlockSize")]
    SUB_TILE_BLOCK_SIZE,
    [Description("RowInterleaveFactor")]
    ROW_INTERLEAVE_FACTOR,
    [Description("ProfileLookTableDims")]
    PROFILE_LOOK_TABLE_DIMS,
    [Description("ProfileLookTableData")]
    PROFILE_LOOK_TABLE_DATA,
    [Description("OpcodeList1")]
    OPCODE_LIST1,
    [Description("OpcodeList2")]
    OPCODE_LIST2,
    [Description("OpcodeList3")]
    OPCODE_LIST3,
    [Description("NoiseProfile")]
    NOISE_PROFILE,
    [Description("OriginalDefaultFinalSize")]
    ORIGINAL_DEFAULT_FINAL_SIZE,
    [Description("OriginalBestQualityFinalSize")]
    ORIGINAL_BEST_QUALITY_FINAL_SIZE,
    [Description("OriginalDefaultCropSize")]
    ORIGINAL_DEFAULT_CROP_SIZE,
    [Description("ProfileHueSatMapEncoding")]
    PROFILE_HUE_SAT_MAP_ENCODING,
    [Description("ProfileLookTableEncoding")]
    PROFILE_LOOK_TABLE_ENCODING,
    [Description("BaselineExposureOffset")]
    BASELINE_EXPOSURE_OFFSET,
    [Description("DefaultBlackRender")]
    DEFAULT_BLACK_RENDER,
    [Description("NewRawImageDigest")]
    NEW_RAW_IMAGE_DIGEST,
    [Description("RawToPreviewGain")]
    RAW_TO_PREVIEW_GAIN,
    [Description("DefaultUserCrop")]
    DEFAULT_USER_CROP
}

/// <summary>
/// TiffPropertyKey 枚举
/// </summary>
public enum TiffPropertyKey
{
    [Description("TiffCompression")]
    COMPRESSION,
    [Description("TiffPhotometricInterpretation")]
    PHOTOMETRIC_INTERPRETATION,
    [Description("TiffTransferFunction")]
    TRANSFER_FUNCTION,
    [Description("TiffOrientation")]
    ORIENTATION,
    [Description("TiffXResolution")]
    X_RESOLUTION,
    [Description("TiffYResolution")]
    Y_RESOLUTION,
    [Description("TiffResolutionUnit")]
    RESOLUTION_UNIT,
    [Description("TiffWhitePoint")]
    WHITE_POINT,
    [Description("TiffPrimaryChromaticities")]
    PRIMARY_CHROMATICITIES,
    [Description("TiffTileLength")]
    TILE_LENGTH,
    [Description("TiffTileWidth")]
    TILE_WIDTH,
    [Description("TiffDocumentName")]
    DOCUMENT_NAME,
    [Description("TiffImageDescription")]
    IMAGE_DESCRIPTION,
    [Description("TiffArtist")]
    ARTIST,
    [Description("TiffCopyright")]
    COPYRIGHT,
    [Description("TiffDateTime")]
    DATE_TIME,
    [Description("TiffMake")]
    MAKE,
    [Description("TiffModel")]
    MODEL,
    [Description("TiffSoftware")]
    SOFTWARE,
    [Description("TiffHostComputer")]
    HOST_COMPUTER
}

/// <summary>
/// JfifPropertyKey 枚举
/// </summary>
public enum JfifPropertyKey
{
    [Description("JfifXDensity")]
    X_DENSITY,
    [Description("JfifYDensity")]
    Y_DENSITY,
    [Description("JfifDensityUnit")]
    DENSITY_UNIT,
    [Description("JfifVersion")]
    VERSION,
    [Description("JfifIsProgressive")]
    IS_PROGRESSIVE
}

/// <summary>
/// PngPropertyKey 枚举
/// </summary>
public enum PngPropertyKey
{
    [Description("PngXPixelsPerMeter")]
    X_PIXELS_PER_METER,
    [Description("PngYPixelsPerMeter")]
    Y_PIXELS_PER_METER,
    [Description("PngGamma")]
    GAMMA,
    [Description("PngInterlaceType")]
    INTERLACE_TYPE,
    [Description("PngSRGBIntent")]
    SRGB_INTENT,
    [Description("PngChromaticities")]
    CHROMATICITIES,
    [Description("PngTitle")]
    TITLE,
    [Description("PngDescription")]
    DESCRIPTION,
    [Description("PngComment")]
    COMMENT,
    [Description("PngDisclaimer")]
    DISCLAIMER,
    [Description("PngWarning")]
    WARNING,
    [Description("PngAuthor")]
    AUTHOR,
    [Description("PngCopyright")]
    COPYRIGHT,
    [Description("PngCreationTime")]
    CREATION_TIME,
    [Description("PngModificationTime")]
    MODIFICATION_TIME,
    [Description("PngSoftware")]
    SOFTWARE
}

/// <summary>
/// Orientation 枚举
/// </summary>
public enum Orientation
{
    TOP_LEFT = 1,
    TOP_RIGHT = 2,
    BOTTOM_RIGHT = 3,
    BOTTOM_LEFT = 4,
    LEFT_TOP = 5,
    RIGHT_TOP = 6,
    RIGHT_BOTTOM = 7,
    LEFT_BOTTOM = 8
}

/// <summary>
/// FocusMode 枚举
/// </summary>
public enum FocusMode
{
    AF_A = 0,
    AF_S = 1,
    AF_C = 2,
    MF = 3
}

/// <summary>
/// XmageColorMode 枚举
/// </summary>
public enum XmageColorMode
{
    NORMAL = 0,
    BRIGHT = 1,
    SOFT = 2,
    MONO = 3
}

/// <summary>
/// WebPPropertyKey 枚举
/// </summary>
public enum WebPPropertyKey
{
    [Description("WebPCanvasWidth")]
    CANVAS_WIDTH,
    [Description("WebPCanvasHeight")]
    CANVAS_HEIGHT,
    [Description("WebPDelayTime")]
    DELAY_TIME,
    [Description("WebPUnclampedDelayTime")]
    UNCLAMPED_DELAY_TIME,
    [Description("WebPLoopCount")]
    LOOP_COUNT
}

/// <summary>
/// XMPTagType 枚举
/// </summary>
public enum XMPTagType
{
    UNKNOWN = 0,
    STRING = 1,
    UNORDERED_ARRAY = 2,
    ORDERED_ARRAY = 3,
    ALTERNATE_ARRAY = 4,
    ALTERNATE_TEXT = 5,
    STRUCTURE = 6
}

/// <summary>
/// AvisPropertyKey 枚举
/// </summary>
public enum AvisPropertyKey
{
    [Description("AvisDelayTime")]
    DELAY_TIME
}