# CrosshairPro.Services - 数据模型

本模块不定义新模型，而是持久化 Core 层模型（模型字段详见 [../Core/README.md](../Core/README.md)）。本文档描述 JSON 文件结构与序列化行为。

## 配置目录布局

`%APPDATA%/CrosshairPro/`：

```
CrosshairPro/
├── config.json               # 当前准心配置 ← JsonConfigRepository
├── appstate.json             # 应用状态     ← JsonConfigRepository
├── presets/{id}.json         # 预设         ← JsonPresetRepository
└── gameconfigs/{gameId}.json # 游戏配置     ← Application 层 GameConfigService
```

> `gameconfigs/` 由 Application 层 `GameConfigService` 直接读写（自带内存缓存与序列化选项），不经过本模块的仓库接口。GameConfig 模型与策略定义见 [../Core/README.md](../Core/README.md)，服务行为见 [../Application/README.md](../Application/README.md)。

## config.json（CrosshairConfig）

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

要点：
- `style` 是 `CrosshairStyle` 枚举的整数值（Cross=0, Dot=1, Circle=2, TShape=3, XShape=4, CustomImage=5）
- `customImagePath` 为 null 时整键省略（`WhenWritingNull`）
- 阴影模糊字段是 `blur`（模型属性 `ShadowConfig.Blur`，camelCase 后为 `blur`，不是 `blurRadius`）

## appstate.json（AppPersistedState）

`JsonConfigRepository`（实现 `IAppStateRepository`）将 Core 层 `AppPersistedState` 模型持久化为 `appstate.json`。模型属性、类型与默认值详见 [../Core/data-model.md](../Core/data-model.md) 的「AppPersistedState」一节，此处不重复。

```json
{
  "currentPresetId": "preset-guid-or-null",
  "isConfigModified": false,
  "language": "zh-CN"
}
```

调用方：`PresetService` 维护 `CurrentPresetId`；App 启动时读取 `Language` 初始化本地化，MainWindow 切换语言时回写（见 [../App/README.md](../App/README.md)）。

## presets/{id}.json（Preset）

```json
{
  "id": "preset-guid",
  "name": "我的狙击预设",
  "config": { "id": "...", "name": "...", "style": 0, "...": "CrosshairConfig 完整结构" },
  "gameAssociation": "Counter-Strike 2",
  "hotkeyBinding": "Ctrl+Shift+C",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z",
  "isDefault": false
}
```

- 文件名 = 预设 ID（`{preset.Id}.json`），见 [pitfalls.md](pitfalls.md) 的命名规则
- `SavePresetAsync` 每次保存前自动刷新 `UpdatedAt = DateTime.UtcNow`
- `ImportPresetAsync` 强制生成新 GUID 并重置 `CreatedAt`/`UpdatedAt`

## JsonSerializerOptions

`JsonConfigRepository` 与 `JsonPresetRepository` 使用完全相同的配置：

```csharp
_jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,                               // 格式化输出
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 驼峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // null 不输出
};
```

对比：Application 层 `GameConfigService` 写 `gameconfigs/*.json` 时只设 `WriteIndented = true`（无 camelCase，键保持 PascalCase），两个 JSON 体系命名风格不一致。

## 文件锁机制

两个仓库各自持有 `SemaphoreSlim(1,1)`，互不共享：

| 锁 | 覆盖范围 |
|----|----------|
| `JsonConfigRepository._fileLock` | `config.json` + `appstate.json` 的 Load/Save（两接口共享同一实例与锁） |
| `JsonPresetRepository._fileLock` | `presets/` 目录的 Load/Save/Delete/Get |

```csharp
await _fileLock.WaitAsync();
try { /* 文件操作 */ }
finally { _fileLock.Release(); }
```

**不加锁的方法**（当前实现）：
- `JsonConfigRepository.ExportConfigAsync` / `ImportConfigAsync`
- `JsonPresetRepository.ExportPresetAsync`（读取走加锁的 `GetPresetAsync`，写目标文件不加锁）
- `JsonPresetRepository.ImportPresetAsync`（读源文件不加锁，保存走加锁的 `SavePresetAsync`）

## 渲染器缓存

