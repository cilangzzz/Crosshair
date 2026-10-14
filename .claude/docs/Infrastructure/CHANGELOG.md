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
| CrosshairPro.Infrastructure.csproj | +16/-0 | 项目文件：net8.0、AllowUnsafeBlocks=true、项目引用 Core |
| Hotkey/HotkeyManager.cs | +247/-0 | 热键管理器实现：WinHotkeyProvider 封装 RegisterHotKey/UnregisterHotKey，创建隐藏消息窗口并在 WndProc 中处理 WM_HOTKEY |
| Win32/Win32Constants.cs | 未跟踪 | 文件存在于工作区（79 行）但从未提交——被根 .gitignore 第 26 行 `[Ww][Ii][Nn]32/` 规则误伤，不在 e649a4f 提交中 |
| Win32/Win32Methods.cs | 未跟踪 | 同上（297 行）：P/Invoke 封装 RegisterHotKey/RegisterClass/CreateWindowEx 等；HotkeyManager 直接引用这两个类型，全新 clone 后模块无法编译 |

### 影响范围
- **基础功能**: 热键注册、Win32 消息窗口创建
- **版本控制警告**: Win32/ 目录下两个源文件不在版本控制中，revert 或重新克隆会静默丢失它们；修复方式为在 .gitignore 追加 `!src/CrosshairPro.Infrastructure/Win32/`（或将目录改名避开该规则）后补交
- **合并说明**: 同日的 31ecd78（Merge remote main: keep comprehensive .gitignore）将本提交内容并入主线，对模块无新增改动