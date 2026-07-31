# CrosshairPro.Core

核心层模块，定义所有模型、接口和枚举。所有其他模块都依赖此模块。

## 概述

Core 模块是整个应用的基石，提供：
- 数据模型（CrosshairConfig、Preset、EffectsConfig、AppPersistedState 等）
- 业务接口（IConfigRepository、IAppStateRepository、ICrosshairRenderer、IHotkeyManager）
- 枚举定义（CrosshairStyle、AppState）

## 目录结构

```
CrosshairPro.Core/
├── Enums/
│   ├── AppState.cs           # 应用状态枚举
│   └── CrosshairStyle.cs     # 准心样式枚举
├── Interfaces/
│   ├── IConfigRepository.cs  # 配置仓库接口 + IAppStateRepository + IPresetRepository
│   ├── ICrosshairRenderer.cs # 准心渲染器接口
│   ├── IGameDetector.cs      # 游戏检测器接口
│   └── IHotkeyManager.cs     # 热键管理器接口
└── Models/
    ├── AppPersistedState.cs  # 应用持久化状态（新增）
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

### AppPersistedState（新增）

应用持久化状态，记录应用运行时状态：

| 属性 | 类型 | 说明 |
|------|------|------|
| CurrentPresetId | string? | 当前使用的预设ID |
| IsConfigModified | bool | 当前配置是否已修改 |

### EffectsConfig

效果配置，包含三种效果：Outline（描边）、Shadow（阴影）、Glow（发光）

### Preset

预设模型，包含 CrosshairConfig 和元数据

## 接口定义

### IConfigRepository

配置持久化接口：
- `LoadConfigAsync()` / `SaveConfigAsync(config)` - 主配置管理
- `ExportConfigAsync(path, config)` / `ImportConfigAsync(path)` - 导入导出

### IAppStateRepository（新增）

应用状态持久化接口：
- `LoadStateAsync()` - 加载应用状态
- `SaveStateAsync(state)` - 保存应用状态

### IPresetRepository

预设管理接口：
- `LoadPresetsAsync()` / `SavePresetAsync(preset)` / `DeletePresetAsync(id)`
- `ExportPresetAsync(id, path)` / `ImportPresetAsync(path)`

## 枚举定义

### CrosshairStyle

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | Cross | 十字准心 |
| 1 | Dot | 点状准心 |
| 2 | Circle | 圆形准心 |
| 3 | TShape | T形准心（倒T） |
| 4 | XShape | X形准心 |
| 5 | CustomImage | 自定义图片 |

## 详细文档

- [数据模型](data-model.md) - 所有模型的详细字段说明
- [坑点](pitfalls.md) - 已知问题和注意事项