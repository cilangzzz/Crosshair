using System.Windows;
using System.Windows.Controls;

namespace CrosshairPro.App.Controls;

/// <summary>
/// ScrollViewer 行为辅助类
/// 提供附加属性支持 ScrollViewer 的垂直偏移量动画
/// </summary>
public static class ScrollViewerBehavior
{
    #region Attached Properties

    /// <summary>
    /// 垂直偏移量附加属性
    /// 用于支持 ScrollViewer 垂直滚动的动画
    /// </summary>
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(ScrollViewerBehavior),
            new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

    #endregion

    #region Attached Property Accessors

    /// <summary>获取垂直偏移量</summary>
    public static double GetVerticalOffset(DependencyObject obj)
    {
        return (double)obj.GetValue(VerticalOffsetProperty);
    }

    /// <summary>设置垂直偏移量</summary>
    public static void SetVerticalOffset(DependencyObject obj, double value)
    {
        obj.SetValue(VerticalOffsetProperty, value);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 当垂直偏移量改变时调用
    /// 将值同步到 ScrollViewer 的实际滚动位置
    /// </summary>
    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is System.Windows.Controls.ScrollViewer scrollViewer)
        {
            // 使用 ScrollToVerticalOffset 方法设置滚动位置
            // 这比直接设置 VerticalOffset 属性更可靠
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }

    #endregion
}
