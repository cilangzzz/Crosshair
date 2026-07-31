# CrosshairPro.Infrastructure 数据模型

## Win32 结构体

### RECT - 矩形区域

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

**用途**: 表示窗口或区域的矩形边界。

**字段**:
- `Left`: 左边界 X 坐标
- `Top`: 上边界 Y 坐标
- `Right`: 右边界 X 坐标
- `Bottom`: 下边界 Y 坐标

**计算属性**:
- `Width`: 宽度
- `Height`: 高度

### POINT - 点坐标

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}
```

**用途**: 表示二维坐标点。

**字段**:
- `X`: X 坐标
- `Y`: Y 坐标

### MARGINS - 边距

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

**用途**: 用于 DWM (Desktop Window Manager) 扩展帧到客户区。

**字段**:
- `Left`: 左边距
- `Right`: 右边距
- `Top`: 上边距
- `Bottom`: 下边距

### WNDCLASS - 窗口类

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct WNDCLASS
{
    public int style;
    public IntPtr lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public string lpszMenuName;
    public string lpszClassName;
}
```

**用途**: 注册窗口类时使用的结构体。

**关键字段**:
- `lpfnWndProc`: 窗口过程函数指针
- `lpszClassName`: 窗口类名称
- `hInstance`: 应用程序实例句柄

## Win32 常量

### 窗口扩展样式 (Window Extended Styles)

| 常量 | 值 | 说明 |
|------|-----|------|
| WS_EX_LAYERED | 0x00080000 | 分层窗口，支持透明度 |
| WS_EX_TRANSPARENT | 0x00000020 | 透明窗口，鼠标事件穿透 |
| WS_EX_TOPMOST | 0x00000008 | 置顶窗口 |
| WS_EX_TOOLWINDOW | 0x00000080 | 工具窗口，不显示在任务栏 |
| WS_EX_APPWINDOW | 0x00040000 | 应用程序窗口，显示在任务栏 |

### 窗口样式 (Window Styles)

| 常量 | 值 | 说明 |
|------|-----|------|
| WS_POPUP | 0x80000000 | 弹出窗口 |
| WS_VISIBLE | 0x10000000 | 可见窗口 |

### SetWindowPos 标志

| 常量 | 值 | 说明 |
|------|-----|------|
| SWP_NOSIZE | 0x0001 | 保持当前大小 |
| SWP_NOMOVE | 0x0002 | 保持当前位置 |
| SWP_NOZORDER | 0x0004 | 保持当前 Z 顺序 |
| SWP_NOACTIVATE | 0x0010 | 不激活窗口 |
| SWP_SHOWWINDOW | 0x0040 | 显示窗口 |
| SWP_NOOWNERZORDER | 0x0200 | 不改变所有者窗口的 Z 顺序 |

### 窗口层级常量

| 常量 | 值 | 说明 |
|------|-----|------|
| HWND_TOPMOST | -1 | 置顶 |
| HWND_NOTOPMOST | -2 | 非置顶 |
| HWND_TOP | 0 | 顶部 |
| HWND_BOTTOM | 1 | 底部 |

### 热键修饰符

| 常量 | 值 | 说明 |
|------|-----|------|
| MOD_ALT | 0x0001 | Alt 键 |
| MOD_CONTROL | 0x0002 | Ctrl 键 |
| MOD_SHIFT | 0x0004 | Shift 键 |
| MOD_WIN | 0x0008 | Win 键 |

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

## 内部数据结构

### KeyCombo - 组合键

HotkeyManager 内部使用的组合键表示。

```csharp
public class KeyCombo
{
    public string Key { get; set; }
    public int Modifiers { get; set; }

    public static KeyCombo Parse(string combo);
}
```

**解析规则**:
- 格式: `"Ctrl+Shift+F1"`
- 支持 `+` 分隔符
- 自动转换为 Win32 修饰符标志

**示例**:
```csharp
var combo = KeyCombo.Parse("Ctrl+Alt+F1");
// combo.Key = "F1"
// combo.Modifiers = MOD_CONTROL | MOD_ALT
```

## 虚拟键码映射

### 特殊键

| 键名 | 虚拟键码 | 说明 |
|------|---------|------|
| F1-F12 | 0x70-0x7B | 功能键 |
| ESC | 0x1B | Escape 键 |
| ENTER | 0x0D | 回车键 |
| TAB | 0x09 | Tab 键 |
| SPACE | 0x20 | 空格键 |
| BACK | 0x08 | 退格键 |
| INSERT | 0x2D | Insert 键 |
| DELETE | 0x2E | Delete 键 |
| HOME | 0x24 | Home 键 |
| END | 0x23 | End 键 |
| PAGEUP | 0x21 | Page Up 键 |
| PAGEDOWN | 0x22 | Page Down 键 |
| UP | 0x26 | 上箭头 |
| DOWN | 0x28 | 下箭头 |
| LEFT | 0x25 | 左箭头 |
| RIGHT | 0x27 | 右箭头 |

### 字母和数字

- A-Z: 0x41-0x5A
- 0-9: 0x30-0x39

### 符号键

- PLUS: 0x6B (数字键盘加号)
- MINUS: 0x6D (数字键盘减号)

## 句柄管理

### 窗口句柄 (HWND)

- 类型: `IntPtr`
- 获取方式: `new WindowInteropHelper(window).Handle`
- 释放: 由 Windows 自动管理

### 热键 ID

- 类型: `int`
- 范围: 1 到 0xBFFF (49151)
- 管理: 由 `HotkeyManager` 自动分配

## 内存布局

### StructLayout 特性

所有 Win32 结构体都使用 `[StructLayout(LayoutKind.Sequential)]` 确保内存布局与 Win32 API 兼容。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct RECT { ... }
```

### CharSet 特性

字符串字段使用 `CharSet.Auto` 自动选择 ANSI 或 Unicode:

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct WNDCLASS { ... }
```