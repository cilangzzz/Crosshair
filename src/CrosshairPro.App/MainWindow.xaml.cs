using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CrosshairPro.App.ViewModels;
using CrosshairPro.App.Views;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;
using CrosshairPro.Infrastructure.Hotkey;

namespace CrosshairPro.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly OverlayWindow _overlayWindow;
    private readonly IHotkeyManager _hotkeyManager;

    public MainWindow()
    {
        InitializeComponent();

        _hotkeyManager = new HotkeyManager();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // 覆盖窗口
        _overlayWindow = new OverlayWindow();
        _overlayWindow.CrosshairVisibilityChanged += OnCrosshairVisibilityChanged;

        // ViewModel 的 ConfigUpdated 事件 → 同步到覆盖窗口 + 更新预览
        _viewModel.ConfigUpdated += (s, e) =>
        {
            _overlayWindow.UpdateConfig(_viewModel.Config);
            DrawPreview();
        };

        // 样式 ComboBox 初始化
        StyleComboBox.ItemsSource = _viewModel.CrosshairStyleNames;

        RegisterHotkeys();

        // 等覆盖窗口 Loaded 后再同步配置并渲染
        _overlayWindow.Loaded += (s, e) =>
        {
            _overlayWindow.UpdateConfig(_viewModel.Config);
        };

        _overlayWindow.Show();

        // 初始绘制预览
        Loaded += (s, e) => DrawPreview();

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// 在预览 Canvas 上绘制准心
    /// </summary>
    private void DrawPreview()
    {
        CrosshairCanvas.Children.Clear();
        GridCanvas.Children.Clear();

        var canvas = CrosshairCanvas;
        var grid = GridCanvas;
        if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0) return;

        double cx = canvas.ActualWidth / 2;
        double cy = canvas.ActualHeight / 2;

        var cfg = _viewModel.Config;
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cfg.Color));
        brush.Opacity = cfg.Opacity / 100.0;
        var outlineBrush = Brushes.Black;
        bool hasOutline = cfg.Effects.Outline.Enabled;

        // 画网格
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), 0.5);
        for (double x = 0; x < canvas.ActualWidth; x += 30)
            grid.Children.Add(new Line { X1 = x, Y1 = 0, X2 = x, Y2 = canvas.ActualHeight, Stroke = gridPen.Brush, StrokeThickness = 0.5 });
        for (double y = 0; y < canvas.ActualHeight; y += 30)
            grid.Children.Add(new Line { X1 = 0, Y1 = y, X2 = canvas.ActualWidth, Y2 = y, Stroke = gridPen.Brush, StrokeThickness = 0.5 });
        // 十字辅助线
        grid.Children.Add(new Line { X1 = cx, Y1 = 0, X2 = cx, Y2 = canvas.ActualHeight, Stroke = gridPen.Brush, StrokeThickness = 1 });
        grid.Children.Add(new Line { X1 = 0, Y1 = cy, X2 = canvas.ActualWidth, Y2 = cy, Stroke = gridPen.Brush, StrokeThickness = 1 });

        double size = cfg.Size;
        double gap = cfg.Gap;
        double thick = cfg.Thickness;
        double halfSize = size / 2;
        double halfGap = gap / 2;

        // 中心点（Dot 样式除外，Dot 自带中心点绘制）
        if (cfg.CenterSize > 0 && cfg.Style != CrosshairStyle.Dot)
        {
            DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, hasOutline, outlineBrush, cfg);
        }

        switch (cfg.Style)
        {
            case CrosshairStyle.Cross:
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg);
                break;

            case CrosshairStyle.Dot:
                DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, hasOutline, outlineBrush, cfg);
                break;

            case CrosshairStyle.Circle:
                DrawCircle(canvas, cx, cy, halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                if (cfg.CenterSize > 0)
                    DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, false, outlineBrush, cfg);
                break;

            case CrosshairStyle.TShape:
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx - halfGap - halfSize, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg);
                break;

            case CrosshairStyle.XShape:
                double off = halfGap * 0.707;
                double len = halfSize * 0.707;
                DrawLine(canvas, cx - off, cy - off, cx - off - len, cy - off - len, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx + off, cy - off, cx + off + len, cy - off - len, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx - off, cy + off, cx - off - len, cy + off + len, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx + off, cy + off, cx + off + len, cy + off + len, brush, thick, hasOutline, outlineBrush, cfg);
                break;

            case CrosshairStyle.CustomImage:
                // 暂用十字替代
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg);
                DrawLine(canvas, cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg);
                break;
        }
    }

    private void DrawLine(Canvas c, double x1, double y1, double x2, double y2, Brush brush, double thick, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg)
    {
        if (hasOutline)
        {
            c.Children.Add(new Line
            {
                X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
                Stroke = outlineBrush,
                StrokeThickness = thick + cfg.Effects.Outline.Thickness * 2
            });
        }
        c.Children.Add(new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = brush,
            StrokeThickness = thick
        });
    }

    private void DrawDot(Canvas c, double cx, double cy, double radius, Brush brush, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg)
    {
        if (hasOutline)
        {
            c.Children.Add(new Ellipse
            {
                Width = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Height = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = cfg.Effects.Outline.Thickness,
                Margin = new Thickness(cx - radius - cfg.Effects.Outline.Thickness, cy - radius - cfg.Effects.Outline.Thickness, 0, 0)
            });
        }
        c.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = brush,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0)
        });
    }

    private void DrawCircle(Canvas c, double cx, double cy, double radius, Brush brush, double thick, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg)
    {
        if (hasOutline)
        {
            c.Children.Add(new Ellipse
            {
                Width = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Height = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = thick + cfg.Effects.Outline.Thickness * 2,
                Margin = new Thickness(cx - radius - cfg.Effects.Outline.Thickness, cy - radius - cfg.Effects.Outline.Thickness, 0, 0)
            });
        }
        c.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = brush,
            StrokeThickness = thick,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0)
        });
    }

    private void RegisterHotkeys()
    {
        var toggleBinding = new HotkeyBinding
        {
            Id = "toggle-crosshair",
            Name = "切换准心",
            Combo = "Ctrl+Shift+X",
            Action = HotkeyAction.ToggleCrosshair
        };

        if (_hotkeyManager.RegisterHotkey(toggleBinding))
            _hotkeyManager.HotkeyTriggered += OnHotkeyTriggered;
    }

    private void OnHotkeyTriggered(object? sender, HotkeyTriggeredEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            switch (e.Binding.Action)
            {
                case HotkeyAction.ToggleCrosshair:
                    _overlayWindow.ToggleVisibility();
                    _viewModel.IsCrosshairVisible = _overlayWindow.IsCrosshairVisible;
                    break;
                case HotkeyAction.IncreaseSize:
                    _viewModel.Config.Size = Math.Min(100, _viewModel.Config.Size + 5);
                    break;
                case HotkeyAction.DecreaseSize:
                    _viewModel.Config.Size = Math.Max(1, _viewModel.Config.Size - 5);
                    break;
            }
        });
    }

    private void OnCrosshairVisibilityChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.IsCrosshairVisible = _overlayWindow.IsCrosshairVisible;
            _viewModel.StatusMessage = _overlayWindow.IsCrosshairVisible ? "准心已启用" : "准心已禁用";
        });
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _hotkeyManager.Dispose();
        _overlayWindow.Close();
        Application.Current.Shutdown();
    }
}
