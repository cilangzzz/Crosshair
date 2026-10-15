# Changelog - Core

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-10-14] feat: ApexLaunchOptions 新增 LoadFromString 反向解析；生成规则调整

**类型**: feat
**提交**: 未提交（工作区）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Models/ApexLaunchOptions.cs | +~130 行 | 新增 `LoadFromString(string?)`（先 `ResetToDefaults()` 重置再解析，保证解析后选项与字符串严格一致）+ `Tokenize`（引号感知：引号内空白不拆分、引号剥除）+ `NextInt`/`NextFloat`（InvariantCulture）；识别 22 种令牌（大小写不敏感，`-refresh` 为 `-freq` 别名），未知令牌忽略 |

### 影响范围
- **生成规则**: 数值参数默认值全部改为 0（0 = 未设置、不输出）——`-w`/`-h` 不再被 `Windowed \|\| Noborder` 门控且不再有 1920/1080 默认值（解析无 -w/-h 的字符串后重新生成不注入默认分辨率）；`+cl_fovScale` 恢复输出（`"{值:F2}"` 带引号），取代同日早前「移除 cl_fovScale 归 settings.cfg」的调整；float 输出/解析一律 InvariantCulture（小数逗号区域安全）
- **消费方**: `ApexConfigViewModel` 切入自定义模式与 Reset 时调用 LoadFromString 回填面板（双向联动）；未知令牌与值为 0 的已知令牌经再生成会丢失，见 GameConfig/Apex/pitfalls.md §6
- **跨模块**: App（ApexConfigViewModel/ApexPage/resx），见 App/CHANGELOG.md 同日条目；GameConfig 文档（launch-options 生成规则表、pitfalls §6）

---

## [2026-10-14] feat: GameProfile 新增 DisplayNameLocKey 与 builtin-pcopt 占位档案

**类型**: feat
**提交**: 未提交（工作区）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Models/GameProfile.cs | +10 行 | 新增 `[ObservableProperty] DisplayNameLocKey`（string?，非空时 UI 优先取 Strings 资源）；`BuiltIn.GetAll()` 追加 builtin-pcopt 条目（DisplayName="PC Optimization"、LocKey="Games.PcOptimize"、ProcessName 空） |

### 影响范围
- **行为**: 游戏页 tab 出现第三个"电脑优化"条目（App 层 PcOptimizePage 占位页渲染）；cs2/apex 不设 LocKey 走 DisplayName 回退，行为不变
- **边界**: GameProfile 无序列化路径；`Matches()` 无调用方（IGameDetector 预留无实现）；GameConfigService 策略字典不含 builtin-pcopt，选中该 tab 不触发配置读写
- **跨模块**: App（GamesPage/GamePageSelector/PcOptimizePage/转换器/resx），见 App/CHANGELOG.md 同日条目；文档同步 Core/data-model.md（双树）

---

## [2026-10-14] feat: 新增 CrosshairProPaths 路径权威定义

**类型**: feat
**提交**: 未提交（工作区）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/CrosshairProPaths.cs | +19（新增） | `AppDirectory`（%APPDATA%\CrosshairPro）与 `AppStateFile`（appstate.json）静态字段；仓库实现与绕过异步接口直接读文件的调用方必须引用此类，避免存储契约多处硬编码后漂移 |

### 影响范围
- **动因**: App 层单实例校验需在语言初始化前同步读 appstate.json（见 App/CHANGELOG 同日条目），出现第二个独立读取方，路径/文件名定义收敛到 Core 单点
- **跨模块**: Services JsonConfigRepository 构造函数改用（见 Services/CHANGELOG 同日条目）；App 单实例校验读取路径改用（见 App/CHANGELOG 同日条目）

---

## [2026-10-14] fix: Apex 启动项生成不再输出默认 fps_max/cl_fovScale

**类型**: fix
**提交**: b546b85
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/ApexLaunchOptions.cs | ~2 处 | `GenerateOptionsString()`：`+fps_max` 由无条件输出改为 `FpsMax > 0` 条件输出；移除 `+cl_fovScale` 输出行 |

### 影响范围
- **动因**: Apex 页游戏设置类条目（fps_max/FOV/显示位置等）已收敛为 settings.cfg 单一来源（App 页面同日把游戏设置卡并入启动选项卡分区）；启动项默认值（`+fps_max 0`、`+cl_fovScale "1.27"`）此前会覆盖 settings.cfg 中的用户设置
- **兼容性**: 自定义模式 JSON 中已保存的 ClFovScale/FpsMax 旧值不再经结构化生成输出；直接编辑的文本模式与 UI 预设字符串（硬编码含 `+fps_max`/`+cl_fovScale`）不受影响
- **跨模块**: App 层 ApexPage 同日重构（见 App/CHANGELOG）；协议文档 GameConfig/Apex/pitfalls.md §6、launch-options.md 生成规则表已同步

---

## [2026-10-14] feat: 内置游戏档案仅保留 CS2 与 Apex

