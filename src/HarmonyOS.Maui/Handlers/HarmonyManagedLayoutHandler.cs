// HarmonyManagedLayoutHandler：Grid / AbsoluteLayout → ArkUI Stack + MAUI 托管布局
//
// 布局模型：与 HarmonyLayoutHandler 的"ArkUI flex 引擎托管"不同，Grid/AbsoluteLayout
// 的语义（Star/Auto 轨道、比例定位）无法直接映射到单个 ArkUI 布局容器，因此改用
// MAUI 托管布局：平台视图是 Stack（子节点默认不参与流式布局），
// Handler 在 C# 侧计算每个子节点的位置与尺寸，经 NODE_POSITION / NODE_WIDTH /
// NODE_HEIGHT 绝对定位。
//
// 尺寸来源：NODE_ON_SIZE_CHANGE（vp）提供容器大小；密度由容器 MeasuredSize(px)
// 与 SizeChange(vp) 的比值推导，用于 Auto 轨道的实测换算。
//
// M1 限制：
//   - Auto 轨道依赖子节点上一帧的自量测结果（子节点不设显式尺寸，由内容自撑），
//     收敛需要 1~2 帧；内容自身变化不会触发重排。
//   - Span>1 的子节点按起始轨道定位、跨轨道尺寸求和，不参与 Star 权重精算。
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Bindings.Runtime;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;
using ArkStack = HarmonyOS.ArkUI.Stack;
using MAbsolute = Microsoft.Maui.Controls.AbsoluteLayout;
using MALayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;
using MBindableObject = Microsoft.Maui.Controls.BindableObject;
using MControlsLayout = Microsoft.Maui.Controls.Layout;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Grid / AbsoluteLayout 的 HarmonyOS Handler（MAUI 托管布局 + 绝对定位）。</summary>
public class HarmonyManagedLayoutHandler : ViewHandler<MControlsLayout, ArkStack>
{
    public static PropertyMapper<MControlsLayout, HarmonyManagedLayoutHandler> Mapper =
        new(ViewHandler.ViewMapper)
        {
            [nameof(MControlsLayout.BackgroundColor)] = (h, v) =>
            {
                if (v.BackgroundColor is { } c)
                    h.PlatformView.SetBackgroundColor((byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255), (byte)(c.Alpha * 255));
            },
        };

