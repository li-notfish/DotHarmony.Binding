#nullable enable
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkXComponent = HarmonyOS.ArkUI.XComponent;

namespace HarmonyOS.Maui.Handlers;

internal sealed class HarmonyXComponentHandler : HarmonyViewHandler<HarmonyXComponentView, ArkXComponent>
{
    private readonly ArkUI_XComponentType _type;

    public static PropertyMapper<HarmonyXComponentView, HarmonyXComponentHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(HarmonyXComponentView.XComponentId)] = MapXComponentId,
        [nameof(HarmonyXComponentView.Type)] = MapType,
    };

    public HarmonyXComponentHandler(ArkUI_XComponentType type = ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_SURFACE)
        : base(Mapper)
    {
        _type = type;
    }

    protected override ArkXComponent CreatePlatformView()
    {
        var node = new ArkXComponent(_type);
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
        platformView.RemoveSurfaceCallbacks();
        base.DisconnectHandler(platformView);
    }

    public static void MapXComponentId(HarmonyXComponentHandler handler, HarmonyXComponentView view)
    {
        var id = view.XComponentId ?? string.Empty;
        handler.PlatformView.SetXComponentId(id);
    }

    public static void MapType(HarmonyXComponentHandler handler, HarmonyXComponentView view)
    {
        if (view.Type != handler._type)
        {
            HiLog.Warn(
                "HarmonyHost",
                $"[XComponent] runtime type change from {handler._type} to {view.Type} is ignored; ArkUI fixes XComponent type at creation");
        }
    }

    private void OnSurfaceCreated(nint handle)
        => VirtualView?.RaiseSurfaceCreated(handle);

    private void OnSurfaceChanged(nint handle, ulong width, ulong height)
        => VirtualView?.RaiseSurfaceChanged(handle, width, height);

    private void OnSurfaceDestroyed(nint handle)
        => VirtualView?.RaiseSurfaceDestroyed(handle);
}
