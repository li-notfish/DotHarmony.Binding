// HandlerAudit 核心逻辑：工厂分派解析、Mapper 键源级扫描、接口继承链属性收集、差异比对。
// 与 Program.cs 分离以便 --selftest 直接驱动各函数。
#nullable enable
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HandlerAudit;

/// <summary>一个工厂分派臂：MAUI 控件类型 → Handler 类。</summary>
public sealed record DispatchArm(string ViewTypeName, string HandlerClassName);

/// <summary>审计结果：一个 handler 的覆盖情况。</summary>
public sealed record HandlerCoverage(
    string Handler, string ViewType,
    List<string> Mapped, List<string> CoveredByBase,
    List<string> Gaps, Dictionary<string, string> Baselined);

public static class AuditCore
{
    /// <summary>参与比对的 MAUI 接口白名单（Core + Controls 的 *Element 契约）。</summary>
    public static readonly HashSet<string> WhitelistedInterfaces = new()
    {
        "Microsoft.Maui.IView", "Microsoft.Maui.IContentView", "Microsoft.Maui.IBorderView",
        "Microsoft.Maui.IText", "Microsoft.Maui.ITextStyle", "Microsoft.Maui.ITextAlignment",
        "Microsoft.Maui.IButton", "Microsoft.Maui.IImage", "Microsoft.Maui.IScrollView",
        "Microsoft.Maui.IStackLayout", "Microsoft.Maui.ILayout", "Microsoft.Maui.IPadding",
        "Microsoft.Maui.IRange", "Microsoft.Maui.ISlider", "Microsoft.Maui.ISwitch",
        "Microsoft.Maui.ICheckBox", "Microsoft.Maui.IRadioButton", "Microsoft.Maui.IEntry",
        "Microsoft.Maui.IEditor", "Microsoft.Maui.IEditText", "Microsoft.Maui.IInputView",
        "Microsoft.Maui.ILabel", "Microsoft.Maui.IPicker", "Microsoft.Maui.IDatePicker",
        "Microsoft.Maui.ITimePicker", "Microsoft.Maui.IProgress", "Microsoft.Maui.IRefreshView",
        "Microsoft.Maui.IItemsView", "Microsoft.Maui.ICollectionView", "Microsoft.Maui.ICarouselView",
        "Microsoft.Maui.IGraphicsView", "Microsoft.Maui.IShapeView", "Microsoft.Maui.IShape",
        "Microsoft.Maui.IBoxView", "Microsoft.Maui.IPage", "Microsoft.Maui.IContentPage",
        "Microsoft.Maui.INavigationView", "Microsoft.Maui.IShell",
        "Microsoft.Maui.Controls.IBorderElement", "Microsoft.Maui.Controls.IFontElement",
        "Microsoft.Maui.Controls.ITextElement", "Microsoft.Maui.Controls.IButtonElement",
        "Microsoft.Maui.Controls.IImageElement", "Microsoft.Maui.Controls.ITextAlignmentElement",
    };

    /// <summary>解析 HarmonyHandlerFactory 的 switch 分派臂（保序，供拓扑检查）。</summary>
    public static List<DispatchArm> ParseFactory(string factorySource)
    {
        var arms = new List<DispatchArm>();
        var rx = new Regex(@"Microsoft\.Maui\.Controls\.(\w+)\s*=>\s*new\s+(\w+)\(\)",
            RegexOptions.Compiled);
        foreach (Match m in rx.Matches(factorySource))
            arms.Add(new DispatchArm(m.Groups[1].Value, m.Groups[2].Value));
        return arms;
    }

