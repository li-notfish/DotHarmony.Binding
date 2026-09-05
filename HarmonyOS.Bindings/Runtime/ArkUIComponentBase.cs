using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ArkUI 组件基类，包含 IDisposable 模板和属性访问方法
/// </summary>
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ArkUIComponentBase))]
public abstract class ArkUIComponentBase : IDisposable
{
    protected IntPtr _jsObject;
    private bool _disposed;

    protected ArkUIComponentBase(IntPtr jsObject)
    {
        _jsObject = jsObject;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) { }
            NodeApi.DestroyComponent(_jsObject);
            _jsObject = IntPtr.Zero;
            _disposed = true;
        }
    }

    ~ArkUIComponentBase() => Dispose(false);

    protected IntPtr GetProperty(string name)
    {
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(name);
        NativeNodeApi.napi_get_named_property(env, _jsObject, nameBytes, out var result);
        return result;
    }

    protected IntPtr GetProperty(byte[] name)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_named_property(env, _jsObject, name, out var result);
        return result;
    }

    protected void SetProperty(string name, IntPtr value)
    {
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(name);
        NativeNodeApi.napi_set_named_property(env, _jsObject, nameBytes, value);
    }

    protected void SetProperty(byte[] name, IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_set_named_property(env, _jsObject, name, value);
    }
}

/// <summary>
/// ArkUI 属性设置器基类，仅包装指针，不负责释放
/// </summary>
[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ArkUIAttributeBase))]
public abstract class ArkUIAttributeBase : IDisposable
{
    protected IntPtr _jsObject;
    private bool _disposed;

    protected ArkUIAttributeBase(IntPtr jsObject)
    {
        _jsObject = jsObject;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) { }
            _jsObject = IntPtr.Zero;
            _disposed = true;
        }
    }

    ~ArkUIAttributeBase() => Dispose(false);
}