**类型**: feat
**提交**: b546b85（当时为工作区变更，后随该提交入库）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/GameProfile.cs | +0/-43 | `BuiltIn.GetAll()` 仅保留 `builtin-cs2`（Counter-Strike 2）与 `builtin-apex`（Apex Legends），移除 CS:GO/Valorant/Overwatch 2/PUBG/Fortnite/Rainbow Six Siege 六款档案 |

### 影响范围
- **API**: `GameProfile.BuiltIn.GetAll()` 返回数量 8 → 2；`Matches()` 全等匹配与 `Priority` 语义不变
- **跨模块**: Application 层 `GameConfigService` 同步移除 6 款游戏策略（见本模块同日 Application CHANGELOG）；App 层 `GamesViewModel` 游戏选择器随数据源自动缩减为两款，无需改动（`GamePageSelector` 本就只映射 builtin-apex/builtin-cs2）
- **兼容性**: 旧版写入的 `%APPDATA%/CrosshairPro/gameconfigs/builtin-{csgo,valorant,overwatch2,pubg,fortnite,r6}.json` 残留文件不再被读取（无对应策略），也不会被删除

---

## [2026-08-10] feat: add localization support and improve UI components

**类型**: feat
**提交**: 55064e1
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/AppPersistedState.cs | +3 | 新增 `Language` 属性（string?，null 表示跟随系统），用于持久化用户语言选择 |
| src/CrosshairPro.Core/Models/ApexVideoConfig.cs | +52 | 新增 `ResolutionIndex` 属性与 `ResolutionOptions` 静态分辨率选项表；`OnResolutionIndexChanged` 联动写入 `DefaultRes`/`DefaultResHeight`；新增 `UpdateResolutionIndex()` 反查方法 |

### 影响范围
- **API**: `ApexVideoConfig` 新增 `ResolutionIndex` 属性与 `UpdateResolutionIndex()` 方法；`AppPersistedState` 新增 `Language` 属性
- **跨模块**: 同一提交为 App 层新增本地化基础设施（Localization 目录、Strings*.resx、LocExtension）与 SmoothScrollViewer/ScrollViewerBehavior 控件，并重构 MainWindow/ApexPage；Application 层 ApexConfigService 同步微调（+3）。App 启动时读取 `state.Language` 初始化本地化，语言切换时写回该字段
- **数据模型**: `appstate.json` 序列化结构新增 `Language` 字段（向后兼容：旧文件缺省为 null，跟随系统语言）
- **配置**: appstate.json 结构扩展，无破坏性变更

---

## [2026-07-31] feat: implement Apex Legends config service and CS2 docs

**类型**: feat
**提交**: c85e699
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/ApexLaunchOptions.cs | +119 | 新增 Apex 启动项模型：22 个可观察属性与启动项字符串生成方法 |
| src/CrosshairPro.Core/Models/ApexSettingsConfig.cs | +251 | 新增 Apex 游戏内设置模型：鼠标灵敏度/加速、1-8 倍镜独立灵敏度等 |
| src/CrosshairPro.Core/Models/ApexVideoConfig.cs | +263 | 新增 Apex 视频设置模型：分辨率、刷新率、显示模式、伽马与画质选项 |

### 影响范围
- **API**: 三个新模型类（均继承 ObservableObject），无既有 API 变更
- **跨模块**: 同一提交新增 Application 层 IApexConfigService（+233）/ApexConfigService（+900）并注册 DI；App 层扩展 ApexConfigViewModel 与 ApexPage；docs 下新增 CS2 文档
- **数据模型**: 新增 Apex 配置数据模型族，作为 Apex 配置读写与 UI 绑定的数据基础
- **配置**: Apex 配置由 Application 层服务读写，本提交未改动 Core 既有模型

---

## [2026-07-31] feat: add game-specific config system with Apex Legends support

**类型**: feat
**提交**: 3f40dcf
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/GameProfile.cs | +4/-1 | 新增 `IsSelected` 布尔属性，用于游戏列表选中状态；文件末尾换行符被移除 |

### 影响范围
- **API**: `GameProfile` 新增 `IsSelected` 属性
- **跨模块**: 同一提交新增 App 层 ApexConfigViewModel、ConfigItemWrapper、ApexPage/GamePageSelector 与 ConfigTypeToVisibilityConverter，重构 GamesViewModel/MainViewModel；同步更新 App/Application/Core 模块文档
- **数据模型**: `IsSelected` 为运行时 UI 状态标记；GameProfile 不经 Services 层持久化（当前仅 Core 定义与 App 的 GamesViewModel 使用）
- **配置**: 无新增配置文件

---

## [2026-07-31] feat: implement tab navigation UI with pages and viewmodels

**类型**: feat
**提交**: ced3254
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/GameConfig.cs | +20 | 新增游戏配置数据模型 |
| src/CrosshairPro.Core/Models/GameConfigStrategy.cs | +91 | 新增游戏配置策略定义 |
| ConfigItemType（内嵌枚举） | 含于 GameConfigStrategy.cs | 配置项类型枚举（Bool/Int/Enum/String） |

### 新增模型

**GameConfig**：
- 存储单个游戏的配置数据
- 包含 GameId、LaunchOptions、Settings 字典

