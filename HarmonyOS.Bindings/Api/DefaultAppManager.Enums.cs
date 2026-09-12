using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ApplicationType 枚举
/// </summary>
public enum ApplicationType
{
    [Description("Web Browser")]
    Browser,
    [Description("Image Gallery")]
    Image,
    [Description("Audio Player")]
    Audio,
    [Description("Video Player")]
    Video,
    [Description("PDF Viewer")]
    Pdf,
    [Description("Word Viewer")]
    Word,
    [Description("Excel Viewer")]
    Excel,
    [Description("PPT Viewer")]
    Ppt,
    [Description("Email")]
    Email
}