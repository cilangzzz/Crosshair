# CrosshairPro 项目文档

## 概述

CrosshairPro 是一款 Windows 桌面准心覆盖工具，为 FPS 游戏玩家提供可自定义的准心叠加层。支持多种准心样式（十字、点、圆、T形、X形、自定义图片）、效果（描边、阴影、发光）、全局热键控制、预设管理、游戏进程匹配与游戏配置管理（Apex Legends / CS2），内置中英文本地化。

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
├── CrosshairPro.Core/            # 核心层：模型、接口、枚举（无项目依赖）
│   ├── Enums/                    # AppState, CrosshairStyle
│   ├── Interfaces/               # IConfigRepository+IAppStateRepository+IPresetRepository, ICrosshairRenderer, IHotkeyManager, IGameDetector（预留）
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
    └── Views/                    # OverlayWindow, CrosshairPage, GamesPage, GamePages/(ApexPage, Cs2Page, GamePageSelector)
```

（App 根目录另有 App.xaml(.cs)、MainWindow.xaml(.cs) 和 4 个值转换器）

## 模块索引

| 层级 | 模块 | 描述 | 文件数 | 文档 |
|------|------|------|--------|------|
| 基础层 | Core | 核心模型、接口、枚举定义 | 18 | [README](Core/README.md) |
| 基础层 | Infrastructure | Win32 API 封装、热键管理 | 3 | [README](Infrastructure/README.md) |
| 数据层 | Services | 配置持久化、准心渲染实现 | 3 | [README](Services/README.md) |
| 应用层 | Application | 业务服务抽象、依赖注入 | 9 | [README](Application/README.md) |
| 表现层 | App | WPF 应用入口、UI 层 | 41 | [README](App/README.md) |
| 配置层 | GameConfig | 游戏配置协议文档（无独立源码目录） | 16 | [README](GameConfig/README.md) |

文件数口径：代码模块 = `find src/CrosshairPro.{模块} -type f ( -name "*.cs" -o -name "*.xaml" ) ! -path "*/obj/*" ! -path "*/bin/*" | wc -l`；GameConfig = 配置文档 .md 文件数。

## 核心功能

### 1. 准心样式与效果
- **十字/点/圆/T形/X形/自定义图片** 6 种样式，可调大小、间隙、厚度、旋转
- **描边/阴影/发光** 效果系统，颜色与强度可调
- 全屏透明置顶叠加窗口（OverlayWindow），鼠标穿透不影响游戏操作

### 2. 热键控制
- 全局热键：切换准心显示/隐藏、切换预设等（HotkeyManager + Win32 RegisterHotKey）
- 热键绑定持久化于 config.json

### 3. 预设管理
- 创建、编辑、删除预设；预设关联游戏进程，游戏运行时自动切换
- 预设导入/导出；默认预设保护（IsDefault 不可删除/覆盖）

### 4. 游戏配置管理
- **Apex Legends 专用通道**：直接读写游戏真实配置（videoconfig.txt / settings.cfg），支持备份、替换、恢复与启动项生成（ApexConfigService + ApexPage）
- **通用策略通道**：内置 2 款 FPS 游戏（CS2、Apex）配置档案（GameProfile + GameConfigStrategy），配置项支持布尔/整数/枚举/字符串类型，CS2 页面基于此通道（GameConfigService + Cs2Page）
- 进程检测预留接口 IGameDetector

### 5. 本地化与 UI
- 中英文本地化（`{loc:Loc}` 标记扩展 + resx 资源），运行时切换，偏好持久化
- 标签页导航系统（CrosshairPage、GamesPage、GamePages 模板选择）
- 自定义控件库（TabNavItem、IconButton、ToastNotification、SmoothScrollViewer 等）
- 主题系统：DesignTokens 设计令牌 + ControlStyles 控件样式 + IconGeometries 图标

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
- **游戏配置**: `gameconfigs/{gameId}.json`（通用策略通道）、`launch_options.txt` 与 `apex-backups/`（Apex 专用通道），详见 [GameConfig/README.md](GameConfig/README.md)
- **日志**: Serilog 静态日志仅在 ViewModel 记录异常，未配置文件 sink（不存在 logs/ 目录）

## 重要坑点

1. **分层依赖非严格隔离**: App.csproj 直接引用全部四个项目（如 MainWindow 直接 using Infrastructure.Hotkey），并非只经 Application 接口；新增跨层调用先确认 DI 接口是否已暴露
2. **状态持久化**: `AppPersistedState` 记录当前预设 ID 与界面语言，应用启动时恢复
3. **OverlayWindow 鼠标穿透**: 使用 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW` 扩展样式
4. **WPF Shape 渲染**: 在 `AllowsTransparency=true` 窗口中必须对每个 Shape 单独设置 Opacity
5. **配置深拷贝**: `ConfigurationService.CloneConfig()` 必须递归克隆所有嵌套对象
6. **默认预设保护**: `IsDefault=true` 的预设不能删除/覆盖保存（PresetService 校验）
7. **仓库单例**: `JsonConfigRepository` 同时实现 IConfigRepository 与 IAppStateRepository，DI 注册为同一实例共享文件锁；`JsonPresetRepository` 独立文件锁
8. **CS2 配置写入时机**: 游戏运行时不得 patch video.txt/config.cfg（会冲突），仅在未运行时写入，见 [GameConfig/CS2/pitfalls.md](GameConfig/CS2/pitfalls.md)
9. **配置文件编码**: 游戏配置文件需 UTF-8 无 BOM，否则可能被游戏拒绝解析
10. **VAC 边界**: 不写入任何受 VAC 监控的命令，仅 patch 白名单字段，见 [GameConfig/CS2/pitfalls.md](GameConfig/CS2/pitfalls.md)

## 模块变更日志

每个模块的变更历史记录在 `docs/{模块}/CHANGELOG.md`（如 [Core/CHANGELOG.md](Core/CHANGELOG.md)），排查问题时优先阅读。

## 文档索引

| 模块 | README | 数据模型 | 坑点 | CHANGELOG | 其他 |
|------|--------|----------|------|-----------|------|
| Core | [README](Core/README.md) | [data-model](Core/data-model.md) | [pitfalls](Core/pitfalls.md) | [CHANGELOG](Core/CHANGELOG.md) | — |
| Infrastructure | [README](Infrastructure/README.md) | [data-model](Infrastructure/data-model.md) | [pitfalls](Infrastructure/pitfalls.md) | [CHANGELOG](Infrastructure/CHANGELOG.md) | — |
| Services | [README](Services/README.md) | [data-model](Services/data-model.md) | [pitfalls](Services/pitfalls.md) | [CHANGELOG](Services/CHANGELOG.md) | — |
| Application | [README](Application/README.md) | [data-model](Application/data-model.md) | [pitfalls](Application/pitfalls.md) | [CHANGELOG](Application/CHANGELOG.md) | [api-configuration](Application/api-configuration.md) · [api-gameconfig](Application/api-gameconfig.md) · [api-preset](Application/api-preset.md) |
| App | [README](App/README.md) | [data-model](App/data-model.md) | [pitfalls](App/pitfalls.md) | [CHANGELOG](App/CHANGELOG.md) | — |
| GameConfig | [README](GameConfig/README.md) | — | — | — | [Apex/](GameConfig/Apex/README.md) · [CS2/](GameConfig/CS2/README.md) |
| design | — | — | — | — | [tab-navigation-design](../.claude/docs/design/tab-navigation-design.md) |

完整逐文件索引见 [INDEX.md](INDEX.md)。设计文档 tab-navigation-design.md 权威位置为 `.claude/docs/design/`。
