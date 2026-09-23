using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// OperationMode 枚举
/// </summary>
public enum OperationMode
{
    ReadMode = 1,
    WriteMode = 2,
    CreateMode = 4,
    DeleteMode = 8,
    RenameMode = 16
}

/// <summary>
/// PolicyErrorCode 枚举
/// </summary>
public enum PolicyErrorCode
{
    PersistenceForbidden = 1,
    InvalidMode = 2,
    InvalidPath = 3,
    PermissionNotPersisted = 4
}

/// <summary>
/// FilesharePolicyType 枚举
/// </summary>
public enum FilesharePolicyType
{
    TemporaryType = 0,
    PersistentType = 1
}