# CrosshairPro.App - 数据模型

## MainViewModel 状态

MainViewModel 管理的核心状态：

```csharp
public partial class MainViewModel : ObservableObject
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

## 命令列表

| 命令 | 说明 |
|------|------|
| SetColorCommand | 设置准心颜色 |
| SelectImageCommand | 选择自定义图片 |
| ToggleCrosshairCommand | 切换准心显示/隐藏 |
| ResetConfigCommand | 重置为默认配置 |
| SavePresetCommand | 保存当前配置为预设 |
| ImportPresetCommand | 导入预设 |
| ExportPresetCommand | 导出当前配置 |
| DeletePresetCommand | 删除选中预设 |

## 事件列表

| 事件 | 说明 |
|------|------|
| ConfigUpdated | 配置变更通知 |
| ToggleCrosshairRequested | 切换准心请求 |
| SelectImageRequested | 选择图片请求（触发 OpenFileDialog） |
| SavePresetRequested | 保存预设请求（触发命名对话框） |
| ImportPresetRequested | 导入预设请求（触发 OpenFileDialog） |
| ExportPresetRequested | 导出配置请求（触发 SaveFileDialog） |

## 预设颜色

`PresetColors` 提供快捷颜色选择：

```csharp
public string[] PresetColors { get; } = new[]
{
    "#00FF00", // 绿色
    "#00FFFF", // 青色
    "#FFFF00", // 黄色
    "#FF0000", // 红色
    "#FF00FF", // 紫色
    "#FFA500", // 橙色
    "#FFFFFF", // 白色
    "#000000"  // 黑色
};
```

## 准心样式名称

`CrosshairStyleNames` 提供本地化名称：

```csharp
public string[] CrosshairStyleNames { get; } = new[]
{
    "十字准心", "点状准心", "圆形准心", "T形准心", "X形准心", "自定义图片"
};
```

## OverlayWindow 状态

```csharp
public sealed class OverlayWindow : Window
{
    private readonly Canvas _canvas;
    private readonly CrosshairConfig _config;
    private bool _isVisible = true;
    private IntPtr _hwnd;
}
```

## Win32 常量

OverlayWindow 使用的 Win32 常量：

| 常量 | 值 | 说明 |
|------|-----|------|
| GWL_EXSTYLE | -20 | 扩展样式偏移 |
| WS_EX_TRANSPARENT | 0x00000020 | 鼠标穿透 |
| WS_EX_TOOLWINDOW | 0x00000080 | 工具窗口（不显示在任务栏） |