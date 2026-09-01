# CS2 配置数据模型

## 数据结构定义

### VideoConfig

视频配置根对象。

```typescript
interface VideoConfig {
  "video": {
    [key: string]: string | number;
  };
}
```

---

## 字段定义

### 一、显示设置 (Display)

#### ResolutionConfig

```typescript
interface ResolutionConfig {
  "setting.defaultres": number;          // 分辨率宽度 (640-3840)
  "setting.defaultresheight": number;    // 分辨率高度 (480-2160)
  "setting.refreshrate": number;         // 刷新率 Hz (60-240)
  "setting.refreshrate_numerator": number;
  "setting.refreshrate_denominator": number;
}
```

#### WindowConfig

```typescript
interface WindowConfig {
  "setting.fullscreen": 0 | 1;           // 全屏模式
  "setting.fullscreen_width": number;
  "setting.fullscreen_height": number;
  "setting.fullscreen_refresh_rate": number;
  "setting.windowed": 0 | 1;             // 窗口模式
  "setting.windowed_width": number;
  "setting.windowed_height": number;
}
```

---

### 二、画质设置 (Quality)

#### TextureConfig

```typescript
interface TextureConfig {
  "setting.mat_picmip": number;          // 纹理质量（CS2已移除）
  "setting.mat_forceaniso": number;      // 各向异性过滤
  "setting.mat_mipmaptextures": 0 | 1;  // Mipmap纹理
}
```

#### ShadowConfig

```typescript
interface ShadowConfig {
  "setting.r_shadows": 0 | 1;            // 阴影开关
  "setting.r_shadowrendertotexture": 0 | 1;
  "setting.r_shadowmaxrendered": number; // 最大渲染阴影数
}
```

#### EffectsConfig

```typescript
interface EffectsConfig {
  "setting.r_drawrain": 0 | 1;           // 雨效果
  "setting.r_drawropes": 0 | 1;          // 绳索渲染
  "setting.r_drawmodeldecals": 0 | 1;    // 模型贴花
  "setting.r_decals": number;            // 贴花数量 (0-256)
  "setting.r_drawtracers_firstperson": 0 | 1; // 第一人称弹道
}
```

#### WaterConfig

```typescript
interface WaterConfig {
  "setting.r_waterforceexpensive": 0 | 1;
  "setting.r_waterforcereflectentities": 0 | 1;
}
```

---

### 三、性能设置 (Performance)

#### VSyncConfig

```typescript
interface VSyncConfig {
  "setting.mat_vsync": 0 | 1;            // 垂直同步
}
```

#### AntiAliasingConfig

```typescript
interface AntiAliasingConfig {
  "setting.mat_antialias": 0 | 2 | 4 | 8; // MSAA模式
}
```

#### ViewportConfig

```typescript
interface ViewportConfig {
  "setting.mat_viewportscale": number;   // 视口缩放 (0.1-1.0)
  "setting.mat_viewportupscale": 0 | 1;
}
```

---

### 四、系统字段 (System)

```typescript
interface SystemConfig {
  "setting.cpu_level": 0 | 1 | 2;        // CPU等级
  "setting.gpu_mem_level": 0 | 1 | 2;    // 显存等级
  "setting.gpu_level": 0 | 1 | 2;        // GPU等级
  "setting.mem_level": 0 | 1 | 2;        // 内存等级
  "setting.steam_session_id": string;
  "setting.steam_device_id": string;
}
```

---

## Config.cfg 数据模型

### 鼠标设置

```typescript
interface MouseSettings {
  sensitivity: number;                   // 灵敏度 (0.1-20.0)
  zoom_sensitivity_ratio: number;        // 狙击镜灵敏度比例
  m_rawinput: 0 | 1;                     // 原始输入
  m_customaccel: number;
  m_mouseaccel1: number;
  m_mouseaccel2: number;
  m_mousespeed: 0 | 1;
}
```

### 准心设置

```typescript
interface CrosshairSettings {
  cl_crosshairsize: number;              // 大小 (0-10)
  cl_crosshairthickness: number;         // 粗细 (0-5)
  cl_crosshairgap: number;               // 间隙 (-10-10)
  cl_crosshaircolor: 0 | 1 | 2 | 3 | 4 | 5; // 颜色
  cl_crosshairalpha: number;             // 透明度 (0-255)
  cl_crosshairdot: 0 | 1;                // 中心点
  cl_crosshair_drawoutline: 0 | 1;       // 描边
  cl_crosshair_outlinethickness: number; // 描边粗细
  cl_crosshairusealpha: 0 | 1;           // 使用透明度
  cl_crosshaircolor_r: number;           // 自定义颜色R (0-255)
  cl_crosshaircolor_g: number;           // 自定义颜色G (0-255)
  cl_crosshaircolor_b: number;           // 自定义颜色B (0-255)
}
```

