# Changelog - Infrastructure

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
| HotkeyManager.cs | 新增 | 热键管理器实现 |
| Win32Constants.cs | 新增 | Win32 常量定义 |
| Win32Methods.cs | 新增 | Win32 API P/Invoke |

### 影响范围
- **基础功能**: 热键注册、Win32 消息窗口创建