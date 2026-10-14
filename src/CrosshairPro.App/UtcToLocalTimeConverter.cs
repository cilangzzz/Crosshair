using System;
using System.Globalization;
using System.Windows.Data;

namespace CrosshairPro.App;

/// <summary>
/// UTC 时间到本地时间字符串转换器
/// 用于预设列表展示创建时间（yyyy-MM-dd HH:mm）
/// </summary>
public class UtcToLocalTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime utc && utc != DateTime.MinValue)
        {
            return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm", culture);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
