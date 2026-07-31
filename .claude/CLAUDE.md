# CrosshairPro

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
│   ├── Interfaces/             # IConfigRepository, IAppStateRepository, ICrosshairRenderer, IHotkeyManager
│   └── Models/                 # CrosshairConfig, EffectsConfig, Preset, AppPersistedState
├── CrosshairPro.Infrastructure/ # 基础层：Win32 API、热键
│   ├── Hotkey/                 # HotkeyManager, WinHotkeyProvider
│   └── Win32/                  # Win32Constants, Win32Methods (P/Invoke)
├── CrosshairPro.Services/      # 数据层：配置、渲染实现
│   ├── Configuration/          # JsonConfigRepository (实现 IConfigRepository + IAppStateRepository)
│   └── Crosshair/              # CrosshairRenderer
├── CrosshairPro.Application/   # 应用服务层：业务逻辑抽象
│   ├── DI/                     # ServiceCollectionExtensions (依赖注入注册)
│   ├── Interfaces/             # IConfigurationService, IPresetService
│   └── Services/               # ConfigurationService, PresetService
└── CrosshairPro.App/           # 表现层：WPF UI
    ├── Controls/               # CrosshairPreview, DialogBase, ToastNotification
    ├── Themes/                 # ControlStyles.xaml, DesignTokens.xaml
    ├── ViewModels/             # MainViewModel
    └── Views/                  # OverlayWindow
```

## 模块索引

| 层级 | 模块 | 描述 | 文件数 | 文档 |
|------|------|------|--------|------|
| 基础层 | Core | 核心模型、接口、枚举定义 | 13 | [README](docs/Core/README.md) |
| 基础层 | Infrastructure | Win32 API封装、热键管理 | 3 | [README](docs/Infrastructure/README.md) |
| 数据层 | Services | 配置持久化、准心渲染实现 | 3 | [README](docs/Services/README.md) |
| 应用层 | Application | 业务服务抽象、依赖注入 | 5 | [README](docs/Application/README.md) |
| 表现层 | App | WPF应用入口、UI层 | 13 | [README](docs/App/README.md) |

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

## 依赖注入配置

```csharp
// Application/DI/ServiceCollectionExtensions.cs
services.AddCrosshairProServices()
    .AddSingleton<MainViewModel>()
    .AddSingleton<OverlayWindow>()
    .AddTransient<MainWindow>();
```

**服务生命周期**:
- Singleton: 所有仓库、服务、ViewModel、OverlayWindow
- Transient: MainWindow

## 环境配置

- **配置目录**: `%APPDATA%/CrosshairPro/`
- **配置文件**: `config.json` (当前配置), `appstate.json` (应用状态), `presets/{id}.json` (预设)
- **日志文件**: `%APPDATA%/CrosshairPro/logs/`

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
| Core | [README](docs/Core/README.md) | [data-model](docs/Core/data-model.md) | [pitfalls](docs/Core/pitfalls.md) | [CHANGELOG](docs/Core/CHANGELOG.md) |
| Infrastructure | [README](docs/Infrastructure/README.md) | [data-model](docs/Infrastructure/data-model.md) | [pitfalls](docs/Infrastructure/pitfalls.md) | [CHANGELOG](docs/Infrastructure/CHANGELOG.md) |
| Services | [README](docs/Services/README.md) | [data-model](docs/Services/data-model.md) | [pitfalls](docs/Services/pitfalls.md) | [CHANGELOG](docs/Services/CHANGELOG.md) |
| Application | [README](docs/Application/README.md) | [data-model](docs/Application/data-model.md) | [pitfalls](docs/Application/pitfalls.md) | [CHANGELOG](docs/Application/CHANGELOG.md) |
| App | [README](docs/App/README.md) | [data-model](docs/App/data-model.md) | [pitfalls](docs/App/pitfalls.md) | [CHANGELOG](docs/App/CHANGELOG.md) |
