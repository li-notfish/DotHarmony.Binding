param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath,

    [string]$ExplicitPermissions = "",

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [string]$ReferencedPermissionsJson = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Reflection.Metadata

function Get-HarmonyPermissionsAttribute {
    param([string]$Path)

    $stream = [System.IO.File]::OpenRead($Path)
    $peReader = [System.Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $reader = [System.Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($peReader)
        $assembly = $reader.GetAssemblyDefinition()

        foreach ($handle in $assembly.GetCustomAttributes()) {
            $attribute = $reader.GetCustomAttribute($handle)
            $namespace = $null
            $name = $null

            if ($attribute.Constructor.Kind -eq [System.Reflection.Metadata.HandleKind]::MemberReference) {
                $member = $reader.GetMemberReference([System.Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
                if ($member.Parent.Kind -ne [System.Reflection.Metadata.HandleKind]::TypeReference) {
                    continue
                }

                $typeReference = $reader.GetTypeReference([System.Reflection.Metadata.TypeReferenceHandle]$member.Parent)
                $namespace = $reader.GetString($typeReference.Namespace)
                $name = $reader.GetString($typeReference.Name)
            }
            elseif ($attribute.Constructor.Kind -eq [System.Reflection.Metadata.HandleKind]::MethodDefinition) {
                $method = $reader.GetMethodDefinition([System.Reflection.Metadata.MethodDefinitionHandle]$attribute.Constructor)
                $declaringType = $method.GetDeclaringType()
                if ($declaringType.IsNil) {
                    continue
                }

                $typeDefinition = $reader.GetTypeDefinition([System.Reflection.Metadata.TypeDefinitionHandle]$declaringType)
                $namespace = $reader.GetString($typeDefinition.Namespace)
                $name = $reader.GetString($typeDefinition.Name)
            }
            else {
                continue
            }

            if ($namespace -ne "HarmonyOS.Maui.Permissions" -or $name -ne "HarmonyPermissionsAttribute") {
                continue
            }

            $blob = $reader.GetBlobReader($attribute.Value)
            $null = $blob.ReadUInt16()
            return $blob.ReadSerializedString()
        }
    }
    finally {
        $peReader.Dispose()
        $stream.Dispose()
    }

    return $null
}

function Convert-PermissionEntry {
    param(
        [string]$Entry,
        [string]$Source = "inferred"
    )

    $parts = $Entry -split '\|'
    $permission = $parts[0]
    $when = if ($parts.Count -gt 1 -and $parts[1]) { $parts[1] } else { "always" }
    $source = if ($parts.Count -gt 2 -and $parts[2]) { $parts[2] } else { "source" }
    $line = if ($parts.Count -gt 3 -and $parts[3]) { [int]$parts[3] } else { 1 }
    [ordered]@{
        name = $permission
        when = $when
        source = $Source
        sourcePath = $source
        sourceLine = $line
    }
}

function Merge-Permissions {
    param([object[]]$Entries)

    $result = @{}
    foreach ($entry in $Entries) {
        if ($result.ContainsKey($entry.name)) {
            if ($entry.when -eq "always" -or $result[$entry.name].when -ne "always") {
                $result[$entry.name] = $entry
            }
        }
        else {
            $result[$entry.name] = $entry
        }
    }
    if ($result.Count -eq 0) {
        return @()
    }

    return @($result.Values | Sort-Object name)
}

$inferredEntries = @()
$attributeValue = Get-HarmonyPermissionsAttribute $AssemblyPath
if (-not [string]::IsNullOrWhiteSpace($attributeValue)) {
    $inferredEntries = @(
        $attributeValue -split ';' |
            ForEach-Object { Convert-PermissionEntry $_ "assembly" }
    )
}

$explicitEntries = @()
if (-not [string]::IsNullOrWhiteSpace($ExplicitPermissions)) {
    $explicitEntries = @(
        $ExplicitPermissions -split ';' |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            ForEach-Object { Convert-PermissionEntry $_ "explicit" }
    )
}

$projectEntries = @()
if (-not [string]::IsNullOrWhiteSpace($ReferencedPermissionsJson)) {
    foreach ($path in ($ReferencedPermissionsJson -split ';')) {
        if (-not (Test-Path -LiteralPath $path)) {
            continue
        }

        $manifest = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
        foreach ($permission in @($manifest.permissions)) {
            $projectEntries += [ordered]@{
                name = $permission.name
                when = $permission.when
                source = "project"
            }
        }
    }
}

$merged = Merge-Permissions ($inferredEntries + $projectEntries + $explicitEntries)

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$result = [ordered]@{
    inferred = $inferredEntries
    projects = $projectEntries
    explicit = $explicitEntries
    permissions = $merged
}

$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath

$reportPath = [System.IO.Path]::ChangeExtension($OutputPath, ".report.md")
$report = @(
    "# Harmony permission report",
    "",
    "## Inferred from assembly",
    ""
)
if ($inferredEntries.Count -eq 0) {
    $report += "- None"
}
else {
    foreach ($entry in $inferredEntries) {
        $report += "- $($entry.name) ($($entry.when)) — $($entry.sourcePath):$($entry.sourceLine)"
    }
}

$report += @(
    "",
    "## Project references",
    ""
)
if ($projectEntries.Count -eq 0) {
    $report += "- None"
}
else {
    foreach ($entry in $projectEntries) {
        $report += "- $($entry.name) ($($entry.when)) — $($entry.source)"
    }
}

$report += @(
    "",
    "## Explicitly declared",
    ""
)
if ($explicitEntries.Count -eq 0) {
    $report += "- None"
}
else {
    foreach ($entry in $explicitEntries) {
        $report += "- $($entry.name) ($($entry.when)) — $($entry.source)"
    }
}

$report += @(
    "",
    "## Final permissions written to module.json5",
    ""
)
if ($merged.Count -eq 0) {
    $report += "- None"
}
else {
    foreach ($entry in $merged) {
        $report += "- $($entry.name) ($($entry.when)) — $($entry.sourcePath):$($entry.sourceLine)"
    }
}

$report += ""
$report | Set-Content -LiteralPath $reportPath
