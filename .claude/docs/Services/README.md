# CrosshairPro.Services

数据层模块，提供配置持久化和准心渲染的具体实现。

## 概述

Services 模块实现数据持久化和渲染逻辑：
- JSON 配置文件持久化（实现 `IConfigRepository` 和 `IAppStateRepository`）
- 准心图形渲染（实现 `ICrosshairRenderer`）

**注意**：业务逻辑已迁移到 `CrosshairPro.Application` 层，此模块只负责具体实现。

## 目录结构

```
CrosshairPro.Services/
├── Configuration/
│   ├── JsonConfigRepository.cs   # 配置仓库实现
│   └── JsonPresetRepository.cs   # 预设仓库实现
└── Crosshair/
    └── CrosshairRenderer.cs      # 准心渲染器实现
```

## 核心类

### JsonConfigRepository

配置仓库实现，使用 JSON 文件存储：

**实现接口**: `IConfigRepository` + `IAppStateRepository`

| 方法 | 说明 |
|------|------|
| `LoadConfigAsync()` | 加载主配置，不存在则返回默认 |
| `SaveConfigAsync(config)` | 保存配置到文件 |
| `ResetToDefaultAsync()` | 返回默认配置 |
| `ExportConfigAsync(path, config)` | 导出配置到指定路径 |
| `ImportConfigAsync(path)` | 从文件导入配置 |
| `LoadStateAsync()` | 加载应用状态（新增） |
| `SaveStateAsync(state)` | 保存应用状态（新增） |

**存储位置**:
- 主配置: `%APPDATA%/CrosshairPro/config.json`
- 应用状态: `%APPDATA%/CrosshairPro/appstate.json`

**JSON 格式**:
```json
{
  "id": "guid",
  "name": "默认配置",
  "style": 0,
  "size": 20,
  "gap": 4,
  "thickness": 2,
  "color": "#00FF00",
  "opacity": 100,
  "brightness": 100,
  "centerSize": 4,
  "rotation": 0,
  "customImagePath": null,
  "effects": { ... },
  "display": { ... }
}
```

### JsonPresetRepository

预设仓库实现，每个预设独立存储为一个 JSON 文件：

| 方法 | 说明 |
|------|------|
| `LoadPresetsAsync()` | 加载所有预设 |
| `SavePresetAsync(preset)` | 保存预设 |
| `DeletePresetAsync(id)` | 删除预设 |
| `GetPresetAsync(id)` | 获取单个预设 |
| `ExportPresetAsync(id, path)` | 导出预设到文件 |
| `ImportPresetAsync(path)` | 从文件导入预设 |

**存储位置**: `%APPDATA%/CrosshairPro/presets/{id}.json`

### CrosshairRenderer

准心渲染器，实现 `ICrosshairRenderer` 接口：

| 方法 | 说明 |
|------|------|
| `Render(drawingContext, config, width, height)` | 渲染准心到 DrawingContext |

| 事件 | 说明 |
|------|------|
| `RenderCompleted` | 渲染完成通知 |

**渲染样式**:
- `RenderCross` - 十字准心
- `RenderDot` - 点状准心
- `RenderCircle` - 圆形准心
- `RenderTShape` - T形准心
- `RenderXShape` - X形准心
- `RenderCustomImage` - 自定义图片

**缓存机制**:
- `_penCache` - 画笔缓存
- `_brushCache` - 画刷缓存
- `_geometryCache` - 几何图形缓存

## 详细文档

- [数据模型](data-model.md) - 相关模型说明
- [坑点](pitfalls.md) - 已知问题和注意事项