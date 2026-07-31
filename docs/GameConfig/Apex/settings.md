# settings.cfg 游戏设置详解

## 文件格式

```cfg
bind_US_standard "key" "command" flags
setting_name "value"
```

- **编码**: UTF-8 (无 BOM)
- **格式**: Valve CFG 文本格式
- **行分隔**: 换行符 `\n`

---

## 配置选项索引

### 一、键位绑定

#### 1.1 绑定语法

```cfg
bind_US_standard "KEY" "COMMAND" FLAGS
bind_held_US_standard "KEY" "COMMAND" FLAGS
```

**参数说明**：

| 参数 | 类型 | 说明 |
|------|------|------|
| `KEY` | string | 按键名称 |
| `COMMAND` | string | 游戏命令 |
| `FLAGS` | int | 绑定标志 (0=普通, 1=覆盖) |

**布局类型**：

| 布局 | 说明 |
|------|------|
| `bind_US_standard` | 美式键盘标准布局 |
| `bind_held_US_standard` | 长按绑定 |

#### 1.2 按键名称表

**字母键**：
```
a b c d e f g h i j k l m n o p q r s t u v w x y z
```

**数字键**：
```
1 2 3 4 5 6 7 8 9 0
```

**功能键**：
```
F1 F2 F3 F4 F5 F6 F7 F8 F9 F10 F11 F12
```

**特殊键**：

