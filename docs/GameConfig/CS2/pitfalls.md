# CS2 配置坑点

> Counter-Strike 2 与 CS:GO 在配置系统、文件格式、游戏内行为上有多个差异点，在编写配置协议、生成配置或自动写入时需要特别留意。

---

## 1. VAC 与反作弊边界

### 1.1 禁止写入的命令

以下命令受 VAC 监控或在 CS2 中行为变更，**不得**由 CrosshairPro 写入游戏配置：

| 命令 | 原因 |
|------|------|
| `sv_cheats` | 一旦启用即触发 VAC |
| `mat_wireframe` | 与 hack 视觉特征冲突 |
| `cl_crosshairthickness` 高频抖动脚本 | 模拟脚本检测 |
| 任何武器参数 / 弹药参数 | 游戏行为修改 |
| `r_drawothermodels` | 透视类相关 |

**实施要求**：
- `GameConfigService.ApplyConfigAsync` 必须存在白名单过滤，仅允许本协议定义的字段被写入
- 写入前校验值域，避免触发异常监控
- 不通过任何 `+exec`/`+command` 启动参数重写受保护命令

### 1.2 配置文件完整性

写入 `config.cfg` / `video.txt` 时必须保留所有现有字段：

```csharp
// ❌ 错误：覆盖整个文件会破坏游戏内未在 UI 暴露的设置
File.WriteAllText(path, generatedText);

// ✅ 正确：逐项 patch 已知字段
foreach (var kvp in patchFields)
{
    ReplaceKeyValue(path, kvp.Key, kvp.Value);
}
```

---

## 2. CS2 vs CS:GO 命令差异

CS2 基于 Source 2，部分 CS:GO 命令被移除或语义变更。`CreateCSGOStrategy()` 复用 `CreateCS2Strategy()` 时需注意：

| CS:GO 命令 | CS2 状态 | 影响 |
|-----------|----------|------|
| `mat_queue_mode` | 已移除 | 写入会被忽略，但游戏无错误 |
| `mat_picmip` | 已移除 | 纹理质量改由其他键控制 |
| `fps_max_menu` | 已移除 | 需用 `fps_max` 统一控制 |
| `cl_righthand` | 保留 | 默认值 1 |
| `cl_crosshairsize` | 保留 | 默认值改变（CS2 默认 5，CS:GO 默认 2） |
| `cl_crosshaircolor` | 保留 | 颜色枚举保持一致 |
| `viewmodel_fov` | 保留 | 默认 60 |

**坑点**：
- 不要把 CS:GO 配置文件整体导入 CS2，部分键会被静默忽略并触发日志污染
- `mat_queue_mode = 2` 在 CS2 下没有任何效果，但写入不会失败
- CS:GO `config.cfg` 中可能有数百条 `bind`，CS2 中全部保留，不应裁剪

---

## 3. video.txt 写入时机

CS2 启动后视频设置会被异步写入 `video.txt`，存在以下竞争：

```
T0  游戏启动
T1  CrosshairPro 检测到 cs2.exe 启动
T2  CrosshairPro 尝试写入 video.txt     ← 可能与 T3 冲突
T3  游戏内 settings UI 写入 video.txt   ← 覆盖 T2 的修改
```

**建议**：

```csharp
private async Task<bool> TryWriteVideoFileAsync(string path, Dictionary<string, string> patches)
{
    // 1. 检查游戏是否在运行
    if (Process.GetProcessesByName("cs2").Any())
    {
        // 方案 A：跳过本次写入，仅保存到 CrosshairPro 配置
        _logger.LogWarning("CS2 正在运行，跳过 video.txt 写入以避免冲突");
        return false;

        // 方案 B：写入后重置游戏设置文件为只读
        // File.SetAttributes(path, FileAttributes.ReadOnly);
    }

    // 2. 不在游戏中时才执行实际写入
    return await PatchVideoFileAsync(path, patches);
}
```

---

## 4. Steam 云同步冲突

CS2 配置会随 Steam 云同步到其他机器。当用户：

1. 在 A 机启动 CS2 → Steam 云下载最新配置
2. CrosshairPro 在 A 机修改配置
3. 退出 CS2 → Steam 云上传

**问题**：如果 CrosshairPro 写入 `config.cfg` 后用户立即退出，可能上传未保存的最新配置。

**缓解**：
- 写入后延迟 5 秒再允许退出
- 使用 `host_writeconfig` 命令确保 CS2 接管最终状态
- CrosshairPro 配置保存到 `%APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json`，不直接覆盖游戏配置

---

## 5. autoexec.cfg 执行时机

CS2 不会自动执行 `autoexec.cfg`，需要：

```
启动参数：+exec autoexec.cfg
或 config.cfg 末尾：host_writeconfig + exec autoexec.cfg
```

**坑点**：
- 修改 `autoexec.cfg` 后必须重启 CS2 才生效
- 多个 `exec` 链可能冲突，建议只保留一个 `exec autoexec.cfg`
- `autoexec.cfg` 中的 `host_writeconfig` 会写入所有当前内存中的 cvar，可能与 Steam 云同步冲突

