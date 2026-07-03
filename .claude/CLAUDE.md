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
│   ├── Interfaces/             # IConfigRepository, ICrosshairRenderer, IHotkeyManager
│   └── Models/                 # CrosshairConfig, EffectsConfig, Preset, GameProfile
├── CrosshairPro.Infrastructure/ # 基础层：Win32 API、热键
│   ├── Hotkey/                 # HotkeyManager, WinHotkeyProvider
│   └── Win32/                  # Win32Constants, Win32Methods (P/Invoke)
├── CrosshairPro.Services/      # 业务层：配置、渲染
│   ├── Configuration/          # JsonConfigRepository, JsonPresetRepository
│   └── Crosshair/              # CrosshairRenderer
└── CrosshairPro.App/           # 应用层：WPF UI
    ├── Controls/               # CrosshairPreview
    ├── Themes/                 # ControlStyles.xaml, DesignTokens.xaml
    ├── ViewModels/             # MainViewModel
    └── Views/                  # OverlayWindow
```

## 模块索引

| 层级 | 模块 | 描述 | 文件数 | 文档 |
|------|------|------|--------|------|
| 基础层 | Core | 核心模型、接口、枚举定义 | 12 | [README](docs/Core/README.md) |
| 基础层 | Infrastructure | Win32 API封装、热键管理 | 3 | [README](docs/Infrastructure/README.md) |
| 业务层 | Services | 配置持久化、准心渲染服务 | 3 | [README](docs/Services/README.md) |
| 打包层 | App | WPF应用入口、UI层 | 10 | [README](docs/App/README.md) |

## 核心架构

```
┌─────────────────────────────────────────────────────┐
│                    App (WPF UI)                      │
│  MainWindow ←→ MainViewModel ←→ OverlayWindow       │
└───────────────────────┬─────────────────────────────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  Services   │ │Infrastructure│ │    Core     │
│ - Renderer  │ │ - HotkeyMgr │ │ - Models    │
│ - ConfigRepo│ │ - Win32 API │ │ - Interfaces│
└─────────────┘ └─────────────┘ └─────────────┘
```

**数据流**:
1. `MainViewModel` 持有 `CrosshairConfig`，用户修改触发 `ConfigUpdated` 事件
2. `MainWindow` 监听事件，调用 `OverlayWindow.UpdateConfig()`
3. `OverlayWindow` 使用 WPF Shape 元素渲染准心
4. 配置通过 `JsonConfigRepository` 持久化到 `%APPDATA%/CrosshairPro/`

## 模块依赖关系

```
App → Core, Services, Infrastructure
Services → Core, Infrastructure
Infrastructure → Core
Core → (无依赖)
```

## 环境配置

- **配置目录**: `%APPDATA%/CrosshairPro/`
- **配置文件**: `config.json` (当前配置), `presets.json` (预设列表)
- **日志文件**: `%APPDATA%/CrosshairPro/logs/`

## 重要坑点

1. **OverlayWindow 鼠标穿透**: 使用 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW` 扩展样式，确保不影响游戏光标
2. **WPF Shape 渲染**: 在 `AllowsTransparency=true` 窗口中，必须对每个 Shape 单独设置 Opacity，不能设置 Canvas.Opacity
3. **热键注册**: 需要创建消息窗口接收 `WM_HOTKEY` 消息，委托必须保持引用防止 GC 回收
4. **配置深拷贝**: `CrosshairConfig.Clone()` 必须递归克隆所有嵌套对象（Effects、Display）
5. **亮度调整**: `ApplyBrightness()` 方法中 factor > 1 时不会超过 255（已 Math.Min 限制）
6. **自定义图片**: 加载失败时静默回退到十字准心，不会抛出异常
7. **预设管理**: 默认预设（IsDefault=true）不能被删除

## 模块变更日志

每个模块的变更历史记录在 `docs/{模块}/CHANGELOG.md`，排查问题时优先阅读。

## 文档索引

| 模块 | README | API文档 | 数据模型 | 坑点 | CHANGELOG |
|------|--------|---------|----------|------|-----------|
| Core | [README](docs/Core/README.md) | - | [data-model](docs/Core/data-model.md) | [pitfalls](docs/Core/pitfalls.md) | [CHANGELOG](docs/Core/CHANGELOG.md) |
| Infrastructure | [README](docs/Infrastructure/README.md) | - | [data-model](docs/Infrastructure/data-model.md) | [pitfalls](docs/Infrastructure/pitfalls.md) | [CHANGELOG](docs/Infrastructure/CHANGELOG.md) |
| Services | [README](docs/Services/README.md) | - | [data-model](docs/Services/data-model.md) | [pitfalls](docs/Services/pitfalls.md) | [CHANGELOG](docs/Services/CHANGELOG.md) |
| App | [README](docs/App/README.md) | - | [data-model](docs/App/data-model.md) | [pitfalls](docs/App/pitfalls.md) | [CHANGELOG](docs/App/CHANGELOG.md) |
