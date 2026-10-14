# Tab 导航系统设计方案

## 概述

为 CrosshairPro 添加左侧图标导航栏，支持多页面切换：
- 准心配置页面（现有功能）
- 游戏配置页面（新功能，用于修改游戏画质文件、启动项等）

## 一、整体布局设计

### 1.1 窗口结构

```
┌─────────────────────────────────────────────────────────────────────┐
│ [×] CROSSHAIR PRO v1.0                                              │  Row 0: 标题栏 36px
├──┬──────────────────────────────────────────────────────────────────┤
│  │                                                                  │
│⌖ │                                                                  │
│  │                    内容区 (Content Area)                         │  Row 1: 主内容区 *
│⚙ │                                                                  │
│  │         根据选中的 Tab 显示不同页面内容                          │
│+ │                                                                  │
│  │                                                                  │
├──┴──────────────────────────────────────────────────────────────────┤
│  工具栏 (仅准心配置页显示)                                          │  Row 2: 工具栏 Auto
├─────────────────────────────────────────────────────────────────────┤
│  状态栏 (仅准心配置页显示)                                          │  Row 3: 状态栏 Auto
└─────────────────────────────────────────────────────────────────────┘
   48px
   导航栏
```

### 1.2 导航栏规格

| 属性 | 值 |
|------|-----|
| 宽度 | 48px |
| 背景 | `SurfaceBrush` |
| 边框 | 右侧 1px `BorderBrush` |
| 内边距 | 4px (上下) |
| 图标大小 | 20x20px |
| 按钮大小 | 40x40px |
| 按钮间距 | 4px |

### 1.3 导航项样式

```
默认状态：
┌────────┐
│        │  透明背景
│   ⌖    │  TextSecondary 颜色
│        │
└────────┘

选中状态：
┌────────┐
│▓▓▓     │  左侧 3px Accent 指示条
│▓⌖▓    │  Control 背景
│▓▓▓     │  Accent 颜色图标
└────────┘

Hover 状态：
┌────────┐
│░░░░░░░░│  ControlHover 背景
│░░⌖░░░░│  TextPrimary 颜色
│░░░░░░░░│
└────────┘
```

## 二、页面布局

### 2.1 准心配置页面 (CrosshairPage)

```
┌──────────────────────────────────────────────────────────────┐
│                    ┌─────────────────────┐                   │
│                    │                     │                   │
│                    │    PREVIEW          │                   │
│                    │    (准心预览)       │   ┌───────────┐   │
│                    │                     │   │ STYLE     │   │
│                    │                     │   │ [十字 ▼]  │   │
│                    └─────────────────────┘   ├───────────┤   │
│                                              │ SIZE      │   │
│                                              │ [====] 20 │   │
│                                              ├───────────┤   │
│                                              │ ...       │   │
│                                              └───────────┘   │
├──────────────────────────────────────────────────────────────┤
│  [PRESET ▼] [✎]                    [Save] [Reset] [Toggle]  │  ← 工具栏
├──────────────────────────────────────────────────────────────┤
│  准心已启用                              Ctrl+Shift+X Toggle │  ← 状态栏
└──────────────────────────────────────────────────────────────┘
```

**布局说明**：
- 左侧：预览区 (flexible width)
- 右侧：控制面板 (360px fixed width)
- 底部：工具栏 + 状态栏（属于 MainWindow，仅在此页显示）

### 2.2 游戏配置页面 (GamesPage)

```
┌──────────────────────────────────────────────────────────────┐
│  GAMES                                                       │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐   │
│  │ CS2    │ │Valorant│ │ Apex   │ │ OW2    │ │ PUBG   │   │  ← 游戏选择器
│  │  ✓     │ │        │ │        │ │        │ │        │   │
│  └────────┘ └────────┘ └────────┘ └────────┘ └────────┘   │
├──────────────────────────────────────────────────────────────┤
│  COUNTER-STRIKE 2                                   [重置]   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  LAUNCH OPTIONS                                             │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ -high -threads 12 -novid -nojoy                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  GRAPHICS SETTINGS                                          │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Fullscreen Mode        [Enabled ✓]                     │ │
│  │ Resolution             [1920x1080 ▼]                   │ │
│  │ Aspect Ratio           [16:9 ▼]                        │ │
│  │ Refresh Rate           [144 Hz ▼]                      │ │
│  │ Windowed Mode          [Borderless ▼]                  │ │
│  │ V-Sync                 [Disabled ✓]                    │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│                                    [Apply] [Reset Default]  │
└──────────────────────────────────────────────────────────────┘
```

