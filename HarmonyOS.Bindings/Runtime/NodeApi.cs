using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// ArkUI 组件操作的高层 API（napi 路径）
/// 提供类型安全的方法，内部处理类型转换和多值属性分发。
/// 注意：UI 组件的主路径是 NativeNode（ArkUI C API）；本类服务非 UI 的 ArkTS 对象操作。
/// </summary>
public static class NodeApi
{
    /// <summary>
    /// 宿主 ArkTS 侧注入到 globalThis 的胶水模块名，其上须提供
    /// createComponent(componentName, ...args) 分发函数（由宿主工程提供）。
    /// </summary>
    internal const string GlueGlobalName = "ArkUI";

    /// <summary>
    /// 获取宿主注入的 ArkUI 胶水模块
    /// </summary>
    private static NativeNodeApi.napi_value GetGlueModule(NativeNodeApi.napi_env env)
    {
        NativeNodeApi.napi_get_global(env, out var global).ThrowIfFailed();
        var globalName = Encoding.UTF8.GetBytes(GlueGlobalName);
        NativeNodeApi.napi_get_named_property(env, global, globalName, out var arkuiModule).ThrowIfFailed();
        NativeNodeApi.napi_typeof(env, arkuiModule, out var valueType).ThrowIfFailed();
        if (valueType != NativeNodeApi.napi_valuetype.napi_object &&
            valueType != NativeNodeApi.napi_valuetype.napi_function)
        {
            throw new InvalidOperationException(
                $"globalThis.{GlueGlobalName} is not defined. The host ArkTS page must inject the glue module before creating components.");
        }
        return arkuiModule;
    }

    /// <summary>
    /// 创建无参组件
    /// </summary>
    /// <param name="componentName">组件名称（如 "Text", "Button"）</param>
    /// <returns>组件的 napi_value 句柄</returns>
    public static IntPtr CreateComponent(string componentName)
    {
#if HARMONYOS
        var env = NapiEnv.Current;
        var arkuiModule = GetGlueModule(env);

        // 调用创建组件的函数
        var createFuncName = Encoding.UTF8.GetBytes("createComponent");
        NativeNodeApi.napi_get_named_property(env, arkuiModule, createFuncName, out var createFunc).ThrowIfFailed();

        var componentNameValue = NativeValue.From(componentName);
        var argv = new IntPtr[] { componentNameValue };
        NativeNodeApi.napi_call_function(env, arkuiModule, createFunc, 1, argv, out var result).ThrowIfFailed();

        return result;
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>
    /// 创建带参组件
    /// </summary>
    /// <param name="componentName">组件名称</param>
    /// <param name="args">构造参数</param>
    /// <returns>组件的 napi_value 句柄</returns>
    public static IntPtr CreateComponent(string componentName, params object[] args)
    {
#if HARMONYOS
        var env = NapiEnv.Current;
        var arkuiModule = GetGlueModule(env);

        // 调用创建组件的函数
        var createFuncName = Encoding.UTF8.GetBytes("createComponent");
        NativeNodeApi.napi_get_named_property(env, arkuiModule, createFuncName, out var createFunc).ThrowIfFailed();

        // 构建参数数组：[componentName, arg1, arg2, ...]
        var argv = new IntPtr[args.Length + 1];
        argv[0] = NativeValue.From(componentName);
        for (int i = 0; i < args.Length; i++)
        {
            argv[i + 1] = NativeValue.From(args[i]);
        }

        NativeNodeApi.napi_call_function(env, arkuiModule, createFunc, argv.Length, argv, out var result).ThrowIfFailed();

        return result;
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>
    /// 设置组件属性（支持链式调用）
    /// </summary>
    /// <param name="jsObject">组件句柄</param>
    /// <param name="attributeName">属性名称</param>
    /// <param name="values">属性值（支持单值和多值）</param>
    public static void SetAttribute(IntPtr jsObject, string attributeName, params object[] values)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));

        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(attributeName);

        if (values.Length == 1)
        {
            // 单值：直接设置属性
            var napiValue = NativeValue.From(values[0]);
            NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, napiValue).ThrowIfFailed();
        }
        else if (values.Length > 1)
        {
            // 多值：创建数组
            NativeNodeApi.napi_create_array_with_length(env, values.Length, out var array).ThrowIfFailed();
            for (int i = 0; i < values.Length; i++)
            {
                var elem = NativeValue.From(values[i]);
                NativeNodeApi.napi_set_element(env, array, (uint)i, elem).ThrowIfFailed();
            }
            NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, array).ThrowIfFailed();
        }
#endif
    }

    /// <summary>
    /// 设置组件属性（预编码属性名版本，避免重复 UTF8 编码）
    /// </summary>
    public static void SetAttribute(IntPtr jsObject, byte[] attributeName, params object[] values)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));

        var env = NapiEnv.Current;

        if (values.Length == 1)
        {
            var napiValue = NativeValue.From(values[0]);
            NativeNodeApi.napi_set_named_property(env, jsObject, attributeName, napiValue).ThrowIfFailed();
        }
        else if (values.Length > 1)
        {
            NativeNodeApi.napi_create_array_with_length(env, values.Length, out var array).ThrowIfFailed();
            for (int i = 0; i < values.Length; i++)
            {
                var elem = NativeValue.From(values[i]);
                NativeNodeApi.napi_set_element(env, array, (uint)i, elem).ThrowIfFailed();
            }
            NativeNodeApi.napi_set_named_property(env, jsObject, attributeName, array).ThrowIfFailed();
        }
