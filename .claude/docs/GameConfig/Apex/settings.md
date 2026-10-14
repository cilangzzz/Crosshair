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
| `LSHIFT` / `RSHIFT` | 左/右 Shift |
| `LCTRL` / `RCTRL` | 左/右 Ctrl |
| `LALT` / `RALT` | 左/右 Alt |
| `INS` `DEL` `HOME` `END` `PGUP` `PGDN` | 编辑区按键 |
| `UP` `DOWN` `LEFT` `RIGHT` | 方向键 |
| `` ` `` | 反引号（控制台） |
| `[` `]` | 方括号 |

**鼠标键**：

| 名称 | 说明 |
|------|------|
| `MOUSE1` / `MOUSE2` / `MOUSE3` | 左键 / 右键 / 中键 |
| `MOUSE4` / `MOUSE5` | 侧键 1 / 侧键 2 |
| `MWHEELUP` / `MWHEELDOWN` | 滚轮向上 / 向下 |

**小键盘**：

| 名称 | 说明 |
|------|------|
| `KP_END` `KP_DOWNARROW` `KP_PGDN` | 小键盘 1/2/3 |
| `KP_LEFTARROW` `KP_5` `KP_RIGHTARROW` | 小键盘 4/5/6 |
| `KP_HOME` `KP_UPARROW` `KP_PGUP` | 小键盘 7/8/9 |
| `KP_SLASH` `KP_MULTIPLY` `KP_MINUS` `KP_PLUS` `KP_DEL` | / * - + . |

**手柄键**：

| 名称 | 说明 |
|------|------|
| `A_BUTTON` `B_BUTTON` `X_BUTTON` `Y_BUTTON` | 面板键 |
| `L_SHOULDER` / `R_SHOULDER` | LB/RB |
| `L_TRIGGER` / `R_TRIGGER` | LT/RT |
| `STICK1` / `STICK2` | 左/右摇杆按下 |
| `BACK` / `START` | 功能键 |
| `UP` `DOWN` `LEFT` `RIGHT` | 方向键 |

#### 1.3 游戏命令表

**移动命令**：

| 命令 | 说明 |
|------|------|
| `+forward` / `+backward` | 前/后移动 |
| `+moveleft` / `+moveright` | 左/右移动 |
| `+jump` / `+duck` | 跳跃 / 蹲下 |
| `+speed` / `+strafe` | 冲刺 / 平移 |

**战斗命令**：

| 命令 | 说明 |
|------|------|
| `+attack` / `+toggle_zoom` | 射击 / 瞄准 |
| `+reload` / `+melee` | 装弹 / 近战 |
| `+offhand1` / `+offhand4` | 战术技能 / 终极技能 |
| `weapon_inspect` | 检视武器 |

**武器选择**：

| 命令 | 说明 |
|------|------|
| `weaponSelectPrimary0` / `weaponSelectPrimary1` / `weaponSelectPrimary2` | 主武器 1/2/3 |
| `weaponSelectOrdnance` | 投掷武器 |
| `+weaponCycle` | 循环切换武器 |

**物品使用**：

| 命令 | 说明 |
|------|------|
| `use_consumable HEALTH_SMALL` / `HEALTH_LARGE` | 小/大血包 |
| `use_consumable SHIELD_SMALL` / `SHIELD_LARGE` | 小/大电池 |
| `use_consumable PHOENIX_KIT` | 凤凰包 |

**交互与标记**：

| 命令 | 说明 |
|------|------|
| `+use` / `+use_long` / `+use_alt` | 交互 / 长按交互 / 备用交互 |
| `+ping` / `ping_specific_type ENEMY` | 标记 / 标记敌人 |

**界面命令**：

| 命令 | 说明 |
|------|------|
| `toggle_inventory` / `toggle_map` | 库存 / 地图 |
| `ingamemenu_activate` | 游戏菜单 |
| `say_team` / `chat_wheel` | 队伍聊天 / 快捷消息轮盘 |
| `+pushtotalk` / `toggleconsole` / `screenshotDevNet` | 按键说话 / 控制台 / 截图 |

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
| `mouse_zoomed_sensitivity_scalar_0` ~ `_7` | float | 1.0 | 依次对应 1倍(红点/全息)、2倍、3倍、4倍、6倍、8倍、10倍、变焦镜 |

---

### 三、帧率 / 视野 / 网络

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `fps_max` | int | 0 | 最大帧率 (0=无限制) |
| `fps_max_menu` | int | 60 | 菜单帧率限制 |
| `cl_showfps` | int | 0 | 显示FPS (0=关, 1=简单, 2=详细) |
| `cl_showpos` | int | 0 | 显示位置信息 |
| `cl_fovScale` | float | 1.27 | FOV缩放 (1.0-1.35, 约90-120度) |
| `cl_viewpitchscale` | float | 0.95 | 垂直视角缩放 |
| `cl_interp` | float | 0.015 | 网络插值 |
| `cl_interp_ratio` | int | 1 | 插值比率 |
| `cl_cmdrate` | int | 60 | 命令速率 |
| `cl_updaterate` | int | 60 | 更新速率 |

---

### 四、音频设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `sound_num_speakers` | int | 2 | 扬声器数量 (2/6/8) |
| `sound_volume_voice` | float | 1.0 | 语音音量 (0.0-1.0) |
| `miles_channels` | int | 0 | 音频通道 |
| `miles_output_device` | string | "" | 输出设备 |

### 五、语音设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `VoiceChatMode` | int | 0 | 语音聊天模式 |
| `voice_forcemicrecord` | int | 1 | 强制麦克风录音 |
| `voice_input_device` | string | "" | 输入设备 |
| `voice_mixer_boost` / `voice_mixer_mute` | int | 0 | 麦克风增益 / 静音 |
| `voice_mixer_volume` | float | 1.0 | 麦克风音量 |
| `voice_modenable` / `voice_vox` | int | 1 | 语音调制 / VOX 模式 |
| `voice_scale` | float | 1.0 | 语音缩放 |

### 六、图形设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `gfx_amdUseLowLatency` | int | 1 | AMD 低延迟模式 |
| `gfx_nvnUseLowLatency` | int | 0 | NVIDIA 低延迟模式 |
| `gfx_nvnUseLowLatencyBoost` | int | 0 | NVIDIA 低延迟增强 |
| `chroma_enable` | int | 0 | Razer Chroma 灯光 |

### 七、其他设置

| 设置项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `name` | string | "" | 玩家名称 |
| `ui_layout_mode` | int | 0 | UI 布局模式 |
| `sv_voiceenable` | int | 1 | 语音启用 |
| `sv_specaccelerate` / `sv_specspeed` | float | 1000.0 / 5.0 | 观战加速 / 观战速度 |
| `sv_specnoclip` | int | 1 | 观战穿墙 |
| `cc_linger_time` / `cc_predisplay_time` | float | 1.0 / 0.25 | 字幕持续时间 / 预显示时间 |
| `func_break_max_pieces` | int | 15 | 最大破碎碎片数 |
| `lookspring` / `lookstrafe` | int | 0 | 自动回正 / 平视移动 |
| `hdr_screenshot_directory` | string | "" | HDR 截图目录 |

---

## 应用侧序列化范围（重要）

`ApexConfigService` 保存时**整体重写** settings.cfg，但只输出以下 17 行，**不包含任何 `bind_*` 键位绑定**：

```
mouse_sensitivity、m_acceleration、
mouse_use_per_scope_sensitivity_scalars、mouse_zoomed_sensitivity_scalar_0 ~ _7、
fps_max、cl_showfps、cl_showpos、cl_fovScale、cl_interp、cl_interp_ratio
```

读取（解析）同样只识别这 17 项；其余设置虽在 `ApexSettingsConfig` 中建模，但不参与解析/序列化（加载后保持模型默认值），仅键位绑定完全不建模。**加载→保存一个真实 settings.cfg 会清掉键位绑定与未参与读写的设置**，务必先备份，详见 [pitfalls.md](pitfalls.md)。

---

## 配置模板（游戏协议参考）

```cfg
// 鼠标设置
mouse_sensitivity "1.0"
m_acceleration "0"

// 瞄准镜灵敏度
mouse_use_per_scope_sensitivity_scalars "1"
mouse_zoomed_sensitivity_scalar_0 "0.9"

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
```

## 键位绑定示例

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
bind_US_standard "7" "use_consumable SHIELD_SMALL" 0
bind_US_standard "9" "use_consumable PHOENIX_KIT" 0

// 交互
bind_US_standard "e" "+use; +use_long" 0
bind_US_standard "f" "ping_specific_type ENEMY" 0
bind_US_standard "MOUSE3" "+ping" 0

// 界面
bind_US_standard "TAB" "toggle_inventory" 0
bind_US_standard "m" "toggle_map" 0
bind_US_standard "t" "+pushtotalk" 0
```

---

## 注意事项

1. **分号分隔**：多个命令可用分号连接 `"+use; +use_long"`
2. **加号前缀**：持续动作需用 `+` 前缀，如 `+forward`
3. **标志字段**：`0`=普通绑定, `1`=覆盖已有绑定
4. **编码格式**：保存为 UTF-8 无 BOM 格式

---
**协议版本**: 1.1
