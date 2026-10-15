# CrosshairPro 文档索引

## 项目概述

CrosshairPro 是一款 Windows 桌面准心覆盖工具，为 FPS 游戏玩家提供可自定义的准心叠加层，支持游戏配置管理（Apex Legends / CS2）与中英文本地化。

**快速链接**:
- [项目主文档](README.md)
- [构建和运行](README.md#构建)
- [核心架构](README.md#核心架构)
- [功能列表](README.md#核心功能)

## 模块文档

### Core - 核心层
核心模型、接口和枚举定义，作为架构基础被所有模块依赖。（19 个源文件）

| 文档 | 描述 |
|------|------|
| [README](Core/README.md) | 模块概述、核心接口、枚举类型 |
| [数据模型](Core/data-model.md) | CrosshairConfig、Preset、GameProfile、Apex 系列模型详解 |
| [坑点](Core/pitfalls.md) | 深拷贝陷阱、ObservableProperty、颜色格式等 |
| [变更日志](Core/CHANGELOG.md) | 模块变更历史 |

### Infrastructure - 基础层
Windows API 封装、热键管理、Win32 P/Invoke 声明。（3 个源文件）

| 文档 | 描述 |
|------|------|
| [README](Infrastructure/README.md) | Win32 API、HotkeyManager、窗口管理 |
| [数据模型](Infrastructure/data-model.md) | Win32 结构体、常量、虚拟键码映射 |
| [坑点](Infrastructure/pitfalls.md) | P/Invoke 资源泄漏、句柄生命周期、热键冲突等 |
| [变更日志](Infrastructure/CHANGELOG.md) | 模块变更历史 |

### Services - 数据层
配置持久化、准心渲染实现，提供数据访问和渲染能力。（3 个源文件）

| 文档 | 描述 |
|------|------|
| [README](Services/README.md) | JsonConfigRepository、JsonPresetRepository、CrosshairRenderer |
| [数据模型](Services/data-model.md) | JSON 序列化配置、数据流、缓存策略 |
| [坑点](Services/pitfalls.md) | 文件锁、JSON 序列化、渲染性能、自定义图片等 |
| [变更日志](Services/CHANGELOG.md) | 模块变更历史 |

### Application - 应用层
业务服务抽象、依赖注入配置，隔离 UI 和数据实现。（9 个源文件）

| 文档 | 描述 |
|------|------|
| [README](Application/README.md) | IConfigurationService、IPresetService、DI 注册 |
| [API: 配置服务](Application/api-configuration.md) | IConfigurationService/ConfigurationService 方法详解 |
| [API: 游戏配置](Application/api-gameconfig.md) | IGameConfigService/IApexConfigService 方法详解 |
| [API: 预设服务](Application/api-preset.md) | IPresetService/PresetService 方法详解 |
| [数据模型](Application/data-model.md) | 服务接口、内部状态、数据流图 |
| [坑点](Application/pitfalls.md) | 配置引用、深拷贝、默认预设保护等 |
| [变更日志](Application/CHANGELOG.md) | 模块变更历史 |

### App - 表现层
WPF 应用入口、UI 层、窗口和控件实现。（46 个源文件）

| 文档 | 描述 |
|------|------|
| [README](App/README.md) | MainWindow、OverlayWindow、页面导航、本地化、主题系统 |
| [数据模型](App/data-model.md) | ViewModel、自定义控件、Design Tokens |
| [坑点](App/pitfalls.md) | 透明窗口、Shape 渲染、配置深拷贝等 |
| [变更日志](App/CHANGELOG.md) | 模块变更历史 |

### GameConfig - 游戏配置层（配置文档模块，无独立源码目录）
游戏配置文件协议、字段定义、解析规则与 CrosshairPro 游戏配置功能。

| 文档 | 描述 |
|------|------|
| [总览](GameConfig/README.md) | 两种配置通道（Apex 专用 / 通用策略）、配置文件存放规则、内置游戏档案 |

#### Apex Legends

| 文档 | 描述 |
|------|------|
| [README](GameConfig/Apex/README.md) | Apex Legends 配置协议概述 |
| [videoconfig](GameConfig/Apex/videoconfig.md) | 视频配置详细选项 |
| [settings](GameConfig/Apex/settings.md) | 游戏设置详细选项 |
| [launch-options](GameConfig/Apex/launch-options.md) | 启动参数详解 |
| [数据模型](GameConfig/Apex/data-model.md) | 配置数据结构定义 |
| [坑点](GameConfig/Apex/pitfalls.md) | Apex 配置坑点（文件格式、写入时机、备份等） |
| [变更日志](GameConfig/Apex/CHANGELOG.md) | 配置协议变更历史 |

#### Counter-Strike 2

| 文档 | 描述 |
|------|------|
| [README](GameConfig/CS2/README.md) | CS2 配置协议概述 |
| [video](GameConfig/CS2/video.md) | 视频配置详细选项 |
| [config](GameConfig/CS2/config.md) | 游戏配置详细选项 |
| [launch-options](GameConfig/CS2/launch-options.md) | 启动参数详解 |
| [数据模型](GameConfig/CS2/data-model.md) | 配置数据结构定义 |
| [integration](GameConfig/CS2/integration.md) | 与 CrosshairPro 集成指南（Strategy/Profile/Config 映射） |
| [坑点](GameConfig/CS2/pitfalls.md) | CS2 配置坑点（VAC、云同步、编码、CS2 vs CS:GO 差异） |
| [变更日志](GameConfig/CS2/CHANGELOG.md) | 配置协议变更历史 |

### design - 设计文档

| 文档 | 描述 |
|------|------|
| [tab-navigation-design](../.claude/docs/design/tab-navigation-design.md) | 标签页导航系统设计（权威位置 `.claude/docs/design/`） |
| [games-tab-overflow-design](../.claude/docs/design/games-tab-overflow-design.md) | 游戏页选择器设计（tab 溢出方案对比 + ComboBox 决策记录，已实现） |
| [pc-optimize-design](../.claude/docs/design/pc-optimize-design.md) | 电脑优化页设计（只开入口不代改原则、29 项清单与命令核验、ShellExecute 坑，已实现） |

## 文档统计

- **模块目录**: 7（5 个代码模块 + GameConfig 配置层 + design 设计文档）
- **文档文件**: 权威树 `.claude/docs/` 共 42 个 .md；docs/ 树 44 个 .md（42 份镜像 + README.md 与本索引）
- **最后更新**: 2026-10-15

## 最新变更

### 2026-10-15 更新
- 实现电脑优化管理页 [PcOptimizePage](App/README.md)：7 分类 29 项优化条目，每项仅提供「打开」按钮直达 Windows 管理页，全部操作由用户手动完成（不修改任何系统设置）
- 新增 [PcOptimizeViewModel](App/data-model.md)（7 组 29 项静态数据 + OpenItem 命令 + ProbePaths 厂商面板探测）与 LocalizedTextConverter（IMultiValueConverter，locKey + CurrentCulture 双绑定）
- resx 新增 PcOptimize.* 键 82 个 ×3（216→297），移除占位键 Games.PcOptimizeDesc
- 新增设计文档 [pc-optimize-design](../.claude/docs/design/pc-optimize-design.md)：研究方法（29 项命令经官方文档核对并本机实测）、ShellExecute 单字符串不拆参坑、避坑迷思（11 研究/6 上页）、验证记录
- 文件数口径更新（App 47）；App 根目录值转换器文件 6 个不变（新增 LocalizedTextConverter、移除 InverseBooleanToVisibilityConverter）

### 2026-10-14 更新
- 游戏页 tab 条改为「游戏」标签 + ComboBox 选择器（用户确认未来游戏 10+，tab 范式不适用）：[games-tab-overflow-design](../.claude/docs/design/games-tab-overflow-design.md) 重写为决策记录与实现说明
- 游戏页新增「电脑优化」占位 tab：[PcOptimizePage](App/README.md)（空状态占位，GameProfile 新增 builtin-pcopt 条目与 DisplayNameLocKey 本地化键）
- 修复游戏页 tab 与内容左对齐（消除子页面双重 20px 边距）、底部保存/重置按钮增加 16px 底部留白
- 新增设计文档 [games-tab-overflow-design](../.claude/docs/design/games-tab-overflow-design.md)（tab 溢出方案：横向滚动推荐 + 阈值分析，仅设计未实现）
- 文件数口径更新（Core 19 / App 46）；App 值转换器 5→6（新增 GameProfileTitleConverter）

### 2026-10-13 更新
- 全文档体系增量更新：本地化（Localization）、Apex/CS2 配置页面、GameConfigService/ApexConfigService、JsonPresetRepository 等新内容入册
- 新增 Application API 文档：[api-configuration](Application/api-configuration.md)、[api-gameconfig](Application/api-gameconfig.md)、[api-preset](Application/api-preset.md)
- 新增 [GameConfig 总览](GameConfig/README.md)（两种配置通道、内置游戏档案）与 [Apex 坑点](GameConfig/Apex/pitfalls.md)
- 主文档与索引对齐文件数口径（Core 18 / Infrastructure 3 / Services 3 / Application 9 / App 41 / GameConfig 16）

### 2026-09-01 更新
- 完善 CS2 配置文档，新增集成指南与坑点文档
- 新增 CS2 [integration.md](GameConfig/CS2/integration.md) - 与 CrosshairPro `GameConfigStrategy`/`GameProfile` 映射
- 新增 CS2 [pitfalls.md](GameConfig/CS2/pitfalls.md) - VAC 边界、Steam 云同步冲突、CS2 vs CS:GO 命令差异、文件 patch 而非覆盖等

### 2026-07-31 更新
- 新增 CS2 与 Apex Legends 配置协议文档（字段级 video/config/launch-options/data-model）
- 新增游戏配置管理功能（GameConfig, GameConfigStrategy）
- 新增页面导航系统（CrosshairPage, GamesPage）与自定义控件（TabNavItem, IconButton, PageTemplateSelector）

## 文档导航

```
docs/
├── README.md                 # 项目主文档
├── INDEX.md                  # 本索引
├── Core/                     # 核心层
│   ├── README.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
├── Infrastructure/           # 基础层
│   ├── README.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
├── Services/                 # 数据层
│   ├── README.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
├── Application/              # 应用层
│   ├── README.md
│   ├── api-configuration.md
│   ├── api-gameconfig.md
│   ├── api-preset.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
├── App/                      # 表现层
│   ├── README.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
└── GameConfig/               # 游戏配置层（配置文档模块）
    ├── README.md             # 总览：两种配置通道
    ├── Apex/                 # Apex Legends 配置
    │   ├── README.md
    │   ├── videoconfig.md
    │   ├── settings.md
    │   ├── launch-options.md
    │   ├── data-model.md
    │   ├── pitfalls.md
    │   └── CHANGELOG.md
    └── CS2/                  # Counter-Strike 2 配置
        ├── README.md
        ├── video.md
        ├── config.md
        ├── launch-options.md
        ├── data-model.md
        ├── integration.md
        ├── pitfalls.md
        └── CHANGELOG.md

.claude/docs/design/          # 设计文档（权威位置，同步镜像到 docs/design/）
├── tab-navigation-design.md
├── games-tab-overflow-design.md
└── pc-optimize-design.md
```

## 快速查找

### 按主题查找

**配置管理**:
- [CrosshairConfig 模型](Core/data-model.md#crosshairconfig)
- [配置仓库接口](Core/README.md#iconfigrepository)
- [JSON 持久化](Services/README.md#jsonconfigrepository)

**准心渲染**:
- [渲染器接口](Core/README.md#icrosshairrenderer)
- [渲染实现](Services/README.md#crosshairrenderer)
- [样式详解](Services/data-model.md#渲染样式)

**热键系统**:
- [HotkeyManager](Infrastructure/README.md#hotkeymanager)
- [Win32 热键 API](Infrastructure/data-model.md#热键修饰符)
- [热键绑定模型](Core/data-model.md#hotkeybinding)

**预设管理**:
- [Preset 模型](Core/data-model.md#preset)
- [预设服务 API](Application/api-preset.md)
- [默认预设保护](Application/pitfalls.md#默认预设保护)

**游戏配置**:
- [两种配置通道总览](GameConfig/README.md)
- [Apex 配置协议](GameConfig/Apex/README.md) · [Apex 配置服务 API](Application/api-gameconfig.md)
- [CS2 配置协议](GameConfig/CS2/README.md) · [CS2 集成](GameConfig/CS2/integration.md) · [CS2 坑点](GameConfig/CS2/pitfalls.md)

**WPF UI**:
- [OverlayWindow](App/README.md#overlaywindow)
- [ViewModel 与控件](App/data-model.md)
- [主题系统](App/README.md#主题系统)

### 按问题查找

**问题: 配置修改后不生效？**
→ 查看 [Services/pitfalls.md](Services/pitfalls.md#文件锁并发)

**问题: 准心渲染性能差？**
→ 查看 [Services/pitfalls.md](Services/pitfalls.md#渲染缓存)

**问题: 热键注册失败？**
→ 查看 [Infrastructure/pitfalls.md](Infrastructure/pitfalls.md#热键冲突)

**问题: 透明窗口无法点击？**
→ 查看 [Infrastructure/pitfalls.md](Infrastructure/pitfalls.md#鼠标穿透)
→ 查看 [App/pitfalls.md](App/pitfalls.md#透明窗口)

**问题: 配置深拷贝不完整？**
→ 查看 [Core/pitfalls.md](Core/pitfalls.md#深拷贝陷阱)
→ 查看 [Application/pitfalls.md](Application/pitfalls.md#配置引用问题)

**问题: 写入游戏配置被拒/损坏？**
→ 查看 [GameConfig/CS2/pitfalls.md](GameConfig/CS2/pitfalls.md) 与 [GameConfig/Apex/pitfalls.md](GameConfig/Apex/pitfalls.md)

## 贡献指南

文档格式遵循:
- Markdown 规范，中文书写
- 代码模块文档：README（≤200 行）+ data-model + pitfalls + CHANGELOG；对外暴露服务的方法级详情拆入 api-{域}.md
- GameConfig 按游戏分子目录：README + 字段级文档 + data-model + pitfalls + CHANGELOG
- 同一事实只在一个模块文档中详述，其他模块用相对链接引用
- 使用相对路径链接；权威文档树为 `.claude/docs/`，docs/ 为其同步镜像

## 许可证

本文档项目遵循项目主许可证。
