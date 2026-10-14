using System.Globalization;
using System.Windows.Data;
using CrosshairPro.App.Localization;

namespace CrosshairPro.App;

/// <summary>
/// 游戏 tab 标题转换器：DisplayNameLocKey 非空时取本地化文本，否则回退 DisplayName。
/// MultiBinding 顺序：[0]=DisplayName（回退值）、[1]=DisplayNameLocKey、[2]=CurrentCulture（仅用于语言切换时触发重算）
/// </summary>
public sealed class GameProfileTitleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var displayName = values.Length > 0 ? values[0] as string : null;
        var locKey = values.Length > 1 ? values[1] as string : null;

        if (!string.IsNullOrEmpty(locKey))
        {
            // LocalizationProvider.Get 缺失键时返回 key 本身，不会返回 null
            return LocalizationProvider.Get(locKey);
        }
        return displayName ?? string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
