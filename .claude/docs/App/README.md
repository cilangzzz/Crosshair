# CrosshairPro.App

应用层模块，WPF 用户界面入口。

## 概述

App 模块是整个应用的用户交互层：
- 主窗口（MainWindow）
- 准心覆盖窗口（OverlayWindow）
- 主视图模型（MainViewModel）
- 控件和样式资源

## 目录结构

```
CrosshairPro.App/
├── App.xaml                   # 应用资源和启动配置
├── App.xaml.cs                # 应用入口
├── MainWindow.xaml            # 主窗口布局
├── MainWindow.xaml.cs         # 主窗口逻辑
├── Controls/
│   └── CrosshairPreview.cs    # 准心预览控件
├── Themes/
│   ├── ControlStyles.xaml     # 控件样式定义
│   └── DesignTokens.xaml      # 设计令牌（颜色、字体）
├── ViewModels/
│   └── MainViewModel.cs       # 主视图模型
└── Views/
    └── OverlayWindow.cs       # 准心覆盖窗口
```

## 核心类

### App

应用入口，继承 `Application`：
- 加载样式资源
- 初始化依赖注入容器
- 创建主窗口

### MainWindow

主窗口，用户控制面板：
- 准心样式选择
- 颜色/大小/间隙调节
- 效果开关（描边、阴影、发光）
- 预设管理（保存/加载/导入/导出）
- 热键绑定配置

**关键职责**：
- 监听 `MainViewModel.ConfigUpdated` 事件
- 调用 `OverlayWindow.UpdateConfig()` 更新准心
- 处理文件对话框（选择图片、导出配置）

### OverlayWindow

准心覆盖窗口，透明置顶：
- 全屏透明窗口
- 使用 WPF Shape 元素渲染准心
- 鼠标穿透（不影响游戏操作）
- 始终置顶

**Win32 扩展样式**：
```csharp
int exStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
```

### MainViewModel

主视图模型，使用 CommunityToolkit.Mvvm：
- 准心配置状态管理
- 预设列表管理
- 命令绑定（SetColor、ToggleCrosshair、ResetConfig）
- 事件通知（ConfigUpdated、ToggleCrosshairRequested）

**关键属性**：
| 属性 | 类型 | 说明 |
|------|------|------|
| Config | CrosshairConfig | 当前准心配置 |
| IsCrosshairVisible | bool | 准心是否显示 |
| Presets | List<Preset> | 预设列表 |
| SelectedPreset | Preset? | 当前选中预设 |

### CrosshairPreview

准心预览控件，在主窗口中实时显示准心效果：
- 缩放显示（适应控件大小）
- 不影响实际 Overlay 窗口

## 详细文档

- [数据模型](data-model.md) - UI 相关模型说明
- [坑点](pitfalls.md) - 已知问题和注意事项