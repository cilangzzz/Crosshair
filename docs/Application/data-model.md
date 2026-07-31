# CrosshairPro.Application - 数据模型

## 服务接口

### IConfigurationService

配置管理服务接口，提供配置的加载、保存、克隆等操作。

```csharp
public interface IConfigurationService
{
    /// <summary>
    /// 获取当前配置实例（返回内部单例引用）
    /// </summary>
    CrosshairConfig GetCurrentConfig();

    /// <summary>
    /// 从持久化加载配置，替换内部实例
    /// </summary>
    Task LoadConfigAsync();

    /// <summary>
    /// 保存当前配置到持久化
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 重置为默认配置并保存
    /// </summary>
    Task ResetToDefaultAsync();

    /// <summary>
    /// 创建配置的深拷贝（生成新 GUID）
    /// </summary>
    CrosshairConfig CloneConfig(CrosshairConfig source);

    /// <summary>
    /// 复制配置值到目标实例（保持目标 Id 不变）
    /// </summary>
    void CopyConfig(CrosshairConfig source, CrosshairConfig target);

    /// <summary>
    /// 创建默认配置实例
    /// </summary>
    CrosshairConfig CreateDefaultConfig();
}
```

**使用场景**:
- `GetCurrentConfig()`: 获取当前配置用于 UI 绑定
- `CloneConfig()`: 创建配置副本用于预设保存或编辑取消
- `CopyConfig()`: 应用预设配置到当前配置，保持实例引用

### IPresetService

预设管理服务接口，提供预设的增删改查和导入导出。

```csharp
public interface IPresetService
{
    /// <summary>
    /// 加载所有预设，自动在开头插入默认预设
    /// </summary>
    Task<IReadOnlyList<Preset>> LoadAllPresetsAsync();

    /// <summary>
    /// 保存预设（默认预设不可保存）
    /// </summary>
    Task SavePresetAsync(Preset preset);

    /// <summary>
    /// 删除预设（默认预设不可删除）
    /// </summary>
    Task DeletePresetAsync(string presetId);

    /// <summary>
    /// 从文件导入预设（生成新 ID）
    /// </summary>
    Task<Preset> ImportPresetAsync(string filePath);

    /// <summary>
    /// 导出预设到文件（保持原 ID）
    /// </summary>
    Task ExportPresetAsync(Preset preset, string filePath);

    /// <summary>
    /// 设置当前使用的预设ID，持久化到 appstate.json
    /// </summary>
    Task SetCurrentPresetAsync(string presetId);

    /// <summary>
    /// 获取当前使用的预设ID
    /// </summary>
    Task<string?> GetCurrentPresetIdAsync();
}
```

**使用场景**:
- `LoadAllPresetsAsync()`: 填充预设下拉列表
- `SetCurrentPresetAsync()`: 切换预设后持久化状态
- `ImportPresetAsync()` / `ExportPresetAsync()`: 预设分享和备份

### IGameConfigService

游戏配置服务接口，管理各游戏的配置数据。

```csharp
public interface IGameConfigService
{
    /// <summary>
    /// 获取所有游戏配置策略
    /// </summary>
    IReadOnlyList<GameConfigStrategy> GetStrategies();

    /// <summary>
    /// 获取指定游戏的配置策略
    /// </summary>
    GameConfigStrategy? GetStrategy(string gameId);

    /// <summary>
    /// 获取指定游戏的配置
    /// </summary>
    Task<GameConfig?> GetConfigAsync(string gameId);

    /// <summary>
    /// 保存游戏配置
    /// </summary>
    Task SaveConfigAsync(GameConfig config);

    /// <summary>
    /// 重置游戏配置到默认值
    /// </summary>
    Task ResetToDefaultAsync(string gameId);

    /// <summary>
    /// 应用配置到游戏（写入配置文件）
    /// </summary>
    Task ApplyConfigAsync(string gameId);
}
```

**使用场景**:
- `GetStrategies()`: 填充游戏列表，展示每款游戏支持的配置项
- `GetConfigAsync()`: 加载用户保存的游戏配置
- `SaveConfigAsync()`: 保存用户修改的游戏配置
- `ApplyConfigAsync()`: 将配置写入游戏配置文件（待实现）

## 服务实现

### ConfigurationService

