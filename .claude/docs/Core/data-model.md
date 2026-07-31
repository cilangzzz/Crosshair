# CrosshairPro.Core - 数据模型

## AppPersistedState（新增）

应用持久化状态，记录应用运行时需要保存的状态：

```csharp
public class AppPersistedState
{
    public string? CurrentPresetId { get; set; }  // 当前使用的预设ID
    public bool IsConfigModified { get; set; }    // 当前配置是否已修改
}
```

**用途**：
- 应用启动时恢复上次使用的预设
- 记录配置修改状态（未实现：用于提示用户保存）

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

## GameConfig（新增）

游戏配置数据模型，存储单个游戏的配置数据：

```csharp
public partial class GameConfig : ObservableObject
{
    public string GameId { get; set; } = string.Empty;
    
    [ObservableProperty]
    private string _launchOptions = string.Empty;
    
    public Dictionary<string, object> Settings { get; set; } = new();
}
```

| 属性 | 类型 | 说明 |
|------|------|------|
| GameId | string | 游戏ID（对应 GameProfile.Id） |
| LaunchOptions | string | 启动项参数 |
| Settings | Dictionary<string, object> | 配置项字典（键为配置项ID，值为配置值） |

**用途**：
- 存储每个游戏的特定配置数据
- 支持启动项参数管理
- 通过 Settings 字典存储任意类型的配置值

## GameConfigStrategy（新增）

游戏配置策略定义，描述每个游戏支持的配置项和操作方式：

```csharp
public class GameConfigStrategy
{
    public string GameId { get; set; } = string.Empty;
    public bool SupportsLaunchOptions { get; set; } = true;
    public string? LaunchOptionsDescription { get; set; }
    public List<ConfigSectionDefinition> Sections { get; set; } = new();
    public string? ConfigFilePath { get; set; }
}
```

| 属性 | 类型 | 说明 |
|------|------|------|
| GameId | string | 游戏ID |
| SupportsLaunchOptions | bool | 是否支持启动项 |
| LaunchOptionsDescription | string? | 启动项说明 |
| Sections | List<ConfigSectionDefinition> | 配置分区列表 |
| ConfigFilePath | string? | 配置文件路径模板（支持环境变量） |

### ConfigItemType 枚举

配置项类型枚举：

```csharp
public enum ConfigItemType
{
    Bool,    // 布尔开关
    Int,     // 整数数值
    Enum,    // 枚举选择
    String   // 字符串
}
```

### ConfigItemDefinition 类

配置项定义，描述单个配置项的元数据：

```csharp
public class ConfigItemDefinition
{
    public string Key { get; set; }              // 配置项ID（唯一键）
    public string DisplayName { get; set; }      // 显示名称
    public ConfigItemType Type { get; set; }     // 配置项类型
    public object? DefaultValue { get; set; }    // 默认值
    public int? MinValue { get; set; }           // 最小值（Int类型）
    public int? MaxValue { get; set; }           // 最大值（Int类型）
    public List<string>? Options { get; set; }   // 枚举选项（Enum类型）
    public string? Description { get; set; }     // 描述说明
    public bool RequiresRestart { get; set; }    // 是否需要重启游戏生效
}
```

### ConfigSectionDefinition 类

配置分区定义，将配置项按功能分组：

```csharp
public class ConfigSectionDefinition
{
    public string Name { get; set; }                               // 分区名称
    public string DisplayName { get; set; }                        // 分区显示名称
    public List<ConfigItemDefinition> Items { get; set; } = new(); // 配置项列表
}
```

**用途**：
- 定义每个游戏支持的配置项结构
- 为 UI 提供配置项元数据（显示名称、类型、范围等）
- 支持配置验证和默认值处理

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

GameConfig (独立，关联 GameProfile.Id)
    └── Settings: Dictionary<string, object>

GameConfigStrategy (独立，关联 GameProfile.Id)
    └── Sections: List<ConfigSectionDefinition>
            └── Items: List<ConfigItemDefinition>

HotkeyBinding (独立)
GameInfo (独立)
```

**模型关联说明**：
- `GameConfig.GameId` 关联 `GameProfile.Id`，存储对应游戏的配置数据
- `GameConfigStrategy.GameId` 关联 `GameProfile.Id`，定义对应游戏的配置策略
- `GameConfig.Settings` 中的键对应 `ConfigItemDefinition.Key`
- `GameConfigStrategy` 为 `GameConfig` 提供元数据和验证规则