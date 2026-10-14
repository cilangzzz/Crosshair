# 预设服务 API — IPresetService

预设管理的业务入口。用户预设持久化为 `%APPDATA%/CrosshairPro/presets/{id}.json`，当前使用的预设ID持久化到 `%APPDATA%/CrosshairPro/appstate.json`。

## 基本信息

| 项 | 值 |
|----|----|
| 接口定义 | `src/CrosshairPro.Application/Interfaces/IPresetService.cs` |
| 实现类 | `src/CrosshairPro.Application/Services/PresetService.cs` |
| DI 注册 | `services.AddSingleton<IPresetService, PresetService>()` |
| 依赖 | `IPresetRepository`（预设文件）、`IAppStateRepository`（应用状态） |
| 事件 | 无 |

## 方法列表

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `LoadAllPresetsAsync()` | `Task<IReadOnlyList<Preset>>` | 加载所有预设，开头插入动态生成的默认预设 |
| `SavePresetAsync(preset)` | `Task` | 保存预设（默认预设静默忽略） |
| `DeletePresetAsync(presetId)` | `Task` | 删除预设（`"default"` 静默忽略） |
| `ImportPresetAsync(filePath)` | `Task<Preset>` | 从文件导入预设（仓库强制生成新ID） |
| `ExportPresetAsync(preset, filePath)` | `Task` | 导出预设到文件（保持原ID，直接写文件） |
| `SetCurrentPresetAsync(presetId)` | `Task` | 记录当前预设ID到 appstate.json |
| `GetCurrentPresetIdAsync()` | `Task<string?>` | 读取当前预设ID |

## 方法详情

### LoadAllPresetsAsync()

- **签名**: `Task<IReadOnlyList<Preset>> LoadAllPresetsAsync()`
- **参数**: 无
- **返回值**: 预设列表，`list[0]` 始终是默认预设
- **行为**: 调用 `_presetRepository.LoadPresetsAsync()` 后按 `CreatedAt` **降序**排列用户预设（最新在前），再在索引 0 处插入 `new Preset { Id = "default", Name = "默认配置", Config = new CrosshairConfig(), IsDefault = true }`。默认预设不落盘，每次调用都生成新实例（见 [pitfalls.md](pitfalls.md) 坑点 3）。

### SavePresetAsync(Preset preset)

- **签名**: `Task SavePresetAsync(Preset preset)`
- **参数**: `preset` — 待保存预设
- **返回值**: `Task`
- **行为**: `preset.IsDefault == true` 时直接返回（不抛异常，见坑点 4）；否则经 `IPresetRepository` 写入 `presets/{id}.json`。

### DeletePresetAsync(string presetId)

- **签名**: `Task DeletePresetAsync(string presetId)`
- **参数**: `presetId` — 预设ID
- **返回值**: `Task`
- **行为**: `presetId == "default"` 时直接返回；否则经仓库删除对应预设文件。

### ImportPresetAsync(string filePath)

- **签名**: `Task<Preset> ImportPresetAsync(string filePath)`
- **参数**: `filePath` — 导入文件路径
- **返回值**: 已保存的新预设（新ID）
- **行为**: 委托 `IPresetRepository.ImportPresetAsync()` — 反序列化后强制 `Id = Guid.NewGuid()`、刷新 `CreatedAt` / `UpdatedAt` 并立即保存（见 [pitfalls.md](pitfalls.md) 坑点 5）。

### ExportPresetAsync(Preset preset, string filePath)

- **签名**: `Task ExportPresetAsync(Preset preset, string filePath)`
- **参数**: `preset` — 待导出预设；`filePath` — 目标文件路径
- **返回值**: `Task`
- **行为**: 用内部 `_jsonOptions` 序列化后直接 `File.WriteAllTextAsync`，**不经过仓库**，因此保留原 ID。

### SetCurrentPresetAsync(string presetId)

- **签名**: `Task SetCurrentPresetAsync(string presetId)`
- **参数**: `presetId` — 当前使用的预设ID
- **返回值**: `Task`
- **行为**: `LoadStateAsync()` → `state.CurrentPresetId = presetId` → `SaveStateAsync()`，立即持久化。切换预设后必须调用，否则应用重启后不恢复（见 [pitfalls.md](pitfalls.md) 坑点 6）。

### GetCurrentPresetIdAsync()

- **签名**: `Task<string?> GetCurrentPresetIdAsync()`
- **参数**: 无
- **返回值**: `appstate.json` 中的 `CurrentPresetId`，可能为 null

## 导出 JSON 格式

预设导出使用的序列化配置：

```csharp
new JsonSerializerOptions
{
    WriteIndented = true,                              // 格式化输出
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // 忽略 null
};
```

导出文件示例：

```json
{
  "id": "preset-guid",
  "name": "我的预设",
  "config": {
    "style": 0,
    "size": 20
  },
  "isDefault": false
}
```

`CrosshairConfig` 字段语义见 [../Core/README.md](../Core/README.md)。

## 主要调用方

| 调用方 | 使用的成员 |
|--------|-----------|
| `CrosshairViewModel`（`src/CrosshairPro.App/ViewModels/CrosshairViewModel.cs`） | `GetCurrentPresetIdAsync()` / `LoadAllPresetsAsync()`（初始化恢复）、`SavePresetAsync()`（另存）、`DeletePresetAsync()`、`ImportPresetAsync()` / `ExportPresetAsync()`、`SetCurrentPresetAsync()`（切换预设） |
| `MainViewModel`（`src/CrosshairPro.App/ViewModels/MainViewModel.cs`） | 仅转发到 `CrosshairViewModel`（`SavePresetWithNameAsync` / `ImportPresetFromFileAsync` / `ExportPresetToFileAsync`） |
