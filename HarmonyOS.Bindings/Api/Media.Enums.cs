using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SoundInterruptMode 枚举
/// </summary>
public enum SoundInterruptMode
{
    NO_INTERRUPT = 0,
    SAME_SOUND_INTERRUPT = 1
}

/// <summary>
/// StateChangeReason 枚举
/// </summary>
public enum StateChangeReason
{
    USER = 1,
    BACKGROUND = 2
}

/// <summary>
/// HdrType 枚举
/// </summary>
public enum HdrType
{
    AV_HDR_TYPE_NONE = 0,
    AV_HDR_TYPE_VIVID = 1
}

/// <summary>
/// AVImageQueryOptions 枚举
/// </summary>
public enum AVImageQueryOptions
{
    AV_IMAGE_QUERY_NEXT_SYNC = 0,
    AV_IMAGE_QUERY_PREVIOUS_SYNC,
    AV_IMAGE_QUERY_CLOSEST_SYNC,
    AV_IMAGE_QUERY_CLOSEST
}

/// <summary>
/// FetchResult 枚举
/// </summary>
public enum FetchResult
{
    FETCH_FAILED = 0,
    FETCH_SUCCEEDED = 1,
    FETCH_CANCELED = 2
}

/// <summary>
/// AVErrorCode 枚举
/// </summary>
public enum AVErrorCode
{
    AVERR_OK = 0,
    AVERR_NO_PERMISSION = 201,
    AVERR_INVALID_PARAMETER = 401,
    AVERR_UNSUPPORT_CAPABILITY = 801,
    AVERR_NO_MEMORY = 5400101,
    AVERR_OPERATE_NOT_PERMIT = 5400102,
    AVERR_IO = 5400103,
    AVERR_TIMEOUT = 5400104,
    AVERR_SERVICE_DIED = 5400105,
    AVERR_UNSUPPORT_FORMAT = 5400106,
    AVERR_AUDIO_INTERRUPTED = 5400107,
    AVERR_IO_HOST_NOT_FOUND = 5411001,
    AVERR_IO_CONNECTION_TIMEOUT = 5411002,
    AVERR_IO_NETWORK_ABNORMAL = 5411003,
    AVERR_IO_NETWORK_UNAVAILABLE = 5411004,
    AVERR_IO_NO_PERMISSION = 5411005,
    AVERR_IO_REQUEST_DENIED = 5411006,
    AVERR_IO_RESOURCE_NOT_FOUND = 5411007,
    AVERR_IO_SSL_CLIENT_CERT_NEEDED = 5411008,
    AVERR_IO_SSL_CONNECTION_FAILED = 5411009,
    AVERR_IO_SSL_SERVER_CERT_UNTRUSTED = 5411010,
    AVERR_IO_UNSUPPORTED_REQUEST = 5411011,
    AVERR_SEEK_CONTINUOUS_UNSUPPORTED = 5410002,
    AVERR_SUPER_RESOLUTION_UNSUPPORTED = 5410003,
    AVERR_SUPER_RESOLUTION_NOT_ENABLED = 5410004,
    AVERR_IO_CLEARTEXT_NOT_PERMITTED = 5411012,
    AVERR_PARAMETER_OUT_OF_RANGE = 5400108
}

/// <summary>
/// AVMetricsEventType 枚举
/// </summary>
public enum AVMetricsEventType
{
    AV_METRICS_EVENT_STALLING = 1,
    AV_METRICS_EVENT_LIP_ASYNC = 2,
    AV_METRICS_EVENT_LOADINGRATE_CHANGE = 3,
    AV_METRICS_EVENT_LOADING_ERROR = 4,
    AV_METRICS_EVENT_CONTENT_CHANGED = 5,
    AV_METRICS_EVENT_CONTENT_DISCONTINUITY = 6,
    AV_METRICS_EVENT_AUDIO_ABNORMAL = 7
}

/// <summary>
/// PlaylistLoopMode 枚举
/// </summary>
public enum PlaylistLoopMode
{
    PLAYLIST_LOOP_MODE_ALL = 1,
    PLAYLIST_LOOP_MODE_ONE = 2,
    PLAYLIST_LOOP_MODE_SHUFFLE = 3,
    PLAYLIST_LOOP_MODE_NONE = 4
}

