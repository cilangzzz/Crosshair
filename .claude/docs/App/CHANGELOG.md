# Changelog - App

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-09-01] feat: add CS2 config support and update docs

**类型**: feat
**提交**: f5d4d18
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| ViewModels/Cs2ConfigViewModel.cs | +195/-0 | 新增 CS2 配置视图模型，复用 GameConfig 策略读写 CS2 配置 |
| Views/GamePages/Cs2Page.xaml | +194/-0 | 新增 CS2 配置页面 UI |
| Views/GamePages/Cs2Page.xaml.cs | +55/-0 | CS2 页面后台（初始化与保存流程） |
| Views/GamePages/GamePageSelector.cs | +5/-1 | 新增 Cs2Template，按游戏 ID "builtin-cs2" 映射到 CS2 页面模板 |
| Localization/Strings.resx | +64/-0 | CS2 词条（默认语言） |
| Localization/Strings.en.resx | +64/-0 | CS2 词条（英文） |
| Localization/Strings.zh-CN.resx | +64/-0 | CS2 词条（简体中文） |
| Views/GamesPage.xaml | +5/-0 | 游戏页布局微调 |
| App.xaml.cs | +1/-0 | DI 注册 Cs2ConfigViewModel 单例 |

### 影响范围
- **API**: DI 容器新增 Cs2ConfigViewModel；GamePageSelector 新增 Cs2Template 属性
- **跨模块**: 无源码变更；同提交更新 docs/GameConfig/CS2 文档（integration、pitfalls、data-model 等）
- **数据模型**: 无
- **配置**: 无

---

## [2026-09-01] feat: add Chinese localization support

**类型**: feat
**提交**: eb57c0b
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Localization/Strings.zh-CN.resx | +637/-0 | 新增简体中文资源文件 |
| CrosshairPro.App.csproj | +2/-1 | NeutralLanguage 设为 zh-CN，SupportedCultures 设为 zh-CN;en |
| Localization/LocExtension.cs | +13/-17 | 由返回静态字符串改为 Binding 索引器路径 [Key]（绑定 LocalizationProvider.this[Key]），"Item[]" PropertyChanged 触发全部绑定自动刷新 |
| Localization/LocalizationProvider.cs | +20/-13 | 配合动态绑定机制调整 |
| Views/GamePages/ApexPage.xaml | +97/-96 | 文案改为 loc:Loc 动态绑定 |
| Views/CrosshairPage.xaml | +17/-16 | 同上 |
| MainWindow.xaml | +11/-11 | 同上 |
| Views/GamesPage.xaml | +2/-1 | 同上 |

### 影响范围
- **API**: loc:Loc 标记扩展返回类型变为 BindingExpression；所有页面文案改为动态绑定
- **跨模块**: 无（仅 App）
- **数据模型**: 无
- **配置**: 程序集中性文化设为 zh-CN，未覆盖语言的回退行为改变

---

## [2026-08-10] fix: improve localization and UI polish

**类型**: fix
**提交**: 73c777d
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| App.xaml.cs | +46/-31 | 新增 DispatcherUnhandledException 全局异常处理；DI 构建包入 try/catch（失败弹窗并 Shutdown）；语言改为 MainWindow.Loaded 后异步加载，移除启动时同步读 appstate 的阻塞 |
| Views/GamePages/ApexPage.xaml | +150/-149 | UI 打磨与本地化修正 |
| Localization/LocExtension.cs | +10/-16 | 修复语言键解析逻辑 |
| MainWindow.xaml | +11/-11 | 布局与文案调整 |
| Views/CrosshairPage.xaml | +16/-17 | 布局调整 |
| Localization/LocalizationProvider.cs | +4/-7 | 修复初始化逻辑 |
| Views/GamesPage.xaml | +1/-2 | 微调 |

### 影响范围
- **API**: 启动流程变化——语言初始化异步化，异常不再静默吞掉而是弹窗
- **跨模块**: 无
- **数据模型**: 无
- **配置**: appstate.json 的 Language 改为窗口加载后异步读取

---

## [2026-08-10] feat: add localization support and improve UI components

