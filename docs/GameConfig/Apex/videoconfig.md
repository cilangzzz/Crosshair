# videoconfig.txt 视频配置详解

## 文件格式

```txt
"VideoConfig"
{
    "setting.key_name"    "value"
}
```

- **编码**: UTF-8 (无 BOM)
- **格式**: KeyValues 文本格式
- **前缀**: 所有键名以 `setting.` 开头

---

## 配置选项索引

### 一、显示设置

#### 1.1 分辨率

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.defaultres` | int | 1920 | 分辨率宽度 |
| `setting.defaultresheight` | int | 1080 | 分辨率高度 |
| `setting.last_display_width` | int | 1920 | 上次显示宽度 |
| `setting.last_display_height` | int | 1080 | 上次显示高度 |
| `setting.m_nRefreshRate` | int | 60 | 刷新率 (Hz) |
| `setting.m_nResolutionScale` | int | 100 | 分辨率缩放百分比 (50-100) |

#### 1.2 窗口模式

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.fullscreen` | int | 1 | 全屏模式 (0=窗口, 1=全屏) |
| `setting.nowindowborder` | int | 1 | 无边框窗口 (0=有边框, 1=无边框) |
| `setting.m_nWindowMode` | int | 0 | 窗口模式 (0=全屏, 1=窗口, 2=无边框) |

#### 1.3 亮度

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.gamma` | float | 1.0 | 伽马值 (0.5-2.5, 越高越亮) |
| `setting.m_flMonitorGamma` | float | 2.2 | 显示器伽马 |

---

### 二、画质设置

#### 2.1 纹理

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_picmip` | int | 0 | 纹理质量 (-1=最高, 0=高, 1=中, 2=低) |
| `setting.mat_forceaniso` | int | 1 | 各向异性过滤 (0-16) |
| `setting.mat_mip_linear` | int | 1 | Mipmap 线性过滤 (0=关, 1=开) |
| `setting.stream_memory` | int | 128000 | 流式内存 (KB) |

#### 2.2 阴影

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.shadow_enable` | int | 1 | 阴影总开关 (0=关, 1=开) |
| `setting.shadow_maxdynamic` | int | 4 | 最大动态阴影数 (0-8) |
| `setting.shadow_depth_dimen_min` | int | 0 | 阴影深度最小维度 |
| `setting.shadow_depth_upres_factor_max` | int | 0 | 阴影上采样因子 |
| `setting.csm_enabled` | int | 1 | 级联阴影映射 (0=关, 1=开) |
| `setting.csm_coverage` | int | 1 | CSM 覆盖范围 (0-1) |
| `setting.csm_cascade_res` | int | 512 | CSM 级联分辨率 (256/512/1024) |
| `setting.new_shadow_settings` | int | 1 | 新阴影设置标记 |

#### 2.3 粒子效果

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.particle_cpu_level` | int | 1 | 粒子CPU等级 (0=低, 1=中, 2=高) |
| `setting.cl_particle_fallback_base` | int | 3 | 粒子回退基数 (0-3) |
| `setting.cl_particle_fallback_multiplier` | int | 2 | 粒子回退乘数 (0-2) |

#### 2.4 物理

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.cl_gib_allow` | int | 1 | 碎片效果 (0=关, 1=开) |
| `setting.cl_ragdoll_maxcount` | int | 8 | 布娃娃物理数量 (0-16) |
| `setting.cl_ragdoll_self_collision` | int | 1 | 布娃娃自碰撞 (0=关, 1=开) |

#### 2.5 贴花

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.r_decals` | int | 256 | 贴花数量 (0-256) |
| `setting.r_createmodeldecals` | int | 1 | 模型贴花 (0=关, 1=开) |

#### 2.6 LOD 与细节

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.r_lod_switch_scale` | float | 0.6 | LOD切换距离 (0.35-1.0) |
| `setting.fadeDistScale` | float | 1.0 | 消失距离缩放 (0.5-2.0) |
| `setting.map_detail_level` | int | 1 | 地图细节等级 (0=低, 1=中, 2=高) |

#### 2.7 体积效果

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.volumetric_lighting` | int | 0 | 体积光 (0=关, 1=开) |
| `setting.volumetric_fog` | int | 0 | 体积雾 (0=关, 1=开) |

#### 2.8 环境光遮蔽

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.ssao_quality` | int | 1 | SSAO质量 (0=关, 1=低, 2=高) |

---

### 三、性能设置

#### 3.1 垂直同步

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_vsync_mode` | int | 0 | 垂直同步 (0=关, 1=开, 2=三重缓冲) |
| `setting.mat_backbuffer_count` | int | 1 | 后缓冲区数量 (1-2) |

