using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// WebHitTestType 枚举
/// </summary>
public enum WebHitTestType
{
    EditText = 0,
    Email = 1,
    HttpAnchor = 2,
    HttpAnchorImg = 3,
    Img = 4,
    Map = 5,
    Phone = 6,
    Unknown = 7
}

/// <summary>
/// SecureDnsMode 枚举
/// </summary>
public enum SecureDnsMode
{
    Off = 0,
    Auto = 1,
    SecureOnly = 2
}

/// <summary>
/// ArkWebEngineVersion 枚举
/// </summary>
public enum ArkWebEngineVersion
{
    SystemDefault = 0,
    M114 = 1,
    M132 = 2,
    M144 = 3,
    ArkwebEvergreen = 99999
}

/// <summary>
/// WebviewSecurityLevel 枚举
/// </summary>
public enum WebviewSecurityLevel
{
    None = 0,
    Secure = 1,
    Warning = 2,
    Dangerous = 3
}

/// <summary>
/// MediaPlaybackState 枚举
/// </summary>
public enum MediaPlaybackState
{
    None = 0,
    Playing = 1,
    Paused = 2,
    Stopped = 3
}

/// <summary>
/// PressureLevel 枚举
/// </summary>
public enum PressureLevel
{
    MemoryPressureLevelModerate = 1,
    MemoryPressureLevelCritical = 2
}

/// <summary>
/// WebMessageType 枚举
/// </summary>
public enum WebMessageType
{
    NotSupport = 0,
    String = 1,
    Number = 2,
    Boolean = 3,
    ArrayBuffer = 4,
    Array = 5,
    Error = 6
}

/// <summary>
/// JsMessageType 枚举
/// </summary>
public enum JsMessageType
{
    NotSupport = 0,
    String = 1,
    Number = 2,
    Boolean = 3,
    ArrayBuffer = 4,
    Array = 5
}

/// <summary>
/// RenderProcessMode 枚举
/// </summary>
public enum RenderProcessMode
{
    Single = 0,
    Multiple = 1
}

/// <summary>
/// OfflineResourceType 枚举
/// </summary>
public enum OfflineResourceType
{
    Image,
    Css,
    ClassicJs,
    ModuleJs
}

/// <summary>
/// ScrollType 枚举
/// </summary>
public enum ScrollType
{
    Event = 0
}

/// <summary>
/// ControllerAttachState 枚举
/// </summary>
public enum ControllerAttachState
{
    Unattached = 0,
    Attached = 1
}

/// <summary>
/// WebBlanklessErrorCode 枚举
/// </summary>
public enum WebBlanklessErrorCode
{
    Success = 0,
    [Description("-1")]
    ErrUnknown,
    [Description("-2")]
    ErrInvalidParam,
    [Description("-3")]
    ErrControllerNotInited,
    [Description("-4")]
    ErrKeyNotMatch,
    [Description("-5")]
    ErrSignificantChange,
    [Description("-6")]
    ErrDurationOutOfRange,
    [Description("-7")]
    ErrExpirationTimeOutOfRange
}

/// <summary>
/// BlanklessFrameInterpolationState 枚举
/// </summary>
public enum BlanklessFrameInterpolationState
{
    FrameInterpolationSucceeded = 0,
    FrameInterpolationFailed = 1,
    FrameInterpolationRemoved = 2
}

/// <summary>
/// WebSoftKeyboardBehaviorMode 枚举
/// </summary>
public enum WebSoftKeyboardBehaviorMode
{
    Default = 0,
    DisableAutoKeyboardOnActive = 1
}

/// <summary>
/// WebDownloadState 枚举
/// </summary>
public enum WebDownloadState
{
    InProgress = 0,
    Completed,
    Canceled,
    Interrupted,
    Pending,
    Paused,
    Unknown
}

