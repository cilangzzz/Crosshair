# CrosshairPro.Services

数据层模块，提供配置持久化和准心渲染的具体实现。

## 概述

Services 模块是准心配置/预设的主持久化路径，实现 Core 层定义的仓库与渲染接口：

> 注意：本模块并非唯一直写文件的模块。预设导出由 Application 层 `PresetService.ExportPresetAsync` 直接 `File.WriteAllTextAsync` 写目标文件（预设内含 CrosshairConfig，属准心相关）；App 层 `CrosshairPage` 另有调试日志直写 `%APPDATA%/CrosshairPro/crosshair_page.log`。

- **主配置与应用状态持久化**：`JsonConfigRepository` 实现 `IConfigRepository` + `IAppStateRepository`（`config.json`、`appstate.json`）
- **预设持久化**：`JsonPresetRepository` 实现 `IPresetRepository`（`presets/{id}.json`）
- **准心渲染**：`CrosshairRenderer` 实现 `ICrosshairRenderer`（WPF DrawingContext 绘制）

业务逻辑在 `CrosshairPro.Application` 层，本模块只负责具体实现（接口见 [../Core/README.md](../Core/README.md)，业务服务见 [../Application/README.md](../Application/README.md)）。

## 目录结构

```
CrosshairPro.Services/
├── Configuration/
│   ├── JsonConfigRepository.cs   # 主配置+应用状态仓库（IConfigRepository + IAppStateRepository）
│   └── JsonPresetRepository.cs   # 预设仓库（IPresetRepository），独立文件锁
├── Crosshair/
│   └── CrosshairRenderer.cs      # 准心渲染器（ICrosshairRenderer），带 Pen/Brush 缓存
└── CrosshairPro.Services.csproj  # net8.0-windows，UseWPF=true
```

源文件共 3 个 `.cs`。（工作区中可见的 `GameDetection/`、`Hotkey/` 为空目录，未被 git 跟踪，属预留占位。）

## 配置文件布局

配置目录：`%APPDATA%/CrosshairPro/`

| 文件 | 写入者 | 内容 |
|------|--------|------|
| `config.json` | JsonConfigRepository | 当前准心配置（CrosshairConfig） |
| `appstate.json` | JsonConfigRepository | 应用状态（当前预设 ID、修改标记、界面语言） |
| `presets/{id}.json` | JsonPresetRepository | 每个预设一个文件 |
| `gameconfigs/{gameId}.json` | **Application 层 GameConfigService** | 各游戏配置（GameConfig），不经过本模块 |

> GameConfig 持久化目前由 Application 层 `GameConfigService` 直接完成，未下沉到本模块（详见 [../Application/README.md](../Application/README.md)）。游戏配置详情模型见 [../Core/README.md](../Core/README.md)。

## 核心类

### JsonConfigRepository

实现 `IConfigRepository` + `IAppStateRepository`，一个类管理两个文件、共享一把 `SemaphoreSlim` 锁。

| 方法 | 说明 |
|------|------|
| `LoadConfigAsync()` | 加载主配置；文件不存在或解析失败返回默认配置 |
| `SaveConfigAsync(config)` | 保存配置到 `config.json` |
| `ResetToDefaultAsync()` | 返回默认配置（不写文件） |
| `ExportConfigAsync(path, config)` | 导出配置到任意路径（不加锁） |
| `ImportConfigAsync(path)` | 从任意路径导入配置（不加锁） |
| `LoadStateAsync()` | 加载 `appstate.json`；不存在/失败返回默认状态 |
| `SaveStateAsync(state)` | 保存 `appstate.json` |

### JsonPresetRepository

实现 `IPresetRepository`，从 JsonConfigRepository 拆出的预设专职仓库，持有自己独立的 `SemaphoreSlim` 锁。

