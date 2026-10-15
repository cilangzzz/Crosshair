# CrosshairPro.App - 数据模型

本文档描述 App 层的 ViewModel、视图、控件、转换器、本地化与主题数据结构。
共享模型（CrosshairConfig、GameProfile、GameConfig、ConfigItemDefinition 等）定义在 Core 模块，见 [../Core/data-model.md](../Core/data-model.md)；服务接口见 [../Application/data-model.md](../Application/data-model.md)。

## MainViewModel

主 ViewModel，管理页面导航并转发子 ViewModel 的事件。

```csharp
public partial class MainViewModel : ObservableObject
{
    public CrosshairViewModel CrosshairViewModel { get; }
    public GamesViewModel GamesViewModel { get; }

    [ObservableProperty] private PageType _currentPage = PageType.Crosshair;

    public bool IsCrosshairPage => CurrentPage == PageType.Crosshair;
    public bool IsGamesPage => CurrentPage == PageType.Games;
}
```

### PageType 枚举

```csharp
public enum PageType
{
    Crosshair,  // 准心配置页面
    Games       // 游戏配置页面
}
```

### 事件

| 事件 | 说明 |
|------|------|
| ConfigUpdated | 配置更新（转发自 CrosshairViewModel） |
| ToggleCrosshairRequested | 切换准心显示请求 |
| SelectImageRequested | 选择自定义图片请求 |
| SavePresetRequested | 保存预设请求 |
| ImportPresetRequested | 导入预设请求 |
| ExportPresetRequested | 导出预设请求 |
| ToastRequested(string) | Toast 通知请求 |

### 命令与业务方法

| 成员 | 说明 |
|------|------|
| NavigateToCrosshairCommand | 导航到准心页面（更新 CurrentPage 并通知 Is*Page） |
| NavigateToGamesCommand | 导航到游戏页面 |
| SavePresetWithNameAsync(name) | 转发到 CrosshairViewModel |
| ImportPresetFromFileAsync(filePath) | 转发到 CrosshairViewModel |
| ExportPresetToFileAsync(filePath) | 转发到 CrosshairViewModel |

构造函数中订阅 CrosshairViewModel 的全部事件并逐一转发给 MainWindow。

## CrosshairViewModel

准心配置页 ViewModel，管理准心配置、预设、状态持久化。

```csharp
public partial class CrosshairViewModel : ObservableObject
{
    [ObservableProperty] private CrosshairConfig _config;
    [ObservableProperty] private bool _isCrosshairVisible = true;
    [ObservableProperty] private string _statusMessage;    // 初始值取 "Status.CrosshairEnabled"
    [ObservableProperty] private string _currentPresetName; // 初始值取 "Preset.Default"
    [ObservableProperty] private int _selectedStyleIndex;
    [ObservableProperty] private List<Preset> _presets = new();
    [ObservableProperty] private Preset? _selectedPreset;
}
```

### 只读属性

| 属性 | 类型 | 说明 |
|------|------|------|
| CrosshairStyleNames | string[] | 6 个样式名（Cross/Dot/Circle/TShape/XShape/CustomImage，构造时按当前语言取词） |
| PresetColors | string[] | 8 个预设颜色（#00FF00、#00FFFF、#FFFF00、#FF0000、#FF00FF、#FFA500、#FFFFFF、#000000） |

### 内部状态

| 字段 | 说明 |
|------|------|
| _currentPresetId | 当前使用的预设 ID（持久化到 appstate） |
| _isInitializing | 初始化标记，避免恢复预设时重复保存状态 |

### 事件

ConfigUpdated、ToggleCrosshairRequested、SelectImageRequested、SavePresetRequested、ImportPresetRequested、ExportPresetRequested、ToastRequested（同 MainViewModel，见上表）。

### 命令

| 命令 | 说明 |
|------|------|
| SetColorCommand(color) | 设置准心颜色 |
| SelectImageCommand | 触发 SelectImageRequested（MainWindow 弹文件对话框） |
| ToggleCrosshairCommand | 切换 IsCrosshairVisible 并更新 StatusMessage |
| ResetConfigCommand | 恢复默认配置（CreateDefaultConfig + CopyConfig），重置样式索引与预设 ID |
| SavePresetCommand / ImportPresetCommand / ExportPresetCommand | 仅触发对应 Requested 事件，对话框由 MainWindow 处理 |
| DeletePresetCommand(preset) | 删除预设（IsDefault = true 时直接返回） |

### 业务方法

