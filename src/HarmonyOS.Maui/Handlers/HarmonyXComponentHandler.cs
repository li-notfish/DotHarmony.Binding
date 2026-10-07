#nullable enable
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkXComponent = HarmonyOS.ArkUI.XComponent;

namespace HarmonyOS.Maui.Handlers;

internal sealed class HarmonyXComponentHandler : HarmonyViewHandler<HarmonyXComponentView, ArkXComponent>
{
    public static PropertyMapper<HarmonyXComponentView, HarmonyXComponentHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(HarmonyXComponentView.XComponentId)] = MapXComponentId,
        [nameof(HarmonyXComponentView.Type)] = MapType,
    };

    public HarmonyXComponentHandler() : base(Mapper) { }

    protected override ArkXComponent CreatePlatformView()
    {
        var node = new ArkXComponent();
        node.EnsureSurfaceCallbacks();
        return node;
    }

    protected override void ConnectHandler(ArkXComponent platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SurfaceCreated += OnSurfaceCreated;
        platformView.SurfaceChanged += OnSurfaceChanged;
        platformView.SurfaceDestroyed += OnSurfaceDestroyed;
    }

    protected override void DisconnectHandler(ArkXComponent platformView)
    {
        platformView.SurfaceCreated -= OnSurfaceCreated;
        platformView.SurfaceChanged -= OnSurfaceChanged;
        platformView.SurfaceDestroyed -= OnSurfaceDestroyed;
        base.DisconnectHandler(platformView);
    }

    public static void MapXComponentId(HarmonyXComponentHandler handler, HarmonyXComponentView view)
    {
        var id = view.XComponentId ?? string.Empty;
        handler.PlatformView.SetXComponentId(id);
    }

    public static void MapType(HarmonyXComponentHandler handler, HarmonyXComponentView view)
    {
        // ArkUI XComponent 的类型在节点创建时固定；这里仅保留 MAUI 侧状态。
        HiLog.Debug("HarmonyHost", $"[XComponent] Type={view.Type}");
    }

    private void OnSurfaceCreated(nint handle)
        => VirtualView?.RaiseSurfaceCreated(handle);

    private void OnSurfaceChanged(nint handle, ulong width, ulong height)
        => VirtualView?.RaiseSurfaceChanged(handle, width, height);

    private void OnSurfaceDestroyed(nint handle)
        => VirtualView?.RaiseSurfaceDestroyed(handle);
}
