using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// PrintDocumentFormat 枚举
/// </summary>
public enum PrintDocumentFormat
{
    DocumentFormatAuto = 0,
    DocumentFormatJpeg = 1,
    DocumentFormatPdf = 2,
    DocumentFormatPostscript = 3,
    DocumentFormatText = 4,
    DocumentFormatRaw = 5
}

/// <summary>
/// DocFlavor 枚举
/// </summary>
public enum DocFlavor
{
    FileDescriptor = 0,
    Bytes = 1
}

/// <summary>
/// PrintDirectionMode 枚举
/// </summary>
public enum PrintDirectionMode
{
    DirectionModeAuto = 0,
    DirectionModePortrait = 1,
    DirectionModeLandscape = 2
}

/// <summary>
/// PrintColorMode 枚举
/// </summary>
public enum PrintColorMode
{
    ColorModeMonochrome = 0,
    ColorModeColor = 1
}

/// <summary>
/// PrintDuplexMode 枚举
/// </summary>
public enum PrintDuplexMode
{
    DuplexModeNone = 0,
    DuplexModeLongEdge = 1,
    DuplexModeShortEdge = 2
}

/// <summary>
/// PrintPageType 枚举
/// </summary>
public enum PrintPageType
{
    PageIsoA3 = 0,
    PageIsoA4 = 1,
    PageIsoA5 = 2,
    PageJisB5 = 3,
    PageIsoC5 = 4,
    PageIsoDl = 5,
    PageLetter = 6,
    PageLegal = 7,
    PagePhoto4X6 = 8,
    PagePhoto5X7 = 9,
    PageIntDlEnvelope = 10,
    PageBTabloid = 11
}

/// <summary>
/// PrintDocumentAdapterState 枚举
/// </summary>
public enum PrintDocumentAdapterState
{
    PreviewDestroy = 0,
    PrintTaskSucceed = 1,
    PrintTaskFail = 2,
    PrintTaskCancel = 3,
    PrintTaskBlock = 4
}

/// <summary>
/// PrintFileCreationState 枚举
/// </summary>
public enum PrintFileCreationState
{
    PrintFileCreated = 0,
    PrintFileCreationFailed = 1,
    PrintFileCreatedUnrendered = 2
}

/// <summary>
/// PrinterState 枚举
/// </summary>
public enum PrinterState
{
    PrinterAdded = 0,
    PrinterRemoved = 1,
    PrinterCapabilityUpdated = 2,
    PrinterConnected = 3,
    PrinterDisconnected = 4,
    PrinterRunning = 5
}

/// <summary>
/// PrintJobState 枚举
/// </summary>
public enum PrintJobState
{
    PrintJobPrepare = 0,
    PrintJobQueued = 1,
    PrintJobRunning = 2,
    PrintJobBlocked = 3,
    PrintJobCompleted = 4
}

/// <summary>
/// PrintJobSubState 枚举
/// </summary>
public enum PrintJobSubState
{
    PrintJobCompletedSuccess = 0,
    PrintJobCompletedFailed = 1,
    PrintJobCompletedCancelled = 2,
    PrintJobCompletedFileCorrupted = 3,
    PrintJobBlockOffline = 4,
    PrintJobBlockBusy = 5,
    PrintJobBlockCancelled = 6,
    PrintJobBlockOutOfPaper = 7,
    PrintJobBlockOutOfInk = 8,
    PrintJobBlockOutOfToner = 9,
    PrintJobBlockJammed = 10,
    PrintJobBlockDoorOpen = 11,
    PrintJobBlockServiceRequest = 12,
    PrintJobBlockLowOnInk = 13,
    PrintJobBlockLowOnToner = 14,
    PrintJobBlockReallyLowOnInk = 15,
    PrintJobBlockBadCertificate = 16,
    PrintJobBlockDriverException = 17,
    PrintJobBlockAccountError = 18,
    PrintJobBlockPrintPermissionError = 19,
    PrintJobBlockPrintColorPermissionError = 20,
    PrintJobBlockNetworkError = 21,
    PrintJobBlockServerConnectionError = 22,
    PrintJobBlockLargeFileError = 23,
    PrintJobBlockFileParsingError = 24,
    PrintJobBlockSlowFileConversion = 25,
    PrintJobRunningUploadingFiles = 26,
    PrintJobRunningConvertingFiles = 27,
    PrintJobBlockFileUploadingError = 30,
    PrintJobBlockDriverMissing = 34,
    PrintJobBlockInterrupt = 35,
    PrintJobBlockPrinterUnavailable = 98,
    PrintJobBlockUnknown = 99
}

/// <summary>
/// PrintErrorCode 枚举
/// </summary>
public enum PrintErrorCode
{
    EPrintNone = 0,
    EPrintNoPermission = 201,
    EPrintInvalidParameter = 401,
    EPrintGenericFailure = 13100001,
    EPrintRpcFailure = 13100002,
    EPrintServerFailure = 13100003,
    EPrintInvalidExtension = 13100004,
    EPrintInvalidPrinter = 13100005,
    EPrintInvalidPrintJob = 13100006,
    EPrintFileIo = 13100007,
    EPrintTooManyFiles = 13100010,
    EPrintSmbLoginLockout = 13100012,
    EPrintSmbConnectionFailure = 13100013,
    EPrintSmbInvalidCredentials = 13100014
}

/// <summary>
/// ApplicationEvent 枚举
/// </summary>
public enum ApplicationEvent
{
    ApplicationCreated = 0,
    ApplicationClosedForStarted = 1,
    ApplicationClosedForCanceled = 2
}

/// <summary>
/// PrintQuality 枚举
/// </summary>
public enum PrintQuality
{
    QualityDraft = 3,
    QualityNormal = 4,
    QualityHigh = 5
}

/// <summary>
/// PrintOrientationMode 枚举
/// </summary>
public enum PrintOrientationMode
{
    OrientationModePortrait = 0,
    OrientationModeLandscape = 1,
    OrientationModeReverseLandscape = 2,
    OrientationModeReversePortrait = 3,
    OrientationModeNone = 4
}

/// <summary>
/// PrinterStatus 枚举
/// </summary>
public enum PrinterStatus
{
    PrinterIdle = 0,
    PrinterBusy = 1,
    PrinterUnavailable = 2
}

/// <summary>
/// PrinterEvent 枚举
/// </summary>
public enum PrinterEvent
{
    PrinterEventAdded = 0,
    PrinterEventDeleted = 1,
    PrinterEventStateChanged = 2,
    PrinterEventInfoChanged = 3,
    PrinterEventPreferenceChanged = 4,
    PrinterEventLastUsedPrinterChanged = 5
}

/// <summary>
/// DefaultPrinterType 枚举
/// </summary>
public enum DefaultPrinterType
{
    DefaultPrinterTypeSetByUser = 0,
    DefaultPrinterTypeLastUsedPrinter = 1
}

/// <summary>
/// WatermarkHandleResult 枚举
/// </summary>
public enum WatermarkHandleResult
{
    WatermarkHandleSuccess = 0,
    WatermarkHandleFailure = 1
}