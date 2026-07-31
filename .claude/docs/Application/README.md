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
│   └── IPresetService.cs               # 预设服务接口
└── Services/
    ├── ConfigurationService.cs         # 配置服务实现
    └── PresetService.cs                # 预设服务实现
```

**文件统计**: 5 个源文件，约 400 行代码

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
```

## 服务生命周期

| 服务类型 | 接口 | 实现类 | 生命周期 |
|----------|------|--------|----------|
| 配置仓库 | `IConfigRepository` | `JsonConfigRepository` | Singleton |
| 状态仓库 | `IAppStateRepository` | `JsonConfigRepository` | Singleton |
| 预设仓库 | `IPresetRepository` | `JsonPresetRepository` | Singleton |
| 配置服务 | `IConfigurationService` | `ConfigurationService` | Singleton |
| 预设服务 | `IPresetService` | `PresetService` | Singleton |
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