| 方法 | 说明 |
|------|------|
| SavePresetWithNameAsync(name) | 用 CloneConfig 深拷贝当前配置创建 Preset，重名时自动在末尾追加数字序号（`x`→`x (1)`→`x (2)`，自带序号则续起），保存并刷新列表，持久化当前预设 ID |
| ImportPresetFromFileAsync(filePath) | ImportPresetAsync 导入后刷新列表并选中 |
| ExportPresetToFileAsync(filePath) | 将当前配置（深拷贝）导出为预设文件 |
| InitializeAsync（私有） | 恢复上次预设 ID → LoadPresets → 选中目标预设 → 触发 ConfigUpdated |
| SubscribeConfigEvents（私有） | 递归订阅 Config / Effects / Outline / Shadow 的 PropertyChanged，触发 ConfigUpdated |

`OnSelectedStyleIndexChanged` 将索引映射为 `CrosshairStyle` 枚举（带边界检查）；`OnSelectedPresetChanged` 复制预设配置、更新名称与 ID，非初始化状态下异步保存状态。

## GamesViewModel

游戏配置页 ViewModel，仅负责内置游戏列表与选中管理（配置读写下沉到 Apex/Cs2 两个 ViewModel）。

```csharp
public partial class GamesViewModel : ObservableObject
{
    public ObservableCollection<GameProfile> Games { get; }

    [ObservableProperty] private string? _currentGameId;
    [ObservableProperty] private GameProfile? _selectedGame;
}
```

| 成员 | 说明 |
|------|------|
| Games | 来自 `GameProfile.BuiltIn.GetAll()` 的 3 个内置条目（Counter-Strike 2、Apex Legends、电脑优化），见 [../Core/data-model.md](../Core/data-model.md) |
| CurrentGameId | 当前选中游戏 ID（GamesPage 的 ContentControl 绑定它驱动 GamePageSelector） |
| SelectedGame | 当前选中的游戏（GamesPage ComboBox 的 SelectedItem 双向绑定），变更时同步 IsSelected 标记与 CurrentGameId |
| SelectGameInternal(game) | private 内部选中方法；构造时默认选中 `builtin-apex` |

## ApexConfigViewModel

Apex Legends 配置 ViewModel（单例），管理 videoconfig.txt、settings.cfg 与启动项，依赖 `IApexConfigService`（见 [../Application/README.md](../Application/README.md)）。videoconfig.txt / settings.cfg 的字段级协议与读写规则见 [../GameConfig/Apex/README.md](../GameConfig/Apex/README.md)。

### 配置数据属性

| 属性 | 类型 | 说明 |
|------|------|------|
| VideoConfig | ApexVideoConfig | 视频配置（分辨率/刷新率/Gamma/画质/垂直同步/抗锯齿等，模型在 Core） |
| SettingsConfig | ApexSettingsConfig | 游戏设置（鼠标灵敏度/FOV/FPS/倍镜灵敏度等，模型在 Core） |
| LaunchOptions | string | 启动项文本 |
| CustomLaunchOptions | ApexLaunchOptions | 自定义启动项结构化选项（高性能/窗口/游戏/网络/调试分组） |
| UseCustomLaunchOptions | bool | 是否使用自定义启动项（选择"自定义"预设时为 true） |
| _isLoadingOptionsFromString | bool | 内部标志：把启动项字符串解析回填面板（选预设 / 切入自定义 / 重置 / 配置加载完成）期间抑制面板→字符串的再生成；钩子置预设回显索引时也短暂置位（面板即源头，避免回读解析） |
| IsApexInstalled | bool | Apex 是否检测到安装 |
| VideoConfigPath / SettingsConfigPath | string | 配置文件路径（未找到时显示 "Path.NotFound" 文案） |
| StatusMessage | string | 状态消息 |

### 枚举选项集合（构造时按当前语言生成）

ResolutionOptions（13 档分辨率）、TextureQualityOptions（最高/-1～低/2）、WindowModeOptions（全屏/窗口/无边框）、ParticleLevelOptions、SsaoQualityOptions、VsyncOptions（关/开/三重缓冲）、AntialiasOptions（关/FXAA/TXAA/MSAA 2x/4x）、LaunchOptionPresets（竞技优化/高画质/调试模式/自定义）。

### 备份历史

| 属性 | 说明 |
|------|------|
| VideoConfigBackups / SettingsConfigBackups | ObservableCollection<BackupFileInfo>（BackupFileInfo 定义在 Application 的 IApexConfigService.cs） |
| ShowVideoConfigHistory / ShowSettingsConfigHistory | 历史面板展开标记 |

### 命令

