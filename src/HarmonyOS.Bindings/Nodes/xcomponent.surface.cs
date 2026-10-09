#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

public unsafe partial class XComponent
{
    private void* _surfaceHolder;
    private void* _surfaceCallback;
    private GCHandle _surfaceGcHandle;

    internal event Action<nint>? SurfaceCreated;
    internal event Action<nint, ulong, ulong>? SurfaceChanged;
    internal event Action<nint>? SurfaceDestroyed;

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_Create")]
    private static extern void* SurfaceHolderCreate(ArkUI_NodeHandle node);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_Dispose")]
    private static extern void SurfaceHolderDispose(void* surfaceHolder);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_SetUserData")]
    private static extern int SurfaceHolderSetUserData(void* surfaceHolder, void* userData);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_GetUserData")]
    private static extern void* SurfaceHolderGetUserData(void* surfaceHolder);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceCallback_Create")]
    private static extern void* SurfaceCallbackCreate();

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceCallback_Dispose")]
    private static extern void SurfaceCallbackDispose(void* callback);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceCallback_SetSurfaceCreatedEvent")]
    private static extern void SurfaceCallbackSetCreated(void* callback, delegate* unmanaged[Cdecl]<void*, void> onCreated);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceCallback_SetSurfaceChangedEvent")]
    private static extern void SurfaceCallbackSetChanged(void* callback, delegate* unmanaged[Cdecl]<void*, ulong, ulong, void> onChanged);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceCallback_SetSurfaceDestroyedEvent")]
    private static extern void SurfaceCallbackSetDestroyed(void* callback, delegate* unmanaged[Cdecl]<void*, void> onDestroyed);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_AddSurfaceCallback")]
    private static extern int SurfaceHolderAddCallback(void* surfaceHolder, void* callback);

    [DllImport("libace_ndk.z.so", EntryPoint = "OH_ArkUI_SurfaceHolder_RemoveSurfaceCallback")]
    private static extern int SurfaceHolderRemoveCallback(void* surfaceHolder, void* callback);

    internal void EnsureSurfaceCallbacks()
    {
        if (_surfaceHolder != null)
            return;

        _surfaceHolder = SurfaceHolderCreate(Handle);
        if (_surfaceHolder == null)
            return;

        _surfaceGcHandle = GCHandle.Alloc(this);
        SurfaceHolderSetUserData(_surfaceHolder, (void*)GCHandle.ToIntPtr(_surfaceGcHandle));

        _surfaceCallback = SurfaceCallbackCreate();
        if (_surfaceCallback == null)
        {
            SurfaceHolderDispose(_surfaceHolder);
            _surfaceHolder = null;
            _surfaceGcHandle.Free();
            return;
        }

        SurfaceCallbackSetCreated(_surfaceCallback, &OnSurfaceCreated);
        SurfaceCallbackSetChanged(_surfaceCallback, &OnSurfaceChanged);
        SurfaceCallbackSetDestroyed(_surfaceCallback, &OnSurfaceDestroyed);
        SurfaceHolderAddCallback(_surfaceHolder, _surfaceCallback);
    }

    internal void RemoveSurfaceCallbacks()
    {
        if (_surfaceHolder != null && _surfaceCallback != null)
            SurfaceHolderRemoveCallback(_surfaceHolder, _surfaceCallback);
        if (_surfaceCallback != null)
            SurfaceCallbackDispose(_surfaceCallback);
        if (_surfaceHolder != null)
            SurfaceHolderDispose(_surfaceHolder);
        if (_surfaceGcHandle.IsAllocated)
            _surfaceGcHandle.Free();

        _surfaceCallback = null;
        _surfaceHolder = null;
    }

    internal void SetXComponentId(string id)
        => SetStringAttribute(ArkUI_NodeAttributeType.NODE_XCOMPONENT_ID, id ?? string.Empty);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnSurfaceCreated(void* surfaceHolder)
    {
        if (FromSurfaceHolder(surfaceHolder) is { } x)
            x.SurfaceCreated?.Invoke((nint)surfaceHolder);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnSurfaceChanged(void* surfaceHolder, ulong width, ulong height)
    {
        if (FromSurfaceHolder(surfaceHolder) is { } x)
            x.SurfaceChanged?.Invoke((nint)surfaceHolder, width, height);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnSurfaceDestroyed(void* surfaceHolder)
    {
        if (FromSurfaceHolder(surfaceHolder) is { } x)
            x.SurfaceDestroyed?.Invoke((nint)surfaceHolder);
    }

    private static XComponent? FromSurfaceHolder(void* surfaceHolder)
    {
        var userData = SurfaceHolderGetUserData(surfaceHolder);
        if (userData == null)
            return null;

        var handle = GCHandle.FromIntPtr((nint)userData);
        return handle.Target as XComponent;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            RemoveSurfaceCallbacks();
        }

        base.Dispose(disposing);
    }
}
