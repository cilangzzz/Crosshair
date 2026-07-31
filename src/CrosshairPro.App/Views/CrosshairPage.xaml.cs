using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrosshairPro.App.ViewModels;
using CrosshairPro.Core.Enums;
using System.IO;

namespace CrosshairPro.App.Views;

/// <summary>
/// 准心配置页面
/// </summary>
public partial class CrosshairPage : UserControl
{
    private CrosshairViewModel? _viewModel;
    private static readonly string LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairPro", "crosshair_page.log");

    public CrosshairPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;

        // DEBUG: 监听 DataContext 变化
        DataContextChanged += (s, e) =>
        {
            Log($"DataContextChanged: {e.NewValue?.GetType().Name ?? "null"}");
            if (e.NewValue is CrosshairViewModel vm)
            {
                Log($"ViewModel found, Config: {vm.Config != null}");
                SetViewModel(vm);
            }
        };
    }

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFile)!);
            File.AppendAllText(LogFile, $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n");
        }
        catch { }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Log($"OnLoaded - DataContext: {DataContext?.GetType().Name ?? "null"}");
        Log($"OnLoaded - GridCanvas ActualWidth: {GridCanvas.ActualWidth}");
        Log($"OnLoaded - CrosshairCanvas ActualWidth: {CrosshairCanvas.ActualWidth}");

        // 从 Window 的 DataContext 获取 MainViewModel，然后获取 CrosshairViewModel
        var window = Window.GetWindow(this);
        if (window?.DataContext is MainViewModel mainVm)
        {
            Log($"Found MainViewModel, CrosshairViewModel: {mainVm.CrosshairViewModel != null}");
            SetViewModel(mainVm.CrosshairViewModel);
        }
        else
        {
            Log($"WARNING: Could not find MainViewModel from Window!");
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Log($"OnSizeChanged - Size: {e.NewSize}");
        if (_viewModel != null && IsLoaded)
        {
            RenderPreview();
        }
    }

    /// <summary>
    /// 设置 ViewModel 并初始化预览渲染
    /// </summary>
    public void SetViewModel(CrosshairViewModel viewModel)
    {
        Log($"SetViewModel called");

        // 设置 DataContext 以便 XAML 绑定生效
        DataContext = viewModel;

        // 移除旧的事件订阅
        if (_viewModel != null)
        {
            _viewModel.ConfigUpdated -= OnConfigUpdated;
        }

        _viewModel = viewModel;

        // 订阅配置更新事件
        _viewModel.ConfigUpdated += OnConfigUpdated;

        // 订阅配置属性变化
        if (_viewModel.Config != null)
        {
            _viewModel.Config.PropertyChanged += (s, e) =>
            {
                Log($"Config.PropertyChanged: {e.PropertyName}");
                Dispatcher.Invoke(() => RenderPreview());
            };
        }

        Log($"ViewModel set - Config: {_viewModel.Config != null}, Size: {_viewModel.Config?.Size}");

        // 初始渲染
        Dispatcher.BeginInvoke(() =>
        {
            Log($"Rendering preview...");
            RenderPreview();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnConfigUpdated(object? sender, EventArgs e)
    {
        Log($"OnConfigUpdated");
        Dispatcher.Invoke(() => RenderPreview());
    }

    /// <summary>
    /// 渲染准心预览
    /// </summary>
    private void RenderPreview()
    {
        Log($"RenderPreview - _viewModel: {_viewModel != null}, Config: {_viewModel?.Config != null}");

        if (_viewModel?.Config == null)
        {
            Log($"RenderPreview ABORTED - no config");
            return;
        }

        // 清空现有内容
        GridCanvas.Children.Clear();
        CrosshairCanvas.Children.Clear();

        Log($"Canvas sizes - GridCanvas: {GridCanvas.ActualWidth}x{GridCanvas.ActualHeight}, CrosshairCanvas: {CrosshairCanvas.ActualWidth}x{CrosshairCanvas.ActualHeight}");

        // 渲染网格
        RenderGrid();

        // 渲染准心
        RenderCrosshair();

        Log($"RenderPreview done - GridCanvas children: {GridCanvas.Children.Count}, CrosshairCanvas children: {CrosshairCanvas.Children.Count}");
    }

    /// <summary>
    /// 渲染预览网格
    /// </summary>
    private void RenderGrid()
    {
        var canvas = GridCanvas;
        var width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 300;
        var height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 300;

        // 网格线颜色
        var gridBrush = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

        // 横线
        for (int i = 0; i <= 6; i++)
        {
            var y = height * i / 6;
            canvas.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = 0, Y1 = y,
                X2 = width, Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = i == 3 ? 1 : 0.5
            });
        }

        // 竖线
        for (int i = 0; i <= 6; i++)
        {
            var x = width * i / 6;
            canvas.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = x, Y1 = 0,
                X2 = x, Y2 = height,
                Stroke = gridBrush,
                StrokeThickness = i == 3 ? 1 : 0.5
            });
        }
    }

    /// <summary>
    /// 渲染准心预览
    /// </summary>
    private void RenderCrosshair()
    {
        if (_viewModel?.Config == null) return;

        var canvas = CrosshairCanvas;
        var width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 300;
        var height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 300;
        var cx = width / 2;
        var cy = height / 2;

        var config = _viewModel.Config;

        // 解析颜色
        Color baseColor;
        try
        {
            baseColor = (Color)ColorConverter.ConvertFromString(config.Color);
        }
        catch
        {
            baseColor = Colors.Lime;
        }

        // 应用亮度
        var brightness = config.Brightness / 100.0;
        var color = Color.FromRgb(
            (byte)Math.Min(255, baseColor.R * brightness),
            (byte)Math.Min(255, baseColor.G * brightness),
            (byte)Math.Min(255, baseColor.B * brightness));
        var brush = new SolidColorBrush(color);

        var opacity = config.Opacity / 100.0;
        var size = config.Size;
        var gap = config.Gap;
        var thick = config.Thickness;
        var halfSize = size / 2;
        var halfGap = gap / 2;

        // 渲染中心点（非 Dot 样式时）
        if (config.CenterSize > 0 && config.Style != CrosshairStyle.Dot)
        {
            var dot = new System.Windows.Shapes.Ellipse
            {
                Width = config.CenterSize,
                Height = config.CenterSize,
                Fill = brush,
                Opacity = opacity
            };
            Canvas.SetLeft(dot, cx - config.CenterSize / 2);
            Canvas.SetTop(dot, cy - config.CenterSize / 2);
            canvas.Children.Add(dot);
        }

        // 根据样式渲染
        switch (config.Style)
        {
            case CrosshairStyle.Cross:
                DrawLine(canvas, cx - halfSize, cy, cx - halfGap, cy, brush, thick, opacity);
                DrawLine(canvas, cx + halfGap, cy, cx + halfSize, cy, brush, thick, opacity);
                DrawLine(canvas, cx, cy - halfSize, cx, cy - halfGap, brush, thick, opacity);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfSize, brush, thick, opacity);
                break;

            case CrosshairStyle.Dot:
                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = config.Size,
                    Height = config.Size,
                    Fill = brush,
                    Opacity = opacity
                };
                Canvas.SetLeft(dot, cx - config.Size / 2);
                Canvas.SetTop(dot, cy - config.Size / 2);
                canvas.Children.Add(dot);
                break;

            case CrosshairStyle.Circle:
                var circle = new System.Windows.Shapes.Ellipse
                {
                    Width = config.Size,
                    Height = config.Size,
                    Stroke = brush,
                    StrokeThickness = config.Thickness,
                    Opacity = opacity
                };
                Canvas.SetLeft(circle, cx - config.Size / 2);
                Canvas.SetTop(circle, cy - config.Size / 2);
                canvas.Children.Add(circle);
                break;

            case CrosshairStyle.TShape:
                DrawLine(canvas, cx - halfSize, cy, cx + halfSize, cy, brush, thick, opacity);
                DrawLine(canvas, cx, cy, cx, cy - halfSize, brush, thick, opacity);
                break;

            case CrosshairStyle.XShape:
                var off = halfGap * 0.707;
                var len = halfSize * 0.707;
                DrawLine(canvas, cx - halfSize, cy - halfSize, cx - off, cy - off, brush, thick, opacity);
                DrawLine(canvas, cx + off, cy + off, cx + halfSize, cy + halfSize, brush, thick, opacity);
                DrawLine(canvas, cx + halfSize, cy - halfSize, cx + off, cy - off, brush, thick, opacity);
                DrawLine(canvas, cx - off, cy + off, cx - halfSize, cy + halfSize, brush, thick, opacity);
                break;

            case CrosshairStyle.CustomImage:
                // 图片样式暂时用十字代替
                DrawLine(canvas, cx - halfSize, cy, cx - halfGap, cy, brush, thick, opacity);
                DrawLine(canvas, cx + halfGap, cy, cx + halfSize, cy, brush, thick, opacity);
                DrawLine(canvas, cx, cy - halfSize, cx, cy - halfGap, brush, thick, opacity);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfSize, brush, thick, opacity);
                break;
        }
    }

    private void DrawLine(Canvas canvas, double x1, double y1, double x2, double y2, Brush brush, double thickness, double opacity)
    {
        canvas.Children.Add(new System.Windows.Shapes.Line
        {
            X1 = x1, Y1 = y1,
            X2 = x2, Y2 = y2,
            Stroke = brush,
            StrokeThickness = thickness,
            Opacity = opacity,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        });
    }

    /// <summary>
    /// 自定义颜色点击处理
    /// </summary>
    private void CustomColor_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel?.Config == null) return;

        var dialog = new System.Windows.Forms.ColorDialog();
        try
        {
            var c = (Color)ColorConverter.ConvertFromString(_viewModel.Config.Color);
            dialog.Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
        }
        catch { }

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = dialog.Color;
            _viewModel.Config.Color = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}