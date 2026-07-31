# Changelog - Core

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-07-31] feat: add game config models

**类型**: feat
**提交**: N/A (pending)
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| GameConfig.cs | +20 | 新增游戏配置数据模型 |
| GameConfigStrategy.cs | +91 | 新增游戏配置策略定义 |
| ConfigItemType.cs | (embedded) | 配置项类型枚举（Bool/Int/Enum/String） |

### 新增模型

**GameConfig**：
- 存储单个游戏的配置数据
- 包含 GameId、LaunchOptions、Settings 字典

**GameConfigStrategy**：
- 定义每个游戏支持的配置项和操作方式
- 包含 ConfigItemType 枚举、ConfigItemDefinition、ConfigSectionDefinition

### 影响范围
- **数据模型**: 新增游戏配置相关模型，用于游戏特定配置管理
- **跨模块**: Application 层可使用 GameConfigStrategy 构建 UI，使用 GameConfig 存储配置

---

## [2026-07-03] refactor: restructure project architecture

**类型**: refactor
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| IConfigRepository.cs | +16 | 新增 IAppStateRepository 接口定义 |
| AppPersistedState.cs | +13 | 新增应用持久化状态模型 |

### 影响范围
- **接口**: 新增 `IAppStateRepository` 用于管理应用状态
- **数据模型**: 新增 `AppPersistedState` 记录当前预设ID
- **跨模块**: Services 层 JsonConfigRepository 需实现新接口

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: IConfigRepository.cs, AppPersistedState.cs
- 副作用: Application 层服务依赖新接口，回滚需同步修改

---

## [2026-06-08] feat: major UI and ViewModel refactoring

**类型**: feat
**提交**: 8d57814
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Preset.cs | +2 | 新增 IsDefault 属性标记默认预设 |

### 影响范围
- **数据模型**: Preset 新增 IsDefault 字段，影响预设列表显示和删除逻辑
- **跨模块**: App 模块的 MainViewModel 需要处理 IsDefault 预设不可删除

---

## [2026-06-08] feat: enhance overlay and crosshair config

**类型**: feat
**提交**: 64f03ed
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| CrosshairConfig.cs | +1 | 新增配置属性 |

### 影响范围
- **数据模型**: CrosshairConfig 扩展，影响所有使用配置的模块