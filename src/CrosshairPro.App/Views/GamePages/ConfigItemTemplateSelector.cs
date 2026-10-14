using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.ViewModels;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// 配置项模板选择器：按 <see cref="ConfigItemType"/> 选择统一的行模板
/// （开关 → Bool / 滑杆+数值框 → Int / 下拉框 → Enum / 文本框 → String），
/// 保证配置项行排版与控件风格统一，并避免硬编码 Sections[x].Items[y] 索引绑定
/// </summary>
public class ConfigItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? BoolTemplate { get; set; }
    public DataTemplate? IntTemplate { get; set; }
    public DataTemplate? EnumTemplate { get; set; }
    public DataTemplate? StringTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is ConfigItemWrapper wrapper)
        {
            return wrapper.Type switch
            {
                ConfigItemType.Bool => BoolTemplate,
                ConfigItemType.Int => IntTemplate,
                ConfigItemType.Enum => EnumTemplate,
                ConfigItemType.String => StringTemplate,
                _ => null
            };
        }
        return null;
    }
}
