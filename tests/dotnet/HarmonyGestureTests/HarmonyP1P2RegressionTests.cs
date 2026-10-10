#nullable enable
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Maui.Handlers;
using HarmonyOS.Maui.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Xunit;

namespace HarmonyGestureTests;

public sealed class HarmonyP1P2RegressionTests
{
    [Fact]
    public void MauiHarmonyHost_RunAndRunApplication_RegisterHostCallbacks()
    {
        var original = MauiHarmonyHost.CallbackRegistrar;
        var registrar = new FakeCallbackRegistrar();
        MauiHarmonyHost.CallbackRegistrar = registrar;

        try
        {
            MauiHarmonyHost.Run(() => new ContentPage());
            MauiHarmonyHost.RunApplication(() => new Application());

            Assert.Equal(2, registrar.ThemeCount);
            Assert.Equal(2, registrar.LifecycleCount);
        }
        finally
        {
            MauiHarmonyHost.CallbackRegistrar = original;
        }
    }

    [Fact]
    public void ArkUINodeEventHub_AllowsMultipleHandlersAndPreciseRemove()
    {
        var hub = new ArkUINodeEventHub();
        var first = 0;
        var second = 0;

        void First(ArkUINodeEvent _) => first++;
        void Second(ArkUINodeEvent _) => second++;

        hub.Add(First);
        hub.Add(Second);
        hub.Invoke(default);

        Assert.Equal(1, first);
        Assert.Equal(1, second);

        Assert.True(hub.Remove(First));
        hub.Invoke(default);

        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.False(hub.IsEmpty);

        hub.Clear();
        Assert.True(hub.IsEmpty);
    }

    [Fact]
    public void NativeChildTracker_ReplacesChildrenAndDisposesOnlyRemovedNodes()
    {
        var attached = new List<FakeNode>();
        var removedAll = false;
        var tracker = new NativeChildTracker<FakeNode>(
            node => attached.Add(node),
            () => removedAll = true);

        var retained = new FakeNode();
        var removed = new FakeNode();
        var added = new FakeNode();

        tracker.Add(retained);
        tracker.Add(removed);
        tracker.Replace(new[] { retained, added });

        Assert.True(removedAll);
        Assert.True(removed.Disposed);
        Assert.False(retained.Disposed);
        Assert.False(added.Disposed);
        Assert.Equal(new[] { retained, added }, tracker.Children);
    }

    [Fact]
    public void ButtonTouchRouter_RoutesPressedAndReleased()
    {
        var button = new Button();
        var pressed = 0;
        var released = 0;
        button.Pressed += (_, _) => pressed++;
        button.Released += (_, _) => released++;

        ButtonTouchRouter.Route(button, ArkPointerTouchAction.Pressed);
        ButtonTouchRouter.Route(button, ArkPointerTouchAction.Released);
        ButtonTouchRouter.Route(button, ArkPointerTouchAction.Canceled);

        Assert.Equal(1, pressed);
        Assert.Equal(2, released);
    }

    [Fact]
    public void PickerIndexTranslator_PreservesNoSelectionSentinel()
    {
        Assert.Equal(0, PickerIndexTranslator.ToNative(-1));
        Assert.Equal(3, PickerIndexTranslator.ToNative(2));
        Assert.Equal(-1, PickerIndexTranslator.FromNative(0));
        Assert.Equal(2, PickerIndexTranslator.FromNative(3));
    }

    [Fact]
    public void ImageEventRouter_RoutesStartCompleteAndFailure()
    {
        var image = new FakeImagePart();

        ImageEventRouter.Started(image, image);
        Assert.True(image.IsLoading);
        Assert.Equal(new[] { "Started" }, image.Events);

        ImageEventRouter.Completed(image, image, successful: true);
        Assert.False(image.IsLoading);
        Assert.Equal(new[] { "Started", "Completed" }, image.Events);

        ImageEventRouter.Started(image, image);
        ImageEventRouter.Failed(image, image, new InvalidOperationException("load failed"));
        Assert.False(image.IsLoading);
        Assert.Equal(new[] { "Started", "Completed", "Started", "Failed" }, image.Events);
    }

    [Fact]
    public void FlexSemanticMapper_MapsCoreFlexSemantics()
    {
        Assert.Equal(ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_ROW, FlexSemanticMapper.MapDirection(FlexDirection.Row));
        Assert.Equal(ArkUI_FlexDirection.ARKUI_FLEX_DIRECTION_COLUMN_REVERSE, FlexSemanticMapper.MapDirection(FlexDirection.ColumnReverse));
        Assert.Equal(ArkUI_FlexWrap.ARKUI_FLEX_WRAP_WRAP_REVERSE, FlexSemanticMapper.MapWrap(FlexWrap.Reverse));
        Assert.Equal(ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_EVENLY, FlexSemanticMapper.MapJustify(FlexJustify.SpaceEvenly));
        Assert.Equal(ArkUI_ItemAlignment.ARKUI_ITEM_ALIGNMENT_END, FlexSemanticMapper.MapAlignItems(FlexAlignItems.End));
        Assert.Equal(ArkUI_FlexAlignment.ARKUI_FLEX_ALIGNMENT_SPACE_AROUND, FlexSemanticMapper.MapAlignContent(FlexAlignContent.SpaceAround));
    }

    [Fact]
    public void ControlsSampleApp_UsesValidContentPresenterAndContentViewContent()
    {
        var xaml = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "samples", "dotnet", "ControlsSampleApp", "MainPage.xaml"));

        Assert.Contains("<ContentPresenter>", xaml, StringComparison.Ordinal);
        Assert.Contains("<ContentPresenter.Content>", xaml, StringComparison.Ordinal);
        Assert.Contains("<ContentView.Content>", xaml, StringComparison.Ordinal);
    }

    private sealed class FakeCallbackRegistrar : IHarmonyHostCallbackRegistrar
    {
        public int ThemeCount { get; private set; }
        public int LifecycleCount { get; private set; }

        public void RegisterThemeCallback() => ThemeCount++;

        public void RegisterLifecycleCallback() => LifecycleCount++;
    }

    private sealed class FakeNode : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private sealed class FakeImagePart : IImageSourcePart, IImageSourcePartEvents
    {
        public IImageSource? Source { get; set; }

        public bool IsAnimationPlaying { get; set; }

        public bool IsLoading { get; private set; }

        public List<string> Events { get; } = new();

        public void UpdateIsLoading(bool isLoading) => IsLoading = isLoading;

        public void LoadingStarted() => Events.Add("Started");

        public void LoadingCompleted(bool successful) => Events.Add("Completed");

        public void LoadingFailed(Exception exception) => Events.Add("Failed");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ArkTsBinding.slnx")))
            directory = directory.Parent;

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
