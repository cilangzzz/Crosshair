# Counter-Strike 2 配置文件协议

## 概述

Counter-Strike 2 (CS2) 使用 Source 2 引擎，配置系统沿袭 CS:GO 但有所变化。主要配置文件包括：

| 文件 | 用途 | 格式 |
|------|------|------|
| `video.txt` | 视频图形配置 | KeyValues |
| `config.cfg` | 游戏设置、键位 | Valve CFG |
| `autoexec.cfg` | 自定义启动脚本 | Valve CFG |
| `cs2_machine_convars.vcfg` | 机器配置 | KeyValues |

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
└── valve.rc               # 启动脚本
```

### SteamID3 说明

- SteamID3 是 32 位整数 ID
- 可通过 `steamrep.com` 或 Steam 个人资料 URL 查找
- 格式：`U:1:<SteamID64 的后 32 位>`

---

## 文档索引

| 文档 | 说明 |
|------|------|
| [video.txt](video.md) | 视频配置详细选项 |
| [config.cfg](config.md) | 游戏设置详细选项 |
| [launch-options.md](launch-options.md) | 启动参数详解 |
| [data-model.md](data-model.md) | 数据结构与字段定义 |
| [integration.md](integration.md) | 与 CrosshairPro 集成的映射指南 |
| [pitfalls.md](pitfalls.md) | CS2 配置常见坑点（VAC、Steam 云同步、编码等） |
| [CHANGELOG.md](CHANGELOG.md) | 变更日志 |

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

**特点**：
- KeyValues 文本格式
- 键名以 `setting.` 前缀
- 值用双引号包裹

### config.cfg 格式

```cfg
// 键位绑定
bind "KEY" "COMMAND"

// 游戏设置
cl_cmdrate "64"
cl_updaterate "64"
fps_max "400"

// 准心设置
cl_crosshairsize "2"
cl_crosshaircolor "1"
```

**特点**：
- Valve CFG 格式
- 每行一个命令
- 支持 `//` 注释
- 使用 `bind` 命令绑定键位

### autoexec.cfg 格式

```cfg
// 帧率设置
fps_max "0"

// 网络设置
cl_cmdrate "128"
cl_updaterate "128"
cl_interp "0"
cl_interp_ratio "1"

// 准心设置
cl_crosshairsize "2"
cl_crosshaircolor "1"

// 买枪绑定
bind "kp_ins" "buy ak47"

// 执行完毕提示
echo "autoexec.cfg loaded"
```

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

---

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

### autoexec.cfg 执行

**方法 1**：启动参数
```
+exec autoexec.cfg
```

**方法 2**：在 `config.cfg` 添加
```cfg
host_writeconfig
exec autoexec.cfg
```

---

## 配置分类

### 按功能分类

| 类别 | 文件 | 说明 |
|------|------|------|
| 显示设置 | video.txt | 分辨率、窗口模式 |
| 画质设置 | video.txt | 纹理、阴影、特效 |
| 性能设置 | config.cfg | 帧率、网络、插值 |
| 准心设置 | config.cfg | 样式、颜色、大小 |
| 键位绑定 | config.cfg | 所有按键映射 |
| 鼠标设置 | config.cfg | 灵敏度、加速 |
| 音频设置 | config.cfg | 音量、设备 |
| 皮肤设置 | config.cfg | 武器皮肤、贴纸 |

---

## 版本兼容性

### CS2 vs CS:GO

| 特性 | CS:GO | CS2 |
|------|-------|-----|
| 引擎 | Source 1 | Source 2 |
| 配置文件 | config.cfg | config.cfg |
| 视频配置 | video.txt | video.txt |
| 启动参数 | 相同 | 相同 |
| 大部分命令 | 兼容 | 兼容 |
| 部分命令 | 已移除 | 已替换 |

### 已移除/更改的命令

| CS:GO 命令 | CS2 状态 |
|------------|----------|
| `mat_queue_mode` | 已移除 |
| `cl_cmdrate` | 保留，默认 64/128 |
| `cl_updaterate` | 保留，默认 64/128 |
| `fps_max_menu` | 已移除 |
| `r_drawtracers_firstperson` | 保留 |

---

## 安全警告

⚠️ **注意事项**：

1. **VAC 兼容**：CS2 使用 VAC 反作弊，禁止使用作弊命令
2. **配置重置**：游戏更新可能重置配置
3. **云同步**：配置会同步到 Steam 云
4. **只读保护**：可将 `video.txt` 设为只读防止重置

---

## 相关资源

- [video.md](video.md) - 视频配置详解
- [config.md](config.md) - 游戏设置详解
- [launch-options.md](launch-options.md) - 启动参数详解
- [integration.md](integration.md) - 与 CrosshairPro 集成指南
- [pitfalls.md](pitfalls.md) - CS2 配置坑点
- [Total CS2 配置指南](https://totalcsgo.com/command)
- [CS2 控制台命令](https://cs2console.com)

---
**创建时间**: 2026-07-31
**协议版本**: 1.1