# CrosshairPro.Application

应用服务层模块，提供业务逻辑抽象和依赖注入配置。

## 概述

Application 模块是 CrosshairPro 架构中的中间层，承担以下职责：

- **业务逻辑抽象**: 封装配置管理和预设管理的业务逻辑
- **接口隔离**: App 层仅依赖 Application 接口，不直接依赖具体实现
- **依赖注入配置**: 统一的服务注册入口，简化启动配置
- **状态管理**: 维护当前配置实例和预设状态

## 目录结构

```
CrosshairPro.Application/
├── DI/
│   └── ServiceCollectionExtensions.cs  # 依赖注入注册扩展
├── Interfaces/
│   ├── IConfigurationService.cs        # 配置服务接口
│   ├── IPresetService.cs               # 预设服务接口
│   └── IGameConfigService.cs           # 游戏配置服务接口
└── Services/
    ├── ConfigurationService.cs         # 配置服务实现
    ├── PresetService.cs                # 预设服务实现
    └── GameConfigService.cs            # 游戏配置服务实现
```

**文件统计**: 7 个源文件，约 700 行代码

## 核心组件

### IConfigurationService

配置管理服务接口，提供配置操作的抽象层。

| 方法 | 说明 | 返回类型 |
|------|------|----------|
| `GetCurrentConfig()` | 获取当前配置实例 | `CrosshairConfig` |
| `LoadConfigAsync()` | 从持久化加载配置 | `Task` |
| `SaveConfigAsync()` | 保存配置到持久化 | `Task` |
| `ResetToDefaultAsync()` | 重置为默认配置 | `Task` |
| `CloneConfig(source)` | 创建配置的深拷贝 | `CrosshairConfig` |
| `CopyConfig(source, target)` | 复制配置值到目标实例 | `void` |
| `CreateDefaultConfig()` | 创建默认配置实例 | `CrosshairConfig` |

### IPresetService

预设管理服务接口，提供预设操作的抽象层。

| 方法 | 说明 | 返回类型 |
|------|------|----------|
| `LoadAllPresetsAsync()` | 加载所有预设（包含默认预设） | `Task<IReadOnlyList<Preset>>` |
| `SavePresetAsync(preset)` | 保存预设 | `Task` |
| `DeletePresetAsync(presetId)` | 删除预设 | `Task` |
| `ImportPresetAsync(filePath)` | 从文件导入预设 | `Task<Preset>` |
| `ExportPresetAsync(preset, filePath)` | 导出预设到文件 | `Task` |
| `SetCurrentPresetAsync(presetId)` | 设置当前使用的预设 | `Task` |
| `GetCurrentPresetIdAsync()` | 获取当前使用的预设ID | `Task<string?>` |

### IGameConfigService

游戏配置服务接口，管理各游戏的配置数据。

| 方法 | 说明 | 返回类型 |
|------|------|----------|
| `GetStrategies()` | 获取所有游戏配置策略 | `IReadOnlyList<GameConfigStrategy>` |
| `GetStrategy(gameId)` | 获取指定游戏的配置策略 | `GameConfigStrategy?` |
| `GetConfigAsync(gameId)` | 获取游戏配置 | `Task<GameConfig?>` |
| `SaveConfigAsync(config)` | 保存游戏配置 | `Task` |
| `ResetToDefaultAsync(gameId)` | 重置为默认配置 | `Task` |
| `ApplyConfigAsync(gameId)` | 应用配置到游戏 | `Task` |

### ConfigurationService

配置管理服务实现，核心特性：

- 内部持有 `_currentConfig` 单例实例，所有操作基于此实例
- 通过 `IConfigRepository` 实现持久化
- 提供深拷贝方法确保配置独立性

### PresetService

预设管理服务实现，核心特性：

- 通过 `IPresetRepository` 管理预设文件
- 通过 `IAppStateRepository` 持久化当前预设ID
- 自动在预设列表开头插入默认预设（`IsDefault = true`）

### GameConfigService

游戏配置服务实现，核心特性：

- 内置 8 款游戏的配置策略（CS2、Valorant、Apex、Overwatch2、PUBG、Fortnite、R6、CSGO）
- 配置缓存机制，避免重复文件读取
- JSON 文件持久化到 `%APPDATA%/CrosshairPro/gameconfigs/`
- 每款游戏有独立的配置策略定义（视频设置、游戏设置等）

**支持的游戏列表**:

| 游戏 | GameId | 支持启动项 |
|------|--------|------------|
| CS2 | `builtin-cs2` | 是 |
| Valorant | `builtin-valorant` | 否 |
| Apex Legends | `builtin-apex` | 是 |
| Overwatch 2 | `builtin-overwatch2` | 否 |
| PUBG | `builtin-pubg` | 是 |
| Fortnite | `builtin-fortnite` | 否 |
| Rainbow Six | `builtin-r6` | 是 |
| CS:GO | `builtin-csgo` | 是 |

### ServiceCollectionExtensions

依赖注入注册扩展，提供统一的服务注册入口：

```csharp
services.AddCrosshairProServices()
    .AddSingleton<MainViewModel>()
    .AddSingleton<OverlayWindow>()
    .AddTransient<MainWindow>();
```

## 依赖关系

### 模块依赖

```
Application → Core (Models, Interfaces)
Application → Services (配置持久化实现)
Application → Infrastructure (热键管理)
```

### 服务依赖

```
ConfigurationService
    └── IConfigRepository (Services层提供)

PresetService
    ├── IPresetRepository (Services层提供)
    └── IAppStateRepository (Services层提供)

GameConfigService
    └── (无外部依赖，直接操作文件系统)
```

## 服务生命周期

| 服务类型 | 接口 | 实现类 | 生命周期 |
|----------|------|--------|----------|
| 配置仓库 | `IConfigRepository` | `JsonConfigRepository` | Singleton |
| 状态仓库 | `IAppStateRepository` | `JsonConfigRepository` | Singleton |
| 预设仓库 | `IPresetRepository` | `JsonPresetRepository` | Singleton |
| 配置服务 | `IConfigurationService` | `ConfigurationService` | Singleton |
| 预设服务 | `IPresetService` | `PresetService` | Singleton |
| 游戏配置服务 | `IGameConfigService` | `GameConfigService` | Singleton |
| 热键管理 | `IHotkeyManager` | `HotkeyManager` | Singleton |
| 准心渲染 | `ICrosshairRenderer` | `CrosshairRenderer` | Singleton |

**重要**: `JsonConfigRepository` 同时实现 `IConfigRepository` 和 `IAppStateRepository`，共享文件锁避免并发冲突。

## 使用示例

### 在 App 层注入服务

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    var services = new ServiceCollection();
    services.AddCrosshairProServices()
        .AddSingleton<MainViewModel>()
        .AddSingleton<OverlayWindow>()
        .AddTransient<MainWindow>();

    _provider = services.BuildServiceProvider();
}
```

### 在 ViewModel 中使用服务

```csharp
public class MainViewModel
{
    private readonly IConfigurationService _configService;
    private readonly IPresetService _presetService;

    public MainViewModel(
        IConfigurationService configService,
        IPresetService presetService)
    {
        _configService = configService;
        _presetService = presetService;
        _config = configService.GetCurrentConfig();
    }
}
```

## 详细文档

- [数据模型](data-model.md) - 服务接口和内部状态
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史
