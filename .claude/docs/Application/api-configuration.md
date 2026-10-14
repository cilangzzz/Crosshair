# 配置服务 API — IConfigurationService

准心配置的业务入口。App 层通过本接口读写当前准心配置，不直接接触持久化仓库。

## 基本信息

| 项 | 值 |
|----|----|
| 接口定义 | `src/CrosshairPro.Application/Interfaces/IConfigurationService.cs` |
| 实现类 | `src/CrosshairPro.Application/Services/ConfigurationService.cs` |
| DI 注册 | `services.AddSingleton<IConfigurationService, ConfigurationService>()` |
| 依赖 | `IConfigRepository`（Services 层 `JsonConfigRepository` 提供，持久化到 `%APPDATA%/CrosshairPro/config.json`） |
| 事件 | 无。变更通知依赖 `CrosshairConfig` 自身的 `PropertyChanged`（见 [../Core/README.md](../Core/README.md)） |

## 方法列表

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `GetCurrentConfig()` | `CrosshairConfig` | 获取当前配置实例（内部单例引用，非副本） |
| `LoadConfigAsync()` | `Task` | 从持久化加载配置，替换内部实例 |
| `SaveConfigAsync()` | `Task` | 保存当前配置到持久化 |
| `ResetToDefaultAsync()` | `Task` | 重置为默认配置并立即保存 |
| `CloneConfig(source)` | `CrosshairConfig` | 深拷贝配置（生成新 GUID） |
| `CopyConfig(source, target)` | `void` | 复制配置值到目标实例（保持目标 Id） |
| `CreateDefaultConfig()` | `CrosshairConfig` | 创建默认配置实例 |

## 方法详情

### GetCurrentConfig()

- **签名**: `CrosshairConfig GetCurrentConfig()`
- **参数**: 无
- **返回值**: 内部字段 `_currentConfig` 的引用，不是副本
- **行为**: 构造时初始化为默认配置；`LoadConfigAsync()` / `ResetToDefaultAsync()` 会整体替换该实例，此前持有的引用会与当前配置脱钩。UI 绑定场景应保留一次引用并依赖 `PropertyChanged` 刷新（见 [pitfalls.md](pitfalls.md) 坑点 1）。

### LoadConfigAsync()

- **签名**: `Task LoadConfigAsync()`
- **参数**: 无
- **返回值**: `Task`
- **行为**: `_currentConfig = await _configRepository.LoadConfigAsync()`，整体替换内部实例。

### SaveConfigAsync()

- **签名**: `Task SaveConfigAsync()`
- **参数**: 无
- **返回值**: `Task`
- **行为**: 将 `_currentConfig` 原样写入持久化。服务不监听配置变更，修改后必须显式调用（见 [pitfalls.md](pitfalls.md) 坑点 9）。

### ResetToDefaultAsync()

- **签名**: `Task ResetToDefaultAsync()`
- **参数**: 无
- **返回值**: `Task`
- **行为**: `_currentConfig = CreateDefaultConfig()` 后立即 `SaveConfigAsync()`，默认值会覆盖 `config.json`。

### CloneConfig(source)

- **签名**: `CrosshairConfig CloneConfig(CrosshairConfig source)`
- **参数**: `source` — 源配置
- **返回值**: 全新 `CrosshairConfig` 实例
- **行为**: 逐字段复制；`Id` 重新生成 GUID；`Effects`（Outline / Shadow / Glow）与 `Display` 递归新建，不共享任何嵌套引用（见 [pitfalls.md](pitfalls.md) 坑点 10）。与 `CopyConfig` 的区别见坑点 2。

### CopyConfig(source, target)

- **签名**: `void CopyConfig(CrosshairConfig source, CrosshairConfig target)`
- **参数**: `source` — 源配置；`target` — 目标配置
- **返回值**: 无
- **行为**: 逐属性赋值到 `target`，保持 `target.Id` 不变；`Effects` / `Display` 嵌套属性逐字段复制。用于切换预设时保持 UI 绑定实例不变。

### CreateDefaultConfig()

- **签名**: `CrosshairConfig CreateDefaultConfig()`
- **参数**: 无
- **返回值**: 新的默认配置实例
- **行为**: 返回以下默认值：

| 字段 | 默认值 |
|------|--------|
| Name | `"默认配置"` |
| Style | `CrosshairStyle.Cross` |
| Size / Gap / Thickness | 20 / 4 / 2 |
| Color | `#00FF00` |
| Opacity | 100 |
| CenterSize | 4 |
| 其余字段 | `CrosshairConfig` 模型默认值 |

## 主要调用方

| 调用方 | 使用的成员 |
|--------|-----------|
| `CrosshairViewModel`（`src/CrosshairPro.App/ViewModels/CrosshairViewModel.cs`） | `GetCurrentConfig()`（构造时绑定）、`CopyConfig()`（恢复上次预设）、`CloneConfig()`（另存为新预设）、`CreateDefaultConfig()`（重置） |
| `MainViewModel`（`src/CrosshairPro.App/ViewModels/MainViewModel.cs`） | 仅注入并转发到 `CrosshairViewModel`，自身不直接调用业务方法 |
