# CrosshairPro.App - 坑点

## 1. OverlayWindow 鼠标穿透设置

在 `AllowsTransparency=true` 的窗口中，不能同时设置 `WS_EX_LAYERED`，否则窗口会不可见：

```csharp
// ❌ 错误：会导致窗口不可见
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

`CrosshairViewModel.SavePresetWithNameAsync()` 必须使用 `CloneConfig()` 创建深拷贝（`MainViewModel.SavePresetWithNameAsync` 只是转发到它）：

```csharp
// ❌ 错误：直接引用会导致预设随当前配置变化
var preset = new Preset { Config = Config };

// ✅ 正确：使用深拷贝
var preset = new Preset { Config = _configService.CloneConfig(Config) };
```

## 4. 配置变更事件订阅

`CrosshairConfig` 的嵌套对象（Effects、Outline、Shadow）属性变更需要递归手动订阅。CrosshairViewModel 用它触发 ConfigUpdated，OverlayWindow 用它触发重绘，两处都要订阅完整层级：

```csharp
// CrosshairViewModel.SubscribeConfigEvents / OverlayWindow 构造函数中
config.PropertyChanged += ...;
config.Effects.PropertyChanged += ...;
config.Effects.Outline.PropertyChanged += ...;
config.Effects.Shadow.PropertyChanged += ...;
```

漏订阅某一层（如只订阅 Config 本身）会导致嵌套属性变化时覆盖窗口不重绘。

## 5. 默认预设不可删除

删除预设前必须检查 `IsDefault` 标记：

```csharp
private async Task DeletePreset(Preset? preset)
{
    if (preset == null || preset.IsDefault) return;  // 默认预设不可删除
    await _presetService.DeletePresetAsync(preset.Id);
    await LoadPresets();
}
```

## 6. 颜色选择器 RadioButton 分组

CrosshairPage 的颜色选择使用 `RadioButton` 分组，必须设置 `GroupName`；命令绑定相对源是 UserControl（页面 DataContext 是 CrosshairViewModel，不是 Window）：

```xml
<RadioButton Foreground="{Binding}"
             GroupName="CrosshairColor"  <!-- 必须设置分组名 -->
             Style="{StaticResource ColorSwatchRadio}"
             Command="{Binding DataContext.SetColorCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
             CommandParameter="{Binding}"/>
```

自定义颜色按钮也必须加入同一 GroupName，否则选预设色后自定义按钮仍保持选中态。

## 7. ComboBox 冻结问题

自定义 ComboBox 模板（ControlStyles.xaml 的 CustomComboBox）时，必须设置 `ClickMode="Press"` 和 `Focusable="False"`：

```xml
<!-- ToggleButton: ClickMode="Press" 防止冻结 -->
<ToggleButton IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}"
              Focusable="False" ClickMode="Press"/>

<!-- Popup: Focusable="False" 防止焦点问题 -->
<Popup IsOpen="{TemplateBinding IsDropDownOpen}" AllowsTransparency="True" Focusable="False"/>
```

## 8. 托盘菜单上下文关闭

托盘右键菜单需要手动调用 `SetForegroundWindow` 才能正确响应外部点击关闭：

```csharp
_trayIcon.TrayRightMouseDown += (s, e) =>
{
    // GetCursorPos + TransformFromDevice 手动定位到鼠标位置（像素→WPF 单位）
    menu.Placement = PlacementMode.AbsolutePoint;
    menu.IsOpen = true;

    // 关键：激活菜单窗口，使其能正确接收外部点击关闭事件
    var hwndSource = PresentationSource.FromVisual(menu) as HwndSource;
    if (hwndSource != null) SetForegroundWindow(hwndSource.Handle);
};
```

## 9. 主窗口关闭行为

主窗口关闭时默认最小化到托盘，只有真正退出时才关闭：

```csharp
private bool _isReallyClosing;  // 标记是否真正退出

protected override void OnClosing(CancelEventArgs e)
{
    if (!_isReallyClosing) { e.Cancel = true; Hide(); return; }
    base.OnClosing(e);
}

