using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// NotificationManagerSlotType 枚举
/// </summary>
public enum NotificationManagerSlotType
{
    UnknownType = 0,
    SocialCommunication = 1,
    ServiceInformation = 2,
    ContentInformation = 3,
    LiveView = 4,
    CustomerService = 5,
    OtherTypes = 65535
}

/// <summary>
/// NotificationManagerContentType 枚举
/// </summary>
public enum NotificationManagerContentType
{
    NotificationContentBasicText,
    NotificationContentLongText,
    NotificationContentPicture,
    NotificationContentConversation,
    NotificationContentMultiline,
    NotificationContentSystemLiveView,
    NotificationContentLiveView
}

/// <summary>
/// NotificationManagerSlotLevel 枚举
/// </summary>
public enum NotificationManagerSlotLevel
{
    LevelNone = 0,
    LevelMin = 1,
    LevelLow = 2,
    LevelDefault = 3,
    LevelHigh = 4
}

/// <summary>
/// PriorityNotificationType 枚举
/// </summary>
public enum PriorityNotificationType
{
    [Description("OTHER")]
    Other,
    [Description("PRIMARY_CONTACT")]
    PrimaryContact,
    [Description("AT_ME")]
    AtMe,
    [Description("URGENT_MESSAGE")]
    UrgentMessage,
    [Description("SCHEDULE_REMINDER")]
    ScheduleReminder
}