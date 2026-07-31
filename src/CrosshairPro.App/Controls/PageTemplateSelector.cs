using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 页面模板选择器
/// 根据 PageType 选择对应的 DataTemplate
/// </summary>
public class PageTemplateSelector : DataTemplateSelector
{
    /// <summary>准心页面模板</summary>
    public DataTemplate? CrosshairTemplate { get; set; }

    /// <summary>游戏页面模板</summary>
    public DataTemplate? GamesTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is PageType pageType)
        {
            return pageType switch
            {
                PageType.Crosshair => CrosshairTemplate,
                PageType.Games => GamesTemplate,
                _ => null
            };
        }

        return base.SelectTemplate(item, container);
    }
}