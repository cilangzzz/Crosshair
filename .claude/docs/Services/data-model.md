# CrosshairPro.Services - 数据模型

## 配置存储结构

### config.json

主配置文件结构：

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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
  "effects": {
    "outline": {
      "enabled": true,
      "color": "#000000",
      "thickness": 1
    },
    "shadow": {
      "enabled": false,
      "color": "#000000",
      "blurRadius": 3,
      "offsetX": 0,
      "offsetY": 2
    },
    "glow": {
      "enabled": false,
      "color": "#00FFFF",
      "intensity": 50,
      "range": 10
    }
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

### Preset 文件结构

预设文件（`presets/{id}.json`）结构：

```json
{
  "id": "preset-guid",
  "name": "我的狙击预设",
  "config": {
    // CrosshairConfig 结构（同上）
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "isDefault": false
}
```

## JsonSerializerOptions 配置

两个仓库使用相同的 JSON 配置：

```csharp
_jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,                     // 格式化输出
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

## 文件锁定机制

使用 `SemaphoreSlim` 实现异步文件锁：

```csharp
private readonly SemaphoreSlim _fileLock = new(1, 1);

public async Task SaveConfigAsync(CrosshairConfig config)
{
    await _fileLock.WaitAsync();
    try
    {
        // 文件操作
    }
    finally
    {
        _fileLock.Release();
    }
}
```

## CrosshairRenderer 缓存结构

渲染器使用三种缓存提高性能：

```csharp
private readonly Dictionary<string, Pen> _penCache = new();
private readonly Dictionary<string, Brush> _brushCache = new();
private readonly Dictionary<string, Geometry> _geometryCache = new();
```

缓存键格式：
- Pen: `{color}_{thickness}_{opacity}`
- Brush: `{color}_{opacity}`
- Geometry: `{style}_{size}_{gap}`