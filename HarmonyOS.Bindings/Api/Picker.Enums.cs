using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PhotoViewMIMETypes 枚举
/// </summary>
public enum PhotoViewMIMETypes
{
    [Description("image/*")]
    IMAGE_TYPE,
    [Description("video/*")]
    VIDEO_TYPE,
    [Description("*/*")]
    IMAGE_VIDEO_TYPE
}

/// <summary>
/// DocumentSelectMode 枚举
/// </summary>
public enum DocumentSelectMode
{
    FILE = 0,
    FOLDER = 1,
    MIXED = 2
}

/// <summary>
/// DocumentPickerMode 枚举
/// </summary>
public enum DocumentPickerMode
{
    DEFAULT = 0,
    DOWNLOAD = 1
}

/// <summary>
/// MergeTypeMode 枚举
/// </summary>
public enum MergeTypeMode
{
    DEFAULT = 0,
    AUDIO = 1,
    VIDEO = 2,
    DOCUMENT = 3,
    PICTURE = 4
}