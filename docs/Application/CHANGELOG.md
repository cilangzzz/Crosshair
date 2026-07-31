# Changelog - Application

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-07-31] docs: update Application module documentation

**类型**: docs
**风险**: LOW

### 变更内容
- 更新 README.md，补充完整的服务列表和使用示例
- 扩展 data-model.md，添加数据流图和 JSON 配置说明
- 完善 pitfalls.md，增加 11 个坑点详解
- 更新 CHANGELOG.md 格式

### 影响范围
- 文档更新，不影响代码

---

## [2026-07-03] refactor: restructure project architecture

**类型**: feat
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| CrosshairPro.Application.csproj | +23 | 新增项目定义 |
| ServiceCollectionExtensions.cs | +38 | 依赖注入注册扩展 |
| IConfigurationService.cs | +44 | 配置服务接口 |
| IPresetService.cs | +44 | 预设服务接口 |
| ConfigurationService.cs | +162 | 配置服务实现 |
| PresetService.cs | +113 | 预设服务实现 |

### 新增功能
1. **配置管理服务**
   - `GetCurrentConfig()`: 获取当前配置单例
   - `LoadConfigAsync()`: 从持久化加载配置
   - `SaveConfigAsync()`: 保存配置到持久化
   - `ResetToDefaultAsync()`: 重置为默认配置
   - `CloneConfig()`: 创建配置深拷贝
   - `CopyConfig()`: 复制配置值到目标实例

2. **预设管理服务**
   - `LoadAllPresetsAsync()`: 加载所有预设（含默认预设）
   - `SavePresetAsync()`: 保存预设
   - `DeletePresetAsync()`: 删除预设
   - `ImportPresetAsync()`: 从文件导入预设
   - `ExportPresetAsync()`: 导出预设到文件
   - `SetCurrentPresetAsync()`: 设置当前预设并持久化

3. **依赖注入统一注册**
   - `AddCrosshairProServices()` 扩展方法
   - 注册所有仓库和服务
   - 确保 `JsonConfigRepository` 双接口共享实例

### 架构变更
- **新增中间层**: Application 层作为业务逻辑抽象层
- **解耦**: App 层不再直接依赖 Services，通过 Application 接口调用
- **统一入口**: 所有服务通过 `AddCrosshairProServices()` 注册

### 影响范围
- **App 层**: ViewModel 构造函数依赖注入变更
- **Services 层**: 仓库实现被 Application 层接口调用
- **Core 层**: 新增 `IAppStateRepository` 接口

### 迁移指南

**旧代码** (直接依赖 Services):
```csharp
public MainViewModel(
    JsonConfigRepository configRepo,
    JsonPresetRepository presetRepo)
{
    var config = await configRepo.LoadConfigAsync();
}
```

**新代码** (通过 Application 接口):
```csharp
public MainViewModel(
    IConfigurationService configService,
    IPresetService presetService)
{
    var config = configService.GetCurrentConfig();
}
```

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: 整个 Application 目录，App 层 ViewModel 构造函数
- 副作用: 需要恢复 App 层对 Services 层的直接依赖

---

## 待办事项

- [ ] 添加配置变更监听和自动保存
- [ ] 支持配置版本迁移
- [ ] 添加预设验证逻辑
- [ ] 支持预设分类和标签