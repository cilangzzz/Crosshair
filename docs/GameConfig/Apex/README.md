# Apex Legends 配置文件协议

## 概述

Apex Legends 使用两个主要配置文件存储游戏设置：

| 文件 | 用途 | 格式 |
|------|------|------|
| `videoconfig.txt` | 视频图形配置 | KeyValues (VDF) |
| `settings.cfg` | 游戏设置、键位、鼠标 | Valve CFG |

---

## 文件位置

```
Windows: %USERPROFILE%\Saved Games\Respawn\Apex\local\
Steam:   Steam\userdata\<id>\1172470\local\
EA App:  同 Windows 路径
```

**完整路径示例**：
```
C:\Users\<用户名>\Saved Games\Respawn\Apex\local\videoconfig.txt
C:\Users\<用户名>\Saved Games\Respawn\Apex\local\settings.cfg
```

---

## 文档索引

| 文档 | 说明 |
|------|------|
| [videoconfig.md](videoconfig.md) | 视频配置详细选项 |
| [settings.md](settings.md) | 游戏设置详细选项 |
| [launch-options.md](launch-options.md) | 启动参数详解 |
| [data-model.md](data-model.md) | 数据结构与字段定义 |
| [CHANGELOG.md](CHANGELOG.md) | 变更日志 |

---

## 配置文件结构

### videoconfig.txt 格式

```txt
"VideoConfig"
{
    "setting.key_name"    "value"
    "setting.key_name"    "value"
}
```

**特点**：
- 使用 KeyValues 文本格式（类似 VDF）
- 所有键名以 `setting.` 前缀
- 字符串值需要双引号
- 数字值也需要双引号包裹
- 支持注释（但游戏会移除）

### settings.cfg 格式

```cfg
bind_US_standard "key" "command" flags
setting_name "value"
```

**特点**：
- 使用 Valve CFG 格式
- 每行一个设置或绑定
- 绑定格式：`bind_<layout> "key" "command" flags`
- 设置格式：`setting_name "value"`
- 支持分号分隔多个命令

---

## 配置优先级

```
1. 游戏内设置 → 最高优先级
2. settings.cfg / videoconfig.txt
3. autoexec.cfg（如果存在）
4. 默认值 → 最低优先级
```

---

## 读写协议

### 读取时机

| 事件 | 行为 |
|------|------|
| 游戏启动 | 读取所有配置文件 |
| 进入比赛 | 应用视频配置 |
| 切换场景 | 重新加载部分设置 |

### 写入时机

| 事件 | 行为 |
|------|------|
| 更改视频设置 | 立即写入 videoconfig.txt |
| 更改游戏设置 | 立即写入 settings.cfg |
| 退出游戏 | 写入所有配置 |

### 文件锁

- 游戏运行时会锁定配置文件
- 修改需要先退出游戏
- 或使用启动参数 `-cfg_save_on_exit`

---

## 修改建议流程

```
1. 备份原始文件
2. 退出游戏
3. 修改配置文件
4. 设置文件为只读（可选，防止重置）
5. 启动游戏验证
```

---

## 配置分类

### 按功能分类

| 类别 | 文件 | 说明 |
|------|------|------|
| 显示设置 | videoconfig.txt | 分辨率、窗口模式 |
| 画质设置 | videoconfig.txt | 纹理、阴影、特效 |
| 性能设置 | videoconfig.txt | 帧率、DVS、缓冲 |
| 键位绑定 | settings.cfg | 所有按键映射 |
| 鼠标设置 | settings.cfg | 灵敏度、加速 |
| 音频设置 | settings.cfg | 音量、设备 |
| 网络设置 | settings.cfg | 插值、更新率 |

---

## 版本兼容性

| 配置版本 | 游戏版本 | 说明 |
|----------|----------|------|
| `configversion=10` | 当前 | 最新版本 |
| `configversion=9` | 2023 | 旧版本字段可能缺失 |
| `configversion≤8` | 2022前 | 不推荐使用 |

---

## 安全警告

⚠️ **注意事项**：

1. **反作弊检测**：部分修改可能触发 Easy Anti-Cheat
2. **文件重置**：游戏更新可能重置配置
3. **只读属性**：设置为只读可防止重置，但游戏内设置不生效
4. **备份重要**：修改前务必备份原始文件

---

## 相关资源

- [data-model.md](data-model.md) - 完整字段定义
- [videoconfig.md](videoconfig.md) - 视频配置详解
- [settings.md](settings.md) - 游戏设置详解
- [Steam 社区指南](https://steamcommunity.com/app/1172470/guides/)
- [EA 官方支持](https://help.ea.com/apex-legends/)

---
**创建时间**: 2026-07-31
**协议版本**: 1.0