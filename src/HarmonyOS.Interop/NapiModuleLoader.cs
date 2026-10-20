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
                    NativeNodeApi.napi_open_handle_scope(env, out var scope).ThrowIfFailed();
                    try
                    {
                        // 先试 "=<module>"（Ability 上下文）再试裸模块名（普通上下文）
                        foreach (var name in new[] { "=" + module, module })
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
                                    NativeNodeApi.napi_get_and_clear_last_exception(env, out _);
                                }
                            }
                        }
                    }
                    finally
                    {
                        NativeNodeApi.napi_close_handle_scope(env, scope).ThrowIfFailed();
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
