# Changelog - Application

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-08-10] feat: add localization support and improve UI components

**类型**: feat
**提交**: 55064e1
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Services/ApexConfigService.cs | +3/-0 | `LoadVideoConfigAsync()` 解析完 videoconfig.txt 分辨率后调用新增的 `config.UpdateResolutionIndex()`，把解析出的分辨率同步到 `ResolutionIndex` 供 UI 下拉框绑定 |

### 影响范围
- **API**: 无 Application 接口签名变更；`ApexConfigService` 新增对 Core `ApexVideoConfig.UpdateResolutionIndex()` 的调用
- **跨模块**: 是。同提交在 App 层新增本地化基础设施（Localization/LocExtension、LocalizationProvider、Strings.resx/Strings.en.resx 约 1500 行）并重写 ApexPage.xaml（929 行变更）、适配 ApexConfigViewModel/CrosshairViewModel；Core 层 `ApexVideoConfig` +52（新增 13 档 `ResolutionOptions` 静态表、`ResolutionIndex` 可观察属性、`OnResolutionIndexChanged` 联动与 `UpdateResolutionIndex()`）、`AppPersistedState` +3（新增 `Language` 字段）
- **数据模型**: `AppPersistedState` 新增持久化字段 `Language`（"zh-CN"/"en-US"，null=跟随系统）
- **配置**: appstate.json 新增 `Language` 键；旧文件无该键时反序列化为 null，向后兼容

### 回滚指南
- 回滚: `git revert 55064e1`
- 检查文件: src/CrosshairPro.Application/Services/ApexConfigService.cs、src/CrosshairPro.Core/Models/ApexVideoConfig.cs、src/CrosshairPro.Core/Models/AppPersistedState.cs、App 层 Localization/ 目录
- 副作用: 中英文本地化能力整体移除，界面回退为硬编码文案；已保存 appstate.json 中的 `Language` 键将被忽略（不报错）；Apex 分辨率下拉框随之失效

---

## [2026-07-31] feat: implement Apex Legends config service and CS2 docs

**类型**: feat
**提交**: c85e699
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Services/ApexConfigService.cs | +900/-0 | 新增 Apex 专用配置服务：解析/回写 `Saved Games/Respawn/Apex/local/` 下 videoconfig.txt 与 settings.cfg 键值、启动项（launch_options.txt）生成与读取、按时间戳备份/恢复/删除、从外部文件导入替换、竞技/高画质/默认三套画质预设、Apex 安装路径检测 |
| Interfaces/IApexConfigService.cs | +233/-0 | 新增接口 25 个成员：Load/Save/Replace Video/SettingsConfigAsync、Backup/Restore、Export 系列、GenerateLaunchOptions、Load/SaveLaunchOptionsAsync、GetVideo/SettingsConfigPath、IsApexInstalled、GetCompetitive/HighQuality/DefaultPreset、GetVideo/SettingsConfigBackups、DeleteBackup |
| DI/ServiceCollectionExtensions.cs | +3/-0 | 注册 `services.AddSingleton<IApexConfigService, ApexConfigService>()`（Singleton 生命周期） |

### 影响范围
- **API**: 新增 `IApexConfigService`（继 IGameConfigService 之后第二张游戏配置接口）；现有 IConfigurationService/IPresetService 未改动
- **跨模块**: 是。同提交在 Core 新增 3 个数据模型 ApexVideoConfig(+263)/ApexSettingsConfig(+251)/ApexLaunchOptions(+119)；App 层 ApexConfigViewModel 大幅扩展（361 行变更）、ApexPage.xaml +495、新增 IntToBoolConverter；docs/ 下新增 GameConfig/CS2 系列文档
- **数据模型**: 新增 ApexVideoConfig、ApexSettingsConfig、ApexLaunchOptions（均为新增文件，不改动现有模型）
- **配置**: 不修改 config.json/presets 结构；本服务读写的是 Apex 游戏自身配置文件与启动项，备份存于独立 backup 目录

