using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.Views;

/// <summary>
/// 准心覆盖窗口 - 透明置顶窗口，使用 WPF Shape 元素渲染准心
/// </summary>
public sealed class OverlayWindow : Window
{
    private readonly Canvas _canvas;
    private readonly CrosshairConfig _config;
    private bool _isVisible = true;

    public event EventHandler? CrosshairVisibilityChanged;
    public bool IsCrosshairVisible => _isVisible;

    public OverlayWindow()
    {
        _config = new CrosshairConfig();
        _canvas = new Canvas { Background = Brushes.Transparent };

        // 窗口设置
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Focusable = false;
        IsHitTestVisible = false;

        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Title = "CrosshairPro Overlay";

        Content = _canvas;

        // 窗口加载完成后初始渲染
        Loaded += (s, e) => RenderCrosshair();

        // Config 内部任何属性变化 → 重绘
        _config.PropertyChanged += (s, e) => RenderCrosshair();
        _config.Effects.PropertyChanged += (s, e) => RenderCrosshair();
        _config.Effects.Outline.PropertyChanged += (s, e) => RenderCrosshair();
        _config.Effects.Shadow.PropertyChanged += (s, e) => RenderCrosshair();
    }

    /// <summary>
    /// 更新准心配置
    /// </summary>
    public void UpdateConfig(CrosshairConfig config)
    {
        _config.CopyFrom(config);
    }

    /// <summary>
    /// 显示/隐藏准心
    /// </summary>
    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        Visibility = _isVisible ? Visibility.Visible : Visibility.Hidden;
        CrosshairVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        RenderCrosshair();
    }

    /// <summary>
    /// 用 WPF Shape 元素绘制准心（分层窗口中可靠显示）
    /// </summary>
    private void RenderCrosshair()
    {
        _canvas.Children.Clear();

        if (!_isVisible) return;

        double cx = Width / 2;
        double cy = Height / 2;

        // 亮度：调整颜色明暗（100=原色，0=黑色，200=白色）
        var baseColor = (Color)ColorConverter.ConvertFromString(_config.Color);
        var color = ApplyBrightness(baseColor, _config.Brightness);
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        // 透明度：在每个 Shape 上单独设置（Canvas.Opacity 在 AllowsTransparency 窗口中会导致整个窗口不可见）
        double shapeOpacity = _config.Opacity / 100.0;

        var outlineBrush = Brushes.Black;
        bool hasOutline = _config.Effects.Outline.Enabled;

        double size = _config.Size;
        double gap = _config.Gap;
        double thick = _config.Thickness;
        double halfSize = size / 2;
        double halfGap = gap / 2;

        // 中心点
        if (_config.CenterSize > 0 &&
            _config.Style != CrosshairStyle.Dot)
        {
            AddDot(cx, cy, _config.CenterSize / 2.0, brush, hasOutline, outlineBrush, shapeOpacity);
        }

        switch (_config.Style)
        {
            case CrosshairStyle.Cross:
                AddLine(cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                break;

            case CrosshairStyle.Dot:
                AddDot(cx, cy, _config.CenterSize > 0 ? _config.CenterSize / 2.0 : 4, brush, hasOutline, outlineBrush, shapeOpacity);
                break;

            case CrosshairStyle.Circle:
                AddCircle(cx, cy, halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                break;

            case CrosshairStyle.TShape:
                AddLine(cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx - halfGap - halfSize, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                break;

            case CrosshairStyle.XShape:
                double off = halfGap * 0.707;
                double len = halfSize * 0.707;
                AddLine(cx - off, cy - off, cx - off - len, cy - off - len, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx + off, cy - off, cx + off + len, cy - off - len, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx - off, cy + off, cx - off - len, cy + off + len, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx + off, cy + off, cx + off + len, cy + off + len, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                break;

            case CrosshairStyle.CustomImage:
                AddLine(cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                AddLine(cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, shapeOpacity);
                break;
        }
    }

    private void AddLine(double x1, double y1, double x2, double y2, Brush brush, double thick, bool hasOutline, Brush outlineBrush, double opacity)
    {
        if (hasOutline)
        {
            _canvas.Children.Add(new Line
            {
                X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                Stroke = outlineBrush,
                StrokeThickness = thick + _config.Effects.Outline.Thickness * 2,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Flat,
                Opacity = opacity
            });
        }
        _canvas.Children.Add(new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = brush,
            StrokeThickness = thick,
            StrokeStartLineCap = PenLineCap.Flat,
            StrokeEndLineCap = PenLineCap.Flat,
            Opacity = opacity
        });
    }

    private void AddDot(double cx, double cy, double radius, Brush brush, bool hasOutline, Brush outlineBrush, double opacity)
    {
        if (hasOutline)
        {
            _canvas.Children.Add(new Ellipse
            {
                Width = radius * 2 + _config.Effects.Outline.Thickness * 2,
                Height = radius * 2 + _config.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = _config.Effects.Outline.Thickness,
                Margin = new Thickness(cx - radius - _config.Effects.Outline.Thickness,
                                       cy - radius - _config.Effects.Outline.Thickness, 0, 0),
                Opacity = opacity
            });
        }
        _canvas.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = brush,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0),
            Opacity = opacity
        });
    }

    private void AddCircle(double cx, double cy, double radius, Brush brush, double thick, bool hasOutline, Brush outlineBrush, double opacity)
    {
        if (hasOutline)
        {
            _canvas.Children.Add(new Ellipse
            {
                Width = radius * 2 + _config.Effects.Outline.Thickness * 2,
                Height = radius * 2 + _config.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = thick + _config.Effects.Outline.Thickness * 2,
                Margin = new Thickness(cx - radius - _config.Effects.Outline.Thickness,
                                       cy - radius - _config.Effects.Outline.Thickness, 0, 0),
                Opacity = opacity
            });
        }
        _canvas.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = brush,
            StrokeThickness = thick,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0),
            Opacity = opacity
        });
    }

    /// <summary>
    /// 应用亮度：100=原色，0=全黑，200=全白
    /// </summary>
    private static Color ApplyBrightness(Color color, int brightness)
    {
        double factor = brightness / 100.0;
        return Color.FromRgb(
            (byte)Math.Min(255, color.R * factor),
            (byte)Math.Min(255, color.G * factor),
            (byte)Math.Min(255, color.B * factor));
    }
}
