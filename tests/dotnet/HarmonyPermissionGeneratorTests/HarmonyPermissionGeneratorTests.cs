using System.Collections.Immutable;
using System.Text;
using HarmonyOS.Maui.Permissions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace HarmonyPermissionGeneratorTests;

public sealed class HarmonyPermissionGeneratorTests
{
    private sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public InMemoryAdditionalText(string path, string text)
        {
            Path = path;
            _text = SourceText.From(text, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }

    [Fact]
    public void Generates_location_camera_media_and_vibration_permissions()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel
            {
                public static class Permissions
                {
                    public static void RequestAsync<T>() { }
                }
            }

            namespace Microsoft.Maui.Devices.Sensors
            {
                public static class Geolocation
                {
                    public static void GetLocationAsync() { }
                }
            }

            namespace Microsoft.Maui.Media
            {
                public static class MediaPicker
                {
                    public static void CapturePhotoAsync() { }
                    public static void PickPhotoAsync() { }
                }
            }

            namespace Microsoft.Maui.Devices
            {
                public static class Vibration
                {
                    public static void Vibrate() { }
                }

                namespace Sensors
                {
                    public static class Accelerometer
                    {
                        public static void Start() { }
                    }
                }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
                    Microsoft.Maui.Devices.Sensors.Geolocation.GetLocationAsync();
                    Microsoft.Maui.Media.MediaPicker.CapturePhotoAsync();
                    Microsoft.Maui.Media.MediaPicker.PickPhotoAsync();
                    Microsoft.Maui.Devices.Vibration.Vibrate();
                    Microsoft.Maui.Devices.Sensors.Accelerometer.Start();
                }
            }

