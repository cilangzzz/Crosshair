# CrosshairPro.Infrastructure 变更日志

所有重要的变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)。

## [Unreleased]

### 新增
- 待添加

## [1.0.0] - 2024-06-XX

### 新增
- 初始版本发布
- Win32 API 封装：
  - 窗口操作 API（SetWindowPos, GetWindowRect 等）
  - 热键操作 API（RegisterHotKey, UnregisterHotKey）
  - 消息窗口 API（CreateWindowEx, DestroyWindow）
  - 键盘操作 API（VkKeyScan, GetKeyState）
  - DWM 操作 API（DwmExtendFrameIntoClientArea）
- Win32 常量定义：
  - 窗口样式常量（WS_EX_*, WS_*）
  - 热键修饰符常量（MOD_*, WM_HOTKEY）
  - SetWindowPos 标志（SWP_*）
  - 窗口层级常量（HWND_*）
- Win32 结构体：
  - RECT, POINT, MARGINS
  - WNDCLASS
- HotkeyManager 实现：
  - 全局热键注册和注销
  - 热键触发事件通知
  - 组合键解析（Ctrl+Shift+Key）
  - 自动 ID 分配
- 辅助方法：
  - SetWindowTransparentClickable: 设置窗口透明点击穿透
  - SetWindowInteractive: 设置窗口可交互
  - GetVirtualKeyCode: 虚拟键码映射

### 技术细节
- 使用 P/Invoke 调用 Win32 API
- 支持 `unsafe` 代码块（项目配置）
- 实现 `IDisposable` 接口管理资源
- 使用 `SetLastError = true` 支持 Win32 错误码获取
- 支持特殊键和组合键映射

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