using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 准心预览控件
/// </summary>
public class CrosshairPreview : Control
{
    private CrosshairConfig? _config;

    static CrosshairPreview()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CrosshairPreview),
            new FrameworkPropertyMetadata(typeof(CrosshairPreview)));
    }

    public static readonly DependencyProperty ConfigProperty =
        DependencyProperty.Register(nameof(Config), typeof(CrosshairConfig),
            typeof(CrosshairPreview), new PropertyMetadata(null, OnConfigChanged));

    public CrosshairConfig? Config
    {
        get => (CrosshairConfig?)GetValue(ConfigProperty);
        set => SetValue(ConfigProperty, value);
    }

    private static void OnConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CrosshairPreview preview)
        {
            preview._config = e.NewValue as CrosshairConfig;
            preview.InvalidateVisual();
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        // 绘制背景网格
        DrawGrid(dc);

        if (_config == null)
            return;

        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        // 根据样式渲染
        switch (_config.Style)
        {
            case CrosshairStyle.Cross:
                RenderCross(dc, center, _config);
                break;
            case CrosshairStyle.Dot:
                RenderDot(dc, center, _config);
                break;
            case CrosshairStyle.Circle:
                RenderCircle(dc, center, _config);
                break;
            case CrosshairStyle.TShape:
                RenderTShape(dc, center, _config);
                break;
            case CrosshairStyle.XShape:
                RenderXShape(dc, center, _config);
                break;
        }
    }

    private void DrawGrid(DrawingContext dc)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(60, 60, 80)), 0.5);
        gridPen.Freeze();

        // 垂直线
        for (double x = 0; x < ActualWidth; x += 20)
        {
            dc.DrawLine(gridPen, new Point(x, 0), new Point(x, ActualHeight));
        }

        // 水平线
        for (double y = 0; y < ActualHeight; y += 20)
        {
            dc.DrawLine(gridPen, new Point(0, y), new Point(ActualWidth, y));
        }

        // 中心十字线
        var centerPen = new Pen(new SolidColorBrush(Color.FromRgb(80, 80, 100)), 1);
        centerPen.Freeze();

        dc.DrawLine(centerPen, new Point(ActualWidth / 2, 0), new Point(ActualWidth / 2, ActualHeight));
        dc.DrawLine(centerPen, new Point(0, ActualHeight / 2), new Point(ActualWidth, ActualHeight / 2));
    }

    private void RenderCross(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = CreateBrush(config.Color, config.Opacity);
        var pen = new Pen(brush, config.Thickness);
        pen.Freeze();

        var halfLength = config.Size * 1.5; // 预览时放大
        var halfGap = config.Gap * 1.5;

        // 描边
        if (config.Effects.Outline.Enabled)
        {
            var outlineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.Effects.Outline.Color));
            var outlinePen = new Pen(outlineBrush, config.Thickness + config.Effects.Outline.Thickness * 2);
            outlinePen.Freeze();

            dc.DrawLine(outlinePen, new Point(center.X, center.Y - halfGap), new Point(center.X, center.Y - halfGap - halfLength));
            dc.DrawLine(outlinePen, new Point(center.X, center.Y + halfGap), new Point(center.X, center.Y + halfGap + halfLength));
            dc.DrawLine(outlinePen, new Point(center.X - halfGap, center.Y), new Point(center.X - halfGap - halfLength, center.Y));
            dc.DrawLine(outlinePen, new Point(center.X + halfGap, center.Y), new Point(center.X + halfGap + halfLength, center.Y));
        }

        // 四条线
        dc.DrawLine(pen, new Point(center.X, center.Y - halfGap), new Point(center.X, center.Y - halfGap - halfLength));
        dc.DrawLine(pen, new Point(center.X, center.Y + halfGap), new Point(center.X, center.Y + halfGap + halfLength));
        dc.DrawLine(pen, new Point(center.X - halfGap, center.Y), new Point(center.X - halfGap - halfLength, center.Y));
        dc.DrawLine(pen, new Point(center.X + halfGap, center.Y), new Point(center.X + halfGap + halfLength, center.Y));
    }

    private void RenderDot(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = CreateBrush(config.Color, config.Opacity);
        var radius = config.CenterSize * 1.5;

        if (config.Effects.Outline.Enabled)
        {
            var outlineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.Effects.Outline.Color));
            var outlinePen = new Pen(outlineBrush, config.Effects.Outline.Thickness * 2);
            outlinePen.Freeze();
            dc.DrawEllipse(null, outlinePen, center, radius + config.Effects.Outline.Thickness, radius + config.Effects.Outline.Thickness);
        }

        dc.DrawEllipse(brush, null, center, radius, radius);
    }

    private void RenderCircle(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = CreateBrush(config.Color, config.Opacity);
        var pen = new Pen(brush, config.Thickness);
        pen.Freeze();

        var radius = config.Size * 1.5;

        if (config.Effects.Outline.Enabled)
        {
            var outlineBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.Effects.Outline.Color));
            var outlinePen = new Pen(outlineBrush, config.Effects.Outline.Thickness * 2);
            outlinePen.Freeze();
            dc.DrawEllipse(null, outlinePen, center, radius + config.Effects.Outline.Thickness, radius + config.Effects.Outline.Thickness);
        }

        dc.DrawEllipse(null, pen, center, radius, radius);

        // 中心点
        if (config.CenterSize > 0)
        {
            var dotRadius = config.CenterSize * 0.75;
            dc.DrawEllipse(brush, null, center, dotRadius, dotRadius);
        }
    }

    private void RenderTShape(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = CreateBrush(config.Color, config.Opacity);
        var pen = new Pen(brush, config.Thickness);
        pen.Freeze();

        var halfLength = config.Size * 1.5;
        var halfGap = config.Gap * 1.5;

        // 上线
        dc.DrawLine(pen, new Point(center.X, center.Y - halfGap), new Point(center.X, center.Y - halfGap - halfLength));
        // 横线
        dc.DrawLine(pen, new Point(center.X - halfLength, center.Y), new Point(center.X + halfLength, center.Y));
    }

    private void RenderXShape(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = CreateBrush(config.Color, config.Opacity);
        var pen = new Pen(brush, config.Thickness);
        pen.Freeze();

        var halfLength = config.Size * 1.5;
        var halfGap = config.Gap * 1.5;
        var offset = halfGap * 0.7; // 对角线偏移

        // 四条对角线
        dc.DrawLine(pen, new Point(center.X - offset, center.Y - offset), new Point(center.X - offset - halfLength, center.Y - offset - halfLength));
        dc.DrawLine(pen, new Point(center.X + offset, center.Y - offset), new Point(center.X + offset + halfLength, center.Y - offset - halfLength));
        dc.DrawLine(pen, new Point(center.X - offset, center.Y + offset), new Point(center.X - offset - halfLength, center.Y + offset + halfLength));
        dc.DrawLine(pen, new Point(center.X + offset, center.Y + offset), new Point(center.X + offset + halfLength, center.Y + offset + halfLength));
    }

    private Brush CreateBrush(string colorHex, double opacity)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);
        var brush = new SolidColorBrush(color);
        brush.Opacity = opacity / 100.0;
        brush.Freeze();
        return brush;
    }
}