---

## 6. 控制台命令的隐式保存

在 CS2 控制台修改任何 cvar（如 `sensitivity 2`）会：

1. 立即应用到当前会话
3. 退出时自动写入 `config.cfg`
4. 触发 Steam 云同步

**风险**：用户在控制台调试时不小心设置的临时值会被持久化，**覆盖** CrosshairPro 的修改。

**建议**：
- CrosshairPro 不直接修改 `config.cfg`，而是建议用户使用 UI 调整
- 提供"导出到 autoexec.cfg"按钮，避免与 `config.cfg` 冲突

---

## 7. 设置项类型不匹配

CrosshairPro 的 `ConfigItemType` 与 CS2 实际值域不完全对应：

```csharp
// CS2 实际值域
cl_crosshairsize: float, 0-10, step 0.5
cl_crosshairthickness: float, 0-5, step 0.1

// 策略中只能定义 min=0, max=10, step=1（Int）
// ❌ 无法表达 0.5 步进
```

**解决方案**：
- `ConfigItemDefinition` 增加 `Step` 属性（`decimal`）
- 或增加 `Float` 类型到 `ConfigItemType` 枚举

**当前状态**：`Strategy` 中 `cl_crosshairsize` 等准心参数 **不在** CS2 策略中暴露（仅暴露 `fps_max`、`cl_showfps`、`fullscreen`、`resolution`），避开了精度问题。

---

## 8. 准心叠加与游戏内准心共存

CS2 本身有 `cl_crosshair*` 设置，CrosshairPro 的全局准心叠加与游戏内准心同时存在：

```
┌────────────────────────────────────┐
│           OverlayWindow            │
│   ┌─────────────────────────┐      │
│   │   CrosshairPro 准心     │      │
│   └─────────────────────────┘      │
│                                    │
│   （游戏窗口中也渲染了 cl_crosshair*）│
│   ┌─────────────────────────┐      │
│   │   cl_crosshair* 准心    │      │
│   └─────────────────────────┘      │
└────────────────────────────────────┘
```

**坑点**：
- 同时显示两个准心会造成视觉混乱
- `OverlayWindow` 鼠标穿透应保持 `WS_EX_TRANSPARENT`
- CrosshairPro 不应尝试修改 `cl_crosshair*`，应只叠加自己的准心
- 提供"隐藏游戏内准心"配置项，引导用户设置 `cl_crosshairstyle 0`

---

## 9. 启动参数长度限制

Steam 启动参数有长度限制（实测约 1023 字符）：

```
-novid -high -threads 8 -freq 144 +exec autoexec.cfg +fps_max 0 ...
```

**风险**：
- 用户配置复杂时（如多个 exec、自定义 cfg）可能超出限制
- Steam UI 截断时无提示

**CrosshairPro 处理**：
- `GameConfig.LaunchOptions` 字段长度校验（< 1000 字符）
- 超出时引导用户改用 `autoexec.cfg`

---

## 10. cs2.exe 与 csgo.exe 同时存在

用户可能同时安装 CS2 和 CS:GO，进程名不同：

| 游戏 | 进程名 | GameId |
|------|--------|--------|
| Counter-Strike 2 | `cs2.exe` | `builtin-cs2` |
| Counter-Strike: Global Offensive | `csgo.exe` | `builtin-csgo` |

**坑点**：
- `CreateCSGOStrategy()` 直接克隆 CS2 策略并修改 `GameId`，两者配置完全独立
- `GameProfile.Matches` 使用 `OrdinalIgnoreCase`，避免大小写问题
- 用户从 CS:GO 升级到 CS2 时，需手动迁移 `autoexec.cfg`

---

## 11. 配置文件编码

| 文件 | 编码 |
|------|------|
| `video.txt` | UTF-8（无 BOM），但部分版本可能有 BOM |
| `config.cfg` | UTF-8（无 BOM） |
| `autoexec.cfg` | UTF-8（无 BOM） |

**坑点**：
- Windows Notepad 默认保存为 UTF-8 BOM，会被 CS2 部分版本拒绝
- 推荐使用 VSCode / Notepad++ 保存为 UTF-8 无 BOM
- CrosshairPro 写入时必须显式 `new UTF8Encoding(false)`：

```csharp
// ✅ 正确：无 BOM
var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
await File.WriteAllTextAsync(path, content, encoding);
```

---

## 12. 只读属性与 Steam 同步

将 `video.txt` 设为只读可以防止游戏重置，但也带来副作用：

```
video.txt 只读
├─ 游戏内修改视频设置 → 写入失败
├─ 控制台修改 → 内存中有效，退出后丢失
└─ CrosshairPro 写入 → 失败
```

**建议**：
- CrosshairPro 不应直接修改 `video.txt`
- 而应通过 Steam 启动参数或 `autoexec.cfg` 间接影响游戏行为
- 这是当前 `GameConfigService.ApplyConfigAsync` 仅保存而不写入游戏配置的设计原因