**类型**: feat
**提交**: 55064e1
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Localization/LocalizationProvider.cs | +119/-0 | 新增语言提供者（单例，负责读取/切换语言并广播变更） |
| Localization/Strings.resx | +637/-0 | 默认语言资源 |
| Localization/Strings.en.resx | +613/-0 | 英文资源 |
| Localization/LocExtension.cs | +48/-0 | 新增 loc:Loc 标记扩展 |
| Localization/Strings.cs | +54/-0 | 强类型资源键访问 |
| Controls/SmoothScrollViewer.cs | +173/-0 | 平滑滚动控件 |
| Controls/ScrollViewerBehavior.cs | +60/-0 | 滚动附加行为 |
| Views/GamePages/ApexPage.xaml | +365/-564 | 大规模重构为本地化绑定布局 |
| ViewModels/ApexConfigViewModel.cs | +63/-50 | 文案绑定本地化 |
| MainWindow.xaml.cs | +58/-22 | 窗口文案与初始化适配 |
| ViewModels/CrosshairViewModel.cs | +22/-14 | 同上 |
| App.xaml.cs | +26/-0 | 启动时 InitializeLocalization（同步读 appstate 的 Language） |
| MainWindow.xaml | +21/-12 | 布局调整 |
| Views/CrosshairPage.xaml | +20/-18 | 布局调整 |
| Themes/ControlStyles.xaml | +39/-0 | 新样式 |
| CrosshairPro.App.csproj | +2/-1 | 声明 SupportedCultures en |
| Views/GamesPage.xaml | +2/-1 | 微调 |
| Core/Models/ApexVideoConfig.cs | +52/-0 | （跨模块，同提交）新增配置字段 |
| Core/Models/AppPersistedState.cs | +3/-0 | （跨模块，同提交）新增 Language 字段 |
| Services/ApexConfigService.cs | +3/-0 | （跨模块，同提交）适配新字段 |

### 影响范围
- **API**: AppPersistedState 新增 Language 字段；新增本地化基础设施（LocalizationProvider/loc:Loc）
- **跨模块**: Core 数据模型（ApexVideoConfig、AppPersistedState）与 Services 同提交变更
- **数据模型**: AppPersistedState、ApexVideoConfig 变更
- **配置**: appstate.json 新增 Language 键；旧文件缺省该键时按默认语言处理

---

## [2026-07-31] refactor: improve Apex config UI binding

**类型**: refactor
**提交**: c69660e
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/GamePages/ApexPage.xaml | +436/-48 | 绑定方式改进、界面分区重排 |
| ViewModels/ApexConfigViewModel.cs | +209/-9 | 绑定属性与保存流程重构 |
| InverseBooleanToVisibilityConverter.cs | +31/-0 | 新增反向布尔到 Visibility 转换器 |
| App.xaml | +3/-0 | 注册 InverseBooleanToVisibilityConverter 资源 |

### 影响范围
- **API**: App 资源字典新增 InverseBooleanToVisibilityConverter
- **跨模块**: 无（4 个文件全部在 App）
- **数据模型**: 无
- **配置**: 无

---

## [2026-07-31] feat: implement Apex Legends config service and CS2 docs

**类型**: feat
**提交**: c85e699
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| ViewModels/ApexConfigViewModel.cs | +316/-45 | 接入 IApexConfigService，新增启动项/设置/视频配置属性与读写流程 |
| Views/GamePages/ApexPage.xaml | +466/-29 | Apex 完整配置界面（启动项、视频、游戏内设置） |
| IntToBoolConverter.cs | +30/-0 | 新增 int 到 bool 转换器（0 为 false） |
| Views/GamePages/ApexPage.xaml.cs | +19/-1 | 页面初始化适配 |
| App.xaml | +3/-0 | 注册 IntToBoolConverter |
| Application/Interfaces/IApexConfigService.cs | 新增 | （跨模块，同提交）Apex 配置服务接口 |
| Application/Services/ApexConfigService.cs | 新增 | （跨模块，同提交）Apex 配置读写实现 |
| Application/DI/ServiceCollectionExtensions.cs | 修改 | （跨模块，同提交）注册 ApexConfigService |
| Core/Models/ApexLaunchOptions.cs | 新增 | （跨模块，同提交）Apex 启动项模型 |
| Core/Models/ApexSettingsConfig.cs | 新增 | （跨模块，同提交）Apex 设置模型（+251 行） |
| Core/Models/ApexVideoConfig.cs | 新增 | （跨模块，同提交）Apex 视频设置模型（+263 行） |

