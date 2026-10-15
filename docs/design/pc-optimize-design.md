# 电脑优化页设计（PcOptimizePage：只开入口、绝不代改）

> 状态：**已实现（2026-10-15）**。
> 关联：[games-tab-overflow-design](games-tab-overflow-design.md)（电脑优化页的入口来源）、[App/README](../App/README.md)、[GameConfig/CS2/pitfalls](../GameConfig/CS2/pitfalls.md)（VAC 边界同源原则）。

## 概述

「电脑优化」页（`Views/GamePages/PcOptimizePage.xaml` + `ViewModels/PcOptimizeViewModel.cs`）为 FPS 玩家提供 29 项 Windows 游戏向优化条目，按 7 个分类组织。**硬约束（用户明确要求）：本页绝不修改任何系统设置**——每项优化只提供「是什么 / 为什么要动 / 收益多大 / 风险何在」的说明，以及一个直达对应 Windows 管理页的「打开」按钮，全部操作由用户在系统页面手动完成。

这与项目既有的 VAC 边界原则（CS2 通道只 patch 白名单字段）一脉相承，且更进一步：连游戏配置都不写，只做「导航」。

## 一、设计原则

| 原则 | 落地 |
|------|------|
| 只开不动 | 应用内唯一的写操作是启动一个 `Process.Start`（`UseShellExecute=true`）打开系统管理页；无注册表、无 powercfg 写入、无服务操作 |
| 一项一页 | 每个条目对应一个确定的 Windows 管理页（ms-settings URI / 控制面板 .cpl / 系统 exe），用户落地后按说明操作 |
| 静默降级 | 命令执行失败 → Toast 提示（`PcOptimize.OpenFailed`），不弹窗、不重试；厂商面板未安装 → Toast 提示（`PcOptimize.VendorNotFound`） |
| 有据可查 | 29 条命令全部经官方文档核对（learn.microsoft.com ms-settings 权威列表、控制面板规范名列表），并在本机 Win11 26200 实测过单字符串 ShellExecute 行为 |
| 反迷思内容 | 页面内置「避坑」卡，列出经来源核实的无效/有害优化（注册表清理、debloat 脚本、内存清理器等），防止用户被第三方「优化工具」误导 |

## 二、研究方法

三项工作由一个三代理 workflow 并行完成（2026-10-15，共 217k tokens / 107 次工具调用）：

