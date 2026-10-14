# CrosshairPro.Infrastructure - 坑点

## 1. WndProc 委托必须保持引用

委托被 `Marshal.GetFunctionPointerForDelegate` 转成函数指针传给非托管代码后，托管对象本身可能被 GC 回收，导致回调时崩溃：

```csharp
// ❌ 错误：临时委托传完即失引用，GC 后回调崩溃
Win32Methods.RegisterClass(ref new WNDCLASS {
    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(WndProc)
});

// ✅ 正确：WinHotkeyProvider 的做法——保存为字段
_wndProcDelegate = WndProc;
var wndClass = new WNDCLASS
{
    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
    // ...
};
```

**影响范围**：窗口过程委托、热键回调、任何传给 Win32 API 的委托。

## 2. 消息窗口生命周期与 Dispose 顺序

`WinHotkeyProvider` 创建的消息窗口必须在 `Dispose` 中按"先注销热键、后销毁窗口"的顺序清理，否则销毁后的窗口仍可能收到 `WM_HOTKEY`：

```csharp
public void Dispose()
{
    UnregisterAll();                       // ① 先注销全部热键
    if (_hwnd != IntPtr.Zero)
    {
        Win32Methods.DestroyWindow(_hwnd); // ② 再销毁消息窗口
        _hwnd = IntPtr.Zero;
    }
}
```

`HotkeyManager.Dispose` 会级联调用 `_provider.Dispose()`。

## 3. 热键注册失败必须检查返回值

`RegisterHotKey` 失败的常见原因：

- 热键已被其他应用占用（Win32 错误码 1409 `ERROR_HOTKEY_ALREADY_REGISTERED`）
- 键名无法映射为虚拟键码（`GetVirtualKeyCode` 返回 0，`WinHotkeyProvider.Register` 直接失败）
- 消息窗口句柄无效

失败时 `HotkeyManager.RegisterHotkey` 返回 `false` 且不写入任何字典；调用方必须检查返回值并提示用户换键。相关 DllImport 均声明了 `SetLastError = true`，可用 `Marshal.GetLastWin32Error()` 取具体错误码。

## 4. KeyCombo.Parse 是大小写不敏感的

`"ctrl+shift+f1"` 与 `"Ctrl+Shift+F1"` 等价。`Parse` 的完整语义见 [../Core/data-model.md](../Core/data-model.md)（KeyCombo 一节）；键名匹配侧的 `GetVirtualKeyCode` 同样大小写不敏感，见 [data-model.md](data-model.md) 虚拟键码映射一节。**旧文档"大小写敏感"的说法已过时**。

## 5. MOD_NOREPEAT 未定义，连按会重复触发

`Win32Constants` 未定义 `MOD_NOREPEAT`（0x4000），注册热键时也没有任何防重复机制——按住组合键会连续触发 `HotkeyTriggered`。如需抑制，须在 `Win32Constants` 增加常量并在 `WinHotkeyProvider.Register` 中 OR 进 `fsModifiers`。

## 6. HotkeyBinding.Enabled 不被检查

`HotkeyManager.RegisterHotkey` 不读取 `binding.Enabled`。禁用某个热键必须由调用方不注册或先 `UnregisterHotkey`，而不是把 `Enabled` 设为 `false` 后期待模块生效。

## 7. 热键 ID 管理与反查成本

- ID 由 `GetNextId()` 从 1 递增分配，注销后不复用；反复注册/注销会使 ID 单调增长（int 范围内无实际风险）
- `HotkeyManager.UnregisterHotkey` 用 `FirstOrDefault` 在 `_hotkeyIdToBindingId` 中反查，O(n)；热键数量极小（当前 1 个）无碍，批量注册时注意
- 反查以 `entry.Key == 0` 作为"未找到"哨兵——仅因 ID 从 1 开始才安全；若改动起始值需同步修改此处

## 8. 热键与游戏键盘输入冲突

部分游戏（尤其独占全屏模式）可能吞掉或独占键盘输入，导致系统热键收不到 `WM_HOTKEY`。缓解方案：

- 引导用户选择冷门组合键，避开游戏内常用键位
- 提供托盘菜单等热键之外的备用触发入口

## 9. SetWindowTransparentClickable 与 WPF AllowsTransparency 的冲突

`SetWindowTransparentClickable` 会设置 `WS_EX_LAYERED` 并调用 `SetLayeredWindowAttributes`。WPF 的 `AllowsTransparency=true` 窗口已由框架管理分层属性，再手动加 `WS_EX_LAYERED` 可能引发样式冲突——App 层 OverlayWindow 自行实现穿透时即刻意不加 `WS_EX_LAYERED`（仅 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW`）。

鼠标穿透与交互的切换必须成对调用：

```csharp
Win32Methods.SetWindowInteractive(hwnd);          // 需要交互时临时关穿透
Win32Methods.SetWindowTransparentClickable(hwnd); // 完成后恢复穿透
```

注意 `WS_EX_TRANSPARENT` 生效后窗口对一切鼠标输入穿透（无法拖动、无法点击控件），只能靠代码或键盘操作。

## 10. 调用方获取窗口句柄的时机

使用本模块辅助方法前，句柄必须已创建。WPF 中在构造函数里取 `WindowInteropHelper.Handle` 会得到 `IntPtr.Zero`：

```csharp
// ✅ 在 SourceInitialized 之后
SourceInitialized += (s, e) =>
{
    var hwnd = new WindowInteropHelper(this).Handle;
    Win32Methods.SetWindowTransparentClickable(hwnd);
};
```

## 11. P/Invoke 声明规范：IntPtr 与 SetLastError

- 句柄/指针参数一律 `IntPtr`，不要用 `int`/`long`——64 位进程下会截断或布局错位
- 可失败的 API 声明 `SetLastError = true`，失败后用 `Marshal.GetLastWin32Error()` 取错误码
- 新增声明统一放入 `Win32Methods`，勿在调用方散落 DllImport（App 层 OverlayWindow 目前存在私有重复声明，属待收敛项）

## 12. 线程亲和性与消息循环

- `WndProc` 在创建消息窗口的线程上执行——`WinHotkeyProvider` 在 UI 线程构造，因此 `HotkeyTriggered` 在 UI 线程同步触发，事件处理器中可安全访问 UI，但不可执行耗时操作
- 对外暴露的 Win32 辅助方法（`SetWindowPos` 等）也必须在窗口所属线程调用；后台线程需 `Dispatcher.Invoke`
- WPF 应用自带消息循环，`WM_HOTKEY` 会自动分发到 `WndProc`，无需手写消息循环

## 13. 置顶窗口层级竞争

Windows 允许多个 `HWND_TOPMOST` 窗口共存，后激活者在上层，准心覆盖层可能被其他置顶窗口盖住。需要时在窗口激活/周期性用 `SetWindowPos + HWND_TOPMOST + SWP_NOMOVE|SWP_NOSIZE|SWP_NOACTIVATE` 重新置顶。

## 14. AllowUnsafeBlocks 已启用但未使用

csproj 配置了 `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`，当前代码无任何 `unsafe` 块，属预留。阅读代码时不要因为该开关去猜测存在指针操作。

## 15. DWM 透明度的可用性限制

`DwmExtendFrameIntoClientArea`/`DwmSetWindowAttribute` 在远程桌面会话、部分虚拟机中可能失效或被禁用；`DwmIsCompositionEnabled`（检测用）未在本模块封装，需要检测时须自行补充声明。依赖 DWM 的效果应有降级路径（分层窗口 Alpha 透明）。
