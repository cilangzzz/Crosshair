using System.Windows.Data;
using System.Windows.Markup;

namespace CrosshairPro.App.Localization;

/// <summary>
/// XAML 本地化标记扩展
/// 用法：{loc:Loc Key=SomeKey} 或 {loc:Loc SomeKey}
/// 通过 Binding 索引器路径 [{Key}] 绑定到 LocalizationProvider，切换语言时自动更新
/// </summary>
[MarkupExtensionReturnType(typeof(BindingExpression))]
public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public LocExtension() { }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        // 使用 Binding 索引器路径：[Key] 等价于 LocalizationProvider.this[Key]
        // 当 LocalizationProvider 触发 "Item[]" PropertyChanged 时，所有绑定自动更新
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationProvider.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}