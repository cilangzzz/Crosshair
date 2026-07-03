# CrosshairPro.App - 坑点

## 1. OverlayWindow 透明度设置

在 `AllowsTransparency=true` 的窗口中，不能设置 `Canvas.Opacity`：

```csharp
// ❌ 错误：整个窗口变透明
_canvas.Opacity = config.Opacity / 100.0;

// ✅ 正确：对每个 Shape 单独设置
shape.Opacity = config.Opacity / 100.0;
```

## 2. 鼠标穿透实现

鼠标穿透需要设置 `WS_EX_TRANSPARENT` 扩展样式：

```csharp
int exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
```

注意：必须在窗口句柄创建后（`SourceInitialized` 事件）设置。

## 3. 配置变更通知链

配置变更需要多层订阅：

```csharp
// MainViewModel 中
private void SubscribeConfigEvents(CrosshairConfig config)
{
    config.PropertyChanged += (s, e) => { ... };
    config.Effects.PropertyChanged += (s, e) => { ... };
    config.Effects.Outline.PropertyChanged += (s, e) => { ... };
    config.Effects.Shadow.PropertyChanged += (s, e) => { ... };
}
```

更换 Config 实例时需要重新订阅。

## 4. 预设选择变更处理

`SelectedPreset` 变更时使用 `CopyFrom` 而非替换实例：

```csharp
partial void OnSelectedPresetChanged(Preset? value)
{
    if (value == null) return;
    Config.CopyFrom(value.Config); // 保持原实例，复制值
    // ❌ Config = value.Config; // 替换实例会断开绑定
}
```

## 5. 默认预设不可删除

删除预设前检查 `IsDefault` 标记：

```csharp
[RelayCommand]
private async Task DeletePreset(Preset? preset)
{
    if (preset == null || preset.IsDefault) return;
    await _presetRepo.DeletePresetAsync(preset.Id);
}
```

## 6. 图片加载失败处理

自定义图片加载失败时静默回退：

```csharp
private void AddImage(double cx, double cy, string path, double size, double opacity)
{
    try
    {
        // 加载图片...
    }
    catch
    {
        // 静默回退到红色十字
        AddLine(cx - 10, cy, cx + 10, cy, Brushes.Red, 2, false, Brushes.Black, opacity);
        AddLine(cx, cy - 10, cx, cy + 10, Brushes.Red, 2, false, Brushes.Black, opacity);
    }
}
```

## 7. 窗口位置初始化

OverlayWindow 必须覆盖整个屏幕：

```csharp
Left = 0;
Top = 0;
Width = SystemParameters.PrimaryScreenWidth;
Height = SystemParameters.PrimaryScreenHeight;
```

多显示器环境下只覆盖主显示器。

## 8. ViewModel 依赖注入

当前 MainViewModel 直接创建 Repository 实例：

```csharp
private readonly JsonPresetRepository _presetRepo = new();
```

更好的做法是通过依赖注入：

```csharp
public MainViewModel(IPresetRepository presetRepo)
{
    _presetRepo = presetRepo;
}
```

## 9. 热键管理器生命周期

热键管理器需要在应用退出时清理：

```csharp
// 注册
_hotkeyManager = new HotkeyManager();
_hotkeyManager.HotkeyTriggered += OnHotkeyTriggered;

// 退出时清理
_hotkeyManager.HotkeyTriggered -= OnHotkeyTriggered;
_hotkeyManager.Dispose();
```

## 10. 日志配置

Serilog 在 App 启动时初始化：

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(Path.Combine(appDataPath, "CrosshairPro", "logs", "app-.log"),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
```