/// <summary>
/// PlaybackMetricsKey 枚举
/// </summary>
public enum PlaybackMetricsKey
{
    [Description("prepare_duration")]
    PREPARE_DURATION,
    [Description("resource_connection_duration")]
    RESOURCE_CONNECTION_DURATION,
    [Description("first_frame_decapsulation_duration")]
    FIRST_FRAME_DECAPSULATION_DURATION,
    [Description("total_playback_time")]
    TOTAL_PLAYING_TIME,
    [Description("loading_requests_count")]
    DOWNLOAD_REQUESTS_COUNT,
    [Description("total_loading_time")]
    TOTAL_DOWNLOAD_TIME,
    [Description("total_loading_bytes")]
    TOTAL_DOWNLOAD_SIZE,
    [Description("stalling_count")]
    STALLING_COUNT,
    [Description("total_stalling_time")]
    TOTAL_STALLING_TIME,
    [Description("lip_async_count")]
    LIP_ASYNC_COUNT,
    [Description("total_lip_async_time")]
    TOTAL_LIP_ASYNC_TIME
}

/// <summary>
/// PlaybackInfoKey 枚举
/// </summary>
public enum PlaybackInfoKey
{
    [Description("server_ip_address")]
    SERVER_IP_ADDRESS,
    [Description("average_download_rate")]
    AVG_DOWNLOAD_RATE,
    [Description("download_rate")]
    DOWNLOAD_RATE,
    [Description("is_downloading")]
    IS_DOWNLOADING,
    [Description("buffer_duration")]
    BUFFER_DURATION
}

/// <summary>
/// MediaErrorCode 枚举
/// </summary>
public enum MediaErrorCode
{
    MSERR_OK = 0,
    MSERR_NO_MEMORY = 1,
    MSERR_OPERATION_NOT_PERMIT = 2,
    MSERR_INVALID_VAL = 3,
    MSERR_IO = 4,
    MSERR_TIMEOUT = 5,
    MSERR_UNKNOWN = 6,
    MSERR_SERVICE_DIED = 7,
    MSERR_INVALID_STATE = 8,
    MSERR_UNSUPPORTED = 9
}

/// <summary>
/// BufferingInfoType 枚举
/// </summary>
public enum BufferingInfoType
{
    BUFFERING_START = 1,
    BUFFERING_END = 2,
    BUFFERING_PERCENT = 3,
    CACHED_DURATION = 4
}

/// <summary>
/// LoadingRequestError 枚举
/// </summary>
public enum LoadingRequestError
{
    LOADING_ERROR_SUCCESS = 0,
    LOADING_ERROR_NOT_READY = 1,
    LOADING_ERROR_NO_RESOURCE = 2,
    LOADING_ERROR_INVAID_HANDLE = 3,
    LOADING_ERROR_ACCESS_DENIED = 4,
    LOADING_ERROR_ACCESS_TIMEOUT = 5,
    LOADING_ERROR_AUTHORIZE_FAILED = 6
}

/// <summary>
/// AVMimeTypes 枚举
/// </summary>
public enum AVMimeTypes
{
    [Description("application/m3u8")]
    APPLICATION_M3U8
}

/// <summary>
/// AudioEncoder 枚举
/// </summary>
public enum AudioEncoder
{
    DEFAULT = 0,
    AMR_NB = 1,
    AMR_WB = 2,
    AAC_LC = 3,
    HE_AAC = 4
}

/// <summary>
/// AudioOutputFormat 枚举
/// </summary>
public enum AudioOutputFormat
{
    DEFAULT = 0,
    MPEG_4 = 2,
    AMR_NB = 3,
    AMR_WB = 4,
    AAC_ADTS = 6
}

/// <summary>
/// PlaybackSpeed 枚举
/// </summary>
public enum PlaybackSpeed
{
    SPEED_FORWARD_0_75_X = 0,
    SPEED_FORWARD_1_00_X = 1,
    SPEED_FORWARD_1_25_X = 2,
    SPEED_FORWARD_1_75_X = 3,
    SPEED_FORWARD_2_00_X = 4,
    SPEED_FORWARD_0_50_X = 5,
    SPEED_FORWARD_1_50_X = 6,
    SPEED_FORWARD_3_00_X = 7,
    SPEED_FORWARD_0_25_X = 8,
    SPEED_FORWARD_0_125_X = 9
}

