# CrosshairPro.Services - 坑点

## 1. 文件锁必要性

多个异步操作可能同时访问同一文件，必须使用 `SemaphoreSlim` 锁定：

```csharp
// ❌ 错误：并发写入可能冲突
await File.WriteAllTextAsync(_configFilePath, json);

// ✅ 正确：使用异步锁
await _fileLock.WaitAsync();
try { await File.WriteAllTextAsync(_configFilePath, json); }
finally { _fileLock.Release(); }
```

## 2. JSON 解析容错

加载配置时必须处理解析失败：

```csharp
public async Task<CrosshairConfig> LoadConfigAsync()
{
    try
    {
        var json = await File.ReadAllTextAsync(_configFilePath);
        var config = JsonSerializer.Deserialize<CrosshairConfig>(json, _jsonOptions);
        return config ?? CreateDefaultConfig();
    }
    catch
    {
        return CreateDefaultConfig(); // 解析失败返回默认
    }
}
```

## 3. 预设导入时 ID 冲突

导入预设时必须生成新 ID 避免与现有预设冲突：

```csharp
public async Task<Preset> ImportPresetAsync(string filePath)
{
    var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions);
    preset.Id = Guid.NewGuid().ToString(); // 生成新 ID
    preset.CreatedAt = DateTime.UtcNow;
    preset.UpdatedAt = DateTime.UtcNow;
    await SavePresetAsync(preset);
    return preset;
}
```

## 4. 配置目录创建

应用首次运行时配置目录不存在，必须创建：

```csharp
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appPath = Path.Combine(appDataPath, "CrosshairPro");
Directory.CreateDirectory(appPath); // 确保目录存在
```

## 5. 渲染器缓存清理

配置变更后需要清理缓存避免渲染错误：

```csharp
public void ClearCache()
{
    _penCache.Clear();
    _brushCache.Clear();
    _geometryCache.Clear();
}
```

调用时机：颜色、厚度、透明度等影响渲染属性的变更。

## 6. 预设文件命名

预设文件以 ID 命名（`{id}.json`），不是以名称命名。好处：
- 避免重命名时文件名变更
- 避免特殊字符导致文件名无效

## 7. 默认配置结构变更

`CreateDefaultConfig()` 返回的默认配置必须与模型默认值一致：

```csharp
private static CrosshairConfig CreateDefaultConfig()
{
    return new CrosshairConfig
    {
        Name = "默认配置",
        Style = CrosshairStyle.Cross,
        Size = 20,
        // ... 必须覆盖所有字段
    };
}
```

如果模型默认值变更，此处也需要同步更新。

## 8. JSON 序列化属性忽略

`JsonIgnoreCondition.WhenWritingNull` 配置下，null 属性不会被序列化：

```json
// CustomImagePath 为 null 时
{
  "customImagePath": null  // ❌ 不写入
}
{
  // ✅ 完全省略
}
```

导入时需要处理缺失属性的情况。