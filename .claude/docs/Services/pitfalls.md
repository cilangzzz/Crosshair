# CrosshairPro.Services - 坑点

## 文件锁

### 1. 锁必须 WaitAsync + finally Release

```csharp
// ✅ 正确
await _fileLock.WaitAsync();
try { await File.WriteAllTextAsync(_configFilePath, json); }
finally { _fileLock.Release(); }

// ❌ 同步 Wait 可能死锁；漏掉 finally 会永久持锁
```

### 2. 两仓库的锁互不共享

`JsonConfigRepository`（锁 config.json + appstate.json）与 `JsonPresetRepository`（锁 presets/）各持一把 `SemaphoreSlim`。不要假设"数据层只有一把全局锁"；也不要让一个仓库的方法去等另一个仓库的锁。

### 3. DI 必须保证 IConfigRepository/IAppStateRepository 同实例

两个接口指向同一个 `JsonConfigRepository` 单例。若分别注册两个接口会得到两个实例、两把锁，失去"配置+状态整体一致"的语义。错误注册示例与 DI 代码详解见 [Application/pitfalls.md §7](../Application/pitfalls.md)。

### 4. Export/Import 方法不加锁（当前实现的坑）

`JsonConfigRepository.ExportConfigAsync/ImportConfigAsync` 全程不取锁；`JsonPresetRepository.ExportPresetAsync` 写目标文件、`ImportPresetAsync` 读源文件也不取锁。并发场景下导出可能读到写了一半的文件（WriteIndented JSON 非原子写）。新增类似方法时要么补锁，要么确认只操作仓库外路径且可容忍竞态。

## JSON 序列化

### 5. 解析失败静默返回默认值

`LoadConfigAsync`/`LoadStateAsync` 捕获所有异常并返回默认配置——应用不会因配置损坏崩溃，但**用户无感知**。排查"配置丢了"的问题时先检查 config.json 是否能被 `JsonSerializer.Deserialize<CrosshairConfig>` 解析。预设仓库行为不同：`LoadPresetsAsync` 跳过坏文件，`ImportPresetAsync` 则直接抛异常。

### 6. 枚举序列化为整数

`style` 存的是 `CrosshairStyle` 枚举整数（Cross=0…CustomImage=5）。在枚举中间插入新值会使旧文件语义错位；新增枚举值只能追加到末尾。如需字符串可加 `JsonStringEnumConverter`（当前未使用）。

### 7. WhenWritingNull 与缺失键

null 属性整键省略（如 `customImagePath`）。反序列化对缺失键与显式 null 都落到模型默认值，导入旧版本文件时不要假设键一定存在。空字符串 `""` 会照常输出，与 null 语义不同：约定 null=未设置，""=显式为空。

### 8. 默认配置必须与模型默认值同步

`CreateDefaultConfig()` 只显式赋 8 个字段，其余依赖模型默认值。修改 Core 模型默认值（如 `ShadowConfig.OffsetY = 2`）后，此处无需改动，但新增"必须非模型默认"的字段要记得同步。详见 [../Core/README.md](../Core/README.md)。

## 渲染

### 9. Freezable 必须 Freeze

Pen/Brush 创建后立即 `Freeze()`，否则被非创建线程访问时抛 `InvalidOperationException`，且只在特定时机出现，难以复现。注意：渲染器当前未被 UI 使用（`CrosshairRenderer` 无任何消费方，覆盖层实际走 `OverlayWindow.RenderCrosshair()` 的 Shape 绘制，不涉及本渲染器的 Pen/Brush），此坑目前是理论风险；若未来把渲染器接入 WPF 渲染线程（如 `OnRender`/后台线程预渲染），需重点防范。

### 10. 缓存键用下划线拼接

键格式 `{color}_{thickness}_{opacity}`。参数是数字、颜色是十六进制字符串时不会冲突；但若未来颜色值允许含 `_` 的自定义格式，键会歧义。要改就用不可能出现在值中的分隔符（如 `|`）。`_geometryCache` 只声明未写入，勿假设有几何缓存命中。

### 11. 自定义图片的两个坑

- **文件锁定**：`new BitmapImage(new Uri(path))` 默认缓存选项会保持源文件流打开，用户无法删除/替换正在使用的图片。修复方式是 `StreamSource` + `BitmapCacheOption.OnLoad` + `Freeze()`。
- **每帧重新加载**：`RenderCustomImage` 不走缓存，每次渲染都新建 `BitmapImage` 并解码。图片较大或窗口频繁重绘时有明显开销；且图片路径不存在时静默不渲染，UI 层无从感知。
- 另：CustomImage 不支持阴影/描边（其他样式支持），效果开关对它无效。

### 12. 阴影使绘制量翻倍

每个图元先画一遍偏移副本（透明度减半）再画主线，启用阴影时 Draw 调用数翻倍。静态准心可考虑预渲染位图。

### 13. 渲染器预留项未实现

- `Glow` 效果：配置结构完整（EffectsConfig.Glow），渲染器完全没有对应绘制逻辑，开了也没视觉效果。
- `RenderCompleted.RenderTimeMs` 恒为 0，不能当真实耗时用。
- `_geometryCache` 未使用（见坑点 10）。

依赖这三个特性的上层功能（App/Application 层）需要确认后再接。

## 预设管理

### 14. 文件名用 ID 不用名称

预设文件名 = `{preset.Id}.json`。避免重命名导致文件名变化、特殊字符产生非法文件名。修改预设名称只需重写同一文件。

### 15. 导入强制换 ID

`ImportPresetAsync` 总是生成新 GUID + 重置时间戳，因此重复导入同一文件会得到多个副本而非覆盖。复制预设的"(副本)"命名与 ID 生成由 `Preset.Clone()`（Core 层）负责。

### 16. 默认预设保护不在本层

`IsDefault=true` 的预设禁止删除/保存的校验由 Application 层（PresetService）负责，本仓库的 `DeletePresetAsync/SavePresetAsync` 不做检查。绕过服务层直接调仓库会破坏保护，见 [../Application/README.md](../Application/README.md)。

## 目录与路径

### 17. 目录创建分布在各仓库构造函数

`JsonConfigRepository` 创建 `%APPDATA%/CrosshairPro/`，`JsonPresetRepository` 创建 `presets/` 子目录。新增仓库时必须在构造函数创建自己的目录，否则首次保存前目录不存在会抛 `DirectoryNotFoundException`。注意 `gameconfigs/` 由 Application 层创建，不归本模块。

### 18. 导入导出路径来自外部

`ExportConfigAsync/ImportConfigAsync/ExportPresetAsync/ImportPresetAsync` 接受任意用户路径，无范围校验。路径合法性由 UI 层（文件对话框）保证；本层不做路径注入防护，也不要在本层添加相对路径拼接逻辑。

### 19. 渲染缓存无淘汰机制

Pen/Brush 缓存按参数组合累积，无上限。用户长期调整颜色/厚度/透明度会缓慢增长内存；`ClearCache()` 是手动全清，上层目前从未调用（全仓库仅命中定义处）。图片内存同样无尺寸/压缩限制。
