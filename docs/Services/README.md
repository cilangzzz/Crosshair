# CrosshairPro.Services 模块

## 概述

数据层模块，负责配置持久化、预设管理和准心渲染实现。实现了 Core 层定义的仓库接口和渲染器接口，是架构中唯一与文件系统直接交互的模块。

## 模块结构

```
CrosshairPro.Services/
├── Configuration/
│   ├── JsonConfigRepository.cs   # 配置仓库实现（同时实现 IAppStateRepository）
│   └── JsonPresetRepository.cs   # 预设仓库实现
└── Crosshair/
    └── CrosshairRenderer.cs      # 准心渲染器实现
```

## 核心组件

### JsonConfigRepository

配置仓库实现，同时实现 `IConfigRepository` 和 `IAppStateRepository` 接口。

**职责**:
- 主配置的加载/保存（`config.json`）
- 应用状态的持久化（`appstate.json`）
- 配置导入/导出功能
- 文件访问同步控制

**文件路径**:
- 配置文件: `%APPDATA%/CrosshairPro/config.json`
- 状态文件: `%APPDATA%/CrosshairPro/appstate.json`

### JsonPresetRepository

预设仓库实现，实现 `IPresetRepository` 接口。

**职责**:
- 预设的 CRUD 操作
- 预设导入/导出功能
- 预设目录管理

**文件路径**:
- 预设目录: `%APPDATA%/CrosshairPro/presets/`
- 预设文件: `{preset-id}.json`

### CrosshairRenderer

准心渲染器实现，实现 `ICrosshairRenderer` 接口。

**支持的准心样式**:
| 样式 | 枚举值 | 渲染方式 |
|------|--------|----------|
| 十字 | `Cross` | 四条直线（上/下/左/右） |
| 点状 | `Dot` | 实心圆 |
| 圆形 | `Circle` | 圆环 + 可选中心点 |
| T形 | `TShape` | 一条竖线 + 一条横线 |
| X形 | `XShape` | 四条对角线 |
| 自定义图片 | `CustomImage` | 用户提供的图片 |

**效果系统支持**:
- 描边（Outline）: 线条外围描边
- 阴影（Shadow）: 带偏移的阴影效果
- 发光（Glow）: 光晕效果（配置已定义，渲染预留）

## 依赖关系

```
Services
├── Core (接口定义、模型)
├── Infrastructure (无直接依赖)
└── WPF (PresentationCore, PresentationFramework)
```

**NuGet 依赖**:
- System.Text.Json: JSON 序列化
- PresentationCore/PresentationFramework: WPF 渲染类型

## 服务注册

```csharp
// Application/DI/ServiceCollectionExtensions.cs
services.AddSingleton<IConfigRepository, JsonConfigRepository>();
services.AddSingleton<IAppStateRepository, JsonConfigRepository>(); // 同一实例
services.AddSingleton<IPresetRepository, JsonPresetRepository>();
services.AddSingleton<ICrosshairRenderer, CrosshairRenderer>();
```

**注意**: `JsonConfigRepository` 同时注册为两个接口，确保单例共享文件锁。

## 文件格式

### config.json 示例
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
  "centerSize": 4,
  "effects": {
    "outline": { "enabled": true, "color": "#000000", "thickness": 1 },
    "shadow": { "enabled": false, "color": "#000000", "blur": 3, "offsetX": 0, "offsetY": 2 },
    "glow": { "enabled": false, "color": "#00FFFF", "intensity": 50, "range": 10 }
  },
  "display": {
    "monitor": "primary",
    "clickThrough": true,
    "alwaysOnTop": true,
    "positionX": 0,
    "positionY": 0
  }
}
```

### appstate.json 示例
```json
{
  "currentPresetId": "preset-guid-or-null",
  "isConfigModified": false
}
```

### preset.json 示例
```json
{
  "id": "preset-guid",
  "name": "CS2 准心",
  "config": { /* CrosshairConfig */ },
  "gameAssociation": "Counter-Strike 2",
  "hotkeyBinding": "Ctrl+Shift+C",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "isDefault": false
}
```

## 性能优化

### 渲染缓存
`CrosshairRenderer` 使用三个缓存字典：
- `_penCache`: 画笔缓存（颜色+厚度+透明度）
- `_brushCache`: 画刷缓存（颜色+透明度）
- `_geometryCache`: 几何图形缓存（预留）

**缓存策略**: 基于字符串键 `"颜色_参数"` 创建，使用 `Freeze()` 冻结为跨线程安全。

### 文件锁
使用 `SemaphoreSlim(1, 1)` 进行异步文件访问同步：
- 同一仓库内的读写操作互斥
- `JsonConfigRepository` 的配置和状态操作共享同一锁

## 相关文档

- [数据模型](data-model.md) - 详细的模型定义和序列化说明
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史