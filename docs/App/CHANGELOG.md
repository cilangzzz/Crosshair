# Changelog - App

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-07-31] feat: add page navigation system and new controls

**类型**: feat
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/CrosshairPage.xaml.cs | +340 | 新增准心配置页面 |
| Views/GamesPage.xaml.cs | +24 | 新增游戏配置页面 |
| ViewModels/CrosshairViewModel.cs | +337 | 新增准心配置视图模型 |
| ViewModels/GamesViewModel.cs | +129 | 新增游戏配置视图模型 |
| ViewModels/MainViewModel.cs | 重构 | 改为页面导航架构 |
| Controls/TabNavItem.cs | +142 | 新增标签页导航项控件 |
| Controls/PageTemplateSelector.cs | +33 | 新增页面模板选择器 |
| Controls/IconButton.cs | +205 | 新增图标按钮控件 |
| Helpers/ThemeHelper.cs | +146 | 新增主题资源访问助手 |
| StringToVisibilityConverter.cs | +27 | 新增字符串到可见性转换器 |
| Themes/IconGeometries.xaml | +147 | 新增图标几何资源 |

### 影响范围
- **架构**: 从单 ViewModel 改为页面导航架构
- **页面**: 新增 CrosshairPage、GamesPage 两个页面
- **控件**: 新增 TabNavItem、PageTemplateSelector、IconButton 等控件
- **资源**: 新增 IconGeometries.xaml 图标资源文件

### 新增功能
- `CrosshairPage`: 准心配置页面，左侧预览 + 右侧控制面板
- `GamesPage`: 游戏配置页面，管理游戏特定配置
- `CrosshairViewModel`: 准心配置逻辑，从 MainViewModel 拆分
- `GamesViewModel`: 游戏配置逻辑，支持多游戏配置
- `TabNavItem`: 左侧导航图标项，支持选中态
- `PageTemplateSelector`: 根据 PageType 选择页面模板
- `StringToVisibilityConverter`: 字符串到可见性转换
- `IconGeometries.xaml`: Material Design 风格图标集合

### 架构变更
- `MainViewModel` 改为持有子 ViewModel，不再直接管理配置
- 新增 `PageType` 枚举（Crosshair、Games）
- 页面渲染逻辑从 MainWindow 移到 CrosshairPage

---

## [2026-07-31] feat: add custom controls and theme system

**类型**: feat
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Controls/DialogBase.cs | +257 | 新增对话框基类 |
| Controls/IconButton.cs | +205 | 新增图标按钮控件 |
| Controls/ToastManager.cs | +238 | 新增 Toast 管理器（独立悬浮窗口） |
| Controls/ToastNotification.cs | +115 | 新增 Toast 通知控件 |
| Controls/CrosshairPreview.cs | +212 | 新增准心预览控件 |
| Helpers/ThemeHelper.cs | +146 | 新增主题资源访问助手 |
| Themes/DesignTokens.xaml | +143 | 新增设计令牌（颜色、字体、间距） |
| Themes/ControlStyles.xaml | +1447 | 新增控件样式定义 |
| Themes/IconGeometries.xaml | + | 新增图标几何数据 |

### 影响范围
- **控件**: 新增多个自定义控件，统一 UI 风格
- **主题**: 引入设计令牌系统，支持主题一致性
- **样式**: 所有控件使用动画过渡，提升用户体验

### 新增功能
- `DialogBase`: 统一对话框样式，支持输入对话框
- `IconButton`: 支持内置图标和自定义 Path Geometry
- `ToastManager`: 独立悬浮窗口显示 Toast，不影响布局
- `ToastNotification`: 自动消失的提示控件
- `CrosshairPreview`: 可复用的准心预览控件
- `ThemeHelper`: 类型安全的主题资源访问

---

## [2026-07-03] refactor: restructure project architecture

**类型**: refactor
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | 重构 | 简化代码，使用事件驱动 |
| MainWindow.xaml | 重构 | 优化布局结构 |
| ViewModels/MainViewModel.cs | 重构 | 异步初始化，事件驱动 |
| Views/OverlayWindow.cs | 重构 | 简化渲染逻辑 |

### 影响范围
- **架构**: App 层只依赖 Application 接口，不直接依赖 Services
- **初始化**: ViewModel 异步加载预设，恢复上次状态
- **事件**: 使用事件驱动模式，解耦 MainWindow 和 ViewModel

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: MainWindow.xaml.cs, MainViewModel.cs
- 副作用: 依赖新的 Application 层接口

---

## [2026-06-08] feat: major UI and ViewModel refactoring

**类型**: feat
**提交**: 8d57814
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | +1127 | 主窗口重构，添加自定义对话框 |
| MainWindow.xaml | +267 | 优化布局，添加控制面板 |
| MainViewModel.cs | +333 | ViewModel 重构，添加命令和事件 |

### 影响范围
- **UI**: 自定义颜色选择器、预设管理对话框
- **命令**: 添加所有操作命令（保存、导入、导出、删除）
- **托盘**: 系统托盘菜单优化

---

## [2026-06-08] feat: enhance overlay and crosshair config

**类型**: feat
**提交**: 64f03ed
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| OverlayWindow.cs | +316 | 使用 WPF Shape 渲染准心 |
| MainWindow.xaml.cs | +520 | 预览渲染逻辑 |

### 影响范围
- **渲染**: 从 GDI+ 切换到 WPF Shape，更稳定
- **预览**: 主窗口实时预览准心
- **效果**: 支持描边、阴影效果

---

## [2026-06-01] feat: initial WPF application

**类型**: feat
**提交**: c33a4a9
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| App.xaml/cs | +33 | 应用程序入口 |
| MainWindow.xaml/cs | + | 主窗口 |
| OverlayWindow.cs | + | 准心叠加窗口 |
| MainViewModel.cs | + | 主视图模型 |

### 影响范围
- **初始化**: 项目初始化，WPF MVVM 架构
- **依赖注入**: 使用 DI 容器管理服务
- **热键**: 注册全局热键 Ctrl+Shift+X
