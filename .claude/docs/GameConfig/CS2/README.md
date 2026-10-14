# Counter-Strike 2 配置文件协议

> 文档型模块：描述 CS2（Source 2 引擎）的外部配置协议，以及 CrosshairPro 对该协议的集成方式。本模块无独立源码目录，相关源码：
> - `src/CrosshairPro.Core/Models/GameConfig.cs`、`GameConfigStrategy.cs`、`GameProfile.cs`
> - `src/CrosshairPro.Application/Services/GameConfigService.cs`（`builtin-cs2` 策略）
> - `src/CrosshairPro.App/ViewModels/Cs2ConfigViewModel.cs`、`src/CrosshairPro.App/Views/GamePages/Cs2Page.xaml(.cs)`

## 概述

CS2 配置系统沿袭 CS:GO 但有所变化。主要配置文件：

| 文件 | 用途 | 格式 |
|------|------|------|
| `video.txt` | 视频图形配置 | KeyValues |
| `config.cfg` | 游戏设置、键位 | Valve CFG |
| `autoexec.cfg` | 自定义启动脚本（需手动创建） | Valve CFG |
| `cs2_machine_convars.vcfg` | 机器配置 | KeyValues |

**CrosshairPro 的集成边界**：仅将配置保存到本地 `%APPDATA%\CrosshairPro\gameconfigs\builtin-cs2.json`，不直接写入 `video.txt` / `autoexec.cfg`，避免与 VAC、Steam 云同步冲突（详见 [integration.md](integration.md) 与 [pitfalls.md](pitfalls.md)）。

---

## 文件位置

### Steam 云同步位置

```
Steam\userdata\<SteamID3>\730\local\cfg\
├── config.cfg              # 主配置文件
├── video.txt               # 视频配置
├── autoexec.cfg            # 自定义脚本（需手动创建）
└── cs2_machine_convars.vcfg # 机器配置
```

**完整路径示例**：
```
C:\Program Files (x86)\Steam\userdata\12345678\730\local\cfg\config.cfg
```

### 游戏安装目录

```
Steam\steamapps\common\Counter-Strike 2\game\cfg\
├── config_default.cfg      # 默认配置
└── valve.rc                # 启动脚本
```

### SteamID3 说明

- SteamID3 是 32 位整数 ID
- 可通过 `steamrep.com` 或 Steam 个人资料 URL 查找
- 格式：`U:1:<SteamID64 的后 32 位>`

---

## 配置文件结构

### video.txt 格式

```txt
"video"
{
    "setting.defaultres"    "1920"
    "setting.defaultresheight"    "1080"
    "setting.fullscreen"    "1"
}
```

**特点**：KeyValues 文本格式；键名以 `setting.` 前缀；值用双引号包裹。字段详解见 [video.md](video.md)。

### config.cfg 格式

```cfg
// 键位绑定
bind "KEY" "COMMAND"

// 游戏设置
fps_max "400"

// 准心设置
cl_crosshairsize "2"
cl_crosshaircolor "1"
```

**特点**：Valve CFG 格式；每行一个命令；支持 `//` 注释；`bind` 绑定键位。字段详解见 [config.md](config.md)。

### autoexec.cfg 格式

```cfg
// 帧率设置
fps_max "0"

// 网络设置
cl_cmdrate "128"
cl_updaterate "128"
cl_interp "0"
cl_interp_ratio "1"

// 执行完毕提示
echo "autoexec.cfg loaded"
```

CS2 **不会**自动执行 `autoexec.cfg`，需启动参数 `+exec autoexec.cfg` 或在 `config.cfg` 末尾追加 `exec autoexec.cfg`。

---

## 配置优先级

```
1. 启动参数 (+command)
2. autoexec.cfg
3. config.cfg
4. 游戏内设置
5. config_default.cfg
6. 引擎默认值
```

## 读写协议

### 读取时机

| 事件 | 行为 |
|------|------|
| 游戏启动 | 读取 `config.cfg`, `video.txt` |
| 地图加载 | 执行 `autoexec.cfg`（如配置） |
| 控制台命令 | 即时执行并写入 `config.cfg` |

### 写入时机

| 事件 | 行为 |
|------|------|
| 更改视频设置 | 写入 `video.txt` |
| 更改游戏设置 | 写入 `config.cfg` |
| 控制台命令 | 自动写入 `config.cfg` |
| 退出游戏 | 写入所有配置 |

---

## CrosshairPro 集成要点

| 概念 | 值 / 位置 |
|------|-----------|
| GameProfile | `builtin-cs2`，进程名 `cs2`，Priority 100（`GameProfile.BuiltIn.GetAll()`） |
| 配置策略 | `GameConfigService.CreateCS2Strategy()`：video 分区（fullscreen / resolution / aspect_ratio / refresh_rate）+ game 分区（fps_max / cl_showfps） |
| 启动项 | `SupportsLaunchOptions = true`，保存在 `GameConfig.LaunchOptions`，不写 Steam |
| 持久化 | `%APPDATA%\CrosshairPro\gameconfigs\builtin-cs2.json` |
| UI 入口 | GamesPage → GamePageSelector → Cs2Page → Cs2ConfigViewModel（DI 单例，注册于 `App.xaml.cs`） |

数据流与字段映射详见 [integration.md](integration.md)；持久化模型详见 [data-model.md](data-model.md)。

---

## 安全警告

1. **VAC 兼容**：CS2 使用 VAC 反作弊，禁止写入作弊命令（白名单见 [pitfalls.md](pitfalls.md)）
2. **配置重置**：游戏更新可能重置配置
3. **云同步**：配置会同步到 Steam 云
4. **只读保护**：可将 `video.txt` 设为只读防止重置，但 CrosshairPro 写入也会失败

---

## 详细文档

| 文档 | 说明 |
|------|------|
| [video.md](video.md) | video.txt 字段定义与配置模板 |
| [config.md](config.md) | config.cfg 字段定义（键位、准心、网络、音频、HUD、雷达、视角） |
| [launch-options.md](launch-options.md) | 启动参数详解与推荐配置 |
| [data-model.md](data-model.md) | 数据结构与字段定义（协议 + CrosshairPro 模型） |
| [integration.md](integration.md) | 与 CrosshairPro 集成的映射指南与数据流 |
| [pitfalls.md](pitfalls.md) | CS2 配置常见坑点（VAC、Steam 云同步、编码等） |
| [CHANGELOG.md](CHANGELOG.md) | 变更日志 |

## 相关资源

- [Total CS2 配置指南](https://totalcsgo.com/command)
- [CS2 控制台命令](https://cs2console.com)

---
**创建时间**: 2026-07-31
**协议版本**: 1.1
