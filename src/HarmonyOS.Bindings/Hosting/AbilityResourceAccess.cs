#nullable enable
using System;
using HarmonyOS.Interop;

namespace HarmonyOS.Bindings.Hosting;

/// <summary>
/// 宿主 abilityContext 资源访问入口（手写，非生成器产物）。
/// 宿主 EntryAbility.ets 导出 globalThis.abilityContext；模块级 GetResourceManager
/// 在宿主环境拿不到正确实例，resourceManager 必须从该上下文读取。
/// 裸 napi 值跨句柄范围会失效（ESSENTIALS.md 已踩坑表），每次调用重新读取，勿缓存。
/// </summary>
public static class AbilityResourceAccess
{
    /// <summary>globalThis.abilityContext；宿主未导出时返回 Zero。</summary>
    public static IntPtr GetAbilityContext()
        => NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);

    /// <summary>abilityContext.resourceManager；上下文或属性缺失时返回 Zero。</summary>
    public static IntPtr GetResourceManager()
    {
        var context = GetAbilityContext();
        if (context == IntPtr.Zero)
            return IntPtr.Zero;
        return NodeApi.GetProperty(context, "resourceManager");
    }
}