### 网络设置

```typescript
interface NetworkSettings {
  rate: number;                          // 最大带宽
  cl_cmdrate: 64 | 128;                  // 命令速率
  cl_updaterate: 64 | 128;               // 更新速率
  cl_interp: number;                     // 插值
  cl_interp_ratio: number;               // 插值比率
}
```

### 帧率设置

```typescript
interface FPSSettings {
  fps_max: number;                       // 最大帧率 (0=无限制)
  fps_max_menu: number;                  // 菜单帧率（已移除）
  fps_max_ui: number;                    // UI帧率
}
```

### 音频设置

```typescript
interface AudioSettings {
  volume: number;                        // 主音量 (0.0-1.0)
  snd_musicvolume: number;               // 音乐音量
  snd_menumusic_volume: number;
  snd_roundend_volume: number;
  snd_roundstart_volume: number;
  snd_tensecondwarning_volume: number;
  snd_mute_losefocus: 0 | 1;
  snd_headphone_pan_exponent: number;
  snd_headphone_pan_radial_weight: number;
  snd_mixahead: number;
}
```

### HUD设置

```typescript
interface HUDSettings {
  hud_scaling: number;                   // HUD缩放 (0.5-1.0)
  hud_showtargetid: 0 | 1;
  cl_hud_color: 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10;
  cl_hud_healthammo_style: 0 | 1;
  cl_showloadout: 0 | 1;
}
```

### 雷达设置

```typescript
interface RadarSettings {
  cl_radar_scale: number;                // 雷达缩放 (0.2-1.0)
  cl_radar_icon_scale_min: number;
  cl_radar_square_with_scoreboard: 0 | 1;
  cl_hud_radar_scale: number;
  cl_radar_always_centered: 0 | 1;
  cl_radar_rotate: 0 | 1;
}
```

### 视角设置

```typescript
interface ViewmodelSettings {
  viewmodel_fov: number;                 // 视野 (54-68)
  viewmodel_offset_x: number;            // X偏移
  viewmodel_offset_y: number;            // Y偏移
  viewmodel_offset_z: number;            // Z偏移
  viewmodel_presetpos: 1 | 2 | 3;        // 预设位置
}
```

### 绑定数据模型

```typescript
interface KeyBind {
  key: string;
  command: string;
}
```

---

## 配置预设

### 竞技预设

```json
{
  "video": {
    "setting.defaultres": "1920",
    "setting.defaultresheight": "1080",
    "setting.refreshrate": "144",
    "setting.fullscreen": "1",
    "setting.mat_vsync": "0",
    "setting.mat_antialias": "0",
    "setting.r_shadows": "0"
  },
  "config": {
    "fps_max": "0",
    "rate": "128000",
    "cl_cmdrate": "128",
    "cl_updaterate": "128",
    "cl_interp": "0",
    "cl_interp_ratio": "1",
    "m_rawinput": "1",
    "cl_crosshairsize": "2",
    "cl_crosshairthickness": "1",
    "cl_crosshairgap": "-2",
    "cl_crosshaircolor": "1"
  }
}
```

### 高画质预设

```json
{
  "video": {
    "setting.defaultres": "3840",
    "setting.defaultresheight": "2160",
    "setting.refreshrate": "60",
    "setting.fullscreen": "1",
    "setting.mat_vsync": "1",
    "setting.mat_antialias": "8",
    "setting.r_shadows": "1"
  },
  "config": {
    "fps_max": "120",
    "sensitivity": "1.0"
  }
}
```

---

## 枚举定义

### WindowMode

```typescript
enum WindowMode {
  Windowed = 0,
  Fullscreen = 1
}
```

### CrosshairColor

```typescript
enum CrosshairColor {
  Red = 0,
  Green = 1,
  Yellow = 2,
  Blue = 3,
  Cyan = 4,
  Custom = 5
}
```

### HUDColor

```typescript
enum HUDColor {
  Default = 0,
  White = 1,
  LightBlue = 2,
  DarkBlue = 3,
  Purple = 4,
  Red = 5,
  Orange = 6,
  Yellow = 7,
  Green = 8,
  LightGreen = 9,
  Pink = 10
}
```

### TickRate

```typescript
enum TickRate {
  Tick64 = 64,
  Tick128 = 128
}
```

---

## 验证规则

### 数值范围验证

| 字段 | 最小值 | 最大值 | 步进 |
|------|--------|--------|------|
| `defaultres` | 640 | 3840 | 1 |
| `defaultresheight` | 480 | 2160 | 1 |
| `sensitivity` | 0.1 | 20.0 | 0.01 |
| `cl_crosshairsize` | 0 | 10 | 0.1 |
| `cl_crosshairthickness` | 0 | 5 | 0.1 |
| `cl_crosshairgap` | -10 | 10 | 0.1 |
| `viewmodel_fov` | 54 | 68 | 1 |

