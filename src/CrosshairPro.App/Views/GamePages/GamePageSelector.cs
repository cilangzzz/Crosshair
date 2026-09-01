using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// 游戏页面选择器
/// </summary>
public class GamePageSelector : DataTemplateSelector
{
    public DataTemplate? ApexTemplate { get; set; }
    public DataTemplate? Cs2Template { get; set; }
    public DataTemplate? DefaultTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is string gameId)
        {
            return gameId switch
            {
                "builtin-apex" => ApexTemplate,
                "builtin-cs2" => Cs2Template,
                _ => DefaultTemplate
            };
        }
        return DefaultTemplate;
    }
}