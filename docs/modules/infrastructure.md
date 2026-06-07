# Infrastructure 基础设施层文档

## 概述

`CrosshairPro.Infrastructure` 是项目的基础设施层，负责封装平台特定的底层功能，主要包括 Win32 API 调用和热键管理系统。

**项目路径**：`src/CrosshairPro.Infrastructure/`

**目标框架**：net8.0

**特殊配置**：`AllowUnsafeBlocks` 已启用（用于 P/Invoke）

**依赖**：CrosshairPro.Core

---

## 目录结构

```
CrosshairPro.Infrastructure/
├── Hotkey/
│   └── HotkeyManager.cs         # 热键管理器实现
├── IO/                          # 文件 I/O 工具（待实现）
└── Win32/
    ├── Win32Constants.cs        # Win32 常量定义
    └── Win32Methods.cs          # Win32 P/Invoke 声明
```

---

## Win32 模块

### Win32Constants

Win32 常量定义类，包含窗口样式、消息类型、热键修饰符等常量。

**文件**：`src/CrosshairPro.Infrastructure\Win32\Win32Constants.cs`

#### 窗口样式常量

| 常量 | 值 | 说明 |
|------|-----|------|
| WS_EX_LAYERED | 0x00080000 | 分层窗口样式 |
| WS_EX_TRANSPARENT | 0x00000020 | 点击穿透 |
| WS_EX_TOPMOST | 0x00000008 | 始终置顶 |
| WS_EX_TOOLWINDOW | 0x00000080 | 工具窗口（不显示在任务栏） |
| WS_EX_NOACTIVATE | 0x08000000 | 不激活窗口 |
| WS_POPUP | 0x80000000 | 弹出窗口 |

#### 窗口位置常量（SWP）

| 常量 | 值 | 说明 |
|------|-----|------|
| SWP_NOMOVE | 0x0002 | 不移动窗口 |
| SWP_NOSIZE | 0x0001 | 不改变大小 |
| SWP_NOACTIVATE | 0x0010 | 不激活窗口 |
| SWP_SHOWWINDOW | 0x0040 | 显示窗口 |
| SWP_NOZORDER | 0x0004 | 不改变 Z 序 |

#### 热键修饰符常量（MOD）

| 常量 | 值 | 说明 |
|------|-----|------|
| MOD_ALT | 0x0001 | Alt 键 |
| MOD_CONTROL | 0x0002 | Ctrl 键 |
| MOD_SHIFT | 0x0004 | Shift 键 |
| MOD_WIN | 0x0008 | Win 键 |
| MOD_NOREPEAT | 0x4000 | 不重复触发 |

#### 窗口消息常量

| 常量 | 值 | 说明 |
|------|-----|------|
| WM_HOTKEY | 0x0312 | 热键消息 |
| WM_DESTROY | 0x0002 | 窗口销毁消息 |
| WM_LBUTTONDOWN | 0x0201 | 鼠标左键按下 |
| WM_LBUTTONUP | 0x0202 | 鼠标左键释放 |

#### 透明度常量

| 常量 | 值 | 说明 |
|------|-----|------|
| LWA_ALPHA | 0x00000002 | 使用 Alpha 透明度 |
| LWA_COLORKEY | 0x00000001 | 使用颜色键透明 |

#### 结构体

```csharp
public struct RECT
{
    public int Left, Top, Right, Bottom;
}

public struct POINT
{
    public int X, Y;
}

public struct MARGINS
{
    public int Left, Right, Top, Bottom;
}
```

---

### Win32Methods

Win32 P/Invoke 声明类，封装了 user32.dll 和 dwmapi.dll 的 API 调用。

**文件**：`src/CrosshairPro.Infrastructure\Win32\Win32Methods.cs`

#### user32.dll 函数

| 函数 | 说明 |
|------|------|
| SetWindowPos | 设置窗口位置和大小 |
| GetWindowRect | 获取窗口矩形 |
| SetWindowLong | 设置窗口属性 |
| GetWindowLong | 获取窗口属性 |
| SetLayeredWindowAttributes | 设置分层窗口透明度 |
| RegisterHotKey | 注册全局热键 |
| UnregisterHotKey | 注销全局热键 |
| CreateWindowEx | 创建窗口 |
| DestroyWindow | 销毁窗口 |
| DefWindowProc | 默认窗口过程 |
| RegisterClass | 注册窗口类 |
| VkKeyScan | 将字符映射为虚拟键码 |
| GetKeyState | 获取键状态 |
| GetAsyncKeyState | 获取异步键状态 |

#### dwmapi.dll 函数

| 函数 | 说明 |
|------|------|
| DwmExtendFrameIntoClientArea | 扩展窗口框架到客户区 |
| DwmSetWindowAttribute | 设置 DWM 窗口属性 |

