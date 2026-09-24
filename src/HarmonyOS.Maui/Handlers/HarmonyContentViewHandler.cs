// ContentView / ContentPresenter 的 HarmonyOS Handler。
// 语义对齐 MAUI 官方（dotnet/maui src/Controls/src/Core/ContentView）：
// 模板化控件的呈现内容为 TemplateRoot（ControlTemplate 实例化产物），
// 其余 ContentView 直接呈现 Content；ContentPresenter 是模板内承载外层
// Content 的槽位（TemplateBinding 已由核心链好）。
#nullable enable
using System.Runtime.CompilerServices;
using HarmonyOS.Interop;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI ContentView（含 TemplatedView 模板化子类）的 Handler。</summary>
public class HarmonyContentViewHandler : HarmonyViewHandler<IContentView, ArkColumn>
{
    public static PropertyMapper<IContentView, HarmonyContentViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(IContentView.Content)] = MapContent,
        [nameof(IContentView.Background)] = MapBackground,
        [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyContentViewHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        column.SetWidthPercent(1.0f);
        column.SetHeightPercent(1.0f);
        return column;
    }

    public static void MapBackground(HarmonyContentViewHandler h, IContentView v)
        => BrushHelper.ApplyBackground(h.PlatformView, v.Background!);

    public static void MapBackgroundColor(HarmonyContentViewHandler h, IContentView v)
    {
        if (v is VisualElement ve && ve.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapContent(HarmonyContentViewHandler handler, IContentView view)
        => ContentHost.Update(handler, ResolvePresentedContent(view));

    /// <summary>ContentView 呈现内容：模板化时为 TemplateRoot（PresentedContent），否则为 Content。</summary>
    internal static IView? ResolvePresentedContent(IContentView view)
        => view.PresentedContent as IView ?? view.Content as IView;
}

/// <summary>布局模板内 ContentPresenter（承载外层 ContentView.Content 的槽位）的 Handler。</summary>
public class HarmonyContentPresenterHandler : HarmonyViewHandler<ContentPresenter, ArkColumn>
{
    public static PropertyMapper<ContentPresenter, HarmonyContentPresenterHandler> Mapper = new(ViewMapper)
    {
        [nameof(ContentPresenter.Content)] = MapContent,
        [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
    };

    public HarmonyContentPresenterHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        column.SetWidthPercent(1.0f);
        column.SetHeightPercent(1.0f);
        return column;
    }

    public static void MapBackgroundColor(HarmonyContentPresenterHandler h, ContentPresenter v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapContent(HarmonyContentPresenterHandler handler, ContentPresenter view)
        => ContentHost.Update(handler, view.Content);
}

/// <summary>单 Content 容器的公共挂树/断树（ContentView/ContentPresenter 共用）。</summary>
internal static class ContentHost
{
    private sealed class Holder { public IElementHandler? Handler; }
    private static readonly ConditionalWeakTable<IElementHandler, Holder> _holders = new();
#pragma warning disable CS0618 // SetChildInheritedBindingContext 由模板核心链好子级 BindingContext
    public static void Update<TVirtualView, TPlatform>(HarmonyViewHandler<TVirtualView, TPlatform> self, IView? content)
        where TVirtualView : class, Microsoft.Maui.IView
        where TPlatform : ArkUINodeBase
    {
        var holder = _holders.GetValue(self, _ => new Holder());
        if (holder.Handler is not null)
        {
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(holder.Handler, self.PlatformView);
            holder.Handler = null;
        }
        if (content is null) return;
        HiLog.Info("HarmonyHost", $"[ContentHost] attach {content.GetType().Name} under {self.VirtualView?.GetType().Name}");
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        holder.Handler = childHandler;
        if (childHandler.PlatformView is ArkUINode node)
        {
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            self.PlatformView.AddChild(node);
        }
    }
#pragma warning restore CS0618
}
