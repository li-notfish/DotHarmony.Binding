// Platform startup (mirrors Platforms/Android/MainActivity placement in MAUI):
// NativeAOT shared-library exports plus the module initializer.
// Verified: the current OpenHarmony ILC toolchain only exports [UnmanagedCallersOnly]
// methods from the entry assembly (library-defined ones never reach app.so's dynamic
// symbol table), so every libapp.so app needs this thin forwarding layer.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using global::HarmonyOS.Bindings.Hosting;
using global::HarmonyOS.Maui.Hosting;

namespace HarmonyApp1.Platforms.HarmonyOS;

internal static class NativeExports
{
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    private static int HarmonyInit(nint env) => Host.InitializeCore(env);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    private static int HarmonyBuildUI(nint env, nint nodeContentValue)
        => Host.BuildUICore(env, nodeContentValue);

    [UnmanagedCallersOnly(EntryPoint = "HarmonyPopPage")]
    private static int HarmonyPopPage(nint env)
        => global::HarmonyOS.Maui.Hosting.HarmonyNavigation.OnBackRequested() ? 1 : 0;
}

// Theme changes need no export: HarmonyOS.Maui registers a function pointer at
// RunApplication time via libentry's HarmonyHostSetThemeChangedCallback export
// (see MauiHarmonyHost). Do not re-add a HarmonyThemeChanged export here.

internal static class Bootstrap
{
    // Runs when the NativeAOT module loads, before the host calls HarmonyBuildUI
    [ModuleInitializer]
    internal static void Init() => Program.Register();
}
