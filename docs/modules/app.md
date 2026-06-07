# App 应用层文档

## 概述

`CrosshairPro.App` 是项目的 WPF 应用层，负责用户界面展示和交互。包含主窗口、覆盖窗口、视图模型和自定义控件。

**项目路径**：`src/CrosshairPro.App/`

**目标框架**：net8.0-windows

**依赖**：CrosshairPro.Services, CrosshairPro.Infrastructure, CrosshairPro.Core

---

## 目录结构

```
CrosshairPro.App/
├── App.xaml                      # 应用资源和全局样式
├── App.xaml.cs                   # 应用入口（空）
├── AssemblyInfo.cs               # 程序集信息
├── MainWindow.xaml               # 主窗口 XAML
├── MainWindow.xaml.cs            # 主窗口代码
├── ViewModels/
│   └── MainViewModel.cs          # 主视图模型
├── Views/
│   └── OverlayWindow.cs          # 覆盖窗口
├── Controls/
│   └── CrosshairPreview.cs       # 准星预览控件
├── Converters/                   # 值转换器（待实现）
├── Themes/                       # 主题资源（待实现）
└── Assets/                       # 静态资源（待实现）
```

---

## App.xaml

应用资源定义文件，包含全局样式和转换器。

**文件**：`src/CrosshairPro.App\App.xaml`

### 全局资源

```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- 全局字体 -->
        <FontFamily x:Key="DefaultFont">Microsoft YaHei UI, Segoe UI</FontFamily>

        <!-- 布尔值到可见性转换器 -->
        <BooleanToVisibilityConverter x:Key="BoolToVisibilityConverter" />
    </ResourceDictionary>
</Application.Resources>
```

---

## MainWindow

主窗口是应用程序的主界面，包含准星预览和参数调节面板。

**文件**：`src/CrosshairPro.App\MainWindow.xaml` + `MainWindow.xaml.cs`

### UI 布局

```
┌─────────────────────────────────────────────────────────────┐
│                        MainWindow                           │
├──────────────────────┬──────────────────────────────────────┤
│                      │                                      │
│   准星预览区域        │        参数调节面板                    │
│   (Canvas)           │                                      │
│                      │  ┌──────────────────────────────┐   │
│   ┌──────────────┐   │  │ 样式选择 (ComboBox)          │   │
│   │              │   │  └──────────────────────────────┘   │
│   │    十字准星    │   │  ┌──────────────────────────────┐   │
│   │              │   │  │ 大小 (Slider + TextBox)       │   │
│   └──────────────┘   │  │ 间距 (Slider + TextBox)       │   │
│                      │  │ 粗细 (Slider + TextBox)       │   │
│                      │  │ 透明度 (Slider + TextBox)     │   │
│                      │  │ 中心点 (Slider + TextBox)     │   │
│                      │  └──────────────────────────────┘   │
│                      │  ┌──────────────────────────────┐   │
│                      │  │ 颜色选择 (RadioButton x 8)    │   │
│                      │  └──────────────────────────────┘   │
│                      │  ┌──────────────────────────────┐   │
│                      │  │ 高级设置 (Expander)           │   │
│                      │  │  ├ 描边启用/粗细              │   │
│                      │  │  └ 阴影启用                   │   │
│                      │  └──────────────────────────────┘   │
├──────────────────────┴──────────────────────────────────────┤
│                     底部工具栏                               │
│  预设选择  保存  导入  导出  重置  切换准星                    │
├─────────────────────────────────────────────────────────────┤
│                     状态栏                                   │
└─────────────────────────────────────────────────────────────┘
```

### 样式定义

MainWindow 定义了 4 个本地样式：

| 样式名 | 目标控件 | 说明 |
|--------|----------|------|
| PrimaryButton | Button | 绿色主按钮（#00FF00） |
| SecondaryButton | Button | 次要按钮（#3d3d5c） |
| CustomSlider | Slider | 自定义滑块样式 |
| CustomTextBox | TextBox | 自定义文本框样式 |

### 颜色方案

- 背景色：#1a1a2e（深蓝黑色）
- 主色调：#00FF00（绿色）
- 次要色：#3d3d5c（深紫色）
- 文字色：#ffffff（白色）

