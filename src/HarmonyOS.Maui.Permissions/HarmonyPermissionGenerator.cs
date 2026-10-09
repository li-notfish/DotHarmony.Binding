using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace HarmonyOS.Maui.Permissions;

[Generator]
public sealed partial class HarmonyPermissionGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor UnmappedPermissionType = new(
        id: "HMP001",
        title: "Unmapped MAUI permission type",
        messageFormat: "MAUI permission type '{0}' is not mapped to a HarmonyOS permission",
        category: "HarmonyOS.Maui.Permissions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor KnownPermissionMethodWithoutMapping = new(
        id: "HMP002",
        title: "Known MAUI permission API has no HarmonyOS mapping",
        messageFormat: "MAUI permission API '{0}.{1}' is not mapped to a HarmonyOS permission",
        category: "HarmonyOS.Maui.Permissions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ExplicitPermissionNotUsed = new(
        id: "HMP003",
        title: "Explicit HarmonyOS permission is not used",
        messageFormat: "Explicit HarmonyOS permission '{0}' is declared but not used by MAUI code",
        category: "HarmonyOS.Maui.Permissions",
        // Explicit permissions commonly declare capabilities that cannot be inferred
        // from code, so an unused match is informational rather than a warning.
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AmbiguousPermissionMapping = new(
        id: "HMP004",
        title: "Ambiguous HarmonyOS permission mapping",
        messageFormat: "MAUI permission type '{0}' has ambiguous HarmonyOS permission candidates: {1}",
        category: "HarmonyOS.Maui.Permissions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor CustomMappingConflict = new(
        id: "HMP005",
        title: "Custom permission mapping conflict",
        messageFormat: "{0}",
        category: "HarmonyOS.Maui.Permissions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidCustomMapping = new(
        id: "HMP006",
        title: "Invalid custom permission mapping",
        messageFormat: "{0}",
        category: "HarmonyOS.Maui.Permissions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var candidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) =>
                    node is InvocationExpressionSyntax or MemberAccessExpressionSyntax,
                static (syntaxContext, _) => ResolvePermissionCandidate(syntaxContext))
            .Where(candidate => candidate is not null)
            .Select(static (candidate, _) => candidate!)
            .Collect();

        var customMappings = context.AdditionalTextsProvider
            .Where(text =>
                text.Path.EndsWith("harmony-permissions.custom.json", StringComparison.OrdinalIgnoreCase) ||
                text.Path.EndsWith("harmony-permissions.capabilities.json", StringComparison.OrdinalIgnoreCase))
            .Select(static (text, _) => ParseCustomMapping(text))
            .Collect();

        var xamlHandlers = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Select(static (text, _) => ParseXamlEventHandlers(text))
            .Collect();

        var explicitPermissions = context.AdditionalTextsProvider
            .Where(text => text.Path.EndsWith("explicit-permissions.json", StringComparison.OrdinalIgnoreCase))
            .Select(static (text, _) => ParseExplicitPermissions(text))
            .Collect();

        var inputs = candidates
            .Combine(customMappings)
            .Combine(xamlHandlers)
            .Combine(explicitPermissions);

        context.RegisterImplementationSourceOutput(inputs, (productionContext, inputs) =>
        {
            var candidates = inputs.Left.Left.Left;
            var customMappings = inputs.Left.Left.Right;
            var xamlHandlers = inputs.Left.Right;
            var explicitPermissions = inputs.Right;

            var diagnostics = new List<Diagnostic>();
            var map = HarmonyPermissionMap.Default;

            foreach (var custom in customMappings)
            {
                if (custom.Document is null)
                {
                    diagnostics.Add(Diagnostic.Create(
                        InvalidCustomMapping,
                        CreateLocation(custom.Path, 1),
                        custom.Error));
                    continue;
                }

                var mappingDiagnostics = new List<HarmonyPermissionMappingDiagnostic>();
                map = map.Merge(custom.Document, custom.Path, mappingDiagnostics);
                diagnostics.AddRange(mappingDiagnostics.Select(diagnostic => Diagnostic.Create(
                    CustomMappingConflict,
                    CreateLocation(diagnostic.SourcePath, diagnostic.Line),
                    diagnostic.Message)));
            }

            var inferred = new List<ResolvedPermission>();
            var handlerLookup = xamlHandlers
                .SelectMany(handler => handler)
                .GroupBy(handler => handler.HandlerName, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

            foreach (var candidate in candidates)
            {
                var sourcePath = candidate.SourcePath;
                var sourceLine = candidate.Line;
                if (handlerLookup.TryGetValue(candidate.EnclosingMethodName, out var handlers))
                {
                    // Prefer the XAML file whose name matches the candidate's source file
                    // (e.g. MainPage.xaml for MainPage.xaml.cs) to avoid misattributing
                    // handlers that share a name across pages.
                    var candidateFile = Path.GetFileNameWithoutExtension(sourcePath);
                    var handler = handlers.FirstOrDefault(h =>
                        string.Equals(
                            Path.GetFileNameWithoutExtension(h.SourcePath),
                            candidateFile,
                            StringComparison.OrdinalIgnoreCase)) ?? handlers[0];
                    sourcePath = handler.SourcePath;
                    sourceLine = handler.Line;
                }

                if (candidate.IsPermissionTypeRequest)
                {
                    // Ambiguity is checked first: an ambiguous type must not silently
                    // resolve even when a direct mapping entry also exists.
                    var ambiguous = map.GetAmbiguousPermissionTypeCandidates(
                        candidate.PermissionTypeFullName!, candidate.PermissionTypeName!);
                    if (ambiguous is not null && ambiguous.Length > 0)
                    {
                        diagnostics.Add(Diagnostic.Create(
                            AmbiguousPermissionMapping,
                            CreateLocation(sourcePath, sourceLine),
                            candidate.PermissionTypeName,
                            string.Join(", ", ambiguous)));
                        continue;
                    }

                    var mapping = map.ResolvePermissionType(
                        candidate.PermissionTypeFullName!, candidate.PermissionTypeName!);
                    if (mapping is null)
                    {
                        diagnostics.Add(Diagnostic.Create(
                            UnmappedPermissionType,
                            CreateLocation(sourcePath, sourceLine),
                            candidate.PermissionTypeName));
                        continue;
                    }

                    inferred.Add(new ResolvedPermission(
                        mapping.Permission,
                        mapping.When,
                        sourcePath,
                        sourceLine));
                    continue;
                }

                if (candidate.MemberKind is not null)
                {
                    var memberMapping = map.ResolveMember(
                        candidate.ContainingType,
                        candidate.MethodName,
                        candidate.MemberKind);
                    if (memberMapping is null)
                    {
                        continue;
                    }

                    inferred.Add(new ResolvedPermission(
                        memberMapping.Permission,
                        memberMapping.When,
                        sourcePath,
                        sourceLine));
                    continue;
                }

                var methodMapping = map.ResolveMethod(candidate.ContainingType, candidate.MethodName);
                if (methodMapping is null)
                {
                    if (map.IsKnownPermissionMethod(candidate.ContainingType, candidate.MethodName))
                    {
                        diagnostics.Add(Diagnostic.Create(
                            KnownPermissionMethodWithoutMapping,
                            CreateLocation(sourcePath, sourceLine),
                            candidate.ContainingType,
                            candidate.MethodName));
                    }

                    continue;
                }

                inferred.Add(new ResolvedPermission(
                    methodMapping.Permission,
                    methodMapping.When,
                    sourcePath,
                    sourceLine));
            }

            var inferredNames = new HashSet<string>(
                inferred.Select(permission => permission.Permission),
                StringComparer.Ordinal);

            foreach (var explicitPermission in explicitPermissions.SelectMany(permission => permission))
            {
                if (!inferredNames.Contains(explicitPermission.Name))
                {
                    diagnostics.Add(Diagnostic.Create(
                        ExplicitPermissionNotUsed,
                        CreateLocation(explicitPermission.SourcePath, 1),
                        explicitPermission.Name));
                }
            }

            var unique = new SortedSet<string>(
                inferred.Select(permission =>
                {
                    var sourcePath = permission.SourcePath.Replace('\\', '/');
                    return $"{permission.Permission}|{permission.When}|{sourcePath}|{permission.Line}";
                }),
                StringComparer.Ordinal);
            var joined = string.Join(";", unique);

            var source = $$"""
                // <auto-generated/>
                [assembly: HarmonyOS.Maui.Permissions.HarmonyPermissionsAttribute("{{joined}}")]
                """;
            productionContext.AddSource(
                "HarmonyPermissions.g.cs",
                SourceText.From(source, Encoding.UTF8));

            foreach (var diagnostic in diagnostics)
            {
                productionContext.ReportDiagnostic(diagnostic);
            }
        });
    }
}
