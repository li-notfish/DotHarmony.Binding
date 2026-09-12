using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CalendarType 枚举
/// </summary>
public enum CalendarType
{
    [Description("local")]
    Local,
    [Description("email")]
    Email,
    [Description("birthday")]
    Birthday,
    [Description("caldav")]
    Caldav,
    [Description("subscribed")]
    Subscribed
}

/// <summary>
/// EventType 枚举
/// </summary>
public enum EventType
{
    Normal = 0,
    Important = 1
}

/// <summary>
/// RecurrenceFrequency 枚举
/// </summary>
public enum RecurrenceFrequency
{
    Yearly = 0,
    Monthly = 1,
    Weekly = 2,
    Daily = 3
}

/// <summary>
/// AttendeeRole 枚举
/// </summary>
public enum AttendeeRole
{
    [Description("organizer")]
    Organizer,
    [Description("participant")]
    Participant
}

/// <summary>
/// AttendeeType 枚举
/// </summary>
public enum AttendeeType
{
    Required = 1,
    Optional = 2,
    Resource = 3
}

/// <summary>
/// AttendeeStatus 枚举
/// </summary>
public enum AttendeeStatus
{
    Unknown = 0,
    Tentative = 1,
    Accepted = 2,
    Declined = 3,
    Unresponsive = 4
}

/// <summary>
/// CalendarManagerServiceType 枚举
/// </summary>
public enum CalendarManagerServiceType
{
    [Description("Meeting")]
    Meeting,
    [Description("Watching")]
    Watching,
    [Description("Repayment")]
    Repayment,
    [Description("Live")]
    Live,
    [Description("Shopping")]
    Shopping,
    [Description("Trip")]
    Trip,
    [Description("Class")]
    Class,
    [Description("SportsEvents")]
    SportsEvents,
    [Description("SportsExercise")]
    SportsExercise
}