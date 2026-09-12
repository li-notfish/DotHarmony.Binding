using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ReminderAgentManagerActionButtonType 枚举
/// </summary>
public enum ReminderAgentManagerActionButtonType
{
    ActionButtonTypeClose = 0,
    ActionButtonTypeSnooze = 1
}

/// <summary>
/// ReminderAgentManagerReminderType 枚举
/// </summary>
public enum ReminderAgentManagerReminderType
{
    ReminderTypeTimer = 0,
    ReminderTypeCalendar = 1,
    ReminderTypeAlarm = 2
}

/// <summary>
/// RingChannel 枚举
/// </summary>
public enum RingChannel
{
    RingChannelAlarm = 0,
    RingChannelMedia = 1,
    RingChannelNotification = 2
}

/// <summary>
/// TimeZoneType 枚举
/// </summary>
public enum TimeZoneType
{
    Default = 0,
    FixedTimeZone = 1,
    SystemTimeZone = 2
}