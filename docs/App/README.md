# CrosshairPro.App

表现层模块（WPF UI）：应用程序入口与依赖注入、主窗口与页面导航、准心叠加窗口、游戏配置页面（Apex Legends / Counter-Strike 2 / 电脑优化）、自定义控件库、中英文本地化与主题系统。

## 概述

App 模块是整个应用的用户界面层，负责：

- 应用程序启动、全局异常处理和依赖注入配置
- 主窗口界面（无边框窗口 + 自定义标题栏 + 左侧图标导航 + 系统托盘 + 全局热键）
- 页面导航系统：准心配置页（CrosshairPage）、游戏配置页（GamesPage）
- 游戏配置页面：ApexPage（videoconfig.txt / settings.cfg / 启动项 / 备份历史）、Cs2Page（video.txt 字段 / autoexec.cfg 字段 / Steam 启动项）、PcOptimizePage（电脑优化：29 项优化条目，仅提供系统管理页入口，不代改任何设置）
- 准心叠加窗口（OverlayWindow，全屏透明置顶 + 鼠标穿透）
- 自定义控件库（DialogBase、ToastManager、ToastNotification、IconButton、TabNavItem、SmoothScrollViewer 等）
- 中英文本地化（`{loc:Loc}` 标记扩展 + 内嵌 resx 资源，支持运行时切换）
- 主题系统（DesignTokens 设计令牌 + ControlStyles 控件样式 + IconGeometries 图标）

源码位置：`src/CrosshairPro.App`，共 52 个文件（37 个 .cs、10 个 .xaml、3 个 .resx、1 个 csproj、1 个图标资源）。

## 目录结构

