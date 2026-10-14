# Apex Legends 配置数据模型

本文档分两部分：**文件协议**（game 侧 videoconfig.txt / settings.cfg 的字段定义）与 **C# 数据模型**（CrosshairPro 应用侧建模）。两者的覆盖范围不同，差异是坑点，见 [pitfalls.md](pitfalls.md)。

---

## 第一部分：文件协议（videoconfig.txt）

### 一、显示设置 (Display)

```typescript
interface DisplayConfig {
  "setting.defaultres": number;          // 分辨率宽度 (640-3840)
  "setting.defaultresheight": number;    // 分辨率高度 (480-2160)
  "setting.last_display_width": number;  // 上次显示宽度
  "setting.last_display_height": number; // 上次显示高度
  "setting.m_nRefreshRate": number;      // 刷新率 Hz (60-360)
  "setting.m_nResolutionScale": number;  // 分辨率缩放 (50-100)
  "setting.fullscreen": 0 | 1;           // 全屏模式 (0=窗口, 1=全屏)
  "setting.nowindowborder": 0 | 1;       // 无边框窗口
  "setting.m_nWindowMode": 0 | 1 | 2;    // 0=全屏, 1=窗口, 2=无边框
  "setting.gamma": number;               // 伽马值 (0.5-2.5)
  "setting.m_flMonitorGamma": number;    // 显示器伽马 (1.8-2.6)
}
```

### 二、画质设置 (Quality)

```typescript
interface QualityConfig {
  // 纹理
  "setting.mat_picmip": -1 | 0 | 1 | 2;  // -1=最高, 0=高, 1=中, 2=低
  "setting.mat_forceaniso": number;      // 各向异性过滤 (0-16)
  "setting.mat_mip_linear": 0 | 1;       // Mipmap 线性过滤
  "setting.stream_memory": number;       // 流式内存 KB

  // 阴影
  "setting.shadow_enable": 0 | 1;
  "setting.shadow_maxdynamic": number;   // (0-8)
  "setting.shadow_depth_dimen_min": number;
  "setting.shadow_depth_upres_factor_max": number;
  "setting.csm_enabled": 0 | 1;
  "setting.csm_coverage": 0 | 1;
  "setting.csm_cascade_res": 256 | 512 | 1024;
  "setting.new_shadow_settings": 0 | 1;

  // 粒子
  "setting.particle_cpu_level": 0 | 1 | 2;
  "setting.cl_particle_fallback_base": 0 | 1 | 2 | 3;
  "setting.cl_particle_fallback_multiplier": 0 | 1 | 2;

  // 物理
  "setting.cl_gib_allow": 0 | 1;
  "setting.cl_ragdoll_maxcount": number; // (0-16)
  "setting.cl_ragdoll_self_collision": 0 | 1;

  // 贴花
  "setting.r_decals": number;            // (0-256)
  "setting.r_createmodeldecals": 0 | 1;

  // LOD 与细节
  "setting.r_lod_switch_scale": number;  // (0.35-1.0)
  "setting.fadeDistScale": number;       // (0.5-2.0)
  "setting.map_detail_level": 0 | 1 | 2;

  // 体积效果 / SSAO
  "setting.volumetric_lighting": 0 | 1;
  "setting.volumetric_fog": 0 | 1;
  "setting.ssao_quality": 0 | 1 | 2;     // 0=关, 1=低, 2=高
}
```

### 三、性能设置 (Performance)

```typescript
interface PerformanceConfig {
  "setting.mat_vsync_mode": 0 | 1 | 2;   // 0=关, 1=开, 2=三重缓冲
  "setting.mat_backbuffer_count": 1 | 2;
  "setting.mat_antialias_mode": 0 | 1 | 2 | 3 | 4; // 0=关, 1=FXAA, 2=TXAA, 3=MSAA2x, 4=MSAA4x
  "setting.dvs_enable": 0 | 1;
  "setting.dvs_gpuframetime_min": number; // 最小帧时间 μs
  "setting.dvs_gpuframetime_max": number; // 最大帧时间 μs
  "setting.dynamic_streaming_budget": number;
}
```

### 四、高级画质 (Advanced)

