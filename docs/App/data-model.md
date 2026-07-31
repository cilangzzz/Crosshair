# CrosshairPro.App - 数据模型

## MainViewModel

主视图模型，管理页面导航和全局状态。

```csharp
public partial class MainViewModel : ObservableObject
{
    public CrosshairViewModel CrosshairViewModel { get; }
    public GamesViewModel GamesViewModel { get; }
    
    [ObservableProperty] private PageType _currentPage = PageType.Crosshair;
    
    public bool IsCrosshairPage => CurrentPage == PageType.Crosshair;
    public bool IsGamesPage => CurrentPage == PageType.Games;
}
```

### PageType 枚举

```csharp
public enum PageType
{
    Crosshair,  // 准心配置页面
    Games       // 游戏配置页面
}
```

### 属性说明

| 属性 | 类型 | 说明 |
|------|------|------|
| CrosshairViewModel | CrosshairViewModel | 准心配置页面 ViewModel |
| GamesViewModel | GamesViewModel | 游戏配置页面 ViewModel |
| CurrentPage | PageType | 当前页面 |
| IsCrosshairPage | bool | 是否显示准心页面 |
| IsGamesPage | bool | 是否显示游戏页面 |

### 事件

| 事件 | 说明 |
|------|------|
| ConfigUpdated | 配置更新事件（转发自 CrosshairViewModel） |
| ToggleCrosshairRequested | 切换准心显示请求 |
| SelectImageRequested | 选择自定义图片请求 |
| SavePresetRequested | 保存预设请求 |
| ImportPresetRequested | 导入预设请求 |
| ExportPresetRequested | 导出预设请求 |
| ToastRequested | Toast 通知请求 |

### 命令

| 命令 | 说明 |
|------|------|
| NavigateToCrosshairCommand | 导航到准心配置页面 |
| NavigateToGamesCommand | 导航到游戏配置页面 |

## CrosshairViewModel

准心配置页面 ViewModel，管理准心配置、预设、预览渲染。

```csharp
public partial class CrosshairViewModel : ObservableObject
{
    [ObservableProperty] private CrosshairConfig _config;
    [ObservableProperty] private bool _isCrosshairVisible = true;
    [ObservableProperty] private string _statusMessage = "准心已启用";
    [ObservableProperty] private string _currentPresetName = "默认配置";
    [ObservableProperty] private int _selectedStyleIndex;
    [ObservableProperty] private List<Preset> _presets = new();
    [ObservableProperty] private Preset? _selectedPreset;
}
```

### 属性说明

| 属性 | 类型 | 说明 |
|------|------|------|
| Config | CrosshairConfig | 当前准心配置 |
| IsCrosshairVisible | bool | 准心是否可见 |
| StatusMessage | string | 状态栏消息 |
| CurrentPresetName | string | 当前预设名称 |
| SelectedStyleIndex | int | 选中的样式索引 |
| Presets | List<Preset> | 预设列表 |
| SelectedPreset | Preset? | 当前选中的预设 |
| CrosshairStyleNames | string[] | 准心样式名称数组 |
| PresetColors | string[] | 预设颜色数组 |

### 事件

| 事件 | 说明 |
|------|------|
| ConfigUpdated | 配置更新事件，通知 OverlayWindow 重绘 |
| ToggleCrosshairRequested | 切换准心显示请求 |
| SelectImageRequested | 选择自定义图片请求 |
| SavePresetRequested | 保存预设请求 |
| ImportPresetRequested | 导入预设请求 |
| ExportPresetRequested | 导出预设请求 |
| ToastRequested | Toast 通知请求 |

### 命令

| 命令 | 说明 |
|------|------|
| SetColorCommand | 设置准心颜色 |
| SelectImageCommand | 选择自定义图片 |
| ToggleCrosshairCommand | 切换准心显示 |
| ResetConfigCommand | 重置配置为默认 |
| SavePresetCommand | 保存当前配置为预设 |
| ImportPresetCommand | 从文件导入预设 |
| ExportPresetCommand | 导出当前配置到文件 |
| DeletePresetCommand | 删除指定预设 |

### 业务方法

| 方法 | 说明 |
|------|------|
| SavePresetWithNameAsync(name) | 保存预设（由 MainWindow 调用） |
| ImportPresetFromFileAsync(filePath) | 从文件导入预设 |
| ExportPresetToFileAsync(filePath) | 导出当前配置到文件 |