| 命令 | 说明 |
|------|------|
| ReplaceVideoConfigCommand / ReplaceSettingsConfigCommand | OpenFileDialog 选择文件后替换目标配置 |
| BackupVideoConfigCommand / BackupSettingsConfigCommand | 创建备份 |
| ExportVideoConfigCommand / ExportSettingsConfigCommand | SaveFileDialog 导出 |
| ToggleVideoConfigHistoryCommand / ToggleSettingsConfigHistoryCommand | 展开/收起历史并加载备份列表 |
| RestoreVideoConfigBackupCommand / RestoreSettingsConfigBackupCommand(backup) | 从备份恢复并刷新 |
| DeleteBackupCommand(backup) | 删除备份文件 |
| CopyLaunchOptionsCommand | 复制当前生效的启动项到剪贴板 |
| GenerateLaunchOptionsFileCommand | SaveFileDialog 将当前启动项导出为文本文件（"配置管理"卡内） |
| ApplyPresetCommand(name) | 应用竞技优化 / 高画质 / 默认预设（VideoConfig 整体替换） |
| SaveCommand | 保存 VideoConfig + SettingsConfig + LaunchOptions（未安装时 Toast 提示） |
| ResetCommand | 恢复默认并立即保存 |

`OnSelectedLaunchPresetIndexChanged`：索引 0/1/2 分别填充竞技、高画质、调试启动项字符串；索引 3（自定义）置 UseCustomLaunchOptions = true。

事件：`ToastRequested(string)`，由 ApexPage 订阅后用 ToastNotification.ShowIn 渲染。

## Cs2ConfigViewModel

Counter-Strike 2 配置 ViewModel（单例），基于 `IGameConfigService` 的 `builtin-cs2` 策略渲染可配置项。CrosshairPro 只保存配置到本地，不直接写入 video.txt / autoexec.cfg（避免与 VAC、Steam 云同步冲突）。CS2 配置协议与策略定义详见 [../GameConfig/CS2/README.md](../GameConfig/CS2/README.md)。

```csharp
public partial class Cs2ConfigViewModel : ObservableObject
{
    public ObservableCollection<ConfigSectionWrapper> Sections { get; } = new();

    [ObservableProperty] private string _launchOptions = string.Empty;
    [ObservableProperty] private int _launchOptionsLength;          // 联动 LaunchOptionsSummary
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isCs2Running;                // 进程 "cs2" 检测
    [ObservableProperty] private string _configFilePath;            // 本地配置 builtin-cs2.json 路径
}
```

| 成员 | 说明 |
|------|------|
| LaunchOptionsSummary | 格式化字符数提示（"Cs2.LaunchOptionsChars"） |
| LoadAsync() | 取策略 → 取配置 → 按 strategy.Sections 构建 Sections 包装 → 恢复启动项 → 检测进程 |
| SaveCommand | 新建/取回配置，写回 LaunchOptions 与 Sections 内非 null 值，SaveConfigAsync 持久化 |
| ResetCommand | ResetToDefaultAsync 后重新 LoadAsync |
| CopyLaunchOptionsCommand | 启动项复制到剪贴板 |
| OpenConfigFolderCommand | explorer /select 定位本地配置文件 |
| GenerateConfigFileCommand | SaveFileDialog 生成 exec 用的 .cfg（fps_max/cl_showfps cvar + 启动项/视频设置注释），UTF-8 无 BOM；值取自当前 Sections（含未保存修改） |
| RefreshRunningStateCommand | 重新检测 cs2 进程 |
| ToastRequested | 事件，由 Cs2Page 订阅（Loaded 订阅 / Unloaded 退订） |

Cs2Page.xaml 通过 `ConfigItemTemplateSelector`（按 `ConfigItemType` 选 开关/滑杆/下拉 统一行模板）渲染 `Sections[x].Items`，不再使用 `Items[j]` 固定索引绑定（坑点 §17 已消除）；分区标题仍为固定本地化键。

## PcOptimizeViewModel

电脑优化 ViewModel（单例），纯静态内容 + 打开命令：7 分类 29 项 Windows 游戏优化条目，每项只提供对应系统管理页的「打开」入口，**不修改任何系统设置**（设计决策、29 项清单与命令核验记录见 [../design/pc-optimize-design.md](../design/pc-optimize-design.md)）。无持久化、无服务依赖。

