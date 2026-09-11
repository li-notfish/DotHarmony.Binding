using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PickerPhotoViewMIMETypes 枚举
/// </summary>
public enum PickerPhotoViewMIMETypes
{
    [Description("image/*")]
    ImageType,
    [Description("video/*")]
    VideoType,
    [Description("*/*")]
    ImageVideoType
}

/// <summary>
/// DocumentSelectMode 枚举
/// </summary>
public enum DocumentSelectMode
{
    File = 0,
    Folder = 1,
    Mixed = 2
}

/// <summary>
/// DocumentPickerMode 枚举
/// </summary>
public enum DocumentPickerMode
{
    Default = 0,
    Download = 1
}

/// <summary>
/// MergeTypeMode 枚举
/// </summary>
public enum MergeTypeMode
{
    Default = 0,
    Audio = 1,
    Video = 2,
    Document = 3,
    Picture = 4
}