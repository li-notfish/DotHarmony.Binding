# ControlsSampleApp

纯 UI 渲染主示例：`MainPage` 以 `TabbedPage` 承载 Home / Basics / Inputs / Layouts / Items / Media /
Menus / More 八个页签，覆盖 MAUI 控件 → HarmonyOS Handler 的映射。

## 运行

```powershell
dotnet build samples/dotnet/ControlsSampleApp -t:HarmonyRun
```

## 覆盖内容

- 基础控件、输入控件、Grid/Stack/Absolute 布局、CollectionView 列表、图片/绘制/WebView
- 菜单类控件，以及 TabbedPage / Shell / FlyoutPage 三种根页组合
- 每个页签的交互会写入 Home 页的 Activity Feed

## More 页

`FlyoutPage` / `Shell` 不能嵌套在 `TabbedPage` 中，因此 More 页用按钮替换根页来打开它们
（`FlyoutDemoPage` / `ShellDemoPage`），两个演示页都有返回按钮。

## 注意

- 本示例不依赖其他示例的资产（`Assets/probe_logo.png` 为本地副本）。
- 布局探针标记（`CS-*-MARK`）供 `scripts/verify-controls-sample.ps1` 使用。