```csharp
public partial class PcOptimizeViewModel : ObservableObject
{
    public ObservableCollection<PcOptimizeCategory> Categories { get; } = new();
    public event EventHandler<string>? ToastRequested;
    [RelayCommand] private void OpenItem(PcOptimizeItem? item);
}

public class PcOptimizeCategory
{
    public string TitleKey { get; }                          // PcOptimize.Cat.* 分组标题键
    public ObservableCollection<PcOptimizeItem> Items { get; }
}

public class PcOptimizeItem
{
    public string Id { get; }                                // 条目标识（本地化键后缀）
    public string TitleKey => $"PcOptimize.{Id}";            // 标题本地化键
    public string DescKey => $"PcOptimize.{Id}Desc";         // 说明本地化键
    public string GainKey { get; }                           // PcOptimize.GainHigh/Medium/Low/Varies
    public string OpenTarget { get; }                        // ms-settings URI / .cpl / .exe
    public string OpenArguments { get; }                     // 带参 cpl 经 control 拆分（ShellExecute 不拆空格）
    public string[] ProbePaths { get; } = [];                // 厂商面板全路径候选（NVIDIA/AMD）
    public bool HasEntry => !string.IsNullOrEmpty(OpenTarget) || ProbePaths.Length > 0;
}
```

| 成员 | 说明 |
|------|------|
| Categories | 构造函数静态构建 7 组：系统 8 / 电源 4 / 显示 6 / 网络 3 / 存储 3 / 驱动与显卡 3 / 输入与音频 2 |
| OpenItemCommand | OpenTarget 为 ms-settings URI 或 exe → `Process.Start(UseShellExecute=true)`（Arguments 非空时附加）；ProbePaths 非空 → `FirstOrDefault(File.Exists)` 探测（未命中 → Toast `PcOptimize.VendorNotFound`）；异常 → Toast `PcOptimize.OpenFailed` |
| ToastRequested | 事件，由 PcOptimizePage 订阅（Loaded 订阅 / Unloaded 退订） |

PcOptimizePage.xaml 条目行经 `LocalizedTextConverter` 双绑定（TitleKey/DescKey + CurrentCulture）渲染，语言切换即时重算；`HasEntry` 控制按钮可见性。

## ConfigItemWrapper / ConfigSectionWrapper

配置项 UI 包装类（定义于 ViewModels/ConfigItemWrapper.cs），把 Core 的 `ConfigItemDefinition` / `ConfigSectionDefinition` 与实际设置字典连接起来供绑定。

```csharp
public partial class ConfigItemWrapper : ObservableObject
{
    public ConfigItemDefinition Definition { get; }  // 元数据（Key/DisplayName/Type/Min/Max/Options…）
    [ObservableProperty] private object? _value;     // OnValueChanged 写回 _settings[Key]
    // DisplayName：项 Key 命中 LocKeyByItemKey 映射（fullscreen/resolution/aspect_ratio/
    // refresh_rate/fps_max/cl_showfps → Cs2.* 键）时取本地化文本，否则回退策略 DisplayName
}

public class ConfigSectionWrapper
{
    public string DisplayName { get; }
    public List<ConfigItemWrapper> Items { get; }
}
```

构造时从 `Dictionary<string, object?> settings` 取当前值，缺省用 `DefaultValue`；Value 变更即写回字典，Save 时统一收集。`DisplayName` 走本地化映射的原因：策略层 `DisplayName` 为硬编码中文，直接绑定会使英文界面回退中文。ConfigItemType 枚举见 [../Core/data-model.md](../Core/data-model.md)。

## OverlayWindow

准心叠加窗口（`sealed class OverlayWindow : Window`），全屏透明置顶，WPF Shape 渲染。

### 字段与事件

```csharp
private readonly Canvas _canvas;            // 渲染画布
private readonly CrosshairConfig _config;   // 配置副本（独立实例）
private bool _isVisible = true;             // 可见性状态
private IntPtr _hwnd;                       // 窗口句柄

public event EventHandler? CrosshairVisibilityChanged;
public bool IsCrosshairVisible => _isVisible;
```

### 关键行为

| 成员 | 说明 |
|------|------|
| 构造 | WindowStyle.None、AllowsTransparency、Topmost、Focusable=false、尺寸=主屏；SourceInitialized 时加 `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW`（不加 WS_EX_LAYERED） |
| UpdateConfig(config) | `_config.CopyFrom(config)` 同步副本；内部订阅自身配置四级 PropertyChanged 自动重绘 |
| ToggleVisibility() | 切换 Visibility 并触发 CrosshairVisibilityChanged |
| RenderCrosshair() | 按样式绘制：Cross 四线 / Dot 圆点 / Circle 圆环 / TShape 倒 T（竖线朝下）/ XShape 四对角线 / CustomImage 加载图片（缺失或失败回退十字，加载失败另回退红色小十字）；`Config.Rotation != 0` 时对 `_canvas` 施加 `RenderTransform(RotateTransform, 窗口中心)`，0° 置 null（CrosshairPage 预览同型实现） |
| AddLine/AddDot/AddCircle/AddImage | 添加形状；描边通过先画黑色加粗底层实现；每个 Shape 单独设置 Opacity |
| ApplyBrightness(color, brightness) | 亮度系数 100=原色、0=全黑、200=全白 |