#### 辅助方法

**SetWindowTransparentClickable(IntPtr hwnd)**

设置窗口为透明且可点击穿透。

实现逻辑：
1. 获取当前窗口样式
2. 添加 `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW`
3. 设置窗口样式
4. 设置 DWM 扩展框架

**SetWindowInteractive(IntPtr hwnd)**

设置窗口为可交互模式。

实现逻辑：
1. 获取当前窗口样式
2. 移除 `WS_EX_TRANSPARENT`（允许点击）
3. 保留其他样式
4. 设置窗口样式

**GetVirtualKeyCode(string keyName)** → int

将键名字符串转换为虚拟键码。

支持的键名：
- 字母：A-Z
- 数字：0-9
- 功能键：F1-F12
- 特殊键：Space, Enter, Escape, Tab, Backspace, Delete, Insert, Home, End, PageUp, PageDown
- 方向键：Up, Down, Left, Right
- 修饰键：Ctrl, Shift, Alt, Win

---

## Hotkey 模块

### WinHotkeyProvider

底层热键提供者，创建隐藏消息窗口并处理 Win32 热键消息。

**文件**：`src/CrosshairPro.Infrastructure\Hotkey\HotkeyManager.cs`

#### 工作原理

1. 创建隐藏窗口类 "CrosshairProHotkeyWindow"
2. 创建消息窗口用于接收 `WM_HOTKEY` 消息
3. 通过 `RegisterHotKey` 注册全局热键
4. 在 `WndProc` 中处理热键消息并触发事件

#### 属性和字段

| 名称 | 类型 | 说明 |
|------|------|------|
| _hwnd | IntPtr | 隐藏窗口句柄 |
| _registeredKeys | Dictionary<int, string> | 已注册的热键 ID 到名称的映射 |
| _nextId | int | 下一个可用的热键 ID |

#### 方法

| 方法 | 说明 |
|------|------|
| RegisterHotkey(int id, int modifiers, int vk) | 注册热键 |
| UnregisterHotkey(int id) | 注销热键 |
| GetNextId() | 获取下一个可用 ID |
| WndProc(IntPtr, int, IntPtr, IntPtr) | 窗口消息处理 |

#### 事件

| 事件 | 说明 |
|------|------|
| HotkeyPressed | 热键按下时触发 |

---

### HotkeyManager

热键管理器，实现 `IHotkeyManager` 接口，提供高层热键管理功能。

**文件**：`src/CrosshairPro.Infrastructure\Hotkey\HotkeyManager.cs`

#### 工作原理

1. 包装 `WinHotkeyProvider` 实例
2. 维护热键绑定字典（按 ID 索引）
3. 维护热键 ID 到绑定 ID 的映射
4. 解析热键组合字符串并注册到 Win32
5. 触发 `HotkeyTriggered` 事件

#### 属性和字段

| 名称 | 类型 | 说明 |
|------|------|------|
| _provider | WinHotkeyProvider | 底层热键提供者 |
| _bindings | Dictionary<string, HotkeyBinding> | 热键绑定字典 |
| _hotkeyIdToBindingId | Dictionary<int, string> | 热键 ID 到绑定 ID 的映射 |

#### 方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| RegisterHotkey(HotkeyBinding) | bool | 注册热键绑定 |
| UnregisterHotkey(string bindingId) | bool | 注销热键绑定 |
| UnregisterAll() | void | 注销所有热键 |

#### 事件

| 事件 | 参数 | 说明 |
|------|------|------|
| HotkeyTriggered | HotkeyBinding | 热键触发时传递绑定信息 |

#### 热键注册流程

```
RegisterHotkey(binding)
    ↓
解析 binding.Combo → KeyCombo.Parse()
    ↓
转换修饰符：Ctrl → MOD_CONTROL, Shift → MOD_SHIFT, Alt → MOD_ALT
    ↓
转换键码：GetVirtualKeyCode(keyName)
    ↓
获取下一个热键 ID：_provider.GetNextId()
    ↓
注册热键：_provider.RegisterHotkey(id, modifiers, vk)
    ↓
保存映射：_hotkeyIdToBindingId[id] = binding.Id
    ↓
保存绑定：_bindings[binding.Id] = binding
```

---

## 依赖关系

```
CrosshairPro.Infrastructure
  ├── HotkeyManager
  │     ├── WinHotkeyProvider
  │     │     └── Win32Methods (P/Invoke)
  │     │           └── Win32Constants
  │     └── IHotkeyManager (Core 层接口)
  └── Win32Methods
        └── Win32Constants
```

## 线程安全

- `WinHotkeyProvider` 的消息窗口在创建线程的消息循环中运行
- `HotkeyManager` 的事件在消息窗口线程上触发
- 消费者需要注意在 UI 线程上处理事件（如需要更新 UI）
