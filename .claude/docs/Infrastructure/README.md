# CrosshairPro.Infrastructure

基础层模块：Win32 API P/Invoke 封装与全局热键管理。所有与 Windows 系统交互的底层调用都收敛在本模块，上层只依赖 Core 的 `IHotkeyManager` 接口与本模块的封装方法。

## 概述

- **Win32 封装**：`Win32Methods`（P/Invoke 声明 + 窗口辅助方法 + 键码映射）、`Win32Constants`（常量与结构体）
- **热键管理**：`HotkeyManager`（实现 Core 的 `IHotkeyManager`）+ `WinHotkeyProvider`（消息窗口 + `WM_HOTKEY` 分发）
- **DI 注册**：`services.AddSingleton<IHotkeyManager, HotkeyManager>()`（位于 Application/DI/ServiceCollectionExtensions.cs）

## 目录结构

```
CrosshairPro.Infrastructure/
├── Hotkey/
│   └── HotkeyManager.cs      # 热键管理：WinHotkeyProvider（消息窗口）+ HotkeyManager（绑定管理），同文件两个类
├── Win32/
│   ├── Win32Constants.cs     # Win32 常量（窗口样式/SWP/HWND/修饰键/消息/透明度）+ RECT/POINT/MARGINS 结构体
│   └── Win32Methods.cs       # P/Invoke 声明（user32/dwmapi）+ 窗口辅助方法 + GetVirtualKeyCode + WNDCLASS 结构
└── CrosshairPro.Infrastructure.csproj  # net8.0，AllowUnsafeBlocks=true，项目引用 Core
```

（工作区中可见的 `IO/` 为空目录，未被 git 跟踪，属预留占位。）

## 核心类

### HotkeyManager — 对外热键管理器

实现 `Core.Interfaces.IHotkeyManager`，接口定义见 [../Core/README.md](../Core/README.md)。

| 成员 | 说明 |
|------|------|
| `RegisterHotkey(HotkeyBinding)` | `KeyCombo.Parse` 解析 `binding.Combo` → `GetNextId()` 分配 ID → 注册到系统；成功才记录绑定 |
| `UnregisterHotkey(string bindingId)` | 反查系统热键 ID（`FirstOrDefault`，O(n)）后注销 |
| `UnregisterAll()` | 注销全部并清空两个映射字典 |
| `HotkeyTriggered` 事件 | 热键触发，参数 `HotkeyTriggeredEventArgs`（携带原始 `HotkeyBinding`） |
| `Dispose()` | 释放 `WinHotkeyProvider` 并清空绑定 |

**行为要点**：

- 不检查 `binding.Enabled`——是否注册由调用方决定
- 注册失败（键名无法解析或系统占用）返回 `false`，不写入任何映射字典、不占用系统热键（但 `GetNextId` 已消耗一个 ID，`_nextId` 计数器仍递增，注销后也不复用）
- `HotkeyTriggered` 在消息窗口所在线程（UI 线程）同步触发

### WinHotkeyProvider — Win32 热键底层

与 `HotkeyManager` 同文件的公开类，封装系统热键 API（当前仅被 `HotkeyManager` 使用）：

| 成员 | 说明 |
|------|------|
| `Register(int id, KeyCombo combo)` | `GetVirtualKeyCode` 把键名转为虚拟键码后调用 `RegisterHotKey`；键码为 0 直接失败 |
| `Unregister(int id)` | 注销单个热键并移除记录 |
| `UnregisterAll()` | 遍历注销全部热键 |
| `GetNextId()` | 分配热键 ID（从 1 递增，注销后不复用） |
| `HotkeyPressed` 事件 | 消息窗口收到 `WM_HOTKEY` 时按 ID 触发 |

**消息窗口机制**：