/// <summary>
/// VideoScaleType 枚举
/// </summary>
public enum VideoScaleType
{
    VIDEO_SCALE_TYPE_FIT = 0,
    VIDEO_SCALE_TYPE_FIT_CROP = 1,
    VIDEO_SCALE_TYPE_SCALED_ASPECT = 2
}

/// <summary>
/// ContainerFormatType 枚举
/// </summary>
public enum ContainerFormatType
{
    [Description("mp4")]
    CFT_MPEG_4,
    [Description("m4a")]
    CFT_MPEG_4A,
    [Description("mp3")]
    CFT_MP3,
    [Description("wav")]
    CFT_WAV,
    [Description("amr")]
    CFT_AMR,
    [Description("aac")]
    CFT_AAC
}

/// <summary>
/// MediaType 枚举
/// </summary>
public enum MediaType
{
    [Description("-1")]
    MEDIA_TYPE_UNSUPPORTED,
    MEDIA_TYPE_AUD = 0,
    MEDIA_TYPE_VID = 1,
    MEDIA_TYPE_SUBTITLE = 2,
    MEDIA_TYPE_ATTACHMENT = 3,
    MEDIA_TYPE_DATA = 4,
    MEDIA_TYPE_TIMED_METADATA = 5,
    MEDIA_TYPE_AUXILIARY = 6
}

/// <summary>
/// MediaDescriptionKey 枚举
/// </summary>
public enum MediaDescriptionKey
{
    [Description("track_index")]
    MD_KEY_TRACK_INDEX,
    [Description("track_type")]
    MD_KEY_TRACK_TYPE,
    [Description("codec_mime")]
    MD_KEY_CODEC_MIME,
    [Description("duration")]
    MD_KEY_DURATION,
    [Description("bitrate")]
    MD_KEY_BITRATE,
    [Description("width")]
    MD_KEY_WIDTH,
    [Description("height")]
    MD_KEY_HEIGHT,
    [Description("frame_rate")]
    MD_KEY_FRAME_RATE,
    [Description("channel_count")]
    MD_KEY_AUD_CHANNEL_COUNT,
    [Description("sample_rate")]
    MD_KEY_AUD_SAMPLE_RATE,
    [Description("sample_depth")]
    MD_KEY_AUD_SAMPLE_DEPTH,
    [Description("language")]
    MD_KEY_LANGUAGE,
    [Description("track_name")]
    MD_KEY_TRACK_NAME,
    [Description("hdr_type")]
    MD_KEY_HDR_TYPE,
    [Description("original_width")]
    MD_KEY_ORIGINAL_WIDTH,
    [Description("original_height")]
    MD_KEY_ORIGINAL_HEIGHT,
    [Description("mime_type")]
    MD_KEY_MIME_TYPE,
    [Description("ref_track_ids")]
    MD_KEY_REFERENCE_TRACK_IDS,
    [Description("track_ref_type")]
    MD_KEY_TRACK_REFERENCE_TYPE
}

/// <summary>
/// AudioSourceType 枚举
/// </summary>
public enum AudioSourceType
{
    AUDIO_SOURCE_TYPE_DEFAULT = 0,
    AUDIO_SOURCE_TYPE_MIC = 1,
    AUDIO_SOURCE_TYPE_VOICE_RECOGNITION = 2,
    AUDIO_SOURCE_TYPE_VOICE_COMMUNICATION = 7,
    AUDIO_SOURCE_TYPE_VOICE_MESSAGE = 10,
    AUDIO_SOURCE_TYPE_CAMCORDER = 13
}

/// <summary>
/// VideoSourceType 枚举
/// </summary>
public enum VideoSourceType
{
    VIDEO_SOURCE_TYPE_SURFACE_YUV = 0,
    VIDEO_SOURCE_TYPE_SURFACE_ES = 1
}

/// <summary>
/// FileGenerationMode 枚举
/// </summary>
public enum FileGenerationMode
{
    APP_CREATE = 0,
    AUTO_CREATE_CAMERA_SCENE = 1
}

