# Apex Legends 启动参数详解

## 概述

启动参数（Launch Options）是在游戏启动时传递给可执行文件的命令行参数，用于控制游戏行为、优化性能、调试问题。

---

## 设置位置

### Steam 版本

1. 右键游戏 → 属性
2. 在"启动选项"文本框中输入参数
3. 格式：`参数1 参数2 参数3`

**位置截图**：
```
Steam 库 → Apex Legends → 右键 → 属性 → 启动选项
```

### EA App 版本

1. 游戏库 → Apex Legends → ...
2. 查看属性 → 高级启动选项
3. 输入参数

### Origin 版本（旧）

1. 游戏库 → 右键游戏 → 游戏属性
2. 高级游戏选项 → 命令行参数

---

## 启动参数索引

### 一、性能优化参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-high` | 开关 | 关 | 设置进程为高优先级 |
| `-threads` | 数值 | 自动 | 指定CPU线程数 |
| `-freq` | 数值 | 60 | 强制刷新率 (Hz) |
| `-refresh` | 数值 | 60 | 同 -freq |
| `-novid` | 开关 | 关 | 跳过开场视频 |
| `-nojoy` | 开关 | 关 | 禁用摇杆支持 |
| `-noborder` | 开关 | 关 | 无边框窗口模式 |
| `-windowed` | 开关 | 关 | 窗口模式 |
| `-fullscreen` | 开关 | 关 | 全屏模式 |

### 二、配置文件参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+exec` | 命令 | 无 | 启动时执行配置文件 |
| `-cfg` | 路径 | 无 | 指定配置文件路径 |
| `+host_writeconfig` | 命令 | 无 | 保存配置到文件 |

### 三、游戏控制参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+fps_max` | 数值 | 0 | 最大帧率限制 |
| `+cl_showfps` | 开关 | 0 | 显示FPS计数器 |
| `+cl_showpos` | 开关 | 0 | 显示位置/速度信息 |
| `+cl_fovScale` | 数值 | 1.0 | 视野缩放 (1.0-1.35) |
| `+mat_letterbox_aspect_min` | 数值 | 0.0 | 信箱模式最小宽高比 |
| `+mat_letterbox_aspect_max` | 数值 | 0.0 | 信箱模式最大宽高比 |
| `+m_rawinput` | 开关 | 0 | 原始鼠标输入 |
| `+m_acceleration` | 开关 | 0 | 鼠标加速 |

### 四、网络参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-tcp` | 开关 | UDP | 强制TCP连接 |
| `-noantiscreenscreenshot` | 开关 | 关 | 禁用屏幕截图保护 |
| `+cl_interp` | 数值 | 0.015 | 网络插值 |
| `+cl_interp_ratio` | 数值 | 1 | 插值比率 |
| `+cl_cmdrate` | 数值 | 60 | 命令速率 |
| `+cl_updaterate` | 数值 | 60 | 更新速率 |

### 五、调试参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-dev` | 开关 | 关 | 开发者模式 |
| `-console` | 开关 | 关 | 启用控制台 |
| `-toconsole` | 开关 | 关 | 输出到控制台 |
| `-condebug` | 开关 | 关 | 控制台日志 |
| `-conclearlog` | 开关 | 关 | 清除日志 |

### 六、图形参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-w` | 数值 | 默认 | 窗口宽度 |
| `-h` | 数值 | 默认 | 窗口高度 |
| `-x` | 数值 | 居中 | 窗口X位置 |
| `-y` | 数值 | 居中 | 窗口Y位置 |
| `-dxlevel` | 数值 | 95 | DirectX级别 (80/81/90/95) |
| `-mat_dxlevel` | 数值 | 95 | 同上 |

### 七、内存参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-heapsize` | 数值 | 自动 | 堆内存大小 (KB) |
| `-noheap` | 开关 | 关 | 禁用堆优化 |