## 页面视图

### CrosshairPage

准心配置页（UserControl）：

- 左侧 PreviewCard：`GridCanvas`（7x7 网格）+ `CrosshairCanvas`（准心），网格在下层
- 右侧控制面板：样式 ComboBox（SelectedStyleIndex）、自定义图片按钮（SelectedStyleIndex==5 时可见）、大小/间隙/粗细/透明度/亮度/旋转（0-360，绑定既有 `Config.Rotation`，此前模型已支持但 UI 未暴露）/中心点滑块+数值框、预设颜色 RadioButton（GroupName="CrosshairColor"）+ 彩虹渐变自定义色按钮、效果区平铺三行（启用描边/启用阴影为标签居左 + `ToggleSwitch` 矩形微圆角开关居右，描边粗细为与开关行同缩进的标签+NumericTextBox 行（数值框 `Margin="8,0,0,0"` 与其他滑杆行同规格），无 Expander）
- `SetViewModel(vm)` / `OnLoaded`：从 `Window.GetWindow(this).DataContext`（MainViewModel）取 CrosshairViewModel 并订阅 ConfigUpdated
- `CustomColor_Click`：使用 `System.Windows.Forms.ColorDialog` 选色，直接写 `Config.Color`

### GamesPage

游戏配置页：顶部「游戏」标签（`Games.SelectorLabel`）+ ComboBox 选择器（`SelectedItem` 绑定 SelectedGame，Width 200，ItemTemplate 经 GameProfileTitleConverter 本地化；下拉项与折叠态选区框共用同一模板）；内容区 ContentControl 绑定 `CurrentGameId`，由 GamePageSelector 切换模板。Loaded 时从主窗口 DataContext 取 GamesViewModel。`GameProfile.IsSelected` 标志仍由 ViewModel 维护但 UI 不再消费（原 tab 高亮已随 tab 条移除）。

### ApexPage / Cs2Page / PcOptimizePage

游戏子页面，均从 `App.Services` 解析各自 ViewModel（单例）：

| 页面 | 加载行为 | Toast 处理 |
|------|------|------|
| ApexPage | OnLoaded 设置 DataContext 并订阅 ToastRequested（无退订） | ToastNotification.ShowIn(Panel) |
| Cs2Page | OnLoaded 订阅 + await LoadAsync()；OnUnloaded 退订 | 同上 |
| PcOptimizePage | OnLoaded 解析 PcOptimizeViewModel、订阅 ToastRequested；OnUnloaded 退订（无加载步骤） | 同上 |

### 模板选择器（GamePageSelector / PageTemplateSelector / ConfigItemTemplateSelector）

三个 DataTemplateSelector：

```csharp
public class GamePageSelector : DataTemplateSelector
{
    public DataTemplate? ApexTemplate { get; set; }     // gameId == "builtin-apex"
    public DataTemplate? Cs2Template { get; set; }      // gameId == "builtin-cs2"
    public DataTemplate? PcOptimizeTemplate { get; set; } // gameId == "builtin-pcopt"（电脑优化页）
    public DataTemplate? DefaultTemplate { get; set; }  // 其他（"请选择游戏"提示）
}

public class PageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CrosshairTemplate { get; set; }  // PageType.Crosshair
    public DataTemplate? GamesTemplate { get; set; }      // PageType.Games
}

public class ConfigItemTemplateSelector : DataTemplateSelector  // Views/GamePages/
{
    public DataTemplate? BoolTemplate { get; set; }     // ConfigItemType.Bool → 开关行
    public DataTemplate? IntTemplate { get; set; }      // ConfigItemType.Int → 滑杆+数值框行
    public DataTemplate? EnumTemplate { get; set; }     // ConfigItemType.Enum → 下拉行
    public DataTemplate? StringTemplate { get; set; }   // ConfigItemType.String → 文本行
}
```

