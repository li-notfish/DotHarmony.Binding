using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BackgroundMode 枚举
/// </summary>
public enum BackgroundMode
{
    DataTransfer = 1,
    AudioPlayback = 2,
    AudioRecording = 3,
    Location = 4,
    BluetoothInteraction = 5,
    MultiDeviceConnection = 6,
    TaskKeeping = 9
}