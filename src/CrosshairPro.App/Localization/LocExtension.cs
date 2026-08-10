using System.Windows.Data;
using System.Windows.Markup;

namespace CrosshairPro.App.Localization;

/// <summary>
/// XAML 本地化标记扩展
/// 用法：{loc:Loc Key=SomeKey}
/// 或简写：{loc:Loc SomeKey}
/// 支持动态更新（语言切换时自动刷新 UI）
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
    /// <summary>
    /// 资源键名
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 格式化参数（可选），用于 string.Format
    /// </summary>
    public object[]? Args { get; set; }

    public LocExtension() { }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        // 创建一个 Binding 到 LocalizationProvider.Instance
        // 使用自定义的索引器路径实现动态更新
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationProvider.Instance,
            Mode = BindingMode.OneWay,
            FallbackValue = Key
        };

        return binding.ProvideValue(serviceProvider);
    }
}
