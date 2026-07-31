# CrosshairPro.App

表现层模块，提供 WPF 用户界面，包括主窗口、准心叠加窗口、页面导航、自定义控件和主题系统。

## 概述

App 模块是整个应用的用户界面层，负责：
- 应用程序启动和依赖注入配置
- 主窗口界面（MainWindow + MainViewModel）
- 页面导航系统（CrosshairPage、GamesPage）
- 准心叠加窗口（OverlayWindow）
- 自定义控件库（CrosshairPreview、DialogBase、ToastNotification、IconButton、TabNavItem）
- 主题系统（DesignTokens、ControlStyles、IconGeometries）

## 目录结构

```
CrosshairPro.App/
├── App.xaml.cs                 # 应用程序入口，DI 配置
├── App.xaml                    # 应用资源，全局样式
├── MainWindow.xaml.cs          # 主窗口代码后置
├── MainWindow.xaml             # 主窗口 XAML
├── StringToVisibilityConverter.cs  # 字符串到可见性转换器
├── ViewModels/
│   ├── MainViewModel.cs        # 主视图模型，页面导航
│   ├── CrosshairViewModel.cs   # 准心配置视图模型
│   └── GamesViewModel.cs       # 游戏配置视图模型
├── Views/
│   ├── OverlayWindow.cs        # 准心叠加窗口
│   ├── CrosshairPage.xaml.cs   # 准心配置页面
│   └── GamesPage.xaml.cs       # 游戏配置页面
├── Controls/
│   ├── CrosshairPreview.cs     # 准心预览控件
│   ├── DialogBase.cs           # 对话框基类
│   ├── ToastNotification.cs    # Toast 通知控件
│   ├── ToastManager.cs         # Toast 通知管理器
│   ├── IconButton.cs           # 图标按钮控件
│   ├── TabNavItem.cs           # 标签页导航项
│   └── PageTemplateSelector.cs # 页面模板选择器
├── Helpers/
│   └── ThemeHelper.cs          # 主题资源访问助手
├── Themes/
│   ├── DesignTokens.xaml       # 设计令牌（颜色、字体、间距）
│   ├── ControlStyles.xaml      # 控件样式定义
│   └── IconGeometries.xaml     # 图标几何数据
└── Assets/
    └── app-icon.ico            # 应用图标
```

## 核心组件

### App.xaml.cs

应用程序入口，负责：
- 配置依赖注入容器
- 注册所有服务（Singleton/Transient）
- 启动主窗口

```csharp
_services = new ServiceCollection()
    .AddCrosshairProServices()      // Application 层服务
    .AddSingleton<CrosshairViewModel>()  // 准心配置 ViewModel
    .AddSingleton<GamesViewModel>()      // 游戏配置 ViewModel
    .AddSingleton<MainViewModel>()       // 主 ViewModel
    .AddSingleton<OverlayWindow>()       // 准心窗口（单例）
    .AddTransient<MainWindow>()          // 主窗口（瞬态）
    .BuildServiceProvider();
```

### MainWindow

主窗口，包含：
- 左侧导航栏（TabNavItem 控件）
- 准心预览区域
- 配置控制面板（样式、大小、间隙、颜色等）
- 预设管理界面
- 系统托盘图标

### MainViewModel

主视图模型，使用 CommunityToolkit.Mvvm 实现 MVVM 模式：
- 管理页面导航（PageType 枚举）
- 持有 CrosshairViewModel 和 GamesViewModel
- 转发子 ViewModel 的事件

### 页面导航

应用采用页面导航架构：

| 页面 | ViewModel | 说明 |
|------|-----------|------|
| CrosshairPage | CrosshairViewModel | 准心配置页面，预览和控制面板 |
| GamesPage | GamesViewModel | 游戏配置页面，管理游戏特定配置 |

**PageType 枚举**：
```csharp
public enum PageType
{
    Crosshair,  // 准心配置页面
    Games       // 游戏配置页面
}
```

### CrosshairPage

准心配置页面：
- 左侧：准心预览区域（带网格）
- 右侧：配置控制面板（样式、大小、间隙、颜色等）
- 支持预设颜色选择和自定义颜色

### CrosshairViewModel

准心配置视图模型：
- 管理当前配置（CrosshairConfig）
- 处理预设加载/保存/导入/导出
- 响应用户操作命令
- 触发事件通知 UI 更新

### GamesPage

游戏配置页面：
- 游戏选择器（内置游戏列表）
- 启动项配置
- 游戏特定配置分区
- 保存/重置/应用按钮

### GamesViewModel

游戏配置视图模型：
- 管理游戏列表（ObservableCollection<GameProfile>）
- 加载/保存游戏特定配置
- 应用配置到游戏

### OverlayWindow

准心叠加窗口：
- 全屏透明置顶窗口
- 鼠标穿透（WS_EX_TRANSPARENT）
- 使用 WPF Shape 元素渲染准心
- 实时响应配置变更

## 依赖关系

```
App → Application → Services, Infrastructure
                   → Core
```

**依赖模块**：
- `CrosshairPro.Application` - 配置服务、预设服务、游戏配置服务
- `CrosshairPro.Services` - 配置仓库、渲染器实现
- `CrosshairPro.Infrastructure` - 热键管理、Win32 API
- `CrosshairPro.Core` - 模型、接口、枚举

**第三方依赖**：
- `CommunityToolkit.Mvvm` - MVVM 框架
- `Hardcodet.Wpf.TaskbarNotification` - 系统托盘
- `Microsoft.Extensions.DependencyInjection` - DI 容器

## 自定义控件