#endif
    }

    /// <summary>
    /// 注册事件处理器（AOT 兼容版本）
    /// </summary>
    /// <param name="jsObject">组件句柄</param>
    /// <param name="eventName">事件名称</param>
    /// <param name="handler">事件处理委托</param>
    /// <param name="trampolinePtr">预编译的跳板函数指针（[UnmanagedCallersOnly]）</param>
    /// <returns>为固定委托而分配的 GCHandle；组件 Dispose 时必须经 <see cref="FreeEventHandle"/> 释放</returns>
    public static GCHandle SetEventHandler(IntPtr jsObject, string eventName, Delegate handler, IntPtr trampolinePtr)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
        if (trampolinePtr == IntPtr.Zero)
            throw new ArgumentException("Trampoline pointer must not be zero", nameof(trampolinePtr));

        var env = NapiEnv.Current;

        // 固定委托，防止 GC 回收；句柄由调用方在组件生命周期结束时释放
        var gch = GCHandle.Alloc(handler);

        // 使用预编译的跳板函数指针（AOT 兼容）
        var nameBytes = Encoding.UTF8.GetBytes(eventName);
        NativeNodeApi.napi_create_function(
            env, nameBytes, (IntPtr)nameBytes.Length,
            trampolinePtr, GCHandle.ToIntPtr(gch), out var jsFunc).ThrowIfFailed();

        // 设置为属性
        NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, jsFunc).ThrowIfFailed();

        return gch;
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>
    /// 释放事件委托的 GCHandle（组件 Dispose 时调用）
    /// </summary>
    public static void FreeEventHandle(GCHandle gch)
    {
        if (gch.IsAllocated)
            gch.Free();
    }

    /// <summary>
    /// 调用组件方法（非链式）
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="jsObject">组件句柄</param>
    /// <param name="methodName">方法名称</param>
    /// <param name="args">方法参数</param>
    /// <returns>方法返回值</returns>
    public static T CallMethod<T>(IntPtr jsObject, string methodName, params object[] args)
#if HARMONYOS
        => CallMethod<T>(jsObject, Encoding.UTF8.GetBytes(methodName), args);
#else
    {
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
    }
#endif

    /// <summary>
    /// 调用组件方法（byte[] 方法名重载：生成器以 "name"u8 常量携带，免每次 UTF8 编码）
    /// </summary>
    public static T CallMethod<T>(IntPtr jsObject, byte[] methodName, params object[] args)
    {
#if HARMONYOS
        var result = InvokeMethod(jsObject, methodName, args);
        return ConvertResult<T>(result);
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>
    /// 调用组件方法（无返回值/void 返回：C# 泛型不支持 CallMethod&lt;void&gt;，void 调用走此重载）
    /// </summary>
    public static void CallMethodVoid(IntPtr jsObject, string methodName, params object[] args)
#if HARMONYOS
        => CallMethodVoid(jsObject, Encoding.UTF8.GetBytes(methodName), args);
#else
    {
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
    }
#endif

    /// <summary>
    /// 调用组件方法并接为 Task&lt;T&gt;（Promise&lt;T&gt; 路线；ROADMAP 2.1 最小切片）。
    /// 返回值非 Promise 时同步转换；Promise 经 PromiseTaskBridge（JS 线程回调 + TCS）。
    /// </summary>
    public static Task<T> CallMethodAsync<T>(IntPtr jsObject, string methodName, params object[] args)
#if HARMONYOS
        => CallMethodAsync<T>(jsObject, Encoding.UTF8.GetBytes(methodName), args);
#else
    {
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
    }
#endif

    /// <summary>调用组件方法并接为 Task&lt;T&gt;（byte[] 方法名重载，供生成器 u8 常量使用）</summary>
    public static Task<T> CallMethodAsync<T>(IntPtr jsObject, byte[] methodName, params object[] args)
    {
#if HARMONYOS
        var result = InvokeMethod(jsObject, methodName, args);
        NativeNodeApi.napi_is_promise(NapiEnv.Current, result, out var isPromise).ThrowIfFailed();
        if (!isPromise)
            return Task.FromResult(ConvertResult<T>(result));
        return PromiseTaskBridge.ToTask<T>(result);
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用组件方法（byte[] 方法名，void 返回）</summary>
    public static void CallMethodVoid(IntPtr jsObject, byte[] methodName, params object[] args)
    {
#if HARMONYOS
        _ = InvokeMethod(jsObject, methodName, args);
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

#if HARMONYOS
    private static IntPtr InvokeMethod(IntPtr jsObject, byte[] methodName, object[] args)
    {
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));

        var env = NapiEnv.Current;

        // 获取方法函数
        NativeNodeApi.napi_get_named_property(env, jsObject, methodName, out var jsFunc).ThrowIfFailed();

        // 构建参数数组
        var argv = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argv[i] = NativeValue.From(args[i]);
        }

        // 调用方法
        NativeNodeApi.napi_call_function(env, jsObject, jsFunc, argv.Length, argv, out var result).ThrowIfFailed();
        return result;
    }
#endif

    /// <summary>
    /// 销毁组件（napi 路径：对象生命周期由 ArkTS GC 管理，无显式销毁语义）
    /// </summary>
    /// <param name="jsObject">组件句柄</param>
    public static void DestroyComponent(IntPtr jsObject)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero) return;
        // napi_value 会被 GC 回收，不需要显式销毁
#endif
    }

#if HARMONYOS
    private static T ConvertResult<T>(IntPtr result)
    {
        var type = typeof(T);
        if (type == typeof(bool))
            return (T)(object)NativeValue.ToBool(result);
        if (type == typeof(double))
            return (T)(object)NativeValue.ToDouble(result);
        if (type == typeof(int))
            return (T)(object)NativeValue.ToInt(result);
        if (type == typeof(string))
            return (T)(object)NativeValue.ToString(result)!;
        if (type == typeof(IntPtr))
            return (T)(object)result;
        throw new NotSupportedException($"Unsupported return type: {type.Name}");
    }
#endif
}