private void ReallyExit()
{
    _isReallyClosing = true;
    _trayIcon?.Dispose();   // 必须先释放托盘图标，否则退出后残留
    _overlayWindow.Close();
    Close();
    System.Windows.Application.Current.Shutdown();
}
```

## 10. 预览 Canvas Z-Order

CrosshairPage 预览区域使用两个 Canvas，网格 Canvas 必须在准心 Canvas 下层：

```xml
<Canvas x:Name="GridCanvas" Margin="20,36,20,20" ClipToBounds="True"/>      <!-- 网格在下 -->
<Canvas x:Name="CrosshairCanvas" Margin="20,36,20,20" ClipToBounds="True"/>  <!-- 准心在上 -->
```

两块画布的 ActualWidth/ActualHeight 为 0 时（未完成布局），RenderCrosshair 会回退到 300x300 兜底值，首帧渲染需通过 `Dispatcher.BeginInvoke(..., DispatcherPriority.Loaded)` 推迟。

## 11. 自定义图片加载

加载自定义图片时必须使用 `BitmapCacheOption.OnLoad` 并 `Freeze()`：

```csharp
var bitmap = new BitmapImage();
bitmap.BeginInit();
bitmap.UriSource = new Uri(path, UriKind.Absolute);
bitmap.CacheOption = BitmapCacheOption.OnLoad;  // 必须设置，否则文件被占用
bitmap.EndInit();
bitmap.Freeze();  // 必须冻结，否则跨线程访问会报错
```

OverlayWindow.AddImage 已包含该逻辑；图片未设置或路径不存在时回退绘制当前准心颜色的十字（RenderCrosshair 的 else 分支），仅当图片存在但 BitmapImage 加载抛异常时才回退红色小十字（AddImage 的 catch 分支），不要让覆盖窗口崩溃。

## 12. 样式索引与枚举对应

`SelectedStyleIndex` 与 `CrosshairStyle` 枚举值对应，修改时需检查边界：

```csharp
partial void OnSelectedStyleIndexChanged(int value)
{
    if (value >= 0 && value < Enum.GetValues(typeof(CrosshairStyle)).Length)
        Config.Style = (CrosshairStyle)value;
}
```

CrosshairPage.xaml 中"选择自定义图片"按钮以 `SelectedStyleIndex == 5`（CustomImage）作为显示条件——枚举顺序调整会同时破坏该触发器。

## 13. Toast 窗口定位

ToastManager 定位悬浮窗口时，必须检查 owner 是否最小化：

```csharp
if (owner != null && owner.WindowState == WindowState.Normal)
{
    window.Left = ownerCenterX - window.ActualWidth / 2;
    window.Top = ownerBottom - window.ActualHeight - 60;
}
else
{
    window.Left = (screenWidth - window.ActualWidth) / 2;
    window.Top = screenHeight - window.ActualHeight - 80;
}
// 之后还要钳制左/右边界，防止超屏
```

## 14. Win32 圆角窗口

Windows 11+ 使用 DWM API 设置窗口圆角，必须包在 try-catch 中（旧系统没有该 API）：

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

`CrosshairViewModel.OnSelectedPresetChanged` 切换预设后需要保存当前预设 ID，以便下次启动恢复；初始化阶段（_isInitializing）不要保存：

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

## 16. ApexPage Toast 重复订阅

ApexPage.OnLoaded 订阅 `vm.ToastRequested` 但没有在 Unloaded 时退订。页面每次导航回来都会追加一个订阅，同一消息会弹出多次 Toast。Cs2Page 已正确处理（OnLoaded 订阅 / OnUnloaded 退订），修改 ApexPage 时必须补齐：

```csharp
// Cs2Page 的正确写法
private async void OnLoaded(object s, RoutedEventArgs e)
{
    _viewModel.ToastRequested += OnToastRequested;   // 订阅
    await _viewModel.LoadAsync();
}
private void OnUnloaded(object s, RoutedEventArgs e)
    => _viewModel?.ToastRequested -= OnToastRequested; // 退订
