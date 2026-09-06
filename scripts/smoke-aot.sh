#!/bin/bash
# 远程冒烟：验证 zig cc + NativeAOT 能否产出 linux-musl-arm64 共享库
set -e
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$HOME/.dotnet:$HOME/zig:$PATH
rm -rf /tmp/smoke && mkdir -p /tmp/smoke && cd /tmp/smoke
cat > smoke.csproj <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>linux-musl-arm64</RuntimeIdentifier>
    <PublishAot>true</PublishAot>
    <NativeLib>Shared</NativeLib>
    <InvariantGlobalization>true</InvariantGlobalization>
    <AssemblyName>app</AssemblyName>
  </PropertyGroup>
</Project>
XML
cat > Program.cs <<'CS'
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
public static class Exports
{
    [UnmanagedCallersOnly(EntryPoint = "smoke_add")]
    public static int Add(int a, int b) => a + b;
}
CS
dotnet publish -c Release -p:CppCompilerAndLinker=zig 2>&1 | tail -5
file bin/Release/net10.0/linux-musl-arm64/native/libapp.so
