using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ArkUI 组件操作的高层 API
/// 提供类型安全的方法，内部处理类型转换和多值属性分发
/// </summary>
public static class NodeApi
{
    /// <summary>
    /// 创建无参组件
    /// </summary>
    /// <param name="componentName">组件名称（如 "Text", "Button"）</param>
    /// <returns>组件的 napi_value 句柄</returns>
    public static IntPtr CreateComponent(string componentName)
    {
#if HARMONYOS
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(componentName);

        // 获取全局的 ArkUI 模块
        var globalName = Encoding.UTF8.GetBytes("ArkUI");
        NativeNodeApi.napi_get_named_property(env, env, globalName, out var arkuiModule);

        // 调用创建组件的函数
        var createFuncName = Encoding.UTF8.GetBytes("createComponent");
        NativeNodeApi.napi_get_named_property(env, arkuiModule, createFuncName, out var createFunc);

        var componentNameValue = NativeValue.From(componentName);
        var argv = new IntPtr[] { componentNameValue };
        NativeNodeApi.napi_call_function(env, arkuiModule, createFunc, 1, argv, out var result);

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
        var nameBytes = Encoding.UTF8.GetBytes(componentName);

        // 获取全局的 ArkUI 模块
        var globalName = Encoding.UTF8.GetBytes("ArkUI");
        NativeNodeApi.napi_get_named_property(env, env, globalName, out var arkuiModule);

        // 调用创建组件的函数
        var createFuncName = Encoding.UTF8.GetBytes("createComponent");
        NativeNodeApi.napi_get_named_property(env, arkuiModule, createFuncName, out var createFunc);

        // 构建参数数组：[componentName, arg1, arg2, ...]
        var argv = new IntPtr[args.Length + 1];
        argv[0] = NativeValue.From(componentName);
        for (int i = 0; i < args.Length; i++)
        {
            argv[i + 1] = NativeValue.From(args[i]);
        }

        NativeNodeApi.napi_call_function(env, arkuiModule, createFunc, argv.Length, argv, out var result);

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
            // 适用于: scrollSnap(options), nestedScroll(options), divider(options)
            var napiValue = NativeValue.From(values[0]);
            NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, napiValue);
        }
        else if (values.Length > 1)
        {
            // 多值：创建数组
            // 适用于: edgeEffect(effect, options), cachedCount(count, show)
            NativeNodeApi.napi_create_array_with_length(env, values.Length, out var array);
            for (int i = 0; i < values.Length; i++)
            {
                var elem = NativeValue.From(values[i]);
                NativeNodeApi.napi_set_element(env, array, (uint)i, elem);
            }
            NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, array);
        }
#endif
    }

    /// <summary>
    /// 注册事件处理器
    /// </summary>
    /// <param name="jsObject">组件句柄</param>
    /// <param name="eventName">事件名称</param>
    /// <param name="handler">事件处理委托</param>
    public static void SetEventHandler(IntPtr jsObject, string eventName, Delegate handler)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var env = NapiEnv.Current;

        // 固定委托，防止 GC 回收
        var gch = GCHandle.Alloc(handler);

        // 获取委托的函数指针
        var funcPtr = Marshal.GetFunctionPointerForDelegate(handler);

        // 创建 JS 函数
        var nameBytes = Encoding.UTF8.GetBytes(eventName);
        NativeNodeApi.napi_create_function(
            env, nameBytes, (IntPtr)nameBytes.Length,
            funcPtr, GCHandle.ToIntPtr(gch), out var jsFunc);

        // 设置为属性
        NativeNodeApi.napi_set_named_property(env, jsObject, nameBytes, jsFunc);
#endif
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
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero)
            throw new ArgumentNullException(nameof(jsObject));

        var env = NapiEnv.Current;

        // 获取方法函数
        var nameBytes = Encoding.UTF8.GetBytes(methodName);
        NativeNodeApi.napi_get_named_property(env, jsObject, nameBytes, out var jsFunc);

        // 构建参数数组
        var argv = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argv[i] = NativeValue.From(args[i]);
        }

        // 调用方法
        NativeNodeApi.napi_call_function(env, jsObject, jsFunc, argv.Length, argv, out var result);

        // 转换返回值
        return ConvertResult<T>(result);
#else
        throw new PlatformNotSupportedException("NodeApi requires HarmonyOS runtime");
#endif
    }

    /// <summary>
    /// 销毁组件
    /// </summary>
    /// <param name="jsObject">组件句柄</param>
    public static void DestroyComponent(IntPtr jsObject)
    {
#if HARMONYOS
        if (jsObject == IntPtr.Zero) return;

        // napi_value 会被 GC 回收，不需要显式销毁
        // 这里可以添加清理逻辑，如释放关联的 GCHandle
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
