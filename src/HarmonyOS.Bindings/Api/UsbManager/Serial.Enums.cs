using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// BaudRates 枚举
/// </summary>
public enum BaudRates
{
    Baudrate50 = 50,
    Baudrate75 = 75,
    Baudrate110 = 110,
    Baudrate134 = 134,
    Baudrate150 = 150,
    Baudrate200 = 200,
    Baudrate300 = 300,
    Baudrate600 = 600,
    Baudrate1200 = 1200,
    Baudrate1800 = 1800,
    Baudrate2400 = 2400,
    Baudrate4800 = 4800,
    Baudrate9600 = 9600,
    Baudrate19200 = 19200,
    Baudrate38400 = 38400,
    Baudrate57600 = 57600,
    Baudrate115200 = 115200,
    Baudrate230400 = 230400,
    Baudrate460800 = 460800,
    Baudrate500000 = 500000,
    Baudrate576000 = 576000,
    Baudrate921600 = 921600,
    Baudrate1000000 = 1000000,
    Baudrate1152000 = 1152000,
    Baudrate1500000 = 1500000,
    Baudrate2000000 = 2000000,
    Baudrate2500000 = 2500000,
    Baudrate3000000 = 3000000,
    Baudrate3500000 = 3500000,
    Baudrate4000000 = 4000000
}

/// <summary>
/// UsbManagerSerialDataBits 枚举
/// </summary>
public enum UsbManagerSerialDataBits
{
    Databit8 = 8,
    Databit7 = 7,
    Databit6 = 6,
    Databit5 = 5
}

/// <summary>
/// UsbManagerSerialParity 枚举
/// </summary>
public enum UsbManagerSerialParity
{
    ParityNone = 0,
    ParityOdd = 1,
    ParityEven = 2,
    ParityMark = 3,
    ParitySpace = 4
}

/// <summary>
/// UsbManagerSerialStopBits 枚举
/// </summary>
public enum UsbManagerSerialStopBits
{
    Stopbit1 = 0,
    Stopbit2 = 1
}