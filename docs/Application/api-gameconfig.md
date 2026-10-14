# 游戏配置 API — IGameConfigService 与 IApexConfigService

游戏配置域包含两个独立接口：

- **`IGameConfigService`**: 通用游戏配置 — 内置 8 款游戏的策略元数据定义 + 配置存取（JSON）
- **`IApexConfigService`**: Apex 专用深度配置 — 直接读写 Apex 存档目录的 `videoconfig.txt` / `settings.cfg`，含备份恢复与启动选项管理

---

## IGameConfigService

### 基本信息

| 项 | 值 |
|----|----|
| 接口定义 | `src/CrosshairPro.Application/Interfaces/IGameConfigService.cs` |
| 实现类 | `src/CrosshairPro.Application/Services/GameConfigService.cs` |
| DI 注册 | `services.AddSingleton<IGameConfigService, GameConfigService>()` |
| 依赖 | 无接口依赖；配置目录 `%APPDATA%/CrosshairPro/gameconfigs/`（构造时创建） |
| 事件 | 无 |

### 方法列表

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetStrategies()` | `IReadOnlyList<GameConfigStrategy>` | 所有内置游戏策略 |
| `GetStrategy(gameId)` | `GameConfigStrategy?` | 指定游戏策略，未知 ID 返回 null |
| `GetConfigAsync(gameId)` | `Task<GameConfig?>` | 读取配置（内存缓存优先；文件缺失/损坏回退默认值） |
| `SaveConfigAsync(config)` | `Task` | 更新缓存并写入 `{gameId}.json` |
| `ResetToDefaultAsync(gameId)` | `Task` | 按策略默认值生成并保存 |
| `ApplyConfigAsync(gameId)` | `Task` | **空实现（TODO）**，见 [pitfalls.md](pitfalls.md) 坑点 12 |

### 方法详情

#### GetStrategies()

- **签名**: `IReadOnlyList<GameConfigStrategy> GetStrategies()`
- **参数**: 无
- **返回值**: 内部策略字典值的只读快照（8 项）

#### GetStrategy(string gameId)

- **签名**: `GameConfigStrategy? GetStrategy(string gameId)`
- **参数**: `gameId` — 游戏ID（如 `"builtin-cs2"`）
- **返回值**: 对应策略；未知 ID 返回 null

#### GetConfigAsync(string gameId)

- **签名**: `Task<GameConfig?> GetConfigAsync(string gameId)`
- **参数**: `gameId` — 游戏ID
- **返回值**: 游戏配置（实际不会为 null，签名保留 `?`）
- **行为**: 缓存命中直接返回；未命中读文件反序列化。文件不存在 → 返回 `CreateDefaultConfig(gameId)` 并写入缓存；解析异常或反序列化结果为 null → 返回 `CreateDefaultConfig(gameId)` 但**不写入缓存**，下次调用重新读文件。未知 gameId 的默认配置 `Settings` 为空字典。

#### SaveConfigAsync(GameConfig config)

- **签名**: `Task SaveConfigAsync(GameConfig config)`
- **参数**: `config` — 待保存配置（`config.GameId` 决定目标文件）
- **返回值**: `Task`
- **行为**: 更新 `_configCache[config.GameId]` 后以 `WriteIndented` 序列化写入 `{gameId}.json`。

#### ResetToDefaultAsync(string gameId)

- **签名**: `Task ResetToDefaultAsync(string gameId)`
- **参数**: `gameId` — 游戏ID
- **返回值**: `Task`
- **行为**: 按策略逐项填充 `DefaultValue` 生成默认配置并保存。

#### ApplyConfigAsync(string gameId)

- **签名**: `Task ApplyConfigAsync(string gameId)`
- **参数**: `gameId` — 游戏ID
- **返回值**: `Task.CompletedTask`
- **行为**: **未实现** — 只返回完成任务，不写入任何游戏配置文件（见坑点 12）。

### 内置策略

| 游戏 | GameId | 启动项 | 配置分区 |
|------|--------|--------|----------|
| CS2 | `builtin-cs2` | 是 | video（fullscreen / resolution / aspect_ratio / refresh_rate）+ game（fps_max / cl_showfps） |
| Valorant | `builtin-valorant` | 否 | video（fullscreen / resolution / fps_limit） |
| Apex Legends | `builtin-apex` | 是 | video（fullscreen / fps_max）+ game（cl_showpos） |
| Overwatch 2 | `builtin-overwatch2` | 否 | video（fullscreen / resolution / fps_limit） |
| PUBG | `builtin-pubg` | 是 | video（fullscreen / resolution） |
| Fortnite | `builtin-fortnite` | 否 | video（fullscreen / fps_limit） |
| Rainbow Six | `builtin-r6` | 是 | video（fullscreen / resolution） |
| CS:GO | `builtin-csgo` | 是 | 复制 CS2 策略后改 GameId |

策略元数据模型（`GameConfigStrategy` / `ConfigSectionDefinition` / `ConfigItemDefinition` / `ConfigItemType`）定义在 Core，详见 [../Core/README.md](../Core/README.md) 与 [../Core/data-model.md](../Core/data-model.md)。

### 主要调用方

| 调用方 | 使用的成员 |
|--------|-----------|
| `Cs2ConfigViewModel`（`src/CrosshairPro.App/ViewModels/Cs2ConfigViewModel.cs`） | `GetStrategy()`、`GetConfigAsync()`、`SaveConfigAsync()`、`ResetToDefaultAsync()` |

---

## IApexConfigService

### 基本信息

| 项 | 值 |
|----|----|
| 接口定义 | `src/CrosshairPro.Application/Interfaces/IApexConfigService.cs`（`BackupFileInfo` 模型也定义于此文件） |
| 实现类 | `src/CrosshairPro.Application/Services/ApexConfigService.cs` |
| DI 注册 | `services.AddSingleton<IApexConfigService, ApexConfigService>()` |
| 依赖 | 无接口依赖；内部 `SemaphoreSlim _fileLock` 串行化文件操作（覆盖范围见坑点 15） |
| 事件 | 无 |

### 目录与文件

| 路径 | 用途 |
|------|------|
| `%USERPROFILE%/Saved Games/Respawn/Apex/local/` | Apex 存档目录（`videoconfig.txt`、`settings.cfg`）；`IsApexInstalled()` 即检测此目录 |
| `%APPDATA%/CrosshairPro/apex-backups/videoconfig/` | videoconfig 备份（`yyyy-MM-dd_HH-mm-ss_videoconfig.txt`） |
| `%APPDATA%/CrosshairPro/apex-backups/settings/` | settings 备份（`yyyy-MM-dd_HH-mm-ss_settings.cfg`） |
| `%APPDATA%/CrosshairPro/launch_options.txt` | Apex 启动选项持久化 |

### 方法列表（按分组）

#### 加载 / 保存

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `LoadVideoConfigAsync()` | `Task<ApexVideoConfig>` | 文件缺失或解析异常返回 `GetDefaultPreset()`；解析 KeyValues `"setting.xxx" "value"` 行 |
| `LoadSettingsConfigAsync()` | `Task<ApexSettingsConfig>` | 文件缺失或解析异常返回 `new ApexSettingsConfig()`；解析 `name "value"` 行，跳过注释与空行 |
| `SaveVideoConfigAsync(config)` | `Task` | 存档目录不存在抛 `InvalidOperationException`；序列化为 KeyValues 文本 |
| `SaveSettingsConfigAsync(config)` | `Task` | 同上，序列化为 CFG 文本 |

#### 替换 / 备份 / 恢复 / 历史版本

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `ReplaceVideoConfigAsync(filePath)` | `Task<bool>` | 验证内容含 `"VideoConfig"` → 备份当前 → `File.Copy` 覆盖；失败返回 false |
| `ReplaceSettingsConfigAsync(filePath)` | `Task<bool>` | 验证内容含 `mouse_sensitivity` 或 `fps_max`，其余同上 |
| `BackupVideoConfigAsync()` | `Task<string>` | 复制到备份目录并返回备份路径；源文件不存在抛 `FileNotFoundException` |
| `BackupSettingsConfigAsync()` | `Task<string>` | 同上 |
| `RestoreVideoConfigFromBackupAsync(backupPath)` | `Task<bool>` | 先备份当前再覆盖；备份缺失或失败返回 false |
| `RestoreSettingsConfigFromBackupAsync(backupPath)` | `Task<bool>` | 同上 |
| `GetVideoConfigBackups()` | `List<BackupFileInfo>` | 枚举备份目录，按备份时间倒序 |
| `GetSettingsConfigBackups()` | `List<BackupFileInfo>` | 同上 |
| `DeleteBackup(backupPath)` | `bool` | 文件存在则删除并返回 true |

#### 导出

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `ExportVideoConfigAsync(filePath)` | `Task` | 读取当前 videoconfig → 序列化 → 写目标路径 |
| `ExportSettingsConfigAsync(filePath)` | `Task` | 读取当前 settings → 序列化 → 写目标路径 |
| `ExportLaunchOptionsAsync(filePath, options)` | `Task` | 原样写文本到目标路径 |

#### 启动选项

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `GenerateLaunchOptions()` | `string` | 返回硬编码模板 `-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -novid`（不读取当前配置，见坑点 14） |
| `LoadLaunchOptionsAsync()` | `Task<string>` | 读 `launch_options.txt`；缺失或失败返回模板 |
| `SaveLaunchOptionsAsync(options)` | `Task` | 写 `launch_options.txt` |

#### 路径检测

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `GetVideoConfigPath()` | `string?` | 存档目录存在才返回完整路径，否则 null |
| `GetSettingsConfigPath()` | `string?` | 同上 |
| `IsApexInstalled()` | `bool` | `Directory.Exists(存档目录)` |

#### 预设

| 方法 | 返回值 | 行为 |
|------|--------|------|
| `GetCompetitivePreset()` | `ApexVideoConfig` | 竞技优化：阴影 / CSM / SSAO / 体积光雾关闭，各向异性 1，粒子等级 0 |
| `GetHighQualityPreset()` | `ApexVideoConfig` | 高画质：阴影 / CSM 开启，各向异性 16，抗锯齿 4x，粒子等级 2 |
| `GetDefaultPreset()` | `ApexVideoConfig` | `new ApexVideoConfig()`（模型默认值） |

### 解析 / 序列化要点

- **videoconfig.txt**（KeyValues 格式）: 逐行正则 `"setting\.(\w+)"\s+"(.+)"` 提取。序列化写出 23 个 setting 键（解析 switch 覆盖其中 19 个），`mat_mip_linear`、`csm_cascade_res`、`dvs_enable`、`cl_ragdoll_self_collision` 四个键只写不读 — 加载→保存往返会重置为模型默认值（见坑点 13）。解析成功后调用 `config.UpdateResolutionIndex()` 同步分辨率索引。
- **settings.cfg**（Valve CFG 格式）: 逐行正则 `^(\w+)\s+"(.+)"`。解析与序列化键覆盖对称，共 17 项：`mouse_sensitivity`、`m_acceleration`、`mouse_use_per_scope_sensitivity_scalars`、`mouse_zoomed_sensitivity_scalar_0..7`、`fps_max`、`cl_showfps`、`cl_showpos`、`cl_fovScale`、`cl_interp`、`cl_interp_ratio`（17 项清单与 ../GameConfig/Apex/data-model.md 一致）。

### BackupFileInfo（定义于接口文件内）

| 成员 | 类型 | 说明 |
|------|------|------|
| `FilePath` / `FileName` | `string` | 完整路径 / 文件名 |
| `BackupTime` | `DateTime` | 从文件名时间戳 `yyyy-MM-dd_HH-mm-ss` 解析；解析失败回退 `File.GetCreationTime`，但回退路径缺少子目录、实际恒得 `1601-01-01` 常量（代码缺陷，见坑点 17） |
| `FileSize` | `long` | 字节数 |
| `DisplayTime` | `string` | `yyyy-MM-dd HH:mm:ss` 格式化 |
| `DisplaySize` | `string` | 自动 B / KB / MB / GB 格式化 |

`ApexVideoConfig` / `ApexSettingsConfig` 模型定义在 Core（`src/CrosshairPro.Core/Models/ApexVideoConfig.cs`、`ApexSettingsConfig.cs`），字段语义见 [../Core/README.md](../Core/README.md)。

### 主要调用方

| 调用方 | 使用的成员 |
|--------|-----------|
| `ApexConfigViewModel`（`src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs`） | 几乎全部成员：加载/保存两个配置、预设切换、启动选项读写、备份/恢复/删除、导出、路径检测与安装检测 |
