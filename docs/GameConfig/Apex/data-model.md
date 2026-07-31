# Apex Legends 配置数据模型

## 数据结构定义

### VideoConfig

视频配置根对象。

```typescript
interface VideoConfig {
  "VideoConfig": {
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
  "setting.last_display_width": number;  // 上次显示宽度
  "setting.last_display_height": number; // 上次显示高度
  "setting.m_nRefreshRate": number;      // 刷新率 Hz (60-240)
  "setting.m_nResolutionScale": number;  // 分辨率缩放 (50-100)
}
```

#### WindowConfig

```typescript
interface WindowConfig {
  "setting.fullscreen": 0 | 1;           // 全屏模式
  "setting.nowindowborder": 0 | 1;       // 无边框窗口
  "setting.m_nWindowMode": 0 | 1 | 2;    // 窗口模式
  // 0 = 全屏
  // 1 = 窗口化
  // 2 = 无边框窗口
}
```

#### BrightnessConfig

```typescript
interface BrightnessConfig {
  "setting.gamma": number;               // 伽马值 (0.5-2.5)
  "setting.m_flMonitorGamma": number;    // 显示器伽马 (1.8-2.6)
}
```

---

### 二、画质设置 (Quality)

#### TextureConfig

```typescript
interface TextureConfig {
  "setting.mat_picmip": -1 | 0 | 1 | 2;  // 纹理质量
  // -1 = 最高
  //  0 = 高
  //  1 = 中
  //  2 = 低

  "setting.mat_forceaniso": number;      // 各向异性过滤 (0-16)
  "setting.mat_mip_linear": 0 | 1;       // Mipmap 线性过滤
  "setting.stream_memory": number;       // 流式内存 KB
}
```

#### ShadowConfig

```typescript
interface ShadowConfig {
  "setting.shadow_enable": 0 | 1;        // 阴影总开关
  "setting.shadow_maxdynamic": number;   // 最大动态阴影数 (0-8)
  "setting.shadow_depth_dimen_min": number;
  "setting.shadow_depth_upres_factor_max": number;

  "setting.csm_enabled": 0 | 1;          // 级联阴影映射
  "setting.csm_coverage": 0 | 1;         // CSM 覆盖范围
  "setting.csm_cascade_res": 256 | 512 | 1024; // CSM 分辨率
  "setting.new_shadow_settings": 0 | 1;
}
```

#### ParticleConfig

```typescript
interface ParticleConfig {
  "setting.particle_cpu_level": 0 | 1 | 2; // 粒子CPU等级
  "setting.cl_particle_fallback_base": 0 | 1 | 2 | 3;
  "setting.cl_particle_fallback_multiplier": 0 | 1 | 2;
}
```

#### PhysicsConfig

```typescript
interface PhysicsConfig {
  "setting.cl_gib_allow": 0 | 1;         // 碎片效果
  "setting.cl_ragdoll_maxcount": number; // 布娃娃数量 (0-16)
  "setting.cl_ragdoll_self_collision": 0 | 1;
}
```

#### DecalConfig

```typescript
interface DecalConfig {
  "setting.r_decals": number;            // 贴花数量 (0-256)
  "setting.r_createmodeldecals": 0 | 1;  // 模型贴花
}
```

#### LODConfig

```typescript
interface LODConfig {
  "setting.r_lod_switch_scale": number;  // LOD 距离 (0.35-1.0)
  "setting.fadeDistScale": number;       // 消失距离缩放 (0.5-2.0)
  "setting.map_detail_level": 0 | 1 | 2; // 地图细节等级
}
```

#### VolumetricConfig

```typescript
interface VolumetricConfig {
  "setting.volumetric_lighting": 0 | 1;  // 体积光
  "setting.volumetric_fog": 0 | 1;       // 体积雾
}
```

#### SSAOConfig

```typescript
interface SSAOConfig {
  "setting.ssao_quality": 0 | 1 | 2;     // SSAO 质量
  // 0 = 关闭
  // 1 = 低
  // 2 = 高
}
```

---

### 三、性能设置 (Performance)

