#nullable enable
using System;
using System.Collections.Generic;

namespace HarmonyOS.Maui.Handlers;

/// <summary>
/// Handler-owned 原生子节点生命周期跟踪器。
/// Replace 保留仍在下一代的节点，只 Dispose 淘汰节点，避免动态重建泄漏。
/// </summary>
internal sealed class NativeChildTracker<TNode> where TNode : IDisposable
{
    private readonly Action<TNode> _addChild;
    private readonly Action _removeAllChildren;
    private readonly List<TNode> _children = new();

    public NativeChildTracker(Action<TNode> addChild, Action removeAllChildren)
    {
        ArgumentNullException.ThrowIfNull(addChild);
        ArgumentNullException.ThrowIfNull(removeAllChildren);
        _addChild = addChild;
        _removeAllChildren = removeAllChildren;
    }

    public IReadOnlyList<TNode> Children => _children;

    public TNode Add(TNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _addChild(child);
        _children.Add(child);
        return child;
    }

    public void Replace(IEnumerable<TNode> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        var next = children as IList<TNode> ?? new List<TNode>(children);
        var retained = new HashSet<TNode>(next);

        foreach (var old in _children)
        {
            if (!retained.Contains(old))
                old.Dispose();
        }

        _removeAllChildren();
        _children.Clear();
        foreach (var child in next)
        {
            _addChild(child);
            _children.Add(child);
        }
    }

    public void Clear()
    {
        foreach (var child in _children)
            child.Dispose();
        _removeAllChildren();
        _children.Clear();
    }
}
