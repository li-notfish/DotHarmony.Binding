using Microsoft.Maui.Controls;
using HarmonyOS.ArkUI;

namespace HarmonyOS.Maui.Hosting;

/// <summary>
/// 宿主级 Application 装配点。
/// </summary>
internal static class HarmonyApplication
{
    /// <summary>
    /// 获取或创建最小 Application，并把全局资源合并到 Application.Resources，
    /// 让页面/控件按 MAUI 标准资源继承链解析 DynamicResource。
    /// 重复 Attach 复用现有实例，仅增量合入未挂过的字典 —— 不丢弃
    /// 运行时写入 Application.Resources 的内容。
    /// </summary>
    internal static Application EnsureCurrent(ResourceDictionary? globalResources)
        => EnsureCurrent(globalResources, application: null);

    /// <summary>
    /// 真实 Application 宿主（RunApplication 路径）：传入应用自己的 app 直接复用，
    /// Resources/UserAppTheme/服务上下文保持应用自带；仅做 Application.Current 接线。
    /// 传 null 时维持最小 Application 的旧语义（Run(globalResources) 路径）。
    /// </summary>
    internal static Application EnsureCurrent(ResourceDictionary? globalResources, Application? application)
    {
        var app = application ?? Application.Current;
        if (app is null)
        {
            app = new Application();
            Application.SetCurrentApplication(app);
        }
        else if (!ReferenceEquals(Application.Current, app))
        {
            Application.SetCurrentApplication(app);
        }

        if (globalResources is not null && !app.Resources.MergedDictionaries.Contains(globalResources))
            app.Resources.MergedDictionaries.Add(globalResources);

        // 对齐官方 MAUI：系统主题写入 PlatformAppTheme；UserAppTheme 保持 Unspecified
        // 才能继续跟随系统，开发者显式设置时则优先覆盖。
        ((Microsoft.Maui.IApplication)app).ThemeChanged();

        return app;
    }

    /// <summary>
    /// 系统配置变化入口：由 ArkTS Ability 的 onConfigurationUpdate 经 native shim 调用。
    /// ThemeChanged 会重新读取 AppInfo.RequestedTheme 并触发 RequestedThemeChanged。
    /// </summary>
    internal static int ThemeChangedCore(nint env, int colorMode)
    {
        try
        {
            global::HarmonyOS.Bindings.Hosting.Host.InitializeCore(env);
            var app = Application.Current;
            if (app is not null)
            {
                global::HarmonyOS.Essentials.HarmonyAppInfo.SetConfigurationColorMode(colorMode);
                ((Microsoft.Maui.IApplication)app).ThemeChanged();
            }
            return 0;
        }
        catch
        {
            return -1;
        }
    }
}
