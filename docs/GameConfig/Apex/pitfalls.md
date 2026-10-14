# Apex 配置坑点

以下坑点均可从当前源码直接验证，涉及文件：

- `src/CrosshairPro.Application/Services/ApexConfigService.cs`
- `src/CrosshairPro.Core/Models/ApexLaunchOptions.cs`
- `src/CrosshairPro.App/ViewModels/ApexConfigViewModel.cs`

---

## 1. 保存会整体重写文件，未建模内容全部丢失

**现象**：`SaveVideoConfigAsync` / `SaveSettingsConfigAsync` 用模型序列化结果**整体覆盖**游戏配置文件，而模型只覆盖一小部分键：

- videoconfig.txt：仅写 23 个键（模型有 50+ 字段，`last_display_*`、`stream_memory`、`m_f*` 系列等都不输出）
- settings.cfg：仅写 17 行，**不含任何 `bind_*` 键位绑定**

**后果**：加载真实配置 → 点保存，文件中所有未建模字段与键位绑定被清空。

**规避**：保存前先备份；优先使用"替换"功能（`ReplaceVideoConfigAsync` / `ReplaceSettingsConfigAsync`，覆盖前自动备份）。覆盖范围明细见 [../../Application/api-gameconfig.md](../../Application/api-gameconfig.md)（解析 / 序列化要点）。

## 2. 直接保存没有自动备份

**证据**：只有 `ReplaceVideoConfigAsync` / `ReplaceSettingsConfigAsync` / `Restore*FromBackupAsync` 内部调用 `Backup*Async`；`SaveVideoConfigAsync` / `SaveSettingsConfigAsync` 直接写文件。

**后果**：坑点 1 发生时没有可回滚的备份。

## 3. 加载失败静默回退默认值

**证据**：`LoadVideoConfigAsync` / `LoadSettingsConfigAsync` 在文件不存在或解析抛异常时 `catch` 后返回默认配置（`GetDefaultPreset()` / `new ApexSettingsConfig()`），不区分"未安装 / 文件损坏 / 正常默认"。

**后果**：文件损坏时 UI 显示默认值，用户点保存即以默认值覆盖原文件（叠加坑点 1/2 不可恢复）。

**规避**：保存前核对 `IsApexInstalled()` 与文件存在性；异常场景给出提示而非静默继续。

## 4. 未安装 Apex 时保存抛异常

**证据**：`GetVideoConfigPath()` / `GetSettingsConfigPath()` 在 `%USERPROFILE%\Saved Games\Respawn\Apex\local` 目录不存在时返回 `null`，`Save*Async` 此时抛 `InvalidOperationException("无法找到 Apex Legends 配置文件目录")`。

**现状**：`ApexConfigViewModel.Save()` 先检查 `IsApexInstalled` 再调用，UI 层已拦截；直接调用服务的代码需自行检查。

## 5. float 序列化/解析未固定 InvariantCulture

**证据**：`SerializeVideoConfig` 用 `$"{config.Gamma:F6}"` 内插（当前区域文化），`ApplyVideoConfigValue` / `ApplySettingsConfigValue` 用无参 `float.TryParse`。

**后果**：在小数逗号区域（如 de-DE）下，`1.0` 会被写成 `"1,000000"`，游戏无法识别；反之文件中的 `"1.27"` 解析失败，静默回落默认值（叠加坑点 3）。

**规避**：读写 float 一律使用 `CultureInfo.InvariantCulture`。

## 6. `GenerateOptionsString` 的无条件与条件输出

**证据**（`ApexLaunchOptions.GenerateOptionsString`）：

- `+fps_max {FpsMax}` **无条件输出**，`FpsMax=0` 也会生成 `+fps_max 0`（语义是"无限制"，通常无害但无法省略）
- `-w` / `-h` 仅在 `Windowed || Noborder` 时输出，单独设置 `Width`/`Height` 不生效
- `+cl_fovScale` 固定两位小数并带引号（`+cl_fovScale "1.27"`）

**规避**：预期输出与实际不一致时先对照 [launch-options.md](launch-options.md) 的生成规则表。

## 7. UI 启动项预设按索引耦合

**证据**：`ApexConfigViewModel.OnSelectedLaunchPresetIndexChanged` 用 `value == 3` 判定"自定义"模式，并用 `value switch` 分发预设字符串；`LaunchOptionPresets` 集合的顺序（竞技优化/高画质/调试模式/自定义启动选项）与此索引硬编码耦合。

**后果**：调整预设列表顺序或增删项会使索引错位，选错预设或误入自定义模式。

## 8. 备份时间解析的回退路径错误

**摘要**：`ParseBackupTime` 在文件名时间戳解析失败时回退 `File.GetCreationTime`，但拼接路径缺少 `videoconfig/` / `settings/` 子目录，恒得文件系统默认值（`1601-01-01`）——时间戳异常的备份文件在历史列表中显示确定性错误时间（代码缺陷，待修；标准命名文件不受影响）。

完整机制、后果与建议见 [Application/pitfalls.md 坑点 17](../../Application/pitfalls.md)。

## 9. 游戏运行时写文件会被回写覆盖

游戏退出时回写全部配置，运行中修改 `videoconfig.txt` / `settings.cfg` 会被覆盖。游戏侧协议与操作建议统一记录在 [README.md](README.md)（修改建议流程、安全警告），此处不重复。

---

**协议版本**: 1.1
