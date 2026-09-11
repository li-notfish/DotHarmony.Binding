using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// USBRequestTargetType 枚举
/// </summary>
public enum USBRequestTargetType
{
    UsbRequestTargetDevice = 0,
    UsbRequestTargetInterface = 1,
    UsbRequestTargetEndpoint = 2,
    UsbRequestTargetOther = 3
}

/// <summary>
/// USBControlRequestType 枚举
/// </summary>
public enum USBControlRequestType
{
    UsbRequestTypeStandard = 0,
    UsbRequestTypeClass = 1,
    UsbRequestTypeVendor = 2
}

/// <summary>
/// USBRequestDirection 枚举
/// </summary>
public enum USBRequestDirection
{
    UsbRequestDirToDevice = 0,
    UsbRequestDirFromDevice = 128
}

/// <summary>
/// UsbTransferFlags 枚举
/// </summary>
public enum UsbTransferFlags
{
    UsbTransferShortNotOk = 0,
    UsbTransferFreeBuffer = 1,
    UsbTransferFreeTransfer = 2,
    UsbTransferAddZeroPacket = 3
}

/// <summary>
/// UsbTransferStatus 枚举
/// </summary>
public enum UsbTransferStatus
{
    TransferCompleted = 0,
    TransferError = 1,
    TransferTimedOut = 2,
    TransferCanceled = 3,
    TransferStall = 4,
    TransferNoDevice = 5,
    TransferOverflow = 6
}

/// <summary>
/// UsbEndpointTransferType 枚举
/// </summary>
public enum UsbEndpointTransferType
{
    TransferTypeIsochronous = 1,
    TransferTypeBulk = 2,
    TransferTypeInterrupt = 3
}