1. **optimizations**：以「每项优化必须对应一个 Windows 管理页」为准入条件，收集 29 项候选（来源：Microsoft Support、Xbox Support、Intel/NVIDIA/AMD 官方文档、主流技术媒体），产出每项的 why/how/gain/risk 四段中文文案与英文标题；
2. **deeplinks**：逐条核验打开命令——优先引用 [learn.microsoft.com ms-settings 权威列表](https://learn.microsoft.com/en-us/windows/apps/develop/launch/launch-settings-app) 与[控制面板规范名列表](https://learn.microsoft.com/en-us/windows/win32/shell/controlpanel-canonical-names)，并在本机（Win11 Pro for Workstations 26200）实测 `Process.Start(UseShellExecute=true)` 的实际行为，记录 Win11 落地路径与 Win10 回退 URI；
3. **myths**：收集 11 条常见「游戏优化」迷思（注册表清理、一键 debloat、计时器强改、HPET 玄学、内存清理、关页面文件、SSD 手动整理、Realtime 优先级、加速器全家桶、笔记本强开卓越性能、批量禁服务），全部附来源。

核验阶段的两个关键本地实测结论直接决定了实现（见第四节）。

## 三、29 项优化清单

按页面分类顺序。命令列为 `PcOptimizeItem` 的 `OpenTarget`/`OpenArguments`；「落地页」为 Win11 zh-CN 实际路径。

### 系统（8 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 开启游戏模式 | `ms-settings:gaming-gamemode` | 设置 > 游戏 > 游戏模式 | 视情况 |
| 游戏栏设置 | `ms-settings:gaming-gamebar` | 设置 > 游戏 > 游戏栏 | 低 |
| 关闭后台录制 | `ms-settings:gaming-gamedvr` | 设置 > 游戏 > 摄像（Win10「游戏 DVR」，URI 同） | 中 |
| 禁用开机自启 | `ms-settings:startupapps` | 设置 > 应用 > 启动 | 中 |
| 游戏时勿扰 | `ms-settings:notifications` | 设置 > 系统 > 通知（Win10「专注助手」=`ms-settings:quiethours`） | 中 |
| 内存完整性权衡 | `windowsdefender://coreisolation` | 安全中心 > 设备安全性 > 内核隔离 | 视情况（含安全权衡警示） |
| 使用时间与暂停更新 | `ms-settings:windowsupdate-activehours` | 设置 > Windows 更新 > 高级选项 > 活动时间 | 低 |
| 卸载闲置软件 | `ms-settings:appsfeatures` | 设置 > 应用 > 安装的应用 | 视情况 |

### 电源（4 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 高性能电源计划 | `powercfg.cpl` | 控制面板 > 电源选项 | 中 |
| 关闭USB选择性暂停 | `control` + `powercfg.cpl,,3` | 电源选项 > 更改高级电源设置 > USB 设置 | 视情况 |
| 关闭PCIe链路节能 | `control` + `powercfg.cpl,,3` | 同上 > PCI Express > 链路状态电源管理 | 低 |
| 电源与睡眠超时 | `ms-settings:powersleep` | 设置 > 系统 > 电源和电池 > 屏幕和睡眠 | 低 |

### 显示（6 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 硬件加速GPU计划 | `ms-settings:display-advancedgraphics-default` | 设置 > 显示 > 显示卡 > 默认图形设置（Win10 回退 `-advancedgraphics`） | 视情况 |
| 逐游戏图形优化 | `ms-settings:display-advancedgraphics` | 设置 > 显示 > 显示卡（按 exe 添加） | 视情况 |
| 刷新率设为最高 | `ms-settings:display-advanced` | 设置 > 显示 > 高级显示器设置（Win11 需再点一次「高级显示器设置」；Win10 直达） | 高 |
| HDR 游戏设置 | `ms-settings:display-hdr` | 设置 > 显示 > HDR（24H2+ 实测有效；Win10 回退 `-advancedcolor`） | 视情况 |
| 显示缩放 100% | `ms-settings:display` | 设置 > 系统 > 显示 | 视情况 |
| 视觉效果调优 | `SystemPropertiesPerformance.exe` | 性能选项 > 视觉效果对话框 | 低 |

### 网络（3 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 网卡节能管理 | `ncpa.cpl` | 网络连接（适配器右键 > 属性 > 配置 > 电源管理/高级） | 视情况 |
| 关闭代理与VPN | `ms-settings:network-proxy` | 设置 > 网络和 Internet > 代理 | 视情况 |
| 限制更新占带宽 | `ms-settings:delivery-optimization` | 设置 > Windows 更新 > 高级选项 > 传递优化 | 低 |

### 存储（3 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 开启存储感知 | `ms-settings:storagesense` | 设置 > 系统 > 存储 > 存储感知 | 低 |
| 碎片整理与优化 | `dfrgui.exe` | 优化驱动器（SSD 显示「优化」= TRIM） | 低 |
| 游戏装进SSD | `ms-settings:savelocations` | 设置 > 存储 > 保存新内容的地方 | 高 |

### 驱动与显卡（3 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 更新显卡驱动 | `devmgmt.msc` | 设备管理器 > 显示适配器 | 中 |
| 可选驱动更新 | `ms-settings:windowsupdate-optionalupdates` | 设置 > Windows 更新 > 可选更新 | 低 |
| 显卡厂商控制面板 | ProbePaths（见 4.2） | NVIDIA 控制面板 / AMD Adrenalin | 中 |

### 输入与音频（2 项）

| 条目 | 打开命令 | Win11 落地页 | 收益 |
|------|----------|--------------|------|
| 关闭增强指针精度 | `control` + `main.cpl,,2` | 鼠标属性 > 指针选项（取消「提高指针精确度」） | 中 |
| 关闭音频增强 | `mmsys.cpl` | 声音控制面板 > 播放设备 > 属性 > 增强功能 | 低 |

## 四、关键技术决策

### 4.1 ShellExecute 单字符串不拆参（本机实测）

`Process.Start(new ProcessStartInfo(cmd) { UseShellExecute = true })` 把 `cmd` **整体**作为 `FileName`，ShellExecute **不拆分空格参数**：

- `"control powercfg.cpl,,3"` → Win32Exception「系统找不到指定的文件」（本机实测复现）；
- 裸 `powercfg.cpl` / `SystemPropertiesPerformance.exe` / `dfrgui.exe` / `devmgmt.msc` / `ncpa.cpl` / `mmsys.cpl` → PATH/System32 搜索后单字符串启动成功（本机实测）；
- `ms-settings:` URI 与 `windowsdefender://` URI → 单字符串启动成功。

因此 `PcOptimizeItem` 拆成 `OpenTarget` + `OpenArguments` 两个字段：带参 cpl 一律走 `FileName="control"` + `Arguments="powercfg.cpl,,3"` 形态（`main.cpl,,2` 同理），其余用裸名或 URI。`OpenItem` 命令实现：

```csharp
var psi = new ProcessStartInfo(item.OpenTarget) { UseShellExecute = true };
if (!string.IsNullOrEmpty(item.OpenArguments)) psi.Arguments = item.OpenArguments;
Process.Start(psi);
```

### 4.2 厂商 GPU 面板探测（ProbePaths）

`nvcplui.exe` 不在 PATH，必须探测全路径；Intel 面板是商店应用无统一直达路径。`PcOptimizeItem.ProbePaths` 按优先级存放候选：

1. `C:\Program Files\NVIDIA Corporation\Control Panel Client\nvcplui.exe`（NVIDIA 官方文档确认的 DCH 标准路径）
2. `C:\Program Files\AMD\CNext\CNext\RadeonSoftware.exe`（本机 AMD 环境实测存在）

`OpenItem` 取 `ProbePaths.FirstOrDefault(File.Exists)`；全部未命中 → Toast `PcOptimize.VendorNotFound`，提示从开始菜单打开（Intel 用户即走此路径）。

### 4.3 打开失败的静默降级

`Process.Start` 抛异常（文件缺失、被安全软件拦截等）→ 捕获 → Toast `PcOptimize.OpenFailed`（带 `{0}` 异常消息）。无弹窗、无重试——失败信息足够用户自查，且保持「工具不纠缠」的交互基调。

## 五、页面结构与数据流

```
┌─ PcOptimizePage ─────────────────────────────────────────┐
│ 电脑优化（16 SemiBold Accent）                            │
│ 免责声明：不会修改任何系统设置，只提供入口（11 Secondary） │
│ ┌─ SmoothScrollViewer ─────────────────────────────────┐ │
│ │ ItemsControl Categories（7 张卡）                     │ │
│ │   CardBorder = 分类头(SectionHeader) + ItemsControl   │ │
│ │     条目行 = 标题+收益徽章 / 描述 / 「打开」按钮       │ │
│ │ 避坑卡（6 条静态 TextBlock）                           │ │
│ │ 页脚注：路径以 Win11 为准（11, 右对齐）                │ │
│ └──────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

数据流：`PcOptimizePage.OnLoaded` 从 `App.Services` 解析 `PcOptimizeViewModel`（DI Singleton）→ `Categories`（`ObservableCollection<PcOptimizeCategory>`，构造函数静态构建 7 组 29 项，无持久化）→ 条目 `HasEntry` 控制按钮可见性 → 点击走 `[RelayCommand] OpenItem`。Toast 经 `ToastRequested` 事件由 code-behind 转发 `ToastNotification.ShowIn`（与 Cs2Page 同构）。

无配置文件读写、无游戏检测、无定时器——页面是纯静态内容 + 事件命令。

## 六、避坑卡（迷思内容）

研究共核实 11 条迷思；页面展示 6 条（与 FPS 玩家最相关、且是本工具用户最可能接触的第三方「优化」），其余 5 条（debloat 脚本、HPET/bcdedit、关页面文件、Realtime 优先级、笔记本强开卓越性能）保留在研究档案中，暂不上页面以控制篇幅：

| 页面条目 | 核心理由（均附来源核实） |
|----------|--------------------------|
| 注册表清理 | 微软官方不支持；残留键值对性能零影响，误删可致系统损坏 |
| 批量禁用系统服务 | 空闲服务不占 CPU；常弄坏更新/音频/网络 |
| 强改计时器分辨率 0.5ms | Win11 已按进程生效，全局强制多数场景无效 |
| 内存清理/加速器 | 空闲内存即文件缓存；清空待机列表反而更卡 |
| 手动整理 SSD 碎片 | 无机械寻道；徒增写入损耗，Windows 自动 TRIM |
| 游戏加速器全家桶 | 实测平均帧率几乎无提升，工具自身常驻占用 |

## 七、本地化

- 新增 `PcOptimize.*` 键 82 个 × 3 份 resx（`Strings.resx` / `zh-CN` / `en`），键总数 216 → 297（三份同步，已 grep 计数核对）；
- 同步移除占位键 `Games.PcOptimizeDesc`（功能开发中文案）；
- 条目文案结构：`PcOptimize.{Id}`（标题）、`PcOptimize.{Id}Desc`（正文 = 是什么 + 怎么做 + 风险注记，合并为一段避免三行排版过碎）、`PcOptimize.Gain{High,Medium,Low,Varies}`（收益徽章）；
- 新增 `LocalizedTextConverter`（IMultiValueConverter，[0]=locKey、[1]=CurrentCulture 触发语言切换重算），App.xaml 注册为资源；`LocalizationProvider.Get` 缺键回退键名，故漏译不致空白。

## 八、验证记录（2026-10-15，UIA 自动化）

| # | 验证项 | 方法 | 结果 |
|---|--------|------|------|
| 1 | 页面可达性 | 启动应用 → 几何匹配导航至游戏页 → ComboBox 选中「电脑优化」 | ✅ |
| 2 | 内容完整性 | UIA 抽取全部文本元素：141 个，7 分类头 + 29 条目（标题/收益/描述）+ 29 个「打开」按钮 + 避坑卡 + 页脚注全部渲染 | ✅ |
| 3 | 入口可用性 | UIA 点击第一个「打开」（游戏模式）→ `SystemSettings` 进程出现（PID 核对）→ 自动关闭还原 | ✅ |
| 4 | 双语切换 | 🌐 切 EN：5 个英文锚点文案全中；切回 ZH：5 个中文锚点全中；结束恢复默认中文 | ✅ |
| 5 | 构建 | `dotnet build` 0 警告 0 错误 | ✅ |

UIA 脚本存于 `.claude/temp/pcopt-verify*.ps1`（会话产物，不入库）。

### 验证期间发现的环境事实（备查）

- `TabNavItem` 无 UIA Name（ToolTip 不映射 Name），自动化需按空名+尺寸过滤；
- PowerShell 5.1 解析无 BOM 的 UTF-8 .ps1 时中文按 GBK 吞字节——脚本必须带 BOM；
- **主窗口关闭后进程仍驻留**（仅剩 Overlay 覆盖层，MainWindowTitle 为空）——经核实为**有意设计**：OnClosing 拦截关闭事件最小化到托盘（代码注释明示），托盘菜单已有「退出」项（`Tray.Exit` → `ReallyExit()` → Shutdown）。对验证脚本的影响：不能按窗口名判断进程是否活着，构建解锁需按进程名杀（见 App/pitfalls §25）。

## 九、已知限制与后续方向

1. **语言偏好被验证脚本改动**：切换语言会写 `appstate.json`，验证结束已恢复为中文（会话初始值）。
2. **刷新率 / HDR 等 URI 的版本差异**：`ms-settings:display-hdr` 未收录于微软官方列表（ElevenForum 24H2 实测确认），未来 Windows 版本若失效，回退路径 `display-advancedcolor` → `display` 需手工加参数级联；当前实现为单命令直开，失败即 Toast——可接受，不做版本嗅探。
3. **厂商面板探测**：仅覆盖 NVIDIA/AMD 标准安装路径；便携版/自定义路径安装的用户会看到 VendorNotFound 提示，属预期降级。
4. **主窗口关闭驻留**：已确认为既有的「关闭到托盘」设计（OnClosing 拦截 + 托盘「退出」菜单项），与本页无关——验证期间仅提示：自动化与构建脚本需按进程名而非窗口名判断应用状态。
5. **后续可选项**（均未排期）：按 OS 版本隐藏不适用条目（如 Win10 的 HDR URI 回退）、条目「复制路径」按钮、把研究档案中未上页的 5 条迷思做成折叠区。
