# CrosshairPro.Core

核心层模块，定义所有模型、接口和枚举。所有其他模块都依赖此模块。

## 概述

Core 模块是整个应用的基石，提供：
- 数据模型（CrosshairConfig、Preset、EffectsConfig 等）
- 业务接口（IConfigRepository、ICrosshairRenderer、IHotkeyManager）
- 枚举定义（CrosshairStyle、AppState）

## 目录结构

```
CrosshairPro.Core/
├── Enums/
│   ├── AppState.cs           # 应用状态枚举
│   └── CrosshairStyle.cs     # 准心样式枚举
├── Interfaces/
│   ├── IConfigRepository.cs  # 配置仓库接口
│   ├── ICrosshairRenderer.cs # 准心渲染器接口
│   ├── IGameDetector.cs      # 游戏检测器接口
│   └── IHotkeyManager.cs     # 热键管理器接口
└── Models/
    ├── CrosshairConfig.cs    # 准心配置（主模型）
    ├── EffectsConfig.cs      # 效果配置（描边、阴影、发光）
    ├── GameInfo.cs           # 游戏信息
    ├── GameProfile.cs        # 游戏配置文件
    ├── HotkeyBinding.cs      # 热键绑定
    └── Preset.cs             # 预设
```

## 核心模型

### CrosshairConfig

准心配置主模型，包含所有可配置项：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | string | GUID | 配置唯一标识 |
| Name | string | "默认配置" | 配置名称 |
| Style | CrosshairStyle | Cross | 准心样式 |
| Size | int | 20 | 准心大小 |
| Gap | int | 4 | 中心间隙 |
| Thickness | int | 2 | 线条粗细 |
| Color | string | "#00FF00" | 颜色（十六进制） |
| Opacity | int | 100 | 不透明度（0-100） |
| Brightness | int | 100 | 亮度（0-200） |
| CenterSize | int | 4 | 中心点大小 |
| Rotation | int | 0 | 旋转角度 |
| CustomImagePath | string? | null | 自定义图片路径 |
| Effects | EffectsConfig | new() | 效果配置 |
| Display | DisplayConfig | new() | 显示配置 |

### EffectsConfig

效果配置，包含三种效果：

| 效果 | 类型 | 说明 |
|------|------|------|
| Outline | OutlineConfig | 描边效果 |
| Shadow | ShadowConfig | 阴影效果 |
| Glow | GlowConfig | 发光效果 |

### DisplayConfig

显示配置：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Monitor | string | "primary" | 目标显示器 |
| ClickThrough | bool | true | 鼠标穿透 |
| AlwaysOnTop | bool | true | 始终置顶 |
| PositionX | int | 0 | X轴偏移 |
| PositionY | int | 0 | Y轴偏移 |

## 接口定义

### IConfigRepository

配置持久化接口：
- `LoadConfigAsync()` - 加载主配置
- `SaveConfigAsync(config)` - 保存配置
- `ResetToDefaultAsync()` - 重置为默认
- `ExportConfigAsync(path, config)` - 导出配置
- `ImportConfigAsync(path)` - 导入配置

### IPresetRepository

预设管理接口：
- `LoadPresetsAsync()` - 加载所有预设
- `SavePresetAsync(preset)` - 保存预设
- `DeletePresetAsync(id)` - 删除预设
- `GetPresetAsync(id)` - 获取单个预设
- `ExportPresetAsync(id, path)` - 导出预设
- `ImportPresetAsync(path)` - 导入预设

### ICrosshairRenderer

准心渲染接口：
- `Render(drawingContext, config, width, height)` - 渲染准心
- `RenderCompleted` 事件 - 渲染完成通知

### IHotkeyManager

热键管理接口：
- `RegisterHotkey(binding)` - 注册热键
- `UnregisterHotkey(id)` - 注销热键
- `UnregisterAll()` - 注销所有热键
- `HotkeyTriggered` 事件 - 热键触发通知

## 枚举定义

### CrosshairStyle

准心样式枚举：

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Cross | 十字准心 |
| 1 | Dot | 点状准心 |
| 2 | Circle | 圆形准心 |
| 3 | TShape | T形准心（倒T） |
| 4 | XShape | X形准心 |
| 5 | CustomImage | 自定义图片 |

### AppState

应用状态枚举：

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Idle | 空闲状态 |
| 1 | Running | 运行中 |
| 2 | GameDetected | 检测到游戏 |

## 详细文档

- [数据模型](data-model.md) - 所有模型的详细字段说明
- [坑点](pitfalls.md) - 已知问题和注意事项
