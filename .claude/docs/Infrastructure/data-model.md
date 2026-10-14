# CrosshairPro.Infrastructure - 数据模型

本模块自有数据：Win32 结构体、常量与虚拟键码映射。热键业务模型（`KeyCombo`、`HotkeyBinding`、`HotkeyAction`、`HotkeyTriggeredEventArgs`）定义在 Core 模块，完整定义见 [../Core/data-model.md](../Core/data-model.md)；本文件只记录 Infrastructure 消费这些类型时的视角与运行时数据。

## Win32 结构体

### RECT - 矩形区域（Win32Constants.cs）

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
    public int Width => Right - Left;
    public int Height => Bottom - Top;
}
```

**用途**：`GetWindowRect` 的出参，表示窗口矩形边界；`Width`/`Height` 为计算属性。

### POINT - 点坐标（Win32Constants.cs）

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}
```

### MARGINS - 边距（Win32Constants.cs）

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct MARGINS
{
    public int Left;
    public int Right;
    public int Top;
    public int Bottom;
}
```

**用途**：`DwmExtendFrameIntoClientArea` 的入参，指定向客户区扩展帧的边距；原生签名为 `const MARGINS*`（纯输入参数，函数不向其写回），P/Invoke 声明的 `ref` 仅是编组惯例。该 API 当前在仓库内仅声明未被调用。

### WNDCLASS - 窗口类（Win32Methods.cs）

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct WNDCLASS
{
    public int style;
    public IntPtr lpfnWndProc;   // 窗口过程函数指针
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public string lpszMenuName;
    public string lpszClassName; // 窗口类名，热键窗口用 "CrosshairProHotkeyWindow"
}
```

**关键字段**：`lpfnWndProc`（必须来自被字段持有的委托，见 [pitfalls.md](pitfalls.md)）、`lpszClassName`。

## Win32 常量（Win32Constants.cs）

### 窗口扩展样式

| 常量 | 值 | 说明 |
|------|-----|------|
| WS_EX_LAYERED | 0x00080000 | 分层窗口，支持透明度 |
| WS_EX_TRANSPARENT | 0x00000020 | 鼠标事件穿透 |
| WS_EX_TOPMOST | 0x00000008 | 置顶窗口 |
| WS_EX_TOOLWINDOW | 0x00000080 | 工具窗口，不显示在任务栏 |
| WS_EX_APPWINDOW | 0x00040000 | 应用窗口，显示在任务栏 |

### 窗口样式

| 常量 | 值 | 说明 |
|------|-----|------|
| WS_POPUP | 0x80000000 | 弹出窗口（uint） |
| WS_VISIBLE | 0x10000000 | 可见窗口 |

### SetWindowPos 标志

| 常量 | 值 | 说明 |
|------|-----|------|
| SWP_NOSIZE | 0x0001 | 保持当前大小 |
| SWP_NOMOVE | 0x0002 | 保持当前位置 |
| SWP_NOZORDER | 0x0004 | 保持当前 Z 顺序 |
| SWP_NOACTIVATE | 0x0010 | 不激活窗口 |
| SWP_SHOWWINDOW | 0x0040 | 显示窗口 |
| SWP_NOOWNERZORDER | 0x0200 | 不改变所有者窗口 Z 顺序 |

### 窗口层级

| 常量 | 值 | 说明 |
|------|-----|------|
| HWND_TOPMOST | -1 | 置顶 |
| HWND_NOTOPMOST | -2 | 取消置顶 |
| HWND_TOP | 0 | 顶部 |
| HWND_BOTTOM | 1 | 底部 |

### 热键修饰键

| 常量 | 值 | 说明 |
|------|-----|------|
| MOD_ALT | 0x0001 | Alt 键 |
| MOD_CONTROL | 0x0002 | Ctrl 键 |
| MOD_SHIFT | 0x0004 | Shift 键 |
| MOD_WIN | 0x0008 | Win 键 |

