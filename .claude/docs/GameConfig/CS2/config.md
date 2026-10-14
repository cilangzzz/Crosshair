# CS2 config.cfg 游戏配置详解

## 文件格式

```cfg
// 注释
bind "KEY" "COMMAND"
setting_name "value"
```

- **编码**: UTF-8 (无 BOM)
- **格式**: Valve CFG 文本格式
- **注释**: 使用 `//` 单行注释

---

## 配置选项索引

### 一、键位绑定

#### 1.1 绑定语法

```cfg
bind "KEY" "COMMAND"
bind "KEY" "COMMAND1; COMMAND2"
```

#### 1.2 按键名称表

**字母键**：
```
a b c d e f g h i j k l m n o p q r s t u v w x y z
```

**数字键**：
```
0 1 2 3 4 5 6 7 8 9
```

**功能键**：
```
F1 F2 F3 F4 F5 F6 F7 F8 F9 F10 F11 F12
```

**特殊键**：

| 名称 | 说明 |
|------|------|
| `SPACE` | 空格 |
| `TAB` | Tab |
| `ESCAPE` | Esc |
| `ENTER` | 回车 |
| `SHIFT` | Shift |
| `CTRL` | Ctrl |
| `ALT` | Alt |
| `BACKSPACE` | 退格 |
| `INS` | Insert |
| `DEL` | Delete |
| `HOME` | Home |
| `END` | End |
| `PGUP` | Page Up |
| `PGDN` | Page Down |
| `UPARROW` `DOWNARROW` `LEFTARROW` `RIGHTARROW` | 方向键 |

**鼠标键**：

| 名称 | 说明 |
|------|------|
| `MOUSE1` | 左键 |
| `MOUSE2` | 右键 |
| `MOUSE3` | 中键 |
| `MOUSE4` | 侧键1 |
| `MOUSE5` | 侧键2 |
| `MWHEELUP` | 滚轮上 |
| `MWHEELDOWN` | 滚轮下 |

**小键盘**：

| 名称 | 说明 |
|------|------|
| `KP_INS` | 0 |
| `KP_END` | 1 |
| `KP_DOWNARROW` | 2 |
| `KP_PGDN` | 3 |
| `KP_LEFTARROW` | 4 |
| `KP_5` | 5 |
| `KP_RIGHTARROW` | 6 |
| `KP_HOME` | 7 |
| `KP_UPARROW` | 8 |
| `KP_PGUP` | 9 |
| `KP_SLASH` | / |
| `KP_MULTIPLY` | * |
| `KP_MINUS` | - |
| `KP_PLUS` | + |
| `KP_DEL` | . |

---

### 二、鼠标设置

#### 2.1 灵敏度

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `sensitivity` | float | 6.0 | 鼠标灵敏度 |
| `zoom_sensitivity_ratio` | float | 1.0 | 狙击镜灵敏度比例 |
| `m_rawinput` | int | 1 | 原始输入 (0=关, 1=开) |
| `m_customaccel` | int | 0 | 自定义加速 |
| `m_mouseaccel1` | float | 0 | 加速阈值1 |
| `m_mouseaccel2` | float | 0 | 加速阈值2 |
| `m_mousespeed` | int | 1 | Windows加速 |

#### 2.2 DPI 与 eDPI 计算

```
eDPI = DPI × sensitivity
```

**推荐 eDPI 范围**：
- 低灵敏度：400-800 eDPI
- 中灵敏度：800-1200 eDPI
- 高灵敏度：1200+ eDPI

---

### 三、准心设置

#### 3.1 基本设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `cl_crosshairsize` | float | 5 | 准心大小 |
| `cl_crosshairthickness` | float | 0.5 | 准心粗细 |
| `cl_crosshairgap` | float | 0 | 准心间隙 |
| `cl_crosshaircolor` | int | 1 | 准心颜色 |
| `cl_crosshairalpha` | int | 200 | 准心透明度 (0-255) |
| `cl_crosshairdot` | int | 0 | 准心中心点 |
| `cl_crosshair_drawoutline` | int | 0 | 准心描边 |
| `cl_crosshair_outlinethickness` | float | 1 | 描边粗细 |
| `cl_crosshairusealpha` | int | 1 | 使用透明度 |

