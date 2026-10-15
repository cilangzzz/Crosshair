using System.Globalization;
using System.Windows.Data;
using CrosshairPro.App.Localization;

namespace CrosshairPro.App;

/// <summary>
/// 本地化文本转换器：按本地化键动态取词（键名在 ViewModel 中定义，无法用 {loc:Loc} 字面量表达）。
/// MultiBinding 顺序：[0]=本地化键、[1]=CurrentCulture（仅用于语言切换时触发重算）
/// </summary>
public sealed class LocalizedTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var locKey = values.Length > 0 ? values[0] as string : null;
        if (string.IsNullOrEmpty(locKey))
        {
            return string.Empty;
        }
        // LocalizationProvider.Get 缺失键时返回 key 本身，不会返回 null
        return LocalizationProvider.Get(locKey);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
