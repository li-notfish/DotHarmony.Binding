using System.Text.RegularExpressions;
using Xunit;

namespace HarmonyGestureTests;

public sealed partial class ControlsSampleAppCoverageTests
{
    private static string RepoRoot { get; } = FindRepoRoot();

    private static string AppDirectory => Path.Combine(RepoRoot, "samples", "dotnet", "ControlsSampleApp");

    private static string FactorySource => File.ReadAllText(Path.Combine(
        RepoRoot,
        "src", "HarmonyOS.Maui", "Handlers", "HarmonyHandlerFactory.cs"));

    private static IReadOnlyList<string> XamlFiles => Directory
        .EnumerateFiles(AppDirectory, "*.xaml", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.Ordinal)
        .ToArray();

    private static string AppSource => string.Join(
        Environment.NewLine,
        Directory.EnumerateFiles(AppDirectory, "*.cs", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(File.ReadAllText));

    private static IReadOnlyDictionary<string, string> ChromeAliases { get; } =
        new Dictionary<string, string>
        {
            // NavigationPage, Toolbar, and MenuBar are created by app/page chrome.
            ["NavigationPage"] = "new NavigationPage(new MainPage())",
            ["Toolbar"] = "new NavigationPage(new MainPage())",
            ["MenuBar"] = "ContentPage.MenuBarItems",
        };

    [Fact]
    public void EveryConcreteFactoryDispatchType_IsCoveredBySampleXamlOrChrome()
    {
        var xaml = string.Join(Environment.NewLine, XamlFiles.Select(File.ReadAllText));
        var allSampleSource = xaml + Environment.NewLine + AppSource;
        var concreteTypes = GetConcreteFactoryDispatchTypes().ToArray();

        Assert.NotEmpty(concreteTypes);
        foreach (var type in concreteTypes)
        {
            if (ChromeAliases.TryGetValue(type, out var source))
            {
                Assert.True(
                    allSampleSource.Contains(source, StringComparison.Ordinal),
                    $"{type} must be triggered by sample chrome: {source}");
                continue;
            }

            Assert.True(
                xaml.Contains('<' + type, StringComparison.Ordinal),
                $"{type} must be declared in ControlsSampleApp XAML.");
        }
    }

    [Fact]
    public void FactoryDispatchTypeList_MatchesSupportedSampleSurface()
    {
        string[] expected =
        [
            "AbsoluteLayout",
            "ActivityIndicator",
            "Border",
            "BoxView",
            "Button",
            "CarouselView",
            "CheckBox",
            "CollectionView",
            "ContentPage",
            "ContentPresenter",
            "ContentView",
            "DatePicker",
            "Editor",
            "Entry",
            "FlexLayout",
            "FlyoutPage",
            "Frame",
            "GraphicsView",
            "Grid",
            "HybridWebView",
            "Image",
            "ImageButton",
            "IndicatorView",
            "Label",
            "ListView",
            "MenuBar",
            "MenuBarItem",
            "MenuFlyout",
            "MenuFlyoutItem",
            "MenuFlyoutSeparator",
            "MenuFlyoutSubItem",
            "Picker",
            "ProgressBar",
            "RadioButton",
            "RefreshView",
            "ScrollView",
            "SearchBar",
            "Shell",
            "Slider",
            "StackLayout",
            "Stepper",
            "SwipeItem",
            "SwipeItemView",
            "SwipeView",
            "Switch",
            "SwitchCell",
            "TabbedPage",
            "TableView",
            "TextCell",
            "TimePicker",
            "ViewCell",
            "WebView",
            "ImageCell",
            "EntryCell",
            "NavigationPage",
        ];

        Assert.Equal(
            expected.OrderBy(type => type, StringComparer.Ordinal),
            GetConcreteFactoryDispatchTypes().OrderBy(type => type, StringComparer.Ordinal));
    }

    [Fact]
    public void EveryXamlPage_HasUniqueSampleMarker()
    {
        var markers = new HashSet<string>(StringComparer.Ordinal);

        Assert.NotEmpty(XamlFiles);
        foreach (var path in XamlFiles)
        {
            var marker = MarkerRegex().Match(File.ReadAllText(path));
            Assert.True(marker.Success, $"{path} must contain a CS-* marker.");
            Assert.True(markers.Add(marker.Value), $"Duplicate marker {marker.Value} in {path}.");
        }
    }

    [GeneratedRegex(@"CS-[A-Z0-9-]+")]
    private static partial Regex MarkerRegex();

    [GeneratedRegex(@"Microsoft\.Maui\.Controls\.(?<type>[A-Za-z0-9_\.]+)\s*=>")]
    private static partial Regex FactoryDispatchRegex();

    private static IEnumerable<string> GetConcreteFactoryDispatchTypes()
    {
        foreach (Match match in FactoryDispatchRegex().Matches(FactorySource))
        {
            var type = match.Groups["type"].Value;

            // These are abstract fallback arms. Concrete StackLayout/Grid/Flex/AbsoluteLayout,
            // Cell subclasses, and Shape subclasses cover the same handlers.
            if (type is "Layout" or "Cell" or "Shapes.Shape")
                continue;

            yield return type.Split('.').Last();
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ArkTsBinding.slnx")))
            directory = directory.Parent;

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }
}