/// <summary>
/// AacProfile 枚举
/// </summary>
public enum AacProfile
{
    AAC_LC = 0,
    AAC_HE = 1,
    AAC_HE_V2 = 2
}

/// <summary>
/// SeekMode 枚举
/// </summary>
public enum SeekMode
{
    SEEK_NEXT_SYNC = 0,
    SEEK_PREV_SYNC = 1,
    SEEK_CLOSEST = 2,
    SEEK_CONTINUOUS = 3
}

/// <summary>
/// SwitchMode 枚举
/// </summary>
public enum SwitchMode
{
    SMOOTH = 0,
    SEGMENT = 1,
    CLOSEST = 2
}

/// <summary>
/// CodecMimeType 枚举
/// </summary>
public enum CodecMimeType
{
    [Description("video/h263")]
    VIDEO_H263,
    [Description("video/avc")]
    VIDEO_AVC,
    [Description("video/mpeg2")]
    VIDEO_MPEG2,
    [Description("video/mp4v-es")]
    VIDEO_MPEG4,
    [Description("video/x-vnd.on2.vp8")]
    VIDEO_VP8,
    [Description("audio/mp4a-latm")]
    AUDIO_AAC,
    [Description("audio/vorbis")]
    AUDIO_VORBIS,
    [Description("audio/flac")]
    AUDIO_FLAC,
    [Description("video/hevc")]
    VIDEO_HEVC,
    [Description("audio/mpeg")]
    AUDIO_MP3,
    [Description("audio/g711mu")]
    AUDIO_G711MU,
    [Description("audio/3gpp")]
    AUDIO_AMR_NB,
    [Description("audio/amr-wb")]
    AUDIO_AMR_WB,
    [Description("audio/raw")]
    AUDIO_RAW
}

/// <summary>
/// AVScreenCaptureRecordPreset 枚举
/// </summary>
public enum AVScreenCaptureRecordPreset
{
    SCREEN_RECORD_PRESET_H264_AAC_MP4 = 0,
    SCREEN_RECORD_PRESET_H265_AAC_MP4 = 1
}

/// <summary>
/// AVScreenCaptureFillMode 枚举
/// </summary>
public enum AVScreenCaptureFillMode
{
    PRESERVE_ASPECT_RATIO = 0,
    SCALE_TO_FILL = 1
}

/// <summary>
/// AVScreenCaptureStateCode 枚举
/// </summary>
public enum AVScreenCaptureStateCode
{
    SCREENCAPTURE_STATE_STARTED = 0,
    SCREENCAPTURE_STATE_CANCELED = 1,
    SCREENCAPTURE_STATE_STOPPED_BY_USER = 2,
    SCREENCAPTURE_STATE_INTERRUPTED_BY_OTHER = 3,
    SCREENCAPTURE_STATE_STOPPED_BY_CALL = 4,
    SCREENCAPTURE_STATE_MIC_UNAVAILABLE = 5,
    SCREENCAPTURE_STATE_MIC_MUTED_BY_USER = 6,
    SCREENCAPTURE_STATE_MIC_UNMUTED_BY_USER = 7,
    SCREENCAPTURE_STATE_ENTER_PRIVATE_SCENE = 8,
    SCREENCAPTURE_STATE_EXIT_PRIVATE_SCENE = 9,
    SCREENCAPTURE_STATE_STOPPED_BY_USER_SWITCHES = 10,
    SCREENCAPTURE_STATE_PAUSED_BY_USER = 11,
    SCREENCAPTURE_STATE_RESUMED_BY_USER = 12,
    SCREENCAPTURE_STATE_PAUSED_BY_APP = 13,
    SCREENCAPTURE_STATE_RESUMED_BY_APP = 14
}

/// <summary>
/// PickerMode 枚举
/// </summary>
public enum PickerMode
{
    WINDOW_ONLY = 0,
    SCREEN_ONLY = 1,
    SCREEN_AND_WINDOW = 2,
    APP_ONLY = 3,
    WINDOW_AND_APP = 4,
    SCREEN_AND_APP = 5,
    SCREEN_WINDOW_AND_APP = 6
}