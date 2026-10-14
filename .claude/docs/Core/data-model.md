# CrosshairPro.Core - 数据模型

模型、接口、枚举的字段级参考。源码位置 `src/CrosshairPro.Core`。

## 模型关系图

```
CrosshairConfig
    ├── EffectsConfig
    │       ├── OutlineConfig（描边）
    │       ├── ShadowConfig（阴影）
    │       └── GlowConfig（发光）
    └── DisplayConfig

Preset ─── CrosshairConfig（配置快照）
    ├── GameAssociation（关联游戏进程名）
    └── HotkeyBinding（热键绑定）

GameProfile（游戏档案，Id 被 GameConfig / GameConfigStrategy 关联）
    └── BuiltIn：内置 8 款游戏

GameConfig（GameId → GameProfile.Id）
GameConfigStrategy（GameId → GameProfile.Id）
    └── Sections: List<ConfigSectionDefinition>
            └── Items: List<ConfigItemDefinition>

AppPersistedState / HotkeyBinding / GameInfo / Apex 三件套（独立）
```

**关联说明**：
- `GameConfig.GameId` 与 `GameConfigStrategy.GameId` 均关联 `GameProfile.Id`
- `GameConfig.Settings` 的键对应 `ConfigItemDefinition.Key`
- `GameConfigStrategy` 为 `GameConfig` 提供元数据与验证规则

## CrosshairConfig — 准心配置主模型（CrosshairConfig.cs）

`ObservableObject` 派生，全部属性经 `[ObservableProperty]` 生成，变更自动触发 `PropertyChanged`。

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 唯一标识 |
| Name | string | "默认配置" | 配置名称 |
| Style | CrosshairStyle | Cross | 准心样式 |
| Size | int | 20 | 准心大小 |
| Gap | int | 4 | 中心间隙 |
| Thickness | int | 2 | 线条粗细 |
| Color | string | "#00FF00" | 颜色（#RRGGBB） |
| Opacity | int | 100 | 不透明度 0-100 |
| Brightness | int | 100 | 亮度 0-200 |
| CenterSize | int | 4 | 中心点大小 |
| Rotation | int | 0 | 旋转角度 |
| CustomImagePath | string? | null | 自定义图片路径（Style=CustomImage 时使用） |
| Effects | EffectsConfig | new() | 效果配置 |
| Display | DisplayConfig | new() | 显示配置 |

### 方法

| 方法 | 行为 |
|------|------|
| `Clone()` | 深拷贝：递归克隆 Effects（Outline/Shadow/Glow）与 Display，生成新 GUID |
| `CopyFrom(other)` | 原地复制全部值（含 Effects/Display 的子属性），保留当前 Id |

### DisplayConfig（定义于 CrosshairConfig.cs）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Monitor | string | "primary" | 目标显示器，"primary" 为主显示器 |
| ClickThrough | bool | true | 鼠标穿透（WS_EX_TRANSPARENT） |
| AlwaysOnTop | bool | true | 始终置顶 |
| PositionX | int | 0 | 相对屏幕中心的 X 偏移 |
| PositionY | int | 0 | 相对屏幕中心的 Y 偏移 |

## EffectsConfig — 效果配置（EffectsConfig.cs）

容器，三个子配置均为 `ObservableObject`：

| 属性 | 类型 | 说明 |
|------|------|------|
| Outline | OutlineConfig | 描边 |
| Shadow | ShadowConfig | 阴影 |
| Glow | GlowConfig | 发光 |

### OutlineConfig（描边）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | true | 是否启用（默认开启） |
| Color | string | "#000000" | 描边颜色 |
| Thickness | int | 1 | 描边厚度 |

### ShadowConfig（阴影）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#000000" | 阴影颜色 |
| Blur | int | 3 | 模糊半径 |
| OffsetX | int | 0 | X 偏移 |
| OffsetY | int | 2 | Y 偏移 |

### GlowConfig（发光）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#00FFFF" | 发光颜色 |
| Intensity | int | 50 | 强度 |
| Range | int | 10 | 范围 |

## Preset — 预设（Preset.cs）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 预设 ID |
| Name | string | "新预设" | 预设名称 |
| Config | CrosshairConfig | new() | 配置快照 |
| GameAssociation | string? | null | 关联的游戏（进程名） |
| HotkeyBinding | string? | null | 热键绑定 |
| CreatedAt | DateTime | UtcNow | 创建时间 |
| UpdatedAt | DateTime | UtcNow | 更新时间 |
| IsDefault | bool | false | 默认预设标记（不可删除/覆盖保存） |

- `ToString()` 返回 `Name`。
- `Clone()`：深拷贝 Config，生成新 GUID，名称追加 " (副本)"，IsDefault 置 false，时间戳重置为当前 UTC。

