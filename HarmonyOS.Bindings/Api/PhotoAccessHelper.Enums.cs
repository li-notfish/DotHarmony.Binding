using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PhotoType 枚举
/// </summary>
public enum PhotoType
{
    Image = 1,
    Video = 2
}

/// <summary>
/// PhotoSubtype 枚举
/// </summary>
public enum PhotoSubtype
{
    Default = 0,
    MovingPhoto = 3,
    Burst = 4
}

/// <summary>
/// DynamicRangeType 枚举
/// </summary>
public enum DynamicRangeType
{
    Sdr = 0,
    Hdr = 1
}

/// <summary>
/// PositionType 枚举
/// </summary>
public enum PositionType
{
    Local = 1,
    Cloud = 2,
    LocalAndCloud = 3
}

/// <summary>
/// RecommendationType 枚举
/// </summary>
public enum RecommendationType
{
    QROrBarCode = 1,
    QRCode = 2,
    BarCode = 3,
    IdCard = 4,
    ProfilePicture = 5,
    Passport = 6,
    BankCard = 7,
    DriverLicense = 8,
    DrivingLicense = 9,
    FeaturedSinglePortrait = 10
}

/// <summary>
/// DeliveryMode 枚举
/// </summary>
public enum DeliveryMode
{
    FastMode = 0,
    HighQualityMode = 1,
    BalanceMode = 2
}

/// <summary>
/// CompatibleMode 枚举
/// </summary>
public enum CompatibleMode
{
    OriginalFormatMode = 0,
    CompatibleFormatMode = 1
}

/// <summary>
/// CompleteButtonText 枚举
/// </summary>
public enum CompleteButtonText
{
    TextDone = 0,
    TextSend = 1,
    TextAdd = 2
}

/// <summary>
/// PhotoKeys 枚举
/// </summary>
public enum PhotoKeys
{
    [Description("uri")]
    Uri,
    [Description("media_type")]
    PhotoType,
    [Description("display_name")]
    DisplayName,
    [Description("size")]
    Size,
    [Description("date_added")]
    DateAdded,
    [Description("date_modified")]
    DateModified,
    [Description("duration")]
    Duration,
    [Description("width")]
    Width,
    [Description("height")]
    Height,
    [Description("date_taken")]
    DateTaken,
    [Description("orientation")]
    Orientation,
    [Description("is_favorite")]
    Favorite,
    [Description("title")]
    Title,
    [Description("position")]
    Position,
    [Description("date_added_ms")]
    DateAddedMs,
    [Description("date_modified_ms")]
    DateModifiedMs,
    [Description("subtype")]
    PhotoSubtype,
    [Description("dynamic_range_type")]
    DynamicRangeType,
    [Description("cover_position")]
    CoverPosition,
    [Description("burst_key")]
    BurstKey,
    [Description("lcd_size")]
    LcdSize,
    [Description("thm_size")]
    ThmSize,
    [Description("detail_time")]
    DetailTime,
    [Description("date_taken_ms")]
    DateTakenMs,
    [Description("owner_album_id")]
    OwnerAlbumId,
    [Description("media_suffix")]
    MediaSuffix,
    [Description("aspect_ratio")]
    AspectRatio,
    [Description("change_time")]
    ChangeTime,
    [Description("local_asset_size")]
    LocalAssetSize
}

/// <summary>
/// FusionAssetType 枚举
/// </summary>
public enum FusionAssetType
{
}

/// <summary>
/// AlbumKeys 枚举
/// </summary>
public enum AlbumKeys
{
    [Description("uri")]
    Uri,
    [Description("album_name")]
    AlbumName,
    [Description("lpath")]
    AlbumLpath,
    [Description("change_time")]
    ChangeTime
}

/// <summary>
/// AlbumType 枚举
/// </summary>
public enum AlbumType
{
    User = 0,
    System = 1024,
    Source = 2048
}

/// <summary>
/// AlbumSubtype 枚举
/// </summary>
public enum AlbumSubtype
{
    UserGeneric = 1,
    Favorite = 1025,
    Video,
    Image = 1031,
    SourceGeneric = 2049,
    SourceGenericFromFileManager = 2050,
    Any = 2147483647
}

/// <summary>
/// NotifyChangeType 枚举
/// </summary>
public enum NotifyChangeType
{
    NotifyChangeAdd = 0,
    NotifyChangeUpdate = 1,
    NotifyChangeRemove = 2
}