**布局说明**：
- 顶部：游戏选择器（横向 Tab 列表）
- 内容区：当前选中游戏的配置项
- 全宽布局，充分利用空间

## 三、数据模型

### 3.1 导航状态

```csharp
// App/ViewModels/MainViewModel.cs
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private PageType _currentPage = PageType.Crosshair;
    
    public bool IsCrosshairPage => CurrentPage == PageType.Crosshair;
    public bool IsGamesPage => CurrentPage == PageType.Games;
    
    [RelayCommand]
    private void NavigateTo(PageType page) => CurrentPage = page;
}

public enum PageType
{
    Crosshair,
    Games
}
```

### 3.2 游戏配置模型（已存在，需扩展）

```csharp
// Core/Models/GameProfile.cs (已有)
public partial class GameProfile : ObservableObject
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string ProcessName { get; set; }
    // ... 已有字段
}

// Core/Models/GameConfig.cs (新增)
public partial class GameConfig : ObservableObject
{
    public string GameId { get; set; }
    public string LaunchOptions { get; set; } = "";
    public Dictionary<string, object> GraphicsSettings { get; set; } = new();
}

// Core/Models/GameConfigDefinition.cs (新增)
// 定义每个游戏支持哪些配置项
public class GameConfigDefinition
{
    public string GameId { get; set; }
    public List<ConfigSection> Sections { get; set; }
}

public class ConfigSection
{
    public string Name { get; set; }  // "启动项", "画质设置"
    public List<ConfigItemDefinition> Items { get; set; }
}

public class ConfigItemDefinition
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public ConfigItemType Type { get; set; }  // Bool, Int, Enum, String
    public string? DefaultValue { get; set; }
    public List<string>? Options { get; set; }  // 枚举选项
}
```

## 四、文件结构

### 4.1 新增文件

```
src/CrosshairPro.App/
├── Controls/
│   └── TabNavItem.cs              # 导航项控件
├── Views/
│   ├── CrosshairPage.xaml         # 准心配置页面
│   ├── CrosshairPage.xaml.cs
│   ├── GamesPage.xaml             # 游戏配置页面
│   └── GamesPage.xaml.cs
├── ViewModels/
│   ├── CrosshairViewModel.cs      # 准心页面 ViewModel (从 MainViewModel 拆分)
│   └── GamesViewModel.cs          # 游戏页面 ViewModel
└── Themes/
    └── ControlStyles.xaml         # 新增 TabNavItem 样式

src/CrosshairPro.Core/
└── Models/
    ├── GameConfig.cs               # 游戏配置模型
    └── GameConfigDefinition.cs    # 配置项定义

src/CrosshairPro.Application/
├── Interfaces/
│   └── IGameConfigService.cs      # 游戏配置服务接口
└── Services/
    └── GameConfigService.cs        # 游戏配置服务实现

src/CrosshairPro.Services/
└── Configuration/
    └── JsonGameConfigRepository.cs # 游戏配置持久化
```

### 4.2 修改文件

| 文件 | 修改内容 |
|------|----------|
| `MainWindow.xaml` | 添加导航栏，调整布局结构 |
| `MainWindow.xaml.cs` | 处理工具栏/状态栏显示逻辑 |
| `MainViewModel.cs` | 添加导航属性，拆分准心逻辑到 CrosshairViewModel |
| `IconGeometries.xaml` | 新增 Crosshair、Gamepad 图标 |
| `ServiceCollectionExtensions.cs` | 注册新服务 |

## 五、实现步骤

### 阶段一：导航框架（Day 1）