### 代码逻辑（MainWindow.xaml.cs）

#### 初始化流程

```csharp
public MainWindow()
{
    InitializeComponent();

    // 1. 创建热键管理器
    _hotkeyManager = new HotkeyManager();

    // 2. 创建视图模型
    _viewModel = new MainViewModel();
    DataContext = _viewModel;

    // 3. 创建覆盖窗口
    _overlayWindow = new OverlayWindow();
    _overlayWindow.Show();

    // 4. 订阅事件
    _viewModel.ConfigUpdated += OnConfigUpdated;
    _hotkeyManager.HotkeyTriggered += OnHotkeyTriggered;

    // 5. 注册热键
    RegisterDefaultHotkeys();

    // 6. 初始绘制预览
    DrawPreview();
}
```

#### DrawPreview 方法

在预览 Canvas 上绘制准星。

```csharp
private void DrawPreview()
{
    PreviewCanvas.Children.Clear();

    var config = _viewModel.Config;
    double centerX = PreviewCanvas.ActualWidth / 2;
    double centerY = PreviewCanvas.ActualHeight / 2;

    // 根据 config.Style 绘制不同样式的准星
    switch (config.Style)
    {
        case CrosshairStyle.Cross:
            DrawCrossPreview(config, centerX, centerY);
            break;
        case CrosshairStyle.Dot:
            DrawDotPreview(config, centerX, centerY);
            break;
        // ... 其他样式
    }
}
```

#### 热键处理

```csharp
private void OnHotkeyTriggered(object? sender, HotkeyBinding binding)
{
    switch (binding.Action)
    {
        case HotkeyAction.ToggleCrosshair:
            _overlayWindow.ToggleVisibility();
            break;
        case HotkeyAction.IncreaseSize:
            _viewModel.Config.Size = Math.Min(100, _viewModel.Config.Size + 5);
            break;
        case HotkeyAction.DecreaseSize:
            _viewModel.Config.Size = Math.Max(1, _viewModel.Config.Size - 5);
            break;
    }
}
```

#### 配置同步

```csharp
private void OnConfigUpdated(object? sender, EventArgs e)
{
    // 同步配置到覆盖窗口
    _overlayWindow.UpdateConfig(_viewModel.Config);

    // 重新绘制预览
    DrawPreview();
}
```

---

## OverlayWindow

覆盖窗口是一个透明、无边框、置顶、不可聚焦、点击穿透的窗口，用于在游戏上方显示准星。

**文件**：`src/CrosshairPro.App\Views\OverlayWindow.cs`

### 窗口特性

| 特性 | 实现方式 |
|------|----------|
| 透明背景 | `AllowsTransparency = true`, `Background = Transparent` |
| 无边框 | `WindowStyle = None` |
| 始终置顶 | `Topmost = true` |
| 全屏覆盖 | `WindowState = Maximized` |
| 点击穿透 | Win32 `WS_EX_TRANSPARENT` |
| 不在任务栏显示 | `ShowInTaskbar = false` |

### 初始化流程

```csharp
public OverlayWindow()
{
    // 设置窗口属性
    AllowsTransparency = true;
    WindowStyle = WindowStyle.None;
    Background = Brushes.Transparent;
    Topmost = true;
    WindowState = WindowState.Maximized;
    ShowInTaskbar = false;

    // 创建渲染画布
    _canvas = new Canvas();
    Content = _canvas;

    Loaded += (s, e) =>
    {
        // 使用 Win32 API 设置点击穿透
        var hwnd = new WindowInteropHelper(this).Handle;
        Win32Methods.SetWindowTransparentClickable(hwnd);
    };
}
```

### 配置更新

```csharp
public void UpdateConfig(CrosshairConfig config)
{
    // 深拷贝配置（避免引用问题）
    _config.CopyFrom(config);

    // 重新渲染准星
    RenderCrosshair();
}
```

### 准星渲染

OverlayWindow 使用 WPF Shape 元素（Line、Ellipse）在 Canvas 上绘制准星，而不是 DrawingContext。

