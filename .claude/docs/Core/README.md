# CrosshairPro.Core

核心层（基础层）模块：定义全部数据模型、业务接口和枚举，本身不含业务实现，被其他所有模块依赖。

## 概述

- 源码位置：`src/CrosshairPro.Core`，共 19 个源文件（根目录 1 / 枚举 2 / 接口文件 4 / 模型文件 12）
- 依赖 NuGet 包：CommunityToolkit.Mvvm 8.3.2（`ObservableObject` / `[ObservableProperty]`）、System.Text.Json 8.0.5（csproj 声明，Core 代码当前未直接使用）
- 被依赖：CrosshairPro.Infrastructure、CrosshairPro.Services、CrosshairPro.Application、CrosshairPro.App

## 目录结构

```
CrosshairPro.Core/
├── CrosshairProPaths.cs       # 应用数据目录/持久化文件路径唯一权威定义（%APPDATA%\CrosshairPro）
├── Enums/
│   ├── AppState.cs            # 应用状态枚举（Idle/GameMode/CrosshairVisible/CrosshairHidden）
│   └── CrosshairStyle.cs      # 准心样式枚举（6 种样式）
├── Interfaces/
│   ├── IConfigRepository.cs   # 一文件三接口：IConfigRepository + IAppStateRepository + IPresetRepository
│   ├── ICrosshairRenderer.cs  # 准心渲染器接口 + RenderCompletedEventArgs
│   ├── IGameDetector.cs       # 游戏检测器接口（仓库内暂无实现，预留）
│   └── IHotkeyManager.cs      # 热键管理器接口（IDisposable）+ HotkeyTriggeredEventArgs
└── Models/
    ├── ApexLaunchOptions.cs   # Apex 启动项模型，GenerateOptionsString() 生成命令行
    ├── ApexSettingsConfig.cs  # Apex settings.cfg 字段映射（鼠标/帧率/FOV/网络/语音等）
    ├── ApexVideoConfig.cs     # Apex videoconfig.txt 字段映射（分辨率/画质/性能等）
    ├── AppPersistedState.cs   # 应用持久化状态（当前预设ID/修改标记/界面语言）
    ├── CrosshairConfig.cs     # 准心配置主模型（含 Clone/CopyFrom）+ DisplayConfig
    ├── EffectsConfig.cs       # 效果配置（OutlineConfig/ShadowConfig/GlowConfig）
    ├── GameConfig.cs          # 单个游戏的配置数据（GameId + LaunchOptions + Settings 字典）
    ├── GameConfigStrategy.cs  # 游戏配置策略 + ConfigItemType/ConfigItemDefinition/ConfigSectionDefinition
    ├── GameInfo.cs            # 游戏信息 record + GameDetectedEventArgs
    ├── GameProfile.cs         # 游戏档案（进程匹配/优先级）+ BuiltIn 内置 2 款游戏（CS2/Apex）
    ├── HotkeyBinding.cs       # 热键绑定 + HotkeyAction 枚举 + KeyCombo 结构
    └── Preset.cs              # 预设模型（配置快照 + 元数据）
```

注意：旧文档提到的 `Enums/ConfigItemType.cs` 已不存在，`ConfigItemType` 枚举现定义在 `Models/GameConfigStrategy.cs` 内。

## 枚举一览

| 枚举 | 定义位置 | 取值 |
|------|----------|------|
| AppState | Enums/AppState.cs | Idle / GameMode / CrosshairVisible / CrosshairHidden |
| CrosshairStyle | Enums/CrosshairStyle.cs | Cross / Dot / Circle / TShape / XShape / CustomImage |
| HotkeyAction | Models/HotkeyBinding.cs | ToggleCrosshair / SwitchPreset / ResetPosition / LockPosition / IncreaseSize / DecreaseSize |
| ConfigItemType | Models/GameConfigStrategy.cs | Bool / Int / Enum / String |

数值与完整说明见 [data-model.md](data-model.md)。

## 接口一览

| 接口 | 职责 | 当前实现 |
|------|------|----------|
| IConfigRepository | 主配置加载/保存/重置/导入导出 | Services 层 JsonConfigRepository |
| IAppStateRepository | 应用持久化状态读写（与 IConfigRepository 同文件定义） | JsonConfigRepository 同类实现 |
| IPresetRepository | 预设增删改查与导入导出 | Services 层 JsonPresetRepository |
| ICrosshairRenderer | 准心渲染 + RenderCompleted 事件 | Services 层 CrosshairRenderer |
| IGameDetector | 游戏进程监控、游戏档案注册 | 暂无实现（预留） |
| IHotkeyManager | 全局热键注册/注销 + HotkeyTriggered 事件 | Infrastructure 层 HotkeyManager |

成员签名见 [data-model.md](data-model.md)。

## 模型一览

| 模型 | 说明 |
|------|------|
| CrosshairConfig | 准心配置主模型；`Clone()` 深拷贝（新 GUID），`CopyFrom()` 原地复制（保留 Id） |
| DisplayConfig | CrosshairConfig 的显示部分：显示器/鼠标穿透/置顶/位置偏移（定义于 CrosshairConfig.cs） |
| EffectsConfig | 效果容器：OutlineConfig（描边）、ShadowConfig（阴影）、GlowConfig（发光） |
| Preset | 预设 = CrosshairConfig 快照 + 游戏关联 + 热键 + IsDefault 标记 |
| AppPersistedState | 启动恢复数据：CurrentPresetId、IsConfigModified、Language |
| HotkeyBinding | 热键绑定：Combo 字符串 + HotkeyAction 动作；配套 KeyCombo 解析结构 |
| GameProfile | 游戏档案：进程名匹配、优先级、预设关联；`BuiltIn.GetAll()` 提供内置 2 款游戏（CS2/Apex） |
| GameInfo | 已检测到的游戏进程（record：ProcessName/DisplayName/ProcessId/StartTime） |
| GameConfig | 单个游戏的配置数据：启动项字符串 + `Dictionary<string, object>` 设置字典 |
| GameConfigStrategy | 游戏配置元数据：分区、配置项定义、配置文件路径模板 |
| ApexVideoConfig | Apex `videoconfig.txt` 全量字段映射（59 个属性，含分辨率索引联动） |
| ApexSettingsConfig | Apex `settings.cfg` 全量字段映射（鼠标/倍镜灵敏度/帧率/网络/语音等） |
| ApexLaunchOptions | Apex 启动项，`GenerateOptionsString()` 生成完整命令行 |

字段级说明见 [data-model.md](data-model.md)。

## 依赖关系

```
Core（无外部项目依赖）
 ↑ Infrastructure（Win32/热键）
 ↑ Services（配置持久化/渲染）
 ↑ Application（业务服务）
 ↑ App（WPF UI）
```

## 详细文档

- [数据模型](data-model.md) — 全部模型/接口/枚举的字段级说明、关系图、数值范围
- [坑点](pitfalls.md) — 已知问题与注意事项
- [变更日志](CHANGELOG.md) — 模块变更历史
