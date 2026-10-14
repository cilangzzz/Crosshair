# Apex Legends 配置变更日志

## [2026-10-14] fix: 启动项生成通道与 settings.cfg 解耦，UI 分区重构

**类型**: fix
**提交**: 部分（b546b85）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Core/Models/ApexLaunchOptions.cs | ~2 处 | `GenerateOptionsString()`：`+fps_max` 改条件输出（`FpsMax > 0`），移除 `+cl_fovScale`（随 b546b85 提交，详见 [Core/CHANGELOG](../../../Core/CHANGELOG.md) 同日条目） |
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +90/-93 | 自定义启动面板移除与游戏设置卡重复的「游戏设置」组（信箱比例移入「窗口」组）；独立「游戏设置」卡并入「启动选项」卡作下半分区；父子联动（级联阴影←启用阴影、体积雾←体积光照、倍镜滑杆←独立倍镜开关，ElementName 绑定 IsChecked→IsEnabled）；子项取消缩进与父项同列；父开关经 UI 点击关闭时子开关值同步置 0（Click 处理器，见 App/CHANGELOG 同日条目） |
| docs/GameConfig/Apex/pitfalls.md | ~§6 | 无条件输出坑点改写为已调整状态（fps_max 条件化、cl_fovScale 移除） |
| docs/GameConfig/Apex/launch-options.md | ~3 处 | 生成规则表 FpsMax/ClFovScale 行更新；补充 settings.cfg 为主通道说明 |

### 影响范围
- **数据流**: fps_max/cl_showfps/cl_showpos/cl_fov_scale 常规配置单一来源为 settings.cfg；**结构化生成通道**（自定义面板）的启动项默认值不再覆盖游戏设置——启动项预设字符串与文本模式行为未变，选用预设或手填后仍可能覆盖 settings.cfg
- **协议**: 生成规则变化只影响结构化生成（自定义模式）；预设字符串与文本模式不变

---

## [2026-09-01] feat: add Chinese localization support

**类型**: feat
**提交**: eb57c0b
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +97/-96 | 迁移页面剩余 48 处硬编码英文文案（窗口、垂直同步、纹理画质、备份历史等）为 loc:Loc 资源键，新增 96 处本地化绑定，中文界面补全 |
| src/CrosshairPro.App/Localization/Strings.zh-CN.resx | +637/-0 | 新增中文资源文件，覆盖 Apex 页全部文案键 |
| src/CrosshairPro.App/Localization/LocExtension.cs | +13/-17 | 本地化扩展适配 zh-CN 文化回退 |
| src/CrosshairPro.App/Localization/LocalizationProvider.cs | +20/-13 | 提供者同步调整 |
| src/CrosshairPro.App/Views/GamesPage.xaml | +2/-1 | 游戏列表页文案绑定微调 |
| src/CrosshairPro.App/CrosshairPro.App.csproj | +2/-1 | 植入 zh-CN 资源文件 |

### 影响范围
- **API**: 无
- **跨模块**: 是 — 同提交修改 App 模块本地化基础设施（Strings.zh-CN.resx、LocExtension、LocalizationProvider）及 MainWindow、CrosshairPage、GamesPage
- **数据模型**: 无
- **配置**: 无

---

## [2026-08-10] fix: improve localization and UI polish

**类型**: fix
**提交**: 73c777d
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +150/-149 | 修正 Apex 页本地化绑定与 Grid 布局（大量分区容器重排），统一 loc:Loc 绑定写法并做视觉打磨 |
| src/CrosshairPro.App/Localization/LocExtension.cs | +10/-16 | 本地化刷新机制修复 |
| src/CrosshairPro.App/Localization/LocalizationProvider.cs | +4/-7 | 提供者瘦身 |
| src/CrosshairPro.App/App.xaml.cs | +46/-31 | 应用级语言切换逻辑调整 |
| src/CrosshairPro.App/Views/GamesPage.xaml | +1/-2 | 列表页绑定修正 |

### 影响范围
- **API**: 无
- **跨模块**: 是 — 同提交调整 App 模块本地化基础设施与 MainWindow、CrosshairPage
- **数据模型**: 无
- **配置**: 无

---

## [2026-08-10] feat: add localization support and improve UI components

