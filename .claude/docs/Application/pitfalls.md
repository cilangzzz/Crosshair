# CrosshairPro.Application - 坑点

## 1. GetCurrentConfig 返回单例引用

`ConfigurationService.GetCurrentConfig()` 返回内部单例实例的引用，不是副本：

```csharp
public CrosshairConfig GetCurrentConfig() => _currentConfig;  // 返回引用
```

**影响**:
- 多次调用返回同一实例
- UI 直接绑定此实例，修改会立即反映到服务内部
- 如果需要独立副本，必须调用 `CloneConfig()`

**正确使用**:

```csharp
// ✅ 绑定场景：直接使用返回的实例
public MainViewModel(IConfigurationService configService)
{
    Config = configService.GetCurrentConfig();  // 保持引用，修改立即生效
}

// ✅ 编辑场景：需要创建副本
var editCopy = _configService.CloneConfig(_configService.GetCurrentConfig());
// 用户编辑 editCopy，取消时不影响原配置
```

## 2. CloneConfig vs CopyConfig 区别

两个方法用途不同，使用场景各异：

| 方法 | 用途 | 返回值 | Id 处理 | 实例引用 |
|------|------|--------|---------|----------|
| `CloneConfig(source)` | 创建独立副本 | 新实例 | 生成新 GUID | 新对象 |
| `CopyConfig(source, target)` | 复制值到目标 | 无（修改目标） | 保持目标 Id | 目标实例 |

**CloneConfig 场景**:
- 创建预设副本用于编辑
- 取消编辑时恢复原配置
- 保存当前配置为新预设

```csharp
// 保存为预设时，需要克隆配置
var presetConfig = _configService.CloneConfig(Config);
presetConfig.Name = "新预设";
await _presetService.SavePresetAsync(new Preset { Config = presetConfig, ... });
```

**CopyConfig 场景**:
- 应用预设配置到当前配置
- 恢复配置时保持 UI 绑定

```csharp
// 切换预设时，保持 Config 实例引用不变
_configService.CopyConfig(preset.Config, Config);  // Config 是 ViewModel 的绑定实例
```

## 3. 默认预设是动态生成的

`PresetService.LoadAllPresetsAsync()` 自动在列表开头插入默认预设：

```csharp
var defaultPreset = new Preset
{
    Id = "default",
    Name = "默认配置",
    Config = new CrosshairConfig(),  // 使用模型默认值
    IsDefault = true
};
list.Insert(0, defaultPreset);
```

**注意点**:
- 默认预设不在文件系统中，是动态生成的
- 每次调用 `LoadAllPresetsAsync()` 都会创建新的默认预设实例
- 默认预设的 `Config` 使用 `CrosshairConfig` 的模型默认值

**UI 处理**:

```csharp
// 加载预设列表时
var presets = await _presetService.LoadAllPresetsAsync();
// presets[0] 始终是默认预设

// 删除/保存时检查 IsDefault
if (selectedPreset?.IsDefault == true)
{
    // 禁用删除和保存按钮
}
```

## 4. 默认预设不可保存/删除

服务层会静默忽略默认预设的保存和删除操作：

```csharp
public async Task SavePresetAsync(Preset preset)
{
    if (preset.IsDefault) return;  // 静默返回，不抛异常
    await _presetRepository.SavePresetAsync(preset);
}

public async Task DeletePresetAsync(string presetId)
{
    if (presetId == "default") return;  // 静默返回
    await _presetRepository.DeletePresetAsync(presetId);
}
```

**建议**: UI 层应该禁用默认预设的保存/删除按钮，而不是依赖服务层的静默忽略。

## 5. 导出预设不通过 Repository

`ExportPresetAsync` 直接写文件，不通过 `IPresetRepository`：

```csharp
public async Task ExportPresetAsync(Preset preset, string filePath)
{
    var json = JsonSerializer.Serialize(preset, _jsonOptions);
    await File.WriteAllTextAsync(filePath, json);  // 直接写文件
}
```

**原因**: `IPresetRepository.ImportPresetAsync()` 会生成新 ID，导出时需要保持原 ID。

**导入行为对比**:

```csharp
// Repository 导入会生成新 ID
public async Task<Preset> ImportPresetAsync(string filePath)
{
    var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions);
    preset.Id = Guid.NewGuid().ToString();  // 强制生成新 ID
    preset.CreatedAt = DateTime.UtcNow;
    preset.UpdatedAt = DateTime.UtcNow;
    await SavePresetAsync(preset);
    return preset;
}
```

## 6. 当前预设ID持久化

当前预设ID通过 `IAppStateRepository` 持久化到 `appstate.json`：

```csharp
public async Task SetCurrentPresetAsync(string presetId)
{
    var state = await _stateRepository.LoadStateAsync();
    state.CurrentPresetId = presetId;  // 更新状态
    await _stateRepository.SaveStateAsync(state);  // 立即保存
}
```

