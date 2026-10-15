# GamesPage 游戏选择器设计（tab 溢出方案的最终决策）

> 状态：**已实现（2026-10-14）**。本文档原为 tab 溢出设计稿（方案 A 横向滚动），实现前经用户决策改为 **方案 E：ComboBox 选择器整替 tab 条**，并已落地。文档保留原对比分析作为决策依据。
> 关联：[tab-navigation-design](tab-navigation-design.md)（左侧一级导航设计）、[App/README](../App/README.md)。

## 概述

GamesPage 顶部原先以 `ItemsControl` + 横向 `StackPanel` 实现游戏 tab 条，外层无滚动/换行容器——tab 总宽超出可用宽度时直接被右侧裁剪。随内置游戏与功能页（Apex、CS2、电脑优化，以及未来可能的 Valorant / OW2 / PUBG 等）持续增加，溢出是必然事件。

本设计文档最初推荐方案 A（横向滚动）。实现前用户提出「如果滚动还是麻烦，选择切换是不是更好」，并确认**未来内置游戏页预计 10+ 或持续增加**——据此改选方案 E（ComboBox 选择器）并直接实现。

## 一、决策依据

### 1.1 tab 范式的容量天花板（原分析保留）

窗口 `MinWidth=800`，tab 条可用宽度 ≈ 窗口宽度 − 90px；单 tab 宽 ≈ 105px（均值）：

| 窗口宽度 | 可容纳 tab 数 |
|---------|--------------|
| 800（最小） | 6 |
| 900（默认） | 7 |
| 1200 | 10 |
| 1920（最大化） | 17 |

tab 是"一览 + 快速切换"范式：个数少时优势明显，个数多时一览性消失、只剩溢出管理成本。**10+ 的预期规模下 tab 范式不成立**，任何溢出方案（滚动/收纳/换行）都只是延缓。

### 1.2 候选方案对比（决策时评估，E 为最终选择）

| 方案 | 交互 | 优点 | 缺点 | 复杂度 | 结论 |
|------|------|------|------|--------|------|
| A. 横向滚动 | tab 条左右滚动 + 边缘渐隐 | 改动最小；语义零改动 | 10+ 规模下滚动定位繁琐；一览性同样丧失 | 低 | 否决（规模不匹配） |
| B. 溢出收纳 | 超出部分进「更多 ▾」 | tab 均可达 | 度量时序/DPI 复杂度高 | 中高 | 否决 |
| C. WrapPanel 换行 | 放不下换行 | 简单 | 垂直空间被挤占，破坏页面节奏 | 低 | 否决 |
| D. 紧凑图标化 | 图标+短标题 | 密度提升 | 依赖图标资源；仍会溢出 | 中 | 否决 |
| **E. ComboBox 选择器** | tab 条换为「游戏」标签 + 下拉选择框 | 一劳永逸容纳任意规模；实现简单；与 MainWindow 预设选择器交互形态一致 | 单击游戏无法一眼看到全部（打开下拉才可见） | 低 | **✅ 采用** |

决策关键：用户确认未来游戏规模 10+ → tab 一览性价值随规模递减至归零，选择器的"隐藏列表"代价反而最小（下拉一次可见全部，且无布局上限）。

## 二、实现说明

### 2.1 XAML（`Views/GamesPage.xaml`）

tab 条（`ItemsControl` + Button + IsSelected DataTrigger）整体移除，替换为：

```xml
<!-- 游戏选择器（ComboBox 形态：预期游戏 10+，tab 条范式不适用） -->
<StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,16">
    <TextBlock Text="{loc:Loc Key=Games.SelectorLabel}"
               Style="{StaticResource SectionHeader}"
               VerticalAlignment="Center" Margin="0,0,8,0"/>
    <ComboBox Width="200"
              Style="{StaticResource CustomComboBox}"
              ItemsSource="{Binding Games}"
              SelectedItem="{Binding SelectedGame, Mode=TwoWay}">
        <ComboBox.ItemTemplate>
            <DataTemplate>
                <TextBlock>
                    <TextBlock.Text>
                        <MultiBinding Converter="{StaticResource GameProfileTitleConverter}">
                            <Binding Path="DisplayName"/>
                            <Binding Path="DisplayNameLocKey"/>
                            <Binding Path="CurrentCulture" Source="{x:Static loc:LocalizationProvider.Instance}"/>
                        </MultiBinding>
                    </TextBlock.Text>
                </TextBlock>
            </DataTemplate>
        </ComboBox.ItemTemplate>
    </ComboBox>
</StackPanel>
```

