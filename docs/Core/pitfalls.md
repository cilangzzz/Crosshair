# CrosshairPro.Core 坑点

## 1. 深拷贝陷阱

**问题**: `CrosshairConfig` 包含嵌套对象（`EffectsConfig`, `DisplayConfig`），简单的浅拷贝会导致引用共享。

**错误做法**:
```csharp
var copy = new CrosshairConfig
{
    Name = original.Name,
    Effects = original.Effects,  // ❌ 引用共享！
    Display = original.Display   // ❌ 引用共享！
};
```

**正确做法**:
```csharp
var copy = original.Clone();  // ✅ 使用 Clone() 方法
```

**影响范围**: 
- `CrosshairConfig.Clone()`
- `Preset.Clone()`
- `ConfigurationService.CloneConfig()`

## 2. ObservableProperty 自动生成

**问题**: 使用 `[ObservableProperty]` 特性时，字段命名必须以下划线开头，属性名会自动生成。

**错误做法**:
```csharp
[ObservableProperty]
private int Size;  // ❌ 不以下划线开头
```

**正确做法**:
```csharp
[ObservableProperty]
private int _size;  // ✅ 正确命名，自动生成 Size 属性
```

**注意**: 生成的属性名是 `_size` → `Size`（去掉下划线，首字母大写）

## 3. 颜色格式约束

**问题**: 颜色属性使用 `string` 类型存储十六进制值，但没有编译时验证。

**风险**:
- 空字符串会导致解析失败
- 格式错误（如 `#GGG`）会导致运行时异常
- 缺少 alpha 通道支持

**建议**: 在 UI 层添加颜色格式验证，或在模型中使用 `System.Windows.Media.Color` 类型。

## 4. Preset.IsDefault 保护

**问题**: 默认预设（`IsDefault=true`）不应被删除或修改，但模型层没有强制约束。

**潜在问题**:
- 用户可能误删默认预设
- 修改默认预设会影响所有使用该预设的配置

**解决方案**: 在 `PresetService` 层添加保护逻辑：
```csharp
if (preset.IsDefault)
{
    throw new InvalidOperationException("默认预设不能被删除或修改");
}
```

## 5. HotkeyBinding 字符串格式

**问题**: `HotkeyBinding.Modifiers` 使用字符串数组，但没有标准化的格式。

**潜在问题**:
- 大小写不一致（"Ctrl" vs "ctrl"）
- 修饰键顺序不确定
- 解析时需要处理多种格式

**建议**: 定义枚举或常量：
```csharp
public static class ModifierKeys
{
    public const string Ctrl = "Ctrl";
    public const string Alt = "Alt";
    public const string Shift = "Shift";
}
```

## 6. GameAssociation 匹配逻辑

**问题**: `Preset.GameAssociation` 存储游戏进程名，但没有定义匹配规则。

**潜在问题**:
- 进程名大小写敏感问题
- 部分匹配 vs 完全匹配
- 多个预设关联同一游戏时的优先级

**建议**: 在游戏检测逻辑中明确定义匹配规则：
```csharp
// 建议使用不区分大小写的完全匹配
preset.GameAssociation?.Equals(processName, StringComparison.OrdinalIgnoreCase)
```

## 7. 配置版本兼容性

**问题**: 配置模型可能随版本更新而变化，但没有版本字段。

**风险**:
- 旧版本配置文件无法正确加载
- 缺失字段使用默认值可能导致用户配置丢失

**建议**: 添加版本字段和迁移逻辑：
```csharp
public class CrosshairConfig
{
    public int Version { get; set; } = 1;
    // ...
}

// 在加载时检查版本并迁移
if (config.Version < 2)
{
    MigrateV1ToV2(config);
}
```

## 8. JSON 序列化陷阱

**问题**: 使用 `System.Text.Json` 序列化 ObservableObject 派生类时，可能遇到循环引用或属性忽略问题。

**注意**:
- `[ObservableProperty]` 生成的属性是普通的读写属性，可以正常序列化
- 但如果添加了 `[JsonIgnore]` 特性，需要手动添加到生成的属性上
- 循环引用需要在 `JsonSerializerOptions` 中配置 `ReferenceHandler`

## 9. 空值处理

**问题**: 部分可空属性（如 `CustomImagePath`）可能为空字符串或 null，语义不明确。

**建议**: 统一使用 null 表示"未设置"，空字符串应该被规范化为 null。

## 10. Id 生成策略

**问题**: `CrosshairConfig.Id` 和 `Preset.Id` 使用 `Guid.NewGuid().ToString()`，但 Clone 方法会生成新 ID。

**注意**:
- `Clone()` 会生成新 ID（适合创建副本）
- `CopyFrom()` 保持 ID 不变（适合更新现有配置）

**使用场景区分**:
- 创建新配置：使用 `Clone()`
- 更新现有配置：使用 `CopyFrom()`