### 回滚指南
- 回滚: `git revert c85e699`（后续 55064e1 修改过 ApexConfigService.cs，需先回滚 55064e1 或手动解决冲突）
- 检查文件: src/CrosshairPro.Application/Services/ApexConfigService.cs、src/CrosshairPro.Application/Interfaces/IApexConfigService.cs、src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs、App 层 ApexConfigViewModel.cs/ApexPage.xaml
- 副作用: Apex 配置页失去服务支撑，App 层绑定将编译失败，需同步移除 App 层 Apex 相关 UI 与 ViewModel

---

## [2026-07-31] feat: implement tab navigation UI with pages and viewmodels

**类型**: feat
**提交**: ced3254
**风险**: HIGH

> 注：本条的 Application 层变更即下方未标注提交哈希的「add IGameConfigService for game configuration management」条目所述内容（文件名与行数一致），此处补充提交哈希与 DI 注册变更，避免该提交在按哈希去重时被遗漏。

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Services/GameConfigService.cs | +497/-0 | 新增通用游戏配置服务：内置 8 款游戏策略（CS2/Valorant/Apex/Overwatch2/PUBG/Fortnite/R6/CSGO），配置缓存避免重复读文件，JSON 持久化到 %APPDATA%/CrosshairPro/gameconfigs/ |
| Interfaces/IGameConfigService.cs | +40/-0 | 新增接口：GetStrategies/GetStrategy/GetConfigAsync/SaveConfigAsync/ResetToDefaultAsync/ApplyConfigAsync（ApplyConfigAsync 为占位实现，直接返回 Task.CompletedTask） |
| DI/ServiceCollectionExtensions.cs | +1/-0 | 注册 `services.AddSingleton<IGameConfigService, GameConfigService>()`（Singleton 生命周期） |

### 影响范围
- **API**: 新增 `IGameConfigService`；现有 IConfigurationService/IPresetService 未改动
- **跨模块**: 是，且为本提交主体。同提交共 74 个文件（+23897/-1032）：App 层完成标签页导航重构（MainWindow 重写、新增 CrosshairPage/GamesPage、CrosshairViewModel/GamesViewModel、DialogBase/IconButton/ToastManager/ThemeHelper 等）；Core 新增 GameConfig.cs/GameConfigStrategy.cs；docs/ 新增各模块文档
- **数据模型**: Core 新增 GameConfig、GameConfigStrategy
- **配置**: 新增 %APPDATA%/CrosshairPro/gameconfigs/ 目录与每游戏独立 JSON

### 回滚指南
- 回滚: `git revert ced3254`（后续 c85e699/3f40dcf/55064e1 均在其基础上开发，需按从新到旧顺序先回滚这些提交，否则必然冲突）
- 检查文件: src/CrosshairPro.Application/ 全目录、src/CrosshairPro.Core/Models/GameConfig.cs、src/CrosshairPro.Core/Models/GameConfigStrategy.cs、App 层 MainWindow/ViewModels/Views
- 副作用: 标签页导航 UI 与游戏配置体系整体消失，回退到旧版单页 MainWindow；gameconfigs/ 下已保存的用户配置文件仍留在磁盘但不再被读取

---

## [2026-07-31] feat: add IGameConfigService for game configuration management

**类型**: feat
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| IGameConfigService.cs | +40 | 新增游戏配置服务接口 |
| GameConfigService.cs | +497 | 游戏配置服务实现 |

### 新增功能
1. **IGameConfigService 接口**
   - `GetStrategies()`: 获取所有游戏配置策略
   - `GetStrategy(gameId)`: 获取指定游戏的配置策略
   - `GetConfigAsync(gameId)`: 获取游戏配置
   - `SaveConfigAsync(config)`: 保存游戏配置
   - `ResetToDefaultAsync(gameId)`: 重置为默认配置
   - `ApplyConfigAsync(gameId)`: 应用配置到游戏

2. **GameConfigService 实现**
   - 内置 8 款游戏配置策略（CS2、Valorant、Apex、Overwatch2、PUBG、Fortnite、R6、CSGO）
   - 配置缓存机制，避免重复文件读取
   - JSON 文件持久化到 `%APPDATA%/CrosshairPro/gameconfigs/`
   - 每款游戏有独立的配置项定义（视频设置、游戏设置等）