```typescript
interface AdvancedQualityConfig {
  "setting.m_fSpecularHighlight": number;     // 高光强度 (0.0-1.0)
  "setting.m_fDynamicDecals": number;         // 动态贴花
  "setting.m_fDynamicLights": number;         // 动态光照
  "setting.m_fShadows": number;               // 阴影质量
  "setting.m_fDecals": number;                // 贴花质量
  "setting.m_fSSAO": number;                  // SSAO 强度
  "setting.m_fSunShadowFilter": number;       // 阳光阴影过滤
  "setting.m_fSunShadowResolution": number;   // 阳光阴影分辨率
  "setting.m_fSpotShadowResolution": number;  // 聚光阴影分辨率
  "setting.m_fVignetteEnable": 0 | 1;         // 暗角效果
  "setting.m_fDepthOfField": number;          // 景深
  "setting.m_fMotionBlur": number;            // 动态模糊
}
```

### 五、系统字段 (System)

```typescript
interface SystemConfig {
  "setting.configversion": number;       // 配置版本 (当前: 10)
  "setting.sound_volume": number;        // 音量 (0.0-1.0)
}
```

---

## 第二部分：文件协议（settings.cfg）

```typescript
interface MouseSettings {
  mouse_sensitivity: number;             // 灵敏度 (0.1-10.0)
  m_acceleration: 0 | 1;                 // 加速
  m_clamp_to_window: 0 | 1;              // 限制窗口
}

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

interface FrameFovNetworkSettings {
  fps_max: number;                       // 0=无限制
  fps_max_menu: number;                  // 菜单帧率
  cl_showfps: 0 | 1 | 2;
  cl_showpos: 0 | 1;
  cl_fovScale: number;                   // (1.0-1.35)
  cl_viewpitchscale: number;
  cl_interp: number;                     // 0.015
  cl_interp_ratio: number;               // 1
  cl_cmdrate: number;                    // 60
  cl_updaterate: number;                 // 60
}

interface AudioSettings {
  sound_num_speakers: 2 | 6 | 8;
  sound_volume_voice: number;
  miles_channels: number;
  miles_output_device: string;
}

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

interface GraphicsSettings {
  gfx_amdUseLowLatency: 0 | 1;
  gfx_nvnUseLowLatency: 0 | 1;
  gfx_nvnUseLowLatencyBoost: 0 | 1;
  chroma_enable: 0 | 1;
}

interface KeyBind {
  layout: "US_standard";
  type: "bind" | "bind_held";
  key: string;
  command: string;
  flags: 0 | 1;
}
```

其余散项（`name`、`ui_layout_mode`、`sv_*`、`cc_*`、`func_break_max_pieces`、`lookspring`、`lookstrafe`、`hdr_screenshot_directory` 等）见 [settings.md](settings.md)。

---

## 第三部分：C# 数据模型（应用侧权威定义）

源码位置：`src/CrosshairPro.Core/Models/`，全部为 CommunityToolkit `ObservableObject`（`[ObservableProperty]` 生成属性）。

### ApexVideoConfig（`ApexVideoConfig.cs`）

按分区建模上述 videoconfig.txt 字段，默认值即模型默认值（如 `RefreshRate=144`、`CsmCascadeRes=512`、`SsaoQuality=1`、`ConfigVersion=10`）。附加辅助成员：

- `ResolutionOptions`：13 档静态分辨率表（3840x2160 → 1280x720）
- `ResolutionIndex`：当前分辨率索引（默认 5 = 1920x1080）；`OnResolutionIndexChanged` 自动同步 `DefaultRes`/`DefaultResHeight`
- `UpdateResolutionIndex()`：按 `DefaultRes`/`DefaultResHeight` 反查索引，未匹配时回退到 5

### ApexSettingsConfig（`ApexSettingsConfig.cs`）

建模 settings.cfg 的设置项（鼠标、瞄准镜、帧率、FOV、网络、音频、语音、图形、其他），**不含键位绑定**。

### ApexLaunchOptions（`ApexLaunchOptions.cs`）

结构化启动项模型，`GenerateOptionsString()` 的输出条件规则详见 [launch-options.md](launch-options.md)。