    /// <summary>从 handler 源文件提取指定 handler 类的 Mapper 键集合与链基。</summary>
    public static (HashSet<string> Keys, string ChainBase) ParseMapperKeys(
        string handlerSource, string handlerClassName)
    {
        const string pattern =
            @"PropertyMapper<(?<view>[^>]+),\s*(?<handler>\w+)>\s*Mapper\s*=\s*new\((?<chain>[^)]*)\)\s*\{(?<keys>.*?)\};";
        var rx = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled);
        var matches = rx.Matches(handlerSource);
        // 优先第二泛型参数 = handler 类名（多 handler 文件）；否则单 handler 文件取首个
        // （ScrollView/Entry 等 Mapper 以接口 IViewHandler 为第二泛型参数）
        var m = matches.Cast<Match>()
            .FirstOrDefault(x => x.Groups["handler"].Value == handlerClassName);
        if (m is null)
        {
            var classCount = Regex.Matches(handlerSource, @"class\s+\w+Handler\b").Count;
            if (classCount == 1) m = matches.FirstOrDefault();
        }
        if (m is null)
            return (new HashSet<string>(), "");
        var keys = new HashSet<string>();
        foreach (Match k in Regex.Matches(m.Groups["keys"].Value, @"nameof\((?:[\w.]+\.)?(?<n>\w+)\)"))
            keys.Add(k.Groups["n"].Value);
        return (keys, m.Groups["chain"].Value.Trim());
    }

    /// <summary>解析 HarmonyViewHandler 的 VisualProperties 拦截集（UpdateValue 通道，视为已覆盖）。</summary>
    public static HashSet<string> ParseVisualProperties(string viewHandlerSource)
    {
        var rx = new Regex(@"VisualProperties\s*=\s*new\([^)]*\)\s*\{(?<keys>.*?)\};",
            RegexOptions.Singleline | RegexOptions.Compiled);
        var m = rx.Match(viewHandlerSource);
        var keys = new HashSet<string>();
        if (m.Success)
            foreach (Match k in Regex.Matches(m.Groups["keys"].Value, @"nameof\((?:[\w.]+\.)?(?<n>\w+)\)"))
                keys.Add(k.Groups["n"].Value);
        return keys;
    }

    /// <summary>收集 arm 类型在白名单接口（含其白名单内基接口）上的属性全集。</summary>
    public static HashSet<string> CollectExpectedProperties(Type viewType)
    {
        var result = new HashSet<string>();
        // 具体类自身声明的属性（LineBreakMode/MaxLines/BorderColor 等 Controls 级契约
        // 不在 Core 接口上，DeclaredOnly 避免 View/Element 基类噪声）
        foreach (var p in viewType.GetProperties(
                     BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            result.Add(p.Name);
        var ifaces = viewType.GetInterfaces()
            .Where(i => i.FullName is not null && WhitelistedInterfaces.Contains(i.FullName))
            .ToList();
        foreach (var iface in ifaces)
        {
            foreach (var p in iface.GetProperties())
                result.Add(p.Name);
            // 接口继承链上同样在白名单内的基接口（如 IButton : IText）
            foreach (var baseIface in iface.GetInterfaces())
                if (baseIface.FullName is not null && WhitelistedInterfaces.Contains(baseIface.FullName))
                    foreach (var p in baseIface.GetProperties())
                        result.Add(p.Name);
        }
        return result;
    }

    /// <summary>工厂臂拓扑检查：派生类型必须排在基类臂之前（switch 顺序截获）。</summary>
    public static List<string> CheckFactoryOrder(List<DispatchArm> arms, Assembly controlsAssembly)
    {
        var errors = new List<string>();
        var types = arms
            .Select(a => controlsAssembly.GetType($"Microsoft.Maui.Controls.{a.ViewTypeName}"))
            .ToList();
        for (int i = 0; i < arms.Count; i++)
        {
            if (types[i] is null) continue;
            for (int j = i + 1; j < arms.Count; j++)
            {
                if (types[j] is not null && types[i]!.IsAssignableFrom(types[j]))
                    errors.Add(
                        $"工厂分派顺序错误：{arms[j].ViewTypeName} 派生自 {arms[i].ViewTypeName} 却排在后面，永远不会命中");
            }
        }
        return errors;
    }

    /// <summary>差异比对：expected - mapped - coveredByBase - baseline = gaps。</summary>
    public static HandlerCoverage Diff(
        string handler, string viewType,
        HashSet<string> expected, HashSet<string> mapped, HashSet<string> coveredByBase,
        Dictionary<string, string> baseline)
    {
        var baselined = new Dictionary<string, string>();
        var gaps = new List<string>();
        foreach (var p in expected.OrderBy(x => x))
        {
            if (mapped.Contains(p) || coveredByBase.Contains(p)) continue;
            if (baseline.TryGetValue(p, out var reason))
                baselined[p] = reason;
            else
                gaps.Add(p);
        }
        return new HandlerCoverage(handler, viewType,
            mapped.OrderBy(x => x).ToList(), coveredByBase.OrderBy(x => x).ToList(),
            gaps, baselined);
    }

    /// <summary>读取 baseline.json：{"*": {"Prop": "reason"}, "HandlerName": {...}}。</summary>
    public static Dictionary<string, Dictionary<string, string>> LoadBaseline(string path)
        => File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                File.ReadAllText(path)) ?? new()
            : new();
}
