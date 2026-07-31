# CrosshairPro.App - 坑点

## 1. OverlayWindow 鼠标穿透设置

在 `AllowsTransparency=true` 的窗口中，不能同时设置 `WS_EX_LAYERED`，否则窗口会不可见：

```csharp
// ❌ 错误：会导致窗口不可见
int exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_LAYERED);

// ✅ 正确：WPF 的 AllowsTransparency 已经处理了分层窗口
SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
```

## 2. WPF Shape 透明度设置

在透明窗口中，必须对每个 Shape 单独设置 Opacity，而不是在 Canvas 上设置：

```csharp
// ❌ 错误：Canvas.Opacity 会导致整个窗口不可见
_canvas.Opacity = 0.5;

// ✅ 正确：在每个 Shape 上单独设置
var line = new Line { ..., Opacity = shapeOpacity };
```

## 3. 配置深拷贝

`MainViewModel.SavePresetWithNameAsync()` 必须使用 `CloneConfig()` 创建深拷贝：

```csharp
// ❌ 错误：直接引用会导致预设随当前配置变化
var preset = new Preset { Config = Config };

// ✅ 正确：使用深拷贝
var preset = new Preset { Config = _configService.CloneConfig(Config) };
```

## 4. 配置变更事件订阅

`CrosshairConfig` 的嵌套对象（Effects、Outline、Shadow）属性变更需要手动订阅：

```csharp
// MainViewModel 构造函数中
SubscribeConfigEvents(_config);

private void SubscribeConfigEvents(CrosshairConfig config)
{
    config.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
    config.Effects.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
    config.Effects.Outline.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
    config.Effects.Shadow.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
}
```

## 5. 默认预设不可删除

删除预设前必须检查 `IsDefault` 标记：

```csharp
[RelayCommand]
private async Task DeletePreset(Preset? preset)
{
    if (preset == null || preset.IsDefault) return;  // 默认预设不可删除
    await _presetService.DeletePresetAsync(preset.Id);
    await LoadPresets();
}
```

## 6. 颜色选择器 RadioButton 分组

颜色选择使用 `RadioButton` 分组，必须设置 `GroupName` 属性：

```xaml
<RadioButton Foreground="{Binding}"
             GroupName="CrosshairColor"  <!-- 必须设置分组名 -->
             Style="{StaticResource ColorSwatchRadio}"
             Command="{Binding DataContext.SetColorCommand, RelativeSource={RelativeSource AncestorType=Window}}"
             CommandParameter="{Binding}"/>
```

## 7. ComboBox 冻结问题

自定义 ComboBox 模板时，必须设置 `ClickMode="Press"` 和 `Focusable="False"`：

```xaml
<!-- ToggleButton: ClickMode="Press" 防止冻结 -->
<ToggleButton x:Name="toggleButton"
              IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"
              Focusable="False"
              ClickMode="Press">  <!-- 必须是 Press -->

<!-- Popup: Focusable="False" 防止焦点问题 -->
<Popup x:Name="Popup"
       IsOpen="{TemplateBinding IsDropDownOpen}"
       Focusable="False"  <!-- 必须为 False -->
       AllowsTransparency="True">
```

## 8. 托盘菜单上下文关闭

托盘右键菜单需要手动调用 `SetForegroundWindow` 才能正确响应外部点击关闭：

```csharp
_trayIcon.TrayRightMouseDown += (s, e) =>
{
    // ...定位菜单...
    menu.IsOpen = true;

    // 关键：激活菜单窗口，使其能正确接收外部点击关闭事件
    var hwndSource = PresentationSource.FromVisual(menu) as HwndSource;
    if (hwndSource != null)
    {
        SetForegroundWindow(hwndSource.Handle);
    }
};
```

## 9. 主窗口关闭行为

主窗口关闭时默认最小化到托盘，只有真正退出时才关闭：

```csharp
private bool _isReallyClosing;  // 标记是否真正退出

protected override void OnClosing(CancelEventArgs e)
{
    if (!_isReallyClosing)
    {
        e.Cancel = true;  // 取消关闭
        Hide();           // 隐藏到托盘
        return;
    }
    base.OnClosing(e);
}

private void ReallyExit()
{
    _isReallyClosing = true;  // 设置标记
    _trayIcon?.Dispose();
    _overlayWindow.Close();
    Close();
    Application.Current.Shutdown();
}
```

## 10. 预览 Canvas Z-Order

预览区域使用两个 Canvas，网格 Canvas 必须在准心 Canvas 下层：

```xaml
<Canvas x:Name="GridCanvas" Margin="20,36,20,20" ClipToBounds="True"/>      <!-- 网格在下 -->
<Canvas x:Name="CrosshairCanvas" Margin="20,36,20,20" ClipToBounds="True"/>  <!-- 准心在上 -->
```

## 11. 自定义图片加载

加载自定义图片时必须使用 `BitmapCacheOption.OnLoad` 并 `Freeze()`：

```csharp
var bitmap = new BitmapImage();
bitmap.BeginInit();
bitmap.UriSource = new Uri(path, UriKind.Absolute);
bitmap.CacheOption = BitmapCacheOption.OnLoad;  // 必须设置
bitmap.EndInit();
bitmap.Freeze();  // 必须冻结，否则跨线程访问会报错
```

## 12. 样式索引与枚举对应

`SelectedStyleIndex` 与 `CrosshairStyle` 枚举值对应，修改时需检查边界：

```csharp
partial void OnSelectedStyleIndexChanged(int value)
{
    if (value >= 0 && value < Enum.GetValues(typeof(CrosshairStyle)).Length)
        Config.Style = (CrosshairStyle)value;
}
```

## 13. Toast 窗口定位

Toast 窗口定位时，必须检查 owner 是否最小化：

```csharp
if (owner != null && owner.WindowState == WindowState.Normal)
{
    // 使用 owner 的位置定位
    window.Left = ownerCenterX - window.ActualWidth / 2;
    window.Top = ownerBottom - window.ActualHeight - 60;
}
else
{
    // 屏幕底部居中
    window.Left = (screenWidth - window.ActualWidth) / 2;
    window.Top = screenHeight - window.ActualHeight - 80;
}
```

## 14. Win32 圆角窗口

Windows 11+ 使用 DWM API 设置窗口圆角：

```csharp
[DllImport("dwmapi.dll")]
private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
private const int DWMWCP_ROUND = 2;

private void OnSourceInitialized(object? sender, EventArgs e)
{
    try
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        int preference = DWMWCP_ROUND;
        DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
    }
    catch { /* Not Windows 11+ */ }
}
```

## 15. 预设切换时保存状态

切换预设后需要保存当前状态（当前预设ID），以便下次启动恢复：

```csharp
partial void OnSelectedPresetChanged(Preset? value)
{
    if (value == null) return;
    _configService.CopyConfig(value.Config, Config);
    CurrentPresetName = value.Name;
    _currentPresetId = value.Id;

    if (!_isInitializing)
    {
        _ = SaveCurrentStateAsync();  // 保存当前状态
    }

    ConfigUpdated?.Invoke(this, EventArgs.Empty);
}
```
