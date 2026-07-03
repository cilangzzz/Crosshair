# CrosshairPro.Infrastructure

基础设施层模块，提供 Win32 API 封装和热键管理功能。

## 概述

Infrastructure 模块负责与 Windows 系统交互：
- Win32 API P/Invoke 封装
- 全局热键注册和管理

## 目录结构

```
CrosshairPro.Infrastructure/
├── Hotkey/
│   └── HotkeyManager.cs      # 热键管理器（实现 IHotkeyManager）
└── Win32/
    ├── Win32Constants.cs     # Win32 常量定义
    └── Win32Methods.cs       # Win32 API P/Invoke 声明
```

## 核心类

### HotkeyManager

热键管理器，实现 `IHotkeyManager` 接口：

| 方法 | 说明 |
|------|------|
| `RegisterHotkey(binding)` | 注册全局热键 |
| `UnregisterHotkey(id)` | 注销指定热键 |
| `UnregisterAll()` | 注销所有热键 |

| 事件 | 说明 |
|------|------|
| `HotkeyTriggered` | 热键触发事件，参数为 `HotkeyTriggeredEventArgs` |

**内部实现**：
- 使用 `WinHotkeyProvider` 创建消息窗口接收 `WM_HOTKEY`
- 通过 `RegisterHotKey` / `UnregisterHotKey` Win32 API 注册/注销

### WinHotkeyProvider

内部类，封装 Win32 热键 API：

| 方法 | 说明 |
|------|------|
| `Register(id, combo)` | 注册热键到系统 |
| `Unregister(id)` | 注销热键 |
| `GetNextId()` | 获取下一个热键 ID |

**关键机制**：
- 创建不可见消息窗口（`CreateWindowEx`）用于接收热键消息
- 定义 `WndProcDelegate` 处理 `WM_HOTKEY` 消息
- 委托必须保持引用防止 GC 回收

### Win32Methods

Win32 API P/Invoke 声明：

| API | 用途 |
|-----|------|
| `RegisterHotKey` | 注册全局热键 |
| `UnregisterHotKey` | 注销热键 |
| `GetVirtualKeyCode` | 获取虚拟键码 |
| `CreateWindowEx` | 创建窗口 |
| `DestroyWindow` | 销毁窗口 |
| `DefWindowProc` | 默认窗口过程 |
| `RegisterClass` | 注册窗口类 |

### Win32Constants

Win32 常量定义：

| 常量 | 值 | 说明 |
|------|-----|------|
| `WM_HOTKEY` | 0x0312 | 热键消息 |
| `MOD_ALT` | 0x0001 | Alt 修饰键 |
| `MOD_CONTROL` | 0x0002 | Ctrl 修饰键 |
| `MOD_SHIFT` | 0x0004 | Shift 修饰键 |
| `MOD_NOREPEAT` | 0x4000 | 禁止重复 |

## KeyCombo 结构

热键组合键解析：

```csharp
public class KeyCombo
{
    public int Key { get; set; }      // 虚拟键码
    public int Modifiers { get; set; } // 修饰键组合

    public static KeyCombo Parse(string combo); // 解析如 "Ctrl+Shift+F1"
}
```

## 详细文档

- [数据模型](data-model.md) - 相关模型说明
- [坑点](pitfalls.md) - 已知问题和注意事项