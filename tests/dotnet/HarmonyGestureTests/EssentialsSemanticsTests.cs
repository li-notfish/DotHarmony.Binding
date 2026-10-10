using HarmonyOS.Essentials;
using Xunit;

namespace HarmonyGestureTests;

public class EssentialsSemanticsTests
{
    [Theory]
    [InlineData("OpenHarmony-7.0.0.105", 26, "7.0.0.105")]
    [InlineData("OpenHarmony", 26, "26.0")]
    public void DeviceInfo_ParsesVersionString(string osFullName, int sdkApiVersion, string expected)
    {
        var version = HarmonyDeviceInfo.ParseOsVersion(osFullName, sdkApiVersion);
        Assert.Equal(expected, version.ToString());
    }

    [Fact]
    public void Accelerometer_ConvertsMetersPerSecondSquaredToGravityUnits()
    {
        Assert.Equal(1, HarmonyAccelerometer.ToGravityUnits(9.81), 12);
        Assert.Equal(0, HarmonyAccelerometer.ToGravityUnits(0), 12);
    }

    [Fact]
    public void AccelerometerShakeQueue_DetectsMajorityAccelerationWindow()
    {
        var queue = new AccelerometerShakeQueue();
        foreach (var timestamp in new[] { 0L, 50_000_000, 100_000_000, 150_000_000, 250_000_000 })
            queue.Add(timestamp, accelerating: true);

        Assert.True(queue.IsShaking);
        queue.Clear();
        Assert.False(queue.IsShaking);
    }

    [Fact]
    public void AccelerometerShakeQueue_IgnoresMixedSamples()
    {
        var queue = new AccelerometerShakeQueue();
        queue.Add(0, accelerating: false);
        queue.Add(100_000_000, accelerating: false);
        queue.Add(200_000_000, accelerating: true);
        queue.Add(250_000_000, accelerating: true);

        Assert.False(queue.IsShaking);
    }

    [Fact]
    public void AccelerometerShakeQueue_IgnoresExpiredSamples()
    {
        var queue = new AccelerometerShakeQueue();
        queue.Add(0, accelerating: true);
        queue.Add(100_000_000, accelerating: true);
        queue.Add(200_000_000, accelerating: true);
        queue.Add(600_000_000, accelerating: true);

        Assert.False(queue.IsShaking);
    }
}
