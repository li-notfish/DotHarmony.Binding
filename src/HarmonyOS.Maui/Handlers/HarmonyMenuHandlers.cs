#nullable enable
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkButton = HarmonyOS.ArkUI.Button;
using ArkRow = HarmonyOS.ArkUI.Row;
using ArkStack = HarmonyOS.ArkUI.Stack;
using ArkText = HarmonyOS.ArkUI.Text;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyMenuBarHandler : ElementHandler<MenuBar, ArkRow>
{
    public static PropertyMapper<MenuBar, HarmonyMenuBarHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(MenuBar.IsEnabled)] = MapIsEnabled,
        };

    public static CommandMapper<MenuBar, HarmonyMenuBarHandler> Commands =
        new(ElementHandler.ElementCommandMapper)
        {
            [nameof(IMenuBarHandler.Add)] = Rebuild,
            [nameof(IMenuBarHandler.Remove)] = Rebuild,
            [nameof(IMenuBarHandler.Clear)] = Rebuild,
            [nameof(IMenuBarHandler.Insert)] = Rebuild,
        };

    public HarmonyMenuBarHandler() : base(Mapper, Commands) { }

    private NativeChildTracker<ArkButton> _childTracker = null!;

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _childTracker = new NativeChildTracker<ArkButton>(row.AddChild, row.RemoveAllChildren);
        return row;
    }

    public static void MapIsEnabled(HarmonyMenuBarHandler handler, MenuBar view)
        => Rebuild(handler, view, null);

    public static void Rebuild(HarmonyMenuBarHandler handler, MenuBar view, object? args)
    {
        var buttons = new List<ArkButton>();
        foreach (var item in view)
        {
            if (item is not MenuBarItem barItem)
                continue;

            var button = new ArkButton
            {
                Label = barItem.Text ?? string.Empty,
                Enabled = barItem.IsEnabled,
            };
            button.SetWidth(96f);
            button.SetHeight(40f);
            buttons.Add(button);
        }
        handler._childTracker.Replace(buttons);
    }
}

public class HarmonyMenuBarItemHandler : ElementHandler<MenuBarItem, ArkButton>
{
    public static PropertyMapper<MenuBarItem, HarmonyMenuBarItemHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(MenuBarItem.Text)] = MapText,
            [nameof(MenuBarItem.IsEnabled)] = MapIsEnabled,
        };

    public HarmonyMenuBarItemHandler() : base(Mapper) { }

    protected override ArkButton CreatePlatformElement() => new();

    public static void MapText(HarmonyMenuBarItemHandler handler, MenuBarItem view)
        => handler.PlatformView.Label = view.Text ?? string.Empty;

    public static void MapIsEnabled(HarmonyMenuBarItemHandler handler, MenuBarItem view)
        => handler.PlatformView.Enabled = view.IsEnabled;
}

public class HarmonyMenuFlyoutHandler : ElementHandler<MenuFlyout, ArkStack>
{
    public static IPropertyMapper<IMenuFlyout, IMenuFlyoutHandler> Mapper =
        new PropertyMapper<IMenuFlyout, IMenuFlyoutHandler>(ElementHandler.ElementMapper);

    public static CommandMapper<MenuFlyout, HarmonyMenuFlyoutHandler> Commands =
        new(ElementHandler.ElementCommandMapper)
        {
            [nameof(IMenuFlyoutHandler.Add)] = Rebuild,
            [nameof(IMenuFlyoutHandler.Remove)] = Rebuild,
            [nameof(IMenuFlyoutHandler.Clear)] = Rebuild,
            [nameof(IMenuFlyoutHandler.Insert)] = Rebuild,
        };

    public HarmonyMenuFlyoutHandler() : base(Mapper, Commands) { }

    private NativeChildTracker<ArkButton> _childTracker = null!;

    protected override ArkStack CreatePlatformElement()
    {
        var stack = new ArkStack();
        _childTracker = new NativeChildTracker<ArkButton>(stack.AddChild, stack.RemoveAllChildren);
        return stack;
    }

