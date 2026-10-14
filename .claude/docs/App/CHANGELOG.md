# Changelog - App

> 模块变更历史。最新变更在最上方。
> 排查问题时优先阅读本文件。

---

## [2026-10-14] feat: 准心设置新增「旋转」参数；描边粗细行对齐滑杆行规格

**类型**: feat
**提交**: 未提交（工作区）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/CrosshairPage.xaml | +11/-4 | 亮度行后新增旋转行（Slider 0-360 + NumericTextBox，绑定既有 `Config.Rotation`——模型/Clone/CopyFrom/ConfigurationService 此前已支持，UI 未暴露）；效果区描边粗细行取消 24px 缩进、与「启用描边」开关行同缩进，数值框补 `Margin="8,0,0,0"` 与其他滑杆行同规格 |
| Views/CrosshairPage.xaml.cs | +6/-0 | 预览 `RenderCrosshair()` 对 `CrosshairCanvas` 施加 `RenderTransform(RotateTransform 旋转角, 画布中心)`，0° 置 null 防残留 |
| Views/OverlayWindow.cs | +5/-0 | 覆盖层 `RenderCrosshair()` 对 `_canvas` 同型施加旋转变换（App 未使用 Services 层 CrosshairRenderer，预览与覆盖层均为自绘 Shape，两条路径必须各自应用，否则覆盖层不随参数旋转） |
| Localization/Strings{,.zh-CN,.en}.resx | +3/-0 ×3 | 新增 `Crosshair.Rotation`（旋转 / 旋转 / ROTATION），置于 `Crosshair.CenterSize` 与 `Crosshair.Color` 之间 |
| docs/App/data-model.md（双树） | 同步 | 控制面板描述补旋转行与描边粗细新对齐；OverlayWindow.RenderCrosshair 表行补旋转变换说明 |

### 影响范围
- **行为**: 旋转 0-360° 实时生效于预览与覆盖层（`UpdateSourceTrigger=PropertyChanged`，走既有四级 PropertyChanged 重绘链路）；`Rotation` 字段本就随 config.json/预设序列化，旧配置文件缺字段反序列化为默认 0°
- **预留渲染器**: Services 层 `CrosshairRenderer` 的 RotateTransform 实现保持不变（App 未引用，属预留代码，本次未改动）
- **验证**: 评审工作流（真实 WPF STA harness：BAML 加载页面、绑定表达式检查、OverlayWindow 旋转变换、三 resx 文化取值、预设往返、旧配置兼容）——进行中，结果回填于本行

**类型**: fix
**提交**: 未提交（工作区）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| ViewModels/CrosshairViewModel.cs | +79/-14 | `LoadPresets()` 替换列表前捕获 `SelectedPreset?.Id`，替换后按 Id 重解析选中项（新增 `_updatingPresets` 抑制标志，`OnSelectedPresetChanged` 对 null/抑制期早退，不触发 CopyConfig/持久化，保住未保存微调）；仅当 Id 确实不存在才回退默认。根因：列表每次整体替换（反序列化新实例+重合成默认预设），ComboBox（TwoWay）与弹窗列表清空选中后 null 回推，触发回退分支把当前配置静默重置为默认并改写 appstate——评审代理在真实 WPF 环境实测复现（弹窗 TwoWay 与 ComboBox 两条路径各复现一次）。二轮评审再发现并修复同型缺口：catch 分支（预设仓库瞬时加载失败）原在抑制块外整体替换降级列表且不保留选中 Id，损失被推迟放大为下一次成功重载时的静默重置——新增 `_lastSelectedPresetId` 字段（非抑制选中时记录），回退分支先按该 Id 无副作用恢复（抑制块内，不 CopyConfig/不写 appstate），Id 确实不存在才回退默认；catch 分支降级列表替换纳入抑制块并按该 Id 尽力恢复选中。附带：`ImportPresetFromFileAsync` 改为按 Id 从重载列表选中（原选保存前实例，不在重载列表中，ComboBox 高亮会丢）；默认预设名随界面语言本地化（`Preset.Default`，堵住英文界面保存 "Default" 不触发重名序号的缺口） |
| MainWindow.xaml.cs | +1/-1 | 弹窗 ListBox SelectedItem 绑定 TwoWay→OneWay：弹窗定位为纯管理界面（删除/导入/导出），点击行仅本地高亮、不影响生效预设（与最初一次性赋值语义一致）；列表刷新能力（ItemsSource 属性绑定）保留 |
| Themes/DesignTokens.xaml | +29/-25 | 全部 22 个 SolidColorBrush 加 `PresentationOptions:Freeze="True"`：冻结后模板内属性路径动画（ToggleSwitch 勾选态、按钮/复选框/文本框悬停聚焦）在元素实例上自动克隆画笔，不再原地改写共享资源——修复开启描边/阴影期间全应用 `TextSecondaryBrush` 等被污染为霓虹绿；既有样式同型泄漏一并修复 |

