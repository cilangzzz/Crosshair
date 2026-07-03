# CrosshairPro.Infrastructure - 坑点

## 1. WndProcDelegate 必须保持引用

委托被传递给非托管代码后，必须保持引用防止被 GC 回收：

```csharp
// ❌ 错误：委托可能被 GC 回收
Win32Methods.RegisterClass(new WNDCLASS {
    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProc)
});

// ✅ 正确：保持委托引用
_wndProcDelegate = WndProc;  // 存储为字段
Win32Methods.RegisterClass(new WNDCLASS {
    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate)
});
```

## 2. 热键 ID 管理

每个热键需要唯一的 ID，由 `GetNextId()` 分配：

```csharp
public int GetNextId() => _nextId++;
```

ID 从 1 开始，注销后不会复用。如果注册大量热键后全部注销，ID 会继续增长。

## 3. 消息窗口生命周期

`WinHotkeyProvider` 创建的消息窗口需要在 `Dispose()` 中销毁：

```csharp
public void Dispose()
{
    UnregisterAll();
    if (_hwnd != IntPtr.Zero)
    {
        Win32Methods.DestroyWindow(_hwnd);
        _hwnd = IntPtr.Zero;
    }
}
```

## 4. 热键注册失败处理

`RegisterHotKey` 可能失败的情况：
- 热键已被其他应用占用
- 无效的组合键
- 窗口句柄无效

必须检查返回值：

```csharp
if (!Win32Methods.RegisterHotKey(_hwnd, id, modifiers, virtualKey))
{
    return false; // 注册失败
}
```

## 5. 组合键解析格式

`KeyCombo.Parse()` 期望格式为 `"修饰键+键名"`：
- ✅ "Ctrl+Shift+F1"
- ✅ "Alt+T"
- ✅ "F5"（无修饰键）
- ❌ "ctrl+shift+f1"（大小写敏感）

## 6. AllowUnsafeBlocks 配置

Infrastructure 项目启用不安全代码：

```xml
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```

当前代码未使用 unsafe 块，但为将来扩展保留。

## 7. 线程安全

`HotkeyManager` 不是线程安全的，所有操作应在 UI 线程执行。
`WndProc` 在创建窗口的线程上执行，即 UI 线程。

## 8. 热键与游戏冲突

某些游戏会独占键盘输入，导致全局热键不生效。解决方案：
- 使用 `MOD_NOREPEAT` 减少误触发
- 提供备用方案（如点击托盘图标切换）