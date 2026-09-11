using System;

using System.ComponentModel;

namespace HarmonyOS.ArkUI;

[Flags]
/// <summary>
/// MonitorObjectType 枚举
/// </summary>
public enum MonitorObjectType
{
    [Description("-1")]
    All,
    CustomComponent = 1,
    Window = 2,
    NodeContainer = 4,
    XComponent = 8,
    Ability = 16
}