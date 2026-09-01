# CS2 与 CrosshairPro 集成指南

> 本文档说明 CS2 外部配置协议如何映射到 CrosshairPro 的 `GameConfigStrategy`、`GameProfile` 和 `GameConfig` 数据模型，并解释配置读写链路。

---

## 1. 集成架构

```
┌──────────────────────────────────────────────────────────┐
│                    CrosshairPro UI                       │
│  GamesPage → Cs2Page → Cs2ConfigViewModel                │
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
│  GameProfile(cs2.exe) ──→ GameConfig                     │
│           │                                               │
│           ▼                                               │
│  GameConfigStrategy(builtin-cs2)                         │
│  ├─ Sections[video, game]                                │
│  └─ Items[fullscreen, resolution, fps_max, ...]          │
└───────────────────────┬──────────────────────────────────┘
                        │
                        ▼
┌──────────────────────────────────────────────────────────┐
│        持久化 / 检测                                      │
│  %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json     │
│  Steam\userdata\<id>\730\local\cfg\video.txt (应用前)     │
│  Steam\userdata\<id>\730\local\cfg\config.cfg (应用前)   │
└──────────────────────────────────────────────────────────┘
```

---

## 2. GameProfile 映射

CS2 在 `GameProfile.BuiltIn.GetAll()` 中的注册：

```csharp
new GameProfile
{
    Id = "builtin-cs2",
    DisplayName = "Counter-Strike 2",
    ProcessName = "cs2",          // cs2.exe 主进程
    Priority = 100,               // 与 Apex/Overwatch2 同档
    // FullscreenOnly = false     // CS2 在窗口化下也能叠加准心
}
```

**关键点**：

| 字段 | 值 | 说明 |
|------|-----|------|
| `Id` | `builtin-cs2` | 与 `GameConfigStrategy.GameId` 一致 |
| `ProcessName` | `cs2` | 进程名（CS2.exe → cs2） |
| `Priority` | 100 | 高优先级，覆盖默认准心 |
| `FullscreenOnly` | false（默认） | CS2 窗口化也支持准心叠加 |

**自动切换流程**：

```
1. MainViewModel 启动后台 GameDetectionService
2. 轮询 Process.GetProcessesByName("cs2")
3. 匹配到 cs2.exe → GameProfile.Matches("cs2") = true
4. 加载 PresetId 关联的准心预设 → OverlayWindow 显示
5. 用户退出 CS2 → OverlayWindow 隐藏
```

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
| `game.fps_max` | Int | `fps_max` | `config.cfg` 或 `autoexec.cfg` | 0（无限制） |
| `game.cl_showfps` | Bool | `cl_showfps` | `config.cfg` 或 `autoexec.cfg` | 0（false） |

### 3.2 分区与文件对应

```
Sections[video] ──→ 写入 video.txt
Sections[game]  ──→ 写入 config.cfg 或 autoexec.cfg
```

> **注意**：`GameConfigService.ApplyConfigAsync` 目前是 stub，实际写入游戏配置文件需要按本协议实现。

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

