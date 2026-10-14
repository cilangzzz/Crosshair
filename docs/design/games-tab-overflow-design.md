# GamesPage 游戏 tab 溢出设计方案

> 状态：**设计稿（未实现）**。本文档为 GamesPage 内部游戏 tab 条在 tab 数量增长后的溢出处理方案，供后续迭代参考。
> 关联：[tab-navigation-design](tab-navigation-design.md)（左侧一级导航设计）、[App/README](../App/README.md)。

## 概述

GamesPage 顶部的游戏 tab 条当前以 `ItemsControl` + 横向 `StackPanel` 实现（`Views/GamesPage.xaml` L19-46），外层无滚动/换行容器——**tab 总宽超出可用宽度时直接被右侧裁剪**，无任何视觉提示，被裁掉的 tab 不可达。

随内置游戏与功能页（Apex、CS2、电脑优化，以及未来可能的 Valorant / OW2 / PUBG 等）持续增加，溢出是必然事件。本文档分析阈值并给出推荐方案（只设计，不实现）。

## 一、现状分析

### 1.1 布局结构

```
┌──────────────────────────────────────────────────────────────┐
│ 标题栏 (36px)                                                 │
├────┬─────────────────────────────────────────────────────────┤
│ 导 │  ┌ GamesPage 根 Grid Margin=20,12,20,0 ────────────────  │
│ 航 │  │ [CS2] [APEX LEGENDS] [电脑优化] …  ← tab 条 (0,0,0,16) │
│ 48 │  │ ─────────────────────────────────────────────────────  │
│ px │  │ ContentControl (CurrentGameId → GamePageSelector)     │
│    │  │ ┌ 卡片 Border (CardBorder) ─────────────────┐          │
│    │  │ └───────────────────────────────────────────┘          │
│    │  └────────────────────────────────────────────────────────┘
└────┴─────────────────────────────────────────────────────────┘
```

### 1.2 现有实现要点

| 项 | 现状 | 对溢出方案的影响 |
|----|------|------------------|
| 容器 | `ItemsControl` + `StackPanel Orientation=Horizontal` | 无滚动/换行能力，溢出即裁剪 |
| tab 宽度 | `Padding="12,6"` + 文本 + `Margin="0,0,8,0"`，随文本长度浮动 | 无法用固定值预估，需按实际宽度度量 |
| 选中态 | `GameProfile.IsSelected` DataTrigger（AccentTint 底 + Accent 前景） | 任何方案都必须保留该语义 |
| 数据源 | `GameProfile.BuiltIn.GetAll()` → `Games` 集合 | 顺序即展示顺序，无优先级字段可借用（`GameProfile.Priority` 存在但未接线） |
| 键盘/滚轮 | 无专门支持 | 推荐方案需补选中项 `BringIntoView` |

### 1.3 可用宽度估算

窗口 `MinWidth=800`（`MainWindow.xaml`），则：

```
tab 条可用宽度 = 窗口宽度 − 48（左侧导航栏）− 40（GamesPage 左右 Margin）− 2（导航栏右边框）
              ≈ 窗口宽度 − 90
```

单 tab 宽度经验值（Padding 24 + 文本 + 右间距 8，FontSize 12）：

| tab | 估算宽度 |
|-----|---------|
| Counter-Strike 2 | ≈ 118px |
| Apex Legends | ≈ 108px |
| 电脑优化 | ≈ 88px |
| （英文界面时）PC Optimization | ≈ 128px |

当前 3 个 tab 中文界面合计 ≈ 314px；默认窗口 900 宽 → 可用 ≈ 810px，**占用率 ≈ 39%**。

### 1.4 容纳能力估算（中文界面）

| 窗口宽度 | tab 条可用宽 | 可容纳 tab 数（按均值 ≈105px/tab） |
|---------|-------------|----------------------------------|
| 800（最小） | ≈710px | 6 |
| 900（默认） | ≈810px | 7 |
| 1200 | ≈1110px | 10 |
| 1920（最大化） | ≈1830px | 17 |

## 二、溢出阈值

- **触发条件（建议）**：tab 条占用率 > 90%，或 tab 数 ≥ 7（对应最小窗口）——二者取先达到者。
- 实现（若需要程序化判断）：监听 `ItemsControl.SizeChanged` 与集合变化，比较 `ItemsControl.ActualWidth` 求和与 `ScrollViewer.ViewportWidth`；或利用 `ScrollViewer.ScrollableWidth > 0` 判定。
- 设计要求：溢出前应预留**半个 tab 宽度的视觉提示**（渐隐边缘 / 「更多」按钮），避免用户误以为 tab 已穷尽。

## 三、候选方案对比