下方内容容器（`ContentControl` + `GamePageSelector`）不变。

要点：

1. **选中项显示与下拉项共用同一 ItemTemplate**：WPF 将 ItemTemplate 传播给折叠态选区框（`SelectionBoxItemTemplate`），无需单独写选区显示模板。
2. **GameProfileTitleConverter 复用**：MultiBinding `[0]` DisplayName 回退、`[1]` DisplayNameLocKey、`[2]` CurrentCulture（语言切换重算触发器，`Source={x:Static loc:LocalizationProvider.Instance}`）。
3. `GameProfile.IsSelected` 标志仍由 ViewModel 维护（见 2.2），但 UI 不再消费（原 tab DataTrigger 已删）；保留以便未来恢复 tab 形态或其他选中态 UI 复用。

### 2.2 ViewModel（`ViewModels/GamesViewModel.cs`）

- 删除 `SelectGameCommand`（`[RelayCommand] SelectGame`）——唯一调用方是被移除的 tab 按钮；`SelectGameInternal` 收为 private。
- 保留 `[ObservableProperty] SelectedGame`，ComboBox `SelectedItem` 双向绑定；页面切换仍走 `OnSelectedGameChanged` → 同步 `IsSelected` 标志 + `CurrentGameId` → `GamePageSelector`。
- 默认选中 `builtin-apex` 逻辑不变。

### 2.3 本地化

新增键 `Games.SelectorLabel`（选择器左侧标签）：`Strings.resx`/`Strings.zh-CN.resx` = `游戏`，`Strings.en.resx` = `Game`。游戏名本身经 GameProfileTitleConverter 本地化（`Games.PcOptimize` 等 LocKey），CS2/Apex 为专有名词不翻译。

## 三、验证记录（2026-10-14，UIA + 截图像素分析）

| # | 项 | 结果 |
|---|----|------|
| 1 | 下拉项渲染 | 3 项内文本正确：Counter-Strike 2 / Apex Legends / 电脑优化（中文界面） |
| 2 | 选择切换 | UIA SelectionItemPattern 选中 CS2 → 页面内容切换为 CS2 配置页 |
| 3 | 选区框内容随选择更新 | 截图像素签名随选中项变化（CS2 文本带 ≈136px，Apex ≈116px，差分 1179px） |
| 4 | 语言切换 | 选「电脑优化」后切 EN：选区框重渲染（差分 1360px，→ "PC Optimization"），标签「游戏」→ "Game" |
| 5 | 键盘/滚轮 | ComboBox 原生行为（方向键/回车选择，无自定义滚动冲突面） |

### 坑点：UIA 折叠态选区框子树为空（自动化假象）

用 UIAutomation 检查折叠态 ComboBox 时，**选区框（ContentPresenter 内部）的 TextBlock 不出现在 UIA 子树中**（子元素计数为 0），极易误判为"选区框未渲染"。实际可视树中模板内容正常存在。

判定手段：`System.Drawing.CopyFromScreen` 截取选区框区域做像素分析——文本行表现为垂直居中的亮像素带（暗色主题下亮度 >180 的像素成行分布）；空框亮像素为 0。同法可对比两次截图差分验证内容更新。`.claude/temp/` 曾有探针脚本（combo-repro / combo-shot / combo-lang），已随验证完成清理。

## 四、验收标准（全部已验证）

| # | 项 | 标准 | 状态 |
|---|----|------|------|
| 1 | 规模承载 | 选择器容纳任意数量游戏，无布局上限 | ✅（结构上无上限） |
| 2 | 选中显示 | 折叠态显示当前游戏本地化标题 | ✅ |
| 3 | 页面联动 | 选择变化 → CurrentGameId → GamePageSelector 切页 | ✅ |
| 4 | 语言切换 | 中/英切换后标签、下拉项、选区框全部重算 | ✅ |
| 5 | 回归 | 内容容器/子页（Apex/CS2/PcOptimize）行为不变 | ✅ |
