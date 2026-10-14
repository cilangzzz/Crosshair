# CrosshairPro

## 概述

CrosshairPro 是一款 Windows 桌面准心覆盖工具（WPF / .NET 8），为 FPS 游戏玩家提供可自定义的准心叠加层：多种样式（十字、点、圆、T形、X形、自定义图片）与效果（描边、阴影、发光），支持全局热键、预设管理、游戏进程匹配及游戏配置管理（Apex Legends / CS2），内置中英文本地化。

## 构建

- **运行时**: .NET 8.0 (Windows)，C# 12，WPF
- **构建**: `dotnet build`
- **运行**: `dotnet run --project src/CrosshairPro.App`
- **打包**: `dotnet publish src/CrosshairPro.App -c Release -r win-x64 --self-contained`

## 项目结构

```
src/
├── CrosshairPro.Core/            # 核心层：模型、接口、枚举（无项目依赖）
│   ├── Enums/                    # AppState, CrosshairStyle
│   ├── Interfaces/               # IConfigRepository+IAppStateRepository+IPresetRepository, ICrosshairRenderer, IHotkeyManager, IGameDetector（预留，暂无实现）
│   └── Models/                   # CrosshairConfig, EffectsConfig, Preset, AppPersistedState, HotkeyBinding,
│                                 #   GameInfo, GameProfile, GameConfig, GameConfigStrategy,
│                                 #   ApexVideoConfig, ApexSettingsConfig, ApexLaunchOptions
├── CrosshairPro.Infrastructure/  # 基础层：Win32 P/Invoke、全局热键
│   ├── Hotkey/                   # HotkeyManager (IHotkeyManager)
│   └── Win32/                    # Win32Constants, Win32Methods
├── CrosshairPro.Services/        # 数据层：JSON 持久化、渲染实现
│   ├── Configuration/            # JsonConfigRepository（IConfigRepository+IAppStateRepository）, JsonPresetRepository（IPresetRepository）
│   └── Crosshair/                # CrosshairRenderer（ICrosshairRenderer）
├── CrosshairPro.Application/     # 应用服务层：业务抽象、DI 注册
│   ├── DI/                       # ServiceCollectionExtensions（AddCrosshairProServices）
│   ├── Interfaces/               # IConfigurationService, IPresetService, IGameConfigService, IApexConfigService
│   └── Services/                 # ConfigurationService, PresetService, GameConfigService, ApexConfigService
└── CrosshairPro.App/             # 表现层：WPF UI
    ├── Localization/             # LocExtension, LocalizationProvider, Strings（resx 中英双语）
    ├── Helpers/                  # ThemeHelper
    ├── Controls/                 # DialogBase, ToastManager/ToastNotification, IconButton, TabNavItem, CrosshairPreview, SmoothScrollViewer, ScrollViewerBehavior, PageTemplateSelector
    ├── Themes/                   # DesignTokens, ControlStyles, IconGeometries
    ├── ViewModels/               # MainViewModel, CrosshairViewModel, GamesViewModel, ApexConfigViewModel, Cs2ConfigViewModel, ConfigItemWrapper
    └── Views/                    # OverlayWindow, CrosshairPage, GamesPage, GamePages/(ApexPage, Cs2Page, PcOptimizePage, GamePageSelector)
```

（App 根目录另有 App.xaml(.cs)、MainWindow.xaml(.cs) 和 6 个值转换器）

## 模块索引

| 层级 | 模块 | 描述 | 文件数 | 文档 |
|------|------|------|--------|------|
| 基础层 | Core | 核心模型、接口、枚举定义 | 19 | [README](docs/Core/README.md) |
| 基础层 | Infrastructure | Win32 API 封装、热键管理 | 3 | [README](docs/Infrastructure/README.md) |
| 数据层 | Services | 配置持久化、准心渲染实现 | 3 | [README](docs/Services/README.md) |
| 应用层 | Application | 业务服务抽象、依赖注入 | 9 | [README](docs/Application/README.md) |
| 表现层 | App | WPF 应用入口、UI 层 | 46 | [README](docs/App/README.md) |
| 配置层 | GameConfig | 游戏配置协议文档（无独立源码目录） | 16 | [README](docs/GameConfig/README.md) |

