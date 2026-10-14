# CrosshairPro.Application - 数据模型

本模块是对内服务层，公开服务接口即 API 面。各服务接口的方法签名、参数与返回值详见：

- 配置服务: [api-configuration.md](api-configuration.md)
- 预设服务: [api-preset.md](api-preset.md)
- 游戏配置服务: [api-gameconfig.md](api-gameconfig.md)

本文记录服务内部状态、依赖注入、持久化文件布局与数据流。`CrosshairConfig` 等数据模型的字段定义见 [../Core/README.md](../Core/README.md) 与 [../Core/data-model.md](../Core/data-model.md)。

## 服务内部状态

### ConfigurationService

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigRepository _configRepository;  // 持久化仓库
    private CrosshairConfig _currentConfig;                // 当前配置单例（构造时 = 默认配置）
}
```

- `_currentConfig` 是单例实例，`GetCurrentConfig()` 返回其引用
- `LoadConfigAsync()` / `ResetToDefaultAsync()` 整体替换 `_currentConfig` 实例
- `CloneConfig()` 递归克隆所有嵌套对象（Effects / Display）并生成新 GUID

### PresetService

```csharp
public class PresetService : IPresetService
{
    private readonly IPresetRepository _presetRepository;   // 预设持久化
    private readonly IAppStateRepository _stateRepository;  // 应用状态持久化
    private readonly JsonSerializerOptions _jsonOptions;    // 导出序列化配置（CamelCase / 缩进 / 忽略 null）
}
```

- `LoadAllPresetsAsync()` 自动在列表开头插入动态生成的默认预设（`Id = "default"`，不落盘）；用户预设按 `CreatedAt` 降序（最新在前）
- 导出预设直接写文件，不经过 Repository（保持原 ID）
- 当前预设ID经 `IAppStateRepository` 持久化

### GameConfigService

```csharp
public class GameConfigService : IGameConfigService
{
    private readonly Dictionary<string, GameConfigStrategy> _strategies;   // 2 款内置游戏策略（CS2/Apex）
    private readonly string _configDir;                                    // %APPDATA%/CrosshairPro/gameconfigs/
    private readonly Dictionary<string, GameConfig> _configCache = new();  // 配置内存缓存
}
```

- 构造时初始化策略并立即创建 `_configDir` 目录
- 缓存优先：命中返回缓存实例，未命中读文件；文件不存在 → 回退策略默认值并写入缓存；解析异常（或反序列化为 null）→ 返回策略默认值但不写入缓存
- `ApplyConfigAsync()` 为空实现（见 [pitfalls.md](pitfalls.md) 坑点 12）

### ApexConfigService

```csharp
public class ApexConfigService : IApexConfigService
{
    private readonly string _apexConfigDir;  // %USERPROFILE%/Saved Games/Respawn/Apex/local/
    private readonly string _backupDir;      // %APPDATA%/CrosshairPro/apex-backups/
    private readonly string _appConfigDir;   // %APPDATA%/CrosshairPro/
    private readonly SemaphoreSlim _fileLock = new(1, 1);  // 文件操作串行锁
}
```

- 无接口依赖，直接操作 Apex 存档目录与应用配置目录（构造时创建备份目录与应用配置目录）
- videoconfig.txt / settings.cfg 的解析与序列化规则见 [api-gameconfig.md](api-gameconfig.md)

## 依赖注入配置

### ServiceCollectionExtensions

服务注册扩展方法（`src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs`）：

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
    services.AddSingleton<IGameConfigService, GameConfigService>();

    // Apex Legends 配置服务
    services.AddSingleton<IApexConfigService, ApexConfigService>();

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
| 状态仓库 | `IAppStateRepository` | `JsonConfigRepository` | Singleton | 同一实例 |
| 预设仓库 | `IPresetRepository` | `JsonPresetRepository` | Singleton | - |
| 配置服务 | `IConfigurationService` | `ConfigurationService` | Singleton | 持有当前配置 |
| 预设服务 | `IPresetService` | `PresetService` | Singleton | 持有导出 JSON 配置 |
| 游戏配置服务 | `IGameConfigService` | `GameConfigService` | Singleton | 持有策略和缓存 |
| Apex 配置服务 | `IApexConfigService` | `ApexConfigService` | Singleton | 持有文件锁 |
| 热键管理 | `IHotkeyManager` | `HotkeyManager` | Singleton | - |
| 准心渲染 | `ICrosshairRenderer` | `CrosshairRenderer` | Singleton | 缓存 Pen/Brush；`_geometryCache` 已声明但从未写入（无实际几何缓存命中，见 ../Services/pitfalls.md 坑点 10） |

**关键点**:
- `JsonConfigRepository` 同时注册为两个接口，确保文件锁共享（错误注册方式见 [pitfalls.md](pitfalls.md) 坑点 7）
- 所有服务都是 Singleton，确保状态一致性

## 持久化文件布局

| 文件 / 目录 | 写入者 | 内容 |
|-------------|--------|------|
| `%APPDATA%/CrosshairPro/config.json` | `ConfigurationService`（经 `IConfigRepository`） | 当前准心配置 |
| `%APPDATA%/CrosshairPro/appstate.json` | `PresetService`（经 `IAppStateRepository`） | `CurrentPresetId` 等应用状态 |
| `%APPDATA%/CrosshairPro/presets/{id}.json` | `PresetService`（经 `IPresetRepository`） | 用户预设 |
| `%APPDATA%/CrosshairPro/gameconfigs/{gameId}.json` | `GameConfigService` | 各游戏配置（如 `builtin-cs2.json`） |
| `%APPDATA%/CrosshairPro/launch_options.txt` | `ApexConfigService` | Apex 启动选项 |
| `%APPDATA%/CrosshairPro/apex-backups/{videoconfig,settings}/` | `ApexConfigService` | Apex 配置备份 |

## 数据流

### 配置加载与预设恢复流程

```
App.OnStartup()（DI 构建容器）
    └── CrosshairViewModel 构造函数
        └── _configService.GetCurrentConfig()        // 默认配置，立即绑定 UI
        └── InitializeAsync()（fire-and-forget）
            ├── _presetService.GetCurrentPresetIdAsync()
            ├── _presetService.LoadAllPresetsAsync() // 列表[0] = 默认预设
            └── savedPresetId 命中
                └── _configService.CopyConfig(preset.Config, Config)