---

## 13. tick rate 与网络配置

CS2 主流服务器分为 64 tick 和 128 tick，跨平台（FACEIT/5E/B5）多为 128 tick：

```csharp
// ✅ 正确：根据用户选择的 tick rate 应用不同配置
if (config.TickRate == TickRate.Tick128)
{
    items.Add(new ConfigItemDefinition { Key = "cl_cmdrate", DefaultValue = 128 });
    items.Add(new ConfigItemDefinition { Key = "cl_updaterate", DefaultValue = 128 });
}
else
{
    items.Add(new ConfigItemDefinition { Key = "cl_cmdrate", DefaultValue = 64 });
    items.Add(new ConfigItemDefinition { Key = "cl_updaterate", DefaultValue = 64 });
}
```

**当前实现**：`Strategy` 仅暴露 `fps_max`、`cl_showfps` 等基础项，不直接暴露 `cl_cmdrate`/`cl_updaterate`，由用户通过 autoexec.cfg 配置。

---

## 14. 全屏与无边框窗口

CS2 在 Source 2 下窗口模式行为变化：

| 设置 | CS:GO | CS2 |
|------|-------|-----|
| `fullscreen` 1 + `windowed` 0 | 全屏独占 | 全屏独占 |
| `fullscreen` 0 + `windowed` 1 | 窗口 | 窗口 |
| `fullscreen` 1 + `windowed` 1 | 无边框 | **无边框（Source 2 行为）** |

**坑点**：
- `GameConfigStrategy` 的 `fullscreen` 是 `bool`，无法表达三种模式
- 简化处理：true = 全屏独占，false = 窗口
- 无边框通过 Steam 启动参数 `-noborder` 设置

---

## 15. 配置文件备份策略

修改 `video.txt` 前**必须**备份：

```csharp
private async Task<string?> BackupConfigFileAsync(string sourcePath)
{
    if (!File.Exists(sourcePath)) return null;

    var backupDir = Path.Combine(_configDir, "backups", "cs2");
    Directory.CreateDirectory(backupDir);

    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var backupPath = Path.Combine(backupDir, $"{Path.GetFileName(sourcePath)}.{timestamp}.bak");

    File.Copy(sourcePath, backupPath, overwrite: true);
    return backupPath;
}
```

**保留策略**：
- 每个文件最多保留 10 个备份
- 自动清理超过 30 天的备份

---

## 16. 视频设置 vs CrosshairPro 准心

CS2 视频设置与 CrosshairPro 全局准心无直接关系：

- **视频设置**：影响 CS2 内部渲染性能
- **CrosshairPro 准心**：WPF 透明窗口叠加，独立于 CS2

**CrosshairPro 不应**：
- 修改 `video.txt` 中的渲染相关字段
- 假设视频设置变化会影响准心叠加
- 将准心参数（如 `cl_crosshairsize`）写入 `video.txt`

---

## 17. 用户场景下的常见错误

### 17.1 用户在 CS2 设置中改了分辨率

```
CS2 设置 UI → 写入 video.txt → Steam 云同步 → CrosshairPro 检测到文件变化
```

CrosshairPro 不应：
- 自动反向同步到自己的配置（会破坏用户的设置选择）
- 提示"恢复默认"（用户可能故意改了）

仅在以下情况同步：
- 用户在 CrosshairPro UI 中显式"导入游戏配置"

### 17.2 用户的 `autoexec.cfg` 被覆盖

如果 CrosshairPro 直接覆盖 `config.cfg`：

```cfg
// 用户的 autoexec.cfg
exec autoexec.cfg   ← 这行在 config.cfg 中
```

**风险**：CrosshairPro 写入时如果误删 `exec autoexec.cfg`，用户的所有 autoexec 设置会失效。

**正确做法**：
- 仅 patch 已知键，不重写整文件
- 或仅在 `autoexec.cfg` 中添加新行

### 17.3 启动参数被 Steam 截断

用户复制了一段长配置（如 `-novid -high -threads 8 -freq 144 +exec autoexec.cfg +cl_cmdrate 128 ...`），超过 1023 字符时被截断。

**CrosshairPro 处理**：
- 检测 `LaunchOptions.Length > 950` 时警告用户
- 建议改用 `autoexec.cfg` 存放长配置

---

## 18. 总结

CS2 配置协议实施时的关键防御点：

1. **VAC 白名单**：仅写入本协议定义的字段
2. **文件 patch 而非覆盖**：保护用户的所有现有配置
3. **运行时检测**：检测到 cs2.exe 运行时跳过游戏配置写入
4. **编码**：UTF-8 无 BOM
5. **备份**：修改前备份原文件
6. **建议而非强写**：引导用户使用 autoexec.cfg，而非强行写入 config.cfg
7. **准心分离**：CrosshairPro 的准心叠加独立于 CS2 的 `cl_crosshair*`

---

**创建时间**: 2026-09-01
**协议版本**: 1.1