| 名称 | 说明 |
|------|------|
| `SPACE` | 空格键 |
| `TAB` | Tab 键 |
| `ESCAPE` | Esc 键 |
| `ENTER` | 回车键 |
| `LSHIFT` | 左 Shift |
| `RSHIFT` | 右 Shift |
| `LCTRL` | 左 Ctrl |
| `RCTRL` | 右 Ctrl |
| `LALT` | 左 Alt |
| `RALT` | 右 Alt |
| `INS` | Insert |
| `DEL` | Delete |
| `HOME` | Home |
| `END` | End |
| `PGUP` | Page Up |
| `PGDN` | Page Down |
| `UP` `DOWN` `LEFT` `RIGHT` | 方向键 |
| `` ` `` | 反引号（控制台） |
| `[` `]` | 方括号 |

**鼠标键**：

| 名称 | 说明 |
|------|------|
| `MOUSE1` | 左键 |
| `MOUSE2` | 右键 |
| `MOUSE3` | 中键 |
| `MOUSE4` | 侧键1 |
| `MOUSE5` | 侧键2 |
| `MWHEELUP` | 滚轮向上 |
| `MWHEELDOWN` | 滚轮向下 |

**小键盘**：

| 名称 | 说明 |
|------|------|
| `KP_END` | 小键盘 1 |
| `KP_DOWNARROW` | 小键盘 2 |
| `KP_PGDN` | 小键盘 3 |
| `KP_LEFTARROW` | 小键盘 4 |
| `KP_5` | 小键盘 5 |
| `KP_RIGHTARROW` | 小键盘 6 |
| `KP_HOME` | 小键盘 7 |
| `KP_UPARROW` | 小键盘 8 |
| `KP_PGUP` | 小键盘 9 |
| `KP_SLASH` | 小键盘 / |
| `KP_MULTIPLY` | 小键盘 * |
| `KP_MINUS` | 小键盘 - |
| `KP_PLUS` | 小键盘 + |
| `KP_DEL` | 小键盘 . |

**手柄键**：

| 名称 | 说明 |
|------|------|
| `A_BUTTON` | A 键 |
| `B_BUTTON` | B 键 |
| `X_BUTTON` | X 键 |
| `Y_BUTTON` | Y 键 |
| `L_SHOULDER` | LB/L1 |
| `R_SHOULDER` | RB/R1 |
| `L_TRIGGER` | LT/L2 |
| `R_TRIGGER` | RT/R2 |
| `STICK1` | 左摇杆按下 |
| `STICK2` | 右摇杆按下 |
| `BACK` | Back/Select |
| `START` | Start |
| `UP` `DOWN` `LEFT` `RIGHT` | 方向键 |

#### 1.3 游戏命令表

**移动命令**：

| 命令 | 说明 |
|------|------|
| `+forward` | 向前移动 |
| `+backward` | 向后移动 |
| `+moveleft` | 向左移动 |
| `+moveright` | 向右移动 |
| `+jump` | 跳跃 |
| `+duck` | 蹲下 |
| `+speed` | 冲刺 |
| `+strafe` | 平移 |

**战斗命令**：

| 命令 | 说明 |
|------|------|
| `+attack` | 射击 |
| `+toggle_zoom` | 瞄准 |
| `+reload` | 装弹 |
| `+melee` | 近战 |
| `+offhand1` | 战术技能 |
| `+offhand4` | 终极技能 |
| `weapon_inspect` | 检视武器 |

**武器选择**：

| 命令 | 说明 |
|------|------|
| `weaponSelectPrimary0` | 主武器 1 |
| `weaponSelectPrimary1` | 主武器 2 |
| `weaponSelectPrimary2` | 主武器 3 |
| `weaponSelectOrdnance` | 投掷武器 |
| `+weaponCycle` | 循环切换武器 |

**物品使用**：

| 命令 | 说明 |
|------|------|
| `use_consumable HEALTH_SMALL` | 小血包 |
| `use_consumable HEALTH_LARGE` | 大血包 |
| `use_consumable SHIELD_SMALL` | 小电池 |
| `use_consumable SHIELD_LARGE` | 大电池 |
| `use_consumable PHOENIX_KIT` | 凤凰包 |

**交互与标记**：

| 命令 | 说明 |
|------|------|
| `+use` | 交互 |
| `+use_long` | 长按交互 |
| `+use_alt` | 备用交互 |
| `+ping` | 标记 |
| `ping_specific_type ENEMY` | 标记敌人 |

**界面命令**：

| 命令 | 说明 |
|------|------|
| `toggle_inventory` | 库存/背包 |
| `toggle_map` | 地图 |
| `ingamemenu_activate` | 游戏菜单 |
| `say_team` | 队伍聊天 |
| `chat_wheel` | 快捷消息轮盘 |

**其他命令**：

| 命令 | 说明 |
|------|------|
| `+pushtotalk` | 按键说话 |
| `toggleconsole` | 控制台 |
| `screenshotDevNet` | 截图 |

---

### 二、鼠标设置

#### 2.1 灵敏度

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `mouse_sensitivity` | float | 1.0 | 鼠标灵敏度 (0.1-10.0) |
| `m_acceleration` | int | 0 | 鼠标加速 (0=关, 1=开) |
| `m_clamp_to_window` | int | 0 | 鼠标限制在窗口 |

#### 2.2 瞄准镜灵敏度

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `mouse_use_per_scope_sensitivity_scalars` | int | 0 | 使用独立瞄准镜灵敏度 |
| `mouse_zoomed_sensitivity_scalar_0` | float | 1.0 | 1倍镜 (红点/全息) |
| `mouse_zoomed_sensitivity_scalar_1` | float | 1.0 | 2倍镜 |
| `mouse_zoomed_sensitivity_scalar_2` | float | 1.0 | 3倍镜 |
| `mouse_zoomed_sensitivity_scalar_3` | float | 1.0 | 4倍镜 |
| `mouse_zoomed_sensitivity_scalar_4` | float | 1.0 | 6倍镜 |
| `mouse_zoomed_sensitivity_scalar_5` | float | 1.0 | 8倍镜 |
| `mouse_zoomed_sensitivity_scalar_6` | float | 1.0 | 10倍镜 |
| `mouse_zoomed_sensitivity_scalar_7` | float | 1.0 | 变焦镜 |

---

### 三、音频设置

#### 3.1 扬声器

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `sound_num_speakers` | int | 2 | 扬声器数量 (2/5.1/7.1) |
| `sound_volume_voice` | float | 1.0 | 语音音量 (0.0-1.0) |

#### 3.2 音频设备

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `miles_channels` | int | 0 | 音频通道 |
| `miles_output_device` | string | "" | 输出设备 |

---

### 四、语音设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `VoiceChatMode` | int | 0 | 语音聊天模式 |
| `voice_forcemicrecord` | int | 1 | 强制麦克风录音 |
| `voice_input_device` | string | "" | 输入设备 |
| `voice_mixer_boost` | int | 0 | 麦克风增益 |
| `voice_mixer_mute` | int | 0 | 麦克风静音 |
| `voice_mixer_volume` | float | 1.0 | 麦克风音量 |
| `voice_modenable` | int | 1 | 语音调制启用 |
| `voice_scale` | float | 1.0 | 语音缩放 |
| `voice_vox` | int | 1 | VOX 模式 |

---

### 五、图形设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `gfx_amdUseLowLatency` | int | 1 | AMD 低延迟模式 |
| `gfx_nvnUseLowLatency` | int | 0 | NVIDIA 低延迟模式 |
| `gfx_nvnUseLowLatencyBoost` | int | 0 | NVIDIA 低延迟增强 |
| `chroma_enable` | int | 0 | Razer Chroma 灯光 |

---

### 六、其他设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `name` | string | "" | 玩家名称 |
| `ui_layout_mode` | int | 0 | UI 布局模式 |
| `sv_voiceenable` | int | 1 | 语音启用 |
| `sv_specaccelerate` | float | 1000.0 | 观战加速 |
| `sv_specnoclip` | int | 1 | 观战穿墙 |
| `sv_specspeed` | float | 5.0 | 观战速度 |
| `cc_linger_time` | float | 1.0 | 闭 captions 持续时间 |
| `cc_predisplay_time` | float | 0.25 | 预显示时间 |
| `func_break_max_pieces` | int | 15 | 最大破碎碎片数 |
| `lookspring` | int | 0 | 自动回正 |
| `lookstrafe` | int | 0 | 平视移动 |
| `hdr_screenshot_directory` | string | "" | HDR 截图目录 |

---

### 七、高级命令（可手动添加）

#### 7.1 帧率设置

```cfg
fps_max "0"              // 最大帧率 (0=无限制)
fps_max_menu "60"        // 菜单帧率限制
cl_showfps "1"           // 显示FPS (0=关, 1=简单, 2=详细)
cl_showpos "0"           // 显示位置信息
```

#### 7.2 视野设置

```cfg
cl_fovScale "1.27"       // FOV缩放 (1.0-1.35, 约90-120度)
cl_viewpitchscale "0.95" // 垂直视角缩放
```

#### 7.3 网络设置

```cfg
cl_interp "0.015"        // 插值
cl_interp_ratio "1"      // 插值比率
cl_cmdrate "60"          // 命令速率
cl_updaterate "60"       // 更新速率
```

#### 7.4 画质优化

```cfg
mat_picmip "-1"          // 纹理质量 (-1=最高)
mat_vsync "0"            // 垂直同步
cl_foliage_scale "0"     // 植被渲染 (0=关闭)
csm_enabled "0"          // 级联阴影
particle_cpu_level "0"   // 粒子效果等级
```

---

## 配置模板

### 竞技配置

```cfg
// 鼠标设置
mouse_sensitivity "1.0"
m_acceleration "0"

