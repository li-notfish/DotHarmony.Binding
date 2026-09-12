using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ResourcescheduleBackgroundTaskManagerBackgroundMode 枚举
/// </summary>
public enum ResourcescheduleBackgroundTaskManagerBackgroundMode
{
    DataTransfer = 1,
    AudioPlayback = 2,
    AudioRecording = 3,
    Location = 4,
    BluetoothInteraction = 5,
    MultiDeviceConnection = 6,
    Voip = 8,
    TaskKeeping = 9
}

/// <summary>
/// BackgroundTaskMode 枚举
/// </summary>
public enum BackgroundTaskMode
{
    ModeDataTransfer = 1,
    ModeAudioPlayback = 2,
    ModeAudioRecording = 3,
    ModeLocation = 4,
    ModeBluetoothInteraction = 5,
    ModeMultiDeviceConnection = 6,
    ModeVoip = 8,
    ModeTaskKeeping = 9,
    ModeAVPlaybackAndRecord = 12,
    ModeSpecialScenarioProcessing = 13,
    ModeNearlink = 14
}

/// <summary>
/// BackgroundTaskSubmode 枚举
/// </summary>
public enum BackgroundTaskSubmode
{
    SubmodeCarKeyNormalNotification = 1,
    SubmodeNormalNotification = 2,
    SubmodeLiveViewNotification = 3,
    SubmodeAudioPlaybackNormalNotification = 4,
    SubmodeAvsessionAudioPlayback = 5,
    SubmodeAudioRecordNormalNotification = 6,
    SubmodeScreenRecordNormalNotification = 7,
    SubmodeVoiceChatNormalNotification = 8,
    SubmodeMediaProcessNormalNotification = 9,
    SubmodeVideoBroadcastNormalNotification = 10,
    SubmodeWorkOutNormalNotification = 11
}

/// <summary>
/// ContinuousTaskCancelReason 枚举
/// </summary>
public enum ContinuousTaskCancelReason
{
    UserCancel = 1,
    SystemCancel = 2,
    UserCancelRemoveNotification = 3,
    SystemCancelDataTransferLowSpeed = 4,
    SystemCancelAudioPlaybackNotUseAvsession = 5,
    SystemCancelAudioPlaybackNotRunning = 6,
    SystemCancelAudioRecordingNotRunning = 7,
    SystemCancelNotUseLocation = 8,
    SystemCancelNotUseBluetooth = 9,
    SystemCancelNotUseMultiDevice = 10,
    SystemCancelUseIllegally = 11
}

/// <summary>
/// ContinuousTaskDetailedCancelReason 枚举
/// </summary>
public enum ContinuousTaskDetailedCancelReason
{
    UserCancelRemoveNotification = 3,
    SystemCancelDataTransferLowSpeed = 4,
    SystemCancelAudioPlaybackNotRunning = 6,
    SystemCancelAudioRecordingNotRunning = 7,
    SystemCancelNotUseLocation = 8,
    SystemCancelNotUseBluetooth = 9,
    SystemCancelNotUseMultiDevice = 10,
    SystemCancelUseIllegally = 11,
    SystemCancelDataTransferNotUpdate = 12,
    SystemCancelVoipNotRunning = 13,
    SystemCancelUserUnauthorized = 14
}

/// <summary>
/// BackgroundSubMode 枚举
/// </summary>
public enum BackgroundSubMode
{
    CarKey = 1
}

/// <summary>
/// BackgroundModeType 枚举
/// </summary>
public enum BackgroundModeType
{
    [Description("subMode")]
    SubMode
}

/// <summary>
/// ContinuousTaskSuspendReason 枚举
/// </summary>
public enum ContinuousTaskSuspendReason
{
    SystemSuspendDataTransferLowSpeed = 4,
    SystemSuspendAudioPlaybackNotUseAvsession = 5,
    SystemSuspendAudioPlaybackNotRunning = 6,
    SystemSuspendAudioRecordingNotRunning = 7,
    SystemSuspendLocationNotUsed = 8,
    SystemSuspendBluetoothNotUsed = 9,
    SystemSuspendMultiDeviceNotUsed = 10,
    SystemSuspendUsedIllegally = 11,
    SystemSuspendSystemLoadWarning = 12,
    SystemSuspendVoipNotUsed = 13,
    SystemSuspendBluetoothDataNotExist = 14,
    SystemSuspendPositionNotMoved = 15,
    SystemSuspendAudioPlaybackMute = 16,
    SystemSuspendNearlinkNotUsed = 17,
    SystemSuspendNearlinkDataNotExist = 18,
    SystemSuspendUserUnauthorized = 19
}

/// <summary>
/// UserAuthResult 枚举
/// </summary>
public enum UserAuthResult
{
    NotSupported = 0,
    NotDetermined = 1,
    Denied = 2,
    GrantedOnce = 3,
    GrantedAlways = 4
}