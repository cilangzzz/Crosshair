using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 图标按钮控件 - 支持内置图标和自定义图标路径
/// 使用 Path Geometry 渲染图标，轻量级无外部依赖
/// </summary>
public class IconButton : Button
{
    #region DependencyProperty

    /// <summary>图标路径几何数据（支持内置图标名称或自定义 Path Data）</summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(nameof(IconGeometry), typeof(string),
            typeof(IconButton), new PropertyMetadata(string.Empty, OnIconGeometryChanged));

    /// <summary>图标大小</summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double),
            typeof(IconButton), new PropertyMetadata(16.0));

    /// <summary>图标颜色（默认使用 Foreground）</summary>
    public static readonly DependencyProperty IconColorProperty =
        DependencyProperty.Register(nameof(IconColor), typeof(Color?),
            typeof(IconButton), new PropertyMetadata(null));

    /// <summary>图标相对于文字的位置</summary>
    public static readonly DependencyProperty IconPositionProperty =
        DependencyProperty.Register(nameof(IconPosition), typeof(IconPosition),
            typeof(IconButton), new PropertyMetadata(IconPosition.Left));

    /// <summary>图标与文字之间的间距</summary>
    public static readonly DependencyProperty IconSpacingProperty =
        DependencyProperty.Register(nameof(IconSpacing), typeof(double),
            typeof(IconButton), new PropertyMetadata(8.0));

    /// <summary>是否只显示图标（无文字）</summary>
    public static readonly DependencyProperty ShowIconOnlyProperty =
        DependencyProperty.Register(nameof(ShowIconOnly), typeof(bool),
            typeof(IconButton), new PropertyMetadata(false));

    #endregion

    #region Properties

    /// <summary>
    /// 图标几何数据
    /// 可以是：
    /// 1. 内置图标名称（如 "Close", "Settings", "Save" 等）
    /// 2. 自定义 Path Data 字符串（如 "M 6,6 L 18,18 M 6,18 L 18,6"）
    /// </summary>
    public string IconGeometry
    {
        get => (string)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    /// <summary>图标大小（像素）</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>图标颜色（可选，默认使用 Foreground）</summary>
    public Color? IconColor
    {
        get => (Color?)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    /// <summary>图标位置</summary>
    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
    }

    /// <summary>图标与文字间距</summary>
    public double IconSpacing
    {
        get => (double)GetValue(IconSpacingProperty);
        set => SetValue(IconSpacingProperty, value);
    }

    /// <summary>是否只显示图标</summary>
    public bool ShowIconOnly
    {
        get => (bool)GetValue(ShowIconOnlyProperty);
        set => SetValue(ShowIconOnlyProperty, value);
    }

    #endregion

    #region Fields

    private Path? _iconPath;

    #endregion

    static IconButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton),
            new FrameworkPropertyMetadata(typeof(IconButton)));
    }

    public IconButton()
    {
        // 默认设置
        Padding = new Thickness(8, 6, 8, 6);
        Cursor = System.Windows.Input.Cursors.Hand;
    }

    private static void OnIconGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IconButton button)
        {
            button.UpdateIconGeometry();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _iconPath = GetTemplateChild("IconPath") as Path;
        UpdateIconGeometry();
    }

    /// <summary>
    /// 更新图标几何数据
    /// </summary>
    private void UpdateIconGeometry()
    {
        if (_iconPath == null || string.IsNullOrEmpty(IconGeometry))
            return;

        // 检查是否是内置图标名称
        var geometryData = GetIconGeometryData(IconGeometry);

        if (!string.IsNullOrEmpty(geometryData))
        {
            try
            {
                var geometry = Geometry.Parse(geometryData);
                _iconPath.Data = geometry;
            }
            catch
            {
                // 如果解析失败，尝试直接解析用户输入的 Path Data
                try
                {
                    var geometry = Geometry.Parse(IconGeometry);
                    _iconPath.Data = geometry;
                }
                catch
                {
                    // 忽略无效的几何数据
                }
            }
        }

        // 设置图标颜色
        if (IconColor.HasValue)
        {
            _iconPath.Stroke = new SolidColorBrush(IconColor.Value);
            _iconPath.Fill = new SolidColorBrush(IconColor.Value);
        }
        else
        {
            _iconPath.SetBinding(Path.StrokeProperty, new System.Windows.Data.Binding("Foreground") { Source = this });
            _iconPath.SetBinding(Path.FillProperty, new System.Windows.Data.Binding("Foreground") { Source = this });
        }

        // 设置图标大小
        _iconPath.Width = IconSize;
        _iconPath.Height = IconSize;
    }

    /// <summary>
    /// 获取内置图标几何数据
    /// </summary>
    private static string GetIconGeometryData(string iconKey)
    {
        // 尝试从 Application.Resources 获取
        try
        {
            if (System.Windows.Application.Current?.FindResource($"{iconKey}IconGeometry") is Geometry geometry)
            {
                return geometry.ToString();
            }
        }
        catch
        {
            // 资源未找到，返回 null 表示需要直接解析用户输入
        }

        return null;
    }
}

/// <summary>
/// 图标位置枚举
/// </summary>
public enum IconPosition
{
    /// <summary>图标在文字左侧</summary>
    Left,

    /// <summary>图标在文字右侧</summary>
    Right,

    /// <summary>图标在文字上方</summary>
    Top,

    /// <summary>图标在文字下方</summary>
    Bottom
}