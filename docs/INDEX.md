# CrosshairPro 文档索引

## 项目概述

CrosshairPro 是一款 Windows 桌面准心覆盖工具，为 FPS 游戏玩家提供可自定义的准心叠加层。

**快速链接**:
- [项目主文档](README.md)
- [构建和运行](README.md#构建)
- [核心架构](README.md#核心架构)
- [功能列表](README.md#核心功能)

## 模块文档

### Core - 核心层
核心模型、接口和枚举定义，作为架构基础被所有模块依赖。

| 文档 | 描述 |
|------|------|
| [README](Core/README.md) | 模块概述、核心接口、枚举类型 |
| [数据模型](Core/data-model.md) | CrosshairConfig、Preset、EffectsConfig 等模型详解 |
| [坑点](Core/pitfalls.md) | 深拷贝陷阱、ObservableProperty、颜色格式等 |
| [变更日志](Core/CHANGELOG.md) | 模块变更历史 |

### Infrastructure - 基础层
Windows API 封装、热键管理、Win32 P/Invoke 声明。

| 文档 | 描述 |
|------|------|
| [README](Infrastructure/README.md) | Win32 API、HotkeyManager、窗口管理 |
| [数据模型](Infrastructure/data-model.md) | Win32 结构体、常量、虚拟键码映射 |
| [坑点](Infrastructure/pitfalls.md) | P/Invoke 资源泄漏、句柄生命周期、热键冲突等 |
| [变更日志](Infrastructure/CHANGELOG.md) | 模块变更历史 |

### Services - 数据层
配置持久化、准心渲染实现，提供数据访问和渲染能力。

| 文档 | 描述 |
|------|------|
| [README](Services/README.md) | JsonConfigRepository、CrosshairRenderer |
| [数据模型](Services/data-model.md) | JSON 序列化配置、数据流、缓存策略 |
| [坑点](Services/pitfalls.md) | 文件锁、JSON 序列化、渲染性能、自定义图片等 |
| [变更日志](Services/CHANGELOG.md) | 模块变更历史 |

### Application - 应用层
业务服务抽象、依赖注入配置，隔离 UI 和数据实现。

| 文档 | 描述 |
|------|------|
| [README](Application/README.md) | IConfigurationService、IPresetService、DI 注册 |
| [数据模型](Application/data-model.md) | 服务接口、内部状态、数据流图 |
| [坑点](Application/pitfalls.md) | 配置引用、深拷贝、默认预设保护等 |
| [变更日志](Application/CHANGELOG.md) | 模块变更历史 |

### App - 表现层
WPF 应用入口、UI 层、窗口和控件实现。

| 文档 | 描述 |
|------|------|
| [README](App/README.md) | MainWindow、OverlayWindow、主题系统 |
| [数据模型](App/data-model.md) | MainViewModel、自定义控件、Design Tokens |
| [坑点](App/pitfalls.md) | 透明窗口、Shape 渲染、配置深拷贝等 |
| [变更日志](App/CHANGELOG.md) | 模块变更历史 |

### GameConfig - 游戏配置层
游戏配置文件协议、字段定义、解析规则。

#### Apex Legends

| 文档 | 描述 |
|------|------|
| [README](GameConfig/Apex/README.md) | Apex Legends 配置协议概述 |
| [videoconfig](GameConfig/Apex/videoconfig.md) | 视频配置详细选项 |
| [settings](GameConfig/Apex/settings.md) | 游戏设置详细选项 |
| [launch-options](GameConfig/Apex/launch-options.md) | 启动参数详解 |
| [数据模型](GameConfig/Apex/data-model.md) | 配置数据结构定义 |
| [变更日志](GameConfig/Apex/CHANGELOG.md) | 配置协议变更历史 |

#### Counter-Strike 2

| 文档 | 描述 |
|------|------|
| [README](GameConfig/CS2/README.md) | CS2 配置协议概述 |
| [video](GameConfig/CS2/video.md) | 视频配置详细选项 |
| [config](GameConfig/CS2/config.md) | 游戏配置详细选项 |
| [launch-options](GameConfig/CS2/launch-options.md) | 启动参数详解 |
| [数据模型](GameConfig/CS2/data-model.md) | 配置数据结构定义 |
| [变更日志](GameConfig/CS2/CHANGELOG.md) | 配置协议变更历史 |

## 文档统计

- **模块数**: 7
- **文档文件**: 33+
- **总行数**: 约 6500+ 行
- **最后更新**: 2026-07-31

## 最新变更

### 2026-07-31 更新
- 新增 CS2 (Counter-Strike 2) 配置协议文档
- 新增 CS2 视频配置详解 (video.md)
- 新增 CS2 游戏配置详解 (config.md)
- 新增 CS2 启动参数详解 (launch-options.md)
- 新增 CS2 数据模型定义 (data-model.md)
- 新增 Apex Legends 配置协议文档
- 包含 videoconfig.txt 和 settings.cfg 完整字段定义
- 提供数据模型、配置模板、优化建议
- 新增游戏配置管理功能（GameConfig, GameConfigStrategy）
- 新增 IGameConfigService 接口和 GameConfigService 实现
- 支持 8 款主流 FPS 游戏配置
- 新增页面导航系统（CrosshairPage, GamesPage）
- 新增视图模型（CrosshairViewModel, GamesViewModel）
- 新增自定义控件（TabNavItem, IconButton, PageTemplateSelector）
- 新增主题辅助类和图标资源

## 文档导航

```
docs/
├── README.md                 # 项目主文档
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
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
├── App/                      # 表现层
│   ├── README.md
│   ├── data-model.md
│   ├── pitfalls.md
│   └── CHANGELOG.md
└── GameConfig/               # 游戏配置层
    ├── Apex/                 # Apex Legends 配置
    │   ├── README.md
    │   ├── videoconfig.md
    │   ├── settings.md
    │   ├── launch-options.md
    │   ├── data-model.md
    │   └── CHANGELOG.md
    └── CS2/                  # Counter-Strike 2 配置
        ├── README.md
        ├── video.md
        ├── config.md
        ├── launch-options.md
        ├── data-model.md
        └── CHANGELOG.md
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
- [预设服务接口](Application/README.md#ipresetservice)
- [默认预设保护](Application/pitfalls.md#默认预设保护)

**WPF UI**:
- [OverlayWindow](App/README.md#overlaywindow)
- [MainViewModel](App/data-model.md#mainviewmodel)
- [主题系统](App/README.md#主题系统)

**游戏配置**:
- [Apex 配置协议](GameConfig/Apex/README.md)
- [Apex 视频配置](GameConfig/Apex/videoconfig.md)
- [Apex 游戏设置](GameConfig/Apex/settings.md)
- [Apex 启动参数](GameConfig/Apex/launch-options.md)
- [CS2 配置协议](GameConfig/CS2/README.md)
- [CS2 视频配置](GameConfig/CS2/video.md)
- [CS2 游戏配置](GameConfig/CS2/config.md)
- [CS2 启动参数](GameConfig/CS2/launch-options.md)

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

## 贡献指南

文档格式遵循:
- Markdown 规范
- 每个模块 4 个文件：README、data-model、pitfalls、CHANGELOG
- README 控制在 200 行以内
- 使用相对路径链接

## 许可证

本文档项目遵循项目主许可证。