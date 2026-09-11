using System;
using System.Threading.Tasks;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// JS 对象实例包装基类。生成器为 @ohos.* 接口类型产出派生类（如 PasteData、ImageSource、DisplayObject），
/// 实例属性/方法经强引用句柄（napi_ref）访问。对象本体由 ArkTS GC 管理，
/// Dispose 仅释放 napi 强引用，不销毁 JS 对象。
/// </summary>
public abstract class JsObject : IDisposable
{
    private readonly NapiReference? _reference;
    private bool _disposed;

    /// <summary>包装一个 JS 对象，立即建立 napi 强引用，防止句柄失效后被 GC 回收。</summary>
    protected JsObject(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("JS object handle must not be zero", nameof(handle));
        Handle = handle;
        _reference = new NapiReference(handle);
    }

    /// <summary>创建包装时的 napi_value；跨句柄范围使用请走 <see cref="PinnedValue"/>。</summary>
    public IntPtr Handle { get; }

    /// <summary>从强引用重新取出的 napi_value（可在当前句柄范围外安全使用）。</summary>
    internal IntPtr PinnedValue => _reference == null ? Handle : _reference.Value;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _reference?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>读取实例属性并按目标类型转换（基元 + 枚举 + IntPtr）。</summary>
    protected T GetProperty<T>(ReadOnlySpan<byte> name)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return ValueConverter.Convert<T>(NodeApi.GetProperty(Handle, name));
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>读取实例属性为原生句柄（数组/包装类等复杂类型，交给调用方处理）。</summary>
    protected IntPtr GetPropertyRaw(ReadOnlySpan<byte> name)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.GetProperty(Handle, name);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>写入实例属性。</summary>
    protected void SetProperty(ReadOnlySpan<byte> name, object? value)
    {
#if HARMONYOS
        ThrowIfDisposed();
        NodeApi.SetProperty(Handle, name, value);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用实例方法（返回值经 <see cref="ValueConverter"/> 转换：基元 + 枚举 + IntPtr）。</summary>
    protected T CallMethod<T>(ReadOnlySpan<byte> methodName, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.CallMethod<T>(Handle, methodName, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用实例方法，返回值经调用点显式转换委托处理（数组/包装类等复杂类型）。</summary>
    protected TCall CallMethod<TCall>(ReadOnlySpan<byte> methodName, Func<IntPtr, TCall> convert, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.CallMethod(Handle, methodName, convert, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用无返回值的实例方法。</summary>
    protected void CallMethodVoid(ReadOnlySpan<byte> methodName, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        NodeApi.CallMethodVoid(Handle, methodName, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用实例方法并把 Promise 接为 Task&lt;T&gt;（基元 + 枚举 + IntPtr）。</summary>
    protected Task<T> CallMethodAsync<T>(ReadOnlySpan<byte> methodName, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.CallMethodAsync<T>(Handle, methodName, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用实例方法并把 Promise 接为 Task&lt;T&gt;，结果经调用点显式转换委托处理。</summary>
    protected Task<TCall> CallMethodAsync<TCall>(ReadOnlySpan<byte> methodName, Func<IntPtr, TCall> convert, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.CallMethodAsync(Handle, methodName, convert, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>调用实例方法并把 Promise 接为 Task（Promise&lt;void&gt; 路线）。</summary>
    protected Task CallMethodAsyncVoid(ReadOnlySpan<byte> methodName, params object?[]? args)
    {
#if HARMONYOS
        ThrowIfDisposed();
        return NodeApi.CallMethodAsyncVoid(Handle, methodName, args);
#else
        throw new PlatformNotSupportedException("JsObject requires HarmonyOS runtime");
#endif
    }

    /// <summary>把 napi 数组拆为元素句柄数组（生成代码的数组转换器基础件）。</summary>
    protected static IntPtr[] GetArrayElements(IntPtr array) => NodeApi.GetArrayElements(array);
}