Cs2Page 资源中声明 `ConfigItemTemplateSelector` 并为三/四类模板提供统一行规格（开关行「标签居左 + `ToggleSwitch` 矩形微圆角开关居右」、滑杆行 `*/160/64` 三列、下拉行 ComboBox 宽 160），ApexPage 手写行遵循同一规格。ApexPage 启动选项卡自上而下：预设 → 启动项字符串（始终可见；`UseCustomLaunchOptions`（含义：面板内容已偏离预设/处于自定义态）为 true 时 IsReadOnly 只读）→ 启动参数面板（**常显**：性能/窗口/游戏参数/网络/调试，游戏参数组含 FOV/最大FPS/显示FPS/显示位置，数值 0 = 不输出对应令牌）→ 复制按钮 →「游戏设置」小节（settings.cfg：仅剩鼠标灵敏度/鼠标加速）；视频设置卡的分辨率/刷新率行已移除（对应 -w/-h/-freq 走启动参数通道，videoconfig.txt 数据链路不变）。启动项字符串与启动参数面板双向联动：面板编辑 → `CustomLaunchOptions.PropertyChanged` → `LaunchOptions` 实时重生成，且回显偏离预设时 `SelectedLaunchPresetIndex` 自动切 3「自定义」（`_isLoadingOptionsFromString` + `_isInitializing` 双重防回环/防初始化误切）；选预设/切入自定义/重置/配置加载完成 → `ApexLaunchOptions.LoadFromString(LaunchOptions)` 先重置后解析回填面板（`SeedPanelFromLaunchOptions()` 助手；`LaunchOptionsSummary` 已删除，复制/生成命令直接读 `LaunchOptions`）。瞄准镜灵敏度独立成卡；父子联动（级联阴影←启用阴影、体积雾←体积光照、倍镜滑杆←独立倍镜灵敏度开关）经 `x:Name` + `ElementName` 绑定 IsChecked→IsEnabled 实现，子项与父项同列无缩进；布尔子项另有值联动——父开关经 UI 点击（Click 事件，绑定更新不触发）关闭时 `CsmEnabled`/`VolumetricFog` 同步置 0（ApexPage.xaml.cs 处理器），重新开启父项不自动恢复。

## 自定义控件

### CrosshairPreview

`Control` 子类，`Config` 依赖属性 + `OnRender` 绘制网格与各样式（预览尺寸放大 1.5 倍）。**注意：当前所有 XAML 均未引用该控件**——CrosshairPage 使用自己的双 Canvas 渲染预览。保留为可复用控件，但属于未接线代码。

### TabNavItem

左侧导航图标项（`Button` 子类）：

```csharp
public string IconGeometry { get; set; }  // 图标资源键名（自动补 "IconGeometry" 后缀查找）
public bool IsSelected { get; set; }      // 选中态
```

模板部件：`IconPath`（Path）、`Indicator`（左侧指示条 Border）、`BackgroundBorder`。选中时指示条用 AccentBrush、背景用 ControlBrush，图标填充 AccentBrush，否则 TextSecondaryBrush（指示条/背景透明）。

### IconButton

图标按钮（`Button` 子类）依赖属性：`IconGeometry`（内置图标名，自动补 "IconGeometry" 后缀查找资源；传入原始 Path Data 字符串时资源查找必然失败、几何解析被跳过——该回退分支在当前 UpdateIconGeometry 实现中不可达）、`IconSize`（默认 16）、`IconColor`（Color?，默认绑定 Foreground）、`IconPosition`（IconPosition 枚举：Left/Right/Top/Bottom）、`IconSpacing`（默认 8）、`ShowIconOnly`。

### DialogBase

对话框基类（`Window` 子类），无边框透明、可拖动：

```csharp
public string TitleText;        public bool ShowCloseButton;
public bool ShowButtons;        public string ConfirmButtonText;
public string CancelButtonText; public object DialogContent;

public event RoutedEventHandler? Confirmed;
public event RoutedEventHandler? Cancelled;
public bool? ShowDialog(Window owner);
public static DialogBase CreateInputDialog(string title, string prompt, string defaultValue = "");
public string? GetInputResult();  // 从 Tag 中取输入框文本
```

模板部件：`CloseButton`、`ConfirmButton`、`CancelButton`、`ButtonPanel`（ShowButtons=false 时隐藏）。Enter 确认 / Escape 取消。

### ToastNotification / ToastManager

| 控件 | 形态 | 关键 API |
|------|------|------|
| ToastNotification | Control，加入指定 Panel 内 | `Show(message, duration=3)`、`Hide()`、`static ShowIn(Panel, message, duration=3)`；DispatcherTimer 定时后自动从容器移除 |
| ToastManager | 独立置顶无边框悬浮窗口 | `static Show(message, duration=3, owner=null)`、`static CloseAll()`；200ms 淡入/淡出动画；定位优先取 owner 底部居中，否则屏幕底部居中，并做屏幕边界钳制 |

