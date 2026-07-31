# CrosshairPro.App

表现层模块，提供 WPF 用户界面，包括主窗口、准心叠加窗口、自定义控件和主题系统。

## 概述

App 模块是整个应用的用户界面层，负责：
- 应用程序启动和依赖注入配置
- 主窗口界面（MainWindow + MainViewModel）
- 准心叠加窗口（OverlayWindow）
- 自定义控件库（CrosshairPreview、DialogBase、ToastNotification、IconButton）
- 主题系统（DesignTokens、ControlStyles）

## 目录结构

```
CrosshairPro.App/
├── App.xaml.cs                 # 应用程序入口，DI 配置
├── App.xaml                    # 应用资源，全局样式
├── MainWindow.xaml.cs          # 主窗口代码后置
├── MainWindow.xaml             # 主窗口 XAML
├── ViewModels/
│   └── MainViewModel.cs        # 主视图模型，业务逻辑
├── Views/
│   └── OverlayWindow.cs        # 准心叠加窗口
├── Controls/
│   ├── CrosshairPreview.cs     # 准心预览控件
│   ├── DialogBase.cs           # 对话框基类
│   ├── ToastNotification.cs    # Toast 通知控件
│   ├── ToastManager.cs         # Toast 通知管理器
│   └── IconButton.cs           # 图标按钮控件
├── Helpers/
│   └── ThemeHelper.cs          # 主题资源访问助手
├── Themes/
│   ├── DesignTokens.xaml       # 设计令牌（颜色、字体、间距）
│   ├── ControlStyles.xaml      # 控件样式定义
│   └── IconGeometries.xaml     # 图标几何数据
└── Assets/
    └── app-icon.ico            # 应用图标
```

## 核心组件

### App.xaml.cs

应用程序入口，负责：
- 配置依赖注入容器
- 注册所有服务（Singleton/Transient）
- 启动主窗口

```csharp
_services = new ServiceCollection()
    .AddCrosshairProServices()      // Application 层服务
    .AddSingleton<MainViewModel>()  // 主 ViewModel
    .AddSingleton<OverlayWindow>()  // 准心窗口（单例）
    .AddTransient<MainWindow>()     // 主窗口（瞬态）
    .BuildServiceProvider();
```

### MainWindow

主窗口，包含：
- 自定义标题栏（最小化、最大化、关闭按钮）
- 准心预览区域
- 配置控制面板（样式、大小、间隙、颜色等）
- 预设管理界面
- 系统托盘图标

### MainViewModel

主视图模型，使用 CommunityToolkit.Mvvm 实现 MVVM 模式：
- 管理当前配置（CrosshairConfig）
- 处理预设加载/保存/导入/导出
- 响应用户操作命令
- 触发事件通知 UI 更新

### OverlayWindow

准心叠加窗口：
- 全屏透明置顶窗口
- 鼠标穿透（WS_EX_TRANSPARENT）
- 使用 WPF Shape 元素渲染准心
- 实时响应配置变更

## 依赖关系

```
App → Application → Services, Infrastructure
                   → Core
```

**依赖模块**：
- `CrosshairPro.Application` - 配置服务、预设服务
- `CrosshairPro.Services` - 配置仓库、渲染器实现
- `CrosshairPro.Infrastructure` - 热键管理、Win32 API
- `CrosshairPro.Core` - 模型、接口、枚举

**第三方依赖**：
- `CommunityToolkit.Mvvm` - MVVM 框架
- `Hardcodet.Wpf.TaskbarNotification` - 系统托盘
- `Microsoft.Extensions.DependencyInjection` - DI 容器

## 主题系统

### 设计令牌（DesignTokens.xaml）

定义应用的全局设计变量：

| 类别 | 令牌 | 值 |
|------|------|------|
| 背景色 | BackgroundColor | #0D0D1A |
| 表面色 | SurfaceColor | #161628 |
| 控件色 | ControlColor | #1E1E36 |
| 强调色 | AccentColor | #00FF00（霓虹绿） |
| 字体 | FontFamilyPrimary | Microsoft YaHei UI, Segoe UI |
| 字体 | FontFamilyMono | Cascadia Code, Consolas |
| 圆角 | RadiusSm/Md/Lg | 4/6/8 |

### 控件样式（ControlStyles.xaml）

自定义控件样式，统一外观：
- `PrimaryButton` - 主要操作按钮（绿色强调）
- `SecondaryButton` - 次要操作按钮
- `CustomSlider` - 自定义滑块
- `CustomTextBox` - 自定义文本框
- `CustomComboBox` - 自定义下拉框
- `CustomCheckBox` - 自定义复选框
- `CustomExpander` - 自定义展开器

## 详细文档

- [数据模型](data-model.md) - UI 组件和数据模型
- [坑点](pitfalls.md) - 已知问题和注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史