    public static void Rebuild(HarmonyMenuFlyoutHandler handler, MenuFlyout view, object? args)
    {
        var buttons = new List<ArkButton>();
        foreach (var item in view)
        {
            if (item is not MenuItem menuItem)
                continue;

            var button = new ArkButton
            {
                Label = menuItem.Text ?? string.Empty,
                Enabled = menuItem.IsEnabled,
            };
            button.SetWidth(120f);
            button.SetHeight(40f);
            buttons.Add(button);
        }
        handler._childTracker.Replace(buttons);
    }
}

public class HarmonyMenuFlyoutItemHandler : ElementHandler<MenuFlyoutItem, ArkButton>
{
    public static PropertyMapper<MenuFlyoutItem, HarmonyMenuFlyoutItemHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(MenuItem.Text)] = MapText,
            [nameof(MenuItem.IsEnabled)] = MapIsEnabled,
        };

    public HarmonyMenuFlyoutItemHandler() : base(Mapper) { }

    protected override ArkButton CreatePlatformElement() => new();

    public static void MapText(HarmonyMenuFlyoutItemHandler handler, MenuFlyoutItem view)
        => handler.PlatformView.Label = view.Text ?? string.Empty;

    public static void MapIsEnabled(HarmonyMenuFlyoutItemHandler handler, MenuFlyoutItem view)
        => handler.PlatformView.Enabled = view.IsEnabled;
}

public class HarmonyMenuFlyoutSubItemHandler : ElementHandler<MenuFlyoutSubItem, ArkStack>
{
    public static IPropertyMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> Mapper =
        new PropertyMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler>(ElementHandler.ElementMapper);

    public HarmonyMenuFlyoutSubItemHandler() : base(Mapper) { }

    protected override ArkStack CreatePlatformElement() => new();
}

public class HarmonyMenuFlyoutSeparatorHandler : ElementHandler<MenuFlyoutSeparator, ArkText>
{
    public HarmonyMenuFlyoutSeparatorHandler() : base(ElementHandler.ElementMapper) { }

    protected override ArkText CreatePlatformElement()
    {
        var text = new ArkText { Content = "────────", FontSize = (float)HarmonyControlDefaults.DetailTextFontSizeDefault };
        text.SetWidth(120f);
        text.SetHeight(1f);
        return text;
    }
}

public class HarmonyToolbarHandler : ElementHandler<Toolbar, ArkRow>
{
    public static PropertyMapper<Toolbar, HarmonyToolbarHandler> Mapper =
        new(ElementHandler.ElementMapper)
        {
            [nameof(Toolbar.Title)] = Rebuild,
            [nameof(Toolbar.ToolbarItems)] = Rebuild,
            [nameof(Toolbar.IsVisible)] = MapIsVisible,
            [nameof(Toolbar.BarTextColor)] = Rebuild,
            [nameof(Toolbar.IconColor)] = Rebuild,
        };

    public HarmonyToolbarHandler() : base(Mapper) { }

    private NativeChildTracker<HarmonyOS.Bindings.NativeNode.ArkUINodeBase> _childTracker = null!;

    protected override ArkRow CreatePlatformElement()
    {
        var row = new ArkRow();
        _childTracker = new NativeChildTracker<HarmonyOS.Bindings.NativeNode.ArkUINodeBase>(row.AddChild, row.RemoveAllChildren);
        return row;
    }

    public static void Rebuild(HarmonyToolbarHandler handler, Toolbar view)
    {
        var children = new List<HarmonyOS.Bindings.NativeNode.ArkUINodeBase>();

        var title = new ArkText
        {
            Content = view.Title ?? string.Empty,
            FontSize = (float)HarmonyControlDefaults.TextFontSizeDefault,
        };
        title.SetWidth(160f);
        title.SetHeight(40f);
        children.Add(title);

        if (view.ToolbarItems is null)
            return;

        foreach (var item in view.ToolbarItems)
        {
            var button = new ArkButton
            {
                Label = item.Text ?? string.Empty,
                Enabled = item.IsEnabled,
            };
            button.SetWidth(80f);
            button.SetHeight(40f);
            button.Click += _ => ((IMenuItemController)item).Activate();
            children.Add(button);
        }
        handler._childTracker.Replace(children);
    }

    public static void MapIsVisible(HarmonyToolbarHandler handler, Toolbar view)
        => handler.PlatformView.SetVisibility(
            view.IsVisible
                ? ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE
                : ArkUI_Visibility.ARKUI_VISIBILITY_NONE);
}
