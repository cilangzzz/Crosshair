# Changelog - Services

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-06-07] Initial commit

**类型**: feat
**提交**: e649a4f
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| JsonConfigRepository.cs | 新增 | JSON 配置仓库实现 |
| JsonPresetRepository.cs | 新增 | JSON 预设仓库实现 |
| CrosshairRenderer.cs | 新增 | 准心渲染器实现 |

### 影响范围
- **配置持久化**: 配置和预设的 JSON 存储
- **渲染服务**: 6 种准心样式的渲染逻辑