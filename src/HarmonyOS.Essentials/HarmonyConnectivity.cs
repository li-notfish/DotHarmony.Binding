// IConnectivity 鸿蒙实现：@ohos.net.connection（2026-09-13 从灰度转正）。
// NetworkAccess 经 HasDefaultNetSync + GetNetCapabilitiesSync（NET_CAPABILITY_INTERNET=12 /
// VALIDATED=16 判定可上网）；ConnectionProfiles 从全部网络的 bearerTypes 映射（并集去重）。
// 变化事件经 createNetConnection 默认连接监听 + register/on(netAvailable|netLost|netConnectionChange)，
// 回调内重读属性 + 去重（连接属性载荷不解析——属性即真相）。
#nullable enable
using Microsoft.Maui.Networking;
using HarmonyOS.Interop;
using HNetConn = HarmonyOS.Bindings.Api.Net.Connection;
using HNetConnectionObject = HarmonyOS.Bindings.Api.Net.NetConnection;
using HNetHandle = HarmonyOS.Bindings.Api.Net.NetHandle;
using HNetCapabilities = HarmonyOS.Bindings.Api.Net.NetCapabilities;
using HNetCap = HarmonyOS.ArkUI.NetCap;
using HNetBear = HarmonyOS.ArkUI.NetBearType;

namespace HarmonyOS.Essentials;

public class HarmonyConnectivity : IConnectivity
{
    public NetworkAccess NetworkAccess
    {
        get
        {
            if (!HNetConn.HasDefaultNetSync())
                return NetworkAccess.None;
            var caps = ReadDefaultCaps();
            // INTERNET（12）标记网络可上网，VALIDATED（16）为已确认——任一即 Internet
            if (caps?.NetworkCap is { } networkCaps &&
                (Array.IndexOf(networkCaps, HNetCap.NetCapabilityInternet) >= 0 ||
                 Array.IndexOf(networkCaps, HNetCap.NetCapabilityValidated) >= 0))
                return NetworkAccess.Internet;
            return NetworkAccess.Local;
        }
    }

    public IEnumerable<ConnectionProfile> ConnectionProfiles
    {
        get
        {
            var profiles = new List<ConnectionProfile>();
            try
            {
                foreach (var net in HNetConn.GetAllNetsSync())
                {
                    foreach (var bearer in HNetConn.GetNetCapabilitiesSync(net).BearerTypes)
                        profiles.Add(MapBearer(bearer));
                }
            }
            catch (Exception ex)
            {
                HiLog.Warn("Essentials", $"[connectivity] profiles failed: {ex.Message}");
            }
            return profiles.Distinct();
        }
    }

    event EventHandler<ConnectivityChangedEventArgs>? _changed;
    HNetConnectionObject? _watch;
    readonly object _eventGate = new();

    // 去重缓存：netAvailable/netLost/netConnectionChange 回调内重读属性，仅变化时才触发
    NetworkAccess _lastAccess;
    List<ConnectionProfile> _lastProfiles = new();

    public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged
    {
        add
        {
            lock (_eventGate)
            {
                bool first = _changed is null;
                _changed += value;
                if (first)
                    StartWatcher();
            }
        }
        remove
        {
            lock (_eventGate)
            {
                _changed -= value;
                if (_changed is null)
                    StopWatcher();
            }
        }
    }

    void StartWatcher()
    {
        try
        {
            // 无 specifier 的默认连接监听：事件访问器保证 on/off 使用同一 registry key。
            var watch = HNetConn.CreateNetConnection();
            watch.NetAvailable += OnNetAvailable;
            watch.NetLost += OnNetLost;
            watch.NetConnectionPropertiesChange += OnNetConnectionPropertiesChange;
            _ = ObserveWatcherTask(watch.RegisterAsync(), "register");
            _watch = watch;
            _lastAccess = NetworkAccess;
            _lastProfiles = new List<ConnectionProfile>(ConnectionProfiles);
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[connectivity] watch failed: {ex.Message}");
        }
    }

    void StopWatcher()
    {
        if (_watch is not null)
        {
            _watch.NetAvailable -= OnNetAvailable;
            _watch.NetLost -= OnNetLost;
            _watch.NetConnectionPropertiesChange -= OnNetConnectionPropertiesChange;
            _ = ObserveWatcherTask(_watch.UnregisterAsync(), "unregister");
            _watch = null;
        }
    }

    private void OnNetAvailable(HNetHandle _) => OnNetChanged();

    private void OnNetLost(HNetHandle _) => OnNetChanged();

    private void OnNetConnectionPropertiesChange(IntPtr _) => OnNetChanged();

    private static async Task ObserveWatcherTask(Task operation, string operationName)
    {
        try
        {
            await operation.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials",
                $"[connectivity] {operationName} failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    void OnNetChanged()
    {
        try
        {
            var access = NetworkAccess;
            var profiles = new List<ConnectionProfile>(ConnectionProfiles);
            if (access == _lastAccess && profiles.SequenceEqual(_lastProfiles))
                return;
            _lastAccess = access;
            _lastProfiles = profiles;
            _changed?.Invoke(this, new ConnectivityChangedEventArgs(access, profiles));
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[connectivity] changed handler failed: {ex.Message}");
        }
    }

    internal static ConnectionProfile MapBearer(HNetBear bearer) => bearer switch
    {
        HNetBear.BearerWifi => ConnectionProfile.WiFi,
        HNetBear.BearerCellular => ConnectionProfile.Cellular,
        HNetBear.BearerEthernet => ConnectionProfile.Ethernet,
        HNetBear.BearerBluetooth => ConnectionProfile.Bluetooth,
        _ => ConnectionProfile.Unknown,
    };

    private static HNetCapabilities? ReadDefaultCaps()
    {
        try
        {
            return HNetConn.GetNetCapabilitiesSync(HNetConn.GetDefaultNetSync());
        }
        catch (Exception ex)
        {
            HiLog.Warn("Essentials", $"[connectivity] caps failed: {ex.Message}");
            return null;
        }
    }
}
