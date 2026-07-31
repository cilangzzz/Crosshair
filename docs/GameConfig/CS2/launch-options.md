# CS2 启动参数详解

## 概述

启动参数（Launch Options）是在游戏启动时传递给可执行文件的命令行参数，用于控制游戏行为、优化性能、执行配置脚本。

---

## 设置位置

### Steam 版本

1. 打开 Steam 库
2. 右键 Counter-Strike 2 → 属性
3. 在"启动选项"文本框中输入参数
4. 格式：`参数1 参数2 参数3`

**位置**：
```
Steam 库 → Counter-Strike 2 → 右键 → 属性 → 启动选项
```

---

## 启动参数索引

### 一、性能优化参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-high` | 开关 | 关 | 高优先级进程 |
| `-threads` | 数值 | 自动 | CPU线程数 |
| `-freq` | 数值 | 60 | 强制刷新率 (Hz) |
| `-refresh` | 数值 | 60 | 同 -freq |
| `-novid` | 开关 | 关 | 跳过开场视频 |
| `-nojoy` | 开关 | 关 | 禁用摇杆 |
| `-noborder` | 开关 | 关 | 无边框窗口 |
| `-windowed` | 开关 | 关 | 窗口模式 |
| `-fullscreen` | 开关 | 关 | 全屏模式 |
| `-w` | 数值 | 默认 | 窗口宽度 |
| `-h` | 数值 | 默认 | 窗口高度 |

### 二、配置执行参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+exec` | 命令 | 无 | 执行配置文件 |
| `+host_writeconfig` | 命令 | 无 | 保存配置 |
| `+fps_max` | 数值 | 400 | 最大帧率 |
| `+clientport` | 数值 | 27005 | 客户端端口 |

### 三、网络参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-tcp` | 开关 | UDP | TCP连接 |
| `+cl_cmdrate` | 数值 | 64 | 命令速率 |
| `+cl_updaterate` | 数值 | 64 | 更新速率 |
| `+rate` | 数值 | 786432 | 最大带宽 |
| `+cl_interp` | 数值 | 0 | 插值 |
| `+cl_interp_ratio` | 数值 | 2 | 插值比率 |

### 四、调试参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-console` | 开关 | 关 | 启用控制台 |
| `-dev` | 开关 | 关 | 开发者模式 |
| `-condebug` | 开关 | 关 | 控制台日志 |
| `-conclearlog` | 开关 | 关 | 清除日志 |
| `-toconsole` | 开关 | 关 | 输出到控制台 |

### 五、系统参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-dxlevel` | 数值 | 95 | DirectX级别 |
| `-heapsize` | 数值 | 自动 | 堆内存 (KB) |
| `-nohltv` | 开关 | 关 | 禁用GOTV |
| `-noservergamelogic` | 开关 | 关 | 禁用服务器逻辑 |

### 六、游戏控制参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `+cl_showfps` | 开关 | 0 | 显示FPS |
| `+cl_showpos` | 开关 | 0 | 显示位置/速度 |
| `+net_graph` | 开关 | 0 | 显示网络图 |
| `+sensitivity` | 数值 | 6.0 | 鼠标灵敏度 |
| `+m_rawinput` | 开关 | 0 | 原始鼠标输入 |
| `+volume` | 数值 | 0.4 | 主音量 |

---

## 参数详细说明

### 性能优化参数

#### `-high` 高优先级

```
-high
```

**作用**：将游戏进程设置为高优先级，减少CPU争抢。

**适用场景**：
- 多任务环境
- 后台有其他程序

**注意**：可能导致系统响应变慢

#### `-threads` 线程数

```
-threads 8
```

**推荐值**：

| CPU核心数 | 推荐值 |
|-----------|--------|
| 4核 | `-threads 4` |
| 6核 | `-threads 6` |
| 8核 | `-threads 8` |
| 12核+ | `-threads 8` |

**注意**：设置过多可能导致性能下降

#### `-freq` / `-refresh` 刷新率

```
-freq 144
-refresh 240
```

**推荐值**：

| 显示器 | 参数 |
|--------|------|
| 60Hz | `-freq 60` |
| 120Hz | `-freq 120` |
| 144Hz | `-freq 144` |
| 165Hz | `-freq 165` |
| 240Hz | `-freq 240` |

#### `-novid` 跳过视频

```
-novid
```

**作用**：跳过开场动画，加快启动速度约2-3秒。

### 配置执行参数

#### `+exec` 执行配置

```
+exec autoexec.cfg
```

**作用**：启动时执行指定的配置文件。

**配置文件位置**：
```
Steam\userdata\<SteamID3>\730\local\cfg\autoexec.cfg
```

