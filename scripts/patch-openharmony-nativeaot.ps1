param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    [switch]$NoBackup
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Path)) {
    throw "NativeAOT library not found: $Path"
}

$resolvedPath = (Resolve-Path -LiteralPath $Path).Path
$bytes = [System.IO.File]::ReadAllBytes($resolvedPath)

# arm64 NUMASupport entry:
#   stp x29, x30, [sp, #-64]!
#   str x23, [sp, #16]
#   stp x22, x21, [sp, #32]
#   stp x20, x19, [sp, #48]
#   mov x29, sp
#   mov w0, #236             // __NR_get_mempolicy
$signature = [byte[]](
    0xFD, 0x7B, 0xBC, 0xA9,
    0xF7, 0x0B, 0x00, 0xF9,
    0xF6, 0x57, 0x02, 0xA9,
    0xF4, 0x4F, 0x03, 0xA9,
    0xFD, 0x03, 0x00, 0x91,
    0x80, 0x1D, 0x80, 0x52
)

# movn w0, #0 makes syscall return ENOSYS; NUMA initialization then exits.
$replacement = [byte[]](0x00, 0x00, 0x80, 0x12)
$patchOffset = 20

function Find-ByteSignature {
    param(
        [byte[]]$Source,
        [byte[]]$Needle
    )

    $matches = @()
    $last = $Source.Length - $Needle.Length
    for ($i = 0; $i -le $last; $i++) {
        $matched = $true
        for ($j = 0; $j -lt $Needle.Length; $j++) {
            if ($Source[$i + $j] -ne $Needle[$j]) {
                $matched = $false
                break
            }
        }
        if ($matched) {
            $matches += $i
        }
    }
    return $matches
}

$patchedSignature = $signature.Clone()
for ($i = 0; $i -lt $replacement.Length; $i++) {
    $patchedSignature[$patchOffset + $i] = $replacement[$i]
}

$matches = Find-ByteSignature -Source $bytes -Needle $signature
if ($matches.Count -eq 0) {
    $patchedMatches = Find-ByteSignature -Source $bytes -Needle $patchedSignature
    if ($patchedMatches.Count -eq 1) {
        Write-Host "OpenHarmony NativeAOT patch already applied: $resolvedPath"
        exit 0
    }

    throw "OpenHarmony NativeAOT patch signature not found. The runtime layout may have changed; update patch-openharmony-nativeaot.ps1."
}

if ($matches.Count -gt 1) {
    throw "OpenHarmony NativeAOT patch signature matched $($matches.Count) locations; expected exactly one."
}

$offset = $matches[0] + $patchOffset
if (-not $NoBackup) {
    $backupPath = "$resolvedPath.bak"
    Copy-Item -LiteralPath $resolvedPath -Destination $backupPath -Force
    Write-Host "Backup created: $backupPath"
}

for ($i = 0; $i -lt $replacement.Length; $i++) {
    $bytes[$offset + $i] = $replacement[$i]
}

[System.IO.File]::WriteAllBytes($resolvedPath, $bytes)
Write-Host "OpenHarmony NativeAOT patch applied: $resolvedPath"
