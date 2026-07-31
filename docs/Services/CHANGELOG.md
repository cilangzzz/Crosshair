# CrosshairPro.Services 变更日志

本文档记录 Services 模块的所有重要变更。

格式遵循 [Keep a Changelog](https://keepachangelog.com/) 规范。

## [Unreleased]

### 新增
- 模块文档：README.md, data-model.md, pitfalls.md

---

## [1.0.0] - 2024-01-15

### 新增
- **JsonConfigRepository**: 配置仓库实现
  - 实现 `IConfigRepository` 接口
  - 实现 `IAppStateRepository` 接口
  - 支持 JSON 文件持久化
  - 配置导入/导出功能
  - 使用 `SemaphoreSlim` 进行文件锁控制

- **JsonPresetRepository**: 预设仓库实现
  - 实现 `IPresetRepository` 接口
  - 预设 CRUD 操作
  - 预设导入/导出功能
  - 自动创建预设目录

- **CrosshairRenderer**: 准心渲染器实现
  - 实现 `ICrosshairRenderer` 接口
  - 支持 6 种准心样式：十字、点、圆、T形、X形、自定义图片
  - 效果系统：描边、阴影
  - 渲染资源缓存（Pen/Brush/Geometry）
  - 旋转变换支持
  - 位置偏移支持

### 技术细节
- 使用 `System.Text.Json` 进行序列化
- 序列化配置：WriteIndented, CamelCase, IgnoreNullValues
- 文件路径：`%APPDATA%/CrosshairPro/`
- WPF 渲染资源使用 `Freeze()` 确保线程安全

---

## 开发历史

### 2024-01 初期设计

**架构决策**:
- Services 层作为数据访问层，只依赖 Core 接口
- 使用 JSON 文件而非数据库，简化部署和备份
- 文件锁确保同一仓库内的读写互斥

**渲染器设计**:
- 直接使用 WPF `DrawingContext` 渲染
- 缓存策略：基于颜色+参数的字符串键
- 效果绘制顺序：阴影 -> 描边 -> 主体

**配置结构**:
- 主配置和状态分离存储
- 预设独立目录管理
- 支持导入导出便于备份和分享

---

## 计划中的改进

### 性能优化
- [ ] 添加渲染缓存清理机制
- [ ] 预设加载延迟初始化
- [ ] 大图片自动压缩/缩放

### 功能增强
- [ ] 配置迁移工具（版本升级时）
- [ ] 预设云同步接口
- [ ] 配置版本控制

### 可靠性
- [ ] 配置文件损坏自动备份
- [ ] 导入文件格式验证
- [ ] 渲染错误恢复机制

---

## 版本兼容性

| 模块版本 | Core 版本 | Application 版本 | .NET 版本 |
|----------|-----------|------------------|-----------|
| 1.0.0    | 1.0.0     | 1.0.0            | 8.0       |

---

## 贡献指南

### 新增功能
1. 在 Core 层定义接口
2. 在 Services 层实现接口
3. 更新本 CHANGELOG
4. 更新 README.md

### 修改现有功能
1. 确保接口兼容性
2. 更新测试用例
3. 记录破坏性变更

### 文件格式变更
1. 提供迁移逻辑
2. 保留向后兼容
3. 更新 data-model.md