## AppPersistedState — 应用持久化状态（AppPersistedState.cs）

| 属性 | 类型 | 说明 |
|------|------|------|
| CurrentPresetId | string? | 当前使用的预设 ID；null/空表示使用默认配置 |
| IsConfigModified | bool | 当前配置是否已修改（未保存到预设） |
| Language | string? | 界面语言（如 "zh-CN"、"en-US"）；null 表示跟随系统 |

由 Services 层 `JsonConfigRepository`（实现 IAppStateRepository）持久化为 `appstate.json`。

## HotkeyBinding — 热键绑定（HotkeyBinding.cs）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 绑定 ID |
| Name | string | "" | 名称 |
| Description | string | "" | 描述 |
| Combo | string | "" | 按键组合字符串，如 "Ctrl+Shift+F1" |
| DefaultCombo | string | "" | 出厂默认组合（用于恢复默认） |
| Enabled | bool | true | 是否启用 |
| Action | HotkeyAction | ToggleCrosshair(0) | 动作类型（普通属性，非可观察） |
| PresetId | string? | null | SwitchPreset 动作关联的预设 ID（普通属性，非可观察） |

### HotkeyAction 枚举

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | ToggleCrosshair | 显示/隐藏准心 |
| 1 | SwitchPreset | 切换预设（配合 PresetId） |
| 2 | ResetPosition | 重置位置 |
| 3 | LockPosition | 锁定/解锁位置 |
| 4 | IncreaseSize | 增大大小 |
| 5 | DecreaseSize | 减小大小 |

### KeyCombo 结构

按键组合的解析与还原：

| 成员 | 说明 |
|------|------|
| Ctrl / Shift / Alt / Win | 修饰键布尔位 |
| Key | 主键（字符串） |
| Modifiers | Win32 MOD_* 位标志：Alt=0x0001、Control=0x0002、Shift=0x0004、Win=0x0008 |
| `Parse(combo)` | 解析 "+" 分隔字符串；大小写不敏感；最后一段非修饰键作为 Key |
| `ToString()` | 还原为 "Ctrl+Shift+KEY" 形式（Key 转大写） |

## GameInfo — 游戏信息（GameInfo.cs）

C# record，不可变：

```csharp
public record GameInfo(string ProcessName, string DisplayName, int ProcessId, DateTime StartTime);
```

同文件定义 `GameDetectedEventArgs(GameInfo Game, bool IsStarting)`，供 `IGameDetector` 的 `GameStarted` / `GameExited` 事件使用。

## GameProfile — 游戏档案（GameProfile.cs）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 档案 ID（GameConfig/GameConfigStrategy 通过它关联） |
| DisplayName | string | "" | 显示名称 |
| ProcessName | string | "" | 进程名（不含 .exe） |
| Priority | int | 100 | 匹配优先级 |
| AutoSwitch | bool | true | 是否自动切换配置 |
| PresetId | string? | null | 关联预设 |
| FullscreenOnly | bool | false | 仅全屏时生效 |
| LastMatchedAt | DateTime? | null | 最近一次匹配时间 |
| IsSelected | bool | false | UI 选中态 |

- `Matches(processName)`：与 ProcessName 做 `OrdinalIgnoreCase` 完全匹配。
- 嵌套静态类 `BuiltIn`，`GetAll()` 返回内置 8 款游戏：

| Id | DisplayName | ProcessName | Priority | FullscreenOnly |
|----|-------------|-------------|----------|----------------|
| builtin-cs2 | Counter-Strike 2 | cs2 | 100 | 否 |
| builtin-csgo | CS:GO | csgo | 90 | 否 |
| builtin-valorant | Valorant | VALORANT-Win64-Shipping | 100 | 是 |
| builtin-apex | Apex Legends | r5apex | 100 | 否 |
| builtin-overwatch2 | Overwatch 2 | Overwatch | 100 | 否 |
| builtin-pubg | PUBG | TslGame | 90 | 否 |
| builtin-fortnite | Fortnite | FortniteClient-Win64-Shipping | 90 | 否 |
| builtin-r6 | Rainbow Six Siege | RainbowSix | 90 | 否 |

## GameConfig — 游戏配置数据（GameConfig.cs）

| 属性 | 类型 | 说明 |
|------|------|------|
| GameId | string | 游戏 ID（对应 GameProfile.Id）；普通属性 |
| LaunchOptions | string | 启动项参数；[ObservableProperty] 可观察 |
| Settings | Dictionary&lt;string, object&gt; | 配置值字典，键对应 ConfigItemDefinition.Key；普通属性 |

