using System;

namespace HarmonyOS.ArkUI;

/// <summary>
/// NetFirewallRuleDirection 枚举
/// </summary>
public enum NetFirewallRuleDirection
{
    RuleIn = 1,
    RuleOut = 2
}

/// <summary>
/// FirewallRuleAction 枚举
/// </summary>
public enum FirewallRuleAction
{
    RuleAllow = 0,
    RuleDeny = 1
}

/// <summary>
/// NetFirewallRuleType 枚举
/// </summary>
public enum NetFirewallRuleType
{
    RuleIP = 1,
    RuleDomain = 2,
    RuleDns = 3
}

/// <summary>
/// NetFirewallOrderField 枚举
/// </summary>
public enum NetFirewallOrderField
{
    OrderByRuleName = 1,
    OrderByRecordTime = 100
}

/// <summary>
/// NetFirewallOrderType 枚举
/// </summary>
public enum NetFirewallOrderType
{
    OrderAsc = 1,
    OrderDesc = 100
}