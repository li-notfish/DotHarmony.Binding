#nullable enable
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Handlers;

/// <summary>HarmonyOS XComponent 的内部 MAUI 宿主视图；公开 API 不变。</summary>
internal sealed class HarmonyXComponentView : View
{
    public static readonly BindableProperty XComponentIdProperty = BindableProperty.Create(
        nameof(XComponentId), typeof(string), typeof(HarmonyXComponentView), string.Empty);

    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(ArkUI_XComponentType), typeof(HarmonyXComponentView),
        ArkUI_XComponentType.ARKUI_XCOMPONENT_TYPE_SURFACE);

    public string? XComponentId
    {
        get => (string?)GetValue(XComponentIdProperty);
        set => SetValue(XComponentIdProperty, value);
    }

    public ArkUI_XComponentType Type
    {
        get => (ArkUI_XComponentType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    internal event Action<nint>? SurfaceCreated;
    internal event Action<nint, ulong, ulong>? SurfaceChanged;
    internal event Action<nint>? SurfaceDestroyed;

    internal void RaiseSurfaceCreated(nint handle) => SurfaceCreated?.Invoke(handle);

    internal void RaiseSurfaceChanged(nint handle, ulong width, ulong height)
        => SurfaceChanged?.Invoke(handle, width, height);

    internal void RaiseSurfaceDestroyed(nint handle) => SurfaceDestroyed?.Invoke(handle);
}
