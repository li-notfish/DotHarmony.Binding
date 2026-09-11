using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// XmlEventType 枚举
/// </summary>
public enum XmlEventType
{
    StartDocument,
    EndDocument,
    StartTag,
    EndTag,
    Text,
    Cdsect,
    Comment,
    Docdecl,
    Instruction,
    EntityReference,
    Whitespace
}