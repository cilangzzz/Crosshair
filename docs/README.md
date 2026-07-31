# CrosshairPro 项目文档

## 概述

CrosshairPro 是一款 Windows 桌面准心覆盖工具，为 FPS 游戏玩家提供可自定义的准心叠加层。支持多种准心样式（十字、点、圆、T形、X形、自定义图片）、效果（描边、阴影、发光）和热键控制。

## 构建

- **运行时**: .NET 8.0 (Windows)
- **语言**: C# 12
- **框架**: WPF
- **构建命令**: `dotnet build`
- **运行命令**: `dotnet run --project src/CrosshairPro.App`
- **打包命令**: `dotnet publish src/CrosshairPro.App -c Release -r win-x64 --self-contained`

## 项目结构

```
src/
├── CrosshairPro.Core/          # 核心层：模型、接口、枚举
│   ├── Enums/                  # AppState, CrosshairStyle
│   ├── Interfaces/             # IConfigRepository, IAppStateRepository, ICrosshairRenderer, IHotkeyManager, IGameConfigService
│   └── Models/                 # CrosshairConfig, EffectsConfig, Preset, AppPersistedState, GameConfig, GameConfigStrategy
├── CrosshairPro.Infrastructure/ # 基础层：Win32 API、热键
│   ├── Hotkey/                 # HotkeyManager, WinHotkeyProvider
│   └── Win32/                  # Win32Constants, Win32Methods (P/Invoke)
├── CrosshairPro.Services/      # 数据层：配置、渲染实现
│   ├── Configuration/          # JsonConfigRepository (实现 IConfigRepository + IAppStateRepository)
│   └── Crosshair/              # CrosshairRenderer
├── CrosshairPro.Application/   # 应用服务层：业务逻辑抽象
│   ├── DI/                     # ServiceCollectionExtensions (依赖注入注册)
│   ├── Interfaces/             # IConfigurationService, IPresetService, IGameConfigService
│   └── Services/               # ConfigurationService, PresetService, GameConfigService
└── CrosshairPro.App/           # 表现层：WPF UI
    ├── Controls/               # CrosshairPreview, DialogBase, ToastNotification, TabNavItem, IconButton
    ├── Themes/                 # ControlStyles.xaml, DesignTokens.xaml, IconGeometries.xaml
    ├── ViewModels/             # MainViewModel, CrosshairViewModel, GamesViewModel
    └── Views/                  # OverlayWindow, CrosshairPage, GamesPage
```

## 模块索引

| 层级 | 模块 | 描述 | 文件数 | 文档 |
|------|------|------|--------|------|
| 基础层 | Core | 核心模型、接口、枚举定义 | 17 | [README](Core/README.md) |
| 基础层 | Infrastructure | Win32 API封装、热键管理 | 6 | [README](Infrastructure/README.md) |
| 数据层 | Services | 配置持久化、准心渲染实现 | 9 | [README](Services/README.md) |
| 应用层 | Application | 业务服务抽象、依赖注入 | 11 | [README](Application/README.md) |
| 表现层 | App | WPF应用入口、UI层 | 20+ | [README](App/README.md) |

## 核心架构

```
┌─────────────────────────────────────────────────────┐
│                    App (WPF UI)                      │
│  MainWindow ←→ MainViewModel ←→ OverlayWindow       │
└───────────────────────┬─────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│               Application (Services)                 │
│  ConfigurationService    PresetService              │
│  (配置管理)              (预设管理)                   │
└───────────────────────┬─────────────────────────────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  Services   │ │Infrastructure│ │    Core     │
│ - JsonRepo  │ │ - HotkeyMgr │ │ - Models    │
│ - Renderer  │ │ - Win32 API │ │ - Interfaces│
└─────────────┘ └─────────────┘ └─────────────┘
```

**数据流**:
1. `App.OnStartup` 配置 DI 容器，注册所有服务
2. `MainViewModel` 通过构造函数注入 `IConfigurationService` 和 `IPresetService`
3. 用户修改配置 → `ConfigurationService` 管理配置状态 → `JsonConfigRepository` 持久化
4. `OverlayWindow` 监听 `ConfigUpdated` 事件，使用 `CrosshairRenderer` 渲染准心