### 八、系统参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-nohltv` | 开关 | 关 | 禁用HLTV |
| `-noservergamelogic` | 开关 | 关 | 禁用服务器游戏逻辑 |

---

## 参数详细说明

### 性能优化参数

#### `-high` 高优先级

```
-high
```

**作用**：将游戏进程设置为"高优先级"，减少被其他进程抢占CPU的情况。

**适用场景**：
- 多任务环境下的游戏
- 后台有其他程序运行

**注意**：
- 可能导致系统响应变慢
- 与其他高优先级程序冲突时可能无效

#### `-threads` 线程数

```
-threads 8
```

**作用**：指定游戏使用的CPU线程数。

**推荐值**：

| CPU核心数 | 推荐值 |
|-----------|--------|
| 4核 | `-threads 4` |
| 6核 | `-threads 6` |
| 8核 | `-threads 8` |
| 12核+ | `-threads 8` 或 `10` |

**注意**：
- 设置过多可能导致性能下降
- 一般设置为物理核心数

#### `-freq` / `-refresh` 刷新率

```
-freq 144
-refresh 144
```

**作用**：强制设置显示器刷新率。

**推荐值**：

| 显示器 | 参数 |
|--------|------|
| 60Hz | `-freq 60` |
| 120Hz | `-freq 120` |
| 144Hz | `-freq 144` |
| 165Hz | `-freq 165` |
| 240Hz | `-freq 240` |

**注意**：
- 需要显示器支持该刷新率
- 需要在Windows显示设置中启用

#### `-novid` 跳过视频

```
-novid
```

**作用**：跳过开场动画视频，加快启动速度。

**效果**：
- 启动速度提升约 2-3 秒
- 节省少量内存

#### `-nojoy` 禁用摇杆

```
-nojoy
```

**作用**：禁用游戏手柄/摇杆支持，减少输入检测开销。

**适用场景**：
- 纯键鼠玩家
- 减少输入延迟

### 配置文件参数

#### `+exec` 执行配置

```
+exec autoexec.cfg
```

**作用**：启动时自动执行指定的配置文件。

**配置文件位置**：
```
Steam\steamapps\common\Apex Legends\cfg\autoexec.cfg
```

**autoexec.cfg 示例**：
```cfg
// 视野设置
cl_fovScale "1.27"

// 鼠标设置
m_rawinput "1"
m_acceleration "0"

// 帧率设置
fps_max "0"
cl_showfps "1"

// 网络设置
cl_interp "0.015"
cl_interp_ratio "1"
cl_cmdrate "60"
cl_updaterate "60"

// 保存配置
host_writeconfig
```

**注意**：
- 文件需要手动创建
- 每次启动都会执行
- 可设置只读防止覆盖

### 游戏控制参数

#### `+fps_max` 帧率限制

```
+fps_max 180
+fps_max 279
+fps_max 0
```

**作用**：设置最大帧率限制。

**推荐值**：

| 场景 | 推荐值 | 说明 |
|------|--------|------|
| 竞技 | `+fps_max 180` | 稳定帧率 |
| 高刷新率 | `+fps_max 279` | 配合240Hz显示器 |
| 无限制 | `+fps_max 0` | 最大性能（可能不稳定） |

**注意**：
- 设置为显示器刷新率的 1.2-1.5 倍可获得最佳体验
- 过高的帧率可能导致帧时间不稳定

#### `+cl_showfps` 显示FPS

```
+cl_showfps 1
```

**作用**：在屏幕右上角显示FPS计数器。

**显示模式**：

| 值 | 显示内容 |
|----|----------|
| 0 | 不显示 |
| 1 | 简单FPS |
| 2 | 详细FPS + 帧时间 |

#### `+cl_showpos` 显示位置信息

```
+cl_showpos 1
```

**作用**：在屏幕上显示玩家位置和速度信息。

**显示内容**：
- 当前位置坐标 (x, y, z)
- 移动速度
- 朝向角度

