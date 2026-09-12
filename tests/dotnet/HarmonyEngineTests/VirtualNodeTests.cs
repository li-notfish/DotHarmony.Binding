// ArkTS 引擎实验层的纯逻辑测试：VirtualNode API → 指令流的映射、指令队列语义。
// 全部经 RecordingSink 假总线，不触碰 napi（真机行为由 EngineLab + hilog 验证）。
using HarmonyOS.Bindings.Experimental;
using Xunit;

namespace HarmonyEngineTests;

/// <summary>记录型假指令总线</summary>
internal sealed class RecordingSink : IArkTsCommandSink
{
    public List<ArkTsCommand> Commands { get; } = new();

    public void Enqueue(ArkTsCommand command) => Commands.Add(command);
}

public class VirtualNodeTests
{
    private static (VirtualNode Node, RecordingSink Sink) NewNode(string type = "Stack")
    {
        var sink = new RecordingSink();
        var node = new VirtualNode(type, 1, sink);
        return (node, sink);
    }

    [Fact]
    public void Create_QueuesCreateCommandWithType()
    {
        var (node, sink) = NewNode("Text");
        Assert.Equal(1, node.Id);
        var cmd = Assert.Single(sink.Commands);
        Assert.Equal(ArkTsCommandOp.Create, cmd.Op);
        Assert.Equal(1, cmd.NodeId);
        Assert.Equal("Text", cmd.NodeType);
    }

    [Fact]
    public void PublicCtor_AllocatesMonotonicIds_AndRegistersInEngine()
    {
        var a = new VirtualNode("Text");
        var b = new VirtualNode("Text");
        Assert.True(b.Id > a.Id);
        Assert.NotNull(ArkTsEngineTestAccess.GetNode(a.Id));
        Assert.NotNull(ArkTsEngineTestAccess.GetNode(b.Id));
    }

    [Fact]
    public void SetAttrs_FluentHelpers_ProduceNameValuePairs()
    {
        var (node, sink) = NewNode("Text");
        node.SetWidth(12f).SetHeightPercent(50f).SetText("hi").SetBackgroundColor(0xFF336699u);

        // [0]=Create（internal ctor 入队）+ 4 条 SetAttrs
        Assert.Equal(5, sink.Commands.Count);
        Assert.Equal(ArkTsCommandOp.Create, sink.Commands[0].Op);
        var width = sink.Commands[1];
        Assert.Equal(ArkTsCommandOp.SetAttrs, width.Op);
        Assert.Equal("width", width.Attrs![0].Key);
        Assert.Equal(12f, width.Attrs[0].Value);

        Assert.Equal("height", sink.Commands[2].Attrs![0].Key);
        Assert.Equal("50%", sink.Commands[2].Attrs[0].Value);

        Assert.Equal("text", sink.Commands[3].Attrs![0].Key);
        Assert.Equal("hi", sink.Commands[3].Attrs[0].Value);

        Assert.Equal("backgroundColor", sink.Commands[4].Attrs![0].Key);
        Assert.Equal("#FF336699", sink.Commands[4].Attrs[0].Value);
    }

    [Fact]
    public void AddChild_ProducesOrderedSetChildren()
    {
        var (parent, sink) = NewNode();
        var a = new VirtualNode("Text", 2, sink);
        var b = new VirtualNode("Button", 3, sink);

        parent.AddChild(a);
        parent.AddChild(b);

        var first = Assert.Single(sink.Commands, c => c.Op == ArkTsCommandOp.SetChildren && c.Children!.SequenceEqual(new[] { 2 }));
        var second = sink.Commands.Last(c => c.Op == ArkTsCommandOp.SetChildren);
        Assert.Equal(new[] { 2, 3 }, second.Children);
    }

    [Fact]
    public void RemoveChild_DetachesOnly_WithoutDelete()
    {
        var (parent, sink) = NewNode();
        var a = new VirtualNode("Text", 2, sink);
        parent.AddChild(a);
        sink.Commands.Clear();

        parent.RemoveChild(a);

        var cmd = Assert.Single(sink.Commands);
        Assert.Equal(ArkTsCommandOp.SetChildren, cmd.Op);
        Assert.Empty(cmd.Children!);
        Assert.Empty(parent.Children);
        // 摘除语义：节点可再挂回，不产生 Delete
        parent.AddChild(a);
        Assert.Equal(new[] { 2 }, sink.Commands.Last(c => c.Op == ArkTsCommandOp.SetChildren).Children);
    }

    [Fact]
    public void Dispose_CascadesDeleteOverSubtree()
    {
        var (root, sink) = NewNode();
        var child = new VirtualNode("Text", 2, sink);
        var grand = new VirtualNode("Button", 3, sink);
        root.AddChild(child);
        child.AddChild(grand);
        sink.Commands.Clear();

        root.Dispose();

        Assert.Equal(new[] { 3, 2, 1 }, sink.Commands.Where(c => c.Op == ArkTsCommandOp.Delete).Select(c => c.NodeId));
        Assert.Throws<ObjectDisposedException>(() => root.SetWidth(1f));
    }

    [Fact]
    public void On_FirstSubscriptionEmitsSetEvents_LaterOnesDoNot()
    {
        var (node, sink) = NewNode();
        node.On("click", _ => { });
        node.On("click", _ => { });

        var setEvents = sink.Commands.Where(c => c.Op == ArkTsCommandOp.SetEvents).ToList();
        var cmd = Assert.Single(setEvents);
        Assert.Equal(new[] { "click" }, cmd.Events);

        node.On("area", _ => { });
        setEvents = sink.Commands.Where(c => c.Op == ArkTsCommandOp.SetEvents).ToList();
        Assert.Equal(2, setEvents.Count);
        Assert.Contains("area", setEvents[1].Events!);
        Assert.Contains("click", setEvents[1].Events!);
    }

    [Fact]
    public void DispatchEvent_InvokesMatchingHandlers_Only()
    {
        var (node, _) = NewNode();
        int clicks = 0, areas = 0;
        node.On("click", e => { Assert.Equal("click", e.Kind); clicks++; });
        node.On("area", _ => areas++);

        node.DispatchEvent(new ArkTsEventArgs { Kind = "click" });
        node.DispatchEvent(new ArkTsEventArgs { Kind = "click" });
        node.DispatchEvent(new ArkTsEventArgs { Kind = "area", WidthPx = 100, HeightPx = 40, Density = 3.25 });

        Assert.Equal(2, clicks);
        Assert.Equal(1, areas);
    }

    [Fact]
    public void MeasuredSize_DefaultsToEmpty_BeforeAreaBackflow()
    {
        var (node, _) = NewNode();
        Assert.Equal(ArkTsSize.Empty, node.MeasuredSize);
    }

    [Fact]
    public void SetRoot_EnqueuesSetRootCommand()
    {
        var before = ArkTsEngineTestAccess.PendingCommandCount;
        var (node, _) = NewNode();
        ArkTsEngine.SetRoot(node);
        Assert.Equal(before + 1, ArkTsEngineTestAccess.PendingCommandCount);
    }
}

/// <summary>测试桥（internal 访问面）</summary>
internal static class ArkTsEngineTestAccess
{
    public static VirtualNode? GetNode(int id) => ArkTsEngine.GetNode(id);
    public static int PendingCommandCount => ArkTsEngine.PendingCommandCount;
}
