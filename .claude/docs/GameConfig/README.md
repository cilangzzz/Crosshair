# GameConfig 配置层文档

## 模块定位

GameConfig 模块记录各游戏的**配置文件协议**（游戏侧配置文件格式、字段、启动项）与 CrosshairPro 的**游戏配置功能**（读取、修改、备份、启动项生成）。与准心渲染、热键等模块无关的内容不在此处。

## 覆盖范围

### 深度文档游戏（本目录有独立子文档）

| 游戏 | 子目录 | 深度 |
|------|--------|------|
| Apex Legends | [Apex/](Apex/README.md) | 完整：videoconfig.txt / settings.cfg 字段级协议 + 专用服务 `ApexConfigService`（真实读写游戏文件） |
| Counter-Strike 2 | [CS2/](CS2/README.md) | 完整：配置协议 + `Cs2ConfigViewModel` 通用策略编辑（仅存本地 JSON，不写游戏文件） |

### 仅内置档案的游戏（无独立子文档）

`GameProfile.BuiltIn`（`src/CrosshairPro.Core/Models/GameProfile.cs`）内置 8 个游戏档案，用于进程匹配与准心自动切换：

| 游戏 | 进程名 | 通用策略启动项 |
|------|--------|----------------|
| Counter-Strike 2 | `cs2` | 支持 |
| CS:GO | `csgo` | 支持（复用 CS2 策略） |
| Valorant | `VALORANT-Win64-Shipping` | 不支持 |
| Apex Legends | `r5apex` | 支持 |
| Overwatch 2 | `Overwatch` | 不支持 |
| PUBG | `TslGame` | 支持 |
| Fortnite | `FortniteClient-Win64-Shipping` | 不支持 |
| Rainbow Six Siege | `RainbowSix` | 支持 |

这些游戏仅出现在通用策略（`GameConfigService`）与进程匹配（`GameProfile`）中，字段级配置协议未文档化。

## 两种配置通道

应用对游戏配置有两条独立通道，不要混淆：

1. **专用深度通道（仅 Apex）**：`IApexConfigService` 直接读写游戏真实配置文件（`videoconfig.txt`、`settings.cfg`），支持备份/替换/恢复。协议与坑点见 [Apex/](Apex/README.md)。
2. **通用策略通道（8 个内置游戏）**：`IGameConfigService` 按 `GameConfigStrategy` 定义渲染可配置项，结果存到 CrosshairPro 自己的 JSON；`ApplyConfigAsync` 尚未实现写游戏文件（`GameConfigService.cs` 中为 TODO）。CS2 页面即基于此通道。

## 配置文件命名与存放规则

### 应用侧（CrosshairPro 自有数据）

| 路径 | 内容 |
|------|------|
| `%APPDATA%\CrosshairPro\gameconfigs\{gameId}.json` | 通用策略通道的各游戏配置（`gameId` = `GameProfile.Id`，如 `builtin-cs2`） |
| `%APPDATA%\CrosshairPro\launch_options.txt` | Apex 启动项字符串 |
| `%APPDATA%\CrosshairPro\apex-backups\videoconfig\yyyy-MM-dd_HH-mm-ss_videoconfig.txt` | Apex videoconfig.txt 备份 |
| `%APPDATA%\CrosshairPro\apex-backups\settings\yyyy-MM-dd_HH-mm-ss_settings.cfg` | Apex settings.cfg 备份 |

准心配置（`config.json`、`appstate.json`、`presets/`）不属于本模块，见 [../Services/README.md](../Services/README.md)。

### 游戏侧（各游戏真实配置文件）

| 游戏 | 配置目录 | 主要文件 |
|------|----------|----------|
| Apex Legends | `%USERPROFILE%\Saved Games\Respawn\Apex\local\` | `videoconfig.txt`（KeyValues）、`settings.cfg`（Valve CFG） |
| CS2 | Steam 用户数据目录（详见 [CS2/config.md](CS2/config.md)） | `video.txt`、`cs2_video.txt`、autoexec.cfg、启动项 |

## 关键配置项类别

各游戏配置项大体落在以下类别（字段细节见各子目录文档）：

| 类别 | 典型项 | 所在文件（Apex 例） |
|------|--------|---------------------|
| 显示 | 分辨率、窗口模式、刷新率、伽马 | videoconfig.txt |
| 画质 | 纹理、阴影、粒子、物理、贴花、LOD、体积效果、SSAO | videoconfig.txt |
| 性能 | 垂直同步、抗锯齿、DVS、帧率上限 | videoconfig.txt / settings.cfg |
| 输入 | 鼠标灵敏度、各倍镜灵敏度、原始输入、键位绑定 | settings.cfg |
| 音频/语音 | 扬声器、设备、麦克风 | settings.cfg |
| 网络 | 插值、命令/更新速率 | settings.cfg |
| 启动项 | 帧率、FOV、窗口、优先级、调试 | Steam/EA App 启动选项框 |

## 与其他模块的关系

- **Application 层**：`IApexConfigService`、`IGameConfigService` 及 DI 注册见 [../Application/README.md](../Application/README.md)
- **Core 层**：`ApexVideoConfig`、`ApexSettingsConfig`、`ApexLaunchOptions`、`GameProfile`、`GameConfigStrategy`、`GameConfig` 模型定义见 [../Core/README.md](../Core/README.md) 与 [Apex/data-model.md](Apex/data-model.md)
- **App 层**：页面 `ApexPage`、`Cs2Page`（经 `GamePageSelector` 按 gameId 分发）与 `ApexConfigViewModel`、`Cs2ConfigViewModel` 见 [../App/README.md](../App/README.md)
- 进程匹配与准心自动切换依赖 `GameProfile`，热键逻辑见 [../Infrastructure/README.md](../Infrastructure/README.md)

## 详细文档

### Apex/

- [Apex/README.md](Apex/README.md) — 配置协议概述与应用侧读写入口
- [Apex/videoconfig.md](Apex/videoconfig.md) — videoconfig.txt 全字段详解
- [Apex/settings.md](Apex/settings.md) — settings.cfg 全字段与键位绑定
- [Apex/launch-options.md](Apex/launch-options.md) — 启动参数与应用侧生成规则
- [Apex/data-model.md](Apex/data-model.md) — 文件协议 + C# 数据模型
- [Apex/pitfalls.md](Apex/pitfalls.md) — 应用侧读写坑点
- [Apex/CHANGELOG.md](Apex/CHANGELOG.md) — 变更日志

### CS2/

- [CS2/README.md](CS2/README.md) — CS2 配置概述
- [CS2/config.md](CS2/config.md) — 配置文件详解
- [CS2/video.md](CS2/video.md) — 视频配置
- [CS2/launch-options.md](CS2/launch-options.md) — 启动项
- [CS2/data-model.md](CS2/data-model.md) — 数据模型
- [CS2/integration.md](CS2/integration.md) — 应用集成
- [CS2/pitfalls.md](CS2/pitfalls.md) — 坑点
- [CS2/CHANGELOG.md](CS2/CHANGELOG.md) — 变更日志

---
**协议版本**: 1.0
