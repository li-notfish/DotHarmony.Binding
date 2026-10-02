param(
    [string]$Permissions = "",

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$entries = @()
if (-not [string]::IsNullOrWhiteSpace($Permissions)) {
    foreach ($entry in ($Permissions -split ';')) {
        if ([string]::IsNullOrWhiteSpace($entry)) {
            continue
        }

        $parts = $entry -split '\|'
        $entries += [ordered]@{
            name = $parts[0]
            when = if ($parts.Count -gt 1 -and $parts[1]) { $parts[1] } else { "always" }
        }
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

[ordered]@{
    permissions = $entries
} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $OutputPath