#### 3.2 准心颜色值

| 值 | 颜色 |
|----|------|
| 0 | 红色 |
| 1 | 绿色 |
| 2 | 黄色 |
| 3 | 蓝色 |
| 4 | 青色 |
| 5 | 自定义 |

#### 3.3 自定义颜色

```cfg
cl_crosshaircolor 5
cl_crosshaircolor_r 255
cl_crosshaircolor_g 0
cl_crosshaircolor_b 0
```

#### 3.4 准心样式参考

**经典准心**：
```cfg
cl_crosshairsize "2"
cl_crosshairthickness "1"
cl_crosshairgap "0"
cl_crosshaircolor "1"
cl_crosshairdot "0"
```

**点准心**：
```cfg
cl_crosshairsize "0"
cl_crosshairthickness "2"
cl_crosshairgap "0"
cl_crosshairdot "1"
```

**四点准心**：
```cfg
cl_crosshairsize "2"
cl_crosshairthickness "1"
cl_crosshairgap "-2"
cl_crosshairdot "0"
```

---

### 四、网络设置

#### 4.1 基本网络

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `cl_cmdrate` | int | 64 | 命令速率 (64/128) |
| `cl_updaterate` | int | 64 | 更新速率 (64/128) |
| `cl_interp` | float | 0 | 插值 |
| `cl_interp_ratio` | int | 2 | 插值比率 |
| `rate` | int | 786432 | 最大带宽 (字节/秒) |

#### 4.2 竞技网络配置

**128 tick 服务器配置**：
```cfg
rate "128000"
cl_cmdrate "128"
cl_updaterate "128"
cl_interp "0"
cl_interp_ratio "1"
```

**64 tick 服务器配置**：
```cfg
rate "786432"
cl_cmdrate "64"
cl_updaterate "64"
cl_interp "0"
cl_interp_ratio "1"
```

---

### 五、帧率设置

#### 5.1 帧率控制

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `fps_max` | int | 400 | 最大帧率 (0=无限制) |
| `fps_max_menu` | int | 120 | 菜单帧率（CS2已移除） |
| `fps_max_ui` | int | 120 | UI帧率 |

#### 5.2 推荐设置

```cfg
// 无限制帧率
fps_max "0"

// 或设置为显示器刷新率 + 10
fps_max "154"  // 144Hz显示器
fps_max "254"  // 240Hz显示器
```

---

### 六、音频设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `volume` | float | 0.4 | 主音量 |
| `snd_musicvolume` | float | 0.1 | 音乐音量 |
| `snd_menumusic_volume` | float | 0.1 | 菜单音乐 |
| `snd_roundend_volume` | float | 0.1 | 回合结束音乐 |
| `snd_roundstart_volume` | float | 0.1 | 回合开始音乐 |
| `snd_tensecondwarning_volume` | float | 0.2 | 10秒警告音量 |
| `snd_mute_losefocus` | int | 1 | 失去焦点静音 |
| `snd_headphone_pan_exponent` | float | 1 | 耳机声像指数 |
| `snd_headphone_pan_radial_weight` | float | 1 | 耳机声像权重 |
| `snd_mixahead` | float | 0.1 | 音频混合延迟 |

---

### 七、HUD 设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `hud_scaling` | float | 0.95 | HUD缩放 |
| `hud_showtargetid` | int | 1 | 显示目标ID |
| `cl_hud_color` | int | 0 | HUD颜色 |
| `cl_hud_healthammo_style` | int | 0 | 血量弹药样式 |
| `cl_showloadout` | int | 1 | 显示装备栏 |

#### HUD 颜色值

| 值 | 颜色 |
|----|------|
| 0 | 默认 |
| 1 | 白色 |
| 2 | 浅蓝 |
| 3 | 深蓝 |
| 4 | 紫色 |
| 5 | 红色 |
| 6 | 橙色 |
| 7 | 黄色 |
| 8 | 绿色 |
| 9 | 浅绿 |
| 10 | 粉色 |

---

