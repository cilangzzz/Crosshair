# CrosshairPro.Core 模块

## 概述

核心层模块，定义项目的核心模型、接口和枚举。作为架构的基础，被所有其他模块依赖，本身不依赖任何业务模块。

## 模块结构

```
CrosshairPro.Core/
├── Enums/
│   ├── AppState.cs           # 应用状态枚举
│   └── CrosshairStyle.cs     # 准心样式枚举
├── Interfaces/
│   ├── IConfigRepository.cs  # 配置仓库接口
│   ├── ICrosshairRenderer.cs # 准心渲染器接口
│   ├── IGameDetector.cs      # 游戏检测器接口
│   └── IHotkeyManager.cs     # 热键管理器接口
├── Models/
│   ├── AppPersistedState.cs  # 应用持久化状态
│   ├── CrosshairConfig.cs    # 准心配置模型
│   ├── EffectsConfig.cs      # 效果配置模型
│   ├── GameInfo.cs           # 游戏信息模型
│   ├── GameProfile.cs        # 游戏配置文件
│   ├── HotkeyBinding.cs      # 热键绑定模型
│   └── Preset.cs             # 预设模型
└── Events/
    └── ConfigUpdatedEventArgs.cs # 配置更新事件参数
```

## 核心枚举

### AppState - 应用状态
```csharp
public enum AppState
{
    Idle = 0,              // 空闲状态
    GameMode = 1,          // 游戏模式（检测到游戏运行）
    CrosshairVisible = 2,  // 准心显示中
    CrosshairHidden = 3    // 准心隐藏
}
```

### CrosshairStyle - 准心样式
```csharp
public enum CrosshairStyle
{
    Cross = 0,        // 十字准心
    Dot = 1,          // 点状准心
    Circle = 2,       // 圆形准心
    TShape = 3,       // T形准心
    XShape = 4,       // X形准心
    CustomImage = 5   // 自定义图片
}
```

## 核心接口

### IConfigRepository - 配置仓库接口
负责配置的持久化操作。

**方法**:
- `LoadConfigAsync()`: 加载主配置
- `SaveConfigAsync(config)`: 保存主配置
- `ResetToDefaultAsync()`: 重置为默认配置
- `ExportConfigAsync(filePath, config)`: 导出配置
- `ImportConfigAsync(filePath)`: 导入配置

### IAppStateRepository - 应用状态仓库接口
负责应用状态的持久化。

**方法**:
- `LoadStateAsync()`: 加载应用持久化状态
- `SaveStateAsync(state)`: 保存应用持久化状态

### IPresetRepository - 预设仓库接口
负责预设的持久化管理。

**方法**:
- `LoadPresetsAsync()`: 加载所有预设
- `SavePresetAsync(preset)`: 保存预设
- `DeletePresetAsync(presetId)`: 删除预设
- `GetPresetAsync(presetId)`: 获取预设
- `ExportPresetAsync(presetId, filePath)`: 导出预设
- `ImportPresetAsync(filePath)`: 导入预设

### ICrosshairRenderer - 准心渲染器接口
负责准心的渲染逻辑。

**方法**:
- `Render(drawingContext, config, width, height)`: 渲染准心

**事件**:
- `RenderCompleted`: 渲染完成事件

### IHotkeyManager - 热键管理器接口
负责系统热键的注册和管理。

**方法**:
- `RegisterHotkey(binding)`: 注册热键
- `UnregisterHotkey(bindingId)`: 注销热键
- `UnregisterAll()`: 注销所有热键

**事件**:
- `HotkeyTriggered`: 热键触发事件

## 核心模型

### CrosshairConfig - 准心配置
主要属性：
- `Id`: 配置ID
- `Name`: 配置名称
- `Style`: 准心样式
- `Size`: 大小
- `Gap`: 间隙
- `Thickness`: 厚度
- `Color`: 颜色
- `Opacity`: 不透明度
- `Brightness`: 亮度
- `CenterSize`: 中心点大小
- `Rotation`: 旋转角度
- `CustomImagePath`: 自定义图片路径
- `Effects`: 效果配置
- `Display`: 显示配置

**方法**:
- `Clone()`: 创建深拷贝
- `CopyFrom(other)`: 从另一个配置复制值

### EffectsConfig - 效果配置
包含三种效果：
- `Outline`: 描边效果
- `Shadow`: 阴影效果
- `Glow`: 发光效果

### Preset - 预设模型
主要属性：
- `Id`: 预设ID
- `Name`: 预设名称
- `Config`: 准心配置
- `GameAssociation`: 游戏关联
- `HotkeyBinding`: 热键绑定
- `CreatedAt`: 创建时间
- `UpdatedAt`: 更新时间
- `IsDefault`: 是否为默认预设

**方法**:
- `Clone()`: 创建深拷贝

## 依赖关系

**依赖**:
- CommunityToolkit.Mvvm (用于 ObservableObject)
- System.Text.Json (用于 JSON 序列化)

**被依赖**:
- CrosshairPro.Infrastructure
- CrosshairPro.Services
- CrosshairPro.Application
- CrosshairPro.App

## 设计原则

1. **无业务逻辑**: Core 层只定义数据结构和接口，不包含业务实现
2. **接口抽象**: 所有对外能力通过接口定义，实现类在 Services/Infrastructure 层
3. **可观察模型**: 使用 CommunityToolkit.Mvvm 的 `ObservableObject` 作为模型基类
4. **深拷贝支持**: 核心配置模型提供 `Clone()` 方法，支持状态隔离

## 相关文档

- [数据模型](data-model.md) - 详细的模型定义和关系图
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史