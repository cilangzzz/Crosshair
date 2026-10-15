# Apex Legends 启动参数详解

## 概述

启动参数（Launch Options）是在游戏启动时传递给可执行文件的命令行参数，用于控制游戏行为、优化性能、调试问题。CrosshairPro 在 Apex 页面提供启动项编辑、预设与一键复制，字符串持久化在 `%APPDATA%\CrosshairPro\launch_options.txt`。

---

## 设置位置

### Steam 版本

1. 右键游戏 → 属性
2. 在"启动选项"文本框中输入参数
3. 格式：`参数1 参数2 参数3`

### EA App 版本

1. 游戏库 → Apex Legends → ...
2. 查看属性 → 高级启动选项
3. 输入参数

### Origin 版本（旧）

1. 游戏库 → 右键游戏 → 游戏属性
2. 高级游戏选项 → 命令行参数

---

## 启动参数索引（游戏协议参考）

### 一、性能优化参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-high` | 开关 | 关 | 设置进程为高优先级 |
| `-threads` | 数值 | 自动 | 指定CPU线程数 |
| `-freq` / `-refresh` | 数值 | 60 | 强制刷新率 (Hz) |
| `-novid` | 开关 | 关 | 跳过开场视频 |
| `-nojoy` | 开关 | 关 | 禁用摇杆支持 |

### 二、窗口参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-windowed` | 开关 | 关 | 窗口模式 |
| `-noborder` | 开关 | 关 | 无边框窗口模式 |
| `-fullscreen` | 开关 | 关 | 全屏模式 |
| `-w` / `-h` | 数值 | 默认 | 窗口宽度 / 高度（需配合 `-windowed`） |
| `-x` / `-y` | 数值 | 居中 | 窗口位置 |

### 三、游戏控制参数（+ 前缀命令）

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+fps_max` | 数值 | 0 | 最大帧率限制 |
| `+cl_showfps` | 开关 | 0 | 显示FPS计数器 |
| `+cl_showpos` | 开关 | 0 | 显示位置/速度信息 |
| `+cl_fovScale` | 数值 | 1.0 | 视野缩放 (1.0-1.35；配合信箱参数可更高) |
| `+mat_letterbox_aspect_min` | 数值 | 0.0 | 信箱模式最小宽高比 |
| `+m_rawinput` | 开关 | 0 | 原始鼠标输入 |

### 四、网络参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+cl_interp` | 数值 | 0.015 | 网络插值 |
| `+cl_interp_ratio` | 数值 | 1 | 插值比率 |
| `+cl_cmdrate` | 数值 | 60 | 命令速率 |
| `+cl_updaterate` | 数值 | 60 | 更新速率 |

### 五、配置与调试参数

| 参数 | 类型 | 说明 |
|------|------|------|
| `+exec autoexec.cfg` | 命令 | 启动时执行配置文件（位于游戏 `cfg` 目录，需手动创建） |
| `-dev` | 开关 | 开发者模式 |
| `-console` | 开关 | 启用控制台 |
| `-condebug` / `-conclearlog` | 开关 | 控制台日志 / 清除日志 |

### 六、关键参数说明

#### `+fps_max` 帧率限制

| 场景 | 推荐值 | 说明 |
|------|--------|------|
| 竞技 | `+fps_max 180` | 稳定帧率 |
| 高刷新率 | `+fps_max 279` | 配合240Hz显示器 |
| 无限制 | `+fps_max 0` | 最大性能（可能不稳定） |

建议设置为显示器刷新率的 1.2-1.5 倍。

#### `+cl_fovScale` 视野缩放

| 参数值 | 实际FOV | 说明 |
|--------|---------|------|
| 1.0 | 90° | 默认视野 |
| 1.27 | 110° | 游戏内最大，常用竞技设置 |
| 2.0 | 超宽 | 需配合 `+mat_letterbox_aspect_min 1.0` |

#### `+m_rawinput` 原始鼠标输入

绕过 Windows 鼠标设置直接读取鼠标数据，减少输入延迟、消除鼠标加速影响；竞技玩家建议开启。

---

## 应用侧生成规则（`ApexLaunchOptions.GenerateOptionsString`）

CrosshairPro 允许两种模式：直接编辑字符串，或使用结构化的 `ApexLaunchOptions` 模型自动生成。生成规则：

