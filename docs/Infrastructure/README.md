# CrosshairPro.Infrastructure 模块

## 概述

基础层模块，封装 Windows API 和底层功能实现。提供 Win32 API P/Invoke 声明、热键管理、文件系统操作等基础能力。

## 模块结构

```
CrosshairPro.Infrastructure/
├── Hotkey/
│   └── HotkeyManager.cs      # 热键管理器实现
├── Win32/
│   ├── Win32Constants.cs     # Win32 常量定义
│   └── Win32Methods.cs       # Win32 API 方法封装
└── IO/
    └── (待扩展)
```

## 核心组件

### 1. HotkeyManager - 热键管理器

**职责**: 实现 `IHotkeyManager` 接口，提供 Windows 系统热键注册和管理功能。

**实现类**:
- `HotkeyManager`: 对外暴露的管理器，实现 `IHotkeyManager` 接口
- `WinHotkeyProvider`: 内部实现，封装 Win32 热键 API

**主要功能**:
- 注册全局热键
- 注销热键
- 热键触发事件通知
- 支持组合键（Ctrl/Alt/Shift + Key）

**使用示例**:
```csharp
var manager = new HotkeyManager();
var binding = new HotkeyBinding
{
    Id = "toggle-crosshair",
    Combo = "Ctrl+Shift+F1"
};

// 注册热键
manager.RegisterHotkey(binding);

// 监听热键触发
manager.HotkeyTriggered += (sender, args) =>
{
    if (args.Binding.Id == "toggle-crosshair")
    {
        // 切换准心显示
    }
};

// 注销热键
manager.UnregisterHotkey("toggle-crosshair");

// 清理资源
manager.Dispose();
```

### 2. Win32Constants - Win32 常量

**职责**: 定义 Win32 API 使用的常量值。

**常量分类**:

| 分类 | 常量 | 说明 |
|------|------|------|
| 窗口扩展样式 | WS_EX_LAYERED | 分层窗口 |
| | WS_EX_TRANSPARENT | 透明窗口（鼠标穿透） |
| | WS_EX_TOPMOST | 置顶窗口 |
| | WS_EX_TOOLWINDOW | 工具窗口（不显示在任务栏） |
| 窗口样式 | WS_POPUP | 弹出窗口 |
| | WS_VISIBLE | 可见窗口 |
| 热键修饰符 | MOD_ALT | Alt 键 |
| | MOD_CONTROL | Ctrl 键 |
| | MOD_SHIFT | Shift 键 |
| | MOD_WIN | Win 键 |
| 消息 | WM_HOTKEY | 热键消息 |
| 透明度 | LWA_ALPHA | Alpha 透明度标志 |

**结构体**:
- `RECT`: 矩形区域
- `POINT`: 点坐标
- `MARGINS`: 边距（用于 DWM）

### 3. Win32Methods - Win32 API 方法

**职责**: 封装 Win32 API 的 P/Invoke 声明。

**API 分类**:

#### 窗口操作
- `SetWindowPos`: 设置窗口位置和层级
- `GetWindowRect`: 获取窗口矩形区域
- `SetWindowLong`: 设置窗口扩展样式
- `GetWindowLong`: 获取窗口扩展样式
- `SetLayeredWindowAttributes`: 设置分层窗口属性
- `GetForegroundWindow`: 获取前台窗口
- `ShowWindow`: 显示窗口

#### 热键操作
- `RegisterHotKey`: 注册热键
- `UnregisterHotKey`: 注销热键

#### 消息窗口
- `CreateWindowEx`: 创建窗口
- `DestroyWindow`: 销毁窗口
- `DefWindowProc`: 默认窗口过程
- `RegisterClass`: 注册窗口类

#### 键盘操作
- `VkKeyScan`: 获取虚拟键码
- `GetKeyState`: 获取键状态
- `GetAsyncKeyState`: 获取异步键状态

#### DWM 操作
- `DwmExtendFrameIntoClientArea`: 扩展帧到客户区
- `DwmSetWindowAttribute`: 设置窗口属性

**辅助方法**:
- `SetWindowTransparentClickable`: 设置窗口透明点击穿透
- `SetWindowInteractive`: 设置窗口可交互
- `GetVirtualKeyCode`: 获取虚拟键码（支持特殊键映射）

## 依赖关系

**依赖**:
- CrosshairPro.Core

**被依赖**:
- CrosshairPro.Services
- CrosshairPro.Application
- CrosshairPro.App

## 设计原则

1. **P/Invoke 封装**: 所有 Win32 API 调用都封装在 `Win32Methods` 中
2. **不安全代码**: 允许使用 `unsafe` 块（项目配置 `AllowUnsafeBlocks`）
3. **资源管理**: 实现 `IDisposable` 接口，确保句柄正确释放
4. **错误处理**: 使用 `SetLastError = true` 并检查返回值

## 使用场景

### 1. 创建透明点击穿透窗口

```csharp
// 获取窗口句柄
var hwnd = new WindowInteropHelper(window).Handle;

// 设置为透明点击穿透
Win32Methods.SetWindowTransparentClickable(hwnd);
```

### 2. 注册全局热键

```csharp
var manager = new HotkeyManager();
manager.RegisterHotkey(new HotkeyBinding
{
    Id = "toggle",
    Combo = "Ctrl+F1"
});

manager.HotkeyTriggered += (s, e) =>
{
    Console.WriteLine($"热键触发: {e.Binding.Id}");
};
```

### 3. 获取前台窗口信息

```csharp
var hwnd = Win32Methods.GetForegroundWindow();
Win32Methods.GetWindowThreadProcessId(hwnd, out var processId);
var process = Process.GetProcessById(processId);
Console.WriteLine($"前台窗口进程: {process.ProcessName}");
```

## 相关文档

- [数据模型](data-model.md) - Win32 结构体和常量定义
- [坑点](pitfalls.md) - P/Invoke 和资源管理陷阱
- [变更日志](CHANGELOG.md) - 模块变更历史