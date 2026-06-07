using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Services.Crosshair;

/// <summary>
/// 准心渲染器 - 核心渲染引擎
/// </summary>
public sealed class CrosshairRenderer : ICrosshairRenderer
{
    private readonly Dictionary<string, Pen> _penCache = new();
    private readonly Dictionary<string, Brush> _brushCache = new();
    private readonly Dictionary<string, Geometry> _geometryCache = new();

    public event EventHandler<RenderCompletedEventArgs>? RenderCompleted;

    /// <summary>
    /// 渲染准心
    /// </summary>
    public void Render(object drawingContext, CrosshairConfig config, double width, double height)
    {
        var dc = (DrawingContext)drawingContext;
        var center = new Point(width / 2, height / 2);

        // 应用位置偏移
        center = new Point(
            center.X + config.Display.PositionX,
            center.Y + config.Display.PositionY);

        // 应用旋转
        if (config.Rotation != 0)
        {
            var rotateTransform = new RotateTransform(config.Rotation, center.X, center.Y);
            dc.PushTransform(rotateTransform);
        }

        // 根据样式渲染
        switch (config.Style)
        {
            case CrosshairStyle.Cross:
                RenderCross(dc, center, config);
                break;
            case CrosshairStyle.Dot:
                RenderDot(dc, center, config);
                break;
            case CrosshairStyle.Circle:
                RenderCircle(dc, center, config);
                break;
            case CrosshairStyle.TShape:
                RenderTShape(dc, center, config);
                break;
            case CrosshairStyle.XShape:
                RenderXShape(dc, center, config);
                break;
            case CrosshairStyle.CustomImage:
                RenderCustomImage(dc, center, config, width, height);
                break;
        }

        // 恢复变换
        if (config.Rotation != 0)
        {
            dc.Pop();
        }

        RenderCompleted?.Invoke(this, new RenderCompletedEventArgs
        {
            Success = true,
            RenderTimeMs = 0
        });
    }

    /// <summary>
    /// 渲染十字准心
    /// </summary>
    private void RenderCross(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var pen = GetOrCreatePen(config.Color, config.Thickness, config.Opacity);
        var halfLength = config.Size / 2.0;
        var halfGap = config.Gap / 2.0;

        // 四条线：上、下、左、右
        var lines = new (Point Start, Point End)[]
        {
            (new Point(center.X, center.Y - halfGap), new Point(center.X, center.Y - halfGap - halfLength)), // 上
            (new Point(center.X, center.Y + halfGap), new Point(center.X, center.Y + halfGap + halfLength)), // 下
            (new Point(center.X - halfGap, center.Y), new Point(center.X - halfGap - halfLength, center.Y)), // 左
            (new Point(center.X + halfGap, center.Y), new Point(center.X + halfGap + halfLength, center.Y))  // 右
        };

        foreach (var (start, end) in lines)
        {
            // 绘制阴影
            if (config.Effects.Shadow.Enabled)
            {
                var shadowPen = GetOrCreatePen(config.Effects.Shadow.Color, config.Thickness, config.Opacity * 0.5);
                var offset = new Vector(config.Effects.Shadow.OffsetX, config.Effects.Shadow.OffsetY);
                dc.DrawLine(shadowPen, start + offset, end + offset);
            }

            // 绘制描边
            if (config.Effects.Outline.Enabled)
            {
                var outlinePen = GetOrCreatePen(config.Effects.Outline.Color, config.Thickness + config.Effects.Outline.Thickness * 2, config.Opacity);
                dc.DrawLine(outlinePen, start, end);
            }

            // 绘制主线
            dc.DrawLine(pen, start, end);
        }
    }

    /// <summary>
    /// 渲染点状准心
    /// </summary>
    private void RenderDot(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var brush = GetOrCreateBrush(config.Color, config.Opacity);
        var radius = config.CenterSize / 2.0;

        // 阴影
        if (config.Effects.Shadow.Enabled)
        {
            var shadowBrush = GetOrCreateBrush(config.Effects.Shadow.Color, config.Opacity * 0.5);
            var offset = new Vector(config.Effects.Shadow.OffsetX, config.Effects.Shadow.OffsetY);
            dc.DrawEllipse(shadowBrush, null, center + offset, radius, radius);
        }

        // 描边
        if (config.Effects.Outline.Enabled)
        {
            var outlinePen = GetOrCreatePen(config.Effects.Outline.Color, config.Effects.Outline.Thickness, config.Opacity);
            dc.DrawEllipse(null, outlinePen, center, radius + config.Effects.Outline.Thickness, radius + config.Effects.Outline.Thickness);
        }

        // 填充
        dc.DrawEllipse(brush, null, center, radius, radius);
    }

    /// <summary>
    /// 渲染圆形准心
    /// </summary>
    private void RenderCircle(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var pen = GetOrCreatePen(config.Color, config.Thickness, config.Opacity);
        var radius = config.Size / 2.0;

        // 阴影
        if (config.Effects.Shadow.Enabled)
        {
            var shadowPen = GetOrCreatePen(config.Effects.Shadow.Color, config.Thickness, config.Opacity * 0.5);
            var offset = new Vector(config.Effects.Shadow.OffsetX, config.Effects.Shadow.OffsetY);
            dc.DrawEllipse(null, shadowPen, center + offset, radius, radius);
        }

        // 描边
        if (config.Effects.Outline.Enabled)
        {
            var outlinePen = GetOrCreatePen(config.Effects.Outline.Color, config.Thickness + config.Effects.Outline.Thickness * 2, config.Opacity);
            dc.DrawEllipse(null, outlinePen, center, radius + config.Effects.Outline.Thickness, radius + config.Effects.Outline.Thickness);
        }

        // 圆圈
        dc.DrawEllipse(null, pen, center, radius, radius);

        // 中心点（如果设置了）
        if (config.CenterSize > 0)
        {
            var dotBrush = GetOrCreateBrush(config.Color, config.Opacity);
            var dotRadius = config.CenterSize / 2.0;
            dc.DrawEllipse(dotBrush, null, center, dotRadius, dotRadius);
        }
    }

