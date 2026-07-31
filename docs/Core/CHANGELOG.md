# CrosshairPro.Core 变更日志

所有重要的变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)。

## [Unreleased]

### 新增
- 待添加

## [1.0.0] - 2024-06-XX

### 新增
- 初始版本发布
- 核心枚举定义：`AppState`, `CrosshairStyle`
- 核心接口定义：
  - `IConfigRepository`: 配置仓库接口
  - `IAppStateRepository`: 应用状态仓库接口
  - `IPresetRepository`: 预设仓库接口
  - `ICrosshairRenderer`: 准心渲染器接口
  - `IHotkeyManager`: 热键管理器接口
  - `IGameDetector`: 游戏检测器接口
- 核心模型定义：
  - `CrosshairConfig`: 准心配置
  - `EffectsConfig`: 效果配置
  - `OutlineConfig`: 描边效果配置
  - `ShadowConfig`: 阴影效果配置
  - `GlowConfig`: 发光效果配置
  - `DisplayConfig`: 显示配置
  - `Preset`: 预设模型
  - `AppPersistedState`: 应用持久化状态
  - `HotkeyBinding`: 热键绑定
  - `GameInfo`: 游戏信息
  - `GameProfile`: 游戏配置文件

### 技术细节
- 使用 CommunityToolkit.Mvvm 的 `ObservableObject` 作为模型基类
- 使用 `[ObservableProperty]` 特性简化属性定义
- 支持深拷贝（`Clone()` 方法）
- 支持配置复制（`CopyFrom()` 方法）

---

## 版本说明

### 版本号规则
- **主版本号**: 重大架构变更或不兼容的 API 修改
- **次版本号**: 新增功能，保持向后兼容
- **修订号**: Bug 修复和小改进

### 变更类型
- `新增`: 新增功能
- `变更`: 现有功能的变更
- `弃用`: 即将移除的功能
- `移除`: 已移除的功能
- `修复`: Bug 修复
- `安全`: 安全相关的修复