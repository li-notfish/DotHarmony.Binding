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

$keystorePassword = $env:HarmonySigningKeystorePassword
$certPassword = $env:HarmonySigningCertPassword
foreach ($item in @(
        @{ Name = "HarmonySigningKeystorePassword"; Value = $keystorePassword },
        @{ Name = "HarmonySigningCertPassword"; Value = $certPassword })) {
    if ([string]::IsNullOrEmpty($item.Value)) {
        throw "缺少环境变量 $($item.Name)"
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
    -compatibleVersion 26 `
    -signCode 1
if ($LASTEXITCODE -ne 0) {
    throw "hap-sign-tool failed with exit code $LASTEXITCODE"
}
Write-Host "=== HAP signed OK: $outputHap"