// 瞄准镜灵敏度
mouse_use_per_scope_sensitivity_scalars "1"
mouse_zoomed_sensitivity_scalar_0 "0.9"
mouse_zoomed_sensitivity_scalar_1 "1.0"
mouse_zoomed_sensitivity_scalar_2 "1.0"
mouse_zoomed_sensitivity_scalar_3 "1.0"

// 帧率
fps_max "0"
cl_showfps "1"

// 视野
cl_fovScale "1.27"

// 网络
cl_interp "0.015"
cl_interp_ratio "1"
cl_cmdrate "60"
cl_updaterate "60"

// 画质优化
mat_picmip "-1"
mat_vsync "0"
cl_foliage_scale "0"
csm_enabled "0"
particle_cpu_level "0"

// 保存配置
host_writeconfig
```

---

## 键位绑定示例

### 默认键位

```cfg
// 移动
bind_US_standard "w" "+forward" 0
bind_US_standard "s" "+backward" 0
bind_US_standard "a" "+moveleft" 0
bind_US_standard "d" "+moveright" 0
bind_US_standard "SPACE" "+jump" 0
bind_US_standard "LCTRL" "+duck" 0
bind_US_standard "LSHIFT" "+speed" 0

// 战斗
bind_US_standard "MOUSE1" "+attack" 0
bind_US_standard "MOUSE2" "+toggle_zoom" 0
bind_US_standard "r" "+reload" 0
bind_US_standard "q" "+melee" 0
bind_US_standard "x" "+offhand1" 0
bind_US_standard "z" "+offhand4" 0

// 武器选择
bind_US_standard "1" "weaponSelectPrimary0" 0
bind_US_standard "2" "weaponSelectPrimary1" 0
bind_US_standard "3" "weaponSelectPrimary2" 0

// 物品
bind_US_standard "5" "use_consumable HEALTH_SMALL" 0
bind_US_standard "6" "use_consumable HEALTH_LARGE" 0
bind_US_standard "7" "use_consumable SHIELD_SMALL" 0
bind_US_standard "8" "use_consumable SHIELD_LARGE" 0
bind_US_standard "9" "use_consumable PHOENIX_KIT" 0

// 交互
bind_US_standard "e" "+use; +use_long" 0
bind_US_standard "f" "ping_specific_type ENEMY" 0
bind_US_standard "MOUSE3" "+ping" 0

// 界面
bind_US_standard "TAB" "toggle_inventory" 0
bind_US_standard "m" "toggle_map" 0
bind_US_standard "ENTER" "say_team" 0
bind_US_standard "t" "+pushtotalk" 0
bind_US_standard "F1" "chat_wheel" 0
```

---

## 注意事项

1. **分号分隔**：多个命令可用分号连接 `"+use; +use_long"`
2. **加号前缀**：持续动作需用 `+` 前缀，如 `+forward`
3. **标志字段**：`0`=普通绑定, `1`=覆盖已有绑定
4. **编码格式**：保存为 UTF-8 无 BOM 格式

---
**创建时间**: 2026-07-31
**协议版本**: 1.0