using System.ComponentModel;
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
using Hardcodet.Wpf.TaskbarNotification;

namespace CrosshairPro.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly OverlayWindow _overlayWindow;
    private readonly IHotkeyManager _hotkeyManager;
    private TaskbarIcon? _trayIcon;
    private bool _isReallyClosing;

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

        // ViewModel 的 ToggleCrosshairRequested 事件 → 操作覆盖窗口
        _viewModel.ToggleCrosshairRequested += (s, e) =>
        {
            _overlayWindow.ToggleVisibility();
            _viewModel.IsCrosshairVisible = _overlayWindow.IsCrosshairVisible;
        };

        // 样式 ComboBox 初始化
        StyleComboBox.ItemsSource = _viewModel.CrosshairStyleNames;

        RegisterHotkeys();
        SetupTrayIcon();

        // 等覆盖窗口 Loaded 后再同步配置并渲染
        _overlayWindow.Loaded += (s, e) =>
        {
            _overlayWindow.UpdateConfig(_viewModel.Config);
        };

        _overlayWindow.Show();

        // 初始绘制预览
        Loaded += (s, e) => DrawPreview();
    }

    /// <summary>
    /// 设置系统托盘图标
    /// </summary>
    private void SetupTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Crosshair Pro - 准心已启用",
            DoubleClickCommand = new RelayCommand(() => ShowMainWindow())
        };

        // 加载应用图标
        try
        {
            var iconUri = new Uri("pack://application:,,,/Assets/app-icon.ico", UriKind.Absolute);
            var iconStream = Application.GetResourceStream(iconUri)?.Stream;
            if (iconStream != null)
                _trayIcon.Icon = new System.Drawing.Icon(iconStream);
            else
                _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        catch
        {
            _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        // 右键菜单
        var menu = new ContextMenu { StaysOpen = false };

        var showItem = new MenuItem { Header = "打开主窗口" };
        showItem.Click += (s, e) => ShowMainWindow();
        menu.Items.Add(showItem);

        var toggleItem = new MenuItem { Header = "切换准心显示" };
        toggleItem.Click += (s, e) =>
        {
            _overlayWindow.ToggleVisibility();
            _viewModel.IsCrosshairVisible = _overlayWindow.IsCrosshairVisible;
        };
        menu.Items.Add(toggleItem);

        menu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "退出" };
        exitItem.Click += (s, e) => ReallyExit();
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;
    }

    /// <summary>
    /// 显示主窗口
    /// </summary>
    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    /// <summary>
    /// 真正退出程序
    /// </summary>
    private void ReallyExit()
    {
        _isReallyClosing = true;
        _trayIcon?.Dispose();
        _hotkeyManager.Dispose();
        _overlayWindow.Close();
        Close();
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 拦截关闭事件 → 最小化到托盘而非退出
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isReallyClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
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

        // 亮度：调整颜色明暗
        var baseColor = (Color)ColorConverter.ConvertFromString(cfg.Color);
        var color = ApplyBrightness(baseColor, cfg.Brightness);
        var brush = new SolidColorBrush(color);
        var outlineBrush = Brushes.Black;

        // 透明度：在每个 Shape 上单独设置
        double shapeOpacity = cfg.Opacity / 100.0;
        bool hasOutline = cfg.Effects.Outline.Enabled;

        // 画网格
        var gridBrush = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
        for (double x = 0; x < canvas.ActualWidth; x += 30)
            grid.Children.Add(new Line { X1 = x, Y1 = 0, X2 = x, Y2 = canvas.ActualHeight, Stroke = gridBrush, StrokeThickness = 0.5 });
        for (double y = 0; y < canvas.ActualHeight; y += 30)
            grid.Children.Add(new Line { X1 = 0, Y1 = y, X2 = canvas.ActualWidth, Y2 = y, Stroke = gridBrush, StrokeThickness = 0.5 });
        grid.Children.Add(new Line { X1 = cx, Y1 = 0, X2 = cx, Y2 = canvas.ActualHeight, Stroke = gridBrush, StrokeThickness = 1 });
        grid.Children.Add(new Line { X1 = 0, Y1 = cy, X2 = canvas.ActualWidth, Y2 = cy, Stroke = gridBrush, StrokeThickness = 1 });

        double size = cfg.Size;
        double gap = cfg.Gap;
        double thick = cfg.Thickness;
        double halfSize = size / 2;
        double halfGap = gap / 2;

        if (cfg.CenterSize > 0 && cfg.Style != CrosshairStyle.Dot)
            DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, hasOutline, outlineBrush, cfg, shapeOpacity);

        switch (cfg.Style)
        {
            case CrosshairStyle.Cross:
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                break;
            case CrosshairStyle.Dot:
                DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, hasOutline, outlineBrush, cfg, shapeOpacity);
                break;
            case CrosshairStyle.Circle:
                DrawCircle(canvas, cx, cy, halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                if (cfg.CenterSize > 0)
                    DrawDot(canvas, cx, cy, cfg.CenterSize / 2.0, brush, false, outlineBrush, cfg, shapeOpacity);
                break;
            case CrosshairStyle.TShape:
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx - halfGap - halfSize, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                break;
            case CrosshairStyle.XShape:
                double off = halfGap * 0.707;
                double len = halfSize * 0.707;
                DrawLine(canvas, cx - off, cy - off, cx - off - len, cy - off - len, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx + off, cy - off, cx + off + len, cy - off - len, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx - off, cy + off, cx - off - len, cy + off + len, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx + off, cy + off, cx + off + len, cy + off + len, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                break;
            case CrosshairStyle.CustomImage:
                DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                DrawLine(canvas, cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                break;
        }
    }

    private void DrawLine(Canvas c, double x1, double y1, double x2, double y2, Brush brush, double thick, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg, double opacity)
    {
        if (hasOutline)
            c.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = outlineBrush, StrokeThickness = thick + cfg.Effects.Outline.Thickness * 2, Opacity = opacity });
        c.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thick, Opacity = opacity });
    }

    private void DrawDot(Canvas c, double cx, double cy, double radius, Brush brush, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg, double opacity)
    {
        if (hasOutline)
            c.Children.Add(new Ellipse
            {
                Width = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Height = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = cfg.Effects.Outline.Thickness,
                Margin = new Thickness(cx - radius - cfg.Effects.Outline.Thickness, cy - radius - cfg.Effects.Outline.Thickness, 0, 0),
                Opacity = opacity
            });
        c.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = brush,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0),
            Opacity = opacity
        });
    }

    private void DrawCircle(Canvas c, double cx, double cy, double radius, Brush brush, double thick, bool hasOutline, Brush outlineBrush, CrosshairConfig cfg, double opacity)
    {
        if (hasOutline)
            c.Children.Add(new Ellipse
            {
                Width = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Height = radius * 2 + cfg.Effects.Outline.Thickness * 2,
                Stroke = outlineBrush,
                StrokeThickness = thick + cfg.Effects.Outline.Thickness * 2,
                Margin = new Thickness(cx - radius - cfg.Effects.Outline.Thickness, cy - radius - cfg.Effects.Outline.Thickness, 0, 0),
                Opacity = opacity
            });
        c.Children.Add(new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = brush,
            StrokeThickness = thick,
            Margin = new Thickness(cx - radius, cy - radius, 0, 0),
            Opacity = opacity
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

    private static Color ApplyBrightness(Color color, int brightness)
    {
        double factor = brightness / 100.0;
        return Color.FromRgb(
            (byte)Math.Min(255, color.R * factor),
            (byte)Math.Min(255, color.G * factor),
            (byte)Math.Min(255, color.B * factor));
    }
}

/// <summary>
/// 简单的 ICommand 实现，用于托盘菜单的 DoubleClick
/// </summary>
public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;
    public RelayCommand(Action execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}
