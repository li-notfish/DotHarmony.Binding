// Shell 第一版验证示例（对齐计划"Shell 示例回归"）：
// tab 切换、GoToAsync 绝对路由 + query 参数、".." 返回、模态、系统返回键。
// 所有验证结果以 [V] 前缀写入 hilog（tag=VProbe）供自动化断言。
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using HarmonyOS.Maui.Hosting;
using HarmonyOS.Interop;

namespace HelloApp;

public class AppShell : Shell
{
    public static AppShell? Instance { get; private set; }

    public AppShell()
    {
        Instance = this;
        HarmonyShellNavigation.RegisterRoute("probeDetail", typeof(ProbeDetailPage));
        HarmonyShellNavigation.RegisterRoute("probeGrid", typeof(GridProbePage));

        Items.Add(new ShellItem
        {
            Route = "home",
            Title = "首页",
            Items =
            {
                new Tab
                {
                    Title = "首页",
                    Items = { new ShellContent { Route = "homeMain", Title = "首页", ContentTemplate = new DataTemplate(() => new ShellHomePage()) } }
                }
            }
        });
        Items.Add(new ShellItem
        {
            Route = "probe",
            Title = "绑定",
            Items =
            {
                new Tab
                {
                    Title = "绑定",
                    Items = { new ShellContent { Route = "probeMain", Title = "绑定", ContentTemplate = new DataTemplate(() => new BindingProbePage()) } }
                }
            }
        });
        HiLog.Info("VProbe", "[V][SHELL] shell built");
    }
}

public class ShellHomePage : ContentPage
{
    private readonly Label _status = new() { Text = "ready", FontSize = 14 };

    public ShellHomePage()
    {
        Title = "首页";
        this.SetAppThemeColor(
            VisualElement.BackgroundColorProperty,
            Colors.White,
            Color.FromRgba(24, 24, 28, 255));

        var layout = new VerticalStackLayout { Spacing = 10, Padding = 12 };

        var title = new Label { Text = "Shell 验证主页", FontSize = 22 };
        title.SetAppThemeColor(
            Label.TextColorProperty,
            Colors.Black,
            Color.FromRgba(235, 235, 240, 255));
        layout.Children.Add(title);

        var routeBtn = new Button { Text = "GoToAsync //probeDetail?value=hello42" };
        routeBtn.Clicked += async (_, _) =>
            await HarmonyShellNavigation.GoToAsync(AppShell.Instance!, "//probeDetail?value=hello42");
        layout.Children.Add(routeBtn);

        var modalBtn = new Button { Text = "Open modal" };
        modalBtn.Clicked += async (_, _) =>
        {
            HiLog.Info("VProbe", "[V][MODAL] open");
            await Navigation.PushModalAsync(new ModalProbePage());
        };
        layout.Children.Add(modalBtn);

        var gridBtn = new Button { Text = "GoToAsync //probeGrid" };
        gridBtn.Clicked += async (_, _) =>
            await HarmonyShellNavigation.GoToAsync(AppShell.Instance!, "//probeGrid");
        layout.Children.Add(gridBtn);

        // M2 探针：FlyoutBehavior / FlyoutIsPresented 双向 / NavBar / TabBar / 标题热更新
        // （Locked 切换放最前：Locked 并排后内容区极窄，靠后的按钮会被挤出可视区）
        var lockBtn = new Button { Text = "Toggle FlyoutBehavior Locked" };
        lockBtn.Clicked += (_, _) =>
        {
            var sh = AppShell.Instance!;
            sh.FlyoutBehavior = sh.FlyoutBehavior == FlyoutBehavior.Locked
                ? FlyoutBehavior.Flyout : FlyoutBehavior.Locked;
            HiLog.Info("VProbe", $"[V][BEHAVIOR] {sh.FlyoutBehavior}");
        };
        layout.Children.Add(lockBtn);

        var flyoutBtn = new Button { Text = "Open flyout via FlyoutIsPresented" };
        flyoutBtn.Clicked += (_, _) =>
        {
            AppShell.Instance!.FlyoutIsPresented = true;
            HiLog.Info("VProbe", $"[V][FLYOUT] set presented=true, actual={AppShell.Instance!.FlyoutIsPresented}");
        };
        layout.Children.Add(flyoutBtn);

        var navBtn = new Button { Text = "Toggle NavBar" };
        navBtn.Clicked += (_, _) =>
        {
            var next = !Shell.GetNavBarIsVisible(this);
            Shell.SetNavBarIsVisible(this, next);
            HiLog.Info("VProbe", $"[V][NAVBAR] visible={next}");
        };
        layout.Children.Add(navBtn);

        var tabBtn = new Button { Text = "Toggle TabBar" };
        tabBtn.Clicked += (_, _) =>
        {
            var next = !Shell.GetTabBarIsVisible(this);
            Shell.SetTabBarIsVisible(this, next);
            HiLog.Info("VProbe", $"[V][TABBAR] visible={next}");
        };
        layout.Children.Add(tabBtn);

        var titleBtn = new Button { Text = "Rename title" };
        titleBtn.Clicked += (_, _) =>
        {
            Title = "已改名";
            HiLog.Info("VProbe", "[V][TITLE] renamed");
        };
        layout.Children.Add(titleBtn);

        // 视觉协议探针：Translation/Scale/Rotation/Opacity 一次落齐
        layout.Children.Add(new Label
        {
            Text = "Transformed label",
            BackgroundColor = Colors.LightBlue,
            Padding = 8,
            Scale = 1.2,
            Rotation = 8,
            TranslationX = 16,
            Opacity = 0.75,
        });

        // InputTransparent 探针：红色半透明层覆盖按钮；InputTransparent 级联下按钮必须仍可点
        var tapme = new Button { Text = "Tap through overlay" };
        tapme.Clicked += (_, _) =>
        {
            _status.Text = "TAP_OK";
            HiLog.Info("VProbe", "[V][INPUT] TAP_OK");
        };
        var overlayGrid = new Grid { HeightRequest = 56 };
        overlayGrid.Children.Add(tapme);
        overlayGrid.Children.Add(new Label
        {
            Text = "transparent overlay",
            BackgroundColor = Colors.Red.WithAlpha(0.25f),
            InputTransparent = true,
        });
        layout.Children.Add(overlayGrid);

        // ScrollView Neither：两轴约束到视口，触摸不得滚动
        var neither = new ScrollView
        {
            Orientation = ScrollOrientation.Neither,
            HeightRequest = 90,
            Content = new Label
            {
                Text = "NEITHER long content 1\nNEITHER long content 2\nNEITHER long content 3\nNEITHER long content 4",
                Padding = 8,
            },
        };
        neither.SetAppThemeColor(
            ScrollView.BackgroundColorProperty,
            Colors.Beige,
            Color.FromHex("#2A2A30"));
        layout.Children.Add(neither);

        layout.Children.Add(_status);
        Content = layout;
        HiLog.Info("VProbe", "[V][SHELL] home built");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HiLog.Info("VProbe", "[V][SHELL] home appearing");
    }
}