## GamesViewModel

游戏配置页面 ViewModel，管理游戏特定配置。

```csharp
public partial class GamesViewModel : ObservableObject
{
    public ObservableCollection<GameProfile> Games { get; }
    
    [ObservableProperty] private GameProfile? _selectedGame;
    [ObservableProperty] private GameConfigStrategy? _currentStrategy;
    [ObservableProperty] private GameConfig? _currentConfig;
    [ObservableProperty] private bool _hasUnsavedChanges;
}
```

### 属性说明

| 属性 | 类型 | 说明 |
|------|------|------|
| Games | ObservableCollection<GameProfile> | 游戏列表 |
| SelectedGame | GameProfile? | 当前选中的游戏 |
| CurrentStrategy | GameConfigStrategy? | 当前游戏的配置策略 |
| CurrentConfig | GameConfig? | 当前游戏的配置 |
| HasUnsavedChanges | bool | 是否有未保存的更改 |

### 事件

| 事件 | 说明 |
|------|------|
| ToastRequested | Toast 通知请求 |

### 命令

| 命令 | 说明 |
|------|------|
| SaveConfigCommand | 保存配置 |
| ResetConfigCommand | 重置为默认配置 |
| ApplyConfigCommand | 应用配置到游戏 |

### 方法

| 方法 | 说明 |
|------|------|
| GetSettingValue(key) | 获取配置项的值 |
| SetSettingValue(key, value) | 设置配置项的值 |

## OverlayWindow

准心叠加窗口，全屏透明置顶显示准心。

### 关键属性

```csharp
public sealed class OverlayWindow : Window
{
    private readonly Canvas _canvas;           // 渲染画布
    private readonly CrosshairConfig _config;  // 配置副本
    private bool _isVisible = true;            // 可见性状态
    private IntPtr _hwnd;                      // 窗口句柄
}
```

### Win32 扩展样式

```csharp
private const int WS_EX_TRANSPARENT = 0x00000020;  // 鼠标穿透
private const int WS_EX_TOOLWINDOW = 0x00000080;   // 工具窗口
```

### 渲染方法

| 方法 | 说明 |
|------|------|
| UpdateConfig(config) | 更新准心配置 |
| ToggleVisibility() | 切换显示/隐藏 |
| RenderCrosshair() | 渲染准心到 Canvas |
| AddLine/AddDot/AddCircle/AddImage | 添加不同形状 |

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

## 设计令牌

### 颜色系统

| 令牌 | 值 | 用途 |
|------|------|------|
| BackgroundColor | #0D0D1A | 主背景 |
| SurfaceColor | #161628 | 卡片背景 |
| ControlColor | #1E1E36 | 控件背景 |
| ControlHoverColor | #262645 | 控件悬停 |
| BorderColor | #2A2A4A | 边框 |
| TextPrimaryColor | #F0F0F5 | 主要文本 |
| TextSecondaryColor | #9898B0 | 次要文本 |
| AccentColor | #00FF00 | 强调色（霓虹绿） |
| ErrorColor | #FF3366 | 错误色 |

### 字体系统

| 令牌 | 值 | 用途 |
|------|------|------|
| FontFamilyPrimary | Microsoft YaHei UI, Segoe UI | 主字体 |
| FontFamilyMono | Cascadia Code, Consolas | 等宽字体 |
| FontSizeDisplay | 24 | 显示文本 |
| FontSizeHeading | 16 | 标题 |
| FontSizeBody | 14 | 正文 |
| FontSizeCaption | 12 | 说明文本 |

### 间距系统

| 令牌 | 值 | 用途 |
|------|------|------|
| SpaceXs | 4 | 极小间距 |
| SpaceSm | 8 | 小间距 |
| SpaceMd | 16 | 中间距 |
| SpaceLg | 24 | 大间距 |
| SpaceXl | 32 | 极大间距 |

### 圆角系统

| 令牌 | 值 | 用途 |
|------|------|------|
| RadiusSm | 4 | 小圆角（按钮、输入框） |
| RadiusMd | 6 | 中圆角（卡片内部） |
| RadiusLg | 8 | 大圆角（卡片、对话框） |
