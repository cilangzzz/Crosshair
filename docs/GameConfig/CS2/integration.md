# CS2 与 CrosshairPro 集成指南

> 本文档说明 CS2 外部配置协议如何映射到 CrosshairPro 的 `GameConfigStrategy`、`GameProfile` 和 `GameConfig` 数据模型，并解释配置读写链路。模型本体定义见 [data-model.md](data-model.md)。

---

## 1. 集成架构

```
┌──────────────────────────────────────────────────────────┐
│                    CrosshairPro UI                       │
│  GamesPage(GamesViewModel)                               │
│    → GamePageSelector("builtin-cs2" → Cs2Page)           │
│    → Cs2ConfigViewModel（DI 单例，注册于 App.xaml.cs）    │
└───────────────────────┬──────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────┐
│          Application Services                            │
│  ┌─────────────────────┐  ┌─────────────────────────┐    │
│  │ IGameConfigService  │  │ IPresetService          │    │
│  │ (通用 8 游戏)       │  │ (准心预设)             │    │
│  └──────────┬──────────┘  └─────────────────────────┘    │
│             │                                            │
│  ┌──────────▼──────────┐                                 │
│  │ GameConfigService   │                                 │
│  │ (含 builtin-cs2)    │                                 │
│  └─────────────────────┘                                 │
└───────────────────────┬──────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────┐
│        Core Models                                       │
│  GameProfile(BuiltIn, cs2) ──→ GameConfig                │
│           │                                              │
│           ▼                                              │
│  GameConfigStrategy(builtin-cs2)                         │
│  ├─ Sections[video, game]                                │
│  └─ Items[fullscreen, resolution, aspect_ratio,          │
│           refresh_rate, fps_max, cl_showfps]             │
└───────────────────────┬──────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────┐
│        持久化 / 检测                                      │
│  %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json     │
│  Steam\userdata\<id>\730\local\cfg\video.txt (协议参考)   │
│  Steam\userdata\<id>\730\local\cfg\config.cfg (协议参考)  │
└──────────────────────────────────────────────────────────┘
```

**DI 注册**：`IGameConfigService → GameConfigService` 注册于 `Application/DI/ServiceCollectionExtensions.cs`；`Cs2ConfigViewModel`（单例）注册于 `App/App.xaml.cs`。

---

## 2. GameProfile 映射与实际加载流程

CS2 在 `GameProfile.BuiltIn.GetAll()` 中的注册：

```csharp
new GameProfile
{
    Id = "builtin-cs2",
    DisplayName = "Counter-Strike 2",
    ProcessName = "cs2",          // cs2.exe 主进程
    Priority = 100,               // 与 Apex 同档
    // FullscreenOnly = false     // CS2 在窗口化下也能叠加准心
}
```

**关键点**：

| 字段 | 值 | 说明 |
|------|-----|------|
| `Id` | `builtin-cs2` | 与 `GameConfigStrategy.GameId` 一致 |
| `ProcessName` | `cs2` | 进程名（CS2.exe → cs2） |
| `Priority` | 100 | 高优先级 |
| `FullscreenOnly` | false（默认） | CS2 窗口化也支持准心叠加 |

**实际加载流程（当前实现）**：

```
1. GamesViewModel 构造时加载 GameProfile.BuiltIn.GetAll()（默认选中 Apex）
2. 用户点击 "Counter-Strike 2" → SelectedGame 变更 → CurrentGameId = "builtin-cs2"
3. GamesPage 的 GamePageSelector 按 gameId 路由：builtin-cs2 → Cs2Page
4. Cs2Page.Loaded → 从 DI 容器取 Cs2ConfigViewModel → DataContext 绑定
5. Cs2ConfigViewModel.LoadAsync()：
   a. GetStrategy("builtin-cs2") — 为 null 时状态栏显示 Path.NotFound
   b. GetConfigAsync("builtin-cs2") — 读 JSON / 生成默认配置
   c. 按 Strategy.Sections 包装为 Sections 集合（ConfigSectionWrapper）
   d. LaunchOptions / LaunchOptionsLength 赋值
   e. IsCs2Running = IsProcessRunning("cs2")（Process.GetProcessesByName）
```