文件数口径：代码模块 = `find src/CrosshairPro.{模块} -type f ( -name "*.cs" -o -name "*.xaml" ) ! -path "*/obj/*" ! -path "*/bin/*" | wc -l`；GameConfig = `.claude/docs/GameConfig` 下全部 .md 文件数。

## 核心架构

```
┌─────────────────────────────────────────────────────────────┐
│                       App (WPF UI)                           │
│  MainWindow ←→ MainViewModel ←→ CrosshairViewModel 等 5 个   │
│       │              │ ConfigUpdated 事件                    │
│  OverlayWindow ←─────┘（全屏透明置顶 + 鼠标穿透）              │
└──────────────────────────┬──────────────────────────────────┘
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              Application（业务服务，全部 Singleton）           │
│  ConfigurationService   PresetService                        │
│  GameConfigService（2 款内置游戏策略）                          │
│  ApexConfigService（Apex 专用：读写游戏真实配置 + 备份）        │
└──────┬───────────────────┬──────────────────┬───────────────┘
       ▼                   ▼                  ▼
┌──────────────┐  ┌─────────────────┐  ┌──────────────┐
│  Services    │  │  Infrastructure │  │     Core     │
│ JsonConfig   │  │  HotkeyManager  │  │ Models       │
│ JsonPreset   │  │  Win32 P/Invoke │  │ Interfaces   │
│ Renderer     │  │                 │  │ Enums        │
└──────────────┘  └─────────────────┘  └──────────────┘
```

**数据流**:
1. `App.OnStartup` 构建容器：`AddCrosshairProServices()` 注册全部仓库/服务（Singleton）；App 内补充注册 5 个 ViewModel（Singleton）、OverlayWindow（Singleton）、MainWindow（Transient）
2. `MainViewModel` 注入 `IPresetService` + `IConfigurationService`；`MainWindow` 注入 `IHotkeyManager` 并注册全局热键
3. 用户修改配置 → `CrosshairViewModel` 触发 `ConfigUpdated` → `MainWindow` 同步到 `OverlayWindow`
4. 配置经 `ConfigurationService`/`PresetService` 由 `JsonConfigRepository`/`JsonPresetRepository` 持久化到 %APPDATA%
5. 语言偏好存于 `appstate.json`，启动时经 `LocalizationProvider` 恢复

## 模块依赖关系

```
App         → Application, Services, Infrastructure, Core
Application → Services, Infrastructure, Core
Services    → Infrastructure, Core
Infrastructure → Core
Core        → (无依赖)
```

csproj 物理引用比理想分层更宽（App 直接引用全部四层），见坑点 1。

## 环境配置

- **配置目录**: `%APPDATA%/CrosshairPro/`
- **准心相关**: `config.json`（当前配置）、`appstate.json`（应用状态/语言）、`presets/{id}.json`（预设）
- **游戏配置**: `gameconfigs/{gameId}.json`（通用策略通道）、`launch_options.txt` 与 `apex-backups/`（Apex 专用通道），详见 [docs/GameConfig/README.md](docs/GameConfig/README.md)
- **日志**: Serilog 静态日志仅在 ViewModel 记录异常，未配置文件 sink（不存在 logs/ 目录）

## 重要坑点