### 八、雷达设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `cl_radar_scale` | float | 0.7 | 雷达缩放 |
| `cl_radar_icon_scale_min` | float | 0.6 | 图标最小缩放 |
| `cl_radar_square_with_scoreboard` | int | 1 | 计分板时雷达方形 |
| `cl_hud_radar_scale` | float | 1 | 雷达大小 |
| `cl_radar_always_centered` | int | 1 | 雷达居中 |
| `cl_radar_rotate` | int | 1 | 雷达旋转 |

**推荐雷达配置**：
```cfg
cl_radar_scale "0.3"
cl_radar_icon_scale_min "1"
cl_radar_always_centered "0"
```

---

### 九、视角设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `viewmodel_fov` | int | 60 | 视野 (54-68) |
| `viewmodel_offset_x` | float | 0 | X偏移 |
| `viewmodel_offset_y` | float | 0 | Y偏移 |
| `viewmodel_offset_z` | float | 0 | Z偏移 |
| `viewmodel_presetpos` | int | 1 | 预设位置 |
| `cl_viewmodel_shift_left_amt` | float | 1.5 | 左移量 |
| `cl_viewmodel_shift_right_amt` | float | 0.75 | 右移量 |

#### 预设位置

| 值 | 位置 |
|----|------|
| 1 | 经典 |
| 2 | 居中 |
| 3 | 经典（偏右） |

---

### 十、其他设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `cl_showfps` | int | 0 | 显示FPS |
| `cl_showpos` | int | 0 | 显示位置 |
| `net_graph` | int | 0 | 显示网络图 |
| `net_graphpos` | int | 1 | 网络图位置 |
| `net_graphproportionalfont` | int | 1 | 网络图比例字体 |
| `cl_righthand` | int | 1 | 右手持枪 |
| `cl_autohelp` | int | 1 | 自动帮助 |
| `cl_showhelp` | int | 1 | 显示帮助 |
| `cl_disablefreezecam` | int | 0 | 禁用死亡回放 |
| `cl_freezecampanel_brutal_gonner_cycle` | int | 0 | 死亡回放循环 |

---

## 买枪绑定

### 小键盘买枪

```cfg
// 主武器
bind "kp_ins" "buy ak47; buy m4a1"
bind "kp_del" "buy awp"
bind "kp_end" "buy galilar; buy famas"
bind "kp_downarrow" "buy sg556; buy aug"

// 冲锋枪
bind "kp_pgdn" "buy mp9; buy mac10"
bind "kp_leftarrow" "buy p90"
bind "kp_5" "buy mp7"

// 步枪
bind "kp_rightarrow" "buy ssg08"

// 手枪
bind "kp_home" "buy deagle"
bind "kp_uparrow" "buy glock; buy usp"
bind "kp_pgup" "buy p250"

// 装备
bind "kp_slash" "buy flashbang"
bind "kp_multiply" "buy smokegrenade"
bind "kp_minus" "buy hegrenade"
bind "kp_plus" "buy vesthelm; buy vest"
```

---

## 配置模板

### 竞技配置

```cfg
// 鼠标设置
m_rawinput "1"
sensitivity "1.5"

// 帧率设置
fps_max "0"

// 网络设置
rate "128000"
cl_cmdrate "128"
cl_updaterate "128"
cl_interp "0"
cl_interp_ratio "1"

// 准心设置
cl_crosshairsize "2"
cl_crosshairthickness "1"
cl_crosshairgap "-2"
cl_crosshaircolor "1"

// 音频设置
volume "0.4"
snd_musicvolume "0"

// HUD设置
hud_scaling "0.95"
cl_hud_color "5"

// 雷达设置
cl_radar_scale "0.3"
cl_radar_icon_scale_min "1"
cl_radar_always_centered "0"

echo "Competitive config loaded"
```

### 自动执行配置

```cfg
// 执行自动配置
exec autoexec

// 或在启动参数添加
// +exec autoexec.cfg
```

---

## 注意事项

1. **分号分隔**：多个命令用分号连接 `command1; command2`
2. **引号包裹**：键位和命令需要用双引号
3. **控制台执行**：修改后需执行 `host_writeconfig` 保存
4. **编码格式**：UTF-8 无 BOM

---
**创建时间**: 2026-07-31
**协议版本**: 1.0