> **与旧文档的差异**：当前代码中**没有** `GameDetectionService`，也不存在"检测到 cs2.exe 自动切换 OverlayWindow 预设"的后台轮询。进程检测仅发生在 `Cs2ConfigViewModel` 内（加载时一次，及 `RefreshRunningStateCommand` 手动刷新），且 `IsCs2Running` / `RefreshRunningStateCommand` 目前**未在 Cs2Page.xaml 中绑定**，UI 不渲染该状态。按进程自动切换准心仍属未实现功能（`GameProfile.Matches`、`PresetId` 字段已就位，等待消费方）。

---

## 3. GameConfigStrategy 映射

CS2 策略定义在 `GameConfigService.CreateCS2Strategy()`：

```csharp
new GameConfigStrategy
{
    GameId = "builtin-cs2",
    SupportsLaunchOptions = true,
    LaunchOptionsDescription = "CS2 启动项参数，如 -high -threads 12 -novid",
    Sections = new List<ConfigSectionDefinition>
    {
        new()
        {
            Name = "video",
            DisplayName = "视频设置",
            Items = new List<ConfigItemDefinition>
            {
                new() { Key = "fullscreen",     Type = ConfigItemType.Bool, DefaultValue = true },
                new() { Key = "resolution",     Type = ConfigItemType.Enum, DefaultValue = "1920x1080", Options = ... },
                new() { Key = "aspect_ratio",   Type = ConfigItemType.Enum, DefaultValue = "16:9", Options = ... },
                new() { Key = "refresh_rate",   Type = ConfigItemType.Int,  DefaultValue = 144, MinValue = 60, MaxValue = 360 },
            }
        },
        new()
        {
            Name = "game",
            DisplayName = "游戏设置",
            Items = new List<ConfigItemDefinition>
            {
                new() { Key = "fps_max",   Type = ConfigItemType.Int,  DefaultValue = 0, MinValue = 0, MaxValue = 999 },
                new() { Key = "cl_showfps", Type = ConfigItemType.Bool, DefaultValue = false },
            }
        }
    }
}
```

### 3.1 配置项到 CS2 cvar 的映射

| Strategy Key | Type | CS2 cvar | 写入位置 | 默认值 |
|--------------|------|----------|----------|--------|
| `video.fullscreen` | Bool | `setting.fullscreen` | `video.txt` | 1（true） |
| `video.resolution` | Enum | `setting.defaultres` + `setting.defaultresheight` | `video.txt` | `1920x1080` |
| `video.aspect_ratio` | Enum | （无直接对应，由分辨率推断） | `video.txt` | `16:9` |
| `video.refresh_rate` | Int | `setting.refreshrate` | `video.txt` | 144 |
| `game.fps_max` | Int | `fps_max` | `autoexec.cfg`（推荐，避免 config.cfg 云同步冲突） | 0（无限制） |
| `game.cl_showfps` | Bool | `cl_showfps` | `autoexec.cfg`（推荐） | 0（false） |

### 3.2 分区与文件对应

```
Sections[video] ──→ 写入 video.txt
Sections[game]  ──→ 写入 autoexec.cfg（推荐）或 config.cfg
```

> **注意**：`GameConfigService.ApplyConfigAsync` 目前是空实现（`Task.CompletedTask` + TODO），CrosshairPro **不写入任何游戏配置文件**。上表的"写入位置"是协议目标，实际写入需按本节与 §4/§5 实现。

---

## 4. video.txt 字段映射

CS2 Strategy 中定义的视频项对应 `video.txt` 的实际键：

| UI 项 | `setting.` 前缀键 | 类型 | 示例值 |
|-------|------------------|------|--------|
| `fullscreen` | `setting.fullscreen` | int | `"1"` |
| `resolution` | `setting.defaultres` + `setting.defaultresheight` | int × 2 | `"1920"` + `"1080"` |
| `refresh_rate` | `setting.refreshrate` | int | `"144"` |

**枚举值展开**：

```csharp
// "1920x1080" → 写入两个键
"setting.defaultres"    "1920"
"setting.defaultresheight" "1080"

// "16:9" → 由分辨率自动推断
// 当前 Strategy 不写入 aspect_ratio，仅作 UI 提示
```

### 写入示例（未实现的协议参考代码）

