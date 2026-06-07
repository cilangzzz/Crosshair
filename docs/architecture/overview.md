# 系统架构概览

## 架构概述

CrosshairPro 采用**四层架构**设计，遵循依赖倒置原则，确保各层职责清晰、耦合度低。

```
┌─────────────────────────────────────────────────────────────┐
│                    CrosshairPro.App                         │
│  (WPF 应用层 - MainWindow, OverlayWindow, ViewModel)        │
├─────────────────────────────────────────────────────────────┤
│                 CrosshairPro.Services                       │
│  (业务服务层 - Renderer, ConfigRepository, PresetRepository)│
├─────────────────────────────────────────────────────────────┤
│              CrosshairPro.Infrastructure                    │
│  (基础设施层 - Win32 P/Invoke, HotkeyManager)                │
├─────────────────────────────────────────────────────────────┤
│                   CrosshairPro.Core                         │
│  (核心层 - Models, Enums, Interfaces)                       │
└─────────────────────────────────────────────────────────────┘
```

## 依赖关系

```
CrosshairPro.App
  ├── CrosshairPro.Services
  │     └── CrosshairPro.Core
  ├── CrosshairPro.Infrastructure
  │     └── CrosshairPro.Core
  └── CrosshairPro.Core
```

**依赖规则**：
- 上层可以依赖下层
- 下层不能依赖上层
- 同层之间尽量避免依赖
- Core 层是最底层，不依赖任何其他项目

## 各层职责

### Core 层（CrosshairPro.Core）

**职责**：定义业务模型、枚举和接口契约

**包含内容**：
- **Models**: 业务数据模型（CrosshairConfig, Preset, GameProfile, HotkeyBinding, GameInfo）
- **Enums**: 业务枚举（CrosshairStyle, AppState）
- **Interfaces**: 服务接口契约（IConfigRepository, IPresetRepository, ICrosshairRenderer, IGameDetector, IHotkeyManager）

**依赖**：CommunityToolkit.Mvvm（用于 ObservableObject 基类）

### Infrastructure 层（CrosshairPro.Infrastructure）

**职责**：封装平台特定的底层功能

**包含内容**：
- **Win32**: Win32 API 常量定义和 P/Invoke 声明
- **Hotkey**: 基于 Win32 RegisterHotKey 的热键管理器
- **IO**: 文件 I/O 工具（待实现）

**依赖**：CrosshairPro.Core

### Services 层（CrosshairPro.Services）

**职责**：实现核心业务逻辑

**包含内容**：
- **Crosshair**: 准星渲染引擎（基于 WPF DrawingContext）
- **Configuration**: JSON 配置和预设仓库实现
- **GameDetection**: 游戏检测服务（待实现）
- **Hotkey**: 热键服务（待实现，当前在 Infrastructure 层）

**依赖**：CrosshairPro.Core, CrosshairPro.Infrastructure

### App 层（CrosshairPro.App）

**职责**：WPF 应用程序入口和用户界面

**包含内容**：
- **Views**: 窗口和页面（MainWindow, OverlayWindow）
- **ViewModels**: MVVM 视图模型（MainViewModel）
- **Controls**: 自定义控件（CrosshairPreview）
- **Converters**: 值转换器（待实现）
- **Themes**: 主题资源（待实现）
- **Assets**: 静态资源（待实现）

**依赖**：CrosshairPro.Services, CrosshairPro.Infrastructure, CrosshairPro.Core

## 数据流

### 准星渲染流程

```
用户操作 UI
    ↓
MainViewModel.Config 属性变更
    ↓
MainWindow.ConfigUpdated 事件
    ↓
├── OverlayWindow.UpdateConfig()  → 更新覆盖窗口配置
│       ↓
│   OverlayWindow.RenderCrosshair()  → 在覆盖窗口渲染准星
│
└── MainWindow.DrawPreview()  → 在主窗口预览区渲染准星
```

### 热键处理流程

```
用户按下热键（如 Ctrl+Shift+X）
    ↓
WinHotkeyProvider.WndProc() 接收 WM_HOTKEY 消息
    ↓
HotkeyManager.HotkeyTriggered 事件
    ↓
MainWindow.OnHotkeyTriggered() 处理
    ↓
├── ToggleCrosshair  → 切换准星显示/隐藏
├── IncreaseSize     → 增大准星尺寸
└── DecreaseSize     → 减小准星尺寸
```

### 配置持久化流程

```
用户修改配置
    ↓
MainViewModel.Config 属性变更
    ↓
JsonConfigRepository.SaveConfigAsync()
    ↓
序列化为 JSON
    ↓
写入 %APPDATA%/CrosshairPro/config.json
```

## 线程模型

- **UI 线程**：所有 WPF 控件操作必须在 UI 线程执行
- **热键线程**：WinHotkeyProvider 在隐藏窗口的消息循环中处理热键消息
- **渲染线程**：准星渲染在 UI 线程的 DrawingContext 中执行

## 当前实现状态

### 已连接的组件

```
MainWindow
  ├── HotkeyManager (手动创建)
  ├── MainViewModel (手动创建)
  ├── OverlayWindow (手动创建)
  └── DrawPreview() (直接在 Canvas 上绘制)
```

### 待连接的组件

- **依赖注入**：已引用 Microsoft.Extensions.DependencyInjection 但未配置
- **日志系统**：已引用 Serilog 但未初始化
- **系统托盘**：已引用 Hardcodet.NotifyIcon.Wpf 但未使用
- **配置自动保存**：JsonConfigRepository 已实现但未从 UI 调用
- **预设管理**：按钮已存在但无命令绑定

## 扩展点

### 接口扩展

所有服务层功能通过 Core 层的接口定义，可以通过实现新接口来扩展功能：

- `ICrosshairRenderer` → 自定义渲染器
- `IConfigRepository` → 不同存储后端（如 SQLite、云端）
- `IPresetRepository` → 不同预设存储方式
- `IGameDetector` → 不同游戏检测策略
- `IHotkeyManager` → 不同热键实现

### 样式扩展

通过 `CrosshairStyle` 枚举和 `CrosshairRenderer` 的渲染方法，可以轻松添加新的准星样式。
