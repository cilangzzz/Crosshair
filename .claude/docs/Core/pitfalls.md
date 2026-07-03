# CrosshairPro.Core - 坑点

## 1. CrosshairConfig 深拷贝必须递归克隆

`Clone()` 方法必须递归克隆所有嵌套对象：

```csharp
// ❌ 错误：浅拷贝，Effects 和 Display 仍引用原对象
return new CrosshairConfig { Effects = config.Effects };

// ✅ 正确：深拷贝
return new CrosshairConfig
{
    Effects = new EffectsConfig
    {
        Outline = new OutlineConfig { ... },
        Shadow = new ShadowConfig { ... },
        Glow = new GlowConfig { ... }
    }
};
```

## 2. CopyFrom vs Clone 的区别

| 方法 | 用途 | 返回值 | Id 处理 |
|------|------|--------|---------|
| Clone() | 创建独立副本 | 新实例 | 生成新 GUID |
| CopyFrom() | 复制值到当前实例 | 无（修改当前） | 保持原 Id |

## 3. 嵌套对象属性变更通知

`CrosshairConfig` 的属性变更会自动通知，但嵌套对象（Effects、Display）的变更需要手动订阅：

```csharp
// MainViewModel 中的订阅示例
config.PropertyChanged += (s, e) => { ... };
config.Effects.PropertyChanged += (s, e) => { ... };
config.Effects.Outline.PropertyChanged += (s, e) => { ... };
```

## 4. 颜色格式

颜色使用十六进制字符串格式（如 `"#00FF00"`），转换时使用 `ColorConverter.ConvertFromString()`：

```csharp
var color = (Color)ColorConverter.ConvertFromString("#00FF00");
```

无效格式会抛出异常，需要在 UI 层验证。

## 5. 亮度调整范围

`ApplyBrightness()` 方法中，factor > 1 时颜色值会超过 255，已使用 `Math.Min(255, ...)` 限制：

```csharp
return Color.FromRgb(
    (byte)Math.Min(255, color.R * factor),
    (byte)Math.Min(255, color.G * factor),
    (byte)Math.Min(255, color.B * factor));
```

factor = 0 时颜色变为纯黑，factor = 200 时接近纯白。

## 6. CrosshairStyle 枚举索引

枚举值与 UI 下拉框索引对应：
- `Cross = 0` → "十字准心"
- `Dot = 1` → "点状准心"
- ...

修改 `SelectedStyleIndex` 时需要检查边界：

```csharp
partial void OnSelectedStyleIndexChanged(int value)
{
    if (value >= 0 && value < Enum.GetValues(typeof(CrosshairStyle)).Length)
        Config.Style = (CrosshairStyle)value;
}
```

## 7. 预设的 IsDefault 标记

默认预设（Id = "default", IsDefault = true）不可删除，删除前需要检查：

```csharp
if (preset == null || preset.IsDefault) return;
await _presetRepo.DeletePresetAsync(preset.Id);
```

## 8. 接口命名约定

所有接口以 `I` 开头，如 `IConfigRepository`、`ICrosshairRenderer`。
实现类通常不加前缀，如 `JsonConfigRepository`、`CrosshairRenderer`。