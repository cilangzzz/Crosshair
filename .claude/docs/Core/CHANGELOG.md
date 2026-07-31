# Changelog - Core

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

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