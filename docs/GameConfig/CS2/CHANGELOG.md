# CS2 配置变更日志

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-09-01] feat: add CS2 config support and update docs

**类型**: feat
**提交**: f5d4d18
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| docs/GameConfig/CS2/integration.md | +466/-0 | 新增集成指南：GameProfile（cs2.exe 进程匹配）注册流程、GameConfigStrategy（builtin-cs2）字段映射到 video.txt / config.cfg、读取/保存/应用数据流、实施检查清单 |
| docs/GameConfig/CS2/pitfalls.md | +401/-0 | 新增坑点：VAC 反作弊边界与白名单、CS2 vs CS:GO 命令差异（`mat_queue_mode` 等已移除）、video.txt 写入时机与 Steam 云同步冲突、autoexec.cfg 执行机制、UTF-8 无 BOM 编码、准心叠加共存、启动参数长度限制、备份与回滚策略 |
| docs/GameConfig/CS2/data-model.md | +98/-1 | 协议版本 1.0→1.1；新增「CrosshairPro 集成数据模型」章节：GameProfile / GameConfigStrategy / GameConfig 定义与 Strategy Key→CS2 cvar 字段映射表（如 `video.fullscreen`→`setting.fullscreen` 写入 video.txt） |
| docs/GameConfig/CS2/README.md | +5/-1 | 文档索引补充 integration.md / pitfalls.md 链接，协议版本升至 1.1 |
| docs/GameConfig/CS2/CHANGELOG.md | +35/-0 | 记录 v1.1 上述文档变更 |
| src/CrosshairPro.App/ViewModels/Cs2ConfigViewModel.cs | +195/-0 | （跨模块·App）新增 CS2 配置视图模型：GameId 固定 `"builtin-cs2"`，经 IGameConfigService 加载/保存 GameConfig，`Sections` 集合绑定策略字段，定义 1000 字符上限常量 `MaxLaunchOptionsLength`（尚未据此强制校验），`ToastRequested` 事件反馈结果 |
| src/CrosshairPro.App/Views/GamePages/Cs2Page.xaml | +194/-0 | （跨模块·App）新增 CS2 配置页面 UI |
| src/CrosshairPro.App/Views/GamePages/Cs2Page.xaml.cs | +55/-0 | （跨模块·App）页面后台：初始化 LoadAsync 与保存流程 |
| src/CrosshairPro.App/Views/GamePages/GamePageSelector.cs | +5/-1 | （跨模块·App）新增 `Cs2Template` 属性，游戏 ID 为 `"builtin-cs2"` 时返回 CS2 页面模板 |
| src/CrosshairPro.App/Localization/Strings.resx | +64/-0 | （跨模块·App）CS2 页面词条（默认语言） |
| src/CrosshairPro.App/Localization/Strings.en.resx | +64/-0 | （跨模块·App）CS2 词条（英文） |
| src/CrosshairPro.App/Localization/Strings.zh-CN.resx | +64/-0 | （跨模块·App）CS2 词条（简体中文） |
| src/CrosshairPro.App/Views/GamesPage.xaml | +5/-0 | （跨模块·App）注册 Cs2Template 数据模板指向 Cs2Page |
| src/CrosshairPro.App/App.xaml.cs | +1/-0 | （跨模块·App）DI 注册 Cs2ConfigViewModel 单例 |

### 影响范围
- **API**: 未改动既有接口；App 层新增 Cs2ConfigViewModel（DI 单例，构造注入 IGameConfigService）与 GamePageSelector.Cs2Template 属性
- **跨模块**: 是。同提交修改 App 模块源码 9 个文件（上表），完整分析见 [App/CHANGELOG](../../../App/CHANGELOG.md) 同哈希条目；Application / Core / Services / Infrastructure 层源码无变更
- **数据模型**: 无源码模型变更；data-model.md 新增内容是对既有 GameProfile / GameConfigStrategy / GameConfig 模型的文档化映射
- **配置**: CS2 配置开始持久化到 `%APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json`（经 GameConfigService 读写）