### 影响范围
- **API**: 新增 IApexConfigService 并注册 DI；App 资源字典新增 IntToBoolConverter
- **跨模块**: Core 新增 3 个 Apex 数据模型、Application 新增服务（同提交变更）
- **数据模型**: ApexLaunchOptions、ApexSettingsConfig、ApexVideoConfig（新文件）
- **配置**: Apex 配置改由 ApexConfigService 持久化（路径见 Services 模块文档）

### 回滚指南（HIGH 风险）
- 回滚: `git revert c85e699`
- 检查文件: src/CrosshairPro.App/App.xaml（转换器注册）、src/CrosshairPro.App/Views/GamePages/ApexPage.xaml、src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs
- 副作用: 后继提交 c69660e、55064e1 均在其基础上重构同一批文件，单独 revert 大概率冲突；如需撤销整个 Apex 功能应连同后续提交一并处理

---

## [2026-07-31] feat: add game-specific config system with Apex Legends support

**类型**: feat
**提交**: 3f40dcf
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/GamesPage.xaml | +24/-154 | 静态游戏卡片改为模板驱动布局 |
| Views/GamePages/ApexPage.xaml | +117/-0 | Apex 页面骨架 |
| ViewModels/GamesViewModel.cs | +34/-85 | 改为基于 GameProfile 的游戏配置管理 |
| ViewModels/ApexConfigViewModel.cs | +84/-0 | Apex 配置 VM 骨架 |
| ViewModels/ConfigItemWrapper.cs | +64/-0 | 通用配置项包装（供绑定） |
| Views/GamePages/GamePageSelector.cs | +26/-0 | 按 GameProfile 选择游戏页面模板 |
| Views/GamePages/ApexPage.xaml.cs | +30/-0 | 页面后台 |
| ConfigTypeToVisibilityConverter.cs | +31/-0 | 配置类型到 Visibility 转换器 |
| Views/GamesPage.xaml.cs | +15/-1 | 适配模板驱动 |
| App.xaml.cs | +7/-2 | DI 注册新 ViewModel |
| App.xaml | +3/-0 | 注册 ConfigTypeToVisibilityConverter |
| ViewModels/MainViewModel.cs | +1/-4 | 微调 |
| Core/Models/GameProfile.cs | +4/-1 | （跨模块，同提交）新增 IsSelected 属性 |

### 影响范围
- **API**: GamePageSelector 新增模板选择机制；DI 新增 ApexConfigViewModel
- **跨模块**: Core GameProfile 模型小改（新增 IsSelected，同提交）
- **数据模型**: GameProfile 变更（向后兼容，新增字段）
- **配置**: 无

---

## [2026-07-31] feat: implement tab navigation UI with pages and viewmodels

**类型**: feat
**提交**: ced3254
**风险**: HIGH

> 注：本文件下方两条无提交号、同为 2026-07-31 的条目（页面导航系统 / 自定义控件与主题）描述的是本提交的部分内容。