**适用场景**：
- 滑铲跳练习
- 身法训练
- 录制分析

#### `+cl_fovScale` 视野缩放

```
+cl_fovScale "2"
+cl_fovScale "1.27"
+cl_fovScale "1.0"
```

**作用**：设置游戏内的视野范围。

**FOV 对照表**：

| 参数值 | 实际FOV | 说明 |
|--------|---------|------|
| 1.0 | 90° | 默认视野 |
| 1.1 | 97° | 略宽 |
| 1.27 | 110° | 常用竞技设置 |
| 1.35 | 120° | 最大视野 |
| 2.0 | 超宽 | 需要特定参数支持 |

**注意**：
- 游戏内最大只能设置到 110° (1.27)
- 使用 `+mat_letterbox_aspect_min 1.0` 可启用更宽视野
- 过大的FOV可能导致画面变形

#### `+mat_letterbox_aspect_min` 信箱模式

```
+mat_letterbox_aspect_min 1.0
```

**作用**：控制画面信箱模式（黑边）的最小宽高比。

**常用值**：

| 值 | 效果 |
|----|------|
| 0.0 | 禁用信箱模式 |
| 1.0 | 启用超宽视野支持 |
| 1.333 | 4:3 比例 |
| 1.777 | 16:9 比例 |

**配合使用**：
```
+mat_letterbox_aspect_min 1.0 +cl_fovScale "2"
```
可启用超宽视野（120°+）

#### `+m_rawinput` 原始鼠标输入

```
+m_rawinput 1
```

**作用**：绕过Windows鼠标设置，直接读取鼠标数据。

**效果**：
- 减少输入延迟
- 消除鼠标加速影响
- 更精准的控制

**推荐**：竞技玩家建议开启

### 图形参数

#### `-w` `-h` 窗口大小

```
-w 1920 -h 1080
```

**作用**：设置游戏窗口的宽度和高度。

**注意**：
- 需要配合 `-windowed` 使用
- 分辨率不应超过显示器分辨率

#### `-noborder` 无边框

```
-noborder -windowed -w 1920 -h 1080
```

**作用**：无边框窗口模式，方便多任务切换。

---

## 推荐配置

### 当前使用配置（参考）

**180帧配置**：
```
-dev +fps_max 180 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -novid
```

**279帧配置**：
```
-dev +fps_max 279 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale "2" -w 2560 -h 1600 -novid
```

**参数说明**：

| 参数 | 值 | 说明 |
|------|-----|------|
| `-dev` | - | 开发者模式 |
| `+fps_max` | 180/279 | 帧率限制 |
| `+cl_showpos` | 1 | 显示位置/速度 |
| `+mat_letterbox_aspect_min` | 1.0 | 启用超宽FOV |
| `+cl_fovScale` | "2" | 超宽视野 |
| `-w` `-h` | 2560x1600 | 窗口分辨率 |
| `-novid` | - | 跳过开场视频 |

### 竞技优化配置

```
+exec autoexec.cfg -high -threads 8 -novid -nojoy -freq 144
```

**说明**：
- `+exec autoexec.cfg` - 执行自定义配置
- `-high` - 高优先级
- `-threads 8` - 8线程（根据CPU调整）
- `-novid` - 跳过视频
- `-nojoy` - 禁用手柄
- `-freq 144` - 144Hz刷新率

### 性能优先配置

```
-high -threads 8 -novid -nojoy -freq 144 -dxlevel 95
```

### 低端配置优化

```
-high -threads 4 -novid -nojoy -freq 60 -w 1280 -h 720 -windowed -noborder
```

**说明**：
- 降低分辨率到 1280x720
- 窗口化运行
- 4线程限制

### 调试配置

```
-console -dev -condebug +exec debug.cfg
```

---

## 数据模型

### LaunchOptions

