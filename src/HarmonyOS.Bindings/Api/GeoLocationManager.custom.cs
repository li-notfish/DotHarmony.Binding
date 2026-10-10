#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyOS.Interop;

namespace HarmonyOS.Bindings.Api;

public static unsafe partial class GeoLocationManager
{
    internal static Task<IntPtr> GetCurrentLocationAsync(
        CurrentLocationRequest request,
        CancellationToken cancellationToken)
        => NodeApi.CallMethodAsync<IntPtr>(
            Module,
            _getCurrentLocation,
            cancellationToken,
            NapiArg.Of(request));
}