```

### 预设切换流程

```
用户选择预设
    └── CrosshairViewModel
        └── _configService.CopyConfig(preset.Config, Config)  // 复制值，保持绑定实例
            └── _presetService.SetCurrentPresetAsync(presetId) // 持久化到 appstate.json
```

### 配置保存流程

```
用户修改配置
    └── CrosshairConfig.PropertyChanged（模型自身通知）
        └── 用户点击保存
            └── _configService.SaveConfigAsync()
                └── _configRepository.SaveConfigAsync(_currentConfig)  // config.json
```

### 游戏配置加载流程

```
Cs2ConfigViewModel 初始化
    └── _gameConfigService.GetStrategy(gameId)     // 配置项元数据
    └── _gameConfigService.GetConfigAsync(gameId)
        └── 缓存命中 → 返回缓存
        └── 缓存未命中 → 读 {gameId}.json
            ├── 文件不存在 → 策略默认值（并写入缓存）
            └── 解析失败 → 策略默认值（不写入缓存）
```

### 游戏配置保存流程

```
用户修改游戏配置
    └── _gameConfigService.SaveConfigAsync(config)
        └── 更新 _configCache
        └── 写入 %APPDATA%/CrosshairPro/gameconfigs/{gameId}.json
```

### Apex 配置读写流程

```
ApexConfigViewModel
    ├── IsApexInstalled() → false 时禁用页面功能
    ├── LoadVideoConfigAsync() / LoadSettingsConfigAsync()   // 直接读 Apex 存档目录
    ├── SaveVideoConfigAsync() / SaveSettingsConfigAsync()   // 序列化写回存档目录
    └── Replace / Backup / Restore / GetBackups              // 经 %APPDATA%/CrosshairPro/apex-backups/
```
