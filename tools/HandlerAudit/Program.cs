// HandlerAudit：Handler PropertyMapper 键 vs MAUI 控件接口继承链的一致性审计。
// 用法：
//   dotnet run --project tools/HandlerAudit            # 全量审计，缺口未入基线则退出码 1
//   dotnet run --project tools/HandlerAudit --selftest # 夹具自验（不依赖仓库源码）
// 输出：docs/handler-coverage.md（矩阵）+ artifacts/handler-coverage.json（机读）。
#nullable enable
using System.Reflection;
using System.Text.Json;

namespace HandlerAudit;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Contains("--selftest"))
            return SelfTest.Run();

        var root = FindRepoRoot(Environment.CurrentDirectory)
            ?? throw new InvalidOperationException("未找到仓库根（ArkTsBinding.slnx）");
        var handlersDir = Path.Combine(root, "src", "HarmonyOS.Maui", "Handlers");
        var baseline = AuditCore.LoadBaseline(
            Path.Combine(root, "tools", "HandlerAudit", "baseline.json"));

        var factorySrc = File.ReadAllText(Path.Combine(handlersDir, "HarmonyHandlerFactory.cs"));
        var arms = AuditCore.ParseFactory(factorySrc);
        var controls = typeof(Microsoft.Maui.Controls.Label).Assembly;

        // 基座覆盖集：HarmonyViewMapper.Base 键 + HarmonyViewHandler.UpdateValue 视觉拦截集
        var baseMapperSrc = File.ReadAllText(Path.Combine(handlersDir, "HarmonyViewMapper.cs"));
        var baseKeys = new HashSet<string>();
        foreach (System.Text.RegularExpressions.Match k in System.Text.RegularExpressions.Regex.Matches(
                     baseMapperSrc, @"nameof\((?:[\w.]+\.)?(?<n>\w+)\)"))
            baseKeys.Add(k.Groups["n"].Value);
        var visualKeys = AuditCore.ParseVisualProperties(
            File.ReadAllText(Path.Combine(handlersDir, "HarmonyViewHandler.cs")));
        foreach (var k in visualKeys) baseKeys.Add(k);
        // 手势经 HarmonyGestureManager 挂载（ConnectHandler），非 mapper 通道
        baseKeys.Add("GestureRecognizers");

        var results = new List<HandlerCoverage>();
        var errors = AuditCore.CheckFactoryOrder(arms, controls);

        foreach (var arm in arms)
        {
            var viewType = controls.GetType($"Microsoft.Maui.Controls.{arm.ViewTypeName}");
            if (viewType is null)
            {
                errors.Add($"工厂臂类型无法解析：{arm.ViewTypeName}");
                continue;
            }
            var handlerFile = Directory.EnumerateFiles(handlersDir, "*.cs")
                .FirstOrDefault(f => File.ReadAllText(f)
                    .Contains($"class {arm.HandlerClassName}"));
            if (handlerFile is null)
            {
                errors.Add($"未找到 handler 源文件：{arm.HandlerClassName}");
                continue;
            }
            var (mapped, chain) = AuditCore.ParseMapperKeys(
                File.ReadAllText(handlerFile), arm.HandlerClassName);
            var covered = new HashSet<string>(baseKeys);
            if (!chain.Contains("HarmonyViewMapper.Base"))
                errors.Add($"{arm.HandlerClassName} 未链接 HarmonyViewMapper.Base（链基：{chain}）");

            var expected = AuditCore.CollectExpectedProperties(viewType);
            var handlerBaseline = baseline.TryGetValue(arm.HandlerClassName, out var hb)
                ? hb : new Dictionary<string, string>();
            var mergedBaseline = new Dictionary<string, string>(
                baseline.TryGetValue("*", out var g) ? g : new());
            foreach (var kv in handlerBaseline) mergedBaseline[kv.Key] = kv.Value;

            results.Add(AuditCore.Diff(arm.HandlerClassName, arm.ViewTypeName,
                expected, mapped, covered, mergedBaseline));
        }

        var disciplineViolations = CheckDefaultValueDiscipline(handlersDir);
        WriteReports(root, results, errors);
        var gapCount = results.Sum(r => r.Gaps.Count);
        foreach (var v in disciplineViolations)
            Console.WriteLine($"  默认值纪律: {v}");
        Console.WriteLine($"HandlerAudit: {arms.Count} 个分派臂，{gapCount} 个未入基线缺口，{errors.Count} 个结构错误，{disciplineViolations.Count} 个默认值纪律违规");
        return gapCount > 0 || errors.Count > 0 || disciplineViolations.Count > 0 ? 1 : 0;
    }

    /// <summary>
    /// 默认值纪律检查：Handler 源码不得硬编码颜色/字号兜底（应走 sentinel 条件判断
    /// 或 HarmonyControlDefaults / HarmonyShellTheme 引用）。
    /// 豁免：HarmonyDrawingCanvas（图形绘制默认）、HarmonyShellTheme /
    /// HarmonyControlDefaults（默认值定义中心本身）、BrushHelper（Transparent 哨兵）。
    /// </summary>
    private static List<string> CheckDefaultValueDiscipline(string handlersDir)
    {
        var violations = new List<string>();
        var exemptFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HarmonyDrawingCanvas.cs",
            "HarmonyShellTheme.cs",
            "HarmonyControlDefaults.cs",
            "BrushHelper.cs",
        };

        // 硬编码字号模式：仅匹配字号语义上下文（FontSize = 16f / SetFontSize(16f) 等），
        // 避免误伤 SetWidth(24f) 之类的合法尺寸字面量
        var fontSizeRx = new System.Text.RegularExpressions.Regex(
            @"\bFontSize\s*=\s*(1[0-9]|2[0-9]|3[0-2])(\.\d+)?f?\b",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        // 硬编码颜色模式：Colors.Black / Colors.White / Colors.Gray / FromRgba(
        var colorRx = new System.Text.RegularExpressions.Regex(
            @"Colors\.(Black|White|Gray|Red|Green|Blue)|FromRgba\(",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        foreach (var file in Directory.EnumerateFiles(handlersDir, "*.cs"))
        {
            var name = Path.GetFileName(file);
            if (exemptFiles.Contains(name))
                continue;
            var source = File.ReadAllText(file);
            var lineNum = 0;
            foreach (var line in source.Split('\n'))
            {
                lineNum++;
                // 剥离行注释，避免命中注释中的示例值
                var code = line;
                var commentIdx = code.IndexOf("//", StringComparison.Ordinal);
                if (commentIdx >= 0)
                    code = code[..commentIdx];
                if (string.IsNullOrWhiteSpace(code))
                    continue;
                if (fontSizeRx.IsMatch(code))
                    violations.Add($"{name}:{lineNum} 硬编码字号兜底（应走 sentinel 判断或 HarmonyControlDefaults）");
                if (colorRx.IsMatch(code))
                    violations.Add($"{name}:{lineNum} 硬编码颜色（应走 sentinel 判断或主题引用）");
            }
        }
        return violations;
    }

    private static string? FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ArkTsBinding.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static void WriteReports(string root, List<HandlerCoverage> results, List<string> errors)
    {
        var md = new List<string>
        {
            "# Handler 覆盖矩阵（HandlerAudit 生成，勿手改）",
            "",
            "| Handler | 控件 | 已映射 | 基座覆盖 | 基线豁免 | 缺口 |",
            "|---|---|---|---|---|---|",
        };
        foreach (var r in results)
            md.Add($"| {r.Handler} | {r.ViewType} | {r.Mapped.Count} | {r.CoveredByBase.Count} " +
                   $"| {r.Baselined.Count} | {(r.Gaps.Count == 0 ? "0" : string.Join(", ", r.Gaps))} |");
        if (errors.Count > 0)
        {
            md.Add("");
            md.Add("## 结构错误");
            md.AddRange(errors.Select(e => $"- {e}"));
        }
        File.WriteAllText(Path.Combine(root, "docs", "handler-coverage.md"), string.Join('\n', md));

        Directory.CreateDirectory(Path.Combine(root, "artifacts"));
        File.WriteAllText(Path.Combine(root, "artifacts", "handler-coverage.json"),
            JsonSerializer.Serialize(new { results, errors },
                new JsonSerializerOptions { WriteIndented = true }));
    }
}