### BackupFileInfo 与解析/序列化覆盖范围

`BackupFileInfo`（定义于 `IApexConfigService.cs`）的成员表，以及 `ApexConfigService` 对 videoconfig.txt / settings.cfg 的解析与序列化键覆盖范围（videoconfig 23 写 / 19 读、settings 17 项对称、4 键只写不读）属于服务实现细节，权威描述见 [../../Application/api-gameconfig.md](../../Application/api-gameconfig.md) 的「解析 / 序列化要点」与「BackupFileInfo」小节，此处不重复。

---

## 内置视频预设（应用侧）

`ApexConfigService` 提供三个预设，预设只显式设置以下字段，其余字段保持模型默认值：

| 字段 | 竞技 `GetCompetitivePreset` | 高画质 `GetHighQualityPreset` |
|------|------|------|
| `DefaultRes` / `DefaultResHeight` | 1920 / 1080 | 1920 / 1080 |
| `RefreshRate` | 144 | 144 |
| `Fullscreen` / `WindowMode` / `Gamma` | 1 / 0 / 1.0 | 1 / 0 / 1.0 |
| `MatPicmip` | 0 | -1 |
| `MatForceaniso` | 1 | 16 |
| `MatMipLinear` | （默认 1） | 1 |
| `ShadowEnable` / `CsmEnabled` | 0 / 0 | 1 / 1 |
| `CsmCascadeRes` | （默认 512） | 1024 |
| `ParticleCpuLevel` | 0 | 2 |
| `SsaoQuality` | 0 | 2 |
| `VolumetricLighting` / `VolumetricFog` | 0 / 0 | 1 / 1 |
| `MatVsyncMode` / `MatAntialiasMode` / `DvsEnable` | 0 / 0 / 0 | 0 / 4 / 0 |
| `ClGibAllow` / `ClRagdollMaxcount` / `ClRagdollSelfCollision` | 0 / 0 / 0 | 1 / 8 / 1 |

`GetDefaultPreset()` 返回全新 `ApexVideoConfig()`（全部字段取模型默认值）。

---

## 取值域约定（伪代码，非 C# 枚举）

> **注意**：`ApexVideoConfig` / `ApexSettingsConfig` 的相关字段均为 `int`，源码中**不存在**与下述名称对应的 C# 枚举类型（`src/CrosshairPro.Core/Enums/` 仅有 `AppState`、`CrosshairStyle`）。取值域由 UI 侧选项集合承载，见 `ApexConfigViewModel` 的 `WindowModeOptions` / `TextureQualityOptions` / `ParticleLevelOptions` / `SsaoQualityOptions` / `VsyncOptions` / `AntialiasOptions` 等字典。以下仅为伪代码速查：

```text
WindowMode          0=Fullscreen  1=Windowed  2=BorderlessWindowed
TextureQuality     -1=Highest     0=High      1=Medium    2=Low
ParticleLevel       0=Low         1=Medium    2=High
SSAOQuality         0=Off         1=Low       2=High
VSyncMode           0=Off         1=On        2=TripleBuffered
AntiAliasingMode    0=Off         1=FXAA      2=TXAA      3=MSAA 2x  4=MSAA 4x
```

---

## 验证规则

### 数值范围（模型注释约定）

| 字段 | 最小值 | 最大值 | 步进 |
|------|--------|--------|------|
| `defaultres` | 640 | 3840 | 1 |
| `defaultresheight` | 480 | 2160 | 1 |
| `m_nRefreshRate` | 60 | 360 | 1 |
| `gamma` | 0.5 | 2.5 | 0.1 |
| `mouse_sensitivity` | 0.1 | 10.0 | 0.1 |
| `cl_fovScale` | 1.0 | 1.35 | 0.01 |

注意：范围来自源码注释约定，服务层保存时**不做范围校验**，UI 控件负责约束输入。模型注释仅定义范围、不含步进；步进列对应 `ApexPage.xaml` 中各 Slider 的 `SmallChange`（如 mouse_sensitivity 滑块 `SmallChange="0.1"`，`{0:F2}` 仅是 TextBox 显示格式）。

### 取值域验证

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
**协议版本**: 1.1
