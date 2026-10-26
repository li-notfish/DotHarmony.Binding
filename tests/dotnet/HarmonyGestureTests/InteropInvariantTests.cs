// 互操作不变量的机械闸：把「约定」变成构建期会失败的断言。
//
// 背景：napi 无法在桌面测试进程里跑，桥/trampoline 的释放路径无法用运行时测试覆盖，
// 只能退而做源码级结构断言。每条断言都对应一次真实缺陷（见各自注释）。
// 与 FunnelDisciplineTests 同一套做法：扫描源码、报告 file:line。
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace HarmonyGestureTests;

public class InteropInvariantTests
{
    private static string Root => FindRepoRoot();

    private static string InteropFile(string name) =>
        Path.Combine(Root, "src", "HarmonyOS.Interop", name);

    /// <summary>
    /// A1：每个 napi 调用点都必须检查返回值。
    /// 历史缺陷：NodeApi.IsNumber 裸调 napi_typeof，失败时 type 为默认值 0（undefined），
    /// 静默返回 false，调用方把「读取失败」当成「字段不存在」。
    /// </summary>
    [Fact]
    public void EveryNapiCallSiteChecksStatus()
    {
        // 只匹配真正的调用（NativeNodeApi.napi_xxx(）；napi_value/napi_env/napi_valuetype
        // 是类型名，napi_typedarray_type 等是枚举名，不在此断言范围。
        var callSite = new Regex(@"NativeNodeApi\.napi_[a-z0-9_]+\s*\(", RegexOptions.Compiled);

        // 故意忽略 status 的调用：清理型 API，失败了也没有补救动作
        var intentionalIgnores = new HashSet<string>(StringComparer.Ordinal)
        {
            "napi_get_and_clear_last_exception",
        };

        var violations = new List<string>();
        foreach (var file in EnumerateSources(Path.Combine(Root, "src", "HarmonyOS.Interop")))
        {
            // NativeNodeApi.cs 是声明与检查器实现自身；其辅助读取函数**故意**不看 status
            // （它们以「失败返回 null」为契约，见 TryReadString 等）。
            if (Path.GetFileName(file) == "NativeNodeApi.cs")
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].TrimStart();
                if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("*", StringComparison.Ordinal))
                    continue;
                if (!callSite.IsMatch(lines[i]))
                    continue;

                // 语句级：把多行调用拼起来再判断（TSFN 创建之类的调用能跨十几行）
                var statement = new StringBuilder(lines[i]);
                int end = i;
                while (!statement.ToString().Contains(';') && end + 1 < lines.Length && end - i < 40)
                    statement.Append(lines[++end]);

                var text = statement.ToString();
                var called = callSite.Match(text).Groups[0].Value;
                var callee = Regex.Match(called, @"napi_[a-z0-9_]+").Value;

                bool checked_ =
                    intentionalIgnores.Contains(callee) ||
                    text.Contains("ThrowIfFailed", StringComparison.Ordinal) ||
                    text.Contains("napi_status", StringComparison.Ordinal) ||       // 与 napi_ok 显式比较
                    text.Contains("status", StringComparison.OrdinalIgnoreCase);     // 先赋值后判断

