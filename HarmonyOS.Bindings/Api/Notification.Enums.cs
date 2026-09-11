using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SlotType 枚举
/// </summary>
public enum SlotType
{
    UnknownType = 0,
    SocialCommunication = 1,
    ServiceInformation = 2,
    ContentInformation = 3,
    OtherTypes = 65535
}

/// <summary>
/// ContentType 枚举
/// </summary>
public enum ContentType
{
    NotificationContentBasicText,
    NotificationContentLongText,
    NotificationContentPicture,
    NotificationContentConversation,
    NotificationContentMultiline
}

/// <summary>
/// SlotLevel 枚举
/// </summary>
public enum SlotLevel
{
    LevelNone = 0,
    LevelMin = 1,
    LevelLow = 2,
    LevelDefault = 3,
    LevelHigh = 4
}