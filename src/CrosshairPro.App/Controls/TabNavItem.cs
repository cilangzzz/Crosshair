using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 左侧导航栏图标项控件
/// 支持选中态、悬停态、左侧指示条
/// </summary>
public class TabNavItem : Button
{
    #region DependencyProperty

    /// <summary>图标路径几何数据（引用 IconGeometries.xaml 中的资源键名）</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(nameof(IconGeometry), typeof(string),
            typeof(TabNavItem), new PropertyMetadata(string.Empty, OnIconGeometryChanged));

    /// <summary>是否选中</summary>
    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool),
            typeof(TabNavItem), new PropertyMetadata(false, OnIsSelectedChanged));

    #endregion

    #region Properties

    /// <summary>
    /// 图标几何数据
    /// 引用 IconGeometries.xaml 中的资源键名（如 "Crosshair", "Gamepad"）
    /// </summary>
    public string IconGeometry
    {
        get => (string)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>是否选中</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion

    #region Fields

    private Path? _iconPath;
    private Border? _indicator;
    private Border? _backgroundBorder;

    #endregion

    static TabNavItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TabNavItem),
            new FrameworkPropertyMetadata(typeof(TabNavItem)));
    }

    public TabNavItem()
    {
        Width = 40;
        Height = 40;
        Cursor = Cursors.Hand;
    }

    private static void OnIconGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabNavItem item)
        {
            item.UpdateIconGeometry();
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabNavItem item)
        {
            item.UpdateVisualState();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _iconPath = GetTemplateChild("IconPath") as Path;
        _indicator = GetTemplateChild("Indicator") as Border;
        _backgroundBorder = GetTemplateChild("BackgroundBorder") as Border;

        UpdateIconGeometry();
        UpdateVisualState();
    }

    /// <summary>
    /// 更新图标几何数据
    /// </summary>
    private void UpdateIconGeometry()
    {
        if (_iconPath == null || string.IsNullOrEmpty(IconGeometry))
            return;

        // 尝试从 Application.Resources 获取
        try
        {
            if (System.Windows.Application.Current?.FindResource($"{IconGeometry}IconGeometry") is Geometry geometry)
            {
                _iconPath.Data = geometry;
            }
        }
        catch
        {
            // 资源未找到，忽略
        }
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        if (_indicator == null || _backgroundBorder == null || _iconPath == null)
            return;

        if (IsSelected)
        {
            _indicator.Background = (Brush)System.Windows.Application.Current.FindResource("AccentBrush");
            _backgroundBorder.Background = (Brush)System.Windows.Application.Current.FindResource("ControlBrush");
            _iconPath.SetBinding(Path.FillProperty, new System.Windows.Data.Binding { Source = System.Windows.Application.Current.FindResource("AccentBrush") });
        }
        else
        {
            _indicator.Background = Brushes.Transparent;
            _backgroundBorder.Background = Brushes.Transparent;
            _iconPath.SetBinding(Path.FillProperty, new System.Windows.Data.Binding { Source = System.Windows.Application.Current.FindResource("TextSecondaryBrush") });
        }
    }
}