#### VSyncConfig

```typescript
interface VSyncConfig {
  "setting.mat_vsync_mode": 0 | 1 | 2;   // 垂直同步
  // 0 = 关闭
  // 1 = 开启
  // 2 = 三重缓冲

  "setting.mat_backbuffer_count": 1 | 2; // 后缓冲区数量
}
```

#### AntiAliasingConfig

```typescript
interface AntiAliasingConfig {
  "setting.mat_antialias_mode": 0 | 1 | 2 | 3 | 4;
  // 0 = 关闭
  // 1 = FXAA
  // 2 = TXAA
  // 3 = MSAA 2x
  // 4 = MSAA 4x
}
```

#### DVSConfig

```typescript
interface DVSConfig {
  "setting.dvs_enable": 0 | 1;           // 动态分辨率
  "setting.dvs_gpuframetime_min": number; // 最小帧时间 μs
  "setting.dvs_gpuframetime_max": number; // 最大帧时间 μs
  "setting.dynamic_streaming_budget": number;
}
```

---

### 四、高级画质 (Advanced)

```typescript
interface AdvancedQualityConfig {
  "setting.m_fSpecularHighlight": number;     // 高光强度 (0.0-1.0)
  "setting.m_fDynamicDecals": number;         // 动态贴花 (0.0-1.0)
  "setting.m_fDynamicLights": number;         // 动态光照 (0.0-1.0)
  "setting.m_fShadows": number;               // 阴影质量 (0.0-1.0)
  "setting.m_fDecals": number;                // 贴花质量 (0.0-1.0)
  "setting.m_fSSAO": number;                  // SSAO 强度 (0.0-1.0)
  "setting.m_fSunShadowFilter": number;       // 阳光阴影过滤
  "setting.m_fSunShadowResolution": number;   // 阳光阴影分辨率
  "setting.m_fSpotShadowResolution": number;  // 聚光阴影分辨率
  "setting.m_fVignetteEnable": 0 | 1;         // 暗角效果
  "setting.m_fDepthOfField": number;          // 景深 (0.0-1.0)
  "setting.m_fMotionBlur": number;            // 动态模糊 (0.0-1.0)
}
```

---

### 五、系统字段 (System)

```typescript
interface SystemConfig {
  "setting.configversion": number;       // 配置版本 (当前: 10)
  "setting.sound_volume": number;        // 音量 (0.0-1.0)
}
```

---

## Settings.cfg 数据模型

### 鼠标设置

```typescript
interface MouseSettings {
  mouse_sensitivity: number;             // 灵敏度 (0.1-10.0)
  m_acceleration: 0 | 1;                 // 加速
  m_clamp_to_window: 0 | 1;              // 限制窗口
}
```

### 瞄准镜灵敏度

```typescript
interface ScopeSensitivitySettings {
  mouse_use_per_scope_sensitivity_scalars: 0 | 1;
  mouse_zoomed_sensitivity_scalar_0: number; // 1倍镜
  mouse_zoomed_sensitivity_scalar_1: number; // 2倍镜
  mouse_zoomed_sensitivity_scalar_2: number; // 3倍镜
  mouse_zoomed_sensitivity_scalar_3: number; // 4倍镜
  mouse_zoomed_sensitivity_scalar_4: number; // 6倍镜
  mouse_zoomed_sensitivity_scalar_5: number; // 8倍镜
  mouse_zoomed_sensitivity_scalar_6: number; // 10倍镜
  mouse_zoomed_sensitivity_scalar_7: number; // 变焦镜
}
```

### 音频设置

```typescript
interface AudioSettings {
  sound_num_speakers: 2 | 6 | 8;         // 扬声器数量
  sound_volume_voice: number;            // 语音音量 (0.0-1.0)
  miles_channels: number;
  miles_output_device: string;
  miles_dumpuploadtime: number;
}
```

### 语音设置

```typescript
interface VoiceSettings {
  VoiceChatMode: number;
  voice_forcemicrecord: 0 | 1;
  voice_input_device: string;
  voice_mixer_boost: number;
  voice_mixer_mute: 0 | 1;
  voice_mixer_volume: number;
  voice_modenable: 0 | 1;
  voice_scale: number;
  voice_vox: 0 | 1;
}
```