                if (!checked_)
                    violations.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}: {callee} — {Compress(text)}");

                i = end;
            }
        }

        Assert.True(violations.Count == 0,
            "有 napi 调用未检查返回值（必须 .ThrowIfFailed() 或显式比较 status）：\n" + string.Join("\n", violations));
    }

    /// <summary>
    /// A2：handle scope 只能经 NapiHandleScope（RAII）使用。
    /// 手写 open/close 配对是「漏掉 finally 就永久泄漏句柄」的经典来源；
    /// 唯一例外是声明处与 RAII 实现自身。
    /// </summary>
    [Fact]
    public void HandleScopeIsOnlyUsedThroughRaii()
    {
        string[] allowed = ["NativeNodeApi.cs", "NapiHandleScope.cs"];
        var offenders = new List<string>();

        foreach (var file in EnumerateSources(Path.Combine(Root, "src")))
        {
            if (allowed.Contains(Path.GetFileName(file)))
                continue;
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("napi_open_handle_scope", StringComparison.Ordinal) ||
                    lines[i].Contains("napi_close_handle_scope", StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "handle scope 必须经 NapiHandleScope.Open 使用（勿手写 open/close 配对）：\n" + string.Join("\n", offenders));
    }

    /// <summary>
    /// A3：NativeNodeApi 里声明的每个 napi_* 入口都必须有真实调用方，
    /// 否则它就是「声明了却没人用」的死面——白名单里的必须写明为什么保留。
    /// </summary>
    [Fact]
    public void EveryDeclaredNapiEntryPointHasACaller()
    {
        var declarations = File.ReadAllText(InteropFile("NativeNodeApi.cs"));
        var declared = Regex.Matches(declarations, @"napi_status\s+(napi_[a-z0-9_]+)\s*\(")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        Assert.True(declared.Count > 20, $"没解析出 napi 声明（{declared.Count} 条），断言本身失效");

        // 保留但不调用：写明理由，避免"死声明"静默沉淀
        var intentionallyUnused = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["napi_get_null"] = "napi_get_undefined 的对偶，错误路径封送 null 时预留",
            ["napi_create_bigint_uint64"] = "只用到 int64 变体；uint64 由 BigInt 支持补全时再启用",
            ["napi_acquire_threadsafe_function"] = "TSFN 由 napi_create_threadsafe_function 的 initial_thread_count 持有，暂不增持",
            ["napi_get_threadsafe_function_thread_id"] = "诊断用，尚未接入日志",
        };

        var callers = new StringBuilder();
        foreach (var dir in new[] { "src", "samples", "tests" })
        {
            var full = Path.Combine(Root, dir);
            if (!Directory.Exists(full))
                continue;
            foreach (var file in EnumerateSources(full))
            {
                var name = Path.GetFileName(file);
                // 声明文件本身不算调用方；本断言文件含白名单字面量，也排除（否则自我命中）
                if (name == "NativeNodeApi.cs" || name == "InteropInvariantTests.cs")
                    continue;
                callers.Append(StripComments(File.ReadAllText(file)));
            }
        }
        var callersText = callers.ToString();

        var orphans = declared
            .Where(n => !callersText.Contains(n, StringComparison.Ordinal))
            .Where(n => !intentionallyUnused.ContainsKey(n))
            .Select(n => $"{n} — 无调用方，请删除声明或在断言的白名单里写明保留理由")
            .ToList();

        Assert.True(orphans.Count == 0, "存在无调用方的 napi 声明：\n" + string.Join("\n", orphans));

        // 白名单也要保持真实：一旦被调用，就必须从白名单移除
        var stale = intentionallyUnused.Keys
            .Where(n => callersText.Contains(n, StringComparison.Ordinal))
            .ToList();
        Assert.True(stale.Count == 0,
            "白名单已过期（这些入口现在有调用方了，请从白名单移除）：\n" + string.Join("\n", stale));
    }

    /// <summary>
    /// A4：两条桥的 State 必须在三条路径（完成/拒绝/取消）上都释放托管资源与
    /// CancellationTokenRegistration，且必须经 <see cref="BridgeSlotRegistry"/> 的 token
    /// 而不是 GCHandle 指针（GCHandle 释放后指针可能被复用 → 迟到回调读到别的对象）。
    /// 历史缺陷：PromiseTaskBridge 的取消路径只 Dispose 了 Registration 没释放句柄，
    /// 两个 trampoline 又只释放句柄没 Dispose Registration。
    /// </summary>
    [Fact]
    public void BridgesReleaseHandleAndRegistrationOnEveryPath()
    {
        var promise = File.ReadAllText(InteropFile("PromiseTaskBridge.cs"));
        var callback = File.ReadAllText(InteropFile("CallbackTaskBridge.cs"));

        foreach (var (name, text) in new[] { ("PromiseTaskBridge.cs", promise), ("CallbackTaskBridge.cs", callback) })
        {
            Assert.Contains("DisposeRegistration", text);
            Assert.Contains("BridgeSlotRegistry", text);

            // 已取消的 token：Register 会同步回调，必须显式补一次取消处理，否则
            // Registration 赋值与 dispose 的顺序会漏掉注册项
            Assert.Contains("IsCancellationRequested", text);

            // 不得再用 GCHandle 承载回调 data：那是 ABA 窗口的来源
            Assert.DoesNotContain("GCHandle.FromIntPtr", text);
            Assert.True(name.Length > 0, name);
        }

        // 取消路径 + 两个 trampoline 的 finally，统一走 Release()
        int promiseReleases = Regex.Matches(promise, @"Release\(\)").Count;
        Assert.True(promiseReleases >= 4,
            $"PromiseTaskBridge 的释放出口只有 {promiseReleases} 处，期望 >= 4（取消 + 两个 finally + 建桥失败回滚）");

        int callbackReleases = Regex.Matches(callback, @"Free\(\)|DisposeRegistration\(\)").Count;
        Assert.True(callbackReleases >= 6,
            $"CallbackTaskBridge 的释放出口只有 {callbackReleases} 处，期望 >= 6（取消/中止 + finally 各两处）");
    }

    /// <summary>
    /// A5：TaskCompletionSource 的续体线程是**有意选择**，不是可以随手"统一"的细节。
    /// 桥上的 TCS 必须内联恢复（续体里要能拿到 napi_env，见 README「已知限制」）；
    /// NativeNode 的 AnimateAsync 反过来必须异步恢复（宿主没装 UI 线程 SynchronizationContext）。
    /// 白名单之外的任何新 TCS 都要先明确属于哪一类。
    /// </summary>
    [Fact]
    public void TaskCompletionSourceUsesAnApprovedContinuationModel()
    {
        var approved = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PromiseTaskBridge.cs"] = "内联恢复：await 续体需继续持有 napi_env",
            ["CallbackTaskBridge.cs"] = "内联恢复：同上",
            ["ArkUINodeBase.cs"] = "异步恢复：回调在原生动画回调内触发，不得内联执行用户续体",
        };

        var offenders = new List<string>();
        foreach (var file in EnumerateSources(Path.Combine(Root, "src")))
        {
            var text = File.ReadAllText(file);
            if (!text.Contains("new TaskCompletionSource", StringComparison.Ordinal))
                continue;
            var name = Path.GetFileName(file);
            if (!approved.ContainsKey(name))
                offenders.Add($"{Path.GetRelativePath(Root, file)} — 新增 TCS 需先确定续体线程模型并登记到本断言白名单");
        }

        Assert.True(offenders.Count == 0, "未登记的 TaskCompletionSource：\n" + string.Join("\n", offenders));
    }

    /// <summary>
    /// A6：异常类型不得携带 IntPtr——napi_value 只在产生它的 handle scope 内有效，
    /// 而异常会跨帧存活（进 Task、被 await 方长期持有），存进去就是悬空句柄。
    /// 历史缺陷：ArkTSException 曾带 internal IntPtr NapiValue。
    /// </summary>
    [Fact]
    public void ExceptionTypesDoNotCarryNativeHandles()
    {
        var offenders = new List<string>();
        var declaration = new Regex(@"(?m)^\s*(?:public|internal|private|sealed|abstract)?[^\n{]*\bclass\s+\w+\s*:\s*[^\n{]*\bException\b");

        foreach (var file in EnumerateSources(Path.Combine(Root, "src")))
        {
            var lines = File.ReadAllLines(file);
            var classLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (declaration.IsMatch(lines[i]))
                    classLine = i;
            }
            if (classLine < 0)
                continue;

            // 到下一个类型声明的 `}` 为止：粗粒度扫该类体的字段
            for (int i = classLine; i < lines.Length; i++)
            {
                var line = lines[i];
                if (i > classLine && line.StartsWith("}", StringComparison.Ordinal))
                    break;
                if (Regex.IsMatch(line, @"\bIntPtr\b") && !line.TrimStart().StartsWith("//", StringComparison.Ordinal) &&
                    !line.Contains("IntPtr.Zero", StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "异常类型携带原生句柄（跨帧即悬空）：\n" + string.Join("\n", offenders));
    }

    /// <summary>去掉注释：注释里提到某个 API 名不构成"有调用方"。</summary>
    private static string StripComments(string text)
    {
        var withoutBlock = Regex.Replace(text, @"/\*.*?\*/", " ", RegexOptions.Singleline);
        return Regex.Replace(withoutBlock, @"//[^\n]*", " ");
    }

    /// <summary>把多行语句压成一行，便于在断言消息里读。</summary>
    private static string Compress(string text)
    {
        var single = Regex.Replace(text, @"\s+", " ").Trim();
        return single.Length > 160 ? single[..160] + "…" : single;
    }

    /// <summary>
    /// A7：JS 回调 data 槽不得再用 GCHandle 指针承载桥状态。
    /// GCHandle 释放后指针可能被后续 Alloc 复用，迟到回调会读到**另一个对象**（ABA）。
    /// 必须走 BridgeSlotRegistry 的单调 token（解析不到即忽略）。
    /// </summary>
    [Fact]
    public void BridgeCallbackDataUsesNonReusableTokens()
    {
        var offenders = new List<string>();
        foreach (var file in EnumerateSources(Path.Combine(Root, "src", "HarmonyOS.Interop")))
        {
            var name = Path.GetFileName(file);
            // 这两处是回调函数的**生命周期**句柄（随 JS 函数 GC 释放），不是桥状态槽，
            // 它们不参与取消竞态，保留 GCHandle 是正确形态。
            if (name is "CallbackTrampolines.cs" or "ThreadSafeFunction.cs" or "NodeApi.cs")
                continue;

            // 注释里解释 GCHandle 的问题不算违规：先剥注释再判断
            var code = StripComments(File.ReadAllText(file));
            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("GCHandle.FromIntPtr", StringComparison.Ordinal))
                    offenders.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}");
            }
        }

        Assert.True(offenders.Count == 0,
            "桥状态的回调 data 必须用 BridgeSlotRegistry token，不得解引用 GCHandle 指针（ABA 窗口）：\n"
            + string.Join("\n", offenders));
    }

    private static IEnumerable<string> EnumerateSources(string root)
    {
        if (!Directory.Exists(root))
            yield break;
        foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                continue;
            yield return file;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir is not null; i++, dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src", "HarmonyOS.Interop")))
                return dir.FullName;
        }
        throw new InvalidOperationException("无法从测试目录定位仓库根（src/HarmonyOS.Interop）");
    }
}