### 影响范围
- **行为**: 删除任意非当前预设不再影响当前准心配置与 appstate；预设仓库瞬时加载失败不再放大为静默配置重置（catch 降级 + 恢复后按最近选中 Id 无副作用恢复）；开启效果开关期间全局次级文字/控件颜色保持不变；英文界面默认预设显示 "Default" 且同名保存会加序号
- **弹窗语义**: 点击行不再应用预设（管理界面定位）；如需行点击应用，应显式加 SelectionChanged→按 Id 设置 `SelectedPreset`（会恢复 CopyConfig 语义，需另行评估未保存微调）
- **冻结语义**: 主题画笔运行时不可变（全仓静态扫描确认无运行时改写点）；后续如需运行时换肤须改为克隆/替换资源方式
- **验证**: 两轮评审工作流（真实 BAML/STA 环境 + Fake 仓库身份抖动/可注入异常）：冻结修复 141 项动态断言全过；选择保全修复首轮 42/42（六场景 + 标志卫生），二轮评审发现 catch 分支缺口，修复后复验 74/74（T7 加载失败→恢复不重置、默认预设降级恢复、六场景回归、新回退分支语义与异常注入）
- **关联**: 修复对象为上文「预设管理优化」条目相关改动引入/暴露的回归

---

## [2026-10-14] feat: 单实例校验（命名 Mutex 防重复启动）

**类型**: feat
**提交**: 未提交（工作区）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| App.xaml.cs | +92/-0 | OnStartup 最先尝试创建 `Local\CrosshairPro.SingleInstance` 命名 Mutex：未获得所有权时经 `ExitAsAlreadyRunning()` 按已保存的语言偏好（`ReadSavedLanguage()` 同步读取，规避 LoadStateAsync 在 UI 线程 sync-over-async 死锁）弹本地化提示（`App.AlreadyRunning`）后 `Shutdown()` 退出；构造抛 `UnauthorizedAccessException`（同名互斥体存在但无权打开，如同会话管理员实例先启动/名称被占用）时走同一优雅退出路径；新增 OnExit 主动 Release/Dispose（未持有所有权时吞异常，退出路径已提前 Dispose 置空） |
| Localization/Strings.resx · Strings.zh-CN.resx · Strings.en.resx | +1 键×3 | 新增 `App.AlreadyRunning` 提示文案（弹窗标题复用既有 `App.Title`） |