**GameConfigStrategy**：
- 定义每个游戏支持的配置项和操作方式
- 包含 ConfigItemType 枚举、ConfigItemDefinition、ConfigSectionDefinition

### 影响范围
- **数据模型**: 新增游戏配置相关模型，用于游戏特定配置管理
- **跨模块**: 同一提交重构了 App 层标签页导航（页面、ViewModel、控件与主题）并新增 Application 层 IGameConfigService/GameConfigService；Application 层可使用 GameConfigStrategy 构建 UI，使用 GameConfig 存储配置

---

## [2026-07-03] refactor: restructure project architecture

**类型**: refactor
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| IConfigRepository.cs | +16 | 新增 IAppStateRepository 接口定义 |
| AppPersistedState.cs | +13 | 新增应用持久化状态模型 |

### 影响范围
- **接口**: 新增 `IAppStateRepository` 用于管理应用状态
- **数据模型**: 新增 `AppPersistedState` 记录当前预设ID
- **跨模块**: Services 层 JsonConfigRepository 需实现新接口

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: IConfigRepository.cs, AppPersistedState.cs
- 副作用: Application 层服务依赖新接口，回滚需同步修改

---

## [2026-06-08] feat: major UI and ViewModel refactoring

**类型**: feat
**提交**: 8d57814
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Preset.cs | +2 | 新增 IsDefault 属性标记默认预设 |

### 影响范围
- **数据模型**: Preset 新增 IsDefault 字段，影响预设列表显示和删除逻辑
- **跨模块**: App 模块的 MainViewModel 需要处理 IsDefault 预设不可删除

---

## [2026-06-08] feat: enhance overlay and crosshair config

**类型**: feat
**提交**: 64f03ed
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| CrosshairConfig.cs | +1 | 补全 CopyFrom 方法中 Brightness 的拷贝 |

### 影响范围
- **数据模型**: CrosshairConfig 拷贝逻辑修复，确保配置复制时 Brightness 不丢失

---

## [2026-06-08] feat: add new controls and model properties

**类型**: feat
**提交**: bace47c
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/CrosshairConfig.cs | +4 | 新增 `Brightness` 属性（int，默认 100），并加入 Clone() 拷贝 |

### 影响范围
- **API**: `CrosshairConfig` 新增 `Brightness` 属性
- **跨模块**: 同一提交修改 App 层 MainWindow/ControlStyles/MainViewModel；Brightness 随后接入准心颜色亮度调节（App 层 OverlayWindow.ApplyBrightness、CrosshairPage 滑块）与 Application 层 CloneConfig 深拷贝
- **数据模型**: config.json 结构新增 brightness 字段（向后兼容）
- **配置**: 无新增配置文件

> 注：本条与上方 64f03ed 条目为同一功能的连续提交——本条引入 Brightness 属性，64f03ed 补全 CopyFrom 中的拷贝。

---

## [2026-06-07] Initial commit: CrosshairPro WPF application

**类型**: feat
**提交**: e649a4f
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/CrosshairPro.Core.csproj | +16 | Core 类库项目文件（net8.0，依赖 CommunityToolkit.Mvvm 8.3.2、System.Text.Json 8.0.5） |
| src/CrosshairPro.Core/Enums/AppState.cs | +27 | 应用状态枚举（Idle/GameMode/CrosshairVisible/CrosshairHidden） |
| src/CrosshairPro.Core/Enums/CrosshairStyle.cs | +37 | 准心样式枚举（Cross/Dot/Circle/TShape/XShape/CustomImage） |
| src/CrosshairPro.Core/Interfaces/IConfigRepository.cs | +70 | 配置与预设仓库接口（IConfigRepository、IPresetRepository） |
| src/CrosshairPro.Core/Interfaces/ICrosshairRenderer.cs | +28 | 准心渲染接口 |
| src/CrosshairPro.Core/Interfaces/IGameDetector.cs | +49 | 游戏检测接口（基于 GameProfile 进程匹配） |
| src/CrosshairPro.Core/Interfaces/IHotkeyManager.cs | +42 | 热键管理接口 |
| src/CrosshairPro.Core/Models/CrosshairConfig.cs | +161 | 准心配置模型 |
| src/CrosshairPro.Core/Models/EffectsConfig.cs | +72 | 效果配置模型（启用开关、颜色等效果参数） |
| src/CrosshairPro.Core/Models/GameInfo.cs | +25 | 游戏信息模型 |
| src/CrosshairPro.Core/Models/GameProfile.cs | +111 | 游戏配置档案模型（进程匹配 Matches、内置档案 BuiltIn） |
| src/CrosshairPro.Core/Models/HotkeyBinding.cs | +144 | 热键绑定模型 |
| src/CrosshairPro.Core/Models/Preset.cs | +51 | 预设模型 |

### 影响范围
- **数据模型**: 模块初始结构（13 个文件、833 行）在本提交一次性创建
- **跨模块**: 同一提交创建整个解决方案（35 个文件、6855 行：App/Core/Infrastructure/Services 四个项目及早期文档）；Application 层当时尚不存在，至 6fad297 重构时才引入