#### 3.2 抗锯齿

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_antialias_mode` | int | 0 | 抗锯齿模式 |

**抗锯齿模式值**：

| 值 | 模式 |
|----|------|
| 0 | 关闭 |
| 1 | FXAA |
| 2 | TXAA |
| 3 | MSAA 2x |
| 4 | MSAA 4x |

#### 3.3 动态分辨率 (DVS)

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.dvs_enable` | int | 0 | 动态分辨率缩放 (0=关, 1=开) |
| `setting.dvs_gpuframetime_min` | int | 15000 | GPU帧时间最小值 (微秒) |
| `setting.dvs_gpuframetime_max` | int | 16500 | GPU帧时间最大值 (微秒) |
| `setting.dynamic_streaming_budget` | int | 0 | 动态流预算 |

---

### 四、高级画质选项

以下选项可能不在默认配置中出现，但可以手动添加：

#### 4.1 光照与阴影

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.m_fSpecularHighlight` | float | 1.0 | 高光强度 (0.0-1.0) |
| `setting.m_fDynamicLights` | float | 1.0 | 动态光照质量 (0.0-1.0) |
| `setting.m_fShadows` | float | 1.0 | 阴影质量乘数 (0.0-1.0) |
| `setting.m_fSunShadowFilter` | float | 1.0 | 阳光阴影过滤 |
| `setting.m_fSunShadowResolution` | float | 1.0 | 阳光阴影分辨率 |
| `setting.m_fSpotShadowResolution` | float | 1.0 | 聚光阴影分辨率 |

#### 4.2 特效

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.m_fDynamicDecals` | float | 1.0 | 动态贴花质量 (0.0-1.0) |
| `setting.m_fDecals` | float | 1.0 | 贴花质量 (0.0-1.0) |
| `setting.m_fSSAO` | float | 1.0 | SSAO强度 (0.0-1.0) |

#### 4.3 后处理

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.m_fVignetteEnable` | int | 0 | 暗角效果 (0=关, 1=开) |
| `setting.m_fDepthOfField` | float | 1.0 | 景深质量 (0.0-1.0) |
| `setting.m_fMotionBlur` | float | 0.0 | 动态模糊强度 (0.0-1.0) |

---

### 五、系统字段

| 键名 | 类型 | 说明 |
|------|------|------|
| `setting.configversion` | int | 配置版本号 (当前: 10) |
| `setting.sound_volume` | float | 音量 (0.0-1.0) |

---

## 配置模板

### 竞技优化配置

```txt
"VideoConfig"
{
    "setting.cl_gib_allow"        "0"
    "setting.cl_ragdoll_maxcount"    "0"
    "setting.cl_ragdoll_self_collision"    "0"
    "setting.particle_cpu_level"    "0"
    "setting.cl_particle_fallback_base"    "3"
    "setting.cl_particle_fallback_multiplier"    "2"
    "setting.mat_picmip"    "0"
    "setting.mat_forceaniso"    "1"
    "setting.mat_mip_linear"    "0"
    "setting.r_lod_switch_scale"    "0.600000"
    "setting.shadow_enable"    "0"
    "setting.csm_enabled"    "0"
    "setting.ssao_quality"    "0"
    "setting.volumetric_lighting"    "0"
    "setting.volumetric_fog"    "0"
    "setting.mat_vsync_mode"    "0"
    "setting.mat_backbuffer_count"    "1"
    "setting.mat_antialias_mode"    "0"
    "setting.fullscreen"    "1"
    "setting.defaultres"    "1920"
    "setting.defaultresheight"    "1080"
    "setting.gamma"    "1.000000"
    "setting.configversion"    "10"
}
```

### 高画质配置

```txt
"VideoConfig"
{
    "setting.cl_gib_allow"        "1"
    "setting.cl_ragdoll_maxcount"    "8"
    "setting.particle_cpu_level"    "2"
    "setting.mat_picmip"    "-1"
    "setting.mat_forceaniso"    "16"
    "setting.mat_mip_linear"    "1"
    "setting.r_lod_switch_scale"    "0.350000"
    "setting.shadow_enable"    "1"
    "setting.csm_enabled"    "1"
    "setting.csm_cascade_res"    "1024"
    "setting.ssao_quality"    "2"
    "setting.volumetric_lighting"    "1"
    "setting.volumetric_fog"    "1"
    "setting.mat_antialias_mode"    "4"
    "setting.configversion"    "10"
}
```

---

## 字段类型参考

| 类型符号 | 说明 | 示例 |
|----------|------|------|
| `int` | 整数 | `"1920"` |
| `float` | 浮点数 | `"0.600000"` |
| `bool` | 布尔 (0/1) | `"0"`, `"1"` |
| `string` | 字符串 | `"value"` |

---

## 注意事项

1. **数值需引号**：所有值都必须用双引号包裹
2. **前缀必须**：所有键名必须以 `setting.` 开头
3. **版本字段**：不要修改 `configversion`，否则可能重置
4. **编码格式**：保存为 UTF-8 无 BOM 格式

---
**创建时间**: 2026-07-31
**协议版本**: 1.0