**注意**: 切换预设后必须调用此方法，否则应用重启后不会恢复正确的预设。

**应用启动恢复**:

```csharp
// CrosshairViewModel.InitializeAsync 初始化（fire-and-forget）
var currentPresetId = await _presetService.GetCurrentPresetIdAsync();
if (!string.IsNullOrEmpty(currentPresetId))
{
    var presets = await _presetService.LoadAllPresetsAsync();
    var preset = presets.FirstOrDefault(p => p.Id == currentPresetId);
    if (preset != null)
    {
        _configService.CopyConfig(preset.Config, Config);
    }
}
```

## 7. JsonConfigRepository 双接口共享锁

`JsonConfigRepository` 同时实现 `IConfigRepository` 和 `IAppStateRepository`：

```csharp
public class JsonConfigRepository : IConfigRepository, IAppStateRepository
{
    private readonly SemaphoreSlim _fileLock = new(1, 1);  // 共享文件锁

    // 两个接口的方法都使用同一个锁
}
```

**依赖注入配置**:

```csharp
// 必须这样注册，确保两个接口指向同一实例
services.AddSingleton<JsonConfigRepository>();
services.AddSingleton<IConfigRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());
services.AddSingleton<IAppStateRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());
```

**错误示例**:

```csharp
// ❌ 错误：分别注册会创建两个实例，锁不共享
services.AddSingleton<IConfigRepository, JsonConfigRepository>();
services.AddSingleton<IAppStateRepository, JsonConfigRepository>();
```

## 8. ViewModel 异步初始化

`CrosshairViewModel` 使用 fire-and-forget 异步初始化模式（`MainViewModel` 现在只做页面导航和事件转发，业务初始化全部在 `CrosshairViewModel`）：

```csharp
private bool _isInitializing = true;

public CrosshairViewModel(...)
{
    _config = _configService.GetCurrentConfig();  // 同步获取默认配置并绑定
    SubscribeConfigEvents(_config);
    InitializeAsync().ConfigureAwait(false);      // 异步初始化预设
}

private async Task InitializeAsync()
{
    try
    {
        _currentPresetId = await _presetService.GetCurrentPresetIdAsync();
        await LoadPresets();
        // ... 恢复上次使用的预设
    }
    finally
    {
        _isInitializing = false;
    }
}
```

**注意点**:
- 构造函数完成时，`InitializeAsync()` 可能还在执行
- 使用 `_isInitializing` 标志防止初始化期间触发保存等副作用（如预设选择联动、自动保存判断）
- `ConfigureAwait(false)` 避免 UI 上下文死锁

## 9. 配置变更不会自动保存

`ConfigurationService` 不会监听配置变更，需要手动调用保存：

```csharp
// ❌ 错误：修改配置后不保存，重启后丢失
Config.Size = 30;

// ✅ 正确：修改后显式保存
Config.Size = 30;
await _configService.SaveConfigAsync();
```

**建议**: 使用 `IsModified` 标志跟踪变更，在适当时机提示用户保存：

```csharp
partial void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    IsModified = true;  // 标记修改
}
```

## 10. 深拷贝必须递归克隆所有嵌套对象

`CloneConfig()` 必须递归克隆 `Effects` 和 `Display`：

```csharp
// ❌ 错误：浅拷贝，嵌套对象仍引用原实例
return new CrosshairConfig
{
    Effects = source.Effects  // 共享引用！
};

// ✅ 正确：深拷贝，创建新的嵌套对象
return new CrosshairConfig
{
    Effects = new EffectsConfig
    {
        Outline = new OutlineConfig
        {
            Enabled = source.Effects.Outline.Enabled,
            Color = source.Effects.Outline.Color,
            Thickness = source.Effects.Outline.Thickness
        },
        // ... 其他效果
    }
};
```

**风险**: 浅拷贝会导致修改副本时影响原配置，引发难以追踪的 bug。

## 11. CopyConfig 会触发逐属性 PropertyChanged，但不会整体替换实例

`CopyConfig()` 逐属性赋值，每次赋值都会触发对应属性的 `PropertyChanged`：`CrosshairConfig` 及全部嵌套模型（`OutlineConfig` / `ShadowConfig` / `GlowConfig` / `EffectsConfig` / `DisplayConfig`）均为 `ObservableObject` + `[ObservableProperty]`，已逐一确认：

```csharp
public void CopyConfig(CrosshairConfig source, CrosshairConfig target)
{
    target.Name = source.Name;  // 触发 ObservableProperty 的 PropertyChanged 通知
    // ...
}
```

**真正的坑**: `CopyConfig` 只复制值、不整体替换实例，因此没有「配置已替换」级别的整体通知 — 目标实例上的每个属性各自发一次通知。这既是设计意图（保持 UI 绑定实例不变），也意味着依赖「整体替换」信号的逻辑感知不到这次批量变更；监听方需处理逐属性通知，或由调用方在 `CopyConfig` 之后自行发出整体变更信号（如 `ConfigUpdated` 事件）。