### 变更文件（App 共 23 个文件，+3218/-708）
| 文件 | 变更 | 说明 |
|------|------|------|
| ViewModels/CrosshairViewModel.cs | +337/-0 | 准心配置 VM（自 MainViewModel 拆分） |
| Views/CrosshairPage.xaml.cs | +340/-0 | 准心页后台（预览渲染） |
| Themes/ControlStyles.xaml | +485/-1 | 按钮、标签页等全套样式 |
| Views/GamesPage.xaml | +200/-0 | 游戏页 UI |
| Views/GamesPage.xaml.cs | +24/-0 | 游戏页后台（从主窗口 DataContext 取 GamesViewModel） |
| MainWindow.xaml | +92/-208 | 移除内嵌控制面板，改为导航壳 |
| MainWindow.xaml.cs | +26/-227 | 内嵌逻辑移入页面 |
| ViewModels/MainViewModel.cs | +59/-269 | 改为页面导航容器 |
| Controls/DialogBase.cs | +257/-0 | 对话框基类 |
| Controls/ToastManager.cs | +238/-0 | 独立悬浮 Toast 窗口 |
| Controls/IconButton.cs | +222/-0 | 图标按钮控件 |
| Controls/ToastNotification.cs | +115/-0 | Toast 通知控件 |
| Controls/TabNavItem.cs | +142/-0 | 侧边导航项控件 |
| Helpers/ThemeHelper.cs | +146/-0 | 主题资源类型安全访问 |
| Themes/IconGeometries.xaml | +148/-0 | 矢量图标资源 |
| ViewModels/GamesViewModel.cs | +129/-0 | 游戏配置 VM |
| Views/CrosshairPage.xaml | +155/-0 | 准心页 UI |
| Controls/PageTemplateSelector.cs | +33/-0 | 页面模板选择器 |
| Themes/DesignTokens.xaml | +33/-0 | 设计令牌补充 |
| StringToVisibilityConverter.cs | +27/-0 | 字符串到可见性转换器 |
| Controls/CrosshairPreview.cs | +5/-3 | 适配 |
| App.xaml | +3/-0 | 资源合并 |
| App.xaml.cs | +2/-0 | DI 注册新 VM |
| Core/Models/GameConfig.cs | +20/-0 | （跨模块，同提交）游戏配置模型 |
| Core/Models/GameConfigStrategy.cs | +91/-0 | （跨模块，同提交）游戏配置策略 |
| Application/Interfaces/IGameConfigService.cs | 新增 | （跨模块，同提交）游戏配置服务接口 |
| Application/Services/GameConfigService.cs | 新增 | （跨模块，同提交）游戏配置服务实现 |
| Application/DI/ServiceCollectionExtensions.cs | 修改 | （跨模块，同提交）注册 GameConfigService |

### 影响范围
- **API**: DI 新增 CrosshairViewModel、GamesViewModel、GameConfigService；新增 IGameConfigService
- **跨模块**: Core 新增 GameConfig/GameConfigStrategy 模型、Application 新增 GameConfigService（同提交变更）
- **数据模型**: GameConfig、GameConfigStrategy（新文件）
- **配置**: 游戏专属配置由 GameConfigService 管理（路径见 Services 模块文档）

### 回滚指南（HIGH 风险）
- 回滚: `git revert ced3254`
- 检查文件: src/CrosshairPro.App/MainWindow.xaml(.cs)、src/CrosshairPro.App/App.xaml.cs（DI 注册）、src/CrosshairPro.Core/Models/GameConfig.cs、src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs
- 副作用: 后继 3f40dcf、c85e699、c69660e 均基于本提交的页面架构，单独 revert 必然冲突，仅适用于该提交刚合入后立即回退

---

## [2026-07-03] fix: tray context menu not closing on outside click

**类型**: fix
**提交**: 215e843
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | +16/-55 | 托盘右键菜单打开后调用 SetForegroundWindow 激活菜单窗口并改 StaysOpen=false，修复点击外部不关闭（参考 Hardcodet NotifyIcon ShowContextMenu 实现）；ShowToast 改为调用独立 Toast 窗口，删除内联构建 Toast 的约 55 行代码 |

### 影响范围
- **API**: 无
- **跨模块**: 无
- **数据模型**: 无
- **配置**: 无

---

## [2026-06-09] feat: improve overlay window stability

**类型**: feat
**提交**: c33a4a9
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/OverlayWindow.cs | +27/-1 | SourceInitialized 时通过 SetWindowLong 设置 WS_EX_TRANSPARENT、WS_EX_TOOLWINDOW 扩展样式，实现鼠标穿透与工具窗口（不抢游戏焦点） |
| MainWindow.xaml | +2/-2 | 控件可见性修正 |
| MainWindow.xaml.cs | +1/-0 | 补充 using System.IO |

### 影响范围
- **API**: 无
- **跨模块**: 无
- **数据模型**: 无
- **配置**: 无
- **备注**: 即 CLAUDE.md 坑点「OverlayWindow 鼠标穿透：使用 WS_EX_TRANSPARENT、WS_EX_TOOLWINDOW 扩展样式」的来源提交