1. **分层依赖非严格隔离**: App.csproj 直接引用全部四个项目（如 MainWindow 直接 using Infrastructure.Hotkey），并非只经 Application 接口；新增跨层调用先确认 DI 接口是否已暴露
2. **状态持久化**: `AppPersistedState` 记录当前预设 ID 与界面语言，应用启动时恢复
3. **OverlayWindow 鼠标穿透**: 使用 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW` 扩展样式
4. **WPF Shape 渲染**: 在 `AllowsTransparency=true` 窗口中必须对每个 Shape 单独设置 Opacity
5. **配置深拷贝**: `ConfigurationService.CloneConfig()` 必须递归克隆所有嵌套对象
6. **默认预设保护**: `IsDefault=true` 的预设不能删除/覆盖保存（PresetService 校验）
7. **仓库单例**: `JsonConfigRepository` 同时实现 IConfigRepository 与 IAppStateRepository，DI 注册为同一实例共享文件锁；`JsonPresetRepository` 独立文件锁
8. **CS2 配置写入时机**: 游戏运行时不得 patch video.txt/config.cfg（会冲突），仅在未运行时写入，见 [docs/GameConfig/CS2/pitfalls.md](docs/GameConfig/CS2/pitfalls.md)
9. **配置文件编码**: 游戏配置文件需 UTF-8 无 BOM，否则可能被游戏拒绝解析
10. **VAC 边界**: 不写入任何受 VAC 监控的命令，仅 patch 白名单字段，见 [docs/GameConfig/CS2/pitfalls.md](docs/GameConfig/CS2/pitfalls.md)

## 模块变更日志

每个模块的变更历史记录在 `docs/{模块}/CHANGELOG.md`（如 [docs/Core/CHANGELOG.md](docs/Core/CHANGELOG.md)），排查问题时优先阅读。

## 文档索引

权威文档树为 `.claude/docs/`，下表覆盖其全部 .md 文件（链接前缀 `docs/` 相对本文件解析到 `.claude/docs/`）：

| 模块 | 文档 |
|------|------|
| Core | [README](docs/Core/README.md) · [data-model](docs/Core/data-model.md) · [pitfalls](docs/Core/pitfalls.md) · [CHANGELOG](docs/Core/CHANGELOG.md) |
| Infrastructure | [README](docs/Infrastructure/README.md) · [data-model](docs/Infrastructure/data-model.md) · [pitfalls](docs/Infrastructure/pitfalls.md) · [CHANGELOG](docs/Infrastructure/CHANGELOG.md) |
| Services | [README](docs/Services/README.md) · [data-model](docs/Services/data-model.md) · [pitfalls](docs/Services/pitfalls.md) · [CHANGELOG](docs/Services/CHANGELOG.md) |
| Application | [README](docs/Application/README.md) · [api-configuration](docs/Application/api-configuration.md) · [api-gameconfig](docs/Application/api-gameconfig.md) · [api-preset](docs/Application/api-preset.md) · [data-model](docs/Application/data-model.md) · [pitfalls](docs/Application/pitfalls.md) · [CHANGELOG](docs/Application/CHANGELOG.md) |
| App | [README](docs/App/README.md) · [data-model](docs/App/data-model.md) · [pitfalls](docs/App/pitfalls.md) · [CHANGELOG](docs/App/CHANGELOG.md) |
| GameConfig | [README](docs/GameConfig/README.md) |
| GameConfig/Apex | [README](docs/GameConfig/Apex/README.md) · [videoconfig](docs/GameConfig/Apex/videoconfig.md) · [settings](docs/GameConfig/Apex/settings.md) · [launch-options](docs/GameConfig/Apex/launch-options.md) · [data-model](docs/GameConfig/Apex/data-model.md) · [pitfalls](docs/GameConfig/Apex/pitfalls.md) · [CHANGELOG](docs/GameConfig/Apex/CHANGELOG.md) |
| GameConfig/CS2 | [README](docs/GameConfig/CS2/README.md) · [video](docs/GameConfig/CS2/video.md) · [config](docs/GameConfig/CS2/config.md) · [launch-options](docs/GameConfig/CS2/launch-options.md) · [integration](docs/GameConfig/CS2/integration.md) · [data-model](docs/GameConfig/CS2/data-model.md) · [pitfalls](docs/GameConfig/CS2/pitfalls.md) · [CHANGELOG](docs/GameConfig/CS2/CHANGELOG.md) |
| design | [tab-navigation-design](docs/design/tab-navigation-design.md) · [games-tab-overflow-design](docs/design/games-tab-overflow-design.md) |