## 12. GameConfigService.ApplyConfigAsync 是空实现

`ApplyConfigAsync(gameId)` 接口注释为「应用配置到游戏（写入配置文件）」，但实现直接返回完成任务：

```csharp
public Task ApplyConfigAsync(string gameId)
{
    // TODO: 实现写入游戏配置文件的逻辑
    return Task.CompletedTask;  // 不写入任何游戏文件
}
```

**影响**: 调用此方法不会对游戏本身产生任何效果，配置只保存在 `%APPDATA%/CrosshairPro/gameconfigs/{gameId}.json` 中。UI 如提供「应用到游戏」按钮，实际不会写入游戏配置文件，需等待按游戏格式逐个实现。

## 13. videoconfig.txt 解析与序列化不对称（4 个键只写不读）

`ApexConfigService` 序列化 `ApexVideoConfig` 时写出 23 个 `setting.*` 键，但 `ApplyVideoConfigValue` 的解析 switch 只覆盖其中 19 个。以下 4 个键**只写不读**：

- `mat_mip_linear`
- `csm_cascade_res`
- `dvs_enable`
- `cl_ragdoll_self_collision`

**影响**: `LoadVideoConfigAsync()` → `SaveVideoConfigAsync()` 往返一次后，这 4 个字段会被重置为 `ApexVideoConfig` 模型默认值，丢失用户文件中的原值。为 `ApexVideoConfig` 新增属性时必须同时补齐序列化行和解析 case，否则往返即丢失。

## 14. GenerateLaunchOptions 返回硬编码模板

接口注释为「根据当前配置生成启动选项字符串」，实际实现返回固定模板，不读取任何配置：

```csharp
public string GenerateLaunchOptions()
{
    return "-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -novid";
}
```

**影响**: 不要期望此方法反映当前 `ApexVideoConfig` / `ApexSettingsConfig` 状态；它只是首次加载（`launch_options.txt` 不存在时）的默认值来源。

## 15. ApexConfigService 文件锁不覆盖全部方法

`_fileLock`（`SemaphoreSlim(1,1)`）只保护 Apex 存档目录文件的读写。以下成员**不加锁**：

- `ExportVideoConfigAsync` / `ExportSettingsConfigAsync`（经 Load→Serialize→Write 组合，且 Write 不持锁）
- `LoadLaunchOptionsAsync` / `SaveLaunchOptionsAsync` / `ExportLaunchOptionsAsync`（`launch_options.txt`）
- `GetVideoConfigBackups` / `GetSettingsConfigBackups`（仅枚举备份目录）
- `DeleteBackup`

**影响**: 并发场景下导出与保存可能交叉；但由于这些操作目标是不同文件或只读枚举，实际风险有限。新增直接读写 Apex 存档文件的方法时必须套用 `_fileLock.WaitAsync()` / `Release()` 模式。

## 16. GameConfigService 静默回退默认配置

`GetConfigAsync` 静默回退策略默认值、不抛异常，但两个失败分支的缓存行为不同：

- **文件不存在**: 返回 `CreateDefaultConfig(gameId)` 并**写入缓存**
- **解析异常 / 反序列化结果为 null**: 返回 `CreateDefaultConfig(gameId)` 但**不写入缓存**，下次调用会重新读文件

```csharp
catch
{
    // 忽略错误，返回默认配置（此分支不写缓存）
}
return CreateDefaultConfig(gameId);
```

**影响**: 不区分「损坏」与「首次使用」；配置文件损坏时用户修改仍会被下次保存覆盖，无任何提示。排查「用户配置丢失」问题时优先检查 `{gameId}.json` 是否可解析。另外构造函数会立即创建 `gameconfigs/` 目录，注意单测或临时环境的副作用。

## 17. 备份文件名时间戳约定

备份文件名必须保留 `yyyy-MM-dd_HH-mm-ss_` 前缀（如 `2026-08-01_12-30-00_videoconfig.txt`）：

- `GetVideoConfigBackups` / `GetSettingsConfigBackups` 依赖该前缀解析 `BackupTime` 并倒序排序
- 解析失败时回退 `File.GetCreationTime`，但回退路径拼接缺少 `videoconfig/` / `settings/` 子目录（实际文件在其下），查询的路径不存在，`File.GetCreationTime` 恒返回 `1601-01-01` 常量而非文件创建时间 — 无时间戳前缀的备份文件 `BackupTime` 是确定性的错误值（代码缺陷，待修）

**建议**: 不要手动重命名备份文件；程序化生成备份时沿用 `{timestamp}_{原文件名}` 格式。

**关联**: Apex 模块侧摘要见 [../GameConfig/Apex/pitfalls.md](../GameConfig/Apex/pitfalls.md) 坑点 8。