| 属性 | 生成参数 | 输出条件 |
|------|----------|----------|
| `High` | `-high` | 为 true |
| `Threads` / `Freq` | `-threads N` / `-freq N` | 值 > 0 |
| `Novid` / `Nojoy` | `-novid` / `-nojoy` | 为 true |
| `Windowed` / `Noborder` / `Fullscreen` | `-windowed` / `-noborder` / `-fullscreen` | 为 true |
| `Width` / `Height` | `-w N` / `-h N` | 值 > 0（0 = 不输出，独立于窗口模式；解析无 -w/-h 的字符串后保持不输出，不注入默认值） |
| `FpsMax` | `+fps_max N` | 值 > 0（显式 `+fps_max 0` 不还原） |
| `ClShowfps` | `+cl_showfps N` | 值 > 0 |
| `ClShowpos` | `+cl_showpos 1` | 为 true |
| `ClFovScale` | `+cl_fovScale "F2"` | 值 > 0（带引号输出；2026-10-14 起回归启动项通道，UI 面板范围 0-3.0，超 1.35 需配合信箱比例） |
| `MatLetterboxAspectMin` | `+mat_letterbox_aspect_min F1` | 值 > 0，一位小数 |
| `MRawinput` | `+m_rawinput 1` | 为 true |
| `ClInterp` / `ClInterpRatio` / `ClCmdrate` / `ClUpdaterate` | 对应 `+cl_*` | 值 > 0 |
| `Dev` / `Console` | `-dev` / `-console` | 为 true |

各参数按上述顺序拼接，空格分隔。UI 上启动参数面板在启动选项卡中常显（预设下方、启动项字符串框下方），fps_max/cl_showfps/cl_showpos/cl_fov_scale 的编辑入口位于面板「游戏参数」组（绑定 `ApexLaunchOptions`）；settings.cfg 渠道仍保留这些字段的读写，但 ApexPage 不再提供其独立编辑行。

`LoadFromString(string?)` 为逆向解析（双向联动：切入自定义模式或自定义模式下重置时把启动项字符串还原为结构化编辑器）：先 `ResetToDefaults()` 重置全部字段（数值 0 = 未设置），再按空白/引号拆分令牌识别上表全部参数（大小写不敏感，`-refresh` 作为 `-freq` 别名），未知令牌忽略——因此经面板重新生成时，字符串中的未知令牌（如 `-condebug`）与值为 0 的已知令牌（如 `+fps_max 0`）会丢失。

## 应用侧预设

服务默认模板（`ApexConfigService.GenerateLaunchOptions()`，重置时使用）：

```
-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -novid
```

UI 启动项预设（`ApexConfigViewModel`，按索引选择）：

| 索引 | 名称 | 内容 |
|------|------|------|
| 0 | 竞技优化 | `-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -novid` |
| 1 | 高画质 | `-dev +fps_max 279 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -w 2560 -h 1600 -novid` |
| 2 | 调试模式 | `-console -dev -condebug +fps_max 0` |
| 3 | 自定义启动选项 | 展开结构化 `ApexLaunchOptions` 编辑器，实时生成 |

索引与预设的耦合关系是坑点之一，见 [pitfalls.md](pitfalls.md)。

### 竞技优化配置（游戏协议参考，需手动组合）

```
+exec autoexec.cfg -high -threads 8 -novid -nojoy -freq 144
```

---

## 参数优先级

```
启动参数 > autoexec.cfg > settings.cfg > videoconfig.txt > 游戏默认值
```

**说明**：
- 启动参数优先级最高，可以覆盖配置文件中的设置
- 游戏内设置会覆盖启动参数（非只读情况）

---

## 注意事项

### 常见问题

1. **参数不生效**：检查参数拼写；确认 Steam/EA App 正确保存；某些参数可能被游戏更新移除
2. **游戏崩溃**：移除所有参数测试，逐个添加排查；检查 autoexec.cfg 语法
3. **配置文件不执行**：确认 `autoexec.cfg` 位置正确（游戏 `cfg` 目录）、编码为 UTF-8 无 BOM
4. **刷新率不生效**：确认显示器支持并在 Windows 显示设置中启用

### 最佳实践

1. **备份配置**：修改前备份 `settings.cfg` 和 `videoconfig.txt`
2. **逐步测试**：添加参数后测试稳定性
3. **版本更新**：游戏更新后重新验证参数有效性

---

## 相关文档

- [README.md](README.md) — 配置协议概述
- [videoconfig.md](videoconfig.md) — 视频配置详解
- [settings.md](settings.md) — 游戏设置详解
- [data-model.md](data-model.md) — C# 数据模型
- [pitfalls.md](pitfalls.md) — 应用侧坑点

---
**协议版本**: 1.1