```typescript
interface LaunchOptions {
  // 性能参数
  high?: boolean;
  threads?: number;
  freq?: number;
  refresh?: number;
  
  // 视频参数
  novid?: boolean;
  nojoy?: boolean;
  
  // 窗口参数
  windowed?: boolean;
  noborder?: boolean;
  fullscreen?: boolean;
  width?: number;
  height?: number;
  x?: number;
  y?: number;
  
  // 配置参数
  exec?: string;
  cfg?: string;
  
  // 游戏控制参数
  fps_max?: number;
  cl_showfps?: 0 | 1 | 2;
  cl_showpos?: 0 | 1;
  cl_fovScale?: number;
  mat_letterbox_aspect_min?: number;
  mat_letterbox_aspect_max?: number;
  m_rawinput?: 0 | 1;
  m_acceleration?: 0 | 1;
  
  // 网络参数
  cl_interp?: number;
  cl_interp_ratio?: number;
  cl_cmdrate?: number;
  cl_updaterate?: number;
  
  // 图形参数
  dxlevel?: 80 | 81 | 90 | 95;
  
  // 调试参数
  dev?: boolean;
  console?: boolean;
  condebug?: boolean;
}
```

### CurrentConfig（当前配置）

```typescript
// 当前使用的配置
const currentConfig180: LaunchOptions = {
  dev: true,
  fps_max: 180,
  cl_showpos: 1,
  mat_letterbox_aspect_min: 1.0,
  cl_fovScale: 2,
  novid: true
};

const currentConfig279: LaunchOptions = {
  dev: true,
  fps_max: 279,
  cl_showpos: 1,
  mat_letterbox_aspect_min: 1.0,
  cl_fovScale: 2,
  width: 2560,
  height: 1600,
  novid: true
};
```

### 参数类型

```typescript
type SwitchParam = `-high` | `-novid` | `-nojoy` | `-console` | `-dev`;

type NumberParam = 
  | `-threads ${number}`
  | `-freq ${number}`
  | `-w ${number}`
  | `-h ${number}`
  | `+fps_max ${number}`
  | `+cl_fovScale ${number}`;

type CommandParam = 
  | `+exec ${string}`
  | `+host_writeconfig`
  | `+cl_showfps ${0|1|2}`
  | `+cl_showpos ${0|1}`;
```

---

## 参数优先级

```
启动参数 > autoexec.cfg > settings.cfg > videoconfig.txt > 游戏默认值
```

**说明**：
- 启动参数优先级最高
- 可以覆盖配置文件中的设置
- 游戏内设置会覆盖启动参数（非只读情况）

---

## 注意事项

### ⚠️ 常见问题

1. **参数不生效**
   - 检查参数拼写
   - 确认 Steam/EA App 正确保存
   - 某些参数可能被游戏更新移除

2. **游戏崩溃**
   - 移除所有参数测试
   - 逐个添加参数排查
   - 检查 autoexec.cfg 语法

3. **配置文件不执行**
   - 确认 `autoexec.cfg` 文件位置正确
   - 检查文件编码（UTF-8 无 BOM）
   - 添加 `host_writeconfig` 保存

4. **刷新率不生效**
   - 确认显示器支持该刷新率
   - 在 Windows 显示设置中启用
   - 使用 `-freq` 而非 `-refresh`

### ✅ 最佳实践

1. **备份配置**：修改前备份 `settings.cfg` 和 `videoconfig.txt`
2. **逐步测试**：添加参数后测试稳定性
3. **文档记录**：记录使用的参数和效果
4. **版本更新**：游戏更新后重新验证参数有效性

---

## 相关资源

- [videoconfig.md](videoconfig.md) - 视频配置详解
- [settings.md](settings.md) - 游戏设置详解
- [Steam 启动选项文档](https://developer.valvesoftware.com/wiki/Command_line_options)
- [Source 引擎参数](https://developer.valvesoftware.com/wiki/Source引擎)

---
**创建时间**: 2026-07-31
**协议版本**: 1.0