---

## [2026-06-08] feat: expand UI controls and overlay features

**类型**: feat
**提交**: c0f944b
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | +330/-8 | 新增系统托盘图标（TrayToolTip、ContextMenu、TrayRightMouseDown，配合 GetCursorPos 定位菜单）；自定义颜色选择器对话框（ShowColorPickerDialog、AddColorSlider）；DrawImage 图片准心预览 |
| Themes/ControlStyles.xaml | +97/-0 | 新增控件样式 |
| MainWindow.xaml | +52/-14 | 布局扩展 |
| Views/OverlayWindow.cs | +51/-5 | 新增 AddImage：图片准心渲染 |
| ViewModels/MainViewModel.cs | +11/-0 | SelectImage 命令与 SelectImageRequested 事件（通知 MainWindow 打开文件对话框） |

### 影响范围
- **API**: MainViewModel 新增 SelectImageRequested 事件
- **跨模块**: 无
- **数据模型**: 无
- **配置**: 无
- **备注**: 准心样式自此支持自定义图片

---

## [2026-06-08] feat: add app icon and refactor code

**类型**: feat
**提交**: 63ca04a
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Assets/app-icon.ico | 新增 | 应用图标（766 字节） |
| Views/OverlayWindow.cs | +35/-31 | AddLine/AddDot/AddCircle 增加 opacity 参数：透明度从 Canvas 整体设置改为逐 Shape 单独设置（修复 AllowsTransparency 窗口下整体透明度不生效） |
| MainWindow.xaml.cs | +43/-34 | 托盘图标改用 app-icon.ico（加载失败回退系统图标）；托盘菜单 StaysOpen=false |
| MainWindow.xaml | +1/-0 | 窗口 Icon 绑定 |

### 影响范围
- **API**: OverlayWindow 渲染辅助方法签名变更（新增 opacity 参数）
- **跨模块**: 无
- **数据模型**: 无
- **配置**: 无
- **备注**: 即 CLAUDE.md 坑点「必须对每个 Shape 单独设置 Opacity」的来源提交

---

## [2026-06-08] feat: add new controls and model properties

**类型**: feat
**提交**: bace47c
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | +169/-57 | CustomScrollBar、CustomScrollViewer 样式及 Thumb/ToggleButton 模板 |
| ViewModels/MainViewModel.cs | +6/-0 | 新增 ToggleCrosshairRequested 事件 |
| MainWindow.xaml.cs | +7/-0 | 订阅切换准心事件，转发操作 OverlayWindow |
| MainWindow.xaml | +1/-1 | 微调 |
| Core/Models/CrosshairConfig.cs | +4/-0 | （跨模块，同提交）新增 Brightness 属性（默认 100）并纳入克隆 |

### 影响范围
- **API**: MainViewModel 新增 ToggleCrosshairRequested 事件
- **跨模块**: Core CrosshairConfig 模型变更（同提交）
- **数据模型**: CrosshairConfig 新增 Brightness（config.json 向后兼容，缺省 100）
- **配置**: config.json 新增 brightness 键

---

## [2026-06-08] feat: enhance UI styles and layout

**类型**: feat
**提交**: 8b9739d
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | +507/-42 | 新增 NumericTextBox、CompactComboBox、ComboBoxItem 样式与 SnappyEase 缓动等 |
| MainWindow.xaml | +8/-7 | 布局微调 |

### 影响范围
- **API**: 无
- **跨模块**: 无
- **数据模型**: 无
- **配置**: 无

---

## [2026-06-07] feat: update UI themes and documentation

**类型**: feat
**提交**: 81a0ba0
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | +448/-0 | 基于设计令牌的全套控件样式 |
| MainWindow.xaml | +146/-171 | 布局重排以适配主题 |
| MainWindow.xaml.cs | +105/-40 | 适配调整 |
| Themes/DesignTokens.xaml | +111/-0 | 新增暗色主题设计令牌（背景/表面/控件/边框/文字/强调色等） |
| App.xaml | +31/-22 | 合并主题资源字典 |

