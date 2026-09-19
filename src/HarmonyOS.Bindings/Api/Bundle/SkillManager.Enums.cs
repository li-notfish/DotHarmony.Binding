using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// SkillInfoFlag 枚举
/// </summary>
public enum SkillInfoFlag
{
    GetSkillInfoDefault = 0,
    GetSkillInfoWithDescription = 1,
    GetSkillInfoWithSrcEntries = 2,
    GetSkillInfoWithPermissions = 4,
    GetSkillInfoWithRequestPermissions = 8
}