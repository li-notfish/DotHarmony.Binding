#nullable enable
using System;
using System.Collections.Concurrent;

namespace HarmonyOS.Interop;

/// <summary>
/// @ohos.* 模块懒加载器：按模块名缓存一次 napi_load_module 结果，避免每个绑定文件
/// 重复生成同一段 handle-scope/加载/异常回滚代码（生成器只为每个模块产出一行调用）。
/// 模块对象生命周期与进程一致；加载失败会被记忆，后续调用直接抛出首次失败原因。
/// </summary>
internal static class NapiModuleLoader
{
    private static readonly ConcurrentDictionary<string, Entry> Entries = new(StringComparer.Ordinal);

    internal static IntPtr Load(string module) =>
        Entries.GetOrAdd(module, static _ => new Entry()).Get(module);

    private sealed class Entry
    {
        private NapiReference? _reference;
        private bool _attempted;
        private string? _failure;

        internal IntPtr Get(string module)
        {
            lock (this)
            {
                if (_reference is not null) return _reference.Value;
                if (_attempted)
                    throw new InvalidOperationException(
                        _failure ?? $"{module} module failed to load (previous attempt)");
                _attempted = true;

                try
                {
                    var env = NapiEnv.Current;
                    // 模块对象必须在本 scope 内转成 napi_ref 才能跨帧持有——这正是
                    // ROADMAP 平台铁律 #4 的形态；scope 关闭后裸句柄失效。
                    using (NapiHandleScope.Open(env))
                    {
                        // 探测顺序：先裸模块名，失败再试 "=<module>"（Ability 上下文）。
                        // 反过来的话，引擎每次都会先对 "=@ohos.xxx" 报
                        // "The module name doesn't comply with the naming rules"，
                        // 并在 hilog 打一整段 ReferenceError + backtrace——虽然随后回退成功，
                        // 但每个模块首载都会留一条看起来像崩溃的日志，掩盖真正的错误。
                        foreach (var name in new[] { module, "=" + module })
                        {
                            var utf8 = System.Text.Encoding.UTF8.GetBytes(name);
                            unsafe
                            {
                                fixed (byte* p = utf8)
                                {
                                    var status = NativeNodeApi.napi_load_module(env, p, out var value);
                                    if (status == napi_status.napi_ok && value != IntPtr.Zero)
                                    {
                                        _reference = new NapiReference(value);
                                        break;
                                    }
                                    // 探测失败：清掉挂起异常，避免污染后续调用的 last exception
                                    NativeNodeApi.napi_get_and_clear_last_exception(env, out _);
                                }
                            }
                        }
                    }

                    if (_reference is null)
                        throw new InvalidOperationException(
                            $"failed to load {module} via napi_load_module (tried with and without '=' prefix)");

                    HiLog.Info("HarmonyHost", $"[{module}] module loaded via napi_load_module");
                    return _reference.Value;
                }
                catch (Exception ex)
                {
                    _failure = $"{module} module failed to load: {ex.Message}";
                    throw;
                }
            }
        }
    }
}