```csharp
private readonly Dictionary<string, Pen> _penCache = new();      // 键: {color}_{thickness}_{opacity}
private readonly Dictionary<string, Brush> _brushCache = new();  // 键: {color}_{opacity}
private readonly Dictionary<string, Geometry> _geometryCache = new(); // 声明但从未写入，仅 ClearCache 时清空
```

- 画刷透明度以 `opacity / 100.0` 设置（配置中 opacity 取值 0-100）
- 资源创建后立即 `Freeze()`，跨线程安全
- 自定义图片**不走缓存**：`RenderCustomImage` 每次渲染都新建 `BitmapImage`

## 数据流向图

```
┌──────────────────────────────────────────────────────────────┐
│                      Application Layer                        │
│   ConfigurationService ──► IConfigRepository                  │
│   PresetService ──────────► IPresetRepository / IAppStateRepository │
│   GameConfigService ─────► (直接 File I/O，绕过 Services 层)   │
└──────────────────────┬───────────────────────────────────────┘
                       ▼
┌──────────────────────────────────────────────────────────────┐
│                       Services Layer                          │
│  ┌───────────────────────┐   ┌───────────────────────┐       │
│  │ JsonConfigRepository  │   │ JsonPresetRepository  │       │
│  │ 锁A: config+appstate  │   │ 锁B: presets/*        │       │
│  └───────────┬───────────┘   └───────────┬───────────┘       │
│              ▼                           ▼                    │
│        %APPDATA%/CrosshairPro/                                │
│        config.json · appstate.json · presets/{id}.json        │
│        gameconfigs/{gameId}.json ← GameConfigService 直写      │
└──────────────────────────────────────────────────────────────┘
```

## CrosshairRenderer 类内部流程

> **接线状态**：`CrosshairRenderer` 目前仅在 DI 中注册（`ServiceCollectionExtensions.cs` 注册 `ICrosshairRenderer`），**没有任何上层消费方**——全仓库无 `.Render(` 调用点，也未注入任何 ViewModel/Window。实际覆盖层绘制路径是 `OverlayWindow.RenderCrosshair()`（用 WPF Shape 元素 Line/Ellipse/Image 自绘，见 [../App/pitfalls.md](../App/pitfalls.md)）。两类渲染逻辑并存且未接线；下图描述的是渲染器自身的内部流程，**不是当前实际生效的绘制路径**。

```
CrosshairRenderer.Render(drawingContext, config, width, height)
       │
       ├── center = 画布中心 + Display.PositionX/Y
       ├── Rotation != 0 → PushTransform(RotateTransform)
       ├── 按样式分发
       │   ├── RenderCross()        # 4 条线
       │   ├── RenderDot()          # 实心圆
       │   ├── RenderCircle()       # 圆环 + 可选中心点
       │   ├── RenderTShape()       # 竖线 + 横线
       │   ├── RenderXShape()       # 4 条对角线
       │   └── RenderCustomImage()  # 图片（每帧重新加载，无效果支持）
       ├── 每条线依次绘制: 阴影(Opacity*0.5) → 描边 → 主线
       ├── Rotation != 0 → Pop()
       └── 触发 RenderCompleted（RenderTimeMs 恒为 0）
```

## 默认配置创建

`JsonConfigRepository.CreateDefaultConfig()` 触发时机：文件不存在、解析失败、反序列化结果为 null。

```csharp
return new CrosshairConfig
{
    Name = "默认配置",
    Style = CrosshairStyle.Cross,
    Size = 20, Gap = 4, Thickness = 2,
    Color = "#00FF00", Opacity = 100, CenterSize = 4
};
```

未显式赋值的字段（Id、Brightness、Rotation、Effects、Display 等）沿用模型默认值。`ResetToDefaultAsync()` 只返回默认对象不写文件，落盘由上层调用 `SaveConfigAsync` 完成。

## 预设导入处理

```csharp
public async Task<Preset> ImportPresetAsync(string filePath)
{
    var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions)
        ?? throw new InvalidOperationException("预设文件格式错误");
    preset.Id = Guid.NewGuid().ToString();   // 强制新 ID，避免冲突
    preset.CreatedAt = DateTime.UtcNow;
    preset.UpdatedAt = DateTime.UtcNow;
    await SavePresetAsync(preset);
    return preset;
}
```

导入策略：总是生成新 ID、重置时间戳、保留原配置内容；JSON 解析失败抛异常（与 LoadConfigAsync 的静默容错不同）。