### SmoothScrollViewer / ScrollViewerBehavior

| 类型 | 说明 |
|------|------|
| SmoothScrollViewer | ScrollViewer 子类；依赖属性 `ScrollDuration`（250ms）、`ScrollStep`（120px）、`EasingMode`（EaseOut）、`IsSmoothScrollEnabled`；OnMouseWheel 拦截后用 CompositionTarget.Rendering + CubicEase 驱动平滑滚动，动画期间新滚轮事件在原目标偏移上累加（连续滚动速度叠加） |
| ScrollViewerBehavior | 附加属性 `VerticalOffset`，变更时调用 `ScrollToVerticalOffset`（供样式内动画驱动滚动位置） |

## 值转换器

均在 App 程序集根命名空间，注册于 App.xaml 资源：

| 转换器 | 资源键 | 规则 |
|------|------|------|
| StringToVisibilityConverter | StringToVisibilityConverter | 非空字符串 → Visible，否则 Collapsed |
| ConfigTypeToVisibilityConverter | ConfigTypeToVisibilityConverter | value 为 ConfigItemType 且 parameter（字符串）解析后相等 → Visible |
| IntToBoolConverter | IntToBoolConverter | 0 → false，非 0 → true；ConvertBack：true → 1 |
| UtcToLocalTimeConverter | UtcToLocalTimeConverter | UTC DateTime → 本地时间字符串（yyyy-MM-dd HH:mm） |
| GameProfileTitleConverter | GameProfileTitleConverter | IMultiValueConverter：[0]=DisplayName、[1]=DisplayNameLocKey、[2]=CurrentCulture（语言切换触发重算）；LocKey 非空取 LocalizationProvider.Get，否则回退 DisplayName |
| LocalizedTextConverter | LocalizedTextConverter | IMultiValueConverter：[0]=locKey、[1]=CurrentCulture（语言切换触发重算）；取 LocalizationProvider.Get(locKey)，键缺失回退键名 |

另使用 WPF 内置 `BooleanToVisibilityConverter`（键 BoolToVisibilityConverter）。

## 本地化架构

```
{loc:Loc Key=App.Title}                        ← XAML 标记扩展
  └─ LocExtension.ProvideValue()
       └─ Binding("[App.Title]") → LocalizationProvider.Instance（索引器）
             └─ Strings.ResourceManager.GetString(key, culture)
                   └─ Strings.resx / Strings.en.resx / Strings.zh-CN.resx（各 297 键）
语言切换
  MainWindow.LanguageButton_Click → LocalizationProvider.SetCulture("en-US"/"zh-CN")
    → CurrentCulture setter 触发 PropertyChanged("Item[]") → 所有索引器 Binding 自动刷新
    → SaveLanguagePreferenceAsync 持久化 state.Language（IAppStateRepository）
启动恢复
  App.OnStartup Initialize()（默认 zh-CN）→ MainWindow.Loaded 后读取 state.Language 重新 Initialize
```

| 类型 | 职责 |
|------|------|
| LocalizationProvider | 懒加载单例，INotifyPropertyChanged；索引器 `this[key]`、静态 `Get(key)`、`GetFormatted(key, args)`（string.Format，失败回退原文）、`SetCulture(name)`、`Initialize(savedLanguage)`、`SupportedCultures`（zh-CN、en-US） |
| LocExtension | MarkupExtension，支持 `{loc:Loc Key=X}` 与 `{loc:Loc X}`；通过 Binding 索引器路径 `[{Key}]` 实现动态语言切换 |
| Strings | `ResourceManager`（基名 `CrosshairPro.App.Localization.Strings`）包装：`Get(key)`、`Get(key, culture)`、`GetFormatted(key, args)` |

## ThemeHelper

静态主题资源访问助手（Helpers/ThemeHelper.cs），从 Application.Resources 按键取值：

| 分类 | 成员 |
|------|------|
| 颜色 | BackgroundColor、SurfaceColor、ControlColor、BorderColor、AccentColor、ErrorColor、ShadowColor、GridLineColor、GridCenterLineColor（另有 GetColor(key)） |
| 画刷 | BackgroundBrush、SurfaceBrush、ControlBrush、ControlHoverBrush、BorderBrush、TextPrimaryBrush、TextSecondaryBrush、AccentBrush、ErrorBrush、GridLineBrush、GridCenterLineBrush（另有 GetBrush(key)） |
| 效果 | DialogShadowEffect、ToastShadowEffect |
| 字体 | FontFamilyPrimary、FontFamilyMono、FontSizeHeading、FontSizeBody、FontSizeCaption |
| 圆角 | RadiusLg、RadiusMd、RadiusSm |

