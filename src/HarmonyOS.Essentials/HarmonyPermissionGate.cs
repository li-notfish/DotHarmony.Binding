#nullable enable
using System;
using System.Threading.Tasks;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;

namespace HarmonyOS.Essentials;

internal interface IHarmonyPermissionGate
{
    Task EnsureGrantedAsync(params string[] permissions);
}

internal sealed class HarmonyPermissionGate : IHarmonyPermissionGate
{
    private readonly string[] _permissions;

    public HarmonyPermissionGate(params string[] permissions)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));

        if (_permissions.Length == 0)
            throw new ArgumentException("At least one permission is required.", nameof(permissions));
    }

    public async Task EnsureGrantedAsync(params string[] permissions)
    {
        if (permissions.Length != _permissions.Length ||
            permissions.Where((permission, index) => permission != _permissions[index]).Any())
        {
            throw new ArgumentException(
                $"Unexpected permissions: {string.Join(", ", permissions)}",
                nameof(permissions));
        }

        var context = NodeApi.GetProperty(NodeApi.GetGlobal(), "abilityContext"u8);
        if (context == IntPtr.Zero)
            throw new PermissionException("host did not export globalThis.abilityContext");

        var manager = AbilityAccessCtrl.CreateAtManager();
        if (_permissions.All(permission =>
                manager.GetSelfPermissionStatus(permission) == HarmonyOS.ArkUI.PermissionStatus.Granted))
            return;

        await manager.RequestPermissionsFromUserAsync(context, _permissions).ConfigureAwait(true);

        var missing = _permissions
            .Where(permission => manager.GetSelfPermissionStatus(permission) != HarmonyOS.ArkUI.PermissionStatus.Granted)
            .ToArray();

        if (missing.Length > 0)
            throw new PermissionException($"Permission was not granted: {string.Join(", ", missing)}");
    }
}
