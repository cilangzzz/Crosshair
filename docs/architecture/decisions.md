# 架构决策记录（ADR）

本文档记录项目中的重要架构决策，包括决策背景、考虑的方案和最终选择。

> 原始详细文档请参考：[ADR.md](../ADR.md)

## 决策索引

| 编号 | 决策 | 状态 | 日期 |
|------|------|------|------|
| ADR-001 | 选择 WPF + .NET 8 技术栈 | 已采纳 | 2026-06 |
| ADR-002 | 使用 WPF Layered Window 实现覆盖窗口 | 已采纳 | 2026-06 |
| ADR-003 | 选择 JSON 文件存储配置 | 已采纳 | 2026-06 |
| ADR-004 | 使用 Win32 RegisterHotKey 实现热键 | 已采纳 | 2026-06 |

---

## ADR-001: 选择 WPF + .NET 8 技术栈

### 背景

需要选择一个桌面应用框架来实现准星覆盖工具。

### 考虑的方案

| 方案 | 优点 | 缺点 |
|------|------|------|
| **WPF + .NET 8** | 原生 Windows 支持、成熟的 UI 框架、良好的性能 | 仅限 Windows |
| Electron | 跨平台、Web 技术栈 | 内存占用大、性能差 |
| Qt | 跨平台、性能好 | 学习曲线陡峭、许可证复杂 |
| WinForms | 简单易用 | UI 能力有限、不支持透明窗口 |
| Flutter | 跨平台、现代 UI | Windows 支持不成熟 |

### 决策

选择 **WPF + .NET 8**。

### 理由

1. 目标平台仅为 Windows
2. WPF 对透明窗口、覆盖层有良好支持
3. .NET 8 提供最新性能优化和语言特性
4. 社区成熟，文档丰富

---

## ADR-002: 使用 WPF Layered Window 实现覆盖窗口

### 背景

需要在游戏上方显示准星覆盖层，要求透明、置顶、点击穿透。

### 考虑的方案

| 方案 | 优点 | 缺点 |
|------|------|------|
| **WPF Layered Window** | 原生支持、简单实现 | 性能一般 |
| Direct3D 注入 | 高性能 | 可能触发反作弊、复杂 |
| OBS 覆盖 | 现成方案 | 需要额外软件 |
| Windows Composition API | 现代 API | 兼容性问题 |
| Overwolf | 游戏覆盖平台 | 限制多、需要审核 |

### 决策

选择 **WPF Layered Window**。

### 理由

1. 使用 `WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST` 窗口样式
2. 实现简单，无需注入游戏进程
3. 不会触发反作弊系统
4. 通过 `SetLayeredWindowAttributes` 实现透明

---

## ADR-003: 选择 JSON 文件存储配置

### 背景

需要持久化用户配置和预设数据。

### 考虑的方案

| 方案 | 优点 | 缺点 |
|------|------|------|
| **JSON 文件** | 简单、人类可读、无需额外依赖 | 并发访问需处理 |
| SQLite | 结构化查询、事务支持 | 增加依赖、过度设计 |
| 注册表 | Windows 原生 | 不便携、权限问题 |
| 加密存储 | 安全性高 | 复杂度高、配置不需要加密 |

### 决策

选择 **JSON 文件存储**。

### 理由

1. 配置数据简单，无需关系型数据库
2. 人类可读，便于调试和手动编辑
3. 存储位置：`%APPDATA%/CrosshairPro/`
4. 使用 `SemaphoreSlim` 处理并发访问

---

## ADR-004: 使用 Win32 RegisterHotKey 实现热键

### 背景

需要实现全局热键，即使应用不在前台也能响应。

### 考虑的方案

| 方案 | 优点 | 缺点 |
|------|------|------|
| **Win32 RegisterHotKey** | 系统级支持、可靠 | 需要 P/Invoke |
| WPF 热键 | 简单 | 仅限应用内 |
| Raw Input | 灵活 | 复杂、需要消息循环 |
| AutoHotkey | 脚本化 | 外部依赖 |

### 决策

选择 **Win32 RegisterHotKey**。

### 理由

1. 系统级全局热键，可靠稳定
2. 通过隐藏消息窗口处理 `WM_HOTKEY` 消息
3. 支持组合键（Ctrl+Shift+X 等）
4. 无需外部依赖

### 实现细节

```
WinHotkeyProvider
  ├── 创建隐藏窗口 "CrosshairProHotkeyWindow"
  ├── RegisterHotKey() 注册热键
  └── WndProc() 处理 WM_HOTKEY 消息

HotkeyManager
  ├── 解析组合键字符串（如 "Ctrl+Shift+X"）
  ├── 调用 WinHotkeyProvider 注册
  └── 触发 HotkeyTriggered 事件
```
