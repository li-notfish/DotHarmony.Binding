using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ActionFlagType 枚举
/// </summary>
public enum ActionFlagType
{
    ActionView = 1,
    ActionSave = 2,
    ActionSaveAs = 4,
    ActionEdit = 8,
    ActionScreenCapture = 16,
    ActionScreenShare = 32,
    ActionScreenRecord = 64,
    ActionCopy = 128,
    ActionPrint = 256,
    ActionExport = 512,
    ActionPermissionChange = 1024
}

/// <summary>
/// DLPFileAccess 枚举
/// </summary>
public enum DLPFileAccess
{
    NoPermission = 0,
    ReadOnly = 1,
    ContentEdit = 2,
    FullControl = 3
}

/// <summary>
/// AccountType 枚举
/// </summary>
public enum AccountType
{
    CloudAccount = 1,
    DomainAccount = 2,
    EnterpriseAccount = 4
}

/// <summary>
/// ActionType 枚举
/// </summary>
public enum ActionType
{
    NotOpen = 0,
    Open = 1
}