/// <summary>
/// WebDownloadErrorCode 枚举
/// </summary>
public enum WebDownloadErrorCode
{
    ErrorUnknown = 0,
    FileFailed = 1,
    FileAccessDenied = 2,
    FileNoSpace = 3,
    FileNameTooLong = 5,
    FileTooLarge = 6,
    FileTransientError = 10,
    FileBlocked = 11,
    FileTooShort = 13,
    FileHashMismatch = 14,
    FileSameAsSource = 15,
    NetworkFailed = 20,
    NetworkTimeout = 21,
    NetworkDisconnected = 22,
    NetworkServerDown = 23,
    NetworkInvalidRequest = 24,
    ServerFailed = 30,
    ServerNoRange = 31,
    ServerBadContent = 33,
    ServerUnauthorized = 34,
    ServerCertProblem = 35,
    ServerForbidden = 36,
    ServerUnreachable = 37,
    ServerContentLengthMismatch = 38,
    ServerCrossOriginRedirect = 39,
    UserCanceled = 40,
    UserShutdown = 41,
    Crash = 50
}

/// <summary>
/// WebResourceType 枚举
/// </summary>
public enum WebResourceType
{
    MainFrame = 0,
    SubFrame = 1,
    StyleSheet = 2,
    Script = 3,
    Image = 4,
    FontResource = 5,
    SubResource = 6,
    Object = 7,
    Media = 8,
    Worker = 9,
    SharedWorker = 10,
    Prefetch = 11,
    Favicon = 12,
    Xhr = 13,
    Ping = 14,
    ServiceWorker = 15,
    CspReport = 16,
    PluginResource = 17,
    NavigationPreloadMainFrame = 19,
    NavigationPreloadSubFrame = 20
}

/// <summary>
/// PlaybackStatus 枚举
/// </summary>
public enum PlaybackStatus
{
    Paused = 0,
    Playing = 1
}

/// <summary>
/// NetworkState 枚举
/// </summary>
public enum NetworkState
{
    Empty = 0,
    Idle = 1,
    Loading = 2,
    NetworkError = 3
}

/// <summary>
/// ReadyState 枚举
/// </summary>
public enum ReadyState
{
    HaveNothing = 0,
    HaveMetadata = 1,
    HaveCurrentData = 2,
    HaveFutureData = 3,
    HaveEnoughData = 4
}

/// <summary>
/// MediaError 枚举
/// </summary>
public enum MediaError
{
    NetworkError = 1,
    FormatError = 2,
    DecodeError = 3
}

/// <summary>
/// SuspendType 枚举
/// </summary>
public enum SuspendType
{
    EnterBackForwardCache = 0,
    EnterBackground,
    AutoCleanup
}

/// <summary>
/// WebviewMediaType 枚举
/// </summary>
public enum WebviewMediaType
{
    Video = 0,
    Audio = 1
}

/// <summary>
/// WebviewSourceType 枚举
/// </summary>
public enum WebviewSourceType
{
    Url = 0,
    Mse = 1
}

/// <summary>
/// Preload 枚举
/// </summary>
public enum Preload
{
    None = 0,
    Metadata = 1,
    Auto = 2
}

/// <summary>
/// ProxySchemeFilter 枚举
/// </summary>
public enum ProxySchemeFilter
{
    MatchAllSchemes = 0,
    MatchHttp = 1,
    MatchHttps = 2
}

/// <summary>
/// WebDestroyMode 枚举
/// </summary>
public enum WebDestroyMode
{
    NormalMode = 0,
    FastMode = 1
}

/// <summary>
/// SiteIsolationMode 枚举
/// </summary>
public enum SiteIsolationMode
{
    Partial = 0,
    Strict = 1
}

/// <summary>
/// ScrollbarMode 枚举
/// </summary>
public enum ScrollbarMode
{
    OverlayLayoutScrollbar = 0,
    ForceDisplayScrollbar = 1,
    OverlayVisualScrollbar = 2
}

/// <summary>
/// WebHttpCookieSameSitePolicy 枚举
/// </summary>
public enum WebHttpCookieSameSitePolicy
{
    None = 0,
    Lax = 1,
    Strict = 2
}

/// <summary>
/// UserAgentFormFactor 枚举
/// </summary>
public enum UserAgentFormFactor
{
    [Description("Automotive")]
    Automotive,
    [Description("Desktop")]
    Desktop,
    [Description("Mobile")]
    Mobile,
    [Description("EInk")]
    Eink,
    [Description("Tablet")]
    Tablet,
    [Description("Watch")]
    Watch,
    [Description("XR")]
    Xr
}