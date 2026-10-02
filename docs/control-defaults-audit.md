# 控件默认值审计（MAUI ↔ ArkUI 差异映射）

> 生成依据：DevEco Studio SDK `component/*.d.ts` 的 `Default value` 标注 + Handler 源码逐一比对。
> 分类说明：
> - **A 类**：ArkUI 已有合理默认，Handler 不覆写（颜色 `null` 不设、FontSize `<=0` 不设）
> - **B 类**：MAUI 语义与 ArkUI 冲突，Handler 显式设置 MAUI 语义值
> - **C 类**：MAUI sentinel 值（`0`、`-1`、`null`）需正确处理，不传给 ArkUI

## Button 字号验收标准

- MAUI `Button.FontSize=0`（Default）必须显式映射到 `HarmonyControlDefaults.ButtonFontSize`（当前为 `16fp`）。
- 不得按 ArkTS 声明式 API 的 `16fp` 默认值推断 NDK C API 也有默认字号；当前 NDK 节点无样式表，缺省会导致按钮子 `Text` 零尺寸不可见。
- HelloApp 回归优先使用 `uitest dumpLayout` 做结构验收：按钮节点必须存在，且 `visible=true`、`clickable=true`、bounds 非零、子 `Text` 存在。截图仅作为可选的视觉样式补充。

## 文本与按钮

| Handler | 属性 | 分类 | ArkUI 原生默认 | MAUI 默认 | Handler 行为 |
|---|---|---|---|---|---|
| Label | FontSize | C | `16fp` | `0`（平台默认） | `>0` 时设，`<=0` 不设 ✅ |
| Label | TextColor | A | 主题色 `#e6182431` | `null` | `null` 不设，主题色生效 ✅ |
| Label | TextAlign | A | `Start` | `Start` | 一致，不覆写 ✅ |
| Label | LineBreakMode | B | `Clip` | `WordWrap` | `WordWrap` 走 switch default（不设），截断模式设 MaxLines ✅ |
| Label | MaxLines | C | 不限制 | `-1` | `>=0` 时设 ✅ |
| Button | FontSize | C | `16fp`（仅 ArkTS 声明式 API；NDK 无样式表） | `0` | `>0` 时设；`=0` 显式映射 `HarmonyControlDefaults.ButtonFontSize`（`16fp`） ✅ |
| Button | TextColor | A | `$r('sys.color.font_on_primary')` | `null` | `null` 不设 ✅ |
| Button | CornerRadius | C | `ROUNDED_RECTANGLE` 自带 20vp | `-1` | `>=0` 时设 ✅ |
| Button | BorderWidth | C | 无边框 | `-1` | `>=0` 时设 ✅ |
| Button | BackgroundColor | A | 主题色 | `null` | `null` 不设 ✅ |
| Button | BorderColor | A | 主题色 | `null` | `null` 不设 ✅ |

## 输入控件

| Handler | 属性 | 分类 | ArkUI 原生默认 | MAUI 默认 | Handler 行为 |
|---|---|---|---|---|---|
| Entry | FontSize | C | `16fp` | `0` | `>0` 时设 ✅ |
| Entry | TextColor | A | 主题色 | `null` | `null` 不设 ✅ |
| Entry | PlaceholderColor | A | 主题占位色 | `null` | `null` 不设 ✅ |
| Entry | IsPassword | A | `NORMAL` | `false` | 一致，直接映射 ✅ |
| Editor | FontSize | C | `16fp` | `0` | `>0` 时设（已修复，原 `>=0`） ✅ |
| Editor | TextColor | A | 主题色 | `null` | `null` 不设 ✅ |
| Editor | PlaceholderColor | A | 主题占位色 | `null` | `null` 不设 ✅ |
| Picker | ItemsSource | - | - | - | 直接映射（无默认值问题） ✅ |

## 开关与选择

| Handler | 属性 | 分类 | ArkUI 原生默认 | MAUI 默认 | Handler 行为 |
|---|---|---|---|---|---|
| Switch | OnColor | A | 主题色 | `null` | `null` 不设 ✅ |
| Switch | ThumbColor | A | 主题色 | `null` | `null` 不设 ✅ |
| CheckBox | Color | A | 主题色 | `null` | `null` 不设 ✅ |
| RadioButton | (无颜色映射) | A | 主题色 | - | 不映射颜色属性，原生默认 ✅ |
| Slider | Min/Max | A | `0..1` | `0..1` | 一致，直接映射 ✅ |
| Slider | TrackColor/ThumbColor | A | 主题色 | `null` | `null` 不设 ✅ |
| ProgressBar | Progress | A | `0` | `0` | 一致，直接映射 ✅ |

## 布局与容器

| Handler | 属性 | 分类 | ArkUI 原生默认 | MAUI 默认 | Handler 行为 |
|---|---|---|---|---|---|
| StackLayout | Spacing | B | `0` | `6` | `>0` 时叠加到子节点 Margin ✅ |
| Grid/AbsoluteLayout | (托管布局) | - | - | - | MAUI 托管布局引擎，不走 ArkUI flex ✅ |
| ScrollView | Orientation | A | `Vertical` | `Vertical` | 一致 ✅ |
| ContentView | Padding | - | - | `0` | 直接映射 ✅ |
| Border/Frame | CornerRadius | C | `0` | `-1` | `>=0` 时设 ✅ |
| Border/Frame | BorderWidth | C | `0` | `-1` | `>=0` 时设 ✅ |

## 图片与集合

| Handler | 属性 | 分类 | ArkUI 原生默认 | MAUI 默认 | Handler 行为 |
|---|---|---|---|---|---|
| Image | Aspect | A | `CONTAIN` | `AspectFit` | 一致（`AspectFit→CONTAIN`） ✅ |
| Image | Source | - | - | - | 直接映射 ✅ |
| CollectionView | ItemsLayout | A | `List(Vertical)` | `Linear(Vertical)` | 一致 ✅ |
| CarouselView | (轮播) | - | - | - | Swiper 映射 ✅ |

## 页面与导航

| Handler | 属性 | 分类 | 说明 |
|---|---|---|---|
| ContentPage | BackgroundColor | A | `null` 不设，ArkUI 页面默认背景生效 ✅ |
| NavigationPage | BarBackground | A | `null` 不设，走 `HarmonyShellTheme.TopBarBackground` ✅ |
| Shell | 全部 | - | 走 `HarmonyShellTheme`（专用主题中心） ✅ |
| TabbedPage | 全部 | - | 走 `HarmonyShellTheme`（专用主题中心） ✅ |

## 修复记录

| 日期 | Handler | 修复内容 |
|---|---|---|
| 2026-10-01 | Button | ~~`MapFontSize` 移除硬编码 `16f` 兜底~~ → 实测回归（按钮文字零尺寸不可见）后**回滚**。最终验收标准：`FontSize=0` 显式映射到 `HarmonyControlDefaults.ButtonFontSize`（`16fp`）；ArkUI 原生 `16fp` 默认仅存在于 ArkTS 声明式 API，NDK C API 节点无样式表。结构回归使用 `dumpLayout` 验证按钮节点、bounds 与子 `Text` |
| 2026-10-01 | Editor | `MapFontSize` 条件从 `>=0` 改为 `>0`，`FontSize=0` 不再传给 ArkUI |
