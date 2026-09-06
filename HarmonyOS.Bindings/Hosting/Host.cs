#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Bindings.Hosting;

/// <summary>
/// .NET 应用在鸿蒙上的宿主入口（由 NativeAOT 导出为 libapp.so 符号）。
/// C shim（libentry.so）从 ArkTS 收到调用后转发到这里。
/// </summary>
public static unsafe class Host
{
    /// <summary>
    /// 根 UI 构建器：宿主应用注册，参数为 ArkUI_NodeContentHandle。
    /// 在 UI 主线程同步调用。
    /// </summary>
    public static Action<IntPtr>? RootBuilder { get; set; }

    /// <summary>
    /// 将 .NET 构建的根节点挂载到 ArkTS ContentSlot 提供的 NodeContent。
    /// </summary>
    public static void AttachRoot(IntPtr contentHandle, ArkUINodeBase root)
    {
        ArkUINativeApi.NodeContentAddNode(contentHandle, root.Handle);
    }

    /// <summary>运行时初始化：绑定 env 与主线程。符号：HarmonyInit（原生入口，仅入口程序集会被导出）</summary>
    [UnmanagedCallersOnly(EntryPoint = "HarmonyInit")]
    public static int HarmonyInit(IntPtr env) => InitializeCore(env);

    /// <summary>普通方法版本（供应用的导出转发层调用）</summary>
    public static int InitializeCore(IntPtr env)
    {
        try
        {
            NapiEnv.Initialize(env);
            NativeMainThread.Capture();
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 构建 UI：将 ArkTS NodeContent 转换为原生句柄并调用 <see cref="RootBuilder"/>。
    /// 符号：HarmonyBuildUI（原生入口，仅入口程序集会被导出）
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "HarmonyBuildUI")]
    public static int HarmonyBuildUI(IntPtr env, IntPtr nodeContentValue) => BuildUICore(env, nodeContentValue);

    /// <summary>普通方法版本（供应用的导出转发层调用）</summary>
    public static int BuildUICore(IntPtr env, IntPtr nodeContentValue)
    {
        try
        {
            NapiEnv.Initialize(env);
            NativeMainThread.Capture();

            var content = ArkUINativeApi.GetNodeContentFromNapiValue(env, nodeContentValue);
            HiLog.Info("HarmonyHost", $"NodeContent handle = {content:X}");
            if (RootBuilder == null)
                throw new InvalidOperationException("Host.RootBuilder was not set by the application");
            RootBuilder(content);
            return 0;
        }
        catch (Exception ex)
        {
            HiLog.Error("HarmonyHost", $"HarmonyBuildUI: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            return -2;
        }
    }
}