**autoexec.cfg 示例**：
```cfg
// 帧率设置
fps_max "0"

// 网络设置
rate "128000"
cl_cmdrate "128"
cl_updaterate "128"
cl_interp "0"
cl_interp_ratio "1"

// 鼠标设置
m_rawinput "1"
sensitivity "1.5"

// 准心设置
cl_crosshairsize "2"
cl_crosshairthickness "1"
cl_crosshairgap "-2"
cl_crosshaircolor "1"

// 买枪绑定
bind "kp_ins" "buy ak47; buy m4a1"

echo "autoexec.cfg loaded"
host_writeconfig
```

#### `+fps_max` 帧率限制

```
+fps_max 0
+fps_max 400
```

**推荐值**：

| 场景 | 推荐值 |
|------|--------|
| 竞技 | `+fps_max 0` 或 `+fps_max 400` |
| 144Hz | `+fps_max 154` |
| 240Hz | `+fps_max 254` |

### 网络参数

#### `+cl_cmdrate` / `+cl_updaterate`

```
+cl_cmdrate 128 +cl_updaterate 128
```

**服务器 Tick Rate**：

| 服务器类型 | 推荐值 |
|------------|--------|
| 64 tick | `+cl_cmdrate 64` |
| 128 tick | `+cl_cmdrate 128` |

**Faceit / 5E / B5 平台**：使用 128 tick

---

## 推荐配置

### 竞技优化配置

```
-novid -high -threads 8 -freq 144 +exec autoexec.cfg
```

**说明**：
- `-novid` - 跳过视频
- `-high` - 高优先级
- `-threads 8` - 8线程
- `-freq 144` - 144Hz刷新率
- `+exec autoexec.cfg` - 执行自定义配置

### 高刷新率配置

```
-novid -high -freq 240 +fps_max 0 +exec autoexec.cfg
```

### 控制台调试配置

```
-console -dev +cl_showfps 1 +cl_showpos 1
```

### 低端配置优化

```
-novid -threads 4 -freq 60 -w 1280 -h 720 -windowed -noborder +fps_max 60
```

### 全网络优化配置

```
-novid +rate 128000 +cl_cmdrate 128 +cl_updaterate 128 +cl_interp 0 +cl_interp_ratio 1 +exec network.cfg
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
  
  // 配置参数
  exec?: string;
  fps_max?: number;
  clientport?: number;
  
  // 网络参数
  rate?: number;
  cl_cmdrate?: 64 | 128;
  cl_updaterate?: 64 | 128;
  cl_interp?: number;
  cl_interp_ratio?: number;
  
  // 调试参数
  dev?: boolean;
  console?: boolean;
  condebug?: boolean;
  
  // 游戏控制
  cl_showfps?: 0 | 1;
  cl_showpos?: 0 | 1;
  net_graph?: 0 | 1 | 2 | 3;
  sensitivity?: number;
  m_rawinput?: 0 | 1;
  volume?: number;
}
```

### 参数类型

```typescript
type SwitchParam = `-high` | `-novid` | `-nojoy` | `-console` | `-dev`;

type NumberParam = 
  | `-threads ${number}`
  | `-freq ${number}`
  | `-w ${number}`
  | `-h ${number}`
  | `+fps_max ${number}`;

type CommandParam = 
  | `+exec ${string}`
  | `+host_writeconfig`
  | `+cl_cmdrate ${64|128}`
  | `+cl_updaterate ${64|128}`;
```

---

## 参数优先级

```
启动参数 > autoexec.cfg > config.cfg > 游戏内设置 > 默认值
```

---

## 注意事项

### ⚠️ 常见问题

1. **参数不生效**
   - 检查拼写
   - 确认 Steam 保存
   - 游戏更新可能移除某些参数

2. **配置文件不执行**
   - 确认文件路径正确
   - 检查文件编码（UTF-8 无 BOM）
   - 添加 `host_writeconfig` 保存

3. **刷新率不生效**
   - 确认显示器支持
   - Windows 显示设置中启用

### ✅ 最佳实践

1. **备份配置**：修改前备份 `config.cfg`
2. **逐步测试**：添加参数后测试稳定性
3. **使用 autoexec.cfg**：将常用设置放入配置文件
4. **定期更新**：游戏更新后验证参数有效性

---

## 相关资源

- [config.md](config.md) - 游戏配置详解
- [video.md](video.md) - 视频配置详解
- [Total CS2 Commands](https://totalcsgo.com/command)
- [Valve Developer Wiki](https://developer.valvesoftware.com/wiki/Counter-Strike_2)

---
**创建时间**: 2026-07-31
**协议版本**: 1.0