internal sealed class HarmonyPermissionMap
{
    private readonly IDictionary<string, List<HarmonyPermissionMapping>> _methodPermissions;
    private readonly IDictionary<string, List<HarmonyPermissionMapping>> _memberPermissions;
    private readonly IDictionary<string, List<HarmonyPermissionMapping>> _permissionTypePermissions;
    private readonly ISet<string> _knownPermissionMethodKeys;
    private readonly IDictionary<string, string[]> _ambiguousPermissionTypes;

    public static HarmonyPermissionMap Default { get; } = LoadDefault();

    private HarmonyPermissionMap(
        IDictionary<string, List<HarmonyPermissionMapping>> methodPermissions,
        IDictionary<string, List<HarmonyPermissionMapping>> memberPermissions,
        IDictionary<string, List<HarmonyPermissionMapping>> permissionTypePermissions,
        ISet<string> knownPermissionMethodKeys,
        IDictionary<string, string[]> ambiguousPermissionTypes)
    {
        _methodPermissions = methodPermissions;
        _memberPermissions = memberPermissions;
        _permissionTypePermissions = permissionTypePermissions;
        _knownPermissionMethodKeys = knownPermissionMethodKeys;
        _ambiguousPermissionTypes = ambiguousPermissionTypes;
    }

    public IReadOnlyList<HarmonyPermissionMapping> ResolveMethod(string containingType, string methodName)
    {
        return _methodPermissions.TryGetValue($"{containingType}.{methodName}", out var mappings)
            ? mappings
            : Array.Empty<HarmonyPermissionMapping>();
    }

    public IReadOnlyList<HarmonyPermissionMapping> ResolveMember(string containingType, string memberName, string memberKind)
    {
        return _memberPermissions.TryGetValue($"{containingType}.{memberName}.{NormalizeMemberKind(memberKind)}", out var mappings)
            ? mappings
            : Array.Empty<HarmonyPermissionMapping>();
    }

    private static string NormalizeMemberKind(string memberKind)
        => memberKind.ToLowerInvariant();

    // Full names (e.g. from custom mappings) win over the simple-name built-ins so
    // user types that happen to share a simple name are not silently mismatched.
    public IReadOnlyList<HarmonyPermissionMapping> ResolvePermissionType(string fullName, string simpleName)
    {
        return _permissionTypePermissions.TryGetValue(fullName, out var mappings)
            ? mappings
            : _permissionTypePermissions.TryGetValue(simpleName, out mappings)
                ? mappings
                : Array.Empty<HarmonyPermissionMapping>();
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
        var methodPermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(
            _methodPermissions,
            StringComparer.Ordinal);
        var memberPermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(
            _memberPermissions,
            StringComparer.Ordinal);
        var permissionTypePermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(
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
            MergePermissionMapping(
                methodPermissions,
                key,
                new HarmonyPermissionMapping(mapping.Permission, mapping.When),
                mapping.Override,
                sourcePath,
                diagnostics);
        }

        foreach (var mapping in custom.MauiMembers)
        {
            var key = $"{mapping.ContainingType}.{mapping.MemberName}.{NormalizeMemberKind(mapping.MemberKind)}";
            MergePermissionMapping(
                memberPermissions,
                key,
                new HarmonyPermissionMapping(mapping.Permission, mapping.When),
                mapping.Override,
                sourcePath,
                diagnostics);
        }

        foreach (var mapping in custom.MauiPermissionTypes)
        {
            if (permissionTypePermissions.TryGetValue(mapping.TypeName, out var existing) &&
                existing.Any(item => item.Permission == mapping.Permission) &&
                (existing.First(item => item.Permission == mapping.Permission).When != mapping.When) &&
                !mapping.Override)
            {
                diagnostics.Add(new HarmonyPermissionMappingDiagnostic(
                    $"Custom mapping for '{mapping.TypeName}' conflicts with the built-in mapping and must set override to true.",
                    sourcePath,
                    1));
                continue;
            }

            if (!permissionTypePermissions.TryGetValue(mapping.TypeName, out var mappings))
            {
                mappings = new List<HarmonyPermissionMapping>();
                permissionTypePermissions[mapping.TypeName] = mappings;
            }

            if (mappings.Any(item => item.Permission == mapping.Permission))
            {
                mappings.Remove(mappings.First(item => item.Permission == mapping.Permission));
            }

            mappings.Add(new HarmonyPermissionMapping(mapping.Permission, mapping.When));
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
            memberPermissions,
            permissionTypePermissions,
            knownPermissionMethodKeys,
            ambiguousPermissionTypes);
    }

    private static void MergePermissionMapping(
        IDictionary<string, List<HarmonyPermissionMapping>> mappings,
        string key,
        HarmonyPermissionMapping mapping,
        bool overrideExisting,
        string sourcePath,
        List<HarmonyPermissionMappingDiagnostic> diagnostics)
    {
        if (!mappings.TryGetValue(key, out var existing))
        {
            mappings[key] = new List<HarmonyPermissionMapping> { mapping };
            return;
        }

        var duplicate = existing.FirstOrDefault(item => item.Permission == mapping.Permission);
        if (duplicate is null)
        {
            existing.Add(mapping);
            return;
        }

        if (duplicate.When == mapping.When)
            return;

        if (!overrideExisting)
        {
            diagnostics.Add(new HarmonyPermissionMappingDiagnostic(
                $"Custom mapping for '{key}' conflicts with the built-in mapping and must set override to true.",
                sourcePath,
                1));
            return;
        }

        existing.Remove(duplicate);
        existing.Add(mapping);
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

        var methodPermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(StringComparer.Ordinal);
        foreach (var mapping in document.MauiMethods)
        {
            var key = $"{mapping.ContainingType}.{mapping.MethodName}";
            if (!methodPermissions.TryGetValue(key, out var mappings))
                methodPermissions[key] = new List<HarmonyPermissionMapping>();

            if (methodPermissions[key].Any(item => item.Permission == mapping.Permission))
                throw new InvalidOperationException($"Duplicate MAUI method permission mapping: {key}");

            methodPermissions[key].Add(new HarmonyPermissionMapping(mapping.Permission, mapping.When));
        }

        var permissionTypePermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(StringComparer.Ordinal);
        var memberPermissions = new Dictionary<string, List<HarmonyPermissionMapping>>(StringComparer.Ordinal);
        foreach (var mapping in document.MauiMembers)
        {
            var key = $"{mapping.ContainingType}.{mapping.MemberName}.{NormalizeMemberKind(mapping.MemberKind)}";
            if (!memberPermissions.TryGetValue(key, out var mappings))
                memberPermissions[key] = new List<HarmonyPermissionMapping>();

            if (memberPermissions[key].Any(item => item.Permission == mapping.Permission))
                throw new InvalidOperationException($"Duplicate MAUI member permission mapping: {key}");

            memberPermissions[key].Add(new HarmonyPermissionMapping(mapping.Permission, mapping.When));
        }
        foreach (var mapping in document.MauiPermissionTypes)
        {
            if (!permissionTypePermissions.TryGetValue(mapping.TypeName, out var mappings))
            {
                mappings = new List<HarmonyPermissionMapping>();
                permissionTypePermissions.Add(mapping.TypeName, mappings);
            }

            if (mappings.Any(item => item.Permission == mapping.Permission))
                throw new InvalidOperationException($"Duplicate MAUI permission type mapping: {mapping.TypeName}");

            mappings.Add(new HarmonyPermissionMapping(mapping.Permission, mapping.When));
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
            memberPermissions,
            permissionTypePermissions,
            knownPermissionMethodKeys,
            ambiguousPermissionTypes);
    }
}
