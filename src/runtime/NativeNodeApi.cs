#if HARMONYOS
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.ArkUI;

/// <summary>
/// HarmonyOS N-API P/Invoke 声明
/// 签名来源：标准 Node-API 官方文档
/// </summary>
internal static partial class NativeNodeApi
{
    private const string NApiLib = "libnapi.so";

    #region 字符串操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_string_utf8(
        napi_env env,
        [MarshalAs(UnmanagedType.LPStr)] byte[] str,
        IntPtr length,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_value_string_utf8(
        napi_env env,
        napi_value value,
        [MarshalAs(UnmanagedType.LPStr)] byte[]? buf,
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
    internal static partial napi_status napi_get_named_property(
        napi_env env,
        napi_value obj,
        [MarshalAs(UnmanagedType.LPStr)] byte[] name,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_set_named_property(
        napi_env env,
        napi_value obj,
        [MarshalAs(UnmanagedType.LPStr)] byte[] name,
        napi_value value);

    #endregion

    #region 函数操作

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_create_function(
        napi_env env,
        [MarshalAs(UnmanagedType.LPStr)] byte[] name,
        IntPtr length,
        IntPtr cb,
        IntPtr data,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_call_function(
        napi_env env,
        napi_value recv,
        napi_value func,
        int argc,
        IntPtr[] argv,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_get_cb_info(
        napi_env env,
        napi_callback_info info,
        out IntPtr argc,
        out IntPtr argv,
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
        napi_value object,
        uint index,
        out napi_value result);

    [LibraryImport(NApiLib)]
    internal static partial napi_status napi_set_element(
        napi_env env,
        napi_value object,
        uint index,
        napi_value value);

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

    #endregion

    #region 类型定义

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