```
CrosshairPro.App/
├── CrosshairPro.App.csproj                 # 项目文件（net8.0-windows / WPF，SupportedCultures: zh-CN;en）
├── App.xaml                                # 应用资源：合并 ControlStyles，注册全局样式与 7 个转换器
├── App.xaml.cs                             # 应用入口：DI 容器、全局异常处理、语言初始化
├── AssemblyInfo.cs                         # ThemeInfo 程序集特性
├── MainWindow.xaml                         # 主窗口布局：标题栏 + 左侧导航 + 页面容器 + 底部工具栏/状态栏
├── MainWindow.xaml.cs                      # 主窗口逻辑：ViewModel 事件接线、托盘、热键、对话框、语言切换
├── ConfigTypeToVisibilityConverter.cs      # ConfigItemType → Visibility（按 parameter 匹配类型）
├── IntToBoolConverter.cs                   # int ↔ bool（0 = false，非 0 = true）
├── StringToVisibilityConverter.cs          # 非空字符串 → Visibility
├── UtcToLocalTimeConverter.cs              # UTC DateTime → 本地时间字符串（yyyy-MM-dd HH:mm，预设创建时间）
├── GameProfileTitleConverter.cs            # 游戏选择器标题（IMultiValueConverter：DisplayNameLocKey 优先，回退 DisplayName）
├── LocalizedTextConverter.cs               # 本地化文本（IMultiValueConverter：locKey + CurrentCulture 触发重算，电脑优化页用）
├── ViewModels/
│   ├── MainViewModel.cs                    # 主 ViewModel：页面导航 + 事件转发（含 PageType 枚举）
│   ├── CrosshairViewModel.cs               # 准心配置 ViewModel：配置、预设加载/保存/导入/导出
│   ├── GamesViewModel.cs                   # 游戏页 ViewModel：内置游戏列表与选中管理
│   ├── ApexConfigViewModel.cs              # Apex 配置 ViewModel：视频/设置配置、启动项、备份历史
│   ├── Cs2ConfigViewModel.cs               # CS2 配置 ViewModel：基于策略的分区配置 + 启动项
│   ├── PcOptimizeViewModel.cs              # 电脑优化 ViewModel：7 分类 29 项静态数据 + OpenItem 命令（打开系统管理页）
│   └── ConfigItemWrapper.cs                # 配置项/分区 UI 包装类（ConfigItemWrapper、ConfigSectionWrapper）
├── Views/
│   ├── OverlayWindow.cs                    # 准心叠加窗口：全屏透明置顶、鼠标穿透、Shape 渲染
│   ├── CrosshairPage.xaml                  # 准心配置页布局：预览画布（网格+准心）+ 控制面板
│   ├── CrosshairPage.xaml.cs               # 准心页逻辑：预览渲染、颜色选择（WinForms ColorDialog）
│   ├── GamesPage.xaml                      # 游戏配置页布局：游戏选择下拉框 + 游戏页面容器
│   ├── GamesPage.xaml.cs                   # 游戏页逻辑：从主窗口 DataContext 获取 GamesViewModel
│   └── GamePages/
│       ├── ApexPage.xaml                   # Apex Legends 配置页布局（配置文件/启动项/视频/画质/灵敏度/预设）
│       ├── ApexPage.xaml.cs                # Apex 页逻辑：解析 ApexConfigViewModel、订阅 Toast
│       ├── Cs2Page.xaml                    # CS2 配置页布局（视频/游戏设置/启动项）
│       ├── Cs2Page.xaml.cs                 # CS2 页逻辑：解析 Cs2ConfigViewModel、加载配置、订阅 Toast
│       ├── PcOptimizePage.xaml             # 电脑优化页布局（7 分类卡片 + 条目行 + 避坑卡 + 页脚注，SmoothScrollViewer）
│       ├── PcOptimizePage.xaml.cs          # 电脑优化页 code-behind：解析 PcOptimizeViewModel、订阅 Toast
│       ├── GamePageSelector.cs             # 按 GameId（builtin-apex/builtin-cs2/builtin-pcopt）选择页面模板
│       └── ConfigItemTemplateSelector.cs   # 按 ConfigItemType 选择配置项统一行模板（CS2 页）
├── Controls/
│   ├── CrosshairPreview.cs                 # 准心预览控件（OnRender 绘制；当前 XAML 未引用，见数据模型）
│   ├── DialogBase.cs                       # 对话框基类：统一无边框对话框、输入对话框工厂
│   ├── IconButton.cs                       # 图标按钮：内置图标名查找（含 IconPosition 枚举）
│   ├── TabNavItem.cs                       # 左侧导航图标项：选中态、悬停态、左侧指示条
│   ├── PageTemplateSelector.cs             # 按 PageType 选择页面 DataTemplate
│   ├── ScrollViewerBehavior.cs             # ScrollViewer 垂直偏移附加属性（配合滚动动画）
│   ├── SmoothScrollViewer.cs               # 平滑滚动 ScrollViewer（CompositionTarget.Rendering 驱动）
│   ├── ToastNotification.cs                # Toast 通知控件：容器内显示、定时自动消失
│   └── ToastManager.cs                     # Toast 管理器：独立置顶悬浮窗口 + 淡入淡出动画
├── Helpers/
│   └── ThemeHelper.cs                      # 主题资源访问助手（颜色/画刷/阴影/字体/圆角）
├── Localization/
│   ├── LocalizationProvider.cs             # 本地化单例：文化管理、索引器取词、动态切换通知
│   ├── LocExtension.cs                     # XAML 标记扩展 {loc:Loc Key=...}（Binding 索引器实现）
│   ├── Strings.cs                          # ResourceManager 包装（访问内嵌 resx）
│   ├── Strings.resx                        # 中性资源（默认中文，297 键）
│   ├── Strings.en.resx                     # 英文资源（297 键）
│   └── Strings.zh-CN.resx                  # 简体中文资源（297 键）
├── Themes/
│   ├── DesignTokens.xaml                   # 设计令牌：颜色/字体/间距/圆角/阴影效果
│   ├── ControlStyles.xaml                  # 控件样式（合并 DesignTokens + IconGeometries）
│   └── IconGeometries.xaml                 # Material Design 风格图标几何数据（窗口/操作/导航/状态等）
└── Assets/
    └── app-icon.ico                        # 应用图标（主窗口 + 托盘）
```

