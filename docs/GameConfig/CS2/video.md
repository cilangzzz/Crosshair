# CS2 video.txt 视频配置详解

## 文件格式

```txt
"video"
{
    "setting.key"    "value"
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
| `setting.refreshrate` | int | 60 | 刷新率 (Hz) |
| `setting.refreshrate_numerator` | int | 0 | 刷新率分子 |
| `setting.refreshrate_denominator` | int | 1 | 刷新率分母 |

#### 1.2 窗口模式

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.fullscreen` | int | 1 | 全屏模式 (0=窗口, 1=全屏) |
| `setting.fullscreen_width` | int | 1920 | 全屏宽度 |
| `setting.fullscreen_height` | int | 1080 | 全屏高度 |
| `setting.fullscreen_refresh_rate` | int | 60 | 全屏刷新率 |
| `setting.windowed` | int | 0 | 窗口模式 |
| `setting.windowed_width` | int | 1920 | 窗口宽度 |
| `setting.windowed_height` | int | 1080 | 窗口高度 |

#### 1.3 显示器

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.monitor` | int | 0 | 显示器索引 |
| `setting.monitor_name` | string | "" | 显示器名称 |

---

### 二、画质设置

#### 2.1 整体质量

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_queue_mode` | int | -1 | 多线程渲染（CS2已移除） |
| `setting.mat_vsync` | int | 0 | 垂直同步 (0=关, 1=开) |

#### 2.2 纹理

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_picmip` | int | 0 | 纹理质量（CS2已移除） |

#### 2.3 阴影

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.r_shadows` | int | 1 | 阴影开关 (0=关, 1=开) |
| `setting.r_shadowrendertotexture` | int | 1 | 阴影纹理渲染 |
| `setting.r_shadowmaxrendered` | int | 32 | 最大渲染阴影数 |

#### 2.4 特效

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.r_drawrain` | int | 1 | 雨效果 |
| `setting.r_drawropes` | int | 1 | 绳索渲染 |
| `setting.r_drawmodeldecals` | int | 1 | 模型贴花 |
| `setting.r_decals` | int | 128 | 贴花数量 |
| `setting.r_drawtracers_firstperson` | int | 1 | 第一人称弹道 |

#### 2.5 水面

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.r_waterforceexpensive` | int | 0 | 水面高质量反射 |
| `setting.r_waterforcereflectentities` | int | 0 | 水面实体反射 |

---

### 三、性能设置

#### 3.1 帧率

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.fps_max` | int | 400 | 最大帧率（在config.cfg设置） |
| `setting.fps_max_menu` | int | 120 | 菜单帧率（CS2已移除） |

#### 3.2 抗锯齿

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_antialias` | int | 8 | 抗锯齿模式 (MSAA) |
| `setting.mat_forceaniso` | int | 1 | 各向异性过滤 |

**抗锯齿模式值**：

| 值 | 模式 |
|----|------|
| 0 | 关闭 |
| 2 | MSAA 2x |
| 4 | MSAA 4x |
| 8 | MSAA 8x |

---

### 四、高级设置

#### 4.1 细节等级

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_viewportscale` | float | 1.0 | 视口缩放 (0.1-1.0) |
| `setting.mat_viewportupscale` | int | 1 | 视口放大 |
| `setting.mat_mipmaptextures` | int | 1 | Mipmap 纹理 |

#### 4.2 HDR 与色彩

| 键名 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `setting.mat_monitorgamma` | float | 2.2 | 显示器伽马值 |
| `setting.mat_enable_hdr` | int | 0 | HDR 支持 |

---

### 五、系统字段

| 键名 | 类型 | 说明 |
|------|------|------|
| `setting.steam_session_id` | string | Steam 会话 ID |
| `setting.steam_device_id` | string | Steam 设备 ID |
| `setting.cpu_level` | int | CPU 等级 (0-2) |
| `setting.gpu_mem_level` | int | 显存等级 (0-2) |
| `setting.gpu_level` | int | GPU 等级 (0-2) |
| `setting.mem_level` | int | 内存等级 (0-2) |

---

## 配置模板

### 竞技优化配置

```txt
"video"
{
    "setting.defaultres"    "1920"
    "setting.defaultresheight"    "1080"
    "setting.refreshrate"    "144"
    "setting.fullscreen"    "1"
    "setting.mat_vsync"    "0"
    "setting.mat_antialias"    "0"
    "setting.r_shadows"    "0"
    "setting.r_drawrain"    "0"
    "setting.r_drawropes"    "0"
    "setting.r_decals"    "0"
    "setting.r_drawtracers_firstperson"    "0"
}
```

### 高刷新率配置

```txt
"video"
{
    "setting.defaultres"    "1920"
    "setting.defaultresheight"    "1080"
    "setting.refreshrate"    "240"
    "setting.fullscreen"    "1"
    "setting.mat_vsync"    "0"
    "setting.mat_antialias"    "4"
}
```

### 4K 高画质配置

```txt
"video"
{
    "setting.defaultres"    "3840"
    "setting.defaultresheight"    "2160"
    "setting.refreshrate"    "60"
    "setting.fullscreen"    "1"
    "setting.mat_vsync"    "1"
    "setting.mat_antialias"    "8"
    "setting.r_shadows"    "1"
}
```

---

## 游戏内设置对照

| 游戏内选项 | 配置键名 | 值 |
|------------|----------|-----|
| 全屏模式 | `fullscreen` | 1 |
| 窗口模式 | `windowed` | 1 |
| 无边框窗口 | `fullscreen` + `windowed` | 1 + 1 |
| 垂直同步 | `mat_vsync` | 0/1 |
| 多核渲染 | `mat_queue_mode` | 已移除 |

---

## 注意事项

1. **数值需引号**：所有值都必须用双引号包裹
2. **前缀必须**：所有键名必须以 `setting.` 开头
3. **CS2 变化**：部分 CS:GO 参数已移除或无效
4. **编码格式**：保存为 UTF-8 无 BOM 格式
5. **游戏内设置优先**：游戏内更改会覆盖配置文件

---
**创建时间**: 2026-07-31
**协议版本**: 1.0