---

## [2026-07-31] feat: implement Apex Legends config service and CS2 docs

**类型**: feat
**提交**: c85e699
**风险**: LOW

### 变更文件
（本模块部分——纯文档新增，无源码变更）
| 文件 | 变更 | 说明 |
|------|------|------|
| docs/GameConfig/CS2/README.md | +236/-0 | 新建配置协议概述：video.txt / config.cfg / autoexec.cfg / cs2_machine_convars.vcfg 用途、Steam 云同步文件位置、配置分类、CS2 vs CS:GO 差异表，协议版本 1.0 |
| docs/GameConfig/CS2/config.md | +447/-0 | 新建游戏配置详解（键位/鼠标/HUD 等 cvar） |
| docs/GameConfig/CS2/data-model.md | +407/-0 | 新建数据结构定义 |
| docs/GameConfig/CS2/launch-options.md | +389/-0 | 新建启动参数详解 |
| docs/GameConfig/CS2/video.md | +228/-0 | 新建视频配置详解 |
| docs/GameConfig/CS2/CHANGELOG.md | +47/-0 | 新建本变更日志并记录 v1.0 |

### 影响范围
- **API**: 无（本模块仅新增文档）
- **跨模块**: 是。同提交主体为 Apex 配置服务落地：Application 层新增 IApexConfigService(+233) / ApexConfigService(+900) 并注册 DI，Core 新增 ApexVideoConfig(+263) / ApexSettingsConfig(+251) / ApexLaunchOptions(+119)，App 层大幅扩展 ApexConfigViewModel / ApexPage——该部分风险评级 HIGH，详见 [Application/CHANGELOG](../../../Application/CHANGELOG.md) 与 [Apex/CHANGELOG](../Apex/CHANGELOG.md) 同哈希条目
- **数据模型**: 本模块无；同提交 Core 新增的 3 个模型均为 Apex 专用，与 CS2 无关
- **配置**: 无

---

## [2026-07-31] feat: implement tab navigation UI with pages and viewmodels

**类型**: feat
**提交**: ced3254
**风险**: HIGH

### 变更文件
（本模块相关部分——游戏配置服务地基与 builtin-cs2 策略；同提交另有约 68 个文件为本模块外变更）
| 文件 | 变更 | 说明 |
|------|------|------|
| src/CrosshairPro.Application/Services/GameConfigService.cs | +497/-0 | 新建游戏配置服务：内置 8 个游戏策略（cs2/valorant/apex/overwatch2/pubg/fortnite/r6/csgo），其中 `CreateCS2Strategy()` 定义 builtin-cs2 的 video 分区（fullscreen、resolution 8 档、aspect_ratio 3 档、refresh_rate 60-360Hz 默认 144）与 game 分区（fps_max 0-999、cl_showfps）；按 gameId 读写并缓存 `%APPDATA%/CrosshairPro/gameconfigs/{id}.json` |
| src/CrosshairPro.Application/Interfaces/IGameConfigService.cs | +40/-0 | 新建接口：GetStrategies / GetStrategy / GetConfigAsync / SaveConfigAsync / ResetToDefaultAsync / ApplyConfigAsync，CS2 页面（f5d4d18）经它读写配置 |
| src/CrosshairPro.Core/Models/GameConfigStrategy.cs | +91/-0 | 新建策略模型（Sections / ConfigSectionDefinition / ConfigItemDefinition），CS2 字段映射的载体 |
| src/CrosshairPro.Core/Models/GameConfig.cs | +20/-0 | 新建 GameConfig 持久化模型（GameId / LaunchOptions / Settings） |
| src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs | +1/-0 | 注册 `IGameConfigService → GameConfigService` 单例 |