## 核心架构

```
App.OnStartup
  ├─ 单实例校验：命名 Mutex `Local\CrosshairPro.SingleInstance`（重复启动→弹提示→Shutdown）
  ├─ 构建 DI 容器（AddCrosshairProServices + App 层注册）
  ├─ LocalizationProvider.Initialize()（默认 zh-CN）
  └─ 显示 MainWindow
        ├─ MainWindow 构造注入 MainViewModel / OverlayWindow / IHotkeyManager
        ├─ 接线 ViewModel 事件 → 覆盖窗口、文件对话框、Toast
        ├─ 注册全局热键 Ctrl+Shift+X；创建托盘图标；显示 OverlayWindow
        └─ Loaded 后从 IAppStateRepository 读取已保存语言并应用
页面导航
  MainViewModel.CurrentPage ──PageTemplateSelector──▶ CrosshairPage / GamesPage
  GamesViewModel.CurrentGameId ──GamePageSelector──▶ ApexPage / Cs2Page / PcOptimizePage
配置更新流
  CrosshairViewModel.Config 变更 → ConfigUpdated 事件 → MainWindow
    → OverlayWindow.UpdateConfig()（CopyFrom 内部副本）→ 重绘准心
游戏配置流
  ApexPage/Cs2Page 从 App.Services 解析各自 Singleton ViewModel
    → IApexConfigService / IGameConfigService 读写配置与启动项
```

### 依赖注入配置

```csharp
_services = new ServiceCollection()
    .AddCrosshairProServices()      // Application 层服务（配置/预设/游戏配置/Apex）
    .AddSingleton<CrosshairViewModel>()
    .AddSingleton<GamesViewModel>()
    .AddSingleton<ApexConfigViewModel>()
    .AddSingleton<Cs2ConfigViewModel>()
    .AddSingleton<PcOptimizeViewModel>()
    .AddSingleton<MainViewModel>()
    .AddSingleton<OverlayWindow>()   // 覆盖窗口单例
    .AddTransient<MainWindow>()      // 主窗口瞬态
    .BuildServiceProvider();
```

`App.Services` 公开容器，ApexPage / Cs2Page / PcOptimizePage / 语言偏好保存等通过它解析服务（如 `IAppStateRepository`）。

## 依赖关系

```
App → Application（配置/预设/游戏配置服务接口）
    → Services、Infrastructure（经由 Application DI 注册引入）
    → Core（模型、接口、枚举）
```

**第三方依赖**（NuGet）：

| 包 | 版本 | 用途 |
|------|------|------|
| CommunityToolkit.Mvvm | 8.3.2 | ObservableObject / RelayCommand |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | DI 容器 |
| Serilog | 4.1.0 | 日志（ViewModel 错误记录） |
| Serilog.Sinks.File / Serilog.Sinks.Console | 6.0.0 | 日志输出目标（文件 / 控制台） |
| Hardcodet.NotifyIcon.Wpf | 1.1.0 | 系统托盘图标 |

游戏数据模型（GameProfile、GameConfig、ConfigItemDefinition 等）定义在 Core 模块，见 [../Core/README.md](../Core/README.md)；服务接口（IConfigurationService、IPresetService、IGameConfigService、IApexConfigService）见 [../Application/README.md](../Application/README.md)；各游戏配置文件协议与游戏配置功能详见 [../GameConfig/README.md](../GameConfig/README.md)。

## 详细文档

- [数据模型](data-model.md) - ViewModel 属性/事件/命令、控件 API、本地化架构、设计令牌
- [坑点](pitfalls.md) - 已知问题与注意事项
- [变更日志](CHANGELOG.md) - 模块变更历史