### 支持的游戏
| 游戏 | GameId | 支持启动项 |
|------|--------|------------|
| CS2 | `builtin-cs2` | 是 |
| Valorant | `builtin-valorant` | 否 |
| Apex Legends | `builtin-apex` | 是 |
| Overwatch 2 | `builtin-overwatch2` | 否 |
| PUBG | `builtin-pubg` | 是 |
| Fortnite | `builtin-fortnite` | 否 |
| Rainbow Six | `builtin-r6` | 是 |
| CS:GO | `builtin-csgo` | 是 |

### 影响范围
- 新增服务，不影响现有功能
- `ApplyConfigAsync()` 待实现，需要根据每款游戏的配置文件格式写入

### 待办事项
- [ ] 实现 `ApplyConfigAsync()` 写入游戏配置文件
- [ ] 添加配置验证逻辑
- [ ] 支持自定义游戏配置策略

---

## [2026-07-31] docs: update Application module documentation

**类型**: docs
**风险**: LOW

### 变更内容
- 更新 README.md，补充完整的服务列表和使用示例
- 扩展 data-model.md，添加数据流图和 JSON 配置说明
- 完善 pitfalls.md，增加 11 个坑点详解
- 更新 CHANGELOG.md 格式

### 影响范围
- 文档更新，不影响代码

---

## [2026-07-03] refactor: restructure project architecture

**类型**: feat
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| CrosshairPro.Application.csproj | +23 | 新增项目定义 |
| ServiceCollectionExtensions.cs | +38 | 依赖注入注册扩展 |
| IConfigurationService.cs | +44 | 配置服务接口 |
| IPresetService.cs | +44 | 预设服务接口 |
| ConfigurationService.cs | +162 | 配置服务实现 |
| PresetService.cs | +113 | 预设服务实现 |

### 新增功能
1. **配置管理服务**
   - `GetCurrentConfig()`: 获取当前配置单例
   - `LoadConfigAsync()`: 从持久化加载配置
   - `SaveConfigAsync()`: 保存配置到持久化
   - `ResetToDefaultAsync()`: 重置为默认配置
   - `CloneConfig()`: 创建配置深拷贝
   - `CopyConfig()`: 复制配置值到目标实例

2. **预设管理服务**
   - `LoadAllPresetsAsync()`: 加载所有预设（含默认预设）
   - `SavePresetAsync()`: 保存预设
   - `DeletePresetAsync()`: 删除预设
   - `ImportPresetAsync()`: 从文件导入预设
   - `ExportPresetAsync()`: 导出预设到文件
   - `SetCurrentPresetAsync()`: 设置当前预设并持久化

3. **依赖注入统一注册**
   - `AddCrosshairProServices()` 扩展方法
   - 注册所有仓库和服务
   - 确保 `JsonConfigRepository` 双接口共享实例

### 架构变更
- **新增中间层**: Application 层作为业务逻辑抽象层
- **解耦**: App 层不再直接依赖 Services，通过 Application 接口调用
- **统一入口**: 所有服务通过 `AddCrosshairProServices()` 注册

### 影响范围
- **App 层**: ViewModel 构造函数依赖注入变更
- **Services 层**: 仓库实现被 Application 层接口调用
- **Core 层**: 新增 `IAppStateRepository` 接口

### 迁移指南

**旧代码** (直接依赖 Services):
```csharp
public MainViewModel(
    JsonConfigRepository configRepo,
    JsonPresetRepository presetRepo)
{
    var config = await configRepo.LoadConfigAsync();
}
```

**新代码** (通过 Application 接口):
```csharp
public MainViewModel(
    IConfigurationService configService,
    IPresetService presetService)
{
    var config = configService.GetCurrentConfig();
}
```

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: 整个 Application 目录，App 层 ViewModel 构造函数
- 副作用: 需要恢复 App 层对 Services 层的直接依赖

---

## 待办事项

- [ ] 添加配置变更监听和自动保存
- [ ] 支持配置版本迁移
- [ ] 添加预设验证逻辑
- [ ] 支持预设分类和标签