public class ProbeDetailPage : ContentPage, IQueryAttributable
{
    private readonly Label _query = new() { Text = "query: (none)", FontSize = 16 };

    public ProbeDetailPage()
    {
        Title = "Detail";
        var layout = new VerticalStackLayout { Spacing = 10, Padding = 12 };
        layout.Children.Add(new Label { Text = "Probe Detail (pushed route)", FontSize = 20 });
        layout.Children.Add(_query);

        var back = new Button { Text = "GoToAsync .." };
        back.Clicked += async (_, _) => await HarmonyShellNavigation.GoToAsync(AppShell.Instance!, "..");
        layout.Children.Add(back);

        Content = layout;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("value", out var value))
        {
            _query.Text = $"query: {value}";
            HiLog.Info("VProbe", $"[V][QUERY] value={value}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HiLog.Info("VProbe", "[V][SHELL] detail appearing");
    }
}

/// <summary>W2 布局夹具：Star 列（2*,1*）+ Auto/Star 混合行 + AbsoluteLayout 比例定位。</summary>
public class GridProbePage : ContentPage
{
    public GridProbePage()
    {
        Title = "GridProbe";
        var root = new VerticalStackLayout { Spacing = 12, Padding = 12 };

        // Star 列 + Auto 行：两列按 2:1 分宽，首行随内容高
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            HeightRequest = 220,
        };
        var header = new Label { Text = "Auto row, span 2", BackgroundColor = Colors.LightGray };
        Grid.SetColumnSpan(header, 2);
        grid.Children.Add(header);
        var left = new Label { Text = "2* col", BackgroundColor = Colors.LightBlue, VerticalTextAlignment = TextAlignment.Center };
        Grid.SetRow(left, 1);
        grid.Children.Add(left);
        var right = new Button { Text = "1* col" };
        Grid.SetRow(right, 1);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        root.Children.Add(grid);

        // AbsoluteLayout 比例定位：50% 宽贴右下
        var abs = new Microsoft.Maui.Controls.AbsoluteLayout { HeightRequest = 160, BackgroundColor = Colors.Beige };
        var box = new BoxView { Color = Colors.Orange };
        Microsoft.Maui.Controls.AbsoluteLayout.SetLayoutFlags(box, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);
        Microsoft.Maui.Controls.AbsoluteLayout.SetLayoutBounds(box, new Microsoft.Maui.Graphics.Rect(0.5, 0.5, 0.5, 0.5));
        abs.Children.Add(box);
        root.Children.Add(abs);

        var back = new Button { Text = "GoToAsync .." };
        back.Clicked += async (_, _) => await HarmonyShellNavigation.GoToAsync(AppShell.Instance!, "..");
        root.Children.Add(back);

        Content = root;
    }
}

public class ModalProbePage : ContentPage
{
    public ModalProbePage()
    {
        Title = "Modal";
        BackgroundColor = Colors.White;
        var layout = new VerticalStackLayout { Spacing = 10, Padding = 12 };
        layout.Children.Add(new Label { Text = "Modal page", FontSize = 22 });

        var close = new Button { Text = "Close modal" };
        close.Clicked += async (_, _) => await Navigation.PopModalAsync();
        layout.Children.Add(close);

        Content = layout;
        HiLog.Info("VProbe", "[V][MODAL] modal built");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HiLog.Info("VProbe", "[V][MODAL] modal appearing");
    }
}
