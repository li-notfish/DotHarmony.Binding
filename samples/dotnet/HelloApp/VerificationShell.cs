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
        var layout = new VerticalStackLayout { Spacing = 10, Padding = 12 };

        layout.Children.Add(new Label { Text = "Shell 验证主页", FontSize = 22 });

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
            BackgroundColor = Colors.Beige,
            Content = new Label
            {
                Text = "NEITHER long content 1\nNEITHER long content 2\nNEITHER long content 3\nNEITHER long content 4",
                Padding = 8,
            },
        };
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
