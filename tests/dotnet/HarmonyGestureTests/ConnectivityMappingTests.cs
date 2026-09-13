// HarmonyConnectivity 纯逻辑单测：OHOS bearer → MAUI ConnectionProfile 映射（不依赖真机）。
// NetBearType 底值锁死（Api 9 起稳定值）；NET_CAPABILITY 底值参与 CurrentAccess 判定。
using HarmonyOS.ArkUI;
using HarmonyOS.Maui.Essentials;
using Microsoft.Maui.Networking;
using Xunit;

namespace HarmonyGestureTests;

public class ConnectivityMappingTests
{
    [Theory]
    [InlineData(NetBearType.BearerWifi, ConnectionProfile.WiFi)]
    [InlineData(NetBearType.BearerCellular, ConnectionProfile.Cellular)]
    [InlineData(NetBearType.BearerEthernet, ConnectionProfile.Ethernet)]
    [InlineData(NetBearType.BearerBluetooth, ConnectionProfile.Bluetooth)]
    [InlineData(NetBearType.BearerVpn, ConnectionProfile.Unknown)]
    public void Bearer_MapsToProfile(NetBearType bearer, ConnectionProfile expected)
        => Assert.Equal(expected, HarmonyConnectivity.MapBearer(bearer));

    // ── 枚举底值锁死：映射与 CurrentAccess 判定依赖这些稳定值 ──

    [Fact]
    public void NetBearType_Values_AreStable()
    {
        Assert.Equal(0, (int)NetBearType.BearerCellular);
        Assert.Equal(1, (int)NetBearType.BearerWifi);
        Assert.Equal(2, (int)NetBearType.BearerBluetooth);
        Assert.Equal(3, (int)NetBearType.BearerEthernet);
        Assert.Equal(4, (int)NetBearType.BearerVpn);
    }

    [Fact]
    public void NetCap_Values_AreStable()
    {
        // CurrentAccess 判定：INTERNET=12 / VALIDATED=16 → NetworkAccess.Internet
        Assert.Equal(12, (int)NetCap.NetCapabilityInternet);
        Assert.Equal(16, (int)NetCap.NetCapabilityValidated);
    }
}
