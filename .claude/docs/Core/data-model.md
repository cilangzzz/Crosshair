# CrosshairPro.Core - 数据模型

## CrosshairConfig

准心配置主模型，使用 CommunityToolkit.Mvvm 的 `ObservableObject` 实现 MVVM 绑定。

```csharp
public partial class CrosshairConfig : ObservableObject
{
    [ObservableProperty] private string _id = Guid.NewGuid().ToString();
    [ObservableProperty] private string _name = "默认配置";
    [ObservableProperty] private CrosshairStyle _style = CrosshairStyle.Cross;
    [ObservableProperty] private int _size = 20;
    [ObservableProperty] private int _gap = 4;
    [ObservableProperty] private int _thickness = 2;
    [ObservableProperty] private string _color = "#00FF00";
    [ObservableProperty] private int _opacity = 100;
    [ObservableProperty] private int _brightness = 100;
    [ObservableProperty] private int _centerSize = 4;
    [ObservableProperty] private int _rotation;
    [ObservableProperty] private string? _customImagePath;
    [ObservableProperty] private EffectsConfig _effects = new();
    [ObservableProperty] private DisplayConfig _display = new();
}
```

### 关键方法

| 方法 | 说明 |
|------|------|
| `Clone()` | 创建深拷贝，递归克隆 Effects 和 Display |
| `CopyFrom(other)` | 从另一个配置复制值，保持当前实例引用 |

### 属性变更通知

由于使用了 `ObservableProperty`，任何属性变更都会自动触发 `PropertyChanged` 事件。
嵌套对象（Effects、Display）的属性变更需要手动订阅。

## EffectsConfig

效果配置，包含三种独立效果：

```csharp
public partial class EffectsConfig : ObservableObject
{
    [ObservableProperty] private OutlineConfig _outline = new();
    [ObservableProperty] private ShadowConfig _shadow = new();
    [ObservableProperty] private GlowConfig _glow = new();
}
```

### OutlineConfig（描边）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | true | 是否启用 |
| Color | string | "#000000" | 描边颜色（黑色） |
| Thickness | int | 1 | 描边厚度 |

### ShadowConfig（阴影）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#000000" | 阴影颜色 |
| Blur | int | 3 | 模糊半径 |
| OffsetX | int | 0 | X轴偏移 |
| OffsetY | int | 2 | Y轴偏移 |

### GlowConfig（发光）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#00FFFF" | 发光颜色（青色） |
| Intensity | int | 50 | 发光强度 |
| Range | int | 10 | 发光范围 |

## DisplayConfig

显示配置：

```csharp
public partial class DisplayConfig : ObservableObject
{
    [ObservableProperty] private string _monitor = "primary";
    [ObservableProperty] private bool _clickThrough = true;
    [ObservableProperty] private bool _alwaysOnTop = true;
    [ObservableProperty] private int _positionX;
    [ObservableProperty] private int _positionY;
}
```

| 属性 | 说明 |
|------|------|
| Monitor | 目标显示器标识，"primary" 表示主显示器 |
| ClickThrough | 鼠标穿透模式，启用时窗口不接收鼠标事件 |
| AlwaysOnTop | 始终置顶 |
| PositionX/Y | 相对于屏幕中心的位置偏移 |

## Preset

预设模型：

```csharp
public class Preset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "新预设";
    public CrosshairConfig Config { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDefault { get; set; }  // 标记默认预设，不可删除
}
```

## HotkeyBinding

热键绑定模型：

```csharp
public class HotkeyBinding
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Combo { get; set; }  // 如 "Ctrl+Shift+F1"
    public string Action { get; set; } // 动作类型
}
```

## GameInfo

游戏信息模型：

```csharp
public class GameInfo
{
    public string ProcessName { get; set; }
    public string DisplayName { get; set; }
    public bool IsRunning { get; set; }
}
```

## GameProfile

游戏配置文件：

```csharp
public class GameProfile
{
    public string Id { get; set; }
    public string GameName { get; set; }
    public string ProcessName { get; set; }
    public CrosshairConfig Config { get; set; }
}
```

## 模型关系图

```
CrosshairConfig
    ├── EffectsConfig
    │       ├── OutlineConfig
    │       ├── ShadowConfig
    │       └── GlowConfig
    └── DisplayConfig

Preset
    └── CrosshairConfig

GameProfile
    └── CrosshairConfig

HotkeyBinding (独立)
GameInfo (独立)
```