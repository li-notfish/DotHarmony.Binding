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

    private static KeyValuePair<string, object?> FirstAttr(ArkTsCommand command)
    {
        var attrs = command.Attrs;
        Assert.NotNull(attrs);
        return attrs[0];
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
        var widthAttr = FirstAttr(width);
        Assert.Equal("width", widthAttr.Key);
        Assert.Equal(12f, widthAttr.Value);

        var heightAttr = FirstAttr(sink.Commands[2]);
        Assert.Equal("height", heightAttr.Key);
        Assert.Equal("50%", heightAttr.Value);

        var textAttr = FirstAttr(sink.Commands[3]);
        Assert.Equal("text", textAttr.Key);
        Assert.Equal("hi", textAttr.Value);

        var backgroundAttr = FirstAttr(sink.Commands[4]);
        Assert.Equal("backgroundColor", backgroundAttr.Key);
        Assert.Equal("#FF336699", backgroundAttr.Value);
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

    [Fact]
    public void SetPosition_ProducesXYAttrs()
    {
        var (node, sink) = NewNode();
        node.SetPosition(16f, 32f);

        var cmds = sink.Commands.Where(c => c.Op == ArkTsCommandOp.SetAttrs).ToList();
        Assert.Equal(2, cmds.Count);
        var xAttr = FirstAttr(cmds[0]);
        Assert.Equal("x", xAttr.Key);
        Assert.Equal(16f, xAttr.Value);
        var yAttr = FirstAttr(cmds[1]);
        Assert.Equal("y", yAttr.Key);
        Assert.Equal(32f, yAttr.Value);
    }

    [Fact]
    public void ImageAndEntryHelpers_ProduceAttrs()
    {
        var (image, sink) = NewNode("Image");
        image.SetImageSource("icon.png");
        var imageAttr = FirstAttr(sink.Commands.Last(c => c.Op == ArkTsCommandOp.SetAttrs));
        Assert.Equal("src", imageAttr.Key);
        Assert.Equal("icon.png", imageAttr.Value);

        var (entry, entrySink) = NewNode("Entry");
        entry.SetPlaceholder("type here").SetText("abc");
        var attrs = entrySink.Commands.Where(c => c.Op == ArkTsCommandOp.SetAttrs).ToList();
        var placeholderAttr = FirstAttr(attrs[0]);
        Assert.Equal("placeholder", placeholderAttr.Key);
        Assert.Equal("type here", placeholderAttr.Value);
        var entryTextAttr = FirstAttr(attrs[1]);
        Assert.Equal("text", entryTextAttr.Key);
        Assert.Equal("abc", entryTextAttr.Value);
    }

    [Fact]
    public void TextChangeSubscription_SetEventsIncludes()
    {
        var (node, sink) = NewNode("Entry");
        node.On("textChange", _ => { });

        var cmd = Assert.Single(sink.Commands, c => c.Op == ArkTsCommandOp.SetEvents);
        Assert.Equal(new[] { "textChange" }, cmd.Events);
    }

    [Fact]
    public void DispatchEvent_TextChangeCarriesTextValue()
    {
        var (node, _) = NewNode("Entry");
        string? received = null;
        node.On("textChange", e => received = e.Text);

        node.DispatchEvent(new ArkTsEventArgs { Kind = "textChange", Text = "hello" });

        Assert.Equal("hello", received);
    }
}

/// <summary>测试桥（internal 访问面）</summary>
internal static class ArkTsEngineTestAccess
{
    public static VirtualNode? GetNode(int id) => ArkTsEngine.GetNode(id);
    public static int PendingCommandCount => ArkTsEngine.PendingCommandCount;
}
