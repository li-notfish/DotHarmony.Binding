using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// CustomResult 枚举
/// </summary>
public enum CustomResult
{
    ResultFail = 0,
    ResultNext = 1,
    ResultFinish = 2
}

/// <summary>
/// EapEapMethod 枚举
/// </summary>
public enum EapEapMethod
{
    EapNone = 0,
    EapPeap = 1,
    EapTls = 2,
    EapTtls = 3,
    EapPwd = 4,
    EapSim = 5,
    EapAka = 6,
    EapAkaPrime = 7,
    EapUnauthTls = 8
}

/// <summary>
/// EapPhase2Method 枚举
/// </summary>
public enum EapPhase2Method
{
    Phase2None = 0,
    Phase2Pap = 1,
    Phase2Mschap = 2,
    Phase2Mschapv2 = 3,
    Phase2Gtc = 4,
    Phase2Sim = 5,
    Phase2Aka = 6,
    Phase2AkaPrime = 7
}