# CS2 配置变更日志

## 2026-09-01 - v1.1

### 新增

- `integration.md` - CS2 与 CrosshairPro 集成指南
  - `GameProfile(cs2.exe)` 注册流程
  - `GameConfigStrategy(builtin-cs2)` 字段映射到 video.txt / config.cfg
  - 数据流：读取 / 保存 / 应用
  - 实施检查清单
- `pitfalls.md` - CS2 配置坑点
  - VAC 反作弊边界与白名单
  - CS2 vs CS:GO 命令差异（`mat_queue_mode`、`mat_picmip` 等）
  - video.txt 写入时机与 Steam 云同步冲突
  - autoexec.cfg 执行机制
  - 文件编码（UTF-8 无 BOM）
  - 准心叠加与游戏内准心共存
  - 启动参数长度限制
  - 备份与回滚策略

### 文档结构更新

```
docs/GameConfig/CS2/
├── README.md           # 配置协议概述
├── video.md            # 视频配置详解
├── config.md           # 游戏配置详解
├── launch-options.md   # 启动参数详解
├── data-model.md       # 数据模型定义
├── integration.md      # 与 CrosshairPro 集成（新增）
├── pitfalls.md         # CS2 配置坑点（新增）
└── CHANGELOG.md        # 变更日志
```

---

## 2026-07-31 - v1.0

### 新增

- 创建完整的 CS2 配置协议文档
- `README.md` - 配置协议概述
- `video.md` - 视频配置详解
- `config.md` - 游戏配置详解
- `launch-options.md` - 启动参数详解
- `data-model.md` - 数据结构定义

### 文档结构

```
docs/GameConfig/CS2/
├── README.md           # 配置协议概述
├── video.md            # 视频配置详解
├── config.md           # 游戏配置详解
├── launch-options.md   # 启动参数详解
├── data-model.md       # 数据模型定义
└── CHANGELOG.md        # 变更日志
```

### 配置分类

- 显示设置：分辨率、窗口模式、刷新率
- 画质设置：纹理、阴影、特效
- 性能设置：帧率、网络、插值
- 游戏设置：准心、键位绑定、鼠标
- 音频设置：音量、设备
- HUD设置：界面、雷达

### CS2 vs CS:GO 变化

| 特性 | CS:GO | CS2 |
|------|-------|-----|
| 引擎 | Source 1 | Source 2 |
| `mat_queue_mode` | 支持 | 已移除 |
| `mat_picmip` | 支持 | 已移除 |
| `fps_max_menu` | 支持 | 已移除 |
| 配置格式 | 兼容 | 兼容 |
| 大部分命令 | 兼容 | 兼容 |

---
**创建时间**: 2026-07-31