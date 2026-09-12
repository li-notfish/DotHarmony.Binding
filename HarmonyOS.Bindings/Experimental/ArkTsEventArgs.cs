namespace HarmonyOS.Bindings.Experimental;

/// <summary>
/// ArkTS 引擎回流事件（MIGRATION_ARKTS_ENGINE.md §2.2 事件/量测回流的实验子集）。
/// napi 回调内解析完成后以纯托管值分派，处理器不得回读 napi 句柄。
/// </summary>
public readonly struct ArkTsEventArgs
{
    /// <summary>事件种类：'click'（点击）或 'area'（量测回流）</summary>
    public string Kind { get; init; }

    /// <summary>仅 area：节点宽度（px）</summary>
    public double WidthPx { get; init; }

    /// <summary>仅 area：节点高度（px）</summary>
    public double HeightPx { get; init; }

    /// <summary>仅 area：屏幕密度（px/vp），引擎随量测直接下发</summary>
    public double Density { get; init; }

    public override string ToString()
        => Kind == "area" ? $"area {WidthPx:0}x{HeightPx:0}px @{Density:0.00}" : Kind;
}

/// <summary>量测影子快照：MeasuredSize 的引擎版读取源（替代同步 P/Invoke）</summary>
public readonly record struct ArkTsSize(int Width, int Height)
{
    public static ArkTsSize Empty => default;
    public override string ToString() => $"{Width}x{Height}";
}
