# 自动定位远程构建机：先试 SSH 配置里的 IP，连不上就扫描局域网。
# 找到后把新 IP 写回 ~/.ssh/config 的 Alias HostName，并把 IP 输出到 stdout。
# 用法: pwsh -NoProfile -File scripts/resolve-remote.ps1
# 自定义（环境变量优先，均无硬编码个人机）：
#   REMOTE_SSH_ALIAS / REMOTE_SSH_USER / REMOTE_EXPECTED_HOSTNAME / REMOTE_SUBNET
param(
    [string]$Alias = ($env:REMOTE_SSH_ALIAS ?? 'wsl_remote'),
    [string]$User = ($env:REMOTE_SSH_USER ?? $env:USERNAME),
    [string]$ExpectedHostname = ($env:REMOTE_EXPECTED_HOSTNAME ?? '').Trim(),
    [string]$Subnet = ($env:REMOTE_SUBNET ?? '192.168.1')
)

$ErrorActionPreference = "Stop"
$sshConfig = "$env:USERPROFILE\.ssh\config"

function Test-SshHost {
    param([string]$Ip)
    # BatchMode 只用密钥认证；直连 IP 不走 ssh config 别名，需显式指定密钥（可用 REMOTE_SSH_KEY 覆盖默认）
    $keyPath = if ($env:REMOTE_SSH_KEY) { $env:REMOTE_SSH_KEY } else { "$env:USERPROFILE\.ssh\id_ed25519" }
    $out = ssh -o BatchMode=yes -o ConnectTimeout=5 -o StrictHostKeyChecking=accept-new `
        -i $keyPath "$User@$Ip" "cat /etc/hostname" 2>$null
    # 未指定意向主机名时，任一"密钥可登录的 SSH 主机"就视命中
    if ([string]::IsNullOrWhiteSpace($ExpectedHostname)) { return $LASTEXITCODE -eq 0 }
    return ($out -eq $ExpectedHostname)
}

# 从 ssh config 读当前 HostName
$currentIp = $null
if (Test-Path $sshConfig) {
    $lines = Get-Content $sshConfig
    $inBlock = $false
    foreach ($line in $lines) {
        if ($line -match "^\s*Host\s+$Alias\s*$") { $inBlock = $true; continue }
        if ($inBlock -and $line -match "^\s*Host\s") { break }
        if ($inBlock -and $line -match "^\s*HostName\s+(.+)$") { $currentIp = $Matches[1].Trim(); break }
    }
}

if ($currentIp -and (Test-SshHost $currentIp)) {
    Write-Output $currentIp
    exit 0
}

Write-Host "当前配置 IP ($currentIp) 不可用，扫描局域网 $Subnet.1-$Subnet.254 ..." -ForegroundColor Yellow

# 并行探测 22 端口
$tasks = @()
foreach ($i in 1..254) {
    $ip = "$Subnet.$i"
    $client = [System.Net.Sockets.TcpClient]::new()
    $tasks += @{
        Ip = $ip; Client = $client;
        Task = $client.ConnectAsync($ip, 22)
    }
}
Start-Sleep -Seconds 3
$open = foreach ($t in $tasks) {
    if ($t.Task.IsCompleted -and -not $t.Task.IsFaulted -and $t.Client.Connected) { $t.Ip }
    $t.Client.Dispose()
}
$open = @($open)
Write-Host "22 端口开放: $($open -join ', ')"

foreach ($ip in $open) {
    if (Test-SshHost $ip) {
        Write-Host "找到构建机: $ip，更新 $sshConfig" -ForegroundColor Green
        # 只改 Alias 块内的 HostName 行，不动其他 Host 条目
        $lines = Get-Content $sshConfig
        $inBlock = $false
        for ($n = 0; $n -lt $lines.Count; $n++) {
            if ($lines[$n] -match "^\s*Host\s+$Alias\s*$") { $inBlock = $true; continue }
            if ($inBlock -and $lines[$n] -match "^\s*Host\s") { break }
            if ($inBlock -and $lines[$n] -match "^(\s*HostName\s+).*$") {
                $lines[$n] = $lines[$n] -replace "^(\s*HostName\s+).*$", "`$1$ip"
                break
            }
        }
        Set-Content $sshConfig -Value $lines -Encoding ascii
        Write-Output $ip
        exit 0
    }
}

throw "局域网内未找到可密钥登录且主机名为 $ExpectedHostname 的构建机"
