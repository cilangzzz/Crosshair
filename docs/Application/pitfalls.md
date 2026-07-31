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
// MainViewModel 初始化
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

`MainViewModel` 使用异步初始化模式：

```csharp
public MainViewModel(...)
{
    _config = _configService.GetCurrentConfig();  // 同步获取配置
    _ = InitializeAsync().ConfigureAwait(false);  // 异步初始化预设
}

private async Task InitializeAsync()
{
    _isInitializing = true;
    var presets = await _presetService.LoadAllPresetsAsync();
    // ...
    _isInitializing = false;
}
```

**注意点**:
- 构造函数完成时，`InitializeAsync()` 可能还在执行
- 使用 `_isInitializing` 标志防止初始化期间的副作用
- `ConfigureAwait(false)` 避免死锁

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

## 11. CopyConfig 不会触发 PropertyChanged

`CopyConfig()` 直接设置字段值，不会触发 MVVM 的 `PropertyChanged` 事件：

```csharp
public void CopyConfig(CrosshairConfig source, CrosshairConfig target)
{
    target.Name = source.Name;  // 直接设置字段，触发 ObservableProperty 的通知
    // ...
}
```

**注意**: 由于 `CrosshairConfig` 使用 `[ObservableProperty]`，字段赋值会触发通知，但需要确认所有嵌套对象的属性也使用了 `[ObservableProperty]`。