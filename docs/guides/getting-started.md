# 快速入门指南

## 环境要求

| 工具 | 版本要求 |
|------|----------|
| Visual Studio | 2022 17.8+ 或 JetBrains Rider |
| .NET SDK | 8.0+ |
| Windows | 10/11 |

## 获取源码

```bash
git clone <repository-url>
cd Crosshair
```

## 打开项目

### Visual Studio

1. 打开 `CrosshairPro.slnx` 解决方案文件
2. 等待 NuGet 包还原完成
3. 设置 `CrosshairPro.App` 为启动项目

### JetBrains Rider

1. 打开 `CrosshairPro.slnx` 解决方案文件
2. 等待索引和包还原完成
3. 设置运行配置指向 `CrosshairPro.App`

## 构建和运行

### 命令行

```bash
# 还原依赖
dotnet restore

# 构建
dotnet build

# 运行
dotnet run --project src/CrosshairPro.App
```

### IDE

按 `F5` 或点击"启动"按钮运行项目。

## 项目结构

```
Crosshair/
├── src/
│   ├── CrosshairPro.Core/            # 核心层（模型和接口）
│   ├── CrosshairPro.Infrastructure/   # 基础设施层（Win32 和热键）
│   ├── CrosshairPro.Services/         # 服务层（渲染和配置）
│   └── CrosshairPro.App/             # 应用层（UI）
├── docs/                              # 文档
└── CrosshairPro.slnx                 # 解决方案文件
```

## 基本使用

### 运行应用

1. 启动应用后，会显示主窗口和覆盖窗口
2. 覆盖窗口在屏幕中央显示默认准星（绿色十字）

### 调整准星

- **样式选择**：使用下拉框选择准星样式
- **大小调整**：拖动"大小"滑块
- **间距调整**：拖动"间距"滑块（十字/T/X 样式）
- **粗细调整**：拖动"粗细"滑块
- **透明度调整**：拖动"透明度"滑块
- **颜色选择**：点击预设颜色按钮

### 切换准星显示

- 点击"切换准星"按钮
- 或使用快捷键 `Ctrl+Shift+X`

## 开发指南

### 添加新准星样式

1. 在 `CrosshairStyle` 枚举中添加新值

```csharp
// src/CrosshairPro.Core/Enums/CrosshairStyle.cs
public enum CrosshairStyle
{
    Cross = 0,
    Dot = 1,
    Circle = 2,
    TShape = 3,
    XShape = 4,
    CustomImage = 5,
    NewStyle = 6  // 新样式
}
```

2. 在 `CrosshairRenderer` 中添加渲染方法

```csharp
// src/CrosshairPro.Services/Crosshair/CrosshairRenderer.cs
private void RenderNewStyle(DrawingContext dc, CrosshairConfig config)
{
    // 实现渲染逻辑
}
```

3. 在 `Render` 方法的 switch 中添加 case

```csharp
case CrosshairStyle.NewStyle:
    RenderNewStyle(dc, config);
    break;
```

4. 在 `OverlayWindow` 中添加对应的渲染方法

```csharp
// src/CrosshairPro.App/Views/OverlayWindow.cs
private void RenderNewStyle()
{
    // 实现覆盖窗口渲染
}
```

5. 更新 `CrosshairStyleNames` 数组

```csharp
// src/CrosshairPro.App/ViewModels/MainViewModel.cs
public string[] CrosshairStyleNames => new[]
{
    "十字", "圆点", "圆形", "T 形", "X 形", "自定义图片", "新样式"
};
```

### 添加新热键操作

1. 在 `HotkeyAction` 枚举中添加新值

```csharp
// src/CrosshairPro.Core/Models/HotkeyBinding.cs
public enum HotkeyAction
{
    ToggleCrosshair = 0,
    SwitchPreset = 1,
    ResetPosition = 2,
    LockPosition = 3,
    IncreaseSize = 4,
    DecreaseSize = 5,
    NewAction = 6  // 新操作
}
```

2. 在 `MainWindow.OnHotkeyTriggered` 中添加处理逻辑

```csharp
// src/CrosshairPro.App/MainWindow.xaml.cs
case HotkeyAction.NewAction:
    // 实现新操作
    break;
```

### 添加新配置属性

1. 在 `CrosshairConfig` 中添加属性

```csharp
// src/CrosshairPro.Core/Models/CrosshairConfig.cs
private int _newProperty = 0;
public int NewProperty
{
    get => _newProperty;
    set => SetProperty(ref _newProperty, value);
}
```

2. 在 UI 中添加对应的控件（MainWindow.xaml）

3. 在渲染器中使用新属性

## 常见问题

### 覆盖窗口不显示

- 检查窗口是否被隐藏：点击"切换准星"按钮
- 检查其他窗口是否遮挡：覆盖窗口应该在最顶层

### 热键不工作

- 检查热键是否被其他应用占用
- 尝试以管理员权限运行应用

### 准星位置偏移

- 当前版本准星固定在屏幕中央
- 后续版本将支持自定义位置

## 下一步

- 阅读 [架构概览](../architecture/overview.md) 了解系统设计
- 阅读 [Core 层文档](../modules/core.md) 了解数据模型
- 阅读 [Services 层文档](../modules/services.md) 了解业务逻辑
