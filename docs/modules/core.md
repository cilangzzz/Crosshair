# Core 核心层文档

## 概述

`CrosshairPro.Core` 是项目的核心层，定义了业务模型、枚举和接口契约。该层不包含任何业务实现，仅定义数据结构和服务接口。

**项目路径**：`src/CrosshairPro.Core/`

**目标框架**：net8.0

**依赖**：CommunityToolkit.Mvvm 8.3.2, System.Text.Json 8.0.5

---

## 目录结构

```
CrosshairPro.Core/
├── Enums/
│   ├── AppState.cs              # 应用状态枚举
│   └── CrosshairStyle.cs        # 准星样式枚举
├── Events/                      # 事件定义（待实现）
├── Interfaces/
│   ├── IConfigRepository.cs     # 配置仓库接口
│   ├── ICrosshairRenderer.cs    # 准星渲染器接口
│   ├── IGameDetector.cs         # 游戏检测器接口
│   └── IHotkeyManager.cs        # 热键管理器接口
└── Models/
    ├── CrosshairConfig.cs       # 准星配置模型
    ├── EffectsConfig.cs         # 效果配置模型
    ├── GameInfo.cs              # 游戏信息记录
    ├── GameProfile.cs           # 游戏配置文件模型
    ├── HotkeyBinding.cs         # 热键绑定模型
    └── Preset.cs                # 预设模型
```

---

## 枚举（Enums）

### CrosshairStyle

准星样式枚举，定义了所有支持的准星类型。

**文件**：`src/CrosshairPro.Core/Enums/CrosshairStyle.cs`

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Cross | 十字准星 |
| 1 | Dot | 圆点准星 |
| 2 | Circle | 圆形准星 |
| 3 | TShape | T 形准星 |
| 4 | XShape | X 形准星 |
| 5 | CustomImage | 自定义图片准星 |

### AppState

应用状态枚举，用于跟踪应用当前状态。

**文件**：`src/CrosshairPro.Core/Enums/AppState.cs`

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Idle | 空闲状态 |
| 1 | GameMode | 游戏模式 |
| 2 | CrosshairVisible | 准星可见 |
| 3 | CrosshairHidden | 准星隐藏 |

---

## 接口（Interfaces）

### IConfigRepository

配置仓库接口，定义配置的加载、保存、重置、导入和导出操作。

**文件**：`src/CrosshairPro.Core/Interfaces/IConfigRepository.cs`

```csharp
public interface IConfigRepository
{
    Task<CrosshairConfig> LoadConfigAsync();
    Task SaveConfigAsync(CrosshairConfig config);
    Task ResetToDefaultAsync();
    Task ExportConfigAsync(string filePath);
    Task<CrosshairConfig> ImportConfigAsync(string filePath);
}
```

### IPresetRepository

预设仓库接口，定义预设的 CRUD 和导入导出操作。

**文件**：`src/CrosshairPro.Core/Interfaces/IConfigRepository.cs`（与 IConfigRepository 同一文件）

```csharp
public interface IPresetRepository
{
    Task<IEnumerable<Preset>> LoadPresetsAsync();
    Task SavePresetAsync(Preset preset);
    Task DeletePresetAsync(string presetId);
    Task<Preset?> GetPresetAsync(string presetId);
    Task ExportPresetAsync(string presetId, string filePath);
    Task<Preset> ImportPresetAsync(string filePath);
}
```

### ICrosshairRenderer

准星渲染器接口，定义准星渲染操作。

**文件**：`src/CrosshairPro.Core/Interfaces/ICrosshairRenderer.cs`

```csharp
public interface ICrosshairRenderer
{
    void Render(object drawingContext, CrosshairConfig config, double width, double height);
    event EventHandler RenderCompleted;
}
```

### IGameDetector

游戏检测器接口，定义游戏进程监控和配置文件注册操作。

**文件**：`src/CrosshairPro.Core/Interfaces/IGameDetector.cs`

```csharp
public interface IGameDetector
{
    GameInfo? CurrentGame { get; }
    Task StartMonitoring();
    Task StopMonitoring();
    Task InitializeAsync();
    void RegisterGameProfile(GameProfile profile);
    IEnumerable<GameProfile> GetRegisteredGames();
    event EventHandler<GameDetectedEventArgs> GameStarted;
    event EventHandler<GameDetectedEventArgs> GameExited;
}
```

### IHotkeyManager

热键管理器接口，定义热键注册、注销和事件处理操作。

**文件**：`src/CrosshairPro.Core/Interfaces/IHotkeyManager.cs`

```csharp
public interface IHotkeyManager
{
    bool RegisterHotkey(HotkeyBinding binding);
    bool UnregisterHotkey(string bindingId);
    void UnregisterAll();
    event EventHandler<HotkeyBinding> HotkeyTriggered;
}
```

---

## 模型（Models）

### CrosshairConfig

准星配置模型，是系统的核心配置类。继承自 `ObservableObject`，支持属性变更通知。

**文件**：`src/CrosshairPro.Core/Models/CrosshairConfig.cs`

#### 属性

| 属性 | 类型 | 默认值 | 范围 | 说明 |
|------|------|--------|------|------|
| Id | string | (GUID) | - | 配置唯一标识 |
| Name | string | "" | - | 配置名称 |
| Style | CrosshairStyle | Cross | - | 准星样式 |
| Size | int | 20 | 1-100 | 准星大小 |
| Gap | int | 4 | 0-50 | 准星间距 |
| Thickness | int | 2 | 1-10 | 准星粗细 |
| Color | string | "#00FF00" | - | 准星颜色（十六进制） |
| Opacity | int | 100 | 0-100 | 透明度 |
| CenterSize | int | 4 | - | 中心点大小 |
| Rotation | double | 0 | - | 旋转角度 |
| CustomImagePath | string | "" | - | 自定义图片路径 |
| Effects | EffectsConfig | (默认) | - | 效果配置 |
| Display | DisplayConfig | (默认) | - | 显示配置 |