// "16:9" → 计算宽高比标记（CS2 通过分辨率自动推断）
// 当前 Strategy 不写入 aspect_ratio，仅作 UI 提示
```

### 写入示例

```csharp
private async Task WriteVideoConfigAsync(
    string videoTxtPath,
    Dictionary<string, object> videoSettings)
{
    // 1. 备份原文件
    await BackupConfigFileAsync(videoTxtPath);

    // 2. 读取现有内容
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

### autoexec.cfg 写入示例

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

### 6.1 启动参数模板

CrosshairPro UI 可以提供预设模板：

| 模板 | 启动参数 |
|------|----------|
| 竞技优化 | `-novid -high -threads 8 -freq 144 +exec autoexec.cfg` |
| 高刷新率 | `-novid -high -freq 240 +fps_max 0 +exec autoexec.cfg` |
| 低端机器 | `-novid -threads 4 -freq 60 -w 1280 -h 720 -windowed -noborder +fps_max 60` |
| 调试 | `-console -dev +cl_showfps 1 +cl_showpos 1` |

### 6.2 启动参数写入

启动参数不属于游戏配置文件，而是 Steam UI 中的"启动选项"字段。CrosshairPro 写入位置：

```
Steam 安装路径\userdata\<SteamID3>\config\localconfig.vdf
  └── Steam 启动项配置
```

**实现建议**：
- 通过 Steamworks SDK 或 `ISteamApps` 接口写入
- 或在 UI 中引导用户手动粘贴

> 当前 `GameConfigService` **不**修改 Steam 启动参数，仅保存在 `GameConfig.LaunchOptions` 中供用户参考。

---

## 7. 数据流

### 7.1 读取流程

```
1. 用户在 GamesPage 选择 "Counter-Strike 2"
   │
2. Cs2ConfigViewModel 加载
   │ IGameConfigService.GetStrategy("builtin-cs2")
   │ → GameConfigStrategy
   │
3. IGameConfigService.GetConfigAsync("builtin-cs2")
   │ 优先读 %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json
   │ 不存在则根据 Strategy.DefaultValue 创建
   │
4. UI 绑定 ConfigItemWrapper 显示配置项
```

### 7.2 保存流程

```
1. 用户在 UI 中修改 fullscreen / resolution / fps_max
   │ ConfigItemWrapper 触发 INotifyPropertyChanged
   │
2. Cs2ConfigViewModel 收集 Settings 字典
   │
3. IGameConfigService.SaveConfigAsync(GameConfig)
   │ 序列化为 JSON
   │ 写入 %APPDATA%/CrosshairPro/gameconfigs/builtin-cs2.json
   │
4. （可选）IGameConfigService.ApplyConfigAsync("builtin-cs2")
   │ 检查 cs2.exe 是否运行
   │ 备份并 patch video.txt / config.cfg / autoexec.cfg
```

### 7.3 应用流程（写入游戏）

```
ApplyConfigAsync("builtin-cs2")
   │
   ├─ 检测 cs2.exe
   │   ├─ 运行中：跳过写入，仅保存 CrosshairPro 配置
   │   └─ 未运行：继续
   │
   ├─ 备份 video.txt / autoexec.cfg
   │
   ├─ 写入 video.txt（patch 而非覆盖）
   │
   ├─ 写入/追加 autoexec.cfg
   │
   └─ 输出："已应用 CS2 配置"
```

> **当前状态**：`ApplyConfigAsync` 是 TODO stub，集成时需要按本节实现。

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
   └─ GameConfig (video/game) → video.txt / config.cfg
```

### 8.2 切换游戏
1. 检测到 cs2.exe
2. 加载该 `GameProfile.PresetId` 关联的 `Preset`
3. 同时加载该 `GameProfile.Id` 关联的 `GameConfig`
4. `OverlayWindow` 显示准心（Preset）
5. 游戏应用配置（GameConfig → 写入游戏）

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

## 10. CS2 vs 其他游戏的差异

| 维度 | CS2 | Apex Legends | Valorant |
|------|-----|--------------|----------|
| 启动项支持 | ✅ | ✅ | ❌ |
| 配置文件位置 | Steam 云 | 本地 Saved Games | 不支持 |
| autoexec | ✅ | ❌ | ❌ |
| 视频配置格式 | KeyValues | KeyValues | 专用客户端 |
| 反作弊 | VAC | Easy Anti-Cheat | Vanguard |
| CrosshairPro 深度集成 | 仅 Strategy | 完整服务（`IApexConfigService`） | 仅 Strategy |

**集成优先级**：
- CS2: 通用 Strategy ✅
- Apex: 专用服务（备份/恢复/导入导出）✅
- CS:GO: 复用 CS2 Strategy ✅
- 其他: 仅通用 Strategy

---

## 11. 实施检查清单

新增 CS2 集成时需要验证：

- [ ] `GameConfigService.GetStrategy("builtin-cs2")` 返回有效策略
- [ ] `GameConfigService.GetConfigAsync("builtin-cs2")` 读取 `builtin-cs2.json`
- [ ] 修改配置后 `SaveConfigAsync` 持久化
- [ ] `ApplyConfigAsync` 检测 cs2.exe，未运行时写入
- [ ] video.txt 写入使用 UTF-8 无 BOM
- [ ] autoexec.cfg 追加而非覆盖
- [ ] 写入前备份原文件
- [ ] LaunchOptions 不超过 1000 字符
- [ ] VAC 白名单：仅写入本协议定义的字段
- [ ] 检测 cs2.exe 与 csgo.exe 同存情况，分别路由

---

## 12. 相关资源

- [video.md](video.md) - video.txt 字段定义
- [config.md](config.md) - config.cfg 字段定义
- [launch-options.md](launch-options.md) - 启动参数说明
- [data-model.md](data-model.md) - 数据结构定义
- [pitfalls.md](pitfalls.md) - CS2 配置坑点
- [Application/README.md](../Application/README.md) - 服务注册与 DI
- [Core/data-model.md](../Core/data-model.md) - GameConfig/GameProfile 模型

---

**创建时间**: 2026-09-01
**协议版本**: 1.1