### 影响范围
- **API**: App 资源合并结构变化——样式通过 DynamicResource 引用 DesignTokens，合并顺序不可随意调整
- **跨模块**: 同提交新增 docs/ 初版文档树（README、architecture、modules、guides）并修改 .claude/settings.json（+7 行）
- **数据模型**: 无
- **配置**: 无

---

## [2026-07-31] feat: add page navigation system and new controls

**类型**: feat
**风险**: HIGH

> 本条与下一条为 commit `ced3254`（见上方带提交号条目）部分内容的早期记录，数字已按 ced3254 的 git numstat 统一。

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/CrosshairPage.xaml.cs | +340/-0 | 新增准心配置页面 |
| Views/GamesPage.xaml.cs | +24/-0 | 新增游戏配置页面 |
| ViewModels/CrosshairViewModel.cs | +337/-0 | 新增准心配置视图模型 |
| ViewModels/GamesViewModel.cs | +129/-0 | 新增游戏配置视图模型 |
| ViewModels/MainViewModel.cs | +59/-269 | 改为页面导航架构 |
| Controls/TabNavItem.cs | +142/-0 | 新增标签页导航项控件 |
| Controls/PageTemplateSelector.cs | +33/-0 | 新增页面模板选择器 |
| Controls/IconButton.cs | +222/-0 | 新增图标按钮控件 |
| Helpers/ThemeHelper.cs | +146/-0 | 新增主题资源访问助手 |
| StringToVisibilityConverter.cs | +27/-0 | 新增字符串到可见性转换器 |
| Themes/IconGeometries.xaml | +148/-0 | 新增图标几何资源 |

### 影响范围
- **架构**: 从单 ViewModel 改为页面导航架构
- **页面**: 新增 CrosshairPage、GamesPage 两个页面
- **控件**: 新增 TabNavItem、PageTemplateSelector、IconButton 等控件
- **资源**: 新增 IconGeometries.xaml 图标资源文件

### 新增功能
- `CrosshairPage`: 准心配置页面，左侧预览 + 右侧控制面板
- `GamesPage`: 游戏配置页面，管理游戏特定配置
- `CrosshairViewModel`: 准心配置逻辑，从 MainViewModel 拆分
- `GamesViewModel`: 游戏配置逻辑，支持多游戏配置
- `TabNavItem`: 左侧导航图标项，支持选中态
- `PageTemplateSelector`: 根据 PageType 选择页面模板
- `StringToVisibilityConverter`: 字符串到可见性转换
- `IconGeometries.xaml`: Material Design 风格图标集合

### 架构变更
- `MainViewModel` 改为持有子 ViewModel，不再直接管理配置
- 新增 `PageType` 枚举（Crosshair、Games）
- 页面渲染逻辑从 MainWindow 移到 CrosshairPage

---

## [2026-07-31] feat: add custom controls and theme system

**类型**: feat
**风险**: HIGH

> 本条与上一条为 commit `ced3254`（见上方带提交号条目）部分内容的早期记录，数字已按 ced3254 的 git numstat 统一。

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Controls/DialogBase.cs | +257/-0 | 新增对话框基类 |
| Controls/IconButton.cs | +222/-0 | 新增图标按钮控件 |
| Controls/ToastManager.cs | +238/-0 | 新增 Toast 管理器（独立悬浮窗口） |
| Controls/ToastNotification.cs | +115/-0 | 新增 Toast 通知控件 |
| Controls/CrosshairPreview.cs | +5/-3 | 适配调整（控件自初始提交已存在，非本提交新增） |
| Helpers/ThemeHelper.cs | +146/-0 | 新增主题资源访问助手 |
| Themes/DesignTokens.xaml | +33/-0 | 设计令牌补充（颜色、字体、间距） |
| Themes/ControlStyles.xaml | +485/-1 | 新增控件样式定义 |
| Themes/IconGeometries.xaml | +148/-0 | 新增图标几何数据 |

### 影响范围
- **控件**: 新增多个自定义控件，统一 UI 风格
- **主题**: 引入设计令牌系统，支持主题一致性
- **样式**: 所有控件使用动画过渡，提升用户体验