```csharp
private async Task WriteVideoConfigAsync(
    string videoTxtPath,
    Dictionary<string, object> videoSettings)
{
    // 1. 备份原文件
    await BackupConfigFileAsync(videoTxtPath);

    // 2. 逐行 patch（UTF-8 无 BOM），未命中的行原样保留
    var lines = await File.ReadAllLinesAsync(videoTxtPath, new UTF8Encoding(false));
    var newLines = new List<string>();

    foreach (var line in lines)
    {
        if (line.Contains("\"setting.defaultres\"") && videoSettings.TryGetValue("resolution", out var res))
        {
            var parts = ((string)res).Split('x');
            newLines.Add($"    \"setting.defaultres\"    \"{parts[0]}\"");
            continue;
        }
        if (line.Contains("\"setting.defaultresheight\"") && videoSettings.TryGetValue("resolution", out res))
        {
            var parts = ((string)res).Split('x');
            newLines.Add($"    \"setting.defaultresheight\"    \"{parts[1]}\"");
            continue;
        }
        if (line.Contains("\"setting.fullscreen\"") && videoSettings.TryGetValue("fullscreen", out var fs))
        {
            newLines.Add($"    \"setting.fullscreen\"    \"{(bool)fs ? 1 : 0}\"");
            continue;
        }
        if (line.Contains("\"setting.refreshrate\"") && videoSettings.TryGetValue("refresh_rate", out var rr))
        {
            newLines.Add($"    \"setting.refreshrate\"    \"{rr}\"");
            continue;
        }

        newLines.Add(line);
    }

    // 3. 写回（UTF-8 无 BOM）
    await File.WriteAllLinesAsync(videoTxtPath, newLines, new UTF8Encoding(false));
}
```

---

## 5. config.cfg / autoexec.cfg 字段映射

CS2 Strategy 的 `game` 分区对应游戏设置：

| UI 项 | CS2 cvar | 写入位置 |
|-------|----------|----------|
| `fps_max` | `fps_max` | `autoexec.cfg`（推荐） |
| `cl_showfps` | `cl_showfps` | `autoexec.cfg` |

### autoexec.cfg 写入示例（未实现的协议参考代码）

```csharp
private async Task WriteAutoExecAsync(
    string autoExecPath,
    Dictionary<string, object> gameSettings)
{
    // 1. 追加而非覆盖
    var lines = new List<string>();
    if (File.Exists(autoExecPath))
    {
        lines.AddRange(await File.ReadAllLinesAsync(autoExecPath, new UTF8Encoding(false)));
    }
    lines.Add(""); // 空行分隔
    lines.Add($"// CrosshairPro managed - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

    if (gameSettings.TryGetValue("fps_max", out var fps))
    {
        lines.Add($"fps_max \"{fps}\"");
    }
    if (gameSettings.TryGetValue("cl_showfps", out var showFps) && (bool)showFps)
    {
        lines.Add("cl_showfps 1");
    }

    await File.WriteAllLinesAsync(autoExecPath, lines, new UTF8Encoding(false));
}
```

### 完整 autoexec.cfg

```cfg
// 用户配置
exec user_settings.cfg

// 鼠标
m_rawinput "1"
sensitivity "1.5"

// 帧率
fps_max "0"

// 显示
cl_showfps 1

// CrosshairPro managed - 2026-09-01 14:32:18
fps_max "0"
cl_showfps 1
```

---

## 6. 启动项集成

CS2 `SupportsLaunchOptions=true` 表示支持 Steam 启动参数：

```csharp
// GameConfig.LaunchOptions 字段
public string LaunchOptions { get; set; } = "";

// 用户设置示例
LaunchOptions = "-novid -high -threads 8 -freq 144 +exec autoexec.cfg";
```

**CrosshairPro 当前行为**：`Cs2ConfigViewModel` 提供启动项编辑框（`LaunchOptions`，`UpdateSourceTrigger=PropertyChanged` 实时更新字符计数 `LaunchOptionsLength`，经本地化键 `Cs2.LaunchOptionsChars` 显示），以及"复制到剪贴板"按钮（`CopyLaunchOptionsCommand`，成功提示 `Cs2.SavedToClipboard`）。常量 `MaxLaunchOptionsLength = 1000` 已定义但保存时尚未据此校验（见 [pitfalls.md](pitfalls.md) 第 9 节）。

### 6.1 启动参数模板

CrosshairPro UI 可以提供预设模板：

