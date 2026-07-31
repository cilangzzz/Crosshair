using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App;

/// <summary>
/// 配置项类型转可见性转换器
/// 用于根据 ConfigItemType 显示/隐藏对应的控件
/// </summary>
public class ConfigTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ConfigItemType type && parameter is string expectedType)
        {
            if (Enum.TryParse<ConfigItemType>(expectedType, out var expected))
            {
                return type == expected ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}