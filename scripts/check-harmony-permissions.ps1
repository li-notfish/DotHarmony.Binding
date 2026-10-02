param(
    [Parameter(Mandatory = $true)]
    [string]$PermissionsJson,

    [Parameter(Mandatory = $true)]
    [string]$ModuleJson5,

    [string]$ReportPath = ""
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $PermissionsJson)) {
    throw "Permissions manifest not found: $PermissionsJson"
}
if (-not (Test-Path -LiteralPath $ModuleJson5)) {
    throw "module.json5 not found: $ModuleJson5"
}
if ($ReportPath -and -not (Test-Path -LiteralPath $ReportPath)) {
    throw "Permission report not found: $ReportPath"
}

$manifest = Get-Content -LiteralPath $PermissionsJson -Raw | ConvertFrom-Json
$module = Get-Content -LiteralPath $ModuleJson5 -Raw | ConvertFrom-Json

function Normalize-StringArray {
    param($Value)
    if ($null -eq $Value) {
        return @()
    }
    return @($Value | Where-Object { $null -ne $_ })
}

function Test-SameSet {
    param(
        [object[]]$Left,
        [object[]]$Right
    )

    if ($Left.Count -ne $Right.Count) {
        return $false
    }

    foreach ($item in $Left) {
        if ($Right -notcontains $item) {
            return $false
        }
    }

    return $true
}

$inferred = Normalize-StringArray ($manifest.inferred | ForEach-Object { $_.name }) | Sort-Object -Unique
$explicit = Normalize-StringArray ($manifest.explicit | ForEach-Object { $_.name }) | Sort-Object -Unique
$project = Normalize-StringArray ($manifest.projects | ForEach-Object { $_.name }) | Sort-Object -Unique
$expected = @($inferred + $project + $explicit) | Sort-Object -Unique
$actual = Normalize-StringArray ($manifest.permissions | ForEach-Object { $_.name }) | Sort-Object -Unique
$modulePermissions = Normalize-StringArray ($module.module.requestPermissions | ForEach-Object { $_.name }) | Sort-Object -Unique

if (-not (Test-SameSet $expected $actual)) {
    throw "Permissions manifest is inconsistent: permissions must equal inferred + projects + explicit."
}

if (-not (Test-SameSet $actual $modulePermissions)) {
    $missing = @($actual | Where-Object { $_ -notin $modulePermissions })
    $extra = @($modulePermissions | Where-Object { $_ -notin $actual })
    throw "module.json5 permissions do not match permissions.json. Missing: $($missing -join ', '). Extra: $($extra -join ', ')."
}

Write-Host "Harmony permission check OK: $($actual.Count) permission(s)."