| 模板 | 启动参数 |
|------|----------|
| 竞技优化 | `-novid -high -threads 8 -freq 144 +exec autoexec.cfg` |
| 高刷新率 | `-novid -high -freq 240 +fps_max 0 +exec autoexec.cfg` |
| 低端机器 | `-novid -threads 4 -freq 60 -w 1280 -h 720 -windowed -noborder +fps_max 60` |
| 调试 | `-console -dev +cl_showfps 1 +cl_showpos 1` |

### 6.2 启动参数写入

启动参数不属于游戏配置文件，而是 Steam UI 中的"启动选项"字段。Steam 存储位置：

```
Steam 安装路径\userdata\<SteamID3>\config\localconfig.vdf
  └── Steam 启动项配置
```

**实现建议**：
- 通过 Steamworks SDK 或 `ISteamApps` 接口写入
- 或在 UI 中引导用户手动粘贴（当前即此模式：复制到剪贴板）

> 当前 `GameConfigService` **不**修改 Steam 启动参数，仅保存在 `GameConfig.LaunchOptions` 中供用户参考。

---

## 7. 数据流

### 7.1 读取流程（Cs2ConfigViewModel.LoadAsync）

```
1. 用户在 GamesPage 选择 "Counter-Strike 2"
   │
2. Cs2ConfigViewModel.LoadAsync()
   │ IGameConfigService.GetStrategy("builtin-cs2")
   │ → null 时 StatusMessage = Path.NotFound，直接返回
   │
3. IGameConfigService.GetConfigAsync("builtin-cs2")
   │ 命中 _configCache 直接返回
   │ 否则读 %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json
   │ 文件不存在 / 解析失败 → CreateDefaultConfig（按 Strategy.DefaultValue 填充）
   │
4. 按 Strategy.Sections 构建 ObservableCollection<ConfigSectionWrapper>
   │ ConfigItemWrapper 从 settings 取值，未命中回退 DefaultValue
   │
5. LaunchOptions / LaunchOptionsLength / IsCs2Running 赋值，StatusMessage 清空
```

### 7.2 保存流程（SaveCommand → SaveAsync）

```
1. 用户在 UI 中修改 fullscreen / resolution / fps_max 等
   │ ConfigItemWrapper.OnValueChanged 同步写入 settings 字典
   │
2. SaveAsync 重新 GetConfigAsync（命中缓存取回同一实例）
   │ config.LaunchOptions = LaunchOptions
   │ 遍历 Sections，将非 null 的 item.Value 写入 config.Settings
   │
3. IGameConfigService.SaveConfigAsync(GameConfig)
   │ 更新 _configCache → 序列化 JSON（WriteIndented）
   │ → 写入 %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json
   │
4. 成功：StatusMessage + Toast（Cs2.SaveSuccess）
   失败：StatusMessage + Toast（Cs2.SaveFailed，含异常消息）
```

### 7.3 重置流程（ResetCommand → ResetAsync）

```
ResetAsync
   ├─ IGameConfigService.ResetToDefaultAsync("builtin-cs2")
   │   └─ CreateDefaultConfig → SaveConfigAsync（覆盖 builtin-cs2.json）
   ├─ LoadAsync()（重新加载默认值到 UI）
   └─ Toast（Cs2.ResetSuccess）
```

### 7.4 应用流程（写入游戏，未实现）

```
ApplyConfigAsync("builtin-cs2")   ← 当前为 TODO stub，直接返回 CompletedTask
   │
   ├─ （计划）检测 cs2.exe
   │   ├─ 运行中：跳过写入，仅保存 CrosshairPro 配置
   │   └─ 未运行：继续
   ├─ （计划）备份 video.txt / autoexec.cfg
   ├─ （计划）写入 video.txt（patch 而非覆盖）
   ├─ （计划）写入/追加 autoexec.cfg
   └─ （计划）输出："已应用 CS2 配置"
```

> **当前状态**：CrosshairPro 仅保存配置到本地 JSON，不写游戏文件。实现时按 §4/§5 的参考代码与 [pitfalls.md](pitfalls.md) 的防御点进行。

---

## 8. 预设与策略分离

### 8.1 准心预设 vs 游戏配置

| 概念 | 类型 | 作用域 | 存储 |
|------|------|--------|------|
| 准心预设 | `Preset` | 跨游戏 | `presets/{id}.json` |
| 游戏配置 | `GameConfig` | 单游戏 | `gameconfigs/{gameId}.json` |
| 游戏元数据 | `GameProfile` | 单游戏 | 内置 + 内存 |
| 协议策略 | `GameConfigStrategy` | 单游戏 | 代码内置 |

