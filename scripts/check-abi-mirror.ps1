# ABI 函数表镜像一致性校验：native_node.h / native_gesture.h / native_animate.h
# 的结构体成员顺序 vs src/HarmonyOS.Bindings/NativeNode/*.cs 的 delegate* 镜像。
# 手工镜像的结构体在 SDK 升级后容易悄悄漂移——结构体顺序错位即 SIGSEGV，
# 本脚本把这类回归从"设备上炸"提前到 CI/本地可检测。
#
# 用法：pwsh scripts/check-abi-mirror.ps1
#   参数 -SdkBase 可显式指向 OpenHarmony SDK 根（默认自动探测：OHOS_SDK_BASE/OHSDK_HOME/内置路径）
# 退出码：0 全部一致；1 存在漂移。
param([string]$SdkBase = "")

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if (-not $SdkBase) {
    $candidates = @($env:OHOS_SDK_BASE, $env:OHSDK_HOME,
        "D:\Harmony\OpenHarmony\Sdk\26.0.0",
        "C:\Program Files\Huawei\DevEco Studio\sdk\default\openharmony") | Where-Object { $_ }
    foreach ($base in $candidates) {
        if (Test-Path (Join-Path $base "native\sysroot\usr\include\arkui\native_node.h")) {
            $SdkBase = $base
            break
        }
    }
}
if (-not $SdkBase) {
    Write-Error "未找到 OpenHarmony SDK（设置 -SdkBase 或 OHOS_SDK_BASE）"
    exit 1
}
Write-Host "SDK: $SdkBase"

# 从 C 头文件提取指定 typedef struct 的函数指针成员名（按声明顺序）。
# 实现：先定位 "} StructName;" 收尾行，再回溯到最近的 "typedef struct {" 起始——
# 头文件里同名 typedef struct 可能多个（事件/句柄等），必须锚定目标结束行
function Get-HeaderMembers([string]$headerPath, [string]$structName) {
    $lines = Get-Content $headerPath
    $members = [System.Collections.Generic.List[string]]::new()
    $endIdx = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "}\s*$structName\s*;") { $endIdx = $i; break }
    }
    if ($endIdx -lt 0) { return @() }
    $startIdx = -1
    for ($i = $endIdx; $i -ge 0; $i--) {
        if ($lines[$i] -match "typedef\s+struct\s*\{") { $startIdx = $i; break }
    }
    if ($startIdx -lt 0) { return @() }
    for ($i = $startIdx; $i -lt $endIdx; $i++) {
        # 成员行：4 空格起始且带 (*name)(——.* 必须懒惰，否则单行内嵌套回调形参
        # （如 void (*registerNodeEventReceiver)(void (*eventReceiver)(...))）会抢匹配
        if ($lines[$i] -match "^    \S.*?\(\s*\*\s*(\w+)\s*\)") {
            $members.Add($Matches[1])
        }
    }
    return $members.ToArray()
}

# 从 C# 镜像提取结构体的 delegate* 成员名（按声明顺序）
function Get-MirrorMembers([string]$csPath, [string]$structName) {
    $lines = Get-Content $csPath
    $members = [System.Collections.Generic.List[string]]::new()
    $inStruct = $false
    foreach ($line in $lines) {
        if ($line -match "struct\s+$structName") { $inStruct = $true; continue }
        if ($inStruct) {
            # .+ 贪婪：签名内部允许嵌套 delegate* unmanaged<..>（如事件接收器回指签名）
            if ($line -match 'delegate\*\s+unmanaged<.+>\s+(\w+)\s*;') {
                $members.Add($Matches[1])
            } elseif ($line -match "^\}") {
                break
            }
        }
    }
    return $members.ToArray()
}

$structMatches = @(
    @{ Header = "native_node.h"; Struct = "ArkUI_NativeNodeAPI_1"; Mirror = "src\HarmonyOS.Bindings\NativeNode\ArkUINativeApi.cs" }
    @{ Header = "native_gesture.h"; Struct = "ArkUI_NativeGestureAPI_1"; Mirror = "src\HarmonyOS.Bindings\NativeNode\ArkUIGestureApi.cs" }
    @{ Header = "native_animate.h"; Struct = "ArkUI_NativeAnimateAPI_1"; Mirror = "src\HarmonyOS.Bindings\NativeNode\ArkUIAnimateApi.cs" }
)

$failures = 0
foreach ($m in $structMatches) {
    $headerPath = Join-Path $SdkBase "native\sysroot\usr\include\arkui\$($m.Header)"
    $mirrorPath = Join-Path $repoRoot $m.Mirror
    $headers = Get-HeaderMembers $headerPath $m.Struct
    $mirror = Get-MirrorMembers $mirrorPath $m.Struct

    if ($headers.Count -eq 0) {
        Write-Warning "$($m.Struct): 未从 $($m.Header) 解析到成员"
        $failures++
        continue
    }
    # 镜像是前缀集（允许的截断：SDK 新增的尾部成员未包装时不破坏 ABI）
    if ($headers.Count -gt 0 -and $mirror.Count -gt $headers.Count) {
        Write-Error "$($m.Struct): 镜像成员数 ($($mirror.Count)) 超过原生 ($($headers.Count))"
        $failures++
        continue
    }
    $mismatch = $false
    for ($i = 0; $i -lt $mirror.Count; $i++) {
        # 镜像用 PascalCase、原生用小驼峰：按序对比时大小写不敏感
        if (-not $mirror[$i].Equals($headers[$i], [StringComparison]::OrdinalIgnoreCase)) {
            Write-Error "$($m.Struct)[$i]: mirror='$($mirror[$i])' != header='$($headers[$i])'"
            $mismatch = $true
            $failures++
            break
        }
    }
    if (-not $mismatch) {
        Write-Host "OK  $($m.Struct): $($mirror.Count)/$($headers.Count) 成员一致"
    }
}

exit ($failures -gt 0 ? 1 : 0)
