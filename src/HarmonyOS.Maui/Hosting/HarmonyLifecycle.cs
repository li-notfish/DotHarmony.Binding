#nullable enable
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Hosting;

/// <summary>
/// Bridges HarmonyOS ability lifecycle events to the current MAUI window.
/// The controller is idempotent because ArkTS can deliver both
/// onWindowStageDestroy and onDestroy for the same termination.
/// </summary>
internal sealed class HarmonyLifecycleController
{
    internal const int Foreground = 1;
    internal const int Background = 2;
    internal const int Destroy = 3;

    internal static HarmonyLifecycleController? Current { get; private set; }

    private static bool _pendingForeground;

    private readonly Window _window;
    private bool _created;
    private bool _activated;
    private bool _foregrounded;
    private bool _destroyed;

    private HarmonyLifecycleController(Window window) => _window = window;

    internal static void Reset()
    {
        Current = null;
        _pendingForeground = false;
    }

    internal static void Attach(Window window) => Current = new HarmonyLifecycleController(window);

    internal static void Notify(int lifecycleEvent)
    {
        var current = Current;
        if (current is null)
        {
            if (lifecycleEvent == Foreground)
            {
                _pendingForeground = true;
                Interop.HiLog.Info("HarmonyHost", "foreground received before MAUI window creation; deferred");
            }
            else
            {
                Interop.HiLog.Warn("HarmonyHost", $"lifecycle event {lifecycleEvent} received before MAUI window creation");
            }
            return;
        }

        switch (lifecycleEvent)
        {
            case Foreground:
                current.ForegroundCore();
                break;
            case Background:
                current.BackgroundCore();
                break;
            case Destroy:
                current.DestroyCore();
                break;
            default:
                Interop.HiLog.Warn("HarmonyHost", $"unknown lifecycle event: {lifecycleEvent}");
                break;
        }
    }

    internal void Created()
    {
        if (_created || _destroyed)
            return;

        ((Microsoft.Maui.IWindow)_window).Created();
        _created = true;
        Interop.HiLog.Info("HarmonyHost", "MAUI lifecycle: Created");

        if (_pendingForeground)
        {
            _pendingForeground = false;
            ForegroundCore();
        }
    }

    private void ForegroundCore()
    {
        if (!_created || _destroyed || _foregrounded)
            return;

        ((Microsoft.Maui.IWindow)_window).Resumed();
        if (!_activated)
        {
            ((Microsoft.Maui.IWindow)_window).Activated();
            _activated = true;
        }
        _foregrounded = true;
        Interop.HiLog.Info("HarmonyHost", "MAUI lifecycle: Resumed/Activated");
    }

    private void BackgroundCore()
    {
        if (!_created || _destroyed || !_foregrounded)
            return;

        if (_activated)
        {
            ((Microsoft.Maui.IWindow)_window).Deactivated();
            _activated = false;
        }
        ((Microsoft.Maui.IWindow)_window).Stopped();
        _foregrounded = false;
        Interop.HiLog.Info("HarmonyHost", "MAUI lifecycle: Deactivated/Stopped");
    }

    private void DestroyCore()
    {
        if (!_created || _destroyed)
            return;

        ((Microsoft.Maui.IWindow)_window).Destroying();
        _destroyed = true;
        _activated = false;
        _foregrounded = false;
        Interop.HiLog.Info("HarmonyHost", "MAUI lifecycle: Destroying");
    }
}