#### Step 1: 新增图标资源
- 文件: `Themes/IconGeometries.xaml`
- 内容:
  - `CrosshairIconGeometry` - 准心图标（十字 + 中心点）
  - `GamepadIconGeometry` - 游戏手柄图标

#### Step 2: 创建 TabNavItem 控件
- 文件: `Controls/TabNavItem.cs`
- 属性:
  - `IconGeometry: string` - 图标路径
  - `IsSelected: bool` - 选中状态
  - `Command: ICommand` - 点击命令
- 样式: 选中态（左侧指示条 + 背景）、Hover 动画

#### Step 3: 创建 CrosshairPage
- 文件: `Views/CrosshairPage.xaml(.cs)`
- 内容: 从 MainWindow.xaml 迁移控制面板部分
- 绑定: 使用 CrosshairViewModel

#### Step 4: 重构 MainViewModel
- 添加 `CurrentPage` 导航属性
- 拆分准心配置逻辑到 `CrosshairViewModel`
- 保留导航和全局状态管理

#### Step 5: 更新 MainWindow 布局
- 添加左侧导航栏 (48px)
- 内容区使用 `ContentControl` 或 Visibility 切换
- 工具栏/状态栏绑定 Visibility

### 阶段二：游戏配置页面（Day 2）

#### Step 6: 创建 GamesViewModel
- 游戏列表管理
- 当前选中游戏
- 配置项读写

#### Step 7: 创建 GamesPage
- 游戏选择器（横向 Tab）
- 配置项表单（启动项、画质设置）
- 应用/重置按钮

#### Step 8: 实现游戏配置服务
- `IGameConfigService` 接口
- `GameConfigService` 实现
- `JsonGameConfigRepository` 持久化

#### Step 9: 集成测试
- 页面切换流畅性
- 配置保存/加载
- 动画效果

## 六、技术细节

### 6.1 页面切换方式

**方案 A：Visibility 切换（推荐）**
```xml
<Grid>
    <views:CrosshairPage Visibility="{Binding IsCrosshairPage, Converter={StaticResource BoolToVisibility}}"/>
    <views:GamesPage Visibility="{Binding IsGamesPage, Converter={StaticResource BoolToVisibility}}"/>
</Grid>
```
优点：保持页面状态，切换快速

### 6.2 工具栏/状态栏显示控制

```xml
<!-- MainWindow.xaml -->
<Grid Grid.Row="2" Visibility="{Binding IsCrosshairPage, Converter={StaticResource BoolToVisibility}}">
    <!-- 工具栏内容 -->
</Grid>
```

### 6.3 导航项样式关键代码

```xml
<!-- ControlStyles.xaml -->
<Style x:Key="TabNavItem" TargetType="controls:TabNavItem">
    <Setter Property="Template">
        <ControlTemplate TargetType="controls:TabNavItem">
            <Grid>
                <!-- 左侧指示条 -->
                <Border x:Name="Indicator" Width="3" HorizontalAlignment="Left"
                        Background="Transparent" CornerRadius="0"/>
                <!-- 背景 -->
                <Border x:Name="Background" Background="Transparent" CornerRadius="4" Margin="4,2"/>
                <!-- 图标 -->
                <Path x:Name="Icon" Data="{TemplateBinding IconGeometry}"
                      Stretch="Uniform" Width="20" Height="20"/>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter TargetName="Indicator" Property="Background" Value="{StaticResource AccentBrush}"/>
                    <Setter TargetName="Background" Property="Background" Value="{StaticResource ControlBrush}"/>
                    <Setter TargetName="Icon" Property="Fill" Value="{StaticResource AccentBrush}"/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>
    </Setter>
</Style>
```

## 七、验收标准

| 功能 | 验收条件 |
|------|----------|
| 导航栏显示 | 左侧 48px 宽，包含准心/游戏图标 |
| 页面切换 | 点击导航项切换页面，有选中态指示 |
| 准心页面 | 功能与现有一致，预览 + 控制面板 |
| 游戏页面 | 游戏选择器 + 配置表单，能保存配置 |
| 工具栏/状态栏 | 仅在准心页面显示 |
| 动画效果 | Hover、选中有流畅的过渡动画 |