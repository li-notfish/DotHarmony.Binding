using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using HarmonyOS.Bindings.Runtime;

namespace HarmonyOS.ArkUI;

/// <summary>
/// ArkUI 组件基类，包含 IDisposable 模板和属性访问方法。
/// 防裁剪由 TrimmerRootAssembly + ILLink.Descriptors.xml 保证。
/// </summary>
public abstract class ArkUIComponentBase : IDisposable
{
    protected IntPtr _jsObject;
    private readonly List<GCHandle> _eventHandles = new();
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
            if (disposing)
            {
                foreach (var gch in _eventHandles)
                {
                    NodeApi.FreeEventHandle(gch);
                }
                _eventHandles.Clear();
            }
            NodeApi.DestroyComponent(_jsObject);
            _jsObject = IntPtr.Zero;
            _disposed = true;
        }
    }

    ~ArkUIComponentBase() => Dispose(false);

    /// <summary>
    /// 注册事件时由生成代码调用，登记委托的 GCHandle，确保组件销毁时释放
    /// </summary>
    protected void TrackEventHandle(GCHandle gch)
    {
        _eventHandles.Add(gch);
    }

    protected IntPtr GetProperty(string name)
    {
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(name);
        NativeNodeApi.napi_get_named_property(env, _jsObject, nameBytes, out var result)
            .ThrowIfFailed();
        return result;
    }

    protected IntPtr GetProperty(byte[] name)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_named_property(env, _jsObject, name, out var result)
            .ThrowIfFailed();
        return result;
    }

    protected void SetProperty(string name, IntPtr value)
    {
        var env = NapiEnv.Current;
        var nameBytes = Encoding.UTF8.GetBytes(name);
        NativeNodeApi.napi_set_named_property(env, _jsObject, nameBytes, value)
            .ThrowIfFailed();
    }

    protected void SetProperty(byte[] name, IntPtr value)
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_set_named_property(env, _jsObject, name, value)
            .ThrowIfFailed();
    }
}

/// <summary>
/// ArkUI 属性设置器基类，仅包装指针，不负责释放
/// </summary>
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
