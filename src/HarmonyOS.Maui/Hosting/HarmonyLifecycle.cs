#nullable enable
using System.Collections.Concurrent;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Hosting;

/// <summary>
/// Bridges HarmonyOS ability lifecycle events to a MAUI window.
/// Controllers are keyed by abilityId so later hosts can support multiple
/// abilities/windows without changing the notification contract again.
/// </summary>
internal sealed class HarmonyLifecycleController
{
    internal const int Foreground = 1;
    internal const int Background = 2;
    internal const int Destroy = 3;

    private const long DefaultAbilityId = 0;
    private static readonly ConcurrentDictionary<long, HarmonyLifecycleController> Controllers = new();

    internal static HarmonyLifecycleController? Current => GetController(DefaultAbilityId);

    private static bool _pendingForeground;

    private readonly Window _window;
    private bool _created;
    private bool _activated;
    private bool _foregrounded;
    private bool _destroyed;

    private HarmonyLifecycleController(Window window) => _window = window;

    internal static void Reset()
    {
        Controllers.Clear();
        _pendingForeground = false;
    }

    internal static void Attach(Window window, long abilityId = DefaultAbilityId)
    {
        var controller = new HarmonyLifecycleController(window);
        Controllers[abilityId] = controller;
    }

    internal static HarmonyLifecycleController? GetController(long abilityId)
        => Controllers.TryGetValue(abilityId, out var controller) ? controller : null;

    internal static void Notify(int lifecycleEvent, long abilityId = DefaultAbilityId)
    {
        var current = GetController(abilityId);
        if (current is null)
        {
            if (abilityId == DefaultAbilityId && lifecycleEvent == Foreground)
            {
                _pendingForeground = true;
                Interop.HiLog.Info("HarmonyHost", "foreground received before MAUI window creation; deferred");
                return;
            }

            if (abilityId == DefaultAbilityId && lifecycleEvent is Background or Destroy)
                _pendingForeground = false;

            Interop.HiLog.Warn(
                "HarmonyHost",
                $"lifecycle event {lifecycleEvent} for ability {abilityId} received before MAUI window creation");
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

        ((Microsoft.Maui.IWindow)_window).Backgrounding(new Microsoft.Maui.PersistedState());
        if (_activated)
        {
            ((Microsoft.Maui.IWindow)_window).Deactivated();
            _activated = false;
        }
        ((Microsoft.Maui.IWindow)_window).Stopped();
        _foregrounded = false;
        Interop.HiLog.Info("HarmonyHost", "MAUI lifecycle: Backgrounding/Deactivated/Stopped");
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
