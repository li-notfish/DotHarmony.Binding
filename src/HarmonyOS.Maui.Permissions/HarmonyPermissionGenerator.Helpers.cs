using System.Collections.Immutable;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HarmonyOS.Maui.Permissions;

public sealed partial class HarmonyPermissionGenerator
{
    private sealed record PermissionInvocationCandidate(
        string ContainingType,
        string MethodName,
        string? PermissionTypeName,
        bool IsPermissionTypeRequest,
        string SourcePath,
        int Line,
        string EnclosingMethodName);

    private sealed record CustomMappingParseResult(
        string Path,
        HarmonyPermissionMappingDocument? Document,
        string? Error);

    private sealed record XamlEventHandler(
        string HandlerName,
        string SourcePath,
        int Line);

    private sealed record ExplicitPermission(
        string Name,
        string SourcePath,
        int Line);

    private sealed record ResolvedPermission(
        string Permission,
        string When,
        string SourcePath,
        int Line);

    private static PermissionInvocationCandidate? ResolvePermissionCandidate(GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        var containingType = method.ContainingType?.ToDisplayString() ?? string.Empty;
        if (string.IsNullOrEmpty(containingType))
        {
            return null;
        }

        var isPermissionTypeRequest =
            containingType == "Microsoft.Maui.ApplicationModel.Permissions" &&
            method.Name is ("RequestAsync" or "CheckStatusAsync" or "ShouldShowRationale" or "EnsureDeclared") &&
            method.TypeArguments.Length == 1;

        if (!isPermissionTypeRequest &&
            !containingType.StartsWith("Microsoft.Maui", StringComparison.Ordinal))
        {
            return null;
        }

        var permissionTypeName = isPermissionTypeRequest
            ? ((INamedTypeSymbol)method.TypeArguments[0]).Name
            : null;

        var enclosingMethod = context.Node.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault()
            ?.Identifier.ValueText ?? string.Empty;

        var location = invocation.GetLocation();
        var lineSpan = location.GetLineSpan();
        var sourcePath = string.IsNullOrEmpty(context.Node.SyntaxTree.FilePath)
            ? "generated"
            : context.Node.SyntaxTree.FilePath;

        return new PermissionInvocationCandidate(
            containingType,
            method.Name,
            permissionTypeName,
            isPermissionTypeRequest,
            sourcePath,
            lineSpan.StartLinePosition.Line + 1,
            enclosingMethod);
    }

    private static CustomMappingParseResult ParseCustomMapping(AdditionalText text)
    {
        try
        {
            var document = HarmonyPermissionMappingDocument.Parse(text.GetText().ToString());
            return new CustomMappingParseResult(text.Path, document, null);
        }
        catch (Exception exception)
        {
            return new CustomMappingParseResult(text.Path, null, exception.Message);
        }
    }

    private static ImmutableArray<XamlEventHandler> ParseXamlEventHandlers(AdditionalText text)
    {
        var result = ImmutableArray.CreateBuilder<XamlEventHandler>();
        try
        {
            var document = XDocument.Parse(
                text.GetText().ToString(),
                LoadOptions.SetLineInfo);

            foreach (var attribute in document.Descendants().SelectMany(element => element.Attributes()))
            {
                if (!IsEventHandlerName(attribute.Name.LocalName))
                {
                    continue;
                }

                var handlerName = attribute.Value.Trim();
                if (string.IsNullOrEmpty(handlerName))
                {
                    continue;
                }

                var lineInfo = (IXmlLineInfo)attribute;
                result.Add(new XamlEventHandler(handlerName, text.Path, lineInfo.LineNumber));
            }
        }
        catch
        {
            // Invalid XAML is reported by the XAML compiler; permission inference should not fail the build.
        }

        return result.ToImmutable();
    }

    private static bool IsEventHandlerName(string localName)
    {
        return localName.EndsWith("Clicked", StringComparison.Ordinal) ||
               localName.EndsWith("Tapped", StringComparison.Ordinal) ||
               localName.EndsWith("Pressed", StringComparison.Ordinal) ||
               localName.EndsWith("Released", StringComparison.Ordinal) ||
               localName.EndsWith("TextChanged", StringComparison.Ordinal) ||
               localName.EndsWith("Focused", StringComparison.Ordinal) ||
               localName.EndsWith("Unfocused", StringComparison.Ordinal) ||
               localName.EndsWith("SelectionChanged", StringComparison.Ordinal) ||
               localName.EndsWith("Completed", StringComparison.Ordinal) ||
               localName.EndsWith("Appearing", StringComparison.Ordinal) ||
               localName.EndsWith("Disappearing", StringComparison.Ordinal) ||
               localName.EndsWith("Scrolled", StringComparison.Ordinal) ||
               localName.EndsWith("Swiped", StringComparison.Ordinal) ||
               localName.EndsWith("Panned", StringComparison.Ordinal) ||
               localName.EndsWith("Pinched", StringComparison.Ordinal) ||
               localName.EndsWith("Dragged", StringComparison.Ordinal) ||
               localName.EndsWith("Dropped", StringComparison.Ordinal);
    }

    private static ImmutableArray<ExplicitPermission> ParseExplicitPermissions(AdditionalText text)
    {
        var result = ImmutableArray.CreateBuilder<ExplicitPermission>();
        try
        {
            using var document = JsonDocument.Parse(text.GetText().ToString());
            if (!document.RootElement.TryGetProperty("permissions", out var permissions))
            {
                return result.ToImmutable();
            }

            foreach (var permission in permissions.EnumerateArray())
            {
                if (permission.TryGetProperty("name", out var name))
                {
                    result.Add(new ExplicitPermission(name.GetString() ?? "", text.Path, 1));
                }
            }
        }
        catch
        {
            // Invalid explicit permission JSON is reported by the MSBuild writer/checker.
        }

        return result.ToImmutable();
    }

    private static Location CreateLocation(string path, int line)
    {
        if (string.IsNullOrEmpty(path))
        {
            return Location.None;
        }

        var position = new LinePosition(Math.Max(0, line - 1), 0);
        return Location.Create(
            path,
            new TextSpan(0, 0),
            new LinePositionSpan(position, position));
    }
}
