# CrosshairPro.Core - 坑点

## 1. IConfigRepository.cs 一个文件定义三个仓库接口

`IConfigRepository`、`IAppStateRepository`、`IPresetRepository` 都在 `Interfaces/IConfigRepository.cs` 中定义，按文件名找接口容易漏掉后两个。实现侧：

```csharp
// JsonConfigRepository 同时实现两个接口，共享文件锁（SemaphoreSlim），避免并发冲突
public class JsonConfigRepository : IConfigRepository, IAppStateRepository

// IPresetRepository 由独立类实现，不在 JsonConfigRepository 上
public class JsonPresetRepository : IPresetRepository
```

DI 注册见 `src/CrosshairPro.Application/DI/ServiceCollectionExtensions.cs`。

## 2. Clone 与 CopyFrom 的语义差异

| 方法 | 用途 | 返回值 | Id 处理 |
|------|------|--------|---------|
| `Clone()` | 创建独立副本 | 新实例 | 生成新 GUID |
| `CopyFrom(other)` | 把值复制到当前实例 | 无 | 保留当前 Id |

`Clone()` 必须递归克隆嵌套对象：

```csharp
// ❌ 浅拷贝：Effects/Display 仍引用原对象，改副本会污染原配置
return new CrosshairConfig { Effects = config.Effects, Display = config.Display };

// ✅ 深拷贝：Outline/Shadow/Glow 与 Display 逐项重建
```

影响范围：`CrosshairConfig.Clone()`、`Preset.Clone()`（内部调用 `Config.Clone()`）、Application 层 `ConfigurationService.CloneConfig()`。
`Preset.Clone()` 额外做三件事：名称追加 " (副本)"、IsDefault 置 false、时间戳重置——复制预设时不要绕过它手工拼。

## 3. 嵌套对象属性变更需要手动订阅

`CrosshairConfig` 顶层属性变更自动通知，但 `Effects`、`Display` 及其子对象（Outline/Shadow/Glow）是独立的 ObservableObject，必须逐层订阅：

```csharp
config.PropertyChanged += ...;
config.Effects.PropertyChanged += ...;
config.Effects.Outline.PropertyChanged += ...;
```

`CopyFrom()` 对嵌套对象是"逐项改子属性值"而不是"整体换引用"，因此已建立的子对象订阅在 CopyFrom 之后仍然有效。

## 4. [ObservableProperty] 字段必须 _camelCase 命名

生成器按 `_size` → `Size` 规则生成公共属性；不加下划线前缀不会生成属性，绑定静默失效。

## 5. 部分公共属性不是可观察的

`[ObservableProperty]` 只加在一部分字段上，其余是普通自动属性，绑定或订阅它们收不到变更通知：

- `GameConfig.GameId`、`GameConfig.Settings`
- `HotkeyBinding.Action`、`HotkeyBinding.PresetId`

对这些属性做 UI 绑定或 `PropertyChanged` 监听时，要么改成可观察属性，要么赋值后手动触发通知。

## 6. 颜色是 #RRGGBB 字符串，无编译期校验

`CrosshairConfig.Color`、`OutlineConfig.Color` 等用 string 存十六进制颜色。`ColorConverter.ConvertFromString()` 遇到空串或非法格式（如 `#GGG`）会抛异常，解析必须放在 UI 层校验之后。

## 7. Brightness 范围是 0-200

UI 滑条 Maximum=200（`CrosshairPage.xaml`），渲染端 `ApplyBrightness()` 用 `Math.Min(255, ...)` 钳制：0 = 纯黑，200 ≈ 过曝偏白。不要按 0-100 处理。

## 8. CrosshairStyle 枚举值与 UI 下拉索引一一对应

`Cross=0 ... CustomImage=5` 的顺序即 UI `SelectedStyleIndex` 的索引；调整枚举顺序或中间插入新值会静默改变已有用户配置的含义。

## 9. Preset.IsDefault 保护

默认预设不可删除、不可被"保存覆盖"，但模型层没有强制约束，检查必须放在 PresetService/UI 层：

```csharp
if (preset == null || preset.IsDefault) return;
await _presetRepo.DeletePresetAsync(preset.Id);
```

## 10. KeyCombo.Modifiers 是 Win32 MOD_* 位标志

```csharp
Modifiers => (Ctrl ? 0x0002 : 0) | (Shift ? 0x0004 : 0) | (Alt ? 0x0001 : 0) | (Win ? 0x0008 : 0)
```

即 MOD_ALT=0x1、MOD_CONTROL=0x2、MOD_SHIFT=0x4、MOD_WIN=0x8，可直接传给 Win32 `RegisterHotKey`。`Parse()` 大小写不敏感，"+" 分段中最后一个非修饰键段作为 Key。

## 11. GameProfile.Matches 是 OrdinalIgnoreCase 完全匹配

进程名比较不含 ".exe" 后缀，是全等匹配而非子串匹配；`BuiltIn` 内置游戏进程名（如 `VALORANT-Win64-Shipping`、`FortniteClient-Win64-Shipping`）必须完整书写。当前仓库无 `Matches` 调用方（`IGameDetector` 未实现，见坑点 14），`Priority` 排序属未来检测器实现的预留语义。

## 12. Apex 配置模型与文件的对应关系

三个 Apex 模型按 cvar 名逐一映射本地文件，文件读写与备份在 Application 层 `ApexConfigService`：

- `ApexVideoConfig` → `%USERPROFILE%\Saved Games\Respawn\Apex\local\videoconfig.txt`（KeyValues 格式）
- `ApexSettingsConfig` → 同目录 `settings.cfg`（Valve CFG 格式）
- `ApexLaunchOptions` → 不落盘，`GenerateOptionsString()` 生成命令行

videoconfig/settings.cfg 的多数修改需重启游戏才生效（对应 `ConfigItemDefinition.RequiresRestart`）。`ApexVideoConfig` 的 `ResolutionIndex` 与 `DefaultRes/DefaultResHeight` 双向联动：直接改分辨率字段后需调用 `UpdateResolutionIndex()` 保持索引一致。

## 13. JSON 序列化与 ObservableObject

`[ObservableProperty]` 生成的是普通公共属性，System.Text.Json 可直接序列化；普通自动属性（如 `GameConfig.Settings`）同样没问题。但仅声明私有字段、未生成公共属性的数据不会进入 JSON——新增字段后若发现"存不进配置文件"，先确认公共属性确实存在。

## 14. IGameDetector 目前没有实现

`IGameDetector`、`GameInfo`、`GameProfile` 已在 Core 定义，但仓库内没有任何 `IGameDetector` 实现类（游戏检测功能预留）。依赖该接口的代码编译能通过，运行时需自行提供实现或桩。
