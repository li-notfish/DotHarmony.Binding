// MAUI Essentials 鸿蒙实现装配器（ROADMAP 2.4）。
// MAUI 10 的 Essentials 静态入口（DeviceInfo.Current / DeviceDisplay.MainDisplayInfo /
// AppInfo.Name / Clipboard.SetTextAsync ...）在 netstandard 产物里缺省实现全部 throw——
// 每个静态类留有 internal 注入点（SetCurrent / SetDefault）。
// 本桥经 [DynamicDependency] 收根 + CreateDelegate 缓存强类型委托完成注入（AOT 安全，
// 与 MauiGestureBridge 同款模式，禁止逐次 MethodInfo.Invoke）。
// 范围说明：依赖宿主 DI 容器（MauiAppBuilder.ConfigureEssentials）的解析路径本宿主不使用——
// 我们直接驱动静态入口，静态入口又同步驱动接口消费方，二者一致。
#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Maui.Essentials;

public static class HarmonyEssentials
{
    static bool _installed;

    /// <summary>注入全部已实现的 Essentials 服务（幂等；MauiHarmonyHost.Run 时调用）。</summary>
    public static void Install()
    {
        if (_installed)
            return;
        _installed = true;

        SetImplementation(typeof(global::Microsoft.Maui.Devices.DeviceInfo), "SetCurrent",
            new HarmonyDeviceInfo(), static (m, impl) => m.CreateDelegate<Action<IDeviceInfo>>()(impl));
        SetImplementation(typeof(DeviceDisplay), "SetCurrent",
            new HarmonyDeviceDisplay(), static (m, impl) => m.CreateDelegate<Action<IDeviceDisplay>>()(impl));
        SetImplementation(typeof(AppInfo), "SetCurrent",
            new HarmonyAppInfo(), static (m, impl) => m.CreateDelegate<Action<IAppInfo>>()(impl));
        SetImplementation(typeof(Clipboard), "SetDefault",
            new HarmonyClipboard(), static (m, impl) => m.CreateDelegate<Action<IClipboard>>()(impl));

        HiLog.Info("Essentials", "HarmonyOS Essentials installed: DeviceInfo / DeviceDisplay / AppInfo / Clipboard");
    }

    private delegate void Setter<TInterface>(MethodInfo m, TInterface impl);

    private static void SetImplementation<TInterface>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)] Type staticClass,
        string setterName,
        TInterface implementation,
        Setter<TInterface> invoke)
    {
        var m = staticClass.GetMethod(setterName, BindingFlags.NonPublic | BindingFlags.Static, null, [typeof(TInterface)], null)
            ?? throw new MissingMethodException(staticClass.Name, setterName);
        invoke(m, implementation);
    }
}
