using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CrosshairPro.App;

/// <summary>
/// 字符串转可见性转换器
/// 非空字符串显示，空字符串或 null 隐藏
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        var str = value.ToString();
        return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}