| 方法 | 说明 |
|------|------|
| `LoadPresetsAsync()` | 扫描 `presets/*.json`，解析失败的文件静默跳过 |
| `SavePresetAsync(preset)` | 保存前自动刷新 `UpdatedAt`（UTC） |
| `DeletePresetAsync(id)` | 删除预设文件（不存在则无操作） |
| `GetPresetAsync(id)` | 读取单个预设，不存在返回 null |
| `ExportPresetAsync(id, path)` | 预设不存在抛 `InvalidOperationException`（读取加锁，写入不加锁） |
| `ImportPresetAsync(path)` | 强制生成新 GUID、重置时间戳后保存（读取不加锁） |

### CrosshairRenderer

`sealed` 类，实现 `ICrosshairRenderer`。`Render(drawingContext, config, width, height)` 按样式分发到六个私有方法：

| 样式 | 绘制方式 |
|------|----------|
| `Cross` | 上下左右四条线 |
| `Dot` | 实心圆（半径 `CenterSize/2`） |
| `Circle` | 圆环（半径 `Size/2`）+ 可选中心点 |
| `TShape` | 上方竖线 + 横线（共 2 条线） |
| `XShape` | 四条对角线 |
| `CustomImage` | 按 `Size/100` 缩放绘制图片 |

渲染前置处理：先应用 `Display.PositionX/Y` 位置偏移，`Rotation != 0` 时 `PushTransform(RotateTransform)`，绘制完成后 `Pop()`。线类与 Circle 样式逐层绘制阴影（`Opacity*0.5` 偏移副本）→ 描边（线宽 `Thickness + Outline.Thickness*2`）→ 主线；**Dot 例外**：描边 pen 线宽为 `Outline.Thickness`（不套用上述公式），且椭圆半径外扩 `Outline.Thickness`，中心填充半径仍为 `CenterSize/2`。自定义图片不支持阴影/描边。

> 接线状态：`CrosshairRenderer` 目前仅注册于 DI、无上层消费方，覆盖层实际绘制路径是 `OverlayWindow.RenderCrosshair()`（WPF Shape 元素），详见 [data-model.md](data-model.md) 的「CrosshairRenderer 类内部流程」。

| 成员 | 说明 |
|------|------|
| `RenderCompleted` 事件 | 每次渲染后触发（`RenderTimeMs` 恒为 0，未实现计时） |
| `ClearCache()` | 清空三类缓存 |

## 职责边界与文件锁

- **JsonConfigRepository**：一把锁覆盖 `config.json` 与 `appstate.json` 的全部读写；两接口在 DI 中解析为**同一实例**，保证跨接口操作互斥。
- **JsonPresetRepository**：独立的锁，只覆盖 `presets/` 目录操作；与 JsonConfigRepository 的锁**互不共享**（操作不同文件，无需互斥）。
- 两个仓库的 `Export*/Import*` 方法部分步骤不加锁，见 [pitfalls.md](pitfalls.md)。

## 依赖关系

```
Services
├── Core          # 接口 + 模型（IConfigRepository、CrosshairConfig 等）
├── Infrastructure# csproj 项目引用（当前源码未直接使用其类型）
└── WPF           # UseWPF：DrawingContext / Pen / Brush / BitmapImage
```

NuGet 包引用：`Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2`、`Serilog 4.1.0`（两者当前均未被本模块源码使用，Serilog 实际使用方为 App 层）。

## 服务注册

```csharp
// Application/DI/ServiceCollectionExtensions.cs
services.AddSingleton<JsonConfigRepository>();
services.AddSingleton<IConfigRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());
services.AddSingleton<IAppStateRepository>(sp => sp.GetRequiredService<JsonConfigRepository>()); // 同一实例
services.AddSingleton<IPresetRepository, JsonPresetRepository>();
services.AddSingleton<ICrosshairRenderer, CrosshairRenderer>();
```

全部为 Singleton，注册代码位于 Application 层（详见 [../Application/README.md](../Application/README.md)）。

## 详细文档

- [数据模型](data-model.md) - JSON 文件结构、序列化配置、文件锁与缓存细节
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史