由 Application 层 `GameConfigService` 按各游戏策略创建与缓存（内置 CS2/Valorant/Apex/Overwatch2/PUBG 等策略），见 [../Application/README.md](../Application/README.md)。

## GameConfigStrategy — 游戏配置策略（GameConfigStrategy.cs）

| 属性 | 类型 | 说明 |
|------|------|------|
| GameId | string | 游戏 ID |
| SupportsLaunchOptions | bool | 是否支持启动项（默认 true） |
| LaunchOptionsDescription | string? | 启动项说明 |
| Sections | List&lt;ConfigSectionDefinition&gt; | 配置分区列表 |
| ConfigFilePath | string? | 配置文件路径模板（支持环境变量） |

### ConfigItemType 枚举

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Bool | 布尔开关 |
| 1 | Int | 整数数值 |
| 2 | Enum | 枚举选择 |
| 3 | String | 字符串 |

### ConfigItemDefinition — 配置项元数据

| 属性 | 类型 | 说明 |
|------|------|------|
| Key | string | 配置项唯一键（GameConfig.Settings 的键） |
| DisplayName | string | 显示名称 |
| Type | ConfigItemType | 类型 |
| DefaultValue | object? | 默认值 |
| MinValue | int? | 最小值（Int 类型） |
| MaxValue | int? | 最大值（Int 类型） |
| Options | List&lt;string&gt;? | 枚举选项（Enum 类型） |
| Description | string? | 描述说明 |
| RequiresRestart | bool | 是否需重启游戏生效 |

### ConfigSectionDefinition — 配置分区

| 属性 | 类型 | 说明 |
|------|------|------|
| Name | string | 分区名 |
| DisplayName | string | 显示名 |
| Items | List&lt;ConfigItemDefinition&gt; | 配置项列表 |

## Apex 配置模型三件套