## 模块依赖关系

```
App → Application → Services, Infrastructure
                → Core
Services → Core
Infrastructure → Core
Core → (无依赖)
```

## 环境配置

- **配置目录**: `%APPDATA%/CrosshairPro/`
- **配置文件**: `config.json` (当前配置), `appstate.json` (应用状态), `presets/{id}.json` (预设)
- **日志文件**: `%APPDATA%/CrosshairPro/logs/`

## 核心功能

### 1. 准心样式
- **十字准心** (Cross): 可调整大小、间隙、厚度
- **点状准心** (Dot): 可调整大小
- **圆形准心** (Circle): 可调整直径、厚度
- **T形准心** (TShape): 可调整大小、间隙
- **X形准心** (XShape): 可调整大小、厚度、旋转角度
- **自定义图片** (CustomImage): 支持加载外部图片文件

### 2. 效果系统
- **描边** (Outline): 可调整颜色、厚度
- **阴影** (Shadow): 可调整颜色、模糊半径、偏移
- **发光** (Glow): 可调整颜色、强度、范围

### 3. 热键控制
- 切换准心显示/隐藏
- 切换准心样式
- 调整准心参数（大小、间隙、厚度等）
- 快速切换预设

### 4. 预设管理
- 创建、编辑、删除预设
- 预设关联游戏进程
- 预设导入/导出
- 默认预设保护机制

### 5. 游戏检测
- 自动检测游戏进程
- 游戏运行时自动切换预设
- 支持自定义游戏配置文件

### 6. 游戏配置管理（新增）
- 支持 8 款主流 FPS 游戏配置（CS2、Valorant、Apex、Overwatch2、PUBG、Fortnite、R6、CSGO）
- 游戏启动项参数管理
- 视频设置、游戏设置配置
- 配置项类型支持：布尔开关、整数数值、枚举选择、字符串
- 配置策略系统，每个游戏有独立的配置模板

### 7. UI 增强（新增）
- 标签页导航系统（CrosshairPage、GamesPage）
- 自定义控件：TabNavItem、IconButton、ToastNotification
- 主题系统和设计令牌（Design Tokens）
- 图标几何资源（IconGeometries）

## 重要坑点

1. **分层架构**: Application 层抽象了业务逻辑，App 层只依赖 Application 接口，不直接依赖 Services
2. **状态持久化**: `AppPersistedState` 记录当前使用的预设ID，应用启动时恢复
3. **OverlayWindow 鼠标穿透**: 使用 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW` 扩展样式
4. **WPF Shape 渲染**: 在 `AllowsTransparency=true` 窗口中，必须对每个 Shape 单独设置 Opacity
5. **配置深拷贝**: `ConfigurationService.CloneConfig()` 必须递归克隆所有嵌套对象
6. **预设管理**: 默认预设（IsDefault=true）不能被删除/保存
7. **服务单例**: `JsonConfigRepository` 同时实现 `IConfigRepository` 和 `IAppStateRepository`，共享文件锁

## 模块变更日志

每个模块的变更历史记录在 `docs/{模块}/CHANGELOG.md`，排查问题时优先阅读。

## 文档索引

| 模块 | README | 数据模型 | 坑点 | CHANGELOG |
|------|--------|----------|------|-----------|
| Core | [README](Core/README.md) | [data-model](Core/data-model.md) | [pitfalls](Core/pitfalls.md) | [CHANGELOG](Core/CHANGELOG.md) |
| Infrastructure | [README](Infrastructure/README.md) | [data-model](Infrastructure/data-model.md) | [pitfalls](Infrastructure/pitfalls.md) | [CHANGELOG](Infrastructure/CHANGELOG.md) |
| Services | [README](Services/README.md) | [data-model](Services/data-model.md) | [pitfalls](Services/pitfalls.md) | [CHANGELOG](Services/CHANGELOG.md) |
| Application | [README](Application/README.md) | [data-model](Application/data-model.md) | [pitfalls](Application/pitfalls.md) | [CHANGELOG](Application/CHANGELOG.md) |
| App | [README](App/README.md) | [data-model](App/data-model.md) | [pitfalls](App/pitfalls.md) | [CHANGELOG](App/CHANGELOG.md) |