            namespace Microsoft.Maui.ApplicationModel
            {
                public sealed class LocationWhenInUse { }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "PermissionGeneratorTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.Devices.Sensors.Geolocation",
                  "methodName": "GetLocationAsync",
                  "permission": "ohos.permission.LOCATION",
                  "when": "inuse"
                },
                {
                  "containingType": "Microsoft.Maui.Media.MediaPicker",
                  "methodName": "CapturePhotoAsync",
                  "permission": "ohos.permission.CAMERA",
                  "when": "inuse"
                },
                {
                  "containingType": "Microsoft.Maui.Media.MediaPicker",
                  "methodName": "PickPhotoAsync",
                  "permission": "ohos.permission.READ_MEDIA",
                  "when": "inuse"
                },
                {
                  "containingType": "Microsoft.Maui.Devices.Vibration",
                  "methodName": "Vibrate",
                  "permission": "ohos.permission.VIBRATE",
                  "when": "always"
                },
                {
                  "containingType": "Microsoft.Maui.Devices.Sensors.Accelerometer",
                  "methodName": "Start",
                  "permission": "ohos.permission.ACCELEROMETER",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(capabilityMap));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.LOCATION|inuse", text);
        Assert.Contains("ohos.permission.CAMERA|inuse", text);
        Assert.Contains("ohos.permission.READ_MEDIA|inuse", text);
        Assert.Contains("ohos.permission.VIBRATE|always", text);
        Assert.Contains("ohos.permission.ACCELEROMETER|inuse", text);
    }

    [Fact]
    public void Warns_when_permission_type_is_not_mapped()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel
            {
                public static class Permissions
                {
                    public static void RequestAsync<T>() { }
                }

                public sealed class UnknownPermission { }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.UnknownPermission>();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "UnmappedPermissionTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id == "HMP001" &&
            diagnostic.GetMessage().Contains("UnknownPermission"));
    }

    [Fact]
    public void Warns_when_known_permission_method_has_no_mapping()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel.Communication
            {
                public static class Contacts
                {
                    public static void GetAllAsync() { }
                }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.ApplicationModel.Communication.Contacts.GetAllAsync();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "KnownPermissionMethodTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id == "HMP002" &&
            diagnostic.GetMessage().Contains("Contacts.GetAllAsync"));
    }

    [Fact]
    public void Warns_when_explicit_permission_is_not_used()
    {
        const string source = """
            static class Program
            {
                static void Main()
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ExplicitPermissionTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var generator = new HarmonyPermissionGenerator();
        var additionalText = new InMemoryAdditionalText(
            @"obj\harmony\explicit-permissions.json",
            """{ "permissions": [ { "name": "ohos.permission.CAMERA", "when": "inuse" } ] }""");
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(additionalText));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id == "HMP003" &&
            diagnostic.GetMessage().Contains("ohos.permission.CAMERA"));
    }

    [Fact]
    public void Warns_when_permission_mapping_is_ambiguous()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel
            {
                public static class Permissions
                {
                    public static void RequestAsync<T>() { }
                }

                public sealed class StorageRead { }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.StorageRead>();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "AmbiguousPermissionTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id == "HMP004" &&
            diagnostic.GetMessage().Contains("StorageRead"));
    }

    [Fact]
    public void Generates_read_pasteboard_for_clipboard_method()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel.DataTransfer
            {
                public interface IClipboard
                {
                    Task<string?> GetTextAsync();
                }

                public static class Clipboard
                {
                    public static IClipboard Default { get; }
                }
            }

            static class Program
            {
                static async Task Main()
                {
                    var text = await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ClipboardMethodPermissionTests",
            new[] { CSharpSyntaxTree.ParseText(source, path: "ClipboardPage.cs") });

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
                  "methodName": "GetTextAsync",
                  "permission": "ohos.permission.READ_PASTEBOARD",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(capabilityMap));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.READ_PASTEBOARD|inuse|ClipboardPage.cs", text);
    }

    [Fact]
    public void Generates_read_pasteboard_for_clipboard_property()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel.DataTransfer
            {
                public interface IClipboard
                {
                    bool HasText { get; }
                }

                public static class Clipboard
                {
                    public static IClipboard Default { get; }
                }
            }

            static class Program
            {
                static void Main()
                {
                    var hasText = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText;
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "ClipboardPropertyPermissionTests",
            new[] { CSharpSyntaxTree.ParseText(source, path: "ClipboardPage.cs") });

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMembers": [
                {
                  "containingType": "Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard",
                  "memberName": "HasText",
                  "memberKind": "property",
                  "permission": "ohos.permission.READ_PASTEBOARD",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(capabilityMap));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.READ_PASTEBOARD|inuse|ClipboardPage.cs", text);
    }

    [Fact]
    public void Applies_capability_map_for_property_access()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel
            {
                public static class DemoApi
                {
                    public static bool NeedsPermission { get; }
                }
            }

            static class Program
            {
                static void Main()
                {
                    var value = Microsoft.Maui.ApplicationModel.DemoApi.NeedsPermission;
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "CapabilityMapTests",
            new[] { CSharpSyntaxTree.ParseText(source, path: "DemoPage.cs") });

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMembers": [
                {
                  "containingType": "Microsoft.Maui.ApplicationModel.DemoApi",
                  "memberName": "NeedsPermission",
                  "memberKind": "property",
                  "permission": "ohos.permission.DEMO",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(capabilityMap));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.DEMO|inuse|DemoPage.cs", text);
    }

    [Fact]
    public void Applies_custom_method_mapping()
    {
        const string source = """
            namespace Microsoft.Maui.ApplicationModel.Communication
            {
                public static class Contacts
                {
                    public static void GetAllAsync() { }
                }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.ApplicationModel.Communication.Contacts.GetAllAsync();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "CustomMappingTests",
            new[] { CSharpSyntaxTree.ParseText(source, path: "Consumer.cs") });

        var customMapping = new InMemoryAdditionalText(
            "harmony-permissions.custom.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.ApplicationModel.Communication.Contacts",
                  "methodName": "GetAllAsync",
                  "permission": "ohos.permission.READ_CONTACTS",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(customMapping));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.READ_CONTACTS|inuse|Consumer.cs", text);
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "HMP002");
    }

    [Fact]
    public void Warns_when_custom_mapping_conflicts_without_override()
    {
        const string source = """
            namespace Microsoft.Maui.Devices
            {
                public static class Vibration
                {
                    public static void Vibrate() { }
                }
            }

            static class Program
            {
                static void Main()
                {
                    Microsoft.Maui.Devices.Vibration.Vibrate();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "CustomMappingConflictTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.Devices.Vibration",
                  "methodName": "Vibrate",
                  "permission": "ohos.permission.VIBRATE",
                  "when": "always"
                }
              ]
            }
            """);

        var customMapping = new InMemoryAdditionalText(
            "harmony-permissions.custom.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.Devices.Vibration",
                  "methodName": "Vibrate",
                  "permission": "ohos.permission.CUSTOM",
                  "when": "always",
                  "override": false
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(capabilityMap, customMapping));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id == "HMP005" &&
            diagnostic.GetMessage().Contains("Vibration.Vibrate"));
        Assert.Contains("ohos.permission.VIBRATE|always", text);
        Assert.DoesNotContain("ohos.permission.CUSTOM|always", text);
    }

    [Fact]
    public void Warns_when_custom_mapping_is_invalid()
    {
        const string source = """
            static class Program
            {
                static void Main()
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "InvalidCustomMappingTests",
            new[] { CSharpSyntaxTree.ParseText(source) });

        var customMapping = new InMemoryAdditionalText(
            "harmony-permissions.custom.json",
            "{ not-json }");

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(customMapping));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "HMP006");
    }

    [Fact]
    public void Uses_xaml_event_handler_as_permission_source()
    {
        const string source = """
            namespace Microsoft.Maui.Devices.Sensors
            {
                public static class Geolocation
                {
                    public static void GetLocationAsync() { }
                }
            }

            static class Program
            {
                static void OnLocationClicked()
                {
                    Microsoft.Maui.Devices.Sensors.Geolocation.GetLocationAsync();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "XamlSourceTests",
            new[] { CSharpSyntaxTree.ParseText(source, path: "MainPage.xaml.cs") });

        var xaml = new InMemoryAdditionalText(
            "MainPage.xaml",
            """
            <?xml version="1.0" encoding="utf-8"?>
            <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
              <Button Clicked="OnLocationClicked" />
            </ContentPage>
            """);

        var capabilityMap = new InMemoryAdditionalText(
            "harmony-permissions.capabilities.json",
            """
            {
              "version": 3,
              "mauiMethods": [
                {
                  "containingType": "Microsoft.Maui.Devices.Sensors.Geolocation",
                  "methodName": "GetLocationAsync",
                  "permission": "ohos.permission.LOCATION",
                  "when": "inuse"
                }
              ]
            }
            """);

        var generator = new HarmonyPermissionGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(xaml, capabilityMap));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out _);

        var generated = updated.SyntaxTrees.Single(tree =>
            tree.FilePath.EndsWith("HarmonyPermissions.g.cs", StringComparison.Ordinal));
        var text = generated.GetText().ToString();

        Assert.Contains("ohos.permission.LOCATION|inuse|MainPage.xaml|3", text);
    }
}
