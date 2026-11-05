using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace HarmonyOS.Maui.Tasks;

/// <summary>
/// OpenHarmony 云设备屏蔽 get_mempolicy：对 linux-musl-arm64 NativeAOT 产物的
/// NUMASupport 探测入口做 ENOSYS 补丁。与 scripts/patch-openharmony-nativeaot.ps1
/// 的签名常量保持一致；不生成 .bak；补丁后立即自检。
/// </summary>
public sealed class PatchOpenHarmonyNativeAot : Task
{
    // arm64 NUMASupport entry:
    //   stp x29, x30, [sp, #-64]!
    //   str x23, [sp, #16]
    //   stp x22, x21, [sp, #32]
    //   stp x20, x19, [sp, #48]
    //   mov x29, sp
    //   mov w0, #236             // __NR_get_mempolicy
    private static readonly byte[] Signature =
    {
        0xFD, 0x7B, 0xBC, 0xA9,
        0xF7, 0x0B, 0x00, 0xF9,
        0xF6, 0x57, 0x02, 0xA9,
        0xF4, 0x4F, 0x03, 0xA9,
        0xFD, 0x03, 0x00, 0x91,
        0x80, 0x1D, 0x80, 0x52,
    };

    private const int PatchOffset = 20;

    // movn w0, #0 makes syscall return ENOSYS; NUMA initialization then exits.
    private static readonly byte[] Replacement = { 0x00, 0x00, 0x80, 0x12 };

    /// <summary>待检查/补丁的 .so 文件（linux-musl-arm64 的 app.so）。</summary>
    [Required]
    public string LibraryFile { get; set; } = string.Empty;

    /// <summary>增量戳文件；成功补丁/已补丁后写入，供 target 的 Outputs 跳过。</summary>
    [Required]
    public string StampFile { get; set; } = string.Empty;

    /// <summary>true 时只验签不写文件（宿主 staging 复核用）。</summary>
    public bool CheckOnly { get; set; }

    public override bool Execute()
    {
        if (!File.Exists(LibraryFile))
        {
            Log.LogError($"NativeAOT library not found: {LibraryFile}");
            return false;
        }

        var bytes = File.ReadAllBytes(LibraryFile);
        var patchedSignature = (byte[])Signature.Clone();
        Array.Copy(Replacement, 0, patchedSignature, PatchOffset, Replacement.Length);

        var unpatchedMatches = FindAll(bytes, Signature);
        var patchedMatches = FindAll(bytes, patchedSignature);

        if (CheckOnly)
        {
            if (patchedMatches.Count != 1)
            {
                Log.LogError(
                    $"OpenHarmony NativeAOT patch verification failed for '{LibraryFile}': "
                    + $"expected 1 patched signature, found {patchedMatches.Count}.");
                return false;
            }

            if (unpatchedMatches.Count != 0)
            {
                Log.LogError(
                    $"OpenHarmony NativeAOT patch verification failed for '{LibraryFile}': "
                    + $"found {unpatchedMatches.Count} unpatched signature(s).");
                return false;
            }

            Log.LogMessage(MessageImportance.High, $"OpenHarmony NativeAOT patch verified: {LibraryFile}");
            WriteStamp();
            return true;
        }

        if (unpatchedMatches.Count == 0)
        {
            if (patchedMatches.Count == 1)
            {
                Log.LogMessage(MessageImportance.High, $"OpenHarmony NativeAOT patch already applied: {LibraryFile}");
                WriteStamp();
                return true;
            }

            Log.LogError(
                $"OpenHarmony NativeAOT patch signature not found in '{LibraryFile}'. "
                + "The runtime layout may have changed; update the signature constants.");
            return false;
        }

        if (unpatchedMatches.Count > 1)
        {
            Log.LogError(
                $"OpenHarmony NativeAOT patch signature matched {unpatchedMatches.Count} locations in '{LibraryFile}'; "
                + "expected exactly one.");
            return false;
        }

        var offset = unpatchedMatches[0] + PatchOffset;
        Array.Copy(Replacement, 0, bytes, offset, Replacement.Length);

        // 原子落盘：失败时保留原文件，不产生 .bak，也不留下半截产物。
        var tempFile = LibraryFile + ".patching.tmp";
        try
        {
            File.WriteAllBytes(tempFile, bytes);
            File.Copy(tempFile, LibraryFile, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        // 补丁后立即验签：0 次原始签名、1 次补丁签名。
        var verifyBytes = File.ReadAllBytes(LibraryFile);
        if (FindAll(verifyBytes, Signature).Count != 0
            || FindAll(verifyBytes, patchedSignature).Count != 1)
        {
            Log.LogError($"OpenHarmony NativeAOT patch verification failed after writing '{LibraryFile}'.");
            return false;
        }

        Log.LogMessage(MessageImportance.High, $"OpenHarmony NativeAOT patch applied: {LibraryFile}");
        WriteStamp();
        return true;
    }

    private void WriteStamp()
    {
        var stampDir = Path.GetDirectoryName(Path.GetFullPath(StampFile));
        if (!string.IsNullOrEmpty(stampDir))
        {
            Directory.CreateDirectory(stampDir);
        }

        File.WriteAllText(StampFile, DateTimeOffset.UtcNow.ToString("O"));
    }

    private static List<int> FindAll(byte[] source, byte[] needle)
    {
        var matches = new List<int>();
        var last = source.Length - needle.Length;
        for (var i = 0; i <= last; i++)
        {
            var matched = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (source[i + j] != needle[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                matches.Add(i);
            }
        }

        return matches;
    }
}