```csharp
private void RenderCrosshair()
{
    _canvas.Children.Clear();

    switch (_config.Style)
    {
        case CrosshairStyle.Cross:
            RenderCross();
            break;
        case CrosshairStyle.Dot:
            RenderDot();
            break;
        case CrosshairStyle.Circle:
            RenderCircle();
            break;
        case CrosshairStyle.TShape:
            RenderTShape();
            break;
        case CrosshairStyle.XShape:
            RenderXShape();
            break;
        case CrosshairStyle.CustomImage:
            RenderCustomImage();
            break;
    }
}
```

### 可见性切换

```csharp
public void ToggleVisibility()
{
    if (IsVisible)
    {
        Hide();
    }
    else
    {
        Show();
    }

    CrosshairVisibilityChanged?.Invoke(this, IsVisible);
}
```

---

## MainViewModel

主视图模型，使用 CommunityToolkit.Mvvm 的源生成器实现 MVVM 模式。

**文件**：`src/CrosshairPro.App\ViewModels\MainViewModel.cs`

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| Config | CrosshairConfig | 准星配置 |
| IsCrosshairVisible | bool | 准星是否可见 |
| StatusMessage | string | 状态栏消息 |
| CurrentPresetName | string | 当前预设名称 |
| SelectedStyleIndex | int | 选中的样式索引 |
| CrosshairStyleNames | string[] | 样式名称数组 |
| PresetColors | string[] | 预设颜色数组 |

### 命令

| 命令 | 参数 | 说明 |
|------|------|------|
| SetColor | string (颜色值) | 设置准星颜色 |
| ToggleCrosshair | - | 切换准星显示/隐藏 |
| ResetConfig | - | 重置配置为默认值 |

### 事件

| 事件 | 说明 |
|------|------|
| ConfigUpdated | 配置更新时触发 |

### 属性变更监听

MainViewModel 订阅了 Config 及其嵌套对象的 PropertyChanged 事件，确保任何属性变更都能触发 ConfigUpdated 事件。

```csharp
public MainViewModel()
{
    _config = new CrosshairConfig();

    // 监听主配置变更
    _config.PropertyChanged += OnConfigPropertyChanged;

    // 监听效果配置变更
    _config.Effects.Outline.PropertyChanged += OnConfigPropertyChanged;
    _config.Effects.Shadow.PropertyChanged += OnConfigPropertyChanged;
    _config.Effects.Glow.PropertyChanged += OnConfigPropertyChanged;
}
```

### 样式索引映射

```csharp
// SelectedStyleIndex 与 CrosshairStyle 枚举的映射
// 0 = Cross, 1 = Dot, 2 = Circle, 3 = TShape, 4 = XShape, 5 = CustomImage
```

---

## CrosshairPreview

准星预览自定义控件，使用 DrawingContext 绘制准星预览。

**文件**：`src/CrosshairPro.App\Controls\CrosshairPreview.cs`

### 功能特性

- 自定义 WPF Control
- Config 依赖属性
- 1.5x 缩放预览
- 网格背景
- 支持描边效果

### 依赖属性

| 属性 | 类型 | 说明 |
|------|------|------|
| Config | CrosshairConfig | 准星配置 |

### 渲染逻辑

```csharp
protected override void OnRender(DrawingContext drawingContext)
{
    // 1. 绘制网格背景
    DrawGridBackground(drawingContext);

    // 2. 应用 1.5x 缩放
    var scale = new ScaleTransform(1.5, 1.5);

    // 3. 根据 Config.Style 绘制准星
    switch (Config.Style)
    {
        case CrosshairStyle.Cross:
            DrawCrossPreview(drawingContext);
            break;
        // ... 其他样式
    }
}
```

### 当前状态

⚠️ **注意**：CrosshairPreview 控件已实现但**未被使用**。当前 MainWindow 直接在 Canvas 上绘制预览。

---

## 待实现模块

### Converters

值转换器，计划实现：
- ColorToBrushConverter：颜色字符串转画刷
- EnumToStringConverter：枚举转显示字符串
- BoolToOppositeVisibilityConverter：布尔值反转可见性

### Themes

主题资源，计划实现：
- 暗色主题样式
- 控件模板
- 动画资源

### Assets

静态资源，计划实现：
- 应用图标
- 准星样式图标
- 按钮图标
