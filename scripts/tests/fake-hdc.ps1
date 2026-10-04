$ErrorActionPreference = "Stop"
$mode = $env:FAKE_HDC_MODE
if (-not $mode) { throw "FAKE_HDC_MODE is required" }

$target = ""
if ($args.Count -ge 2 -and $args[0] -eq "-t") {
    $target = $args[1]
    $args = @($args[2..($args.Count - 1)])
}

if ($args[0] -eq "list" -and $args[1] -eq "targets") {
    if ($mode -eq "no-device") { Write-Output "[Empty]" }
    else { Write-Output "127.0.0.1:5555" }
    exit 0
}

if ($args[0] -eq "install") {
    Write-Output "install bundle successfully"
    exit 0
}

if ($args[0] -eq "shell") {
    $command = [string]$args[1]
    if ($command -like "pidof *") {
        if ($mode -eq "success") { Write-Output "4242" }
        exit 0
    }
    if ($command -eq "hilog -x") {
        if ($mode -eq "success") {
            Write-Output "01-01 00:00:00.000  4242  4242 I A00000/HarmonyHost: Ability onCreate"
        }
        exit 0
    }
    if ($command -like "aa start *") {
        Write-Output "start ability successfully."
        exit 0
    }
}

exit 0