### 新增功能
- `DialogBase`: 统一对话框样式，支持输入对话框
- `IconButton`: 支持内置图标和自定义 Path Geometry
- `ToastManager`: 独立悬浮窗口显示 Toast，不影响布局
- `ToastNotification`: 自动消失的提示控件
- `CrosshairPreview`: 可复用的准心预览控件（本提交为其做适配，控件自初始提交已存在）
- `ThemeHelper`: 类型安全的主题资源访问

---

## [2026-07-03] refactor: restructure project architecture

**类型**: refactor
**提交**: 6fad297
**风险**: HIGH

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | +75/-9 | 简化代码，使用事件驱动 |
| ViewModels/MainViewModel.cs | +133/-86 | 异步初始化，事件驱动 |
| App.xaml.cs | +24/-4 | OnStartup 重写：构建 DI 容器并显示主窗口，移除模板代码 |
| App.xaml | +1/-2 | 移除 StartupUri（主窗口改由 DI 容器手动显示） |
| MainWindow.xaml | +2/-4 | 移除 XAML DataContext 声明（改由 DI 注入），根网格命名 RootGrid |
| CrosshairPro.App.csproj | +1/-0 | 新增对 CrosshairPro.Application 的项目引用 |

### 影响范围
- **架构**: App 层只依赖 Application 接口，不直接依赖 Services
- **初始化**: ViewModel 异步加载预设，恢复上次状态
- **事件**: 使用事件驱动模式，解耦 MainWindow 和 ViewModel
- **跨模块**: 同提交在 Application 新建 IConfigurationService/IPresetService/ConfigurationService/PresetService 与 DI 注册，Core 新增 AppPersistedState 并扩展 IConfigRepository 状态方法，Services 的 JsonConfigRepository 适配新接口

### 回滚指南
- 回滚: `git revert 6fad297`
- 检查文件: MainWindow.xaml.cs, MainViewModel.cs
- 副作用: 依赖新的 Application 层接口

---

## [2026-06-08] feat: major UI and ViewModel refactoring

**类型**: feat
**提交**: 8d57814
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | +422/-0 | 主窗口重构，添加自定义对话框 |
| MainWindow.xaml | +106/-148 | 优化布局，添加控制面板 |
| ViewModels/MainViewModel.cs | +198/-79 | ViewModel 重构，添加命令和事件 |
| Themes/ControlStyles.xaml | +73/-98 | 控件样式调整 |
| Core/Models/Preset.cs | +2/-0 | （跨模块，同提交）重写 ToString() 返回预设名 |

### 影响范围
- **UI**: 自定义颜色选择器、预设管理对话框
- **命令**: 添加所有操作命令（保存、导入、导出、删除）
- **托盘**: 系统托盘菜单优化

---

## [2026-06-08] feat: enhance overlay and crosshair config

**类型**: feat
**提交**: 64f03ed
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/OverlayWindow.cs | +18/-2 | 渲染时按 Brightness 调整准心颜色明暗（新增 ApplyBrightness） |
| MainWindow.xaml.cs | +17/-2 | 预览渲染同步应用亮度（新增 ApplyBrightness） |
| MainWindow.xaml | +17/-0 | 新增亮度滑块 UI（BRIGHTNESS 区块：Slider + 数值框） |
| Core/Models/CrosshairConfig.cs | +1/-0 | （跨模块，同提交）CopyFrom 纳入 Brightness 字段 |

### 影响范围
- **功能**: 新增亮度调节（滑块 UI + ApplyBrightness，Overlay/预览同步应用亮度）
- **数据模型**: CrosshairConfig.Brightness 参与克隆（CopyFrom）

---

## [2026-06-07] feat: initial WPF application

**类型**: feat
**提交**: e649a4f
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| App.xaml/cs | +33 | 应用程序入口 |
| MainWindow.xaml/cs | + | 主窗口 |
| OverlayWindow.cs | + | 准心叠加窗口 |
| MainViewModel.cs | + | 主视图模型 |

### 影响范围
- **初始化**: 项目初始化，WPF MVVM 架构
- **依赖注入**: 使用 DI 容器管理服务
- **热键**: 注册全局热键 Ctrl+Shift+X