### 影响范围
- **行为**: 重复启动不再产生第二个准心叠加层，同时避免第二实例全局热键注册失败（RegisterHotKey 冲突）；互斥体被无权打开时不再崩溃（此前会在 DispatcherUnhandledException 注册前直接崩溃，已实测复现）
- **锁定语义**: Mutex 为 `Local\` 命名空间（同一登录会话内唯一，多用户会话互不影响）；进程异常退出时系统自动回收，不会残留锁
- **跨模块**: Core（新增 CrosshairProPaths.cs，见 Core/CHANGELOG）；Services（JsonConfigRepository 路径推导改用，见 Services/CHANGELOG）

---

## [2026-10-14] style: 开关滑块改方圆造型

**类型**: style
**提交**: 未提交（工作区）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | ~1 处 | `ToggleSwitch` 滑块 CornerRadius 8（圆形）→ `RadiusSm`（方圆），与轨道圆角一致；尺寸/颜色/行程/动画不变 |

### 影响范围
- **UI**: 全部 `ToggleSwitch` 用点（CrosshairPage 效果区、ApexPage、Cs2Page）滑块同步变方圆；此前 cdf152f 仅轨道改矩形微圆角、滑块仍为圆形。CustomSlider 的圆形滑块点不受影响

---

## [2026-10-14] refactor: Apex 启动参数与游戏设置同卡分区，开关改矩形微圆角并加父子联动

**类型**: refactor
**提交**: ControlStyles 外形随 cdf152f 提交；ApexPage 分区重构与父子联动未提交（工作区）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | ~3 处 | `ToggleSwitch` 40×22 胶囊 → 44×22 矩形微圆角（CornerRadius 用 `RadiusSm`），滑块边距 2→3、行程 X 20→22（随 cdf152f 提交） |
| Views/GamePages/ApexPage.xaml | +90/-93 | ① 自定义启动面板移除「游戏设置」组（MaxFPS/ShowPositionInfo/FOVScale/LetterboxAspect 4 行，与游戏设置卡重复）；信箱比例行移入「窗口」组 ② 独立「游戏设置」卡并入「启动选项」卡：Separator + Accent 子标题分区，含鼠标灵敏度/鼠标加速/FOV/FPS/显示FPS/显示位置 6 行——启动参数与游戏设置同卡、与视频/画质等其他配置分开 ③ 父子联动：级联阴影←启用阴影、体积雾←体积光照、倍镜灵敏度 6 个滑杆行←独立倍镜灵敏度开关（`x:Name` + `ElementName` 绑定 IsChecked→IsEnabled，父关子整行禁用） ④ 联动子项取消缩进（级联阴影/体积雾标签与父项同列） ⑤ 值联动：父开关经 UI 点击关闭时子开关值同步置 0 |
| Views/GamePages/ApexPage.xaml.cs | +22 | `OnEnableShadowsClick`/`OnVolumetricLightingClick`：用 Click 事件（仅用户点击触发，加载配置/应用预设经绑定更新 IsChecked 不触发）检测父开关关闭，置 `CsmEnabled`/`VolumetricFog` = 0，经 PropertyChanged 刷新子开关 UI |

### 影响范围
- **数据流**: 游戏设置类条目（fps_max/cl_fov_scale/cl_showfps/cl_showpos）常规配置入口收敛为 settings.cfg；配套 Core `ApexLaunchOptions.GenerateOptionsString()` 改为 `+fps_max` 条件输出并移除 `+cl_fovScale`（见 Core/CHANGELOG 同日条目，随 b546b85 提交），**结构化生成通道**（自定义面板）的启动项默认值不再覆盖游戏设置。注意：启动项预设字符串（竞技优化/高画质仍含 `+fps_max 0`/`+cl_fovScale "2"`）与文本模式行为未变，选用预设或手填后仍可能覆盖 settings.cfg——如需彻底收敛可后续清理预设模板
- **UI**: 开关为 44×22 矩形微圆角（滑块同为方圆，见上方 style 条目）；联动子项父开关关闭时整行禁用（Opacity 0.4），布尔子项值同步置 0（重新开启父项不自动恢复，保持关闭）；倍镜滑杆子项仅禁用不禁值
- **跨模块**: Core（ApexLaunchOptions.cs）；文档同步 Core/data-model、GameConfig/Apex/pitfalls+launch-options（双树）

---

## [2026-10-14] feat: 预设管理优化（创建时间/重名序号）与效果区平铺药丸开关

**类型**: feat
**提交**: cdf152f（App 代码）/ b2b8175（PresetService 排序），文档随 ddfff7e；行数以 e946b95..cdf152f 为基线
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/CrosshairPage.xaml | +18/-12 | 效果区去掉 Expander 展开收起，平铺三行：启用描边/启用阴影改为「标签居左 + `ToggleSwitch` 药丸开关居右」（复用 ControlStyles 既有样式），描边粗细为缩进 24 的标签+NumericTextBox 行；区块标题由 `Crosshair.Advanced`（高级）改为 `Effects.Header`（效果） |
| MainWindow.xaml.cs | +33/-10 | 预设管理弹窗列表项改为「名称 + 创建时间（11px 次级色，`IsDefault` 时经 DataTrigger 隐藏）+ 删除×」两行布局，创建时间 Tooltip 用新键 `Preset.CreatedAt`；列表 ItemsSource/SelectedItem 由一次性赋值改为对 CrosshairViewModel 的属性绑定（`LoadPresets()` 会替换整个列表实例，原写法弹窗内删除后不刷新） |
| ViewModels/CrosshairViewModel.cs | +36/-3 | `SavePresetWithNameAsync` 重名处理：已存在同名预设时末尾追加数字序号（`x`→`x (1)`→`x (2)`；名称自带 ` (n)` 序号时从下一号续起，超长序号 TryParse 防御），Toast/`CurrentPresetName` 使用最终名称 |
| UtcToLocalTimeConverter.cs | +26/-0 | 新增 UTC DateTime → 本地时间字符串转换器（`yyyy-MM-dd HH:mm`），供创建时间绑定 |
| App.xaml | +3/-0 | 注册 `UtcToLocalTimeConverter` 资源（代码构建的模板内亦直接实例化） |
| Localization/Strings{,.zh-CN,.en}.resx | +3/each | 新增键 `Preset.CreatedAt`（创建时间 / Created at） |

### 影响范围
- **行为**: 效果开关绑定路径不变（`Config.Effects.Outline.Enabled` / `Config.Effects.Shadow.Enabled`），仅布局与视觉变化；保存同名预设不再静默产生重名而是自动追加序号去重
- **排序**: 预设列表（工具栏 ComboBox + 管理弹窗）默认按创建时间降序，由 Application 层 `LoadAllPresetsAsync()` 实现（见 Application CHANGELOG 同日条目）；默认预设仍固定置顶
- **样式**: `CustomExpander` 样式及 App.xaml 隐式 Expander 样式保留于 ControlStyles（当前无使用方，留作主题资源）
- **跨模块**: Application 层 PresetService；文档同步 docs/App/*、docs/Application/api-preset.md（双树）

---

## [2026-10-14] style: 布尔选项行改为药丸开关（标签居左、开关居右）

**类型**: style
**提交**: cdf152f（当时为工作区变更，后与后续样式调整合并入该提交）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Themes/ControlStyles.xaml | +95/-0 | 新增 `ToggleSwitch` 样式：CheckBox 重模板为 40×22 药丸轨道 + 16px 圆形滑块；选中 `#0D260D` 底/`#00FF00` 强调色，滑块 150ms CubicEase 平移；悬停仅在未选中时静态提亮（MultiTrigger 带 IsChecked=False 条件，避免与 Checked 动画冲突）；禁用 Opacity 0.4 |
| Views/GamePages/ApexPage.xaml | +165/-39 | 18 处 `CustomCheckBox` 勾选行改为 `*/Auto` 双列行（标签居左 + `ToggleSwitch` 居右）：自定义启动项面板 8 处（HighPriority/SkipIntro/DisableJoystick/WindowedMode/BorderlessWindow/ShowPositionInfo/DevMode/EnableConsole）+ 画质/性能/游戏/倍镜设置 10 处（EnableShadows/CascadeShadowMaps/VolumetricLighting/VolumetricFog/RagdollPhysics/DynamicResolution/MouseAcceleration/ShowFPS/ShowPosition/PerScopeSensitivity）；子选项改为标签缩进 24（开关仍右对齐）；文件头行排版规格注释同步 |
| Views/GamePages/Cs2Page.xaml | +7/-0 | `ConfigItemTemplateSelector.BoolTemplate` 同步改为「标签居左 + ToggleSwitch 居右」双列行 |

