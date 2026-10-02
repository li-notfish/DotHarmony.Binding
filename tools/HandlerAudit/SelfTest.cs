// --selftest：不依赖仓库源树的夹具自验（伪造 factory/handler 源文本 + 真实 MAUI 类型反射）。
#nullable enable
namespace HandlerAudit;

public static class SelfTest
{
    public static int Run()
    {
        var failures = new List<string>();

        // 1. 工厂臂解析 + 拓扑检查：Layout 臂在 Grid 臂之前必须报错（Grid 派生自 Layout）
        const string fakeFactory = """
            Microsoft.Maui.Controls.Layout => new HarmonyLayoutHandler(),
            Microsoft.Maui.Controls.Grid => new HarmonyManagedLayoutHandler(),
            """;
        var arms = AuditCore.ParseFactory(fakeFactory);
        if (arms.Count != 2 || arms[0].ViewTypeName != "Layout" || arms[1].HandlerClassName != "HarmonyManagedLayoutHandler")
            failures.Add("ParseFactory 解析错误");
        var orderErrors = AuditCore.CheckFactoryOrder(arms, typeof(Microsoft.Maui.Controls.Label).Assembly);
        if (orderErrors.Count != 1)
            failures.Add($"CheckFactoryOrder 应报 1 个顺序错误，实际 {orderErrors.Count}");
        // 顺序正确时不报
        var okErrors = AuditCore.CheckFactoryOrder(
            new List<DispatchArm> { arms[1], arms[0] }, typeof(Microsoft.Maui.Controls.Label).Assembly);
        if (okErrors.Count != 0)
            failures.Add("CheckFactoryOrder 误报正确顺序");

        // 2. Mapper 键源级扫描（含链基识别）
        const string fakeHandler = """
            public static PropertyMapper<Label, HarmonyLabelHandler> Mapper = new(HarmonyViewMapper.Base)
            {
                [nameof(Label.Text)] = MapText,
                [nameof(Label.TextColor)] = MapTextColor,
            };
            """;
        var (keys, chain) = AuditCore.ParseMapperKeys(fakeHandler, "HarmonyLabelHandler");
        if (!keys.SetEquals(new[] { "Text", "TextColor" }) || chain != "HarmonyViewMapper.Base")
            failures.Add($"ParseMapperKeys 错误：keys=[{string.Join(",", keys)}] chain={chain}");

        // 3. 差异比对：Label 全量期望 - 部分映射 - 基线 = 缺口；补齐基线后归零
        var expected = AuditCore.CollectExpectedProperties(typeof(Microsoft.Maui.Controls.Label));
        if (!expected.Contains("TextColor") || !expected.Contains("LineBreakMode"))
            failures.Add("CollectExpectedProperties 未覆盖 ILabel/IText 属性，实际=[" +
                string.Join(",", expected.OrderBy(x => x)) + "] 接口=[" +
                string.Join(",", typeof(Microsoft.Maui.Controls.Label).GetInterfaces()
                    .Select(i => i.FullName).OrderBy(x => x)) + "]");
        var diff = AuditCore.Diff("Fake", "Label", expected,
            new HashSet<string> { "Text" }, new HashSet<string>(), new());
        if (!diff.Gaps.Contains("TextColor") || diff.Gaps.Contains("Text"))
            failures.Add("Diff 缺口计算错误");
        var fullBaseline = diff.Gaps.ToDictionary(g => g, _ => "selftest");
        var diff2 = AuditCore.Diff("Fake", "Label", expected,
            new HashSet<string> { "Text" }, new HashSet<string>(), fullBaseline);
        if (diff2.Gaps.Count != 0 || diff2.Baselined.Count != diff.Gaps.Count)
            failures.Add("Diff 基线豁免错误");

        foreach (var f in failures) Console.Error.WriteLine($"SELFTEST FAIL: {f}");
        Console.WriteLine(failures.Count == 0 ? "SelfTest: 全部通过" : $"SelfTest: {failures.Count} 项失败");
        return failures.Count == 0 ? 0 : 1;
    }
}
