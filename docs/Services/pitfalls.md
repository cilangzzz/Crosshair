# CrosshairPro.Services 坑点

## 文件锁并发问题

### 问题描述
`JsonConfigRepository` 同时实现 `IConfigRepository` 和 `IAppStateRepository`，配置和状态操作共享同一 `SemaphoreSlim` 锁。

### 影响
- 配置和状态不能同时读写（符合预期）
- 多个调用方需要等待锁释放

### 最佳实践
```csharp
// 正确：使用异步等待
await _fileLock.WaitAsync();
try
{
    // 文件操作
}
finally
{
    _fileLock.Release();
}

// 错误：同步等待可能死锁
_fileLock.Wait(); // 不要这样做
```

### 注意事项
- 必须在 `finally` 块中释放锁
- 服务注册为 Singleton，锁对象不会重建
- 不要在持有锁时调用其他可能也需要锁的方法

## JSON 序列化问题

### 枚举序列化为整数

**现象**: `CrosshairStyle` 枚举序列化为整数（`Cross` = 0, `Dot` = 1, ...）

**原因**: 默认的 `JsonSerializer` 不使用字符串枚举转换器

**影响**: 配置文件可读性降低，但对功能无影响

**解决方案**（如需字符串）:
```csharp
_jsonOptions.Converters.Add(new JsonStringEnumConverter());
```

### 日期时间格式

**现象**: `DateTime` 序列化为 ISO 8601 格式 `"2024-01-01T00:00:00Z"`

**影响**: 时区信息包含在字符串中，反序列化时转换为本地时间

**建议**: 所有时间戳使用 UTC 存储，显示时再转换为本地时间

### 空值处理

**现象**: 空字符串 `""` 和 `null` 在 JSON 中表现不同

**示例**:
```json
{
  "customImagePath": null,    // 不输出（WhenWritingNull）
  "gameAssociation": ""       // 输出空字符串
}
```

**建议**: 使用 `null` 表示"未设置"，空字符串表示"显式设置为空"

## 渲染性能

### 缓存键冲突

**问题**: 缓存键 `"颜色_参数"` 可能因字符串拼接导致冲突

**示例**:
```
"Red_1_50"  // Red, thickness=1, opacity=50
"Red_15_0"  // Red, thickness=15, opacity=0 -> 相同键！
```

**当前实现**: 使用下划线分隔参数，假设参数中不含下划线

**安全做法**:
```csharp
// 更安全的键生成
var key = $"{color}|{thickness}|{opacity}";
```

### Freezable 线程安全

**要求**: 所有 WPF 渲染资源必须 `Freeze()` 后才能跨线程访问

**正确做法**:
```csharp
var brush = new SolidColorBrush(color);
brush.Freeze(); // 必须
_brushCache[key] = brush;
```

**忘记 Freeze 的后果**:
- 在非创建线程访问时抛出 `InvalidOperationException`
- 可能只在特定运行时机出现（难以调试）

### 阴影渲染性能

**问题**: 每个阴影都需要额外绘制一次完整的准心形状

**影响**: 启用阴影时渲染负载翻倍

**优化建议**:
- 对于静态准心，考虑预渲染到 Bitmap
- 使用 `RenderTargetBitmap` 缓存复杂效果

## 自定义图片加载

### 文件锁定

**问题**: `BitmapImage` 创建后会锁定源文件

**现象**: 用户无法修改/删除正在使用的自定义图片

**解决方案**:
```csharp
// 方式1: 使用 Stream 加载（推荐）
using var stream = File.OpenRead(path);
var image = new BitmapImage();
image.BeginInit();
image.StreamSource = stream;
image.CacheOption = BitmapCacheOption.OnLoad;
image.EndInit();
image.Freeze();

// 方式2: 复制到内存（大文件）
var bytes = File.ReadAllBytes(path);
var image = new BitmapImage();
image.BeginInit();
image.StreamSource = new MemoryStream(bytes);
image.EndInit();
image.Freeze();
```

### 路径验证

**问题**: 配置中的 `CustomImagePath` 可能在运行时不存在

**当前处理**: 静默忽略，不渲染任何内容

**建议**: 在 UI 层验证路径有效性，显示加载状态

## 预设管理

### 默认预设保护

**问题**: 默认预设（`IsDefault=true`）不应该被删除/保存

**责任**: Services 层不强制保护，由 Application 层负责

**建议验证点**:
```csharp
// Application 层示例
if (preset.IsDefault)
{
    throw new InvalidOperationException("默认预设不能修改");
}
```

### 预设 ID 变化

**导入预设时**:
- 强制生成新 ID（避免冲突）
- 重置时间戳

**复制预设时**:
- `Preset.Clone()` 方法处理 ID 生成
- 名称自动添加 "(副本)" 后缀

## 配置导入导出

### 异常处理

**当前行为**: 所有文件操作异常被静默捕获，返回默认值

**优点**: 应用启动不会因配置损坏而崩溃

**缺点**: 用户无法感知配置加载失败

**建议**: 在 Application 层添加日志记录

```csharp
// 建议的增强实现
catch (Exception ex)
{
    // 添加日志
    Logger.LogError(ex, "配置加载失败，使用默认配置");
    return CreateDefaultConfig();
}
```

### 路径安全

**问题**: 导入/导出方法接受用户提供的文件路径

**风险**:
- 路径注入（如 `../../../system/file`）
- 路径遍历攻击

**建议**: 在 UI 层验证路径范围（如限制在用户目录）

## 内存管理

### 缓存清理

**问题**: 渲染缓存无限增长，无清理机制

**影响**: 长时间运行后内存占用增加

**当前缓解**: 配置变化通常有限，缓存增长可控

**建议添加**:
```csharp
// 添加定期清理或手动清理
public void ClearCache()
{
    _penCache.Clear();
    _brushCache.Clear();
    _geometryCache.Clear();
}
```

### 大对象处理

**自定义图片**:
- 大图片会占用大量内存
- 没有尺寸限制或压缩

**建议**: 在 UI 层限制图片大小，或自动缩放