**类型**: feat
**提交**: 55064e1
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +365/-564 | 页面重构：引入 SmoothScrollViewer 平滑滚动、Grid 分区布局，硬编码文案整体替换为 loc:Loc 资源键 |
| src/CrosshairPro.Core/Models/ApexVideoConfig.cs | +52/-0 | 新增 ResolutionOptions 静态分辨率表与 ResolutionIndex 辅助属性，索引变更联动 DefaultRes/DefaultResHeight |
| src/CrosshairPro.Application/Services/ApexConfigService.cs | +3/-0 | LoadVideoConfigAsync 解析后调用 UpdateResolutionIndex 同步分辨率索引 |
| src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs | +63/-50 | 适配本地化键与分辨率索引绑定 |
| src/CrosshairPro.App/Controls/SmoothScrollViewer.cs | +173/-0 | 平滑滚动控件（Apex 页等长页面使用） |
| src/CrosshairPro.App/Localization/* | +1471/-0 | 本地化基础设施：LocExtension、LocalizationProvider、Strings.cs、Strings.resx、Strings.en.resx |

### 影响范围
- **API**: ApexVideoConfig 新增 ResolutionIndex 属性与 UpdateResolutionIndex() 方法；IApexConfigService 接口无变化
- **跨模块**: 是 — 同提交为整个 App 模块引入本地化体系（Localization 目录、SmoothScrollViewer/ScrollViewerBehavior 控件），并修改 MainWindow、CrosshairPage、AppPersistedState
- **数据模型**: ApexVideoConfig 增加辅助字段（仅 UI 绑定用，不写入 videoconfig.txt）
- **配置**: 无（videoconfig.txt 键集不变）

### 回滚指南（HIGH）
- 执行 `git revert 55064e1`。
- 回滚后检查：
  - `src/CrosshairPro.Core/Models/ApexVideoConfig.cs`：若需保留 ResolutionIndex，必须同时移除 `ApexConfigService.LoadVideoConfigAsync` 中的 `config.UpdateResolutionIndex()` 调用，否则编译失败；
  - `src/CrosshairPro.App/Views/GamePages/ApexPage.xaml`：后续 73c777d、eb57c0b 的本地化修改均基于本提交的页面结构，直接 revert 必然冲突，且会连带失去中文界面（需一并回退这两个提交或手工合并）；
  - `src/CrosshairPro.App/CrosshairPro.App.csproj`：确认 resx 资源声明一并移除。
- 可能副作用：整个 App 模块的本地化能力（含英文资源与语言切换）消失；SmoothScrollViewer 被移除后其他依赖它的页面滚动行为退化。

---

## [2026-07-31] refactor: improve Apex config UI binding

**类型**: refactor
**提交**: c69660e
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs | +209/-9 | 新增备份历史管理（视频/设置配置备份列表加载、恢复、删除）与自定义启动项（ApexLaunchOptions 编辑、LaunchOptionsSummary 摘要、UseCustomLaunchOptions 切换）等状态与命令 |
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +436/-48 | 新增备份历史面板、自定义启动项编辑区等 UI，绑定到上述新命令 |
| src/CrosshairPro.App/InverseBooleanToVisibilityConverter.cs | +31/-0 | 反向布尔→可见性转换器（折叠/展开面板用） |
| src/CrosshairPro.App/App.xaml | +3/-0 | 注册新转换器资源 |

### 影响范围
- **API**: ApexConfigViewModel 新增备份历史与自定义启动项命令（RestoreVideoConfigBackup / RestoreSettingsConfigBackup / DeleteBackup / ToggleVideoConfigHistory 等）；IApexConfigService 无变化
- **跨模块**: 无（仅 App 模块）
- **数据模型**: 无
- **配置**: 无（备份/恢复复用 c85e699 引入的 Get*ConfigBackups / Restore*FromBackupAsync / DeleteBackup）

---

## [2026-07-31] feat: implement Apex Legends config service and CS2 docs

**类型**: feat
**提交**: c85e699
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Application/Services/ApexConfigService.cs | +900/-0 | Apex 配置服务实现：videoconfig.txt / settings.cfg 加载与整体重写保存、文件替换（覆盖前自动备份）、备份/恢复/删除/导出、launch_options.txt 管理、GetVideoConfigBackups 等备份枚举 |
| src/CrosshairPro.Application/Interfaces/IApexConfigService.cs | +233/-0 | Apex 配置服务接口（Load/Save/Replace/Backup/Restore/Export/GetBackups 系列） |
| src/CrosshairPro.Core/Models/ApexVideoConfig.cs | +263/-0 | 视频配置模型（分辨率、亮度、DVS 等字段） |
| src/CrosshairPro.Core/Models/ApexSettingsConfig.cs | +251/-0 | 游戏设置模型 |
| src/CrosshairPro.Core/Models/ApexLaunchOptions.cs | +119/-0 | 启动项模型 |
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +466/-29 | 页面接入服务：视频/设置/启动项三大配置区块与操作按钮 |
| src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs | +316/-45 | 数据源从 IGameConfigService 切换到 IApexConfigService，实现加载/保存/备份命令 |
| src/CrosshairPro.App/IntToBoolConverter.cs | +30/-0 | int→bool 转换器（0/1 开关绑定） |
| src/CrosshairPro.App/App.xaml | +3/-0 | 注册转换器资源 |
| src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs | +3/-0 | 注册 IApexConfigService 单例 |
| docs/GameConfig/Apex/launch-options.md | +215/-8 | 启动项文档补充游戏控制/网络/调试/图形/内存/系统参数章节与 CurrentConfig 说明 |
| docs/GameConfig/CS2/* | +1754/-0 | 同提交为 CS2 新建文档（不属于本模块，详见 CS2 CHANGELOG） |

### 影响范围
- **API**: 新增 IApexConfigService 并以单例注册进 DI 容器；ApexPage 数据上下文随之切换
- **跨模块**: 是 — Core（3 个新模型）、Application（服务+接口+DI）、App（UI/ViewModel）同提交变更；CS2 文档同提交新增
- **数据模型**: 新增 ApexVideoConfig / ApexSettingsConfig / ApexLaunchOptions
- **配置**: 应用自此直接读写游戏侧 `%USERPROFILE%\Saved Games\Respawn\Apex\local\` 下的 videoconfig.txt 与 settings.cfg；保存为整体重写式，未建模键会丢失（详见 [pitfalls.md](pitfalls.md) 坑点 1/2/3）

### 回滚指南（HIGH）
- 执行 `git revert c85e699`。
- 回滚后检查：
  - `src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs`：确认服务注册随之移除，DI 容器无残留解析；
  - `src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs` 与 `src/CrosshairPro.App/Views/GamePages/ApexPage.xaml`：所有引用 IApexConfigService 的代码将编译失败，需一并回退到基于 IGameConfigService 的 3f40dcf 版本，或连同后续 c69660e / 55064e1 / 73c777d / eb57c0b 一起处置；
  - `src/CrosshairPro.App/App.xaml`：确认 IntToBoolConverter 资源移除。
- 可能副作用：docs/GameConfig/CS2/ 文档一并删除；docs/GameConfig/Apex/launch-options.md 回退到无参数章节版本；已产生的游戏侧配置备份目录残留（需手工清理）。

---

## [2026-07-31] feat: add game-specific config system with Apex Legends support

**类型**: feat
**提交**: 3f40dcf
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml | +117/-0 | 新增 Apex 专属页面初版（GamePageSelector 按 gameId="builtin-apex" 路由） |
| src/CrosshairPro.App/Views/GamePages/ApexPage.xaml.cs | +30/-0 | 页面代码后置 |
| src/CrosshairPro.App/Views/GamePages/GamePageSelector.cs | +26/-0 | DataTemplateSelector：builtin-apex → ApexTemplate，其余走默认模板 |
| src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs | +84/-0 | Apex 配置 ViewModel 初版（基于 IGameConfigService 的简单字段绑定：全屏、FPS 上限、刷新率等） |
| src/CrosshairPro.App/ViewModels/ConfigItemWrapper.cs | +64/-0 | 通用配置项包装器（键值绑定） |
| src/CrosshairPro.App/ConfigTypeToVisibilityConverter.cs | +31/-0 | 配置类型→可见性转换器 |
| src/CrosshairPro.App/Views/GamesPage.xaml | +24/-154 | 移除内联 Apex 控件，改为 ContentControl + 模板选择 |
| src/CrosshairPro.App/ViewModels/GamesViewModel.cs | +34/-85 | 配置逻辑下沉到 ApexConfigViewModel |
| src/CrosshairPro.Core/Models/GameProfile.cs | +4/-1 | GameProfile 新增 IsSelected 选中状态属性 |
| src/CrosshairPro.App/App.xaml、App.xaml.cs、ViewModels/MainViewModel.cs | +3/-0、+7/-2、+1/-4 | 模板资源与页面路由注册 |

### 影响范围
- **API**: 无新增服务接口；GameProfile 新增 IsSelected 属性
- **跨模块**: 是 — Core（GameProfile）同提交变更；同提交还更新了 App/Application/Core 三个模块的 .claude/docs 与 docs 文档及 docs/INDEX.md、docs/README.md
- **数据模型**: GameProfile 增加 UI 选中状态字段
- **配置**: 无

### 回滚指南（HIGH）
- 执行 `git revert 3f40dcf`。
- 回滚后检查：
  - `src/CrosshairPro.App/Views/GamesPage.xaml`：需确认恢复到内联 Apex 控件的旧版式，避免 ContentControl 悬空；
  - `src/CrosshairPro.App/App.xaml`：确认 ApexTemplate/GamePageSelector 资源移除；
  - 本提交是 c85e699 / c69660e / 55064e1 / 73c777d / eb57c0b 全部 Apex 功能的共同基线，单独 revert 必然与后续提交冲突，实际回滚需连同整个 Apex 功能系列一起评估。
- 可能副作用：GamePages 目录、页面模板选择机制整体消失，游戏配置入口不可用。

---

## 2026-07-31 - v1.0

### 新增

- 创建完整的配置协议文档
- `README.md` - 配置文件概述
- `videoconfig.md` - 视频配置详细选项
- `settings.md` - 游戏设置详细选项
- `launch-options.md` - 启动参数详解
- `data-model.md` - 数据结构定义

### 文档结构

```
docs/GameConfig/Apex/
├── README.md           # 配置协议概述
├── videoconfig.md      # 视频配置详解
├── settings.md         # 游戏设置详解
├── launch-options.md   # 启动参数详解
├── data-model.md       # 数据模型定义
└── CHANGELOG.md        # 变更日志
```

### 配置分类

- 显示设置：分辨率、窗口模式、亮度
- 画质设置：纹理、阴影、粒子、物理
- 性能设置：垂直同步、抗锯齿、DVS
- 游戏设置：键位绑定、鼠标、音频
- 启动参数：性能优化、配置执行、调试选项

---
**创建时间**: 2026-07-31