### 影响范围
- **API**: 新增 IGameConfigService 接口（此后 CS2 配置读写的唯一通道）
- **跨模块**: 是且为主体。同提交共 74 个文件（+23897/-1032）：App 层完成标签页导航重构（CrosshairPage / GamesPage / TabNavItem / DialogBase 等），Core 新增 GameConfig / GameConfigStrategy，docs/ 各模块文档更新——完整清单与回滚分析见 [Application/CHANGELOG](../../../Application/CHANGELOG.md) 同哈希条目
- **数据模型**: 新增 GameConfig、GameConfigStrategy（均为新增文件，不改既有模型）
- **配置**: 新增运行时配置目录 `%APPDATA%/CrosshairPro/gameconfigs/`

### 回滚指南
- 回滚: `git revert ced3254` —— 后续 c85e699 / 3f40dcf / 55064e1 / f5d4d18 均在其基础上开发，需从新到旧先回滚这些提交，否则必然冲突
- 检查文件: src/CrosshairPro.Application/Services/GameConfigService.cs、src/CrosshairPro.Application/Interfaces/IGameConfigService.cs、src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs、src/CrosshairPro.Core/Models/GameConfig.cs、src/CrosshairPro.Core/Models/GameConfigStrategy.cs、App 层 Views/GamePages/ 与 ViewModels/GamesViewModel.cs
- 副作用: IGameConfigService 移除后 Cs2ConfigViewModel（f5d4d18）编译失败（ApexConfigViewModel 仅注入 IApexConfigService，不受影响），且 GameConfigService 实现与 ServiceCollectionExtensions 的注册需同步移除；docs/GameConfig 系列文档（含本目录）描述的策略字段映射将失去源码对应

---

## 2026-09-01 - v1.1

### 新增

- `integration.md` - CS2 与 CrosshairPro 集成指南
  - `GameProfile(cs2.exe)` 注册流程
  - `GameConfigStrategy(builtin-cs2)` 字段映射到 video.txt / config.cfg
  - 数据流：读取 / 保存 / 应用
  - 实施检查清单
- `pitfalls.md` - CS2 配置坑点
  - VAC 反作弊边界与白名单
  - CS2 vs CS:GO 命令差异（`mat_queue_mode`、`mat_picmip` 等）
  - video.txt 写入时机与 Steam 云同步冲突
  - autoexec.cfg 执行机制
  - 文件编码（UTF-8 无 BOM）
  - 准心叠加与游戏内准心共存
  - 启动参数长度限制
  - 备份与回滚策略

### 文档结构更新

```
docs/GameConfig/CS2/
├── README.md           # 配置协议概述
├── video.md            # 视频配置详解
├── config.md           # 游戏配置详解
├── launch-options.md   # 启动参数详解
├── data-model.md       # 数据模型定义
├── integration.md      # 与 CrosshairPro 集成（新增）
├── pitfalls.md         # CS2 配置坑点（新增）
└── CHANGELOG.md        # 变更日志
```

---

## 2026-07-31 - v1.0

### 新增

- 创建完整的 CS2 配置协议文档
- `README.md` - 配置协议概述
- `video.md` - 视频配置详解
- `config.md` - 游戏配置详解
- `launch-options.md` - 启动参数详解
- `data-model.md` - 数据结构定义

### 文档结构

```
docs/GameConfig/CS2/
├── README.md           # 配置协议概述
├── video.md            # 视频配置详解
├── config.md           # 游戏配置详解
├── launch-options.md   # 启动参数详解
├── data-model.md       # 数据模型定义
└── CHANGELOG.md        # 变更日志
```

### 配置分类

- 显示设置：分辨率、窗口模式、刷新率
- 画质设置：纹理、阴影、特效
- 性能设置：帧率、网络、插值
- 游戏设置：准心、键位绑定、鼠标
- 音频设置：音量、设备
- HUD设置：界面、雷达

### CS2 vs CS:GO 变化

| 特性 | CS:GO | CS2 |
|------|-------|-----|
| 引擎 | Source 1 | Source 2 |
| `mat_queue_mode` | 支持 | 已移除 |
| `mat_picmip` | 支持 | 已移除 |
| `fps_max_menu` | 支持 | 已移除 |
| 配置格式 | 兼容 | 兼容 |
| 大部分命令 | 兼容 | 兼容 |

---
**创建时间**: 2026-07-31