#### 内部类 DisplayConfig

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Monitor | int | 0 | 显示器编号 |
| ClickThrough | bool | true | 点击穿透 |
| AlwaysOnTop | bool | true | 始终置顶 |
| PositionX | double | 0 | X 坐标偏移 |
| PositionY | double | 0 | Y 坐标偏移 |

#### 方法

- `Clone()` → CrosshairConfig：深拷贝配置
- `CopyFrom(CrosshairConfig other)`：从另一个配置复制所有属性

### EffectsConfig

效果配置模型，包含描边、阴影和发光效果配置。

**文件**：`src/CrosshairPro.Core/Models/EffectsConfig.cs`

#### OutlineConfig（描边配置）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | true | 是否启用 |
| Color | string | "#000000" | 描边颜色 |
| Thickness | int | 1 | 描边粗细 |

#### ShadowConfig（阴影配置）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#000000" | 阴影颜色 |
| Blur | int | 3 | 模糊半径 |
| OffsetX | int | 0 | X 偏移 |
| OffsetY | int | 2 | Y 偏移 |

#### GlowConfig（发光配置）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#00FFFF" | 发光颜色 |
| Intensity | int | 50 | 发光强度 |
| Range | int | 10 | 发光范围 |

### Preset

预设模型，保存一组准星配置及其关联信息。

**文件**：`src/CrosshairPro.Core/Models/Preset.cs`

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 预设唯一标识 |
| Name | string | 预设名称 |
| Config | CrosshairConfig | 准星配置 |
| GameAssociation | string | 关联游戏名称 |
| HotkeyBinding | HotkeyBinding | 热键绑定 |
| CreatedAt | DateTime | 创建时间 |
| UpdatedAt | DateTime | 更新时间 |
| IsDefault | bool | 是否默认预设 |

#### 方法

- `Clone()` → Preset：深拷贝预设，名称添加 " (副本)" 后缀

### GameProfile

游戏配置文件模型，定义游戏进程匹配规则和关联预设。

**文件**：`src/CrosshairPro.Core/Models/GameProfile.cs`

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 配置文件唯一标识 |
| DisplayName | string | 游戏显示名称 |
| ProcessName | string | 进程名称（如 "cs2.exe"） |
| Priority | int | 优先级 |
| AutoSwitch | bool | 是否自动切换 |
| PresetId | string | 关联预设 ID |
| FullscreenOnly | bool | 仅全屏模式生效 |
| LastMatchedAt | DateTime? | 最后匹配时间 |

#### 内置游戏配置

`BuiltIn.GetAll()` 返回 8 个内置游戏配置：

| 游戏 | ProcessName |
|------|-------------|
| Counter-Strike 2 | cs2.exe |
| CS:GO | csgo.exe |
| Valorant | valorant-win64-shipping.exe |
| Apex Legends | r5apex.exe |
| Overwatch 2 | overwatch.exe |
| PUBG | tslgame.exe |
| Fortnite | fortnite-win64-shipping.exe |
| Rainbow Six Siege | rainbowSixGame.exe |

#### 方法

- `Matches(string processName)` → bool：不区分大小写匹配进程名

### HotkeyBinding

热键绑定模型，定义热键组合和关联操作。

**文件**：`src/CrosshairPro.Core/Models/HotkeyBinding.cs`

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| Id | string | 绑定唯一标识 |
| Name | string | 绑定名称 |
| Description | string | 描述 |
| Combo | string | 热键组合（如 "Ctrl+Shift+X"） |
| DefaultCombo | string | 默认热键组合 |
| Enabled | bool | 是否启用 |
| Action | HotkeyAction | 关联操作 |
| PresetId | string | 关联预设 ID |

### HotkeyAction

热键操作枚举。

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | ToggleCrosshair | 切换准星显示/隐藏 |
| 1 | SwitchPreset | 切换预设 |
| 2 | ResetPosition | 重置位置 |
| 3 | LockPosition | 锁定位置 |
| 4 | IncreaseSize | 增大尺寸 |
| 5 | DecreaseSize | 减小尺寸 |

### KeyCombo

热键组合结构体，用于解析和格式化热键组合字符串。

**文件**：`src/CrosshairPro.Core/Models/HotkeyBinding.cs`

#### 方法

- `Parse(string combo)` → KeyCombo：解析热键组合字符串
- `ToString()` → string：格式化为热键组合字符串

#### 格式

热键组合字符串格式：`Modifier1+Modifier2+Key`

支持的修饰键：`Ctrl`, `Shift`, `Alt`, `Win`

示例：`Ctrl+Shift+X`, `Alt+F1`

### GameInfo

游戏信息记录，表示一个正在运行的游戏进程。

**文件**：`src/CrosshairPro.Core/Models/GameInfo.cs`

```csharp
public record GameInfo
{
    public string ProcessName { get; init; }
    public string DisplayName { get; init; }
    public int ProcessId { get; init; }
    public DateTime StartTime { get; init; }
}
```

### GameDetectedEventArgs

游戏检测事件参数。

**文件**：`src/CrosshairPro.Core/Models/GameInfo.cs`

```csharp
public class GameDetectedEventArgs : EventArgs
{
    public GameInfo GameInfo { get; init; }
    public GameProfile Profile { get; init; }
}
```
