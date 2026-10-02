internal sealed class HarmonyPermissionMap
{
    private readonly IDictionary<string, HarmonyPermissionMapping> _methodPermissions;
    private readonly IDictionary<string, HarmonyPermissionMapping> _permissionTypePermissions;
    private readonly ISet<string> _knownPermissionMethodKeys;
    private readonly IDictionary<string, string[]> _ambiguousPermissionTypes;

    public static HarmonyPermissionMap Default { get; } = LoadDefault();

    private HarmonyPermissionMap(
        IDictionary<string, HarmonyPermissionMapping> methodPermissions,
        IDictionary<string, HarmonyPermissionMapping> permissionTypePermissions,
        ISet<string> knownPermissionMethodKeys,
        IDictionary<string, string[]> ambiguousPermissionTypes)
    {
        _methodPermissions = methodPermissions;
        _permissionTypePermissions = permissionTypePermissions;
        _knownPermissionMethodKeys = knownPermissionMethodKeys;
        _ambiguousPermissionTypes = ambiguousPermissionTypes;
    }

    public HarmonyPermissionMapping? ResolveMethod(string containingType, string methodName)
    {
        return _methodPermissions.TryGetValue($"{containingType}.{methodName}", out var mapping)
            ? mapping
            : null;
    }

    // Full names (e.g. from custom mappings) win over the simple-name built-ins so
    // user types that happen to share a simple name are not silently mismatched.
    public HarmonyPermissionMapping? ResolvePermissionType(string fullName, string simpleName)
    {
        return _permissionTypePermissions.TryGetValue(fullName, out var mapping) ||
               _permissionTypePermissions.TryGetValue(simpleName, out mapping)
            ? mapping
            : null;
    }

    public bool IsKnownPermissionMethod(string containingType, string methodName)
    {
        return _knownPermissionMethodKeys.Contains($"{containingType}.{methodName}");
    }

    public string[]? GetAmbiguousPermissionTypeCandidates(string fullName, string simpleName)
    {
        return _ambiguousPermissionTypes.TryGetValue(fullName, out var candidates) ||
               _ambiguousPermissionTypes.TryGetValue(simpleName, out candidates)
            ? candidates
            : null;
    }

    public HarmonyPermissionMap Merge(
        HarmonyPermissionMappingDocument custom,
        string sourcePath,
        List<HarmonyPermissionMappingDiagnostic> diagnostics)
    {
        var methodPermissions = new Dictionary<string, HarmonyPermissionMapping>(
            _methodPermissions,
            StringComparer.Ordinal);
        var permissionTypePermissions = new Dictionary<string, HarmonyPermissionMapping>(
            _permissionTypePermissions,
            StringComparer.Ordinal);
        var knownPermissionMethodKeys = new HashSet<string>(
            _knownPermissionMethodKeys,
            StringComparer.Ordinal);
        var ambiguousPermissionTypes = new Dictionary<string, string[]>(
            _ambiguousPermissionTypes,
            StringComparer.Ordinal);

        foreach (var mapping in custom.MauiMethods)
        {
            var key = $"{mapping.ContainingType}.{mapping.MethodName}";
            if (methodPermissions.TryGetValue(key, out var existing) &&
                (existing.Permission != mapping.Permission || existing.When != mapping.When) &&
                !mapping.Override)
            {
                diagnostics.Add(new HarmonyPermissionMappingDiagnostic(
                    $"Custom mapping for '{key}' conflicts with the built-in mapping and must set override to true.",
                    sourcePath,
                    1));
                continue;
            }

            methodPermissions[key] = new HarmonyPermissionMapping(mapping.Permission, mapping.When);
        }

        foreach (var mapping in custom.MauiPermissionTypes)
        {
            if (permissionTypePermissions.TryGetValue(mapping.TypeName, out var existing) &&
                (existing.Permission != mapping.Permission || existing.When != mapping.When) &&
                !mapping.Override)
            {
                diagnostics.Add(new HarmonyPermissionMappingDiagnostic(
                    $"Custom mapping for '{mapping.TypeName}' conflicts with the built-in mapping and must set override to true.",
                    sourcePath,
                    1));
                continue;
            }

            permissionTypePermissions[mapping.TypeName] = new HarmonyPermissionMapping(mapping.Permission, mapping.When);
        }

        foreach (var mapping in custom.KnownPermissionMethods)
        {
            knownPermissionMethodKeys.Add($"{mapping.ContainingType}.{mapping.MethodName}");
        }

        foreach (var mapping in custom.AmbiguousPermissionTypes)
        {
            ambiguousPermissionTypes[mapping.TypeName] = mapping.Candidates.ToArray();
        }

        return new HarmonyPermissionMap(
            methodPermissions,
            permissionTypePermissions,
            knownPermissionMethodKeys,
            ambiguousPermissionTypes);
    }

    private static HarmonyPermissionMap LoadDefault()
    {
        var assembly = typeof(HarmonyPermissionMap).Assembly;
        using var stream = assembly.GetManifestResourceStream(
            "HarmonyOS.Maui.Permissions.permission-mapping.json");
        if (stream is null)
        {
            throw new InvalidOperationException("Embedded permission mapping not found.");
        }

        var document = HarmonyPermissionMappingDocument.Parse(
            new StreamReader(stream).ReadToEnd());

        var methodPermissions = new Dictionary<string, HarmonyPermissionMapping>(StringComparer.Ordinal);
        foreach (var mapping in document.MauiMethods)
        {
            var key = $"{mapping.ContainingType}.{mapping.MethodName}";
            if (methodPermissions.ContainsKey(key))
            {
                throw new InvalidOperationException($"Duplicate MAUI method permission mapping: {key}");
            }
            methodPermissions.Add(key, new HarmonyPermissionMapping(mapping.Permission, mapping.When));
        }

        var permissionTypePermissions = new Dictionary<string, HarmonyPermissionMapping>(StringComparer.Ordinal);
        foreach (var mapping in document.MauiPermissionTypes)
        {
            if (permissionTypePermissions.ContainsKey(mapping.TypeName))
            {
                throw new InvalidOperationException($"Duplicate MAUI permission type mapping: {mapping.TypeName}");
            }
            permissionTypePermissions.Add(mapping.TypeName, new HarmonyPermissionMapping(mapping.Permission, mapping.When));
        }

        var knownPermissionMethodKeys = new HashSet<string>(
            document.KnownPermissionMethods.Select(mapping => $"{mapping.ContainingType}.{mapping.MethodName}"),
            StringComparer.Ordinal);

        var ambiguousPermissionTypes = document.AmbiguousPermissionTypes.ToDictionary(
            mapping => mapping.TypeName,
            mapping => mapping.Candidates.ToArray(),
            StringComparer.Ordinal);

        return new HarmonyPermissionMap(
            methodPermissions,
            permissionTypePermissions,
            knownPermissionMethodKeys,
            ambiguousPermissionTypes);
    }
}
