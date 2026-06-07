# CrosshairPro 文档中心

欢迎来到 CrosshairPro 项目文档中心。本文档按照分类目录组织，涵盖架构设计、模块说明、开发指南等内容。

## 目录结构

```
docs/
├── README.md                    # 文档导航首页（本文件）
├── architecture/                # 架构设计文档
│   ├── overview.md              # 系统架构概览
│   └── decisions.md             # 架构决策记录（ADR）
├── modules/                     # 模块详细文档
│   ├── core.md                  # Core 核心层（Models, Enums, Interfaces）
│   ├── infrastructure.md        # Infrastructure 基础设施层（Win32, Hotkey）
│   ├── services.md              # Services 服务层（Renderer, Config, Preset）
│   └── app.md                   # App 应用层（Views, ViewModels, Controls）
├── guides/                      # 开发指南
│   └── getting-started.md       # 快速入门指南
├── PRD.md                       # 产品需求文档
└── prototype-design.md          # UI/UX 原型设计文档
```

## 快速导航

### 按角色分类

| 角色 | 推荐阅读顺序 |
|------|-------------|
| **新开发者** | [快速入门](guides/getting-started.md) → [架构概览](architecture/overview.md) → [Core 层](modules/core.md) |
| **功能开发** | [架构概览](architecture/overview.md) → [Services 层](modules/services.md) → [App 层](modules/app.md) |
| **架构评审** | [架构概览](architecture/overview.md) → [架构决策](architecture/decisions.md) → [PRD](PRD.md) |
| **UI/UX 开发** | [原型设计](prototype-design.md) → [App 层](modules/app.md) → [Core 层](modules/core.md) |

### 按功能分类

- **准星渲染**: [Services - CrosshairRenderer](modules/services.md#crosshairrenderer) → [Core - CrosshairConfig](modules/core.md#crosshairconfig)
- **热键系统**: [Infrastructure - HotkeyManager](modules/infrastructure.md#hotkeymanager) → [Core - HotkeyBinding](modules/core.md#hotkeybinding)
- **配置管理**: [Services - JsonConfigRepository](modules/services.md#jsonconfigrepository) → [Core - IConfigRepository](modules/core.md#iconfigrepository)
- **预设系统**: [Services - JsonPresetRepository](modules/services.md#jsonpresetrepository) → [Core - Preset](modules/core.md#preset)
- **游戏检测**: [Core - IGameDetector](modules/core.md#igamedetector) → [Core - GameProfile](modules/core.md#gameprofile)

## 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0 | 运行时框架 |
| C# | 12 | 编程语言 |
| WPF | - | UI 框架 |
| CommunityToolkit.Mvvm | 8.3.2 | MVVM 框架 |
| System.Text.Json | 8.0.5 | JSON 序列化 |
| Serilog | 4.1.0 | 日志框架 |
| Hardcodet.NotifyIcon.Wpf | 1.1.0 | 系统托盘图标 |

## 项目状态

### 已实现功能

- ✅ 6 种准星样式渲染（十字、圆点、圆形、T 形、X 形、自定义图片）
- ✅ 实时参数调节（大小、间距、粗细、透明度、中心点大小）
- ✅ 8 种预设颜色选择
- ✅ 描边和阴影效果
- ✅ 全局热键切换准星显示（Ctrl+Shift+X）
- ✅ 热键调整准星大小
- ✅ 主窗口实时预览
- ✅ JSON 配置/预设持久化
- ✅ Win32 热键基础设施

### 待实现功能

- ⬜ 依赖注入容器配置
- ⬜ Serilog 日志初始化
- ⬜ 系统托盘图标
- ⬜ 游戏检测服务
- ⬜ 配置自动保存/启动加载
- ⬜ 预设管理 UI
- ⬜ 导入/导出功能
- ⬜ 发光效果渲染
- ⬜ 多显示器支持
- ⬜ 自定义图片样式在 OverlayWindow 中的实现
