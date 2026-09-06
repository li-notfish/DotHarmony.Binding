// M0 通道验证绑定：@ohos.deviceInfo（纯只读 string 常量集）
// 验证目标：C# 经 napi_load_module 加载系统模块 → 属性读取 → 字符串回传
// 通道验证通过后，同类绑定将由 src/parser/codeGenerator.ts（napi 服务路线）批量生成
using System;
using System.Runtime.InteropServices;
using System.Text;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.Bindings.Api;

/// <summary>
/// @ohos.deviceInfo 绑定（设备信息查询，@since 6）。
/// 官方 .d.ts 形状：declare namespace deviceInfo { const xxx: string }
/// </summary>
public static unsafe partial class DeviceInfo
{
    private const string ModuleName = "@ohos.deviceInfo";

    private static NapiReference? _moduleRef;
    private static bool _loadAttempted;

    private static IntPtr Module
    {
        get
        {
            if (_moduleRef != null) return _moduleRef.Value;
            if (_loadAttempted)
                throw new InvalidOperationException("@ohos.deviceInfo module failed to load (previous attempt)");
            _loadAttempted = true;

            var env = NapiEnv.Current;

            // 模块 napi_value 只在 handle scope 内有效，加载后立即转为 napi_ref 长期持有
            NativeNodeApi.napi_open_handle_scope(env, out var scope).ThrowIfFailed();
            try
            {
                // 系统模块加载格式：优先 "=@ohos.xxx"（系统预置模块前缀），回退不带前缀。
                // 注意：模块不存在时 napi_load_module 会挂起 ArkTS 异常，
                // 每次失败后必须 napi_get_and_clear_last_exception，否则异常传回宿主执行流导致闪退。
                foreach (var name in new[] { "=" + ModuleName, ModuleName })
                {
                    var utf8 = Encoding.UTF8.GetBytes(name);
                    fixed (byte* p = utf8)
                    {
                        var status = NativeNodeApi.napi_load_module(env, p, out var module);
                        if (status == NativeNodeApi.napi_status.napi_ok && module != IntPtr.Zero)
                        {
                            _moduleRef = new NapiReference(module);
                            break;
                        }
                        NativeNodeApi.napi_get_and_clear_last_exception(env, out _);
                    }
                }
            }
            finally
            {
                NativeNodeApi.napi_close_handle_scope(env, scope).ThrowIfFailed();
            }

            if (_moduleRef == null)
                throw new InvalidOperationException(
                    $"failed to load {ModuleName} via napi_load_module (tried with and without '=' prefix)");

            Runtime.HiLog.Info("HarmonyHost", $"[deviceInfo] module loaded via napi_load_module");
            return _moduleRef.Value;
        }
    }

    private static string GetString(string propertyName)
    {
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(propertyName);
        NativeNodeApi.napi_get_named_property(env, Module, nameBytes, out var value).ThrowIfFailed();
        var result = NativeValue.ToString(value);
        return result ?? string.Empty;
    }

    /// <summary>设备类型（phone/tablet/2in1...）</summary>
    public static string DeviceType => GetString("deviceType");

    /// <summary>厂商</summary>
    public static string Manufacture => GetString("manufacture");

    /// <summary>品牌</summary>
    public static string Brand => GetString("brand");

    /// <summary>上市版本号（对外展示型号）</summary>
    public static string MarketName => GetString("marketName");

    /// <summary>产品型号</summary>
    public static string ProductModel => GetString("productModel");

    /// <summary>系统全名（HarmonyOS x.y.z）</summary>
    public static string OsFullName => GetString("osFullName");
}
