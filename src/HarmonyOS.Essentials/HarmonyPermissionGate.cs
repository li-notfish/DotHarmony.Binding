#nullable enable
using System;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;

namespace HarmonyOS.Essentials;

internal interface IHarmonyPermissionGate
{
    Task EnsureGrantedAsync(string permission);
}

internal sealed class HarmonyPermissionGate : IHarmonyPermissionGate
{
    private readonly string _permission;

    public HarmonyPermissionGate(string permission)
    {
        _permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    public async Task EnsureGrantedAsync(string permission)
    {
        if (permission != _permission)
            throw new ArgumentException($"Unexpected permission: {permission}", nameof(permission));

        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
            throw new PermissionException("host did not export globalThis.abilityContext");

        var manager = AbilityAccessCtrl.CreateAtManager();
        var status = manager.GetSelfPermissionStatus(permission);
        if (status == HarmonyOS.ArkUI.PermissionStatus.Granted)
            return;

        await manager.RequestPermissionsFromUserAsync(context, [permission]).ConfigureAwait(true);

        status = manager.GetSelfPermissionStatus(permission);
        if (status != HarmonyOS.ArkUI.PermissionStatus.Granted)
            throw new PermissionException($"Permission was not granted: {permission}");
    }
}