### 影响范围
- **行为**: 布尔选项视觉由勾选框变为药丸开关，绑定与数值语义不变（仍为 CheckBox IsChecked；Apex 整型配置项继续经 IntToBoolConverter）
- **样式**: `CustomCheckBox` 样式保留（CrosshairPage 等仍在使用），仅游戏配置页换用 `ToggleSwitch`；ControlStyles.xaml 工作区累计 diff 另含已记录的 SmoothScrollViewer ScrollStep +1/-1（见上文同日条目）
- **跨模块**: 无；文档同步 docs/App/data-model.md、docs/GameConfig/CS2/CHANGELOG.md（双树）

---

## [2026-10-14] fix: 游戏配置导航图标改为设置样式

**类型**: fix
**提交**: cdf152f（当时为工作区变更，后随该提交入库）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| MainWindow.xaml | +1/-1 | 左侧导航"游戏配置"TabNavItem 的 IconGeometry 由 `Gamepad` 改为 `Settings`（复用 IconGeometries.xaml 已有 `SettingsIconGeometry` ⚙ 齿轮图标，与准心图标同为线条风格） |

### 影响范围
- **行为**: 仅导航栏游戏配置入口图标变化；Tooltip/本地化/命令绑定不变
- **跨模块**: 无；`GamepadIconGeometry` 资源保留（其他位置可复用），文档同步 docs/App/CHANGELOG.md（双树）

---

## [2026-10-14] fix: 加快 SmoothScrollViewer 滚轮滚动速度