    /// <summary>
    /// 渲染T形准心
    /// </summary>
    private void RenderTShape(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var pen = GetOrCreatePen(config.Color, config.Thickness, config.Opacity);
        var halfLength = config.Size / 2.0;
        var halfGap = config.Gap / 2.0;

        // 三条线：上、左、右
        var lines = new (Point Start, Point End)[]
        {
            (new Point(center.X, center.Y - halfGap), new Point(center.X, center.Y - halfGap - halfLength)), // 上
            (new Point(center.X - halfGap - halfLength, center.Y), new Point(center.X + halfGap + halfLength, center.Y)) // 横线
        };

        foreach (var (start, end) in lines)
        {
            if (config.Effects.Shadow.Enabled)
            {
                var shadowPen = GetOrCreatePen(config.Effects.Shadow.Color, config.Thickness, config.Opacity * 0.5);
                var offset = new Vector(config.Effects.Shadow.OffsetX, config.Effects.Shadow.OffsetY);
                dc.DrawLine(shadowPen, start + offset, end + offset);
            }

            if (config.Effects.Outline.Enabled)
            {
                var outlinePen = GetOrCreatePen(config.Effects.Outline.Color, config.Thickness + config.Effects.Outline.Thickness * 2, config.Opacity);
                dc.DrawLine(outlinePen, start, end);
            }

            dc.DrawLine(pen, start, end);
        }
    }

    /// <summary>
    /// 渲染X形准心
    /// </summary>
    private void RenderXShape(DrawingContext dc, Point center, CrosshairConfig config)
    {
        var pen = GetOrCreatePen(config.Color, config.Thickness, config.Opacity);
        var halfLength = config.Size / 2.0;
        var halfGap = config.Gap / 2.0;

        // 四条对角线
        var lines = new (Point Start, Point End)[]
        {
            // 左上到中心
            (new Point(center.X - halfGap, center.Y - halfGap),
             new Point(center.X - halfGap - halfLength, center.Y - halfGap - halfLength)),
            // 右上到中心
            (new Point(center.X + halfGap, center.Y - halfGap),
             new Point(center.X + halfGap + halfLength, center.Y - halfGap - halfLength)),
            // 左下到中心
            (new Point(center.X - halfGap, center.Y + halfGap),
             new Point(center.X - halfGap - halfLength, center.Y + halfGap + halfLength)),
            // 右下到中心
            (new Point(center.X + halfGap, center.Y + halfGap),
             new Point(center.X + halfGap + halfLength, center.Y + halfGap + halfLength))
        };

        foreach (var (start, end) in lines)
        {
            if (config.Effects.Shadow.Enabled)
            {
                var shadowPen = GetOrCreatePen(config.Effects.Shadow.Color, config.Thickness, config.Opacity * 0.5);
                var offset = new Vector(config.Effects.Shadow.OffsetX, config.Effects.Shadow.OffsetY);
                dc.DrawLine(shadowPen, start + offset, end + offset);
            }

            if (config.Effects.Outline.Enabled)
            {
                var outlinePen = GetOrCreatePen(config.Effects.Outline.Color, config.Thickness + config.Effects.Outline.Thickness * 2, config.Opacity);
                dc.DrawLine(outlinePen, start, end);
            }

            dc.DrawLine(pen, start, end);
        }
    }

    /// <summary>
    /// 渲染自定义图片
    /// </summary>
    private void RenderCustomImage(DrawingContext dc, Point center, CrosshairConfig config, double width, double height)
    {
        if (string.IsNullOrEmpty(config.CustomImagePath) || !File.Exists(config.CustomImagePath))
            return;

        try
        {
            var image = new BitmapImage(new Uri(config.CustomImagePath));
            image.Freeze();

            var scale = config.Size / 100.0;
            var imageWidth = image.PixelWidth * scale;
            var imageHeight = image.PixelHeight * scale;

            var rect = new Rect(
                center.X - imageWidth / 2,
                center.Y - imageHeight / 2,
                imageWidth,
                imageHeight);

            // 应用透明度
            if (config.Opacity < 100)
            {
                dc.PushOpacity(config.Opacity / 100.0);
            }

            dc.DrawImage(image, rect);

            if (config.Opacity < 100)
            {
                dc.Pop();
            }
        }
        catch
        {
            // 图片加载失败，忽略
        }
    }

    /// <summary>
    /// 获取或创建画笔
    /// </summary>
    private Pen GetOrCreatePen(string color, double thickness, double opacity)
    {
        var key = $"{color}_{thickness}_{opacity}";
        if (!_penCache.TryGetValue(key, out var pen))
        {
            var brush = GetOrCreateBrush(color, opacity);
            pen = new Pen(brush, thickness);
            pen.Freeze();
            _penCache[key] = pen;
        }
        return pen;
    }

    /// <summary>
    /// 获取或创建画刷
    /// </summary>
    private Brush GetOrCreateBrush(string color, double opacity)
    {
        var key = $"{color}_{opacity}";
        if (!_brushCache.TryGetValue(key, out var brush))
        {
            var colorObj = (Color)ColorConverter.ConvertFromString(color);
            brush = new SolidColorBrush(colorObj);
            brush.Opacity = opacity / 100.0;
            brush.Freeze();
            _brushCache[key] = brush;
        }
        return brush;
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        _penCache.Clear();
        _brushCache.Clear();
        _geometryCache.Clear();
    }
}
