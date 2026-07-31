# CrosshairPro.Services 数据模型

## 模型概览

Services 模块不定义新模型，而是实现 Core 层定义的接口。本文档描述数据持久化格式和序列化行为。

## 持久化模型

### CrosshairConfig（准心配置）

**文件**: `config.json` 或 `{preset-id}.json`

| 属性 | 类型 | JSON 键 | 默认值 | 说明 |
|------|------|---------|--------|------|
| Id | string | id | GUID | 配置唯一标识 |
| Name | string | name | "默认配置" | 配置名称 |
| Style | CrosshairStyle | style | 0 | 准心样式（枚举整数） |
| Size | int | size | 20 | 准心大小（像素） |
| Gap | int | gap | 4 | 中心间隙（像素） |
| Thickness | int | thickness | 2 | 线条厚度（像素） |
| Color | string | color | "#00FF00" | 主色（十六进制） |
| Opacity | int | opacity | 100 | 不透明度（0-100） |
| Brightness | int | brightness | 100 | 亮度（0-100） |
| CenterSize | int | centerSize | 4 | 中心点大小（像素） |
| Rotation | int | rotation | 0 | 旋转角度（度） |
| CustomImagePath | string? | customImagePath | null | 自定义图片路径 |
| Effects | EffectsConfig | effects | {} | 效果配置 |
| Display | DisplayConfig | display | {} | 显示配置 |

**序列化特性**:
- 使用 camelCase 命名策略
- 空值属性不序列化（`JsonIgnoreCondition.WhenWritingNull`）

### EffectsConfig（效果配置）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Outline | OutlineConfig | enabled=true | 描边效果 |
| Shadow | ShadowConfig | enabled=false | 阴影效果 |
| Glow | GlowConfig | enabled=false | 发光效果 |

#### OutlineConfig
| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | true | 是否启用 |
| Color | string | "#000000" | 描边颜色 |
| Thickness | int | 1 | 描边厚度 |

#### ShadowConfig
| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#000000" | 阴影颜色 |
| Blur | int | 3 | 模糊半径 |
| OffsetX | int | 0 | X偏移 |
| OffsetY | int | 2 | Y偏移 |

#### GlowConfig
| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Enabled | bool | false | 是否启用 |
| Color | string | "#00FFFF" | 发光颜色 |
| Intensity | int | 50 | 强度（0-100） |
| Range | int | 10 | 范围（像素） |

### DisplayConfig（显示配置）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Monitor | string | "primary" | 目标显示器 |
| ClickThrough | bool | true | 鼠标穿透 |
| AlwaysOnTop | bool | true | 置顶显示 |
| PositionX | int | 0 | X偏移 |
| PositionY | int | 0 | Y偏移 |

### AppPersistedState（应用状态）

**文件**: `appstate.json`

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| CurrentPresetId | string? | null | 当前预设ID（null表示默认配置） |
| IsConfigModified | bool | false | 配置是否已修改未保存 |

### Preset（预设）

**文件**: `presets/{preset-id}.json`

| 属性 | 类型 | JSON 键 | 说明 |
|------|------|---------|------|
| Id | string | id | 预设唯一标识（文件名） |
| Name | string | name | 预设名称 |
| Config | CrosshairConfig | config | 内嵌配置对象 |
| GameAssociation | string? | gameAssociation | 关联游戏名 |
| HotkeyBinding | string? | hotkeyBinding | 绑定热键 |
| CreatedAt | DateTime | createdAt | 创建时间（UTC） |
| UpdatedAt | DateTime | updatedAt | 更新时间（UTC） |
| IsDefault | bool | isDefault | 是否为默认预设 |

## 序列化配置

```csharp
_jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
```

**关键行为**:
- **WriteIndented**: 输出格式化 JSON，便于人工阅读和调试
- **CamelCase**: 属性名转换为 camelCase（`Opacity` -> `opacity`）
- **WhenWritingNull**: 空值属性不输出，减少文件大小

## 数据流向图

```
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│                ConfigurationService                          │
│                PresetService                                 │
└─────────────────────┬───────────────────────────────────────┘
                      │ 接口调用
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                       Services Layer                         │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │JsonConfigRepo    │  │JsonPresetRepo    │                 │
│  │- config.json     │  │- presets/*.json  │                 │
│  │- appstate.json   │  │                  │                 │
│  └────────┬─────────┘  └────────┬─────────┘                 │
│           │ SemaphoreSlim      │ SemaphoreSlim              │
│           ▼                    ▼                             │
│  ┌──────────────────────────────────────────┐               │
│  │            File System                    │               │
│  │  %APPDATA%/CrosshairPro/                  │               │
│  └──────────────────────────────────────────┘               │
└─────────────────────────────────────────────────────────────┘
```

## 渲染数据流

```
OverlayWindow.OnRender()
       │
       ▼
CrosshairRenderer.Render(drawingContext, config, width, height)
       │
       ├── 解析配置（样式、效果、位置）
       │
       ├── 获取/创建缓存资源
       │   ├── GetOrCreatePen(color, thickness, opacity)
       │   └── GetOrCreateBrush(color, opacity)
       │
       ├── 应用变换（旋转、偏移）
       │
       ├── 按样式渲染
       │   ├── RenderCross()
       │   ├── RenderDot()
       │   ├── RenderCircle()
       │   ├── RenderTShape()
       │   ├── RenderXShape()
       │   └── RenderCustomImage()
       │
       └── 触发 RenderCompleted 事件
```

## 默认配置创建

```csharp
private static CrosshairConfig CreateDefaultConfig()
{
    return new CrosshairConfig
    {
        Name = "默认配置",
        Style = CrosshairStyle.Cross,
        Size = 20,
        Gap = 4,
        Thickness = 2,
        Color = "#00FF00",
        Opacity = 100,
        CenterSize = 4
    };
}
```

**触发时机**:
- 配置文件不存在
- 配置文件解析失败
- 手动调用 `ResetToDefaultAsync()`

## 预设导入处理

```csharp
public async Task<Preset> ImportPresetAsync(string filePath)
{
    var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions);

    // 强制生成新ID，避免与现有预设冲突
    preset.Id = Guid.NewGuid().ToString();
    preset.CreatedAt = DateTime.UtcNow;
    preset.UpdatedAt = DateTime.UtcNow;

    await SavePresetAsync(preset);
    return preset;
}
```

**导入策略**:
- 总是生成新 ID，防止 ID 冲突
- 重置时间戳为当前时间
- 保留原配置内容不变