### 枚举值验证

| 字段 | 有效值 |
|------|--------|
| `fullscreen` | [0, 1] |
| `mat_antialias` | [0, 2, 4, 8] |
| `r_shadows` | [0, 1] |
| `cl_crosshaircolor` | [0, 1, 2, 3, 4, 5] |
| `cl_cmdrate` | [64, 128] |
| `cl_updaterate` | [64, 128] |

---

## 版本历史

| 版本 | 变更 |
|------|------|
| CS2 | 从 Source 1 迁移到 Source 2 |
| CS2 | 移除 `mat_queue_mode` |
| CS2 | 移除 `mat_picmip` |
| CS2 | 移除 `fps_max_menu` |
| CS:GO | 最后版本 |

---

## CrosshairPro 集成数据模型

CS2 配置协议 → CrosshairPro `GameConfigStrategy(builtin-cs2)` 的字段映射。

### GameProfile

```typescript
interface GameProfile {
  Id: "builtin-cs2";
  DisplayName: "Counter-Strike 2";
  ProcessName: "cs2";          // cs2.exe 主进程名
  Priority: 100;
  AutoSwitch: true;
  FullscreenOnly: false;
  PresetId?: string;            // 关联的准心预设
}
```

### GameConfigStrategy

```typescript
interface GameConfigStrategy {
  GameId: "builtin-cs2";
  SupportsLaunchOptions: true;
  LaunchOptionsDescription: "CS2 启动项参数，如 -high -threads 12 -novid";
  Sections: [
    {
      Name: "video",
      DisplayName: "视频设置",
      Items: [
        { Key: "fullscreen",    Type: "Bool", DefaultValue: true },
        { Key: "resolution",    Type: "Enum", DefaultValue: "1920x1080", Options: ["1920x1080","1680x1050","1600x900","1440x900","1280x1024","1280x960","1280x800","1280x720"] },
        { Key: "aspect_ratio",  Type: "Enum", DefaultValue: "16:9", Options: ["16:9","16:10","4:3"] },
        { Key: "refresh_rate",  Type: "Int",  DefaultValue: 144, MinValue: 60, MaxValue: 360 }
      ]
    },
    {
      Name: "game",
      DisplayName: "游戏设置",
      Items: [
        { Key: "fps_max",    Type: "Int",  DefaultValue: 0,   MinValue: 0, MaxValue: 999, Description: "0 表示无限制" },
        { Key: "cl_showfps", Type: "Bool", DefaultValue: false }
      ]
    }
  ];
}
```

### 字段映射表

| Strategy Key | Type | CS2 cvar / 配置键 | 写入文件 | 默认值 |
|--------------|------|-------------------|----------|--------|
| `video.fullscreen` | Bool | `setting.fullscreen` | `video.txt` | `1` |
| `video.resolution` | Enum | `setting.defaultres` + `setting.defaultresheight` | `video.txt` | `1920x1080` |
| `video.aspect_ratio` | Enum | （由分辨率推断，不直接写入） | — | `16:9` |
| `video.refresh_rate` | Int | `setting.refreshrate` | `video.txt` | `144` |
| `game.fps_max` | Int | `fps_max` | `autoexec.cfg` | `0`（无限制） |
| `game.cl_showfps` | Bool | `cl_showfps` | `autoexec.cfg` | `0` |

### GameConfig 持久化

```typescript
interface GameConfig {
  GameId: "builtin-cs2";
  LaunchOptions: string;        // Steam 启动参数，如 "-novid -high +exec autoexec.cfg"
  Settings: {
    "fullscreen": boolean;
    "resolution": string;       // "1920x1080"
    "aspect_ratio": string;     // "16:9"
    "refresh_rate": number;     // 144
    "fps_max": number;          // 0
    "cl_showfps": boolean;
  };
}
```

**存储路径**：`%APPDATA%\CrosshairPro\gameconfigs\builtin-cs2.json`

```json
{
  "GameId": "builtin-cs2",
  "LaunchOptions": "-novid -high -threads 8 -freq 144 +exec autoexec.cfg",
  "Settings": {
    "fullscreen": true,
    "resolution": "1920x1080",
    "aspect_ratio": "16:9",
    "refresh_rate": 144,
    "fps_max": 0,
    "cl_showfps": false
  }
}
```

> 完整集成流程见 [integration.md](integration.md)，CS2 配置坑点见 [pitfalls.md](pitfalls.md)。

---
**创建时间**: 2026-07-31
**协议版本**: 1.1