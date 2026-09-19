using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ShareOptions 枚举
/// </summary>
public enum ShareOptions
{
    InApp = 0,
    CrossApp = 1
}

/// <summary>
/// UriPermission 枚举
/// </summary>
public enum UriPermission
{
    None = 0,
    Read = 1,
    Write = 2,
    Persist = 3
}

/// <summary>
/// Intention 枚举
/// </summary>
public enum Intention
{
    [Description("DataHub")]
    DataHub,
    [Description("Drag")]
    Drag,
    [Description("SystemShare")]
    SystemShare,
    [Description("Picker")]
    Picker,
    [Description("Menu")]
    Menu
}

/// <summary>
/// Visibility 枚举
/// </summary>
public enum Visibility
{
    All,
    OwnProcess
}

/// <summary>
/// FileConflictOptions 枚举
/// </summary>
public enum FileConflictOptions
{
    Overwrite = 0,
    Skip = 1
}

/// <summary>
/// ProgressIndicator 枚举
/// </summary>
public enum ProgressIndicator
{
    None = 0,
    Default = 1
}

/// <summary>
/// ListenerStatus 枚举
/// </summary>
public enum ListenerStatus
{
    Finished = 0,
    Processing = 1,
    Canceled = 2,
    InnerError = 200,
    InvalidParameters = 201,
    DataNotFound = 202,
    SyncFailed = 203,
    CopyFileFailed = 204
}