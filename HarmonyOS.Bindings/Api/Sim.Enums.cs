using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SimType 枚举
/// </summary>
public enum SimType
{
    Psim = 0,
    Esim = 1
}

/// <summary>
/// SimCardType 枚举
/// </summary>
public enum SimCardType
{
    [Description("-1")]
    UnknownCard,
    SingleModeSimCard = 10,
    SingleModeUsimCard = 20,
    SingleModeRuimCard = 30,
    DualModeCgCard = 40,
    CtNationalRoamingCard = 41,
    CuDualModeCard = 42,
    DualModeTelecomLteCard = 43,
    DualModeUgCard = 50,
    SingleModeIsimCard = 60
}

/// <summary>
/// SimState 枚举
/// </summary>
public enum SimState
{
    SimStateUnknown = 0,
    SimStateNotPresent = 1,
    SimStateLocked = 2,
    SimStateNotReady = 3,
    SimStateReady = 4,
    SimStateLoaded = 5
}