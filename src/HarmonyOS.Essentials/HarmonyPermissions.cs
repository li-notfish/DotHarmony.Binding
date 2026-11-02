// MAUI Permissions 的鸿蒙运行时实现。
// MAUI 10 的 Permissions 静态类没有 SetCurrent/SetDefault 注入点——平台实现全部由
// 编译期 TFM partial 提供，net10.0 下 BasePlatformPermission 的方法全部 throw
// NotImplementedInReferenceAssembly，因此无法走 HarmonyEssentials 的静态注入装配。
// 本类补齐运行时缺口：
// 1. 泛型入口与 MAUI Permissions 同形状，以 MAUI 权限类型为 key 映射到
//    @ohos.abilityAccessCtrl 权限组后经 atManager 查询/请求；
// 2. 常用权限提供 BasePlatformPermission 派生类，Permissions.RequestAsync<T>()
//    等标准调用路径传入派生类型即可工作（override 覆盖 netstandard 兜底实现）。
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;

namespace HarmonyOS.Essentials;

/// <summary>鸿蒙宿主上的 MAUI Permissions 运行时实现。</summary>
public static class HarmonyPermissions
{
    private static readonly string[] LocationPermissions =
        ["ohos.permission.APPROXIMATELY_LOCATION", "ohos.permission.LOCATION"];

    // MAUI 权限类型 → HarmonyOS 用户级权限。空数组 = 无需运行时权限（系统 picker、
    // 应用沙箱内访问直接 Granted）。未收录的类型在请求时给出明确错误而不是静默拒绝。
    private static readonly Dictionary<Type, string[]> PermissionMap = new()
    {
        [typeof(Permissions.LocationWhenInUse)] = LocationPermissions,
        [typeof(Permissions.LocationAlways)] = LocationPermissions,
        [typeof(Permissions.Camera)] = ["ohos.permission.CAMERA"],
        [typeof(Permissions.Microphone)] = ["ohos.permission.MICROPHONE"],
        [typeof(Permissions.Bluetooth)] = ["ohos.permission.ACCESS_BLUETOOTH"],
        [typeof(Permissions.Photos)] = [],
        [typeof(Permissions.PhotosAddOnly)] = [],
        [typeof(Permissions.Media)] = [],
        [typeof(Permissions.StorageRead)] = [],
        [typeof(Permissions.StorageWrite)] = [],
        [typeof(Permissions.Vibrate)] = [],
        [typeof(Permissions.Flashlight)] = [],
        [typeof(Permissions.NetworkState)] = [],
        [typeof(Permissions.Battery)] = [],
        [typeof(Permissions.LaunchApp)] = [],
        [typeof(Permissions.Maps)] = [],
    };

    public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
        where TPermission : Permissions.BasePermission, new()
        => Task.FromResult(CheckCore(Resolve<TPermission>()));

    public static async Task<PermissionStatus> RequestAsync<TPermission>()
        where TPermission : Permissions.BasePermission, new()
    {
        var permissions = Resolve<TPermission>();
        var manager = AbilityAccessCtrl.CreateAtManager();
        var missing = permissions
            .Where(p => manager.GetSelfPermissionStatus(p) != HarmonyOS.ArkUI.PermissionStatus.Granted)
            .ToArray();
        if (missing.Length > 0)
            await manager.RequestPermissionsFromUserAsync(RequireAbilityContext(), missing).ConfigureAwait(true);

        return CheckCore(permissions);
    }

    public static bool ShouldShowRationale<TPermission>()
        where TPermission : Permissions.BasePermission, new()
        => false;

    internal static void EnsureDeclared<TPermission>()
        where TPermission : Permissions.BasePermission, new()
    {
        // module.json5 声明在构建期由 HarmonyOS.Maui.Permissions 生成器推导校验；
        // atManager 没有运行时"是否已声明"查询，未声明时 RequestAsync 只会得到 Denied。
    }

    private static string[] Resolve<TPermission>() where TPermission : Permissions.BasePermission, new()
        => PermissionMap.TryGetValue(typeof(TPermission), out var permissions)
            ? permissions
            : throw new PermissionException(
                $"{typeof(TPermission).Name} is not mapped to a HarmonyOS permission; extend HarmonyPermissions");

    private static IntPtr RequireAbilityContext()
    {
        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        return context != IntPtr.Zero
            ? context
            : throw new PermissionException("host did not export globalThis.abilityContext");
    }

    private static PermissionStatus CheckCore(string[] permissions)
    {
        if (permissions.Length == 0)
            return PermissionStatus.Granted;

        var manager = AbilityAccessCtrl.CreateAtManager();
        return permissions
            .Select(p => ToMauiStatus(manager.GetSelfPermissionStatus(p)))
            .FirstOrDefault(s => s != PermissionStatus.Granted, PermissionStatus.Granted);
    }

    // ArkUI PermissionStatus（Denied=-1、Granted=0、NotDetermined=1、Invalid=2、Restricted=3）
    // 与 MAUI PermissionStatus（Unknown=0、Denied=1、...、Granted=3）数值含义不同，必须显式映射。
    // 绑定生成器缺陷：Denied 未显式赋 -1（[Description] 只是元数据），枚举成员与 Granted 同为 0，
    // 故按原始数值判断而不是成员比较。
    private static PermissionStatus ToMauiStatus(HarmonyOS.ArkUI.PermissionStatus status) => (int)status switch
    {
        0 => PermissionStatus.Granted,
        3 => PermissionStatus.Restricted,
        -1 => PermissionStatus.Denied,
        _ => PermissionStatus.Unknown,
    };

    /// <summary>
    /// 鸿蒙权限公共骨架：四个虚方法统一转发到 <typeparamref name="TMaui"/> 对应的权限组，
    /// 使 Permissions.RequestAsync&lt;T&gt;() 标准路径（泛型 new 约束）不再落到 netstandard 兜底 throw。
    /// </summary>
    public abstract class HarmonyPermission<TMaui> : Permissions.BasePlatformPermission
        where TMaui : Permissions.BasePlatformPermission, new()
    {
        public override Task<PermissionStatus> CheckStatusAsync() => CheckStatusAsync<TMaui>();
        public override Task<PermissionStatus> RequestAsync() => RequestAsync<TMaui>();
        public override void EnsureDeclared() { }
        public override bool ShouldShowRationale() => false;
    }

    public sealed class LocationWhenInUse : HarmonyPermission<Permissions.LocationWhenInUse> { }

    public sealed class LocationAlways : HarmonyPermission<Permissions.LocationAlways> { }

    public sealed class Camera : HarmonyPermission<Permissions.Camera> { }

    public sealed class Microphone : HarmonyPermission<Permissions.Microphone> { }

    public sealed class Bluetooth : HarmonyPermission<Permissions.Bluetooth> { }

    public sealed class Photos : HarmonyPermission<Permissions.Photos> { }

    public sealed class StorageRead : HarmonyPermission<Permissions.StorageRead> { }

    public sealed class StorageWrite : HarmonyPermission<Permissions.StorageWrite> { }

    public sealed class Vibrate : HarmonyPermission<Permissions.Vibrate> { }
}
