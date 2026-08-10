using System.Windows.Markup;

namespace CrosshairPro.App.Localization;

/// <summary>
/// XAML 本地化标记扩展
/// 用法：{loc:Loc Key=SomeKey}
/// 或简写：{loc:Loc SomeKey}
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
    /// <summary>
    /// 资源键名
    /// </summary>
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

        try
        {
            // 直接返回翻译文本
            var result = LocalizationProvider.Instance[Key];
            return result ?? Key;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LocExtension error for key '{Key}': {ex}");
            return Key;
        }
    }
}