/// <summary>
/// PhotoSource 枚举
/// </summary>
public enum PhotoSource
{
    All = 0,
    Camera = 1,
    Screenshot = 2
}

/// <summary>
/// PhotoAccessHelperNotifyType 枚举
/// </summary>
public enum PhotoAccessHelperNotifyType
{
    NotifyAdd,
    NotifyUpdate,
    NotifyRemove,
    NotifyAlbumAddAsset,
    NotifyAlbumRemoveAsset
}

/// <summary>
/// DefaultChangeUri 枚举
/// </summary>
public enum DefaultChangeUri
{
    [Description("file://media/Photo")]
    DefaultPhotoUri,
    [Description("file://media/PhotoAlbum")]
    DefaultAlbumUri
}

/// <summary>
/// PhotoViewMIMETypes 枚举
/// </summary>
public enum PhotoViewMIMETypes
{
    [Description("image/*")]
    ImageType,
    [Description("video/*")]
    VideoType,
    [Description("*/*")]
    ImageVideoType,
    [Description("image/movingPhoto")]
    MovingPhotoImageType
}

/// <summary>
/// FilterOperator 枚举
/// </summary>
public enum FilterOperator
{
    EqualTo = 0,
    NotEqualTo = 1,
    MoreThan = 2,
    LessThan = 3,
    MoreThanOrEqualTo = 4,
    LessThanOrEqualTo = 5,
    Between = 6
}

/// <summary>
/// SingleSelectionMode 枚举
/// </summary>
public enum SingleSelectionMode
{
    BrowserMode = 0,
    SelectMode = 1,
    BrowserAndSelectMode = 2
}

/// <summary>
/// MovingPhotoBadgeStateType 枚举
/// </summary>
public enum MovingPhotoBadgeStateType
{
    NotMovingPhoto = 0,
    MovingPhotoEnabled = 1,
    MovingPhotoDisabled = 2
}

/// <summary>
/// PhotoAccessHelperResourceType 枚举
/// </summary>
public enum PhotoAccessHelperResourceType
{
    ImageResource = 1,
    VideoResource = 2
}

/// <summary>
/// ImageFileType 枚举
/// </summary>
public enum ImageFileType
{
    Jpeg = 1,
    Heif = 2
}

/// <summary>
/// PhotoAccessHelperOperationType 枚举
/// </summary>
public enum PhotoAccessHelperOperationType
{
    EqualTo = 1,
    NotEqualTo = 2,
    GreaterThan = 3,
    LessThan = 4,
    GreaterThanOrEqualTo = 5,
    LessThanOrEqualTo = 6,
    And = 7,
    Or = 8,
    In = 9,
    NotIn = 10,
    BeginWrap = 11,
    EndWrap = 12,
    Between = 13,
    NotBetween = 14
}

/// <summary>
/// SceneType 枚举
/// </summary>
public enum SceneType
{
    GridToPhotoBrowser = 0,
    PhotoBrowserSwipe = 1
}

/// <summary>
/// GridPinchModeType 枚举
/// </summary>
public enum GridPinchModeType
{
    FullFunctionGrid = 0
}

/// <summary>
/// GridLevel 枚举
/// </summary>
public enum GridLevel
{
    Spacious = 0,
    Standard = 1,
    Compact = 2
}

/// <summary>
/// PlayMode 枚举
/// </summary>
public enum PlayMode
{
    Default = 0,
    AutoPlay = 1
}

/// <summary>
/// VideoMode 枚举
/// </summary>
public enum VideoMode
{
    Default = 0,
    LogVideo = 1
}

/// <summary>
/// PreferredCompatibleMode 枚举
/// </summary>
public enum PreferredCompatibleMode
{
    Default = 0,
    Current = 1,
    Compatible = 2
}

/// <summary>
/// MediaAssetPermissionState 枚举
/// </summary>
public enum MediaAssetPermissionState
{
    UriFormatError = 0,
    FileNotExist = 1,
    ReadPermission = 2,
    NoReadPermission = 3
}

/// <summary>
/// AvailabilityStatus 枚举
/// </summary>
public enum AvailabilityStatus
{
    [Description("available")]
    Available,
    [Description("unavailable")]
    Unavailable
}