### 图形设置

```typescript
interface GraphicsSettings {
  gfx_amdUseLowLatency: 0 | 1;
  gfx_nvnUseLowLatency: 0 | 1;
  gfx_nvnUseLowLatencyBoost: 0 | 1;
  chroma_enable: 0 | 1;
}
```

### 绑定数据模型

```typescript
interface KeyBind {
  layout: "US_standard";
  type: "bind" | "bind_held";
  key: string;
  command: string;
  flags: 0 | 1;
}
```

---

## 配置预设

### 竞技预设

```json
{
  "videoconfig": {
    "setting.cl_gib_allow": "0",
    "setting.cl_ragdoll_maxcount": "0",
    "setting.particle_cpu_level": "0",
    "setting.shadow_enable": "0",
    "setting.csm_enabled": "0",
    "setting.ssao_quality": "0",
    "setting.volumetric_lighting": "0",
    "setting.volumetric_fog": "0",
    "setting.mat_vsync_mode": "0",
    "setting.mat_antialias_mode": "0"
  },
  "settings": {
    "m_acceleration": "0",
    "fps_max": "0",
    "cl_fovScale": "1.27",
    "cl_interp": "0.015",
    "cl_interp_ratio": "1"
  }
}
```

### 高画质预设

```json
{
  "videoconfig": {
    "setting.mat_picmip": "-1",
    "setting.mat_forceaniso": "16",
    "setting.shadow_enable": "1",
    "setting.csm_enabled": "1",
    "setting.csm_cascade_res": "1024",
    "setting.ssao_quality": "2",
    "setting.volumetric_lighting": "1",
    "setting.volumetric_fog": "1",
    "setting.mat_antialias_mode": "4"
  }
}
```

---

## 枚举定义

### WindowMode

```typescript
enum WindowMode {
  Fullscreen = 0,
  Windowed = 1,
  BorderlessWindowed = 2
}
```

### TextureQuality

```typescript
enum TextureQuality {
  Highest = -1,
  High = 0,
  Medium = 1,
  Low = 2
}
```

### ParticleLevel

```typescript
enum ParticleLevel {
  Low = 0,
  Medium = 1,
  High = 2
}
```

### SSAOQuality

```typescript
enum SSAOQuality {
  Off = 0,
  Low = 1,
  High = 2
}
```

### VSyncMode

```typescript
enum VSyncMode {
  Off = 0,
  On = 1,
  TripleBuffered = 2
}
```

### AntiAliasingMode

```typescript
enum AntiAliasingMode {
  Off = 0,
  FXAA = 1,
  TXAA = 2,
  MSAA_2x = 3,
  MSAA_4x = 4
}
```

---

## 验证规则

### 数值范围验证

| 字段 | 最小值 | 最大值 | 步进 |
|------|--------|--------|------|
| `defaultres` | 640 | 3840 | 1 |
| `defaultresheight` | 480 | 2160 | 1 |
| `gamma` | 0.5 | 2.5 | 0.1 |
| `mouse_sensitivity` | 0.1 | 10.0 | 0.01 |
| `cl_fovScale` | 1.0 | 1.35 | 0.01 |

### 枚举值验证

| 字段 | 有效值 |
|------|--------|
| `fullscreen` | [0, 1] |
| `mat_picmip` | [-1, 0, 1, 2] |
| `particle_cpu_level` | [0, 1, 2] |
| `ssao_quality` | [0, 1, 2] |
| `mat_vsync_mode` | [0, 1, 2] |
| `mat_antialias_mode` | [0, 1, 2, 3, 4] |

---

## 版本历史

| 版本 | 变更 |
|------|------|
| 10 | 当前版本 |
| 9 | 添加 `new_shadow_settings` |
| 8 | 添加 DVS 字段 |
| 7 | 添加 CSM 字段 |
| 6 | 添加体积光/雾 |

---
**创建时间**: 2026-07-31
**协议版本**: 1.0