using System;
using System.Collections.Generic;

namespace HarmonyOS.Bindings.Experimental;

/// <summary>
/// ArkTS 引擎实验层的虚拟节点：公开方法面是 <c>ArkUINodeBase</c> 的子集
/// （MIGRATION_ARKTS_ENGINE.md §2.1 接口不变量的实验验证），差异仅在
/// "同步原生写入/回读" 换成 "指令入队 / 影子量测快照"。
/// 所有写操作只翻译为 <see cref="ArkTsCommand"/> 入队，经 <see cref="ArkTsEngine.Flush"/>
/// 批量冲刷——本类不触碰任何 napi/C API，可离线单测。
/// </summary>
public class VirtualNode : IDisposable
{
    private readonly int _id;
    private readonly IArkTsCommandSink _sink;
    private readonly List<VirtualNode> _children = new();
    private Dictionary<string, List<Action<ArkTsEventArgs>>>? _handlers;
    private bool _disposed;

    /// <summary>引擎侧句柄（指令载荷中的 nodeId）</summary>
    public int Id => _id;

    public VirtualNode(string nodeType)
        : this(nodeType, ArkTsEngine.AllocateId(), ArkTsEngine.Sink)
    {
        ArkTsEngine.Register(this);
    }

    internal VirtualNode(string nodeType, int id, IArkTsCommandSink sink)
    {
        _id = id;
        _sink = sink;
        _sink.Enqueue(ArkTsCommand.Create(id, nodeType));
    }

    /// <summary>量测影子快照（引擎 onAreaChange 回流；回流前为 0x0）</summary>
    public ArkTsSize MeasuredSize => ArkTsEngine.GetMeasuredSize(_id);

    #region 属性（对齐 ArkUINodeBase 公开面的子集）

    public VirtualNode SetWidth(float vp) => SetAttr("width", vp);
    public VirtualNode SetWidthPercent(float percent) => SetAttr("width", $"{percent}%");
    public VirtualNode SetHeight(float vp) => SetAttr("height", vp);
    public VirtualNode SetHeightPercent(float percent) => SetAttr("height", $"{percent}%");

    /// <summary>绝对定位（vp；引擎侧 .position({x,y})）</summary>
    public VirtualNode SetPosition(float x, float y) => SetAttr("x", x).SetAttr("y", y);

    public VirtualNode SetOpacity(float opacity) => SetAttr("opacity", opacity);

    public VirtualNode SetBackgroundColor(byte a, byte r, byte g, byte b)
        => SetAttr("backgroundColor", ToHex(a, r, g, b));

    public VirtualNode SetBackgroundColor(uint argb)
        => SetAttr("backgroundColor", ToHex((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));

    // —— Text / Button 专属 ——
    public VirtualNode SetText(string text) => SetAttr("text", text);
    public VirtualNode SetFontSize(float vp) => SetAttr("fontSize", vp);
    public VirtualNode SetFontColor(byte a, byte r, byte g, byte b) => SetAttr("fontColor", ToHex(a, r, g, b));
    public VirtualNode SetFontColor(uint argb)
        => SetAttr("fontColor", ToHex((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));
    public VirtualNode SetLabel(string label) => SetAttr("label", label);

    /// <summary>通用属性写入：属性名与值语义由引擎映射表（DynamicNode/mapping）解释</summary>
    public VirtualNode SetAttr(string name, object value)
    {
        ThrowIfDisposed();
        _sink.Enqueue(ArkTsCommand.SetAttrs(_id, new[] { new KeyValuePair<string, object?>(name, value) }));
        return this;
    }

    #endregion

    #region 树操作

    /// <summary>当前托管子节点（顺序即引擎侧渲染顺序）</summary>
    public IReadOnlyList<VirtualNode> Children => _children;

    public VirtualNode AddChild(VirtualNode child)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
        _sink.Enqueue(ArkTsCommand.SetChildren(_id, CollectIds(_children)));
        return this;
    }

    /// <summary>摘除子节点（仅断开，与 ArkUINodeBase.RemoveChild 语义一致：节点可再挂回）</summary>
    public void RemoveChild(VirtualNode child)
    {
        ThrowIfDisposed();
        if (_children.Remove(child))
            _sink.Enqueue(ArkTsCommand.SetChildren(_id, CollectIds(_children)));
    }

    /// <summary>摘除全部子节点（仅断开；整树回收走 Dispose 的级联删除）</summary>
    public void RemoveAllChildren()
    {
        ThrowIfDisposed();
        if (_children.Count == 0) return;
        _children.Clear();
        _sink.Enqueue(ArkTsCommand.SetChildren(_id, CollectIds(_children)));
    }

    private static int[] CollectIds(List<VirtualNode> nodes)
    {
        var ids = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
            ids[i] = nodes[i]._id;
        return ids;
    }

    #endregion

    #region 事件

    /// <summary>订阅引擎回流事件（实验子集：'click'；量测 'area' 由引擎主动推送，同样可订阅）</summary>
    public VirtualNode On(string eventType, Action<ArkTsEventArgs> handler)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(handler);
        bool first = false;
        if (_handlers is null)
        {
            _handlers = new Dictionary<string, List<Action<ArkTsEventArgs>>>(StringComparer.Ordinal);
        }
        if (!_handlers.TryGetValue(eventType, out var list))
        {
            list = new List<Action<ArkTsEventArgs>>();
            _handlers[eventType] = list;
            first = true;
        }
        list.Add(handler);

        if (first)
        {
            // SetEvents 为整体替换：当前节点已订阅的事件名 + 新事件
            var names = new List<string>(_handlers.Keys);
            _sink.Enqueue(ArkTsCommand.SetEvents(_id, names.ToArray()));
        }
        return this;
    }

    /// <summary>引擎回流分发（ArkTsEngine 在 napi 回调内调用）</summary>
    internal void DispatchEvent(in ArkTsEventArgs args)
    {
        if (_handlers is null) return;
        if (!_handlers.TryGetValue(args.Kind, out var list)) return;
        foreach (var handler in list)
            handler(args);
    }

    #endregion

    /// <summary>级联释放：递归 Dispose 子树后删除自身（引擎侧 removeRecursive 对已删 id 幂等）</summary>
    public void Dispose()
    {
        if (_disposed) return;
        foreach (var child in _children)
            child.Dispose();
        _children.Clear();
        ArkTsEngine.Unregister(_id);
        _handlers = null;
        _disposed = true;
        _sink.Enqueue(ArkTsCommand.Delete(_id));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VirtualNode));
    }

    private static string ToHex(byte a, byte r, byte g, byte b)
        => $"#{a:X2}{r:X2}{g:X2}{b:X2}";
}