```
GameProfile (cs2)
   │
   ├─ Preset (Crosshair) → OverlayWindow
   │
   └─ GameConfig (video/game) → builtin-cs2.json（暂不写游戏文件）
```

### 8.2 切换游戏
1. 用户在 GamesPage 选择游戏（当前为手动选择，无进程自动切换）
2. 加载该 `GameProfile.PresetId` 关联的 `Preset`（字段已定义，切换联动未实现）
3. `GamePageSelector` 路由到对应游戏页，页面加载该 `GameProfile.Id` 关联的 `GameConfig`

---

## 9. 关键文件路径

| 用途 | 路径 |
|------|------|
| CrosshairPro 配置 | `%APPDATA%\CrosshairPro\gameconfigs\builtin-cs2.json` |
| CS2 视频配置 | `Steam\userdata\<SteamID3>\730\local\cfg\video.txt` |
| CS2 游戏配置 | `Steam\userdata\<SteamID3>\730\local\cfg\config.cfg` |
| CS2 自定义配置 | `Steam\userdata\<SteamID3>\730\local\cfg\autoexec.cfg` |
| CS2 机器配置 | `Steam\userdata\<SteamID3>\730\local\cfg\cs2_machine_convars.vcfg` |
| CS2 安装默认配置 | `Steam\steamapps\common\Counter-Strike 2\game\cfg\config_default.cfg` |
| Steam 启动项 | `Steam\userdata\<SteamID3>\config\localconfig.vdf` |

---

## 10. CS2 vs 其他内置游戏的差异

| 维度 | CS2 | Apex Legends |
|------|-----|--------------|
| 启动项支持 | ✅ | ✅ |
| 配置文件位置 | Steam 云 | 本地 Saved Games |
| autoexec | ✅ | ❌ |
| 视频配置格式 | KeyValues | KeyValues |
| 反作弊 | VAC | Easy Anti-Cheat |
| CrosshairPro 深度集成 | 通用 Strategy + 专用页面 | 完整服务（`IApexConfigService`） |

**集成状态**：
- CS2: 通用 Strategy + 专用页面（Cs2Page/Cs2ConfigViewModel）✅
- Apex: 专用服务（备份/恢复/导入导出）+ 通用 Strategy ✅
- 其他游戏: 未内置

---

## 11. 实施检查清单

新增 CS2 集成时需要验证：

- [ ] `GameConfigService.GetStrategy("builtin-cs2")` 返回有效策略
- [ ] `GameConfigService.GetConfigAsync("builtin-cs2")` 读取 `builtin-cs2.json`
- [ ] 修改配置后 `SaveConfigAsync` 持久化
- [ ] `ApplyConfigAsync` 从 stub 升级为真实实现：检测 cs2.exe，未运行时写入
- [ ] video.txt 写入使用 UTF-8 无 BOM
- [ ] autoexec.cfg 追加而非覆盖
- [ ] 写入前备份原文件
- [ ] 启动项长度校验落地：`Cs2ConfigViewModel.MaxLaunchOptionsLength = 1000` 常量已定义但 `SaveAsync` 尚未据此拒绝/警告
- [ ] `IsCs2Running` / `RefreshRunningStateCommand` 目前未在 Cs2Page.xaml 绑定，如需显示运行状态需补 UI
- [ ] VAC 白名单：仅写入本协议定义的字段
- [ ] 检测 cs2.exe 与 csgo.exe 同存情况，分别路由
- [ ] UI 绑定依赖 Sections/Items 顺序（见 [pitfalls.md](pitfalls.md) 第 18 节），调整策略定义顺序需同步修改 Cs2Page.xaml

---

## 12. 相关资源

- [video.md](video.md) - video.txt 字段定义
- [config.md](config.md) - config.cfg 字段定义
- [launch-options.md](launch-options.md) - 启动参数说明
- [data-model.md](data-model.md) - 数据结构定义
- [pitfalls.md](pitfalls.md) - CS2 配置坑点
- [Application/README.md](../../Application/README.md) - 服务注册与 DI
- [Core/data-model.md](../../Core/data-model.md) - GameConfig/GameProfile 模型
- [App/README.md](../../App/README.md) - Cs2Page / ConfigItemWrapper / ToastNotification

---

**创建时间**: 2026-09-01
**协议版本**: 1.1
