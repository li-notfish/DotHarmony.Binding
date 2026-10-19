# Signs an unsigned Harmony HAP with the OpenHarmony hap-sign-tool.
# Passwords are read from environment variables so they are not passed on the
# MSBuild command line.
param(
    [Parameter(Mandatory = $true)][string]$HostDir,
    [Parameter(Mandatory = $true)][string]$SigningKeystore,
    [Parameter(Mandatory = $true)][string]$SigningCertAlias,
    [Parameter(Mandatory = $true)][string]$SigningCertPath,
    [Parameter(Mandatory = $true)][string]$SigningProfile
)
$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "lib\sdk.ps1")

$keystorePassword = $env:HarmonySigningKeystorePassword
$certPassword = $env:HarmonySigningCertPassword
# 可选：从文件读取口令，避免把明文放进父进程环境变量。注意 hap-sign-tool 只接受
# 命令行 argv（不支持 stdin），子进程命令行仍可见；此处只减少泄露面，不能彻底隐藏。
function Read-SecretFile([string]$Path, [string]$Label) {
    if ([string]::IsNullOrEmpty($Path)) { return $null }
    if (-not (Test-Path -LiteralPath $Path)) { throw "$Label 文件不存在：$Path" }
    return (Get-Content -LiteralPath $Path -Raw).Trim()
}
if ([string]::IsNullOrEmpty($keystorePassword)) {
    $keystorePassword = Read-SecretFile $env:HarmonySigningKeystorePasswordFile "Keystore 口令"
}
if ([string]::IsNullOrEmpty($certPassword)) {
    $certPassword = Read-SecretFile $env:HarmonySigningCertPasswordFile "证书口令"
}
foreach ($item in @(
        @{ Name = "HarmonySigningKeystorePassword"; Value = $keystorePassword },
        @{ Name = "HarmonySigningCertPassword"; Value = $certPassword })) {
    if ([string]::IsNullOrEmpty($item.Value)) {
        throw "缺少环境变量 $($item.Name)（或其对应的 *_FILE）"
    }
}

$deveco = $env:DEVECO_HOME
if ([string]::IsNullOrEmpty($deveco)) {
    $candidates = @(
        "D:\Program Files\Huawei\DevEco Studio",
        "C:\Program Files\Huawei\DevEco Studio")
    $deveco = $candidates | Where-Object {
        Test-Path (Join-Path $_ "sdk\default\openharmony\toolchains\lib\hap-sign-tool.jar")
    } | Select-Object -First 1
}
if ([string]::IsNullOrEmpty($deveco)) {
    throw "未找到 DevEco Studio。请设置 DEVECO_HOME。"
}

$signTool = Join-Path $deveco "sdk\default\openharmony\toolchains\lib\hap-sign-tool.jar"
if (-not (Test-Path $signTool)) {
    throw "未找到 hap-sign-tool.jar：$signTool"
}
$java = Join-Path $deveco "jbr\bin\java.exe"
if (-not (Test-Path $java)) { $java = "java" }

$inputHap = Join-Path $HostDir "entry\build\default\outputs\default\entry-default-unsigned.hap"
$outputHap = Join-Path $HostDir "entry\build\default\outputs\default\entry-default-signed.hap"
if (-not (Test-Path $inputHap)) {
    throw "未找到 unsigned HAP：$inputHap"
}

Write-Host "=== signing HAP: $outputHap"
$compatibleVersion = Get-HarmonyCompatibleVersion
Write-Host "compatibleVersion: $compatibleVersion"
& $java -jar $signTool sign-app `
    -mode localSign `
    -keyAlias $SigningCertAlias `
    -keyPwd $certPassword `
    -appCertFile $SigningCertPath `
    -profileFile $SigningProfile `
    -inFile $inputHap `
    -signAlg SHA256withECDSA `
    -keystoreFile $SigningKeystore `
    -keystorePwd $keystorePassword `
    -outFile $outputHap `
    -compatibleVersion $compatibleVersion `
    -signCode 1
if ($LASTEXITCODE -ne 0) {
    throw "hap-sign-tool failed with exit code $LASTEXITCODE"
}
Write-Host "=== HAP signed OK: $outputHap"
