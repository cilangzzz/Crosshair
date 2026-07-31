# CrosshairPro.Infrastructure 坑点

## 1. P/Invoke 资源泄漏

**问题**: Win32 句柄和委托如果管理不当会导致资源泄漏。

**错误做法**:
```csharp
// ❌ 委托未保持引用，可能被 GC 回收
Win32Methods.RegisterClass(ref new WNDCLASS { ... });
```

**正确做法**:
```csharp
// ✅ 保持委托引用
private WndProcDelegate? _wndProcDelegate;

_wndProcDelegate = WndProc;
var wndClass = new WNDCLASS
{
    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
    // ...
};
```

**影响范围**:
- 窗口过程委托
- 热键回调
- 任何传递给 Win32 API 的委托

## 2. 窗口句柄生命周期

**问题**: 窗口句柄必须在正确的时机获取，过早获取会得到 null。

**错误时机**:
```csharp
public MainWindow()
{
    InitializeComponent();
    var hwnd = new WindowInteropHelper(this).Handle;  // ❌ 返回 null
}
```

**正确时机**:
```csharp
public MainWindow()
{
    InitializeComponent();
    SourceInitialized += OnSourceInitialized;
}

private void OnSourceInitialized(object sender, EventArgs e)
{
    var hwnd = new WindowInteropHelper(this).Handle;  // ✅ 有效句柄
    Win32Methods.SetWindowTransparentClickable(hwnd);
}
```

## 3. 热键冲突

**问题**: 全局热键可能与其他应用程序冲突，导致注册失败。

**检查方式**:
```csharp
if (!RegisterHotKey(hwnd, id, modifiers, vk))
{
    var error = Marshal.GetLastWin32Error();
    // 错误码 1410: 热键已被占用
    if (error == 1410)
    {
        // 提示用户选择其他热键
    }
}
```

**建议**:
- 提供热键冲突检测
- 允许用户自定义热键
- 避免使用常用快捷键（如 Ctrl+C）

## 4. WS_EX_TRANSPARENT 鼠标穿透

**问题**: 设置 `WS_EX_TRANSPARENT` 后，窗口对所有输入都穿透，包括拖拽。

**现象**:
- 无法通过鼠标移动窗口
- 无法点击窗口上的控件
- 只能通过键盘或代码操作

**解决方案**:
```csharp
// 需要交互时临时禁用穿透
Win32Methods.SetWindowInteractive(hwnd);

// 完成交互后恢复穿透
Win32Methods.SetWindowTransparentClickable(hwnd);
```

## 5. 窗口层级管理

**问题**: 置顶窗口可能被其他置顶窗口覆盖。

**原因**:
- Windows 允许多个置顶窗口
- 最后激活的置顶窗口在最上层

**解决方案**:
```csharp
// 在窗口激活时重新设置层级
window.Activated += (s, e) =>
{
    var hwnd = new WindowInteropHelper(window).Handle;
    Win32Methods.SetWindowPos(hwnd, Win32Constants.HWND_TOPMOST,
        0, 0, 0, 0,
        Win32Constants.SWP_NOMOVE | Win32Constants.SWP_NOSIZE | Win32Constants.SWP_NOACTIVATE);
};
```

## 6. 64位兼容性

**问题**: 指针和句柄在 64 位系统上是 8 字节，在 32 位系统上是 4 字节。

**注意**:
- 使用 `IntPtr` 而不是 `int` 或 `long`
- P/Invoke 声明会自动处理大小

**正确做法**:
```csharp
// ✅ 使用 IntPtr
[DllImport("user32.dll")]
public static extern IntPtr GetForegroundWindow();

// ❌ 不要使用 int
[DllImport("user32.dll")]
public static extern int GetForegroundWindow();  // 在 64 位系统上会截断
```

## 7. SetLastError 重要性

**问题**: P/Invoke 默认不清除 Win32 错误码，可能导致误导性错误信息。

**正确声明**:
```csharp
// ✅ 添加 SetLastError = true
[DllImport("user32.dll", SetLastError = true)]
public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

// 调用后检查错误
if (!RegisterHotKey(...))
{
    var error = Marshal.GetLastWin32Error();
    // 处理具体错误
}
```

## 8. WPF 线程亲和性

**问题**: Win32 API 调用必须在创建窗口的 UI 线程上执行。

**错误做法**:
```csharp
Task.Run(() =>
{
    Win32Methods.SetWindowPos(hwnd, ...);  // ❌ 非 UI 线程
});
```

**正确做法**:
```csharp
Dispatcher.Invoke(() =>
{
    Win32Methods.SetWindowPos(hwnd, ...);  // ✅ UI 线程
});
```

## 9. 热键 ID 管理

**问题**: 热键 ID 在应用程序内必须唯一，冲突会导致注销失败。

**风险**:
- 硬编码 ID 容易冲突
- 多个模块注册热键时可能使用相同 ID

**建议**: 使用管理器自动分配 ID
```csharp
public class HotkeyManager
{
    private int _nextId = 1;

    public int GetNextId() => _nextId++;
}
```

## 10. 窗口消息循环

**问题**: 创建消息窗口接收热键消息需要正确的消息循环。

**注意**:
- WPF 应用程序已有消息循环
- 纯 Win32 应用需要手动实现消息循环

**WPF 中的使用**:
```csharp
// WPF 的消息循环会自动处理 WM_HOTKEY
// 只需要创建消息窗口并注册类
var hwnd = CreateMessageWindow();
// 热键消息会自动分发到 WndProc
```

## 11. DWM 透明度限制

**问题**: DWM (Desktop Window Manager) 透明度在某些情况下不可用。

**限制**:
- Windows 7 Basic 主题不支持
- 远程桌面连接时可能禁用
- 虚拟机中可能受限

**检测方式**:
```csharp
// 检查 DWM 是否可用
if (DwmIsCompositionEnabled(out var enabled) == 0 && enabled)
{
    // 使用 DWM 透明效果
}
else
{
    // 回退到分层窗口
}
```

## 12. 句柄验证

**问题**: 无效句柄传递给 Win32 API 会导致异常或未定义行为。

**验证方式**:
```csharp
if (hwnd == IntPtr.Zero)
{
    throw new InvalidOperationException("窗口句柄无效");
}

// 调用 Win32 API 前检查
if (!IsWindow(hwnd))
{
    return false;
}
```