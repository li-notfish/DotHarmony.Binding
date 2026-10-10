$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptDir
$repoRoot = Split-Path -Parent $scriptsDir
$stage = Join-Path $scriptsDir "stage-host.ps1"
$source = Join-Path $repoRoot "samples\HarmonyHost"
$pwsh = (Get-Process -Id $PID).Path
$work = Join-Path ([IO.Path]::GetTempPath()) ("dot-harmony-stage-" + [Guid]::NewGuid().ToString("N"))
$template = Join-Path $work "HarmonyHost"
$destination = Join-Path $work "staged"
New-Item -ItemType Directory -Path $work | Out-Null
Copy-Item -LiteralPath $source -Destination $work -Recurse

function Invoke-Stage {
    $script:StageOutput = & $pwsh -NoProfile -File $stage -Template $template -Destination $destination `
        -BundleId com.example.stage -AppTitle "Stage Test" 2>&1 | Out-String
    $script:StageExit = $LASTEXITCODE
}

try {
    Invoke-Stage
    if ($StageExit -ne 0) { throw "initial staging failed: $StageOutput" }
    $firstStamp = (Get-Content (Join-Path $destination ".stage-stamp") -Raw).Trim()

    $entryAbility = Join-Path $template "entry\src\main\ets\entryability\EntryAbility.ets"
    Add-Content -LiteralPath $entryAbility -Value "`n// content-only stamp regression"
    (Get-Item $entryAbility).LastWriteTime = (Get-Date).AddDays(-3)
    Invoke-Stage
    if ($StageExit -ne 0) { throw "content-change staging failed: $StageOutput" }
    $secondStamp = (Get-Content (Join-Path $destination ".stage-stamp") -Raw).Trim()
    if ($firstStamp -eq $secondStamp) { throw "content-only template change did not invalidate the stamp" }
    Write-Output "PASS content stamp ignores mtime"

    $appJson = Join-Path $template "AppScope\app.json5"
    $corrupted = (Get-Content -LiteralPath $appJson -Raw) -replace
        '"bundleName"\s*:\s*"[^"]*"', '"unexpected": "value"'
    Set-Content -LiteralPath $appJson -Value $corrupted
    Invoke-Stage
    if ($StageExit -eq 0 -or $StageOutput -notmatch "缺少 bundleName") {
        throw "unknown JSON5 structure did not fail clearly: exit=$StageExit output=$StageOutput"
    }
    Write-Output "PASS unknown JSON5 structure fails"
} finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
}