| 方案 | 交互 | 优点 | 缺点 | 复杂度 | 风险 |
|------|------|------|------|--------|------|
| **A. 横向滚动** | tab 条可左右滚动，边缘渐隐 + 可选左右步进按钮 | 改动最小（只换 ItemsPanel/加容器）；选中态与命令语义零改动；桌面端惯例（浏览器/IDE 同型）；支持触控板 | 被滚走的 tab 不可见；需补 `BringIntoView`；滚轮横向滚动易与页面滚动冲突需禁用 | 低 | 低 |
| **B. 溢出收纳** | 超出部分收进「更多 ▾」下拉 | 所有 tab 始终可达；视觉整洁 | 需宽度度量 + 动态计算收纳数量；下拉内选中态需额外 UI；实现与测试成本高 | 中高 | 中（度量时序、DPI） |
| **C. WrapPanel 换行** | 放不下自动换到第二行 | 无信息隐藏；实现简单（换 ItemsPanel 即可） | 垂直空间被挤占，破坏"tab 条单行"的页面节奏；两行时视觉重心不稳 | 低 | 低（仅观感风险） |
| **D. 紧凑图标化** | 每个 tab 缩为图标+短标题 | 容纳密度成倍提升 | 依赖图标设计资源；游戏名缩写可读性差；仍会溢出，只是延后 | 中 | 中 |
| **E. ComboBox 替代** | tab 条换为下拉选择器 | 一劳永逸 | 折叠了 tab 的一览性（无法横向对比/快速切换）；与现有交互习惯断裂 | 低 | 低（体验风险为主） |

## 四、推荐方案与实现草图

**推荐 A（横向滚动）**，理由：改动面最小（仅 tab 条容器）、`IsSelected`/`SelectGameCommand` 语义完全保留、符合桌面端惯例；B 可作为 A 之后 tab 数进一步增长（>10）的二期增强。

### 4.1 XAML 草图（替换 GamesPage.xaml L18 的 tab 条 StackPanel）

```xml
<Grid Grid.Row="0" Margin="0,0,0,16">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <!-- 可滚动 tab 条：边缘渐隐提示还有更多 -->
    <ScrollViewer Grid.Column="0" HorizontalScrollBarVisibility="Hidden"
                  VerticalScrollBarVisibility="Disabled"
                  x:Name="TabScroller">
        <ScrollViewer.OpacityMask>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
                <GradientStop Color="#FF000000" Offset="0"/>
                <GradientStop Color="#FF000000" Offset="0.97"/>
                <GradientStop Color="#00000000" Offset="1"/>
            </LinearGradientBrush>
        </ScrollViewer.OpacityMask>
        <ItemsControl ItemsSource="{Binding Games}">
            <!-- ItemsPanel / ItemTemplate 与现实现完全一致 -->
        </ItemsControl>
    </ScrollViewer>

    <!-- 二期可选：溢出收纳「更多 ▾」，ScrollableWidth>0 时显示 -->
    <Button Grid.Column="1" Content="▾" Visibility="Collapsed" x:Name="MoreTabsButton"/>
</Grid>
```

### 4.2 关键 C#

```csharp
// GamesPage.xaml.cs —— 选中项滚动可见 + 禁用滚轮抢占（滚轮留给页面内容）
private void TabScroller_OnScrollChanged(object sender, ScrollChangedEventArgs e)
{
    // ScrollableWidth > 0 时可显示 MoreTabsButton（二期）
}

// GamesViewModel 选中变化后（或页面 OnLoaded）：
TabScroller.UpdateLayout();
_selectedTabButton?.BringIntoView();   // 也可在 tab Button GotFocus/IsSelected 变化时调用
```

注意事项：
1. `ScrollViewer` 需禁用滚轮横向捕获（`ScrollViewer.HorizontalScrollBarVisibility="Hidden"` 且 WPF 滚轮默认走垂直，需显式处理 `PreviewMouseWheel` 转发或忽略）；
2. 渐隐 `OpacityMask` 右端 3% 仅为提示，若与「更多」按钮并存可移除；
3. 语言切换后 tab 宽度变化，需在 `Item[]` 通知后重新 `UpdateLayout` + `BringIntoView`。

### 4.3 分期建议

| 阶段 | 内容 | 触发条件 |
|------|------|---------|
| 一期 | 方案 A：ScrollViewer + 渐隐 + BringIntoView | tab 数 ≥ 6 或出现裁剪 |
| 二期 | 方案 B：二期「更多 ▾」收纳 + 溢出检测 | tab 数 ≥ 10 或用户反馈滚动不可接受 |

## 五、验收标准

| # | 项 | 标准 |
|---|----|------|
| 1 | 溢出行为 | tab 总宽 > 可用宽时可横向滚动，无 tab 被永久裁剪不可达 |
| 2 | 视觉提示 | 有溢出时右缘出现渐隐；无溢出时无渐隐 |
| 3 | 选中可见性 | 点击任意 tab 后其完整可见（含选中背景）；程序化选中（如恢复状态）后 `BringIntoView` 生效 |
| 4 | 滚轮/键盘 | tab 条不抢占页面垂直滚轮；Tab 键焦点导航可用 |
| 5 | 语言切换 | 中/英切换后宽度重排，选中项仍可见 |
| 6 | 回归 | 现有 3 tab 场景（800px 最小窗口）无滚动条、无渐隐，视觉与现状一致 |