> 注意：源码未定义 `MOD_NOREPEAT`（0x4000）。旧文档列出的该常量已删除；若要防连按重复触发，需自行在 `Win32Constants` 补充并在注册时 OR 进修饰键（见 [pitfalls.md](pitfalls.md)）。

### Windows 消息

| 常量 | 值 | 说明 |
|------|-----|------|
| WM_HOTKEY | 0x0312 | 热键消息 |
| WM_DESTROY | 0x0002 | 窗口销毁消息 |
| WM_CLOSE | 0x0010 | 窗口关闭消息 |

### 透明度标志

| 常量 | 值 | 说明 |
|------|-----|------|
| LWA_ALPHA | 0x02 | 使用 Alpha 透明度 |
| LWA_COLORKEY | 0x01 | 使用颜色键透明 |

## 虚拟键码映射（Win32Methods.GetVirtualKeyCode）

键名先 `ToUpperInvariant` 再比较，大小写不敏感。

### 特殊键

| 键名 | 虚拟键码 | 键名 | 虚拟键码 |
|------|---------|------|---------|
| F1-F12 | 0x70-0x7B | BACK | 0x08 |
| ESC | 0x1B | INSERT | 0x2D |
| ENTER | 0x0D | DELETE | 0x2E |
| TAB | 0x09 | HOME / END | 0x24 / 0x23 |
| SPACE | 0x20 | PAGEUP / PAGEDOWN | 0x21 / 0x22 |
| UP / DOWN | 0x26 / 0x28 | LEFT / RIGHT | 0x25 / 0x27 |
| PLUS | 0x6B | MINUS | 0x6D |

### 字母与数字

- `A`-`Z`：0x41-0x5A
- `0`-`9`：0x30-0x39

### 兜底规则

- 其余长度为 1 的字符：`VkKeyScan(ch) & 0xFF`
- 空串或未识别的多字符键名：返回 0（`Register` 视为注册失败）

## Core 模型（Infrastructure 消费视角）

`KeyCombo` 结构、`Parse`/`ToString` 语义与示例、`HotkeyBinding` 字段表、`HotkeyAction` 枚举六个取值、`HotkeyTriggeredEventArgs` 的 `Binding` 属性，均已在 [../Core/data-model.md](../Core/data-model.md) 详述，此处不重复。Infrastructure 消费侧特有事实：

- `Enabled` **不被本模块检查**——是否注册由调用方决定（见 [pitfalls.md](pitfalls.md) 第 6 条）
- `Combo` 解析出的 `Modifiers`（MOD_* 位标志）被 `WinHotkeyProvider.Register` 直接用作系统 `RegisterHotKey` 的修饰键参数；键名侧经本模块 `GetVirtualKeyCode` 映射为虚拟键码（见上文）

## 热键运行时数据

| 数据 | 位置 | 说明 |
|------|------|------|
| `_registeredKeys: Dictionary<int, KeyCombo>` | WinHotkeyProvider | 热键 ID → 组合键 |
| `_nextId` | WinHotkeyProvider | ID 计数器，从 1 递增，注销后不复用 |
| `_bindings: Dictionary<string, HotkeyBinding>` | HotkeyManager | 绑定 Id → 绑定实例 |
| `_hotkeyIdToBindingId: Dictionary<int, string>` | HotkeyManager | 系统 ID → 绑定 Id；反查用 `FirstOrDefault`，O(n) |
| `_idToBindingMap: Dictionary<int, string>` | WinHotkeyProvider | 预留字典，当前无写入方 |
| `_hwnd` 消息窗口句柄 | WinHotkeyProvider | 类名 `CrosshairProHotkeyWindow` 的不可见窗口，接收 `WM_HOTKEY` |

## 内存布局

- 所有结构体使用 `[StructLayout(LayoutKind.Sequential)]`，保证与 Win32 期望布局一致
- `WNDCLASS` 额外指定 `CharSet.Auto`，字符串字段按系统自动选择 ANSI/Unicode
- 句柄一律使用 `IntPtr`，64 位进程下长度正确（见 [pitfalls.md](pitfalls.md)）
