// HarmonyShellTheme：Shell / TabbedPage 视觉默认值中心。
// 用户未显式设置 Shell attached 属性时，Handler 从这里取兜底值；
// 全部 static 可变属性，应用可在启动前统一覆写。
using MColor = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Handlers;

public static class HarmonyShellTheme
{
    private static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

    public static MColor ShellBackground => IsDark ? MColor.FromRgba(0.09, 0.09, 0.11, 1) : MColor.FromRgba(255, 255, 255, 255);

    // ── Flyout ──
    public static float FlyoutWidthVp { get; set; } = 280f;
    public static int FlyoutAnimationMs { get; set; } = 250;
    public static float FlyoutPaddingTop { get; set; } = 8f;
    public static float FlyoutPaddingRight { get; set; } = 0f;
    public static float FlyoutPaddingBottom { get; set; } = 8f;
    public static float FlyoutPaddingLeft { get; set; } = 16f;
    public static float FlyoutItemHeightVp { get; set; } = 48f;
    public static float FlyoutItemMarginLeft { get; set; } = 16f;
    public static float FlyoutItemFontSize { get; set; } = 15f;
    public static float FlyoutIconSizeVp { get; set; } = 22f;
    public static MColor FlyoutBackground => IsDark ? MColor.FromRgba(0.09, 0.09, 0.11, 1) : MColor.FromRgba(255, 255, 255, 255);
    public static MColor FlyoutForeground => IsDark ? MColor.FromRgba(0.92, 0.92, 0.95, 1) : MColor.FromRgba(0, 0, 0, 1);

    // ── 顶部导航栏 ──
    public static float TopBarHeightVp { get; set; } = 56f;
    public static float TopBarTitleFontSize { get; set; } = 18f;
    public static float HamburgerFontSize { get; set; } = 22f;
    public static float HamburgerMarginLeft { get; set; } = 16f;
    public static float HamburgerMarginRight { get; set; } = 16f;
    public static MColor TopBarBackground => IsDark ? MColor.FromRgba(0.09, 0.09, 0.11, 1) : MColor.FromRgba(255, 255, 255, 255);

    // ── TabBar ──
    public static float TabBarHeightVp { get; set; } = 56f;
    public static float TabIconSizeVp { get; set; } = 20f;
    public static MColor TabSelected => IsDark ? MColor.FromRgba(0.92, 0.92, 0.95, 1) : MColor.FromRgba(0, 0, 0, 1);
    public static MColor TabUnselected => IsDark ? MColor.FromRgba(0.58, 0.58, 0.62, 1) : MColor.FromRgba(0.5, 0.5, 0.5, 1);

    // ── 遮罩 ──
    public static byte BackdropAlpha { get; set; } = 102; // 40%
}