### CrosshairPreview

准心预览控件，在主窗口中显示准心预览。

```csharp
public class CrosshairPreview : Control
{
    public static readonly DependencyProperty ConfigProperty;
    public CrosshairConfig? Config { get; set; }
    
    protected override void OnRender(DrawingContext dc);
}
```

### TabNavItem

左侧导航栏图标项控件，支持选中态、悬停态、左侧指示条。

```csharp
public class TabNavItem : Button
{
    public static readonly DependencyProperty IconGeometryProperty;
    public static readonly DependencyProperty IsSelectedProperty;
    
    public string IconGeometry { get; set; }  // 图标几何资源键名
    public bool IsSelected { get; set; }      // 是否选中
}
```

**使用示例**：
```xml
<controls:TabNavItem IconGeometry="Crosshair"
                     IsSelected="{Binding IsCrosshairPage}"
                     Command="{Binding NavigateToCrosshairCommand}"/>
```

### IconButton

图标按钮控件，支持内置图标和自定义 Path Geometry。

```csharp
public class IconButton : Button
{
    public static readonly DependencyProperty IconGeometryProperty;
    public static readonly DependencyProperty IconSizeProperty;
    public static readonly DependencyProperty IconColorProperty;
    public static readonly DependencyProperty IconPositionProperty;
    public static readonly DependencyProperty IconSpacingProperty;
    public static readonly DependencyProperty ShowIconOnlyProperty;
}
```

### DialogBase

对话框基类，提供统一的对话框样式。

```csharp
public class DialogBase : Window
{
    public static readonly DependencyProperty TitleTextProperty;
    public static readonly DependencyProperty ShowCloseButtonProperty;
    public static readonly DependencyProperty ShowButtonsProperty;
    public static readonly DependencyProperty ConfirmButtonTextProperty;
    public static readonly DependencyProperty CancelButtonTextProperty;
    public static readonly DependencyProperty DialogContentProperty;
    
    public event RoutedEventHandler? Confirmed;
    public event RoutedEventHandler? Cancelled;
    
    public static DialogBase CreateInputDialog(string title, string prompt, string defaultValue);
    public string? GetInputResult();
}
```

### ToastNotification

Toast 通知控件，短暂显示后自动消失。

```csharp
public class ToastNotification : Control
{
    public static readonly DependencyProperty MessageProperty;
    public static readonly DependencyProperty DurationProperty;
    public static readonly DependencyProperty CornerRadiusProperty;
    
    public void Show(string message, int duration = 3);
    public void Hide();
    public static ToastNotification ShowIn(Panel container, string message, int duration = 3);
}
```

### ToastManager

Toast 通知管理器，创建独立的悬浮窗口显示通知。

```csharp
public static class ToastManager
{
    public static void Show(string message, int duration = 3, Window? owner = null);
    public static void CloseAll();
}
```

### PageTemplateSelector

页面模板选择器，根据 PageType 选择对应的 DataTemplate。

```csharp
public class PageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CrosshairTemplate { get; set; }
    public DataTemplate? GamesTemplate { get; set; }
    
    public override DataTemplate? SelectTemplate(object item, DependencyObject container);
}
```

## 辅助类

### ThemeHelper

主题资源访问助手，从 Application.Resources 获取主题颜色和样式。

```csharp
public static class ThemeHelper
{
    // 颜色
    public static Color BackgroundColor { get; }
    public static Color SurfaceColor { get; }
    public static Color ControlColor { get; }
    public static Color BorderColor { get; }
    public static Color AccentColor { get; }
    public static Color ErrorColor { get; }
    
    // 画刷
    public static SolidColorBrush BackgroundBrush { get; }
    public static SolidColorBrush SurfaceBrush { get; }
    public static SolidColorBrush ControlBrush { get; }
    public static SolidColorBrush TextPrimaryBrush { get; }
    public static SolidColorBrush TextSecondaryBrush { get; }
    public static SolidColorBrush AccentBrush { get; }
    
    // 字体
    public static FontFamily FontFamilyPrimary { get; }
    public static FontFamily FontFamilyMono { get; }
    
    // 圆角
    public static CornerRadius RadiusLg { get; }
    public static CornerRadius RadiusMd { get; }
    public static CornerRadius RadiusSm { get; }
}
```

### StringToVisibilityConverter

字符串到可见性转换器，非空字符串显示，空字符串或 null 隐藏。

```csharp
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
}
```

## 主题系统

### 设计令牌（DesignTokens.xaml）

定义应用的全局设计变量：

| 类别 | 令牌 | 值 |
|------|------|------|
| 背景色 | BackgroundColor | #0D0D1A |
| 表面色 | SurfaceColor | #161628 |
| 控件色 | ControlColor | #1E1E36 |
| 强调色 | AccentColor | #00FF00（霓虹绿） |
| 字体 | FontFamilyPrimary | Microsoft YaHei UI, Segoe UI |
| 字体 | FontFamilyMono | Cascadia Code, Consolas |
| 圆角 | RadiusSm/Md/Lg | 4/6/8 |

### 控件样式（ControlStyles.xaml）

自定义控件样式，统一外观：
- `PrimaryButton` - 主要操作按钮（绿色强调）
- `SecondaryButton` - 次要操作按钮
- `CustomSlider` - 自定义滑块
- `CustomTextBox` - 自定义文本框
- `CustomComboBox` - 自定义下拉框
- `CustomCheckBox` - 自定义复选框
- `CustomExpander` - 自定义展开器

## 详细文档

- [数据模型](data-model.md) - UI 组件和数据模型
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史
