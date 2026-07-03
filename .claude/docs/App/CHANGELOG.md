# Changelog - App

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-06-09] feat: improve overlay window stability

**类型**: feat
**提交**: c33a4a9
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml | +4/-2 | 布局微调 |
| MainWindow.xaml.cs | +1 | 代码清理 |
| OverlayWindow.cs | +28/-1 | 增强稳定性逻辑 |

### 影响范围
- **Overlay 窗口**: 改进准心渲染稳定性
- **UI**: 主窗口布局小调整

---

## [2026-06-08] feat: expand UI controls and overlay features

**类型**: feat
**提交**: c0f944b
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml | +66 | 扩展主窗口布局 |
| MainWindow.xaml.cs | +338 | 新增大量功能代码 |
| ControlStyles.xaml | +97 | 新增控件样式 |
| MainViewModel.cs | +11 | 扩展 ViewModel 属性 |
| OverlayWindow.cs | +56 | 增强 Overlay 功能 |

### 影响范围
- **UI 控件**: 主窗口新增控制面板
- **样式系统**: 新增控件样式定义
- **ViewModel**: 新增属性和命令
- **Overlay**: 改进准心渲染

---

## [2026-06-08] feat: major UI and ViewModel refactoring

**类型**: refactor
**提交**: 8d57814
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml | +254/-254 | 重构布局 |
| MainWindow.xaml.cs | +422 | 大量新增功能 |
| ControlStyles.xaml | +171/-171 | 重构样式 |
| MainViewModel.cs | +277/-277 | 重构 ViewModel |

### 影响范围
- **UI 层**: 主窗口完全重构
- **ViewModel**: 架构重新设计
- **样式系统**: 样式重新组织

### 回滚指南
- 回滚: `git revert 8d57814`
- 检查文件: MainWindow.xaml, MainWindow.xaml.cs, MainViewModel.cs
- 副作用: 预设管理功能可能受影响

---

## [2026-06-08] feat: add app icon and refactor code

**类型**: feat
**提交**: 63ca04a
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| app-icon.ico | 新增 | 应用图标 |
| MainWindow.xaml | +1 | 引用图标 |
| MainWindow.xaml.cs | +77/-65 | 重构代码 |
| OverlayWindow.cs | +66/-66 | 重构代码 |

### 影响范围
- **资源**: 新增应用图标
- **代码质量**: 代码重构

---

## [2026-06-08] feat: enhance overlay and crosshair config

**类型**: feat
**提交**: 64f03ed
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml | +17 | 新增 UI 元素 |
| MainWindow.xaml.cs | +19/-2 | 新增功能 |
| OverlayWindow.cs | +20/-2 | 增强 Overlay |
| CrosshairConfig.cs | +1 | 新增属性 |

### 影响范围
- **UI**: 主窗口新增控制项
- **Overlay**: 改进准心渲染
- **数据模型**: 配置扩展