```

## 17. Cs2Page 按 Sections 固定索引绑定（已解决）

Cs2Page.xaml 曾通过 `Sections[x].Items[y]` 硬编码索引绑定配置项，调整 `CreateCS2Strategy()` 的分区/条目顺序会静默错绑。2026-10 起改为 `ItemsControl` + `ConfigItemTemplateSelector`（按 `ConfigItemType` 选统一行模板）直接渲染 `Sections[x].Items`，项级索引绑定已消除；仅分区级 `Sections[x]` 保留。背景与细节见 [../GameConfig/CS2/pitfalls.md](../GameConfig/CS2/pitfalls.md) §18。

## 18. 运行时切换语言不会更新已生成的文本

`LocalizationProvider.SetCulture` 只会刷新通过 `{loc:Loc}` / 索引器 Binding 的 XAML 元素。以下内容在构造时按当时语言一次性生成，切换语言后保持旧值：

- `CrosshairViewModel.CrosshairStyleNames`（样式下拉框选项）
- `ApexConfigViewModel` 的各选项集合（TextureQualityOptions / WindowModeOptions 等字典以本地化文本作键；LaunchOptionPresets 为 ObservableCollection<string>，其中「调试模式」为硬编码中文，其余 3 项为 LocalizationProvider.Get 结果）
- 各 ViewModel 的 `StatusMessage` / `CurrentPresetName` 历史值

在代码后置中取词应每次调用 `LocalizationProvider.Get(key)`（不要缓存结果），需要语言级联更新的列表应在语言切换事件后重建。

## 19. CrosshairPage 调试日志

CrosshairPage 内置 `Log()` 方法，把每个事件（DataContextChanged、每次渲染、每次属性变更）追加写入 `%APPDATA%\CrosshairPro\crosshair_page.log`。该日志无开关、无上限，频繁拖动滑块时显著膨胀并产生 IO。排查 UI 问题时可利用它，但发布前应移除或加开关。

## 20. OverlayWindow 使用配置副本

OverlayWindow 持有独立的 `CrosshairConfig _config`，外部修改 ViewModel 的 Config 不会自动反映到覆盖窗口，必须经过：

```
CrosshairViewModel.Config 变更 → ConfigUpdated 事件
  → MainWindow 订阅回调 → _overlayWindow.UpdateConfig(config)（内部 CopyFrom）
```

新增修改配置的入口时，如果绕过了 ConfigUpdated 事件（例如直接改 Config 后只刷新预览页），覆盖层会停留在旧配置。

## 21. 预览与覆盖窗口的 T 形方向不一致

OverlayWindow.RenderCrosshair 绘制 TShape 为倒 T（竖线朝下，注释说明是 FPS 标准、不遮挡目标上方）；CrosshairPage 与 CrosshairPreview 控件的预览绘制的 T 形竖线朝上。调整 T 形渲染时两处必须同步，否则预览与实际覆盖层方向不符。

## 22. 全局热键只注册了切换键

MainWindow.RegisterHotkeys 仅注册 `Ctrl+Shift+X`（toggle-crosshair）。`OnHotkeyTriggered` 中保留 `IncreaseSize` / `DecreaseSize` 分支（Size ±5），但没有对应的热键绑定——新增调节大小热键时需在 RegisterHotkeys 中补充注册，否则分支是死代码。

## 23. 单实例 Mutex 校验

`App.OnStartup` 最先创建命名 Mutex `Local\CrosshairPro.SingleInstance`（`createdNew=false` 即重复启动），经 `ExitAsAlreadyRunning()` 弹本地化提示后 `Shutdown()`。改动启动流程时注意：

- 构造函数需 try/catch `UnauthorizedAccessException`：同名互斥体已存在但当前用户无权打开时（如同会话管理员实例先启动、或名称被其他程序占用）构造会抛异常，不捕获会在 DispatcherUnhandledException 注册前直接崩溃（已实测复现），应按"已在运行"走同一优雅退出路径
- 校验位于 `LocalizationProvider.Initialize()` 之前，此时语言尚未恢复——`App.ReadSavedLanguage()` 同步读 appstate.json 取语言偏好。不要改回 `LoadStateAsync().GetAwaiter().GetResult()`：UI 线程 sync-over-async，await 续延派回 Dispatcher 而 Dispatcher 正在执行 OnStartup，会死锁
- `OnExit` 主动 `ReleaseMutex()` + `Dispose()`；`ReleaseMutex` 在线程未持有时抛异常（退出路径已提前 Dispose 并置空字段），需 try/catch 兜底
- Mutex 用 `Local\` 而非 `Global\`：准心覆盖层按登录会话隔离，多用户会话内各允许一个实例
- appstate.json 路径来自 `Core.CrosshairProPaths.AppStateFile`（与 JsonConfigRepository 同源），不要重新硬编码 `%APPDATA%\CrosshairPro` 路径
