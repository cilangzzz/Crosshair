# CrosshairPro.Application

应用服务层模块，提供业务逻辑抽象、游戏配置管理和依赖注入配置。

## 概述

Application 模块是 CrosshairPro 架构中的中间层，承担以下职责：

- **业务逻辑抽象**: 封装准心配置、预设、内置游戏配置与 Apex 游戏配置的业务逻辑
- **接口隔离**: App 层仅依赖 Application 接口，不直接依赖 Services / Infrastructure 实现
- **依赖注入配置**: 统一服务注册入口 `AddCrosshairProServices()`
- **状态管理**: 维护当前准心配置实例、当前预设ID、游戏配置内存缓存

## 目录结构

```
CrosshairPro.Application/
├── DI/
│   └── ServiceCollectionExtensions.cs   # 依赖注入注册扩展（AddCrosshairProServices）
├── Interfaces/
│   ├── IConfigurationService.cs         # 准心配置服务接口
│   ├── IPresetService.cs                # 预设服务接口
│   ├── IGameConfigService.cs            # 游戏配置服务接口（内置 8 款游戏策略）
│   └── IApexConfigService.cs            # Apex Legends 配置服务接口（含 BackupFileInfo 模型）
├── Services/
│   ├── ConfigurationService.cs          # 准心配置服务实现
│   ├── PresetService.cs                 # 预设服务实现
│   ├── GameConfigService.cs             # 游戏配置服务实现
│   └── ApexConfigService.cs             # Apex Legends 配置服务实现
└── CrosshairPro.Application.csproj      # net8.0-windows，UseWPF=true；引用 Core/Services/Infrastructure + Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2
```

**文件统计**: 9 个源文件，约 2070 行代码

## 服务组件一览

所有服务均注册为 Singleton，完整注册代码与服务注册表见 [data-model.md](data-model.md)。

| 服务接口 | 实现类 | 职责 | API 详情 |
|----------|--------|------|----------|
| `IConfigurationService` | `ConfigurationService` | 当前准心配置的加载/保存/重置/深拷贝 | [api-configuration.md](api-configuration.md) |
| `IPresetService` | `PresetService` | 预设增删改查、导入导出、当前预设持久化 | [api-preset.md](api-preset.md) |
| `IGameConfigService` | `GameConfigService` | 8 款内置游戏的配置策略定义与配置存取 | [api-gameconfig.md](api-gameconfig.md) |
| `IApexConfigService` | `ApexConfigService` | Apex 的 videoconfig.txt / settings.cfg 读写、备份恢复、启动选项 | [api-gameconfig.md](api-gameconfig.md) |

四个服务接口均不定义事件；配置变更通知依赖数据模型自身的 `PropertyChanged`（见 [../Core/README.md](../Core/README.md)）。

## 内置游戏配置策略

`GameConfigService` 在构造时注册 8 款游戏的配置策略，配置持久化到 `%APPDATA%/CrosshairPro/gameconfigs/{gameId}.json`（详见 [api-gameconfig.md](api-gameconfig.md)）：

| 游戏 | GameId | 支持启动项 |
|------|--------|------------|
| CS2 | `builtin-cs2` | 是 |
| Valorant | `builtin-valorant` | 否 |
| Apex Legends | `builtin-apex` | 是 |
| Overwatch 2 | `builtin-overwatch2` | 否 |
| PUBG | `builtin-pubg` | 是 |
| Fortnite | `builtin-fortnite` | 否 |
| Rainbow Six | `builtin-r6` | 是 |
| CS:GO | `builtin-csgo` | 是（复用 CS2 策略定义） |

## 模块依赖

### 模块依赖

```
Application → Core (Models, Interfaces)
Application → Services (JsonConfigRepository / JsonPresetRepository 等持久化实现)
Application → Infrastructure (HotkeyManager)
```

### 服务依赖

```
ConfigurationService → IConfigRepository（Services 层）
PresetService        → IPresetRepository + IAppStateRepository（Services 层）
GameConfigService    → 无接口依赖，直接操作 %APPDATA%/CrosshairPro/gameconfigs/
ApexConfigService    → 无接口依赖，直接操作 Apex 存档目录与 %APPDATA%/CrosshairPro/
```

## 使用示例

### 服务注册（App.OnStartup）

```csharp
var services = new ServiceCollection();
services.AddCrosshairProServices()
    .AddSingleton<MainViewModel>()
    .AddSingleton<OverlayWindow>()
    .AddTransient<MainWindow>();
```

### ViewModel 中消费服务

```csharp
public CrosshairViewModel(IPresetService presetService, IConfigurationService configService)
{
    _presetService = presetService;
    _configService = configService;
    _config = configService.GetCurrentConfig();   // 保持引用，UI 直接绑定
    InitializeAsync().ConfigureAwait(false);      // 异步恢复上次使用的预设
}
```

## 详细文档

- [配置服务 API](api-configuration.md) — IConfigurationService 方法签名、行为与调用方
- [预设服务 API](api-preset.md) — IPresetService 方法签名、导入导出规则与调用方
- [游戏配置 API](api-gameconfig.md) — IGameConfigService / IApexConfigService 方法签名与调用方
- [数据模型](data-model.md) — 服务内部状态、DI 注册表、持久化文件布局、数据流
- [坑点](pitfalls.md) — 已知问题和注意事项
- [变更日志](CHANGELOG.md) — 模块变更历史
