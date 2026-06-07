# Services 服务层文档

## 概述

`CrosshairPro.Services` 是项目的业务服务层，实现了核心业务逻辑，包括准星渲染引擎和配置/预设的 JSON 文件持久化。

**项目路径**：`src/CrosshairPro.Services/`

**目标框架**：net8.0-windows

**依赖**：CrosshairPro.Core, CrosshairPro.Infrastructure

---

## 目录结构

```
CrosshairPro.Services/
├── Configuration/
│   ├── JsonConfigRepository.cs    # JSON 配置仓库实现
│   └── JsonPresetRepository.cs    # JSON 预设仓库实现
├── Crosshair/
│   └── CrosshairRenderer.cs       # 准星渲染引擎
├── GameDetection/                 # 游戏检测服务（待实现）
└── Hotkey/                        # 热键服务（待实现）
```

---

## CrosshairRenderer

准星渲染引擎，实现 `ICrosshairRenderer` 接口，使用 WPF `DrawingContext` 绘制各种样式的准星。

**文件**：`src/CrosshairPro.Services\Crosshair\CrosshairRenderer.cs`

### 功能特性

- 支持 6 种准星样式（十字、圆点、圆形、T 形、X 形、自定义图片）
- 支持描边效果
- 支持阴影效果
- 支持旋转
- 支持位置偏移
- Pen/Brush 缓存和 Freeze() 优化
- 基于 WPF DrawingContext 的高质量渲染

### 方法

#### Render

```csharp
public void Render(object drawingContext, CrosshairConfig config, double width, double height)
```

主渲染方法，根据配置绘制准星。

**参数**：
- `drawingContext`：WPF DrawingContext 对象
- `config`：准星配置
- `width`：渲染区域宽度
- `height`：渲染区域高度

**渲染流程**：
1. 创建主画笔（SolidColorBrush，应用透明度）
2. 应用旋转变换（如果有 Rotation）
3. 应用位置偏移（如果有 PositionX/Y）
4. 根据 Style 调用对应的渲染方法
5. 触发 RenderCompleted 事件

### 样式渲染方法

#### RenderCross（十字准星）

绘制四条线段组成十字形。

```
        │
        │
   ─────┼─────
        │
        │
```

- 水平线：从中心向左/右各延伸 Size/2，中间留 Gap 间距
- 垂直线：从中心向上/下各延伸 Size/2，中间留 Gap 间距
- 支持描边效果

#### RenderDot（圆点准星）

绘制一个填充的圆形。

```
        ●
```

- 大小由 Size 属性控制
- 完全填充，无描边

#### RenderCircle（圆形准星）

绘制一个空心圆形，可选中心点。

```
        ○
```

- 大小由 Size 属性控制
- 线条粗细由 Thickness 控制
- 可选中心点（CenterSize > 0 时绘制）

#### RenderTShape（T 形准星）

绘制 T 形准星。

```
   ─────────
        │
        │
        │
```

- 顶部水平线 + 底部垂直线
- 支持描边效果

#### RenderXShape（X 形准星）

绘制四条对角线组成 X 形。

```
    ╲   ╱
      ╳
    ╱   ╲
```

- 四条对角线从中心向外延伸
- 支持描边效果

#### RenderCustomImage（自定义图片准星）

加载并绘制自定义图片。

- 从 `config.CustomImagePath` 加载图片
- 使用 BitmapImage 和 BitmapCacheBrush
- 大小按 Size/100 比例缩放
- 应用透明度

### 效果渲染

#### 描边效果（Outline）

在主绘制之前绘制一个更粗、颜色不同的底层图形。

```
实现方式：
1. 创建描边画笔（OutlineConfig.Color, OutlineConfig.Thickness + Thickness）
2. 在主绘制之前绘制描边层
3. 然后在上面绘制主图形
```

#### 阴影效果（Shadow）

在主绘制之前绘制一个偏移的阴影副本。

```
实现方式：
1. 创建阴影画笔（ShadowConfig.Color, 应用模糊）
2. 偏移 (ShadowConfig.OffsetX, ShadowConfig.OffsetY)
3. 在主绘制之前绘制阴影层
```

