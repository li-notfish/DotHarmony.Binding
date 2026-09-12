// GesturesDemoPage 的 code-behind：手势识别管线在鸿蒙上的端到端验证
// （Tap/Double-Tap/Pan/Pinch/Swipe/Pointer/动态增删/IsEnabled 静默）
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace HelloApp;

public partial class GesturesDemoPage : ContentPage
{
    private static readonly Color AreaIdle = Color.FromArgb("#F1F5F9");

    private int _taps;
    private int _doubleTaps;
    private int _dynamicTaps;
    private TapGestureRecognizer? _dynamicTap;
    private bool _isEnabled = true;

    public GesturesDemoPage()
    {
        InitializeComponent();

        // ── Tap / Double-Tap ──
        var tap = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
        tap.Tapped += (_, e) =>
        {
            _taps++;
            Status.Text = $"tapped #{_taps} (pos={Fmt(e.GetPosition(TapArea))})";
        };
        TapArea.GestureRecognizers.Add(tap);

        var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTap.Tapped += (_, _) =>
        {
            _doubleTaps++;
            Status.Text = $"double-tapped #{_doubleTaps}";
        };
        DoubleTapArea.GestureRecognizers.Add(doubleTap);

        // ── Pan（TotalX/TotalY 单位 vp，等价 iOS points；Completed 事件本身不带坐标——
        //    MAUI 官方语义，故记录最后一次 Running 的累计值）──
        var pan = new PanGestureRecognizer();
        double panLastX = 0, panLastY = 0;
        pan.PanUpdated += (_, e) =>
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    panLastX = e.TotalX;
                    panLastY = e.TotalY;
                    PanLabel.Text = $"pan {e.TotalX:F0},{e.TotalY:F0} vp";
                    break;
                case GestureStatus.Completed:
                    PanLabel.Text = $"pan completed ({panLastX:F0},{panLastY:F0})";
                    break;
                case GestureStatus.Started:
                    PanLabel.Text = "pan started";
                    break;
                default:
                    PanLabel.Text = "pan canceled";
                    break;
            }
        };
        PanArea.GestureRecognizers.Add(pan);

        // ── Pinch ──
        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += (_, e) =>
        {
            PinchLabel.Text = e.Status switch
            {
                GestureStatus.Started => "pinch started",
                GestureStatus.Running => $"pinch scale {e.Scale:F2}",
                GestureStatus.Completed => $"pinch ended (scale {e.Scale:F2})",
                _ => "pinch canceled",
            };
        };
        PinchArea.GestureRecognizers.Add(pinch);

        // ── Swipe（四方向）──
        var swipe = new SwipeGestureRecognizer();
        swipe.Swiped += (_, e) => SwipeLabel.Text = $"swiped {e.Direction}";
        SwipeArea.GestureRecognizers.Add(swipe);

        // ── Pointer ──
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { PointerArea.BackgroundColor = Colors.Bisque; PointerLabel.Text = "pointer entered"; };
        pointer.PointerPressed += (_, e) =>
            PointerLabel.Text = $"pressed @ {Fmt(e.GetPosition(PointerArea))}";
        pointer.PointerMoved += (_, e) =>
            PointerLabel.Text = $"moved @ {Fmt(e.GetPosition(PointerArea))}";
        pointer.PointerReleased += (_, _) => PointerLabel.Text = "released";
        pointer.PointerExited += (_, _) =>
        {
            PointerArea.BackgroundColor = Colors.AliceBlue;
            PointerLabel.Text = "pointer exited";
        };
        PointerArea.GestureRecognizers.Add(pointer);

        // ── 动态增删 + IsEnabled 共用演示区：初始挂 tap，可手动增删/禁用 ──
        var areaTap = new TapGestureRecognizer();
        areaTap.Tapped += (_, _) => Status.Text = "demo area tapped";
        DisabledArea.GestureRecognizers.Add(areaTap);
    }

    private void OnToggleTapClicked(object? sender, EventArgs e)
    {
        if (_dynamicTap is null)
        {
            _dynamicTap = new TapGestureRecognizer();
            _dynamicTap.Tapped += (_, _) =>
            {
                _dynamicTaps++;
                DynamicTapLabel.Text = $"动态 tap：{_dynamicTaps}";
            };
            DisabledArea.GestureRecognizers.Add(_dynamicTap);
            ToggleTapButton.Text = "Remove tap recognizer";
        }
        else
        {
            DisabledArea.GestureRecognizers.Remove(_dynamicTap);
            _dynamicTap = null;
            ToggleTapButton.Text = "Add tap recognizer";
        }
    }

    private void OnToggleEnableClicked(object? sender, EventArgs e)
    {
        _isEnabled = !_isEnabled;
        DisabledArea.IsEnabled = _isEnabled;
        ToggleEnableButton.Text = _isEnabled ? "Set demo view IsEnabled=false" : "Set demo view IsEnabled=true";
        DisabledArea.BackgroundColor = _isEnabled ? AreaIdle : Colors.LightSlateGray;
    }

    private static string Fmt(Point? p) => p is { } v ? $"{v.X:F0},{v.Y:F0}" : "null";

    private void OnPopClicked(object? sender, EventArgs e)
    {
        Navigation.PopAsync().FireAndForgetNavigation();
    }
}
