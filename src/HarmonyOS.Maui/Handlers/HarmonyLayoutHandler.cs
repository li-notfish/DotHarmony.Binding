// HarmonyLayoutHandler：MAUI ILayout（StackLayout 等）→ ArkUI Column/Row 容器
//
// 布局模型（M1）：ArkUI flex 引擎托管子节点布局（Column/Row 自治），
// MAUI 的 Measure/Arrange 请求不透传——WidthRequest 等约束暂不生效。
// CommandMapper 实现 Controls 的 ILayoutHandler 子树协议
// （Add/Remove/Clear/Insert/Update/UpdateZIndex），子 handler 由 HarmonyHandlerFactory 装配。
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using MControlsLayout = Microsoft.Maui.Controls.Layout;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Layout 的 HarmonyOS Handler（ArkUI Column/Row 托管布局）。</summary>
public class HarmonyLayoutHandler : ViewHandler<MControlsLayout, ArkUINode>
{
    public static PropertyMapper<MControlsLayout, HarmonyLayoutHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(MControlsLayout.BackgroundColor)] = (h, v) =>
            {
                if (v.BackgroundColor is { } c)
                    h.PlatformView.SetBackgroundColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
        };

    // Controls 布局的子树变更协议（字符串命令，经 Handler.Invoke 派发）
    public static CommandMapper<MControlsLayout, HarmonyLayoutHandler> LayoutCommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ILayoutHandler.Add)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyLayoutHandler)h).Add(u.View); },
            [nameof(ILayoutHandler.Remove)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyLayoutHandler)h).Remove(u.View); },
            [nameof(ILayoutHandler.Clear)] = (h, v, args) => ((HarmonyLayoutHandler)h).Clear(),
            [nameof(ILayoutHandler.Insert)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyLayoutHandler)h).Insert(u.Index, u.View); },
            [nameof(ILayoutHandler.Update)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyLayoutHandler)h).Update(u.Index, u.View); },
            [nameof(ILayoutHandler.UpdateZIndex)] = (h, v, args) => { /* ArkUI flex 按 addChild 顺序，z-index M1 忽略 */ },
        };

    public HarmonyLayoutHandler() : base(Mapper, LayoutCommandMapper) { }

    protected override ArkUINode CreatePlatformView()
    {
        // StackLayout 按 Orientation 选择 ArkUI 容器；其余布局默认 Column
        if (VirtualView is Microsoft.Maui.Controls.StackLayout sl &&
            sl.Orientation == Microsoft.Maui.Controls.StackOrientation.Horizontal)
        {
            return new HarmonyOS.ArkUI.Row();
        }
        return new HarmonyOS.ArkUI.Column();
    }

    protected override void ConnectHandler(ArkUINode platformView)
    {
        base.ConnectHandler(platformView);
        // 连接时全量同步已存在的 Children（Controls 侧在 Handler 连接前添加的子节点不会发 Add 命令）
        foreach (var child in ((Microsoft.Maui.Controls.Layout)VirtualView).Children)
        {
            AttachChild(child);
        }
    }

    private readonly Dictionary<IView, IElementHandler> _children = new();

    private void AttachChild(IView view)
    {
        if (_children.ContainsKey(view)) return;
        var handler = HarmonyHandlerFactory.Create(view);
        handler.SetVirtualView(view);
        if (handler.PlatformView is ArkUINode node)
        {
            PlatformView.AddChild(node);
            _children[view] = handler;
        }
    }

    internal void Add(IView view) => AttachChild(view);

    internal void Remove(IView view)
    {
        if (_children.Remove(view, out var handler) && handler.PlatformView is ArkUINode node)
        {
            PlatformView.RemoveChild(node);
        }
    }

    internal void Clear()
    {
        PlatformView.RemoveAllChildren();
        _children.Clear();
    }

    internal void Insert(int index, IView view)
    {
        // M1：中段插入暂退化为顺序追加（ArkUI flex 场景以追加为主）
        AttachChild(view);
    }

    internal void Update(int index, IView view)
    {
        // M1：替换语义简化为 Remove + Insert
        if (_children.TryGetValue(view, out var old) && old.PlatformView is ArkUINode oldNode)
            PlatformView.RemoveChild(oldNode);
        Insert(index, view);
    }
}