配置管理服务实现，内部状态：

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigRepository _configRepository;  // 持久化仓库
    private CrosshairConfig _currentConfig;                // 当前配置单例

    public ConfigurationService(IConfigRepository configRepository)
    {
        _configRepository = configRepository;
        _currentConfig = CreateDefaultConfig();  // 构造时初始化默认配置
    }
}
```

**核心逻辑**:
- `_currentConfig` 是单例实例，`GetCurrentConfig()` 返回其引用
- `LoadConfigAsync()` 会替换 `_currentConfig` 实例
- `CloneConfig()` 递归克隆所有嵌套对象（Effects、Display）

**深拷贝实现**:

```csharp
public CrosshairConfig CloneConfig(CrosshairConfig source)
{
    return new CrosshairConfig
    {
        Id = Guid.NewGuid().ToString(),  // 生成新 ID
        Name = source.Name,
        Style = source.Style,
        Size = source.Size,
        Gap = source.Gap,
        Thickness = source.Thickness,
        Color = source.Color,
        Opacity = source.Opacity,
        Brightness = source.Brightness,
        CenterSize = source.CenterSize,
        Rotation = source.Rotation,
        CustomImagePath = source.CustomImagePath,
        Effects = new EffectsConfig  // 深拷贝 Effects
        {
            Outline = new OutlineConfig { ... },
            Shadow = new ShadowConfig { ... },
            Glow = new GlowConfig { ... }
        },
        Display = new DisplayConfig  // 深拷贝 Display
        {
            Monitor = source.Display.Monitor,
            ...
        }
    };
}
```

**CopyConfig 实现**:

```csharp
public void CopyConfig(CrosshairConfig source, CrosshairConfig target)
{
    // 复制所有属性值到目标实例，保持目标 Id 不变
    target.Name = source.Name;
    target.Style = source.Style;
    // ... 其他属性

    // 递归复制嵌套对象
    target.Effects.Outline.Enabled = source.Effects.Outline.Enabled;
    // ... 其他嵌套属性
}
```

### PresetService

预设管理服务实现，内部状态：

```csharp
public class PresetService : IPresetService
{
    private readonly IPresetRepository _presetRepository;     // 预设持久化
    private readonly IAppStateRepository _stateRepository;    // 应用状态持久化
    private readonly JsonSerializerOptions _jsonOptions;      // JSON 配置（导出用）
}
```

**核心逻辑**:
- `LoadAllPresetsAsync()` 自动在列表开头插入默认预设
- 默认预设（`Id = "default"`, `IsDefault = true`）不可保存/删除
- 导出预设直接写文件，不通过 Repository

**默认预设生成**:

```csharp
public async Task<IReadOnlyList<Preset>> LoadAllPresetsAsync()
{
    var presets = await _presetRepository.LoadPresetsAsync();
    var list = presets.ToList();

    // 在最前面插入默认预设
    var defaultPreset = new Preset
    {
        Id = "default",
        Name = "默认配置",
        Config = new CrosshairConfig(),
        IsDefault = true
    };
    list.Insert(0, defaultPreset);

    return list;
}
```

**导出实现**:

```csharp
public async Task ExportPresetAsync(Preset preset, string filePath)
{
    // 直接写文件，不通过 Repository（保持原 ID）
    var json = JsonSerializer.Serialize(preset, _jsonOptions);
    await File.WriteAllTextAsync(filePath, json);
}
```

### GameConfigService

游戏配置服务实现，内部状态：

```csharp
public class GameConfigService : IGameConfigService
{
    private readonly Dictionary<string, GameConfigStrategy> _strategies;  // 游戏策略字典
    private readonly string _configDir;                                   // 配置目录
    private readonly Dictionary<string, GameConfig> _configCache = new(); // 配置缓存
}
```

**核心逻辑**:
- 构造时初始化 8 款内置游戏的配置策略
- 配置文件存储在 `%APPDATA%/CrosshairPro/gameconfigs/{gameId}.json`
- 使用内存缓存避免重复文件读取
- `ApplyConfigAsync()` 待实现，需要根据每款游戏的配置文件格式写入

**游戏策略初始化**:

```csharp
private Dictionary<string, GameConfigStrategy> InitializeStrategies()
{
    return new Dictionary<string, GameConfigStrategy>
    {
        ["builtin-cs2"] = CreateCS2Strategy(),
        ["builtin-valorant"] = CreateValorantStrategy(),
        ["builtin-apex"] = CreateApexStrategy(),
        ["builtin-overwatch2"] = CreateOverwatch2Strategy(),
        ["builtin-pubg"] = CreatePUBGStrategy(),
        ["builtin-fortnite"] = CreateFortniteStrategy(),
        ["builtin-r6"] = CreateR6Strategy(),
        ["builtin-csgo"] = CreateCSGOStrategy()
    };
}
```

**配置策略示例** (CS2):

```csharp
private GameConfigStrategy CreateCS2Strategy()
{
    return new GameConfigStrategy
    {
        GameId = "builtin-cs2",
        SupportsLaunchOptions = true,
        LaunchOptionsDescription = "CS2 启动项参数，如 -high -threads 12 -novid",
        Sections = new List<ConfigSectionDefinition>
        {
            new()
            {
                Name = "video",
                DisplayName = "视频设置",
                Items = new List<ConfigItemDefinition>
                {
                    new()
                    {
                        Key = "fullscreen",
                        DisplayName = "全屏模式",
                        Type = ConfigItemType.Bool,
                        DefaultValue = true
                    },
                    new()
                    {
                        Key = "resolution",
                        DisplayName = "分辨率",
                        Type = ConfigItemType.Enum,
                        DefaultValue = "1920x1080",
                        Options = new List<string> { "1920x1080", "1680x1050", "1600x900", ... }
                    }
                }
            }
        }
    };
}
```

**配置缓存机制**:

```csharp
public async Task<GameConfig?> GetConfigAsync(string gameId)
{
    // 优先返回缓存
    if (_configCache.TryGetValue(gameId, out var cached))
        return cached;

    var filePath = GetConfigFilePath(gameId);
    if (!File.Exists(filePath))
    {
        // 文件不存在时返回默认配置
        var defaultConfig = CreateDefaultConfig(gameId);
        _configCache[gameId] = defaultConfig;
        return defaultConfig;
    }

    // 从文件加载并缓存
    var json = await File.ReadAllTextAsync(filePath);
    var config = JsonSerializer.Deserialize<GameConfig>(json);
    _configCache[gameId] = config!;
    return config;
}
```

## 依赖注入配置

### ServiceCollectionExtensions

服务注册扩展方法：

```csharp
public static IServiceCollection AddCrosshairProServices(this IServiceCollection services)
{
    // Repositories (Singleton - 单例共享状态)
    services.AddSingleton<JsonConfigRepository>();
    services.AddSingleton<IConfigRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());
    services.AddSingleton<IAppStateRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());

    services.AddSingleton<IPresetRepository, JsonPresetRepository>();

    // Application Services
    services.AddSingleton<IPresetService, PresetService>();
    services.AddSingleton<IConfigurationService, ConfigurationService>();

    // Infrastructure Services
    services.AddSingleton<IHotkeyManager, HotkeyManager>();
    services.AddSingleton<ICrosshairRenderer, CrosshairRenderer>();

    return services;
}
```

### 服务注册表

| 服务 | 接口 | 实现类 | 生命周期 | 说明 |
|------|------|--------|----------|------|
| 配置仓库 | `IConfigRepository` | `JsonConfigRepository` | Singleton | 共享实例 |
| 状态仓库 | `IAppStateRepository` | `JsonConfigRepository` | Singleton | 同上实例 |
| 预设仓库 | `IPresetRepository` | `JsonPresetRepository` | Singleton | - |
| 配置服务 | `IConfigurationService` | `ConfigurationService` | Singleton | 持有当前配置 |
| 预设服务 | `IPresetService` | `PresetService` | Singleton | 持有 JSON 配置 |
| 游戏配置服务 | `IGameConfigService` | `GameConfigService` | Singleton | 持有策略和缓存 |
| 热键管理 | `IHotkeyManager` | `HotkeyManager` | Singleton | - |
| 准心渲染 | `ICrosshairRenderer` | `CrosshairRenderer` | Singleton | 缓存几何图形 |

**关键点**:
- `JsonConfigRepository` 同时注册为两个接口，确保文件锁共享
- 所有服务都是 Singleton，确保状态一致性

## 数据流

### 配置加载流程

```
App.OnStartup()
    └── MainViewModel 构造函数
        └── _configService.GetCurrentConfig()  // 返回默认配置
            └── _presetService.GetCurrentPresetIdAsync()
                └── 如果有保存的预设ID
                    └── _presetService.LoadAllPresetsAsync()
                        └── 找到对应预设
                            └── _configService.CopyConfig(preset.Config, _config)