**类型**: fix
**提交**: cdf152f（当时为工作区变更，后随该提交入库）
**风险**: LOW

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Controls/SmoothScrollViewer.cs | ~3 处 | ScrollStep 默认值 32→120px；步长按实际 Delta 等比缩放（`-(e.Delta/120)*ScrollStep`，触控板小 Delta 不过跳）；动画期间新滚轮事件改为在 `_targetOffset` 上累加（连续滚动速度叠加），且动画中跳过目标相同早退守卫（避免误吞反向滚轮事件） |
| Themes/ControlStyles.xaml | +1/-1 | `SmoothScrollViewer` 样式 ScrollStep setter 32→120 |

### 影响范围
- **行为**: CrosshairPage / ApexPage / Cs2Page 三处使用该样式的滚动区域每格滚轮滚动距离约为原来的 3.75 倍，快速连续滚动可叠加
- **跨模块**: 无；文档同步 docs/App/data-model.md（双树）

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

## [2026-10-14] feat: 统一 Apex/CS2 配置页排版，配置管理移至底部并支持生成配置文件

**类型**: feat
**提交**: cdf152f（当时为工作区变更，后随该提交入库）
**风险**: MEDIUM

### 变更文件
| 文件 | 变更 | 说明 |
|------|------|------|
| Views/GamePages/ConfigItemTemplateSelector.cs | +35/-0 | 新增：按 `ConfigItemType` 选择配置项统一行模板（Bool→开关 / Int→滑杆+数值框 / Enum→下拉 / String→文本） |
| Views/GamePages/Cs2Page.xaml | +125/-83 | 配置项改经模板选择器渲染（含 String 行模板），消除 `Sections[x].Items[y]` 项级索引绑定；新增底部"配置管理"卡（本地配置路径 + 打开目录 + 生成配置文件）；底部按钮改用 `Cs2.*` 本地化键 |
| Views/GamePages/ApexPage.xaml | +341/-301 | 行规格统一（开关 `CustomCheckBox`、滑杆行 `*/160/64` 三列、下拉行宽 160、子选项缩进 24）；"配置文件管理"卡自顶部移至最下方并更名"配置管理"，新增启动项文件生成行；修复历史空提示 int 绑定 Bool 转换器永不显示的问题；修复注释乱码 |
| ViewModels/Cs2ConfigViewModel.cs | +111/-0 | 新增 `ConfigFilePath`、`OpenConfigFolderCommand`、`GenerateConfigFileCommand`（生成 exec 用 .cfg：fps_max/cl_showfps cvar + 启动项/视频设置注释；bool 渲染为 1/0；UTF-8 无 BOM；值取自当前 Sections 含未保存修改） |
| ViewModels/ConfigItemWrapper.cs | +22/-1 | `DisplayName` 增加项 Key→本地化键映射（英文界面不再显示策略层硬编码中文），未映射项回退策略定义 |
| ViewModels/ApexConfigViewModel.cs | +30/-0 | 新增 `GenerateLaunchOptionsFileCommand`（SaveFileDialog 导出当前启动项为文本文件） |
| Localization/Strings.resx / Strings.zh-CN.resx / Strings.en.resx | 各 +54/-0 | 新增 18 键：Game.ConfigManagement/GenerateConfig/OpenConfigFolder 系列、Cs2.ResetToDefault/SaveConfiguration/LocalConfigFile/GenerateConfigDesc、Apex.ScopeSection/LaunchOptionsFile/GenerateLaunchOptionsDesc、Dialog.GenerateCs2Config/GenerateApexLaunchOptions、Filter.CfgFiles/TextFiles |

### 影响范围
- **UI**: 两页卡片顺序统一——Apex：启动选项→视频→画质→性能→游戏→瞄准镜→预设→**配置管理（底部）**；CS2：视频→游戏→启动项→**配置管理（底部）**
- **CS2 生成文件**: `crosshairpro_cs2.cfg` 仅含安全白名单 cvar 与注释，不写游戏目录，用户自行放入 cfg 目录（编码 UTF-8 无 BOM，见 GameConfig/CS2/pitfalls.md §11）；坑点 App §17 / CS2 §18（索引绑定）标记已解决，CS2 底部按钮复用 Apex 本地化键的怪癖已修复
- **兼容性**: 无持久化结构变更，无 DI 变更，本地化仅新增键

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