对应 Apex Legends 本地配置文件（目录 `%USERPROFILE%\Saved Games\Respawn\Apex\local\`），均为 `ObservableObject` 派生，属性名与游戏内 cvar 一一对应。文件读写、备份与预设由 Application 层 `ApexConfigService` 完成，见 [../Application/README.md](../Application/README.md)。

### ApexVideoConfig — videoconfig.txt（KeyValues 格式）

59 个属性，按分区归纳（括号为默认值）：

| 分区 | 代表属性 |
|------|----------|
| 显示 | DefaultRes(1920) / DefaultResHeight(1080)、RefreshRate(144)、ResolutionScale(100)、Fullscreen(1)、Nowindowborder(1)、WindowMode(0)、Gamma(1.0)、MonitorGamma(2.2) |
| 分辨率联动 | `ResolutionOptions` 静态表（13 档）、ResolutionIndex(5 = 1920x1080)；索引变更经 `OnResolutionIndexChanged` 自动回写 DefaultRes/DefaultResHeight，`UpdateResolutionIndex()` 反向计算索引 |
| 画质 | MatPicmip(0)、MatForceaniso(1)、ShadowEnable(1)、ShadowMaxdynamic(4)、CsmEnabled(1)、CsmCascadeRes(512)、ParticleCpuLevel(1)、ClRagdollMaxcount(8)、RDecals(256)、MapDetailLevel(1)、VolumetricLighting(0)、VolumetricFog(0)、SsaoQuality(1) 等 |
| 性能 | MatVsyncMode(0)、MatBackbufferCount(1)、MatAntialiasMode(0)、DvsEnable(0)、DvsGpuframetimeMin/Max(15000/16500 微秒)、DynamicStreamingBudget(0) |
| 高级画质 | MFSpecularHighlight、MFDynamicDecals、MFDynamicLights、MFShadows、MFDecals、MFSSAO 等 0.0-1.0 质量乘数 |
| 系统 | ConfigVersion(10)、SoundVolume(1.0) |

### ApexSettingsConfig — settings.cfg（Valve CFG 格式）

| 分区 | 代表属性 |
|------|----------|
| 鼠标 | MouseSensitivity(1.0)、MAcceleration(0)、MClampToWindow(0) |
| 倍镜灵敏度 | MouseUsePerScopeSensitivityScalars(0) + MouseZoomedSensitivityScalar0-7（对应 1/2/3/4/6/8/10 倍镜与变焦镜） |
| 帧率 | FpsMax(0=不限)、FpsMaxMenu(60)、ClShowfps(0)、ClShowpos(0) |
| 视野 | ClFovScale(1.27，约 90-120 度)、ClViewpitchscale(0.95) |
| 网络 | ClInterp(0.015)、ClInterpRatio(1)、ClCmdrate(60)、ClUpdaterate(60) |
| 音频 | SoundNumSpeakers(2)、SoundVolumeVoice(1.0)、MilesChannels(0)、MilesOutputDevice |
| 语音 | VoiceChatMode、VoiceForcemicrecord(1)、VoiceMixerVolume(1.0)、VoiceModenable(1)、VoiceVox(1) 等 |
| 图形 | GfxAmdUseLowLatency(1)、GfxNvnUseLowLatency(0)、GfxNvnUseLowLatencyBoost(0)、ChromaEnable(0) |
| 其他 | Name、UiLayoutMode(0)、SvVoiceenable(1)、观战参数（SvSpecaccelerate/SvSpecnoclip/SvSpecspeed）、字幕与 HDR 截图目录等 |

### ApexLaunchOptions — 启动项

| 分区 | 参数（默认值） |
|------|----------------|
| 性能 | High、Threads(0)、Freq(0)、Novid、Nojoy |
| 窗口 | Windowed、Noborder、Fullscreen、Width(1920)、Height(1080)——仅 Windowed 或 Noborder 时追加 -w/-h |
| 游戏控制 | FpsMax(0)、ClShowfps(0)、ClShowpos、ClFovScale(1.27)、MatLetterboxAspectMin(0)、MRawinput |
| 网络 | ClInterp(0.015)、ClInterpRatio(1)、ClCmdrate(60)、ClUpdaterate(60) |
| 调试 | Dev、Console |

`GenerateOptionsString()`：按启用状态拼接完整命令行（如 `-high -novid +fps_max 0 +cl_fovScale "1.27"`）。数值参数仅在大于 0 时输出；`+fps_max` 恒输出。

## 接口定义

Interfaces/ 目录 4 个文件共定义 6 个接口 + 2 个事件参数类。

### IConfigRepository / IAppStateRepository / IPresetRepository（同文件 IConfigRepository.cs）

| 接口 | 成员 |
|------|------|
| IConfigRepository | `LoadConfigAsync()`、`SaveConfigAsync(config)`、`ResetToDefaultAsync()`、`ExportConfigAsync(path, config)`、`ImportConfigAsync(path)` |
| IAppStateRepository | `LoadStateAsync()`、`SaveStateAsync(state)` |
| IPresetRepository | `LoadPresetsAsync()`、`SavePresetAsync(preset)`、`DeletePresetAsync(presetId)`、`GetPresetAsync(presetId)`、`ExportPresetAsync(presetId, path)`、`ImportPresetAsync(path)` |

### ICrosshairRenderer（ICrosshairRenderer.cs）

- `Render(object drawingContext, CrosshairConfig config, double width, double height)`
- 事件 `RenderCompleted`，参数 `RenderCompletedEventArgs`（`RenderTimeMs`、`Success`）
- 实现：Services 层 `CrosshairRenderer`

### IGameDetector（IGameDetector.cs）

- 属性 `CurrentGame: GameInfo?`
- `StartMonitoring()` / `StopMonitoring()` / `InitializeAsync()`
- `RegisterGameProfile(profile)` / `GetRegisteredGames()`
- 事件 `GameStarted` / `GameExited`（GameDetectedEventArgs）
- 注意：仓库内暂无实现类（预留接口）

### IHotkeyManager（IHotkeyManager.cs）

- 继承 `IDisposable`
- `RegisterHotkey(HotkeyBinding)` / `UnregisterHotkey(bindingId)` / `UnregisterAll()`
- 事件 `HotkeyTriggered`，参数 `HotkeyTriggeredEventArgs` 携带触发的 `HotkeyBinding`
- 实现：Infrastructure 层 `HotkeyManager`

## 枚举定义

### AppState（Enums/AppState.cs）

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Idle | 空闲 |
| 1 | GameMode | 游戏模式（检测到游戏运行） |
| 2 | CrosshairVisible | 准心显示中 |
| 3 | CrosshairHidden | 准心隐藏 |

### CrosshairStyle（Enums/CrosshairStyle.cs）

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Cross | 十字准心 |
| 1 | Dot | 点状准心 |
| 2 | Circle | 圆形准心 |
| 3 | TShape | T 形准心 |
| 4 | XShape | X 形准心 |
| 5 | CustomImage | 自定义图片 |

HotkeyAction 见上文 HotkeyBinding 章节；ConfigItemType 见上文 GameConfigStrategy 章节。

## 数值范围

Core 模型层不做范围校验，实际范围由 App 层 UI 控件约束（`src/CrosshairPro.App/Views/CrosshairPage.xaml` 滑条）：
Size 1-100、Gap 0-50、Thickness 1-10、Opacity 0-100、Brightness 0-200、CenterSize 0-30。
UI 细节见 [../App/README.md](../App/README.md)。