    // 与 HarmonyLayoutHandler 相同的子树变更协议，但子节点装配交给 Arrange 统一约束
    public static CommandMapper<MControlsLayout, HarmonyManagedLayoutHandler> LayoutCommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(ILayoutHandler.Add)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyManagedLayoutHandler)h).Add(u.View); },
            [nameof(ILayoutHandler.Remove)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyManagedLayoutHandler)h).Remove(u.View); },
            [nameof(ILayoutHandler.Clear)] = (h, v, args) => ((HarmonyManagedLayoutHandler)h).Clear(),
            [nameof(ILayoutHandler.Insert)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyManagedLayoutHandler)h).Add(u.View); },
            [nameof(ILayoutHandler.Update)] = (h, v, args) => { if (args is LayoutHandlerUpdate u) ((HarmonyManagedLayoutHandler)h).Add(u.View); },
            [nameof(ILayoutHandler.UpdateZIndex)] = (h, v, args) => { /* Stack 按 addChild 顺序决定叠放，M1 忽略 */ },
        };

    public HarmonyManagedLayoutHandler() : base(Mapper, LayoutCommandMapper) { }

    // 容器最近一次实测尺寸（vp），NODE_ON_SIZE_CHANGE 提供
    private float _containerW;
    private float _containerH;
    private readonly Dictionary<IView, IElementHandler> _children = new();

    protected override ArkStack CreatePlatformView() => new();

    protected override void ConnectHandler(ArkStack platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SizeChange += OnSizeChange;
        // 让 Stack 填满父容器（Column），确保 ArkUI 给它真实尺寸，SizeChange 能触发
        platformView.SetWidthPercent(1.0f);
        platformView.SetHeightPercent(1.0f);
        HiLog.Debug("Grid", $"ConnectHandler#{GetHashCode():X}: Stack W%+H% set, children={VirtualView.Children.Count}");
        // 连接时全量同步已存在的 Children（Controls 侧在 Handler 连接前添加的子节点不会发 Add 命令）
        foreach (var child in VirtualView.Children)
        {
            AttachChild(child);
        }
    }

    private void OnSizeChange(ArkUINodeEvent e)
    {
        _containerW = e.SizeChangeWidth;
        _containerH = e.SizeChangeHeight;
        HiLog.Debug("Grid", $"SizeChange: W={_containerW} H={_containerH}");
        Arrange();
    }

    protected override void DisconnectHandler(ArkStack platformView)
    {
        platformView.SizeChange -= OnSizeChange;
        base.DisconnectHandler(platformView);
    }

    private void AttachChild(IView view)
    {
        if (_children.ContainsKey(view)) return;
        var handler = HarmonyHandlerFactory.Create(view);
        handler.SetVirtualView(view);
        if (handler.PlatformView is ArkUINode node)
        {
            // 尺寸与位置全部由 Arrange 决定，此处不设任何布局属性
            PlatformView.AddChild(node);
            _children[view] = handler;
            Arrange();
        }
    }

    internal void Add(IView view) => AttachChild(view);

    internal void Remove(IView view)
    {
        if (_children.Remove(view, out var handler) && handler.PlatformView is ArkUINode node)
        {
            PlatformView.RemoveChild(node);
        }
    }

    internal void Clear()
    {
        PlatformView.RemoveAllChildren();
        _children.Clear();
    }

    // ───────────────────────── 托管布局 ─────────────────────────

    /// <summary>px/vp 密度：由容器 MeasuredSize(px) 与 SizeChange(vp) 比值推导。</summary>
    private float GetDensity()
    {
        var px = PlatformView.MeasuredSize;
        if (px.width > 0 && _containerW > 0)
        {
            var d = px.width / _containerW;
            if (d is >= 0.5f and <= 8f) return d;
        }
        return 3f; // 常见手机密度兜底
    }

    private void Arrange()
    {
        if (_containerW <= 0 || _containerH <= 0) return;
        HiLog.Debug("Grid", $"Arrange: containerW={_containerW} containerH={_containerH}");
        if (VirtualView is Grid grid)
            ArrangeGrid(grid, GetDensity());
        else if (VirtualView is MAbsolute absolute)
            ArrangeAbsolute(absolute);
    }

    private void ArrangeGrid(Grid grid, float density)
    {
        var colDefs = grid.ColumnDefinitions;
        var rowDefs = grid.RowDefinitions;
        int nCols = Math.Max(colDefs.Count, 1);
        int nRows = Math.Max(rowDefs.Count, 1);

        var colUnit = new GridUnitType[nCols];
        var rowUnit = new GridUnitType[nRows];
        var colValue = new float[nCols];
        var rowValue = new float[nRows];
        var colAuto = new float[nCols];
        var rowAuto = new float[nRows];

        for (int i = 0; i < nCols; i++)
        {
            var gl = i < colDefs.Count ? colDefs[i].Width : new GridLength(1, GridUnitType.Star);
            colUnit[i] = gl.GridUnitType;
            colValue[i] = (float)gl.Value;
        }
        for (int i = 0; i < nRows; i++)
        {
            var gl = i < rowDefs.Count ? rowDefs[i].Height : new GridLength(1, GridUnitType.Star);
            rowUnit[i] = gl.GridUnitType;
            rowValue[i] = (float)gl.Value;
        }

        // Auto 轨道：取子节点上一帧自量测尺寸（px→vp）的最大值
        foreach (var (view, handler) in _children)
        {
            if (handler.PlatformView is not ArkUINode node) continue;
            int c = Math.Clamp(Grid.GetColumn((MBindableObject)view), 0, nCols - 1);
            int r = Math.Clamp(Grid.GetRow((MBindableObject)view), 0, nRows - 1);
            var size = node.MeasuredSize;
            if (colUnit[c] == GridUnitType.Auto && Grid.GetColumnSpan((MBindableObject)view) == 1 && size.width > 0)
                colAuto[c] = Math.Max(colAuto[c], size.width / density);
            if (rowUnit[r] == GridUnitType.Auto && Grid.GetRowSpan((MBindableObject)view) == 1 && size.height > 0)
                rowAuto[r] = Math.Max(rowAuto[r], size.height / density);
        }

        // Auto 轨道兜底：子节点未被 ArkUI 量测时（首帧），按 MAUI 控件语义估算高度
        // 下一帧 SizeChange 触发后 MeasuredSize 有真实值，自动修正
        foreach (var (view, handler) in _children)
        {
            if (handler.PlatformView is not ArkUINode node) continue;
            int c = Math.Clamp(Grid.GetColumn((MBindableObject)view), 0, nCols - 1);
            int r = Math.Clamp(Grid.GetRow((MBindableObject)view), 0, nRows - 1);
            if (colUnit[c] == GridUnitType.Auto && Grid.GetColumnSpan((MBindableObject)view) == 1 && colAuto[c] <= 0)
                colAuto[c] = view switch
                {
                    Label => 100,
                    Button => 120,
                    _ => 80,
                };
            if (rowUnit[r] == GridUnitType.Auto && Grid.GetRowSpan((MBindableObject)view) == 1 && rowAuto[r] <= 0)
                rowAuto[r] = view switch
                {
                    Label => 30,
                    Button => 44,
                    Image img => (float)(img.HeightRequest > 0 ? img.HeightRequest : 100),
                    _ => 30,
                };
        }

        HiLog.Debug("Grid", $"Auto rows: [{string.Join(",", rowAuto.Select(x => x.ToString("F0")))}] cols: [{string.Join(",", colAuto.Select(x => x.ToString("F0")))}]");

        var widths = ResolveTracks(colUnit, colValue, colAuto, _containerW);
        var heights = ResolveTracks(rowUnit, rowValue, rowAuto, _containerH);

        HiLog.Debug("Grid", $"Tracks: widths=[{string.Join(",", widths.Select(x => x.ToString("F0")))}] heights=[{string.Join(",", heights.Select(x => x.ToString("F0")))}]");

        foreach (var (view, handler) in _children)
        {
            if (handler.PlatformView is not ArkUINode node) continue;
            int c = Math.Clamp(Grid.GetColumn((MBindableObject)view), 0, nCols - 1);
            int r = Math.Clamp(Grid.GetRow((MBindableObject)view), 0, nRows - 1);
            int cs = Math.Clamp(Grid.GetColumnSpan((MBindableObject)view), 1, nCols - c);
            int rs = Math.Clamp(Grid.GetRowSpan((MBindableObject)view), 1, nRows - r);

            float x = Sum(widths, 0, c);
            float y = Sum(heights, 0, r);
            float w = Sum(widths, c, cs);
            float h = Sum(heights, r, rs);

            var size = node.MeasuredSize;
            HiLog.Debug("Grid", $"  [{view.GetType().Name}] r={r}c={c} span={rs}x{cs} -> ({x:F0},{y:F0}) {w:F0}x{h:F0} measured={size.width}x{size.height}");

            // 自适应维不设显式尺寸，让节点内容自撑（下一帧据此量测 Auto 轨道）
            bool wAuto = colUnit[c] == GridUnitType.Auto && cs == 1;
            bool hAuto = rowUnit[r] == GridUnitType.Auto && rs == 1;

            // MAUI 子节点 Margin：在轨道单元内内缩（auto 维不缩，让内容自撑）
            var mg = view.Margin;
            float mgL = (float)mg.Left, mgT = (float)mg.Top, mgR = (float)mg.Right, mgB = (float)mg.Bottom;
            if (mgL > 0 || mgT > 0 || mgR > 0 || mgB > 0)
            {
                x += mgL; y += mgT;
                if (!wAuto) w = Math.Max(w - mgL - mgR, 0);
                if (!hAuto) h = Math.Max(h - mgT - mgB, 0);
            }

            if (!wAuto) node.SetWidth(w);
            if (!hAuto) node.SetHeight(h);
            node.SetPosition(x, y);
        }
    }

    private void ArrangeAbsolute(MAbsolute absolute)
    {
        foreach (var (view, handler) in _children)
        {
            if (handler.PlatformView is not ArkUINode node) continue;
            var bounds = MAbsolute.GetLayoutBounds((MBindableObject)view);
            var flags = MAbsolute.GetLayoutFlags((MBindableObject)view);
            bool sizeProp = flags.HasFlag(MALayoutFlags.SizeProportional);
            bool posProp = flags.HasFlag(MALayoutFlags.PositionProportional);

            bool wAuto = !sizeProp && bounds.Width == MAbsolute.AutoSize;
            bool hAuto = !sizeProp && bounds.Height == AbsoluteLayout.AutoSize;
            float w = sizeProp ? (float)bounds.Width * _containerW : (float)bounds.Width;
            float h = sizeProp ? (float)bounds.Height * _containerH : (float)bounds.Height;

            // 比例定位锚定的是"扣除自身尺寸后的可放置区"
            float x = posProp ? (float)bounds.X * (_containerW - w) : (float)bounds.X;
            float y = posProp ? (float)bounds.Y * (_containerH - h) : (float)bounds.Y;

            // MAUI 子节点 Margin：定位偏移总是生效；比例/显式尺寸维内缩，auto 维不缩
            var mg = view.Margin;
            float mgL = (float)mg.Left, mgT = (float)mg.Top, mgR = (float)mg.Right, mgB = (float)mg.Bottom;
            if (mgL > 0 || mgT > 0 || mgR > 0 || mgB > 0)
            {
                x += mgL; y += mgT;
                if (!wAuto) w = Math.Max(w - mgL - mgR, 0);
                if (!hAuto) h = Math.Max(h - mgT - mgB, 0);
            }

            if (!wAuto) node.SetWidth(w);
            if (!hAuto) node.SetHeight(h);
            node.SetPosition(Math.Max(x, 0), Math.Max(y, 0));
        }
    }

    /// <summary>按 Absolute/Auto/Star 语义解析轨道尺寸（vp）；无 Star 时剩余空间留空。</summary>
    private static float[] ResolveTracks(GridUnitType[] units, float[] values, float[] autoSizes, float container)
    {
        var result = new float[units.Length];
        float fixedTotal = 0;
        float starWeight = 0;
        for (int i = 0; i < units.Length; i++)
        {
            switch (units[i])
            {
                case GridUnitType.Absolute:
                    result[i] = values[i];
                    fixedTotal += values[i];
                    break;
                case GridUnitType.Auto:
                    result[i] = autoSizes[i];
                    fixedTotal += autoSizes[i];
                    break;
                case GridUnitType.Star:
                    starWeight += Math.Max(values[i], 0);
                    break;
            }
        }
        float remaining = Math.Max(container - fixedTotal, 0);
        if (starWeight > 0)
        {
            for (int i = 0; i < units.Length; i++)
            {
                if (units[i] == GridUnitType.Star)
                    result[i] = remaining * Math.Max(values[i], 0) / starWeight;
            }
        }
        return result;
    }

    private static float Sum(float[] tracks, int start, int count)
    {
        float sum = 0;
        for (int i = start; i < start + count && i < tracks.Length; i++)
            sum += tracks[i];
        return sum;
    }
}
