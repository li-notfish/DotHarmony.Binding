using System.Text.Json;

internal sealed class HarmonyPermissionMapping
{
    public HarmonyPermissionMapping(string permission, string when)
    {
        Permission = permission;
        When = when;
    }

    public string Permission { get; }
    public string When { get; }
}

internal sealed class HarmonyPermissionMappingDiagnostic
{
    public HarmonyPermissionMappingDiagnostic(string id, string message, string sourcePath, int line)
    {
        Id = id;
        Message = message;
        SourcePath = sourcePath;
        Line = line;
    }

    public string Id { get; }
    public string Message { get; }
    public string SourcePath { get; }
    public int Line { get; }
}

internal sealed class HarmonyPermissionMappingDocument
{
    public int Version { get; set; }
    public List<MethodMapping> MauiMethods { get; set; } = new();
    public List<PermissionTypeMapping> MauiPermissionTypes { get; set; } = new();
    public List<KnownMethodMapping> KnownPermissionMethods { get; set; } = new();
    public List<AmbiguousPermissionTypeMapping> AmbiguousPermissionTypes { get; set; } = new();

    public static HarmonyPermissionMappingDocument Parse(string json)
    {
        var document = JsonSerializer.Deserialize<HarmonyPermissionMappingDocument>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (document is null || document.Version < 2)
        {
            throw new InvalidOperationException("Unsupported permission mapping document.");
        }

        return document;
    }
}

internal sealed class MethodMapping
{
    public string ContainingType { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string Permission { get; set; } = "";
    public string When { get; set; } = "always";
    public bool Override { get; set; }
}

internal sealed class PermissionTypeMapping
{
    public string TypeName { get; set; } = "";
    public string Permission { get; set; } = "";
    public string When { get; set; } = "always";
    public bool Override { get; set; }
}

internal sealed class KnownMethodMapping
{
    public string ContainingType { get; set; } = "";
    public string MethodName { get; set; } = "";
}

internal sealed class AmbiguousPermissionTypeMapping
{
    public string TypeName { get; set; } = "";
    public List<string> Candidates { get; set; } = new();
}
