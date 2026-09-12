using System.Collections.Generic;

namespace HarmonyOS.Bindings.Experimental;

/// <summary>
/// ArkTS 命令总线引擎的指令模型（MIGRATION_ARKTS_ENGINE.md §2.2 的最小实验子集）。
/// VirtualNode 的每次属性/树操作都翻译为一条指令入队，Flush 时一次 napi 调用批量发给引擎。
/// 指令与 napi 封送解耦：纯托管模型，可离线单测。
/// </summary>
public enum ArkTsCommandOp
{
    /// <summary>创建节点：nodeId + nodeType</summary>
    Create,
    /// <summary>写属性：nodeId + Attrs（name→value 有序列表，值支持 double/int/string）</summary>
    SetAttrs,
    /// <summary>整体替换有序子节点列表：nodeId + Children（childId）</summary>
    SetChildren,
    /// <summary>整体替换事件订阅表：nodeId + Events（事件名）</summary>
    SetEvents,
    /// <summary>删除节点（引擎级联删除子树）：nodeId</summary>
    Delete,
    /// <summary>设定引擎根节点：nodeId</summary>
    SetRoot,
}

/// <summary>
/// 单条指令。Attrs/Children/Events 按指令类型选择性填充。
/// </summary>
public sealed class ArkTsCommand
{
    public ArkTsCommandOp Op { get; }
    public int NodeId { get; }
    /// <summary>仅 Create：ArkUI 组件类型名（'Stack'/'Text'/'Button'，引擎映射表负责分派）</summary>
    public string? NodeType { get; }
    /// <summary>仅 SetAttrs：name→value 有序对（保持插入序便于测试与日志）</summary>
    public IReadOnlyList<KeyValuePair<string, object?>>? Attrs { get; }
    /// <summary>仅 SetChildren：有序 childId 列表</summary>
    public IReadOnlyList<int>? Children { get; }
    /// <summary>仅 SetEvents：事件名列表（去重后）</summary>
    public IReadOnlyList<string>? Events { get; }

    private ArkTsCommand(ArkTsCommandOp op, int nodeId, string? nodeType = null,
        KeyValuePair<string, object?>[]? attrs = null,
        int[]? children = null,
        string[]? events = null)
    {
        Op = op;
        NodeId = nodeId;
        NodeType = nodeType;
        Attrs = attrs;
        Children = children;
        Events = events;
    }

    public static ArkTsCommand Create(int nodeId, string nodeType) => new(ArkTsCommandOp.Create, nodeId, nodeType);

    public static ArkTsCommand SetAttrs(int nodeId, KeyValuePair<string, object?>[] attrs)
        => new(ArkTsCommandOp.SetAttrs, nodeId, attrs: attrs);

    public static ArkTsCommand SetChildren(int nodeId, int[] children) => new(ArkTsCommandOp.SetChildren, nodeId, children: children);

    public static ArkTsCommand SetEvents(int nodeId, string[] events) => new(ArkTsCommandOp.SetEvents, nodeId, events: events);

    public static ArkTsCommand Delete(int nodeId) => new(ArkTsCommandOp.Delete, nodeId);

    public static ArkTsCommand SetRoot(int nodeId) => new(ArkTsCommandOp.SetRoot, nodeId);

    public override string ToString()
    {
        var detail = Op switch
        {
            ArkTsCommandOp.Create => NodeType,
            ArkTsCommandOp.SetAttrs => string.Join(",", Attrs ?? []),
            ArkTsCommandOp.SetChildren => string.Join(",", Children ?? []),
            ArkTsCommandOp.SetEvents => string.Join(",", Events ?? []),
            _ => "",
        };
        return $"{Op}(id={NodeId}{(detail.Length > 0 ? $", {detail}" : "")})";
    }
}

/// <summary>
/// 指令出口抽象：VirtualNode 只依赖此接口（真实实现为引擎队列+Flush，测试用记录型假总线）。
/// </summary>
public interface IArkTsCommandSink
{
    void Enqueue(ArkTsCommand command);
}
