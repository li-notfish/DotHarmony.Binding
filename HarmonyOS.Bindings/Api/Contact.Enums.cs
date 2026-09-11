using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// Attribute 枚举
/// </summary>
public enum Attribute
{
    AttrContactEvent = 0,
    AttrEmail = 1,
    AttrGroupMembership = 2,
    AttrIm = 3,
    AttrName = 4,
    AttrNickname = 5,
    AttrNote = 6,
    AttrOrganization = 7,
    AttrPhone = 8,
    AttrPortrait = 9,
    AttrPostalAddress = 10,
    AttrRelation = 11,
    AttrSipAddress = 12,
    AttrWebsite = 13
}

/// <summary>
/// FilterType 枚举
/// </summary>
public enum FilterType
{
    ShowFilter = 0,
    DefaultSelect = 1,
    ShowFilterAndDefaultSelect = 2
}

/// <summary>
/// FilterCondition 枚举
/// </summary>
public enum FilterCondition
{
    EqualTo = 1,
    NotEqualTo = 2,
    In = 3,
    IsNotNull = 0,
    NotIn = 4,
    Contains = 5
}

/// <summary>
/// DataField 枚举
/// </summary>
public enum DataField
{
    Phone = 1,
    Organization = 2,
    Email = 0
}

/// <summary>
/// ContactSyncMode 枚举
/// </summary>
public enum ContactSyncMode
{
    ModeIncremental = 1,
    ModeCloudBased = 2
}