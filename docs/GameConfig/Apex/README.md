# Apex Legends 配置文档

## 概述

Apex Legends 使用两个主要配置文件存储游戏设置，另有启动项参数控制启动行为：

| 文件 | 用途 | 格式 | 应用侧管理方式 |
|------|------|------|----------------|
| `videoconfig.txt` | 视频图形配置 | KeyValues (VDF) | `ApexConfigService` 反序列化/序列化重写 |
| `settings.cfg` | 游戏设置、键位、鼠标 | Valve CFG | `ApexConfigService` 反序列化/序列化重写 |
| 启动项 | 进程启动参数 | 命令行字符串 | 保存到应用自有文件，用户复制后粘贴到 Steam/EA App |

应用侧入口：`IApexConfigService`（实现类 `ApexConfigService`，DI 单例注册），UI 页面 `ApexPage` + `ApexConfigViewModel`。服务与 ViewModel 细节见 [../../Application/README.md](../../Application/README.md)。

## 游戏侧文件位置

```
Windows/EA App: %USERPROFILE%\Saved Games\Respawn\Apex\local\
Steam:          Steam\userdata\<id>\1172470\local\（本工具按上述 UserProfile 路径读写）
```

**完整路径示例**：
```
C:\Users\<用户名>\Saved Games\Respawn\Apex\local\videoconfig.txt
C:\Users\<用户名>\Saved Games\Respawn\Apex\local\settings.cfg
```

## 配置文件格式

### videoconfig.txt

```txt
"VideoConfig"
{
    "setting.key_name"    "value"
}
```

- KeyValues 文本格式（类似 VDF），所有键名以 `setting.` 前缀，值（含数字）需双引号包裹
- 字段全表见 [videoconfig.md](videoconfig.md)

### settings.cfg

```cfg
bind_US_standard "key" "command" flags
setting_name "value"
```

- Valve CFG 格式，每行一个设置或绑定，多个命令可用分号连接
- 字段全表见 [settings.md](settings.md)

## 配置优先级

```
1. 游戏内设置 → 最高优先级
2. settings.cfg / videoconfig.txt
3. autoexec.cfg（如果存在，经 +exec 加载）
4. 默认值 → 最低优先级
```

启动参数优先级与覆盖关系的完整说明见 [launch-options.md](launch-options.md)。

## CrosshairPro 读写协议（应用侧）

### 应用自有存储

| 路径 | 用途 |
|------|------|
| `%APPDATA%\CrosshairPro\launch_options.txt` | Apex 启动项字符串持久化 |
| `%APPDATA%\CrosshairPro\apex-backups\videoconfig\` | videoconfig.txt 备份（`yyyy-MM-dd_HH-mm-ss_videoconfig.txt`） |
| `%APPDATA%\CrosshairPro\apex-backups\settings\` | settings.cfg 备份（`yyyy-MM-dd_HH-mm-ss_settings.cfg`） |

### 读写行为

| 操作 | 行为 |
|------|------|
| 加载 | 目录不存在或文件缺失/解析异常时，静默返回默认配置（不报错） |
| 保存 | 序列化模型并**整体重写**目标文件；Apex 目录不存在时抛 `InvalidOperationException`；**不会自动备份** |
| 替换 | 先校验内容（videoconfig 须含 `"VideoConfig"`；settings 须含 `mouse_sensitivity` 或 `fps_max`），校验通过后**自动备份**再 `File.Copy` 覆盖 |
| 备份/恢复 | 按时间戳文件备份；恢复前会再备份当前文件 |
| 并发 | 服务内部用 `SemaphoreSlim(1,1)` 串行化游戏配置文件读写 |

上述行为的边界与风险（保存丢字段、静默回退等）统一记录在 [pitfalls.md](pitfalls.md)。

### 检测与预设

- `IsApexInstalled()`：仅检测 `%USERPROFILE%\Saved Games\Respawn\Apex\local` 目录是否存在
- 内置视频预设：竞技优化（`GetCompetitivePreset`）、高画质（`GetHighQualityPreset`）、默认（`GetDefaultPreset`），字段差异见 [data-model.md](data-model.md)

## 游戏侧读写时机（协议参考）

| 事件 | 行为 |
|------|------|
| 游戏启动 | 读取所有配置文件 |
| 进入比赛 | 应用视频配置 |
| 更改视频/游戏设置 | 立即写入对应配置文件 |
| 退出游戏 | 回写所有配置 |

因此**游戏运行时修改配置文件可能被退出时回写覆盖**，务必先退出游戏再修改（见下方安全警告）。

## 修改建议流程

```
1. 备份原始文件（或使用本工具的备份/替换功能，自动备份）
2. 退出游戏
3. 修改配置文件
4. 设置文件为只读（可选，防止重置；注意游戏内设置将不再生效）
5. 启动游戏验证
```

## 配置分类

| 类别 | 所在文件 | 说明 |
|------|----------|------|
| 显示设置 | videoconfig.txt | 分辨率、窗口模式、伽马 |
| 画质设置 | videoconfig.txt | 纹理、阴影、粒子、物理、贴花、LOD、体积效果、SSAO |
| 性能设置 | videoconfig.txt | 垂直同步、抗锯齿、DVS |
| 键位绑定 | settings.cfg | 所有按键映射（应用侧不建模，见 pitfalls.md） |
| 鼠标设置 | settings.cfg | 灵敏度、加速、各倍镜灵敏度 |
| 音频/语音设置 | settings.cfg | 音量、设备 |
| 网络设置 | settings.cfg | 插值、更新率 |
| 启动参数 | Steam/EA App + 应用内编辑 | 帧率、FOV、窗口、调试 |

## 版本兼容性

| 配置版本 | 游戏版本 | 说明 |
|----------|----------|------|
| `configversion=10` | 当前 | 最新版本（本工具模型默认值亦为 10） |
| `configversion=9` | 2023 | 旧版本字段可能缺失 |
| `configversion≤8` | 2022前 | 不推荐使用 |

不要手动修改 `configversion`，否则游戏可能重置配置。

## 安全警告

1. **反作弊检测**：部分修改可能触发 Easy Anti-Cheat
2. **文件重置**：游戏更新可能重置配置
3. **只读属性**：设置为只读可防止重置，但游戏内设置不再生效
4. **备份重要**：修改前务必备份原始文件；直接"保存"操作不会自动备份（见 [pitfalls.md](pitfalls.md)）

## 详细文档

- [videoconfig.md](videoconfig.md) — videoconfig.txt 全字段详解与应用侧序列化范围
- [settings.md](settings.md) — settings.cfg 全字段、键位绑定语法与应用侧序列化范围
- [launch-options.md](launch-options.md) — 启动参数详解、应用侧生成规则与预设
- [data-model.md](data-model.md) — 文件字段协议 + C# 数据模型（ApexVideoConfig / ApexSettingsConfig / ApexLaunchOptions）
- [pitfalls.md](pitfalls.md) — 应用侧读写坑点
- [CHANGELOG.md](CHANGELOG.md) — 变更日志

## 相关资源

- [Steam 社区指南](https://steamcommunity.com/app/1172470/guides/)
- [EA 官方支持](https://help.ea.com/apex-legends/)
- CS2 游戏配置文档：[../CS2/README.md](../CS2/README.md)

---
**协议版本**: 1.1
