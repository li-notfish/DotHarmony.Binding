#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyOS.Essentials;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Xunit;

namespace HarmonyGestureTests;

public class EssentialsContractTests
{
    [Theory]
    [InlineData("phone", "Phone")]
    [InlineData("tablet", "Tablet")]
    [InlineData("tv", "TV")]
    [InlineData("wearable", "Watch")]
    [InlineData("desktop", "Desktop")]
    [InlineData("unknown", "Unknown")]
    public void DeviceInfo_Maps_Idiom_Unknown(string deviceType, string expected)
        => Assert.Equal(
            DeviceIdiom.Create(expected),
            HarmonyDeviceInfo.MapIdiom(deviceType));

    [Fact]
    public async Task Geolocation_GetLocationAsync_NullRequest_Throws()
    {
        var geolocation = new HarmonyGeolocation(new FakePermissionGate());
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => geolocation.GetLocationAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Geolocation_StartListening_NullRequest_Throws()
    {
        var geolocation = new HarmonyGeolocation(new FakePermissionGate());
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => geolocation.StartListeningForegroundAsync(null!));
    }

    [Fact]
    public async Task Geolocation_Propagates_PermissionException()
    {
        var gate = new FakePermissionGate();
        var geolocation = new HarmonyGeolocation(gate);

        await Assert.ThrowsAsync<PermissionException>(
            geolocation.GetLastKnownLocationAsync);

        Assert.Equal(1, gate.CallCount);
    }

    private sealed class FakePermissionGate : IHarmonyPermissionGate
    {
        public int CallCount { get; private set; }

        public Task EnsureGrantedAsync(params string[] permissions)
        {
            CallCount++;
            throw new PermissionException(
                $"Permission was not granted: {string.Join(", ", permissions)}");
        }
    }
}