```

### 预设切换流程

```
用户选择预设
    └── MainViewModel.ApplyPresetCommand
        └── _configService.CopyConfig(preset.Config, Config)  // 复制值，保持实例
            └── _presetService.SetCurrentPresetAsync(presetId)  // 持久化状态
                └── _stateRepository.SaveStateAsync(state)
```

### 配置保存流程

```
用户修改配置
    └── Config.PropertyChanged 事件
        └── MainViewModel 标记 IsModified = true
            └── 用户点击保存
                └── _configService.SaveConfigAsync()
                    └── _configRepository.SaveConfigAsync(_currentConfig)
```

### 游戏配置加载流程

```
用户选择游戏
    └── _gameConfigService.GetStrategy(gameId)
        └── 返回游戏配置策略（配置项定义）
    └── _gameConfigService.GetConfigAsync(gameId)
        └── 检查缓存
            └── 缓存命中 → 返回缓存
            └── 缓存未命中 → 从文件加载
                └── 文件不存在 → 返回默认配置
```

### 游戏配置保存流程

```
用户修改游戏配置
    └── _gameConfigService.SaveConfigAsync(config)
        └── 更新内存缓存
        └── 写入 JSON 文件
            └── %APPDATA%/CrosshairPro/gameconfigs/{gameId}.json
```

## JsonSerializerOptions

预设导出使用的 JSON 配置：

```csharp
_jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,                              // 格式化输出
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // 忽略 null
};
```

导出的 JSON 示例：

```json
{
  "id": "preset-guid",
  "name": "我的预设",
  "config": {
    "style": 0,
    "size": 20,
    ...
  },
  "isDefault": false
}
```