1. 构造时 `RegisterClass`（窗口类名 `CrosshairProHotkeyWindow`）+ `CreateWindowEx` 创建不可见消息窗口
2. `WndProc` 委托保存在字段 `_wndProcDelegate`，防止被 GC 回收后非托管回调崩溃
3. `WM_HOTKEY` 到达时从 `wParam` 取热键 ID 触发 `HotkeyPressed`，其余消息交给 `DefWindowProc`
4. `Dispose` 中先 `UnregisterAll` 再 `DestroyWindow`

### Win32Methods — P/Invoke 与辅助方法

`static partial class`，所有 DllImport 集中于此：

| 分组 | API |
|------|-----|
| 窗口操作 | `SetWindowPos`、`GetWindowRect`、`SetWindowLong`/`GetWindowLong`、`SetLayeredWindowAttributes`、`GetForegroundWindow`、`GetWindowThreadProcessId`、`GetWindowText`、`ShowWindow` |
| 热键 | `RegisterHotKey`、`UnregisterHotKey` |
| 消息窗口 | `CreateWindowEx`、`DestroyWindow`、`DefWindowProc`、`RegisterClass` |
| 键盘 | `VkKeyScan`、`GetKeyState`、`GetAsyncKeyState` |
| DWM | `DwmExtendFrameIntoClientArea`、`DwmSetWindowAttribute` |

**辅助方法**（非 P/Invoke，组合上述 API）：

| 方法 | 说明 |
|------|------|
| `SetWindowTransparentClickable(hWnd)` | 置 `WS_EX_LAYERED\|WS_EX_TRANSPARENT\|WS_EX_TOPMOST` + 置顶 + Alpha=255，即鼠标穿透置顶 |
| `SetWindowInteractive(hWnd)` | 清除 `WS_EX_TRANSPARENT`，恢复鼠标交互 |
| `GetVirtualKeyCode(key)` | 键名 → 虚拟键码，映射表见 [data-model.md](data-model.md)；单字符走 `VkKeyScan` 兜底，未知返回 0 |

### Win32Constants — 常量与结构体

常量分组：窗口扩展样式（`WS_EX_*`）、窗口样式（`WS_POPUP`/`WS_VISIBLE`）、`SetWindowPos` 标志（`SWP_*`）、窗口层级（`HWND_TOPMOST` 等）、热键修饰键（`MOD_*`）、消息（`WM_HOTKEY` 等）、透明度（`LWA_*`）。完整取值表见 [data-model.md](data-model.md)。

结构体：`RECT`（含 `Width`/`Height` 计算属性）、`POINT`、`MARGINS`（DWM 扩展帧用）。

## 依赖关系

- **依赖**：仅 `CrosshairPro.Core`（`KeyCombo`、`HotkeyBinding`、`IHotkeyManager` 等类型定义在 Core）
- **被项目引用**：Services、Application、App（均为 csproj 引用）
- **实际代码消费**：Application/DI（注册单例）、App/MainWindow（注册并监听热键）；Win32 封装当前仅被模块内部热键管线使用

**热键数据流**（当前唯一注册的绑定：`"toggle-crosshair"`，`Ctrl+Shift+X`，`HotkeyAction.ToggleCrosshair`）：

```
MainWindow.RegisterHotkeys()
  → HotkeyManager.RegisterHotkey(binding)
    → KeyCombo.Parse("Ctrl+Shift+X")
    → WinHotkeyProvider.Register(id, combo)      # RegisterHotKey + 消息窗口
用户按键 → WM_HOTKEY → WndProc → HotkeyPressed(id)
  → HotkeyManager.OnHotkeyPressed → HotkeyTriggered(binding)
  → MainWindow.OnHotkeyTriggered                 # 按 binding.Action 分发（Dispatcher.Invoke 回 UI 线程）
```

## 详细文档

- [数据模型](data-model.md) - Win32 结构体/常量表、虚拟键码映射、热键运行时数据
- [坑点](pitfalls.md) - P/Invoke 与热键的已知陷阱
- [变更日志](CHANGELOG.md) - 模块变更历史
- 相关模型（`KeyCombo`/`HotkeyBinding`/`HotkeyAction`）定义在 Core：[../Core/data-model.md](../Core/data-model.md)
