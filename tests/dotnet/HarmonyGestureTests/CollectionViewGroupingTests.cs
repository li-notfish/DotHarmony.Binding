using System.Collections.ObjectModel;
using HarmonyOS.Maui.Handlers;
using Microsoft.Maui.Controls;
using Xunit;

namespace HarmonyGestureTests;

public class CollectionViewGroupingTests
{
    [Fact]
    public void BuildGroups_CreatesHeaderItemsAndFooterPerGroup()
    {
        var first = new ObservableCollection<string> { "a1", "a2" };
        var second = new ObservableCollection<string> { "b1" };
        var headerTemplate = new DataTemplate(() => new Label());
        var footerTemplate = new DataTemplate(() => new Label());
        var view = new CollectionView
        {
            IsGrouped = true,
            ItemsSource = new[] { first, second },
            GroupHeaderTemplate = headerTemplate,
            GroupFooterTemplate = footerTemplate,
        };

        var groups = HarmonyCollectionViewHandler.BuildGroups(view);

        Assert.Equal(2, groups.Count);
        Assert.Equal(4, groups[0].Slots.Count);
        Assert.Equal(3, groups[1].Slots.Count);
        Assert.Equal(new[]
        {
            CollectionViewSlotKind.Header,
            CollectionViewSlotKind.Item,
            CollectionViewSlotKind.Item,
            CollectionViewSlotKind.Footer,
        }, groups[0].Slots.Select(s => s.Kind).ToArray());
        Assert.Equal(new[]
        {
            CollectionViewSlotKind.Header,
            CollectionViewSlotKind.Item,
            CollectionViewSlotKind.Footer,
        }, groups[1].Slots.Select(s => s.Kind).ToArray());

        Assert.Same(headerTemplate, groups[0].Slots[0].Template);
        Assert.Same(footerTemplate, groups[0].Slots[3].Template);
        Assert.Same(first, groups[0].Group);
        Assert.Equal(new object?[] { first, "a1", "a2", first },
            groups[0].Slots.Select(s => s.Context).ToArray());

        Assert.Equal(new object?[] { second, "b1", second },
            groups[1].Slots.Select(s => s.Context).ToArray());
    }

    [Fact]
    public void BuildGroups_PlainViewKeepsTopLevelHeaderFooterAndItems()
    {
        var headerTemplate = new DataTemplate(() => new Label());
        var footerTemplate = new DataTemplate(() => new Label());
        var view = new CollectionView
        {
            ItemsSource = new[] { "one", "two" },
            Header = "header",
            HeaderTemplate = headerTemplate,
            Footer = "footer",
            FooterTemplate = footerTemplate,
        };

        var group = Assert.Single(HarmonyCollectionViewHandler.BuildGroups(view));

        Assert.Null(group.Group);
        Assert.Equal(new[]
        {
            CollectionViewSlotKind.Header,
            CollectionViewSlotKind.Item,
            CollectionViewSlotKind.Item,
            CollectionViewSlotKind.Footer,
        }, group.Slots.Select(s => s.Kind).ToArray());
        Assert.Equal(new object?[] { "header", "one", "two", "footer" },
            group.Slots.Select(s => s.Context).ToArray());
        Assert.Same(headerTemplate, group.Slots[0].Template);
        Assert.Same(footerTemplate, group.Slots[3].Template);
    }

    [Fact]
    public void BuildGroups_EmptySourceUsesEmptyViewSlot()
    {
        var emptyTemplate = new DataTemplate(() => new Label());
        var view = new CollectionView
        {
            ItemsSource = Array.Empty<string>(),
            EmptyView = "empty",
            EmptyViewTemplate = emptyTemplate,
        };

        var group = Assert.Single(HarmonyCollectionViewHandler.BuildGroups(view));

        var slot = Assert.Single(group.Slots);
        Assert.Equal(CollectionViewSlotKind.Empty, slot.Kind);
        Assert.Equal("empty", slot.Context);
        Assert.Same(emptyTemplate, slot.Template);
    }

    [Fact]
    public void ApplySelection_SingleSelectsOneItem()
    {
        var view = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemsSource = new[] { "one", "two" },
        };

        HarmonyCollectionViewHandler.ApplySelection(view, "two");

        Assert.Equal("two", view.SelectedItem);
        Assert.Equal(new[] { "two" }, view.SelectedItems);
    }

    [Fact]
    public void ApplySelection_MultipleTogglesItems()
    {
        var view = new CollectionView
        {
            SelectionMode = SelectionMode.Multiple,
            ItemsSource = new[] { "one", "two" },
        };

        HarmonyCollectionViewHandler.ApplySelection(view, "one");
        HarmonyCollectionViewHandler.ApplySelection(view, "two");
        HarmonyCollectionViewHandler.ApplySelection(view, "one");

        Assert.Equal(new[] { "two" }, view.SelectedItems);
    }

    [Fact]
    public void ApplySelection_NoneDoesNotChangeSelection()
    {
        var view = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemsSource = new[] { "one" },
        };

        HarmonyCollectionViewHandler.ApplySelection(view, "one");

        Assert.Null(view.SelectedItem);
        Assert.Empty(view.SelectedItems);
    }

    [Fact]
    public void Mapper_ContainsSelectionAndStructureProperties()
    {
        var keys = HarmonyCollectionViewHandler.Mapper.GetKeys().ToArray();

        Assert.Contains(nameof(SelectableItemsView.SelectionMode), keys);
        Assert.Contains(nameof(SelectableItemsView.SelectedItem), keys);
        Assert.Contains(nameof(SelectableItemsView.SelectedItems), keys);
        Assert.Contains(nameof(GroupableItemsView.GroupFooterTemplate), keys);
        Assert.Contains(nameof(StructuredItemsView.Header), keys);
        Assert.Contains(nameof(StructuredItemsView.Footer), keys);
        Assert.Contains(nameof(ItemsView.EmptyView), keys);
    }
}