### 性能优化

- **Pen/Brush 缓存**：使用 Dictionary 缓存已创建的 Pen 和 Brush 对象
- **Freeze()**：调用 Freeze() 使画笔/画刷不可变，可在多线程间共享
- **BitmapCacheBrush**：自定义图片使用 BitmapCacheBrush 缓存

---

## JsonConfigRepository

JSON 配置仓库，实现 `IConfigRepository` 接口，将配置持久化为 JSON 文件。

**文件**：`src/CrosshairPro.Services\Configuration\JsonConfigRepository.cs`

### 存储位置

```
%APPDATA%/CrosshairPro/config.json
```

### 功能特性

- 异步文件操作
- 使用 SemaphoreSlim 处理并发访问
- 自动创建目录
- 错误时返回默认配置
- 支持导入/导出

### 方法

#### LoadConfigAsync

```csharp
public async Task<CrosshairConfig> LoadConfigAsync()
```

从文件加载配置。

**行为**：
1. 检查文件是否存在
2. 读取文件内容
3. 反序列化为 CrosshairConfig
4. 如果任何步骤失败，返回默认配置

#### SaveConfigAsync

```csharp
public async Task SaveConfigAsync(CrosshairConfig config)
```

保存配置到文件。

**行为**：
1. 获取信号量（线程安全）
2. 创建目录（如果不存在）
3. 序列化配置为 JSON
4. 写入文件

#### ResetToDefaultAsync

```csharp
public async Task ResetToDefaultAsync()
```

重置配置为默认值。

#### ExportConfigAsync

```csharp
public async Task ExportConfigAsync(string filePath)
```

导出配置到指定路径。

#### ImportConfigAsync

```csharp
public async Task<CrosshairConfig> ImportConfigAsync(string filePath)
```

从指定路径导入配置。

### 默认配置

```json
{
  "style": "Cross",
  "size": 20,
  "gap": 4,
  "thickness": 2,
  "color": "#00FF00",
  "opacity": 100,
  "centerSize": 4
}
```

### JSON 序列化选项

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};
```

---

## JsonPresetRepository

JSON 预设仓库，实现 `IPresetRepository` 接口，将预设持久化为独立的 JSON 文件。

**文件**：`src/CrosshairPro.Services\Configuration\JsonPresetRepository.cs`

### 存储位置

```
%APPDATA%/CrosshairPro/presets/{id}.json
```

每个预设保存为独立文件，文件名为预设 ID。

### 功能特性

- 每个预设一个文件
- 异步文件操作
- 使用 SemaphoreSlim 处理并发访问
- 导入时生成新 ID 避免冲突

### 方法

#### LoadPresetsAsync

```csharp
public async Task<IEnumerable<Preset>> LoadPresetsAsync()
```

加载所有预设。

**行为**：
1. 扫描 presets 目录中的所有 .json 文件
2. 逐个反序列化
3. 返回预设列表

#### SavePresetAsync

```csharp
public async Task SavePresetAsync(Preset preset)
```

保存预设。

**行为**：
1. 获取信号量
2. 更新 UpdatedAt 时间戳
3. 序列化并写入 `{id}.json`

#### DeletePresetAsync

```csharp
public async Task DeletePresetAsync(string presetId)
```

删除预设。

#### GetPresetAsync

```csharp
public async Task<Preset?> GetPresetAsync(string presetId)
```

获取单个预设。

#### ExportPresetAsync

```csharp
public async Task ExportPresetAsync(string presetId, string filePath)
```

导出预设到指定路径。

#### ImportPresetAsync

```csharp
public async Task<Preset> ImportPresetAsync(string filePath)
```

从指定路径导入预设。

**行为**：
1. 读取文件
2. 反序列化为 Preset
3. 生成新的 GUID 作为 ID（避免冲突）
4. 保存到 presets 目录

---

## 待实现模块

### GameDetection

游戏检测服务，计划实现：
- 进程监控（WMI 或轮询）
- 游戏配置文件匹配
- 自动切换预设

### Hotkey

热键服务层，计划实现：
- 热键配置管理
- 预设关联热键
- 热键冲突检测
