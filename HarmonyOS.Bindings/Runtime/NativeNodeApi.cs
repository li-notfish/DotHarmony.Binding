#if HARMONYOS
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// HarmonyOS N-API P/Invoke 声明
/// 签名来源：标准 Node-API 官方文档
/// </summary>
internal static partial class NativeNodeApi
{
    // Node-API 实现库：系统里不存在 libnapi.so，真实名字是 libace_napi.z.so
    private const string NApiLib = "libace_napi.z.so";

    #region 字符串操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_string_utf8(
        napi_env env,
        ReadOnlySpan<byte> str,
        IntPtr length,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_string_utf8(
        napi_env env,
        napi_value value,
        byte[]? buf,
        IntPtr bufsize,
        out IntPtr result);

    #endregion

    #region 数值操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_double(
        napi_env env,
        double value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_double(
        napi_env env,
        napi_value value,
        out double result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_int32(
        napi_env env,
        int value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_int32(
        napi_env env,
        napi_value value,
        out int result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_uint32(
        napi_env env,
        uint value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_uint32(
        napi_env env,
        napi_value value,
        out uint result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_int64(
        napi_env env,
        long value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_int64(
        napi_env env,
        napi_value value,
        out long result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_uint64(
        napi_env env,
        ulong value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_uint64(
        napi_env env,
        napi_value value,
        out ulong result);

    #endregion

    #region 布尔操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_boolean(
        napi_env env,
        [MarshalAs(UnmanagedType.U1)] bool value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_bool(
        napi_env env,
        napi_value value,
        [MarshalAs(UnmanagedType.U1)] out bool result);

    #endregion

    #region 对象操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_object(
        napi_env env,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_global(
        napi_env env,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_named_property(
        napi_env env,
        napi_value obj,
        byte[] name,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_set_named_property(
        napi_env env,
        napi_value obj,
        byte[] name,
        napi_value value);

    /// <summary>Span 重载：生成的 u8 常量字段零拷贝传递（LibraryImport 固定钉扎）</summary>
    [LibraryImport(NApiLib, EntryPoint = "napi_set_named_property")]
    internal static unsafe partial napi_status napi_set_named_property(
        napi_env env,
        napi_value obj,
        ReadOnlySpan<byte> name,
        napi_value value);

    #endregion

    #region 函数操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_function(
        napi_env env,
        byte[] name,
        IntPtr length,
        IntPtr cb,
        IntPtr data,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_is_promise(
        napi_env env,
        napi_value value,
        [MarshalAs(UnmanagedType.U1)] out bool isPromise);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_call_function(
        napi_env env,
        napi_value recv,
        napi_value func,
        int argc,
        ReadOnlySpan<IntPtr> argv,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_new_instance(
        napi_env env,
        napi_value constructor,
        int argc,
        ReadOnlySpan<IntPtr> argv,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_cb_info(
        napi_env env,
        napi_callback_info info,
        ref IntPtr argc,
        Span<IntPtr> argv,
        out IntPtr thisArg,
        out IntPtr data);

    #endregion

    #region 引用操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_reference(
        napi_env env,
        napi_value value,
        uint initial_refcount,
        out napi_ref result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_delete_reference(
        napi_env env,
        napi_ref ref_value);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_reference_value(
        napi_env env,
        napi_ref ref_value,
        out napi_value result);

    #endregion

    #region 数组操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_array_with_length(
        napi_env env,
        int length,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_array_length(
        napi_env env,
        napi_value value,
        out uint result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_element(
        napi_env env,
        napi_value @object,
        uint index,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_set_element(
        napi_env env,
        napi_value @object,
        uint index,
        napi_value value);

    #endregion

    #region ArrayBuffer / TypedArray

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_is_arraybuffer(
        napi_env env,
        napi_value value,
        [MarshalAs(UnmanagedType.U1)] out bool result);

    [LibraryImport(NApiLib)]
    internal static unsafe partial napi_status napi_get_arraybuffer_info(
        napi_env env,
        napi_value arraybuffer,
        byte** data,
        out IntPtr byte_length);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_arraybuffer(
        napi_env env,
        IntPtr byte_length,
        out IntPtr data,
        out napi_value result);
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_is_typedarray(
        napi_env env,
        napi_value value,
        [MarshalAs(UnmanagedType.U1)] out bool result);

    /// <summary>napi_typedarray_type：Int8=0, Uint8=1, Uint8Clamped=2, Int16=3, Uint16=4, Int32=5, Uint32=6, Float32=7, Float64=8, BigInt64=9, Uint64=10</summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_typedarray_info(
        napi_env env,
        napi_value typedarray,
        out int type,
        out IntPtr length,
        out IntPtr data,
        out napi_value arraybuffer,
        out IntPtr byte_offset);

    #endregion

    #region BigInt

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_bigint_int64(
        napi_env env,
        napi_value value,
        out long result,
        [MarshalAs(UnmanagedType.U1)] out bool lossless);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_bigint_uint64(
        napi_env env,
        napi_value value,
        out ulong result,
        [MarshalAs(UnmanagedType.U1)] out bool lossless);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_bigint_int64(
        napi_env env,
        long value,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_bigint_uint64(
        napi_env env,
        ulong value,
        out napi_value result);

    #endregion

    #region 生命周期

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_undefined(
        napi_env env,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_null(
        napi_env env,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_typeof(
        napi_env env,
        napi_value value,
        out napi_valuetype result);

    [LibraryImport(NApiLib)]
    internal static unsafe partial napi_status napi_open_handle_scope(napi_env env, out IntPtr scope);

    [LibraryImport(NApiLib)]
    internal static unsafe partial napi_status napi_close_handle_scope(napi_env env, IntPtr scope);

    [LibraryImport(NApiLib)]
    internal static unsafe partial napi_status napi_load_module(napi_env env, byte* path, out napi_value result);

    /// <summary>
    /// 清除挂起的 ArkTS 异常。可能抛出 ArkTS 异常的 napi 调用（如 napi_load_module）
    /// 失败后必须调用，否则异常会在控制流返回宿主时 rethrow 导致应用崩溃。
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_and_clear_last_exception(napi_env env, out napi_value result);

    #endregion

    #region Thread-Safe Function (TSFN)

    /// <summary>
    /// 回调类型：JS 线程调用 C# 的入口
    /// void callback(napi_env env, napi_value js_callback, void* context, void* data)
    /// </summary>
    internal delegate void NapiThreadSafeFunctionCallJs(
        napi_env env,
        napi_value js_callback,
        IntPtr context,
        IntPtr data);

    /// <summary>
    /// 创建线程安全函数，允许任意线程安全地调用 JS 函数
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_threadsafe_function(
        napi_env env,
        napi_value func,
        napi_value async_resource,
        napi_value async_resource_name,
        IntPtr max_queue_size,
        IntPtr initial_thread_count,
        IntPtr thread_finalize_data,
        IntPtr thread_finalize_callback,
        IntPtr context,
        IntPtr call_js,
        out napi_threadsafe_function result);

    /// <summary>
    /// 释放线程安全函数。调用后不得再使用该句柄。
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_release_threadsafe_function(
        napi_threadsafe_function tsfn,
        napi_threadsafe_function_release_mode mode);

    /// <summary>
    /// 从任意线程调用线程安全函数
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_call_threadsafe_function(
        napi_threadsafe_function tsfn,
        IntPtr data,
        napi_threadsafe_function_call_mode is_blocking);

    /// <summary>
    /// 增加线程安全函数的引用计数
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_acquire_threadsafe_function(
        napi_threadsafe_function tsfn);

    /// <summary>
    /// 获取线程安全函数所在线程的 ID
    /// </summary>
    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_threadsafe_function_thread_id(
        napi_threadsafe_function tsfn,
        out ulong result);

    #endregion

    #region 状态检查

    /// <summary>
    /// napi 调用失败时抛出异常。任何 napi_* 返回值都不允许静默忽略。
    /// </summary>
    internal static void ThrowIfFailed(this napi_status status, [System.Runtime.CompilerServices.CallerMemberName] string op = "")
    {
        if (status != napi_status.napi_ok)
            throw new NapiException(status, op);
    }

    #endregion

    #region 类型定义

    /// <summary>
    /// Node-API 调用失败异常，携带 napi_status
    /// </summary>
    internal sealed class NapiException(napi_status status, string op)
        : Exception($"NAPI call '{op}' failed with status {status} ({(int)status})")
    {
        public napi_status Status { get; } = status;
        public string Operation { get; } = op;
    }

    internal readonly struct napi_env : IEquatable<napi_env>
    {
        private readonly IntPtr _handle;
        public napi_env(IntPtr handle) => _handle = handle;
        public static implicit operator IntPtr(napi_env env) => env._handle;
        public static implicit operator napi_env(IntPtr handle) => new(handle);
        public bool Equals(napi_env other) => _handle == other._handle;
        public override bool Equals(object? obj) => obj is napi_env other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public override string ToString() => _handle.ToString("X");
    }

    internal readonly struct napi_value : IEquatable<napi_value>
    {
        private readonly IntPtr _handle;
        public napi_value(IntPtr handle) => _handle = handle;
        public static implicit operator IntPtr(napi_value value) => value._handle;
        public static implicit operator napi_value(IntPtr handle) => new(handle);
        public bool Equals(napi_value other) => _handle == other._handle;
        public override bool Equals(object? obj) => obj is napi_value other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public override string ToString() => _handle.ToString("X");
    }

    internal readonly struct napi_ref : IEquatable<napi_ref>
    {
        private readonly IntPtr _handle;
        public napi_ref(IntPtr handle) => _handle = handle;
        public static implicit operator IntPtr(napi_ref ref_value) => ref_value._handle;
        public static implicit operator napi_ref(IntPtr handle) => new(handle);
        public bool Equals(napi_ref other) => _handle == other._handle;
        public override bool Equals(object? obj) => obj is napi_ref other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public override string ToString() => _handle.ToString("X");
    }

    internal readonly struct napi_callback_info : IEquatable<napi_callback_info>
    {
        private readonly IntPtr _handle;
        public napi_callback_info(IntPtr handle) => _handle = handle;
        public static implicit operator IntPtr(napi_callback_info info) => info._handle;
        public static implicit operator napi_callback_info(IntPtr handle) => new(handle);
        public bool Equals(napi_callback_info other) => _handle == other._handle;
        public override bool Equals(object? obj) => obj is napi_callback_info other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public override string ToString() => _handle.ToString("X");
    }

    internal readonly struct napi_threadsafe_function : IEquatable<napi_threadsafe_function>
    {
        private readonly IntPtr _handle;
        public napi_threadsafe_function(IntPtr handle) => _handle = handle;
        public static implicit operator IntPtr(napi_threadsafe_function tsfn) => tsfn._handle;
        public static implicit operator napi_threadsafe_function(IntPtr handle) => new(handle);
        public bool Equals(napi_threadsafe_function other) => _handle == other._handle;
        public override bool Equals(object? obj) => obj is napi_threadsafe_function other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public override string ToString() => _handle.ToString("X");
    }

    internal enum napi_threadsafe_function_release_mode
    {
        napi_tsfn_release = 0,
        napi_tsfn_abort = 1
    }

    internal enum napi_threadsafe_function_call_mode
    {
        napi_tsfn_nonblocking = 0,
        napi_tsfn_blocking = 1
    }

    internal enum napi_status
    {
        napi_ok = 0,
        napi_invalid_arg = 1,
        napi_object_expected = 2,
        napi_string_expected = 3,
        napi_name_expected = 4,
        napi_function_expected = 5,
        napi_number_expected = 6,
        napi_boolean_expected = 7,
        napi_array_expected = 8,
        napi_generic_failure = 9,
        napi_pending_exception = 10,
        napi_cancelled = 11,
        napi_escape_called_twice = 12,
        napi_handle_scope_mismatch = 13,
        napi_callback_scope_mismatch = 14,
        napi_queue_full = 15,
        napi_closing = 16,
        napi_bigint_expected = 17,
        napi_date_expected = 18,
        napi_arraybuffer_expected = 19,
        napi_detachable_arraybuffer_expected = 20,
        napi_would_deadlock = 21
    }

    internal enum napi_valuetype
    {
        napi_undefined = 0,
        napi_null = 1,
        napi_boolean = 2,
        napi_number = 3,
        napi_string = 4,
        napi_symbol = 5,
        napi_object = 6,
        napi_function = 7,
        napi_external = 8,
        napi_bigint = 9
    }

    #endregion
}
#endif
