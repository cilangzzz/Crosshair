# CrosshairPro.Infrastructure - 数据模型

## KeyCombo

热键组合键结构，用于表示解析后的热键组合：

```csharp
public class KeyCombo
{
    public int Key { get; set; }       // 虚拟键码 (VK_*)
    public int Modifiers { get; set; } // 修饰键组合 (MOD_*)
}
```

### 解析方法

`KeyCombo.Parse(string combo)` 解析组合键字符串：

| 输入 | Key | Modifiers |
|------|-----|-----------|
| "Ctrl+Shift+F1" | VK_F1 (112) | MOD_CONTROL | MOD_SHIFT |
| "Alt+T" | VK_T (84) | MOD_ALT |
| "F5" | VK_F5 (116) | 0 |

## HotkeyBinding

热键绑定模型（定义在 Core 模块）：

```csharp
public class HotkeyBinding
{
    public string Id { get; set; }      // 绑定唯一标识
    public string Name { get; set; }    // 显示名称
    public string Combo { get; set; }   // 组合键字符串
    public string Action { get; set; }  // 动作类型
}
```

## HotkeyTriggeredEventArgs

热键触发事件参数：

```csharp
public class HotkeyTriggeredEventArgs : EventArgs
{
    public HotkeyBinding Binding { get; } // 触发的热键绑定
}
```

## Win32Methods.WNDCLASS

窗口类结构：

```csharp
public struct WNDCLASS
{
    public int style;
    public IntPtr lpfnWndProc;   // 窗口过程函数指针
    public IntPtr hInstance;
    public string lpszClassName; // 窗口类名
}
```

## Win32 相关常量

### 虚拟键码 (Virtual Key Codes)

| 常量 | 值 | 说明 |
|------|-----|------|
| VK_F1 | 112 | F1 键 |
| VK_F5 | 116 | F5 键 |
| VK_T | 84 | T 键 |

### 修饰键 (Modifiers)

| 常量 | 值 | 说明 |
|------|-----|------|
| MOD_ALT | 0x0001 | Alt 键 |
| MOD_CONTROL | 0x0002 | Ctrl 键 |
| MOD_SHIFT | 0x0004 | Shift 键 |
| MOD_NOREPEAT | 0x4000 | 按住时不重复触发 |