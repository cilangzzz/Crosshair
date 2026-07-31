using System.Windows;
using System.Windows.Controls;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// 游戏页面选择器
/// </summary>
public class GamePageSelector : DataTemplateSelector
{
    public DataTemplate? ApexTemplate { get; set; }
    public DataTemplate? DefaultTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is string gameId)
        {
            return gameId switch
            {
                "builtin-apex" => ApexTemplate,
                _ => DefaultTemplate
            };
        }
        return DefaultTemplate;
    }
}