## 设计令牌（Themes/DesignTokens.xaml）

### 颜色系统

| 令牌 | 值 | 用途 |
|------|------|------|
| BackgroundColor | #0D0D1A | 主背景 |
| SurfaceColor | #161628 | 卡片背景 |
| ControlColor | #1E1E36 | 控件背景 |
| ControlHoverColor | #262645 | 控件悬停 |
| ControlPressedColor | #1A1A30 | 控件按下 |
| BorderColor | #2A2A4A | 边框 |
| BorderFocusColor | #3D3D60 | 聚焦边框 |
| TextPrimaryColor | #F0F0F5 | 主要文本 |
| TextSecondaryColor | #9898B0 | 次要文本 |
| TextDisabledColor | #5A5A78 | 禁用文本 |
| AccentColor | #00FF00 | 强调色（霓虹绿） |
| AccentHoverColor / AccentDimColor / AccentGlowColor / AccentTintColor | #00CC00 / #009900 / #33FF33 / #0D260D | 强调色变体 |
| SecondaryAccentColor | #7B61FF | 次强调色（紫，信息/装饰用） |
| SuccessColor / WarningColor / ErrorColor | #00FF00 / #FFB800 / #FF3366 | 语义色 |
| GridLineColor / GridCenterLineColor | #3C3C50 / #505064 | 预览网格线 |

除 AccentGlowColor 外，以上颜色均有同名 SolidColorBrush（Brush 后缀）；AccentGlowColor 没有对应的 Brush 资源，仅被 AccentGlowEffect（DropShadowEffect）用作发光颜色。

### 字体 / 间距 / 圆角 / 效果

| 令牌 | 值 |
|------|------|
| FontFamilyPrimary | Microsoft YaHei UI, Segoe UI |
| FontFamilyMono | Cascadia Code, Consolas, Courier New |
| FontSizeDisplay / Heading / Body / Caption / Label | 24 / 16 / 14 / 12 / 11 |
| SpaceXs / Sm / Md / Lg / Xl | 4 / 8 / 16 / 24 / 32 |
| RadiusSm / Md / Lg | 4 / 6 / 8 |
| AccentGlowEffect | 强调色发光（Blur 12） |
| CardShadowEffect | 卡片阴影（Blur 8） |
| DialogShadowEffect / ToastShadowEffect | 对话框 / Toast 阴影 |

## ControlStyles.xaml 样式清单

`ControlStyles.xaml` 合并 DesignTokens.xaml 与 IconGeometries.xaml（App.xaml 只需合并它一个）。样式键：

SectionHeader、BodyText、PrimaryButton、SecondaryButton、AccentButton、CustomSlider、CustomTextBox、NumericTextBox、CustomComboBox、CompactComboBox、CustomCheckBox、ToggleSwitch、CustomExpander、ColorSwatchRadio、CardBorder、PreviewCard、TitleBarButton、TitleBarCloseButton、CustomScrollBar、CustomScrollViewer、SmoothScrollViewer、PrimaryIconButton、TransparentIconButton、DangerIconButton，以及共享缓动资源 `SnappyEase`（CubicEase, EaseOut）。悬停/聚焦过渡以 150ms 为主（约 29/47 处），实际 100–250ms 不等（如 CustomTextBox 聚焦 200ms、CustomCheckBox 悬停 120ms、ColorSwatchRadio 悬停 100ms、PreviewCard 悬停 250ms；CardBorder 无悬停动画）。

App.xaml 另定义 Window/TextBlock/Button/TextBox/ComboBox/CheckBox/Expander 的隐式全局样式（均基于上述键）。

## 遗留 / 未接线代码

以下代码存在于源码但当前没有调用方，清理或恢复引用前不要依赖：

| 位置 | 内容 | 说明 |
|------|------|------|
| MainWindow.xaml.cs | `CustomColor_Click` / `ShowColorPickerDialog` / `AddColorSlider` | 自绘 RGB 选色对话框；实际自定义颜色入口在 CrosshairPage（WinForms ColorDialog） |
| Controls/CrosshairPreview.cs | 准心预览控件 | 未被任何 XAML 引用，CrosshairPage 用双 Canvas 自绘预览 |
| Cs2ConfigViewModel | `MaxLaunchOptionsLength` 常量（1000） | 仅声明未使用 |
