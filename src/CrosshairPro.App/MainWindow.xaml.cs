using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using CrosshairPro.App.ViewModels;
using CrosshairPro.App.Views;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;
using Hardcodet.Wpf.TaskbarNotification;

namespace CrosshairPro.App;

public partial class MainWindow : Window
{
    // DWM API for window corner rounding (Windows 11+)
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;

    // Win32 cursor position
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    private readonly MainViewModel _viewModel;
    private readonly OverlayWindow _overlayWindow;
    private readonly IHotkeyManager _hotkeyManager;
    private TaskbarIcon? _trayIcon;
    private bool _isReallyClosing;

    public MainWindow(
        MainViewModel viewModel,
        OverlayWindow overlayWindow,
        IHotkeyManager hotkeyManager)
    {
        InitializeComponent();

        // Apply rounded corners (Windows 11+)
        SourceInitialized += OnSourceInitialized;

        _hotkeyManager = hotkeyManager;
        _viewModel = viewModel;
        _overlayWindow = overlayWindow;

        DataContext = _viewModel;

        // 覆盖窗口事件
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

        // 选择自定义图片 → 打开文件对话框
        _viewModel.SelectImageRequested += (s, e) =>
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "图片文件 (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|所有文件 (*.*)|*.*",
                Title = "选择准心图片"
            };
            if (dlg.ShowDialog() == true)
            {
                _viewModel.Config.CustomImagePath = dlg.FileName;
                _overlayWindow.UpdateConfig(_viewModel.Config);
                DrawPreview();
            }
        };

        // 保存预设 → 弹出命名对话框
        _viewModel.SavePresetRequested += async (s, e) =>
        {
            var name = ShowInputDialog("保存预设", "请输入预设名称:", _viewModel.CurrentPresetName);
            if (!string.IsNullOrWhiteSpace(name))
                await _viewModel.SavePresetWithNameAsync(name);
        };

        // 导入预设 → 打开文件对话框
        _viewModel.ImportPresetRequested += async (s, e) =>
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导入预设"
            };
            if (dlg.ShowDialog() == true)
                await _viewModel.ImportPresetFromFileAsync(dlg.FileName);
        };

        // 导出预设 → 打开保存对话框
        _viewModel.ExportPresetRequested += async (s, e) =>
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                Title = "导出预设",
                FileName = _viewModel.CurrentPresetName
            };
            if (dlg.ShowDialog() == true)
                await _viewModel.ExportPresetToFileAsync(dlg.FileName);
        };

        // Toast 提示
        _viewModel.ToastRequested += (s, message) =>
        {
            Dispatcher.Invoke(() => ShowToast(message));
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
            ToolTipText = "Crosshair Pro",
            TrayToolTip = new ToolTip
            {
                Content = "Crosshair Pro",
                Background = (SolidColorBrush)FindResource("SurfaceBrush"),
                Foreground = (SolidColorBrush)FindResource("TextPrimaryBrush")
            },
            DoubleClickCommand = new RelayCommand(() => ShowMainWindow())
        };

        // 加载应用图标
        try
        {
            var iconUri = new Uri("pack://application:,,,/Assets/app-icon.ico", UriKind.Absolute);
            var iconStream = System.Windows.Application.GetResourceStream(iconUri)?.Stream;
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
        var menu = new ContextMenu();
        // 在 Resources 中覆盖 Separator 样式，消除白线
        var sepBorder = new FrameworkElementFactory(typeof(Border));
        sepBorder.SetValue(Border.HeightProperty, 1.0);
        sepBorder.SetValue(Border.BackgroundProperty, (SolidColorBrush)FindResource("BorderBrush"));
        sepBorder.SetValue(Border.MarginProperty, new Thickness(8, 4, 8, 4));
        var sepTemplate = new ControlTemplate(typeof(Separator)) { VisualTree = sepBorder };
        var sepStyle = new Style(typeof(Separator));
        sepStyle.Setters.Add(new Setter(Separator.TemplateProperty, sepTemplate));
        menu.Resources[typeof(Separator)] = sepStyle;

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

        // 手动处理右键：定位到鼠标位置（像素→WPF单位）
        _trayIcon.TrayRightMouseDown += (s, e) =>
        {
            GetCursorPos(out var pos);
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                var transform = source.CompositionTarget.TransformFromDevice;
                var wpfPos = transform.Transform(new System.Windows.Point(pos.X, pos.Y));
                menu.HorizontalOffset = wpfPos.X;
                menu.VerticalOffset = wpfPos.Y;
            }
            else
            {
                menu.HorizontalOffset = pos.X;
                menu.VerticalOffset = pos.Y;
            }
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            menu.StaysOpen = true;
            menu.IsOpen = true;
        };

        // 关键：在 Opened 事件中重新设置 StaysOpen=false，WPF 才会绑定外部点击关闭逻辑
        menu.Opened += (s, e) => menu.StaysOpen = false;
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
    /// 显示悬浮提示（Toast）
    /// </summary>
    private void ShowToast(string message)
    {
        var bgBrush = (SolidColorBrush)FindResource("SurfaceBrush");
        var borderBrush = (SolidColorBrush)FindResource("BorderBrush");
        var textPrimary = (SolidColorBrush)FindResource("TextPrimaryBrush");
        var accentBrush = (SolidColorBrush)FindResource("AccentBrush");

        var toast = new Border
        {
            Background = bgBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 40),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 12,
                ShadowDepth = 2,
                Opacity = 0.3
            }
        };

        var text = new TextBlock
        {
            Text = message,
            Foreground = textPrimary,
            FontSize = 13,
            FontFamily = new FontFamily("Cascadia Code, Consolas")
        };
        toast.Child = text;

        // 添加到主容器（假设 RootGrid 是主 Grid）
        if (RootGrid != null)
        {
            RootGrid.Children.Add(toast);

            // 3秒后自动消失
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                RootGrid.Children.Remove(toast);
            };
            timer.Start();
        }
    }

    /// <summary>
    /// 真正退出程序
    /// </summary>
    private void ReallyExit()
    {
        _isReallyClosing = true;
        _trayIcon?.Dispose();
        // _hotkeyManager 由 DI 容器管理，不需要手动释放
        _overlayWindow.Close();
        Close();
        System.Windows.Application.Current.Shutdown();
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
                // 倒T形：竖线朝下（FPS标准）
                DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
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
                if (!string.IsNullOrEmpty(cfg.CustomImagePath) && File.Exists(cfg.CustomImagePath))
                {
                    DrawImage(canvas, cx, cy, cfg.CustomImagePath, size, shapeOpacity);
                }
                else
                {
                    DrawLine(canvas, cx, cy - halfGap, cx, cy - halfGap - halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                    DrawLine(canvas, cx, cy + halfGap, cx, cy + halfGap + halfSize, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                    DrawLine(canvas, cx - halfGap, cy, cx - halfGap - halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                    DrawLine(canvas, cx + halfGap, cy, cx + halfGap + halfSize, cy, brush, thick, hasOutline, outlineBrush, cfg, shapeOpacity);
                }
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

    private void DrawImage(Canvas c, double cx, double cy, string path, double size, double opacity)
    {
        try
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            double scale = size / Math.Max(bitmap.PixelWidth, bitmap.PixelHeight);
            double w = bitmap.PixelWidth * scale;
            double h = bitmap.PixelHeight * scale;

            var image = new System.Windows.Controls.Image
            {
                Source = bitmap,
                Width = w,
                Height = h,
                Opacity = opacity
            };

            Canvas.SetLeft(image, cx - w / 2);
            Canvas.SetTop(image, cy - h / 2);
            c.Children.Add(image);
        }
        catch
        {
            // 图片加载失败时回退到十字
            DrawLine(c, cx - 10, cy, cx + 10, cy, Brushes.Red, 2, false, Brushes.Black, new CrosshairConfig(), opacity);
            DrawLine(c, cx, cy - 10, cx, cy + 10, Brushes.Red, 2, false, Brushes.Black, new CrosshairConfig(), opacity);
        }
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

    /// <summary>
    /// 简单的输入对话框
    /// </summary>
    private string? ShowInputDialog(string title, string prompt, string defaultValue = "")
    {
        // Use design token colors
        var bgBrush = (SolidColorBrush)FindResource("BackgroundBrush");
        var surfaceBrush = (SolidColorBrush)FindResource("SurfaceBrush");
        var controlBrush = (SolidColorBrush)FindResource("ControlBrush");
        var borderBrush = (SolidColorBrush)FindResource("BorderBrush");
        var textPrimary = (SolidColorBrush)FindResource("TextPrimaryBrush");
        var textSecondary = (SolidColorBrush)FindResource("TextSecondaryBrush");
        var accentBrush = (SolidColorBrush)FindResource("AccentBrush");

        var dialog = new Window
        {
            Title = title,
            Width = 360,
            Height = 185,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Owner = this
        };

        var border = new Border
        {
            Background = bgBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(6),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 16,
                ShadowDepth = 2,
                Opacity = 0.4
            }
        };

        var panel = new StackPanel { Margin = new Thickness(16, 12, 16, 12) };

        // Title
        panel.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = accentBrush,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Cascadia Code, Consolas"),
            Margin = new Thickness(0, 0, 0, 8)
        });

        // Prompt
        panel.Children.Add(new TextBlock
        {
            Text = prompt,
            Foreground = textSecondary,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6)
        });

        // Input
        var textBox = new TextBox
        {
            Text = defaultValue,
            Background = controlBrush,
            Foreground = textPrimary,
            CaretBrush = accentBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8, 5, 8, 5),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 12)
        };
        textBox.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        panel.Children.Add(textBox);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        string? result = null;

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(16, 6, 16, 6),
            Margin = new Thickness(0, 0, 8, 0),
            Background = controlBrush,
            Foreground = textPrimary,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        cancelBtn.Click += (s, e) => dialog.Close();

        var okBtn = new Button
        {
            Content = "Save",
            Padding = new Thickness(16, 6, 16, 6),
            Background = accentBrush,
            Foreground = Brushes.Black,
            FontWeight = FontWeights.Bold,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        okBtn.Click += (s, e) => { result = textBox.Text; dialog.Close(); };

        buttonPanel.Children.Add(cancelBtn);
        buttonPanel.Children.Add(okBtn);
        panel.Children.Add(buttonPanel);

        border.Child = panel;
        dialog.Content = border;

        // Enter/Escape key handling
        textBox.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter) { result = textBox.Text; dialog.Close(); }
            if (e.Key == System.Windows.Input.Key.Escape) dialog.Close();
        };

        dialog.MouseLeftButtonDown += (s, e) => dialog.DragMove();

        textBox.SelectAll();
        textBox.Focus();
        dialog.ShowDialog();

        return result;
    }

    // ── Custom Color Picker ──

    private void CustomColor_Click(object sender, RoutedEventArgs e)
    {
        var color = ShowColorPickerDialog();
        if (color != null)
        {
            _viewModel.SetColorCommand.Execute(color);
            // Update the custom radio button's foreground to show selected color
            if (sender is RadioButton rb)
                rb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
    }

    private string? ShowColorPickerDialog()
    {
        var bgBrush = (SolidColorBrush)FindResource("BackgroundBrush");
        var surfaceBrush = (SolidColorBrush)FindResource("SurfaceBrush");
        var controlBrush = (SolidColorBrush)FindResource("ControlBrush");
        var borderBrush = (SolidColorBrush)FindResource("BorderBrush");
        var textPrimary = (SolidColorBrush)FindResource("TextPrimaryBrush");
        var textSecondary = (SolidColorBrush)FindResource("TextSecondaryBrush");
        var accentBrush = (SolidColorBrush)FindResource("AccentBrush");

        // Parse current color
        byte r = 0, g = 255, b = 0;
        try
        {
            var c = (Color)ColorConverter.ConvertFromString(_viewModel.Config.Color);
            r = c.R; g = c.G; b = c.B;
        }
        catch { }

        var dialog = new Window
        {
            Title = "Pick Color",
            Width = 300,
            Height = 340,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Owner = this
        };

        var outer = new Border
        {
            Background = bgBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(6),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black, BlurRadius = 16, ShadowDepth = 2, Opacity = 0.4
            }
        };

        var panel = new StackPanel { Margin = new Thickness(16, 12, 16, 12) };

        // Title
        panel.Children.Add(new TextBlock
        {
            Text = "CUSTOM COLOR",
            Foreground = accentBrush,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Cascadia Code, Consolas"),
            Margin = new Thickness(0, 0, 0, 12)
        });

        // Color preview
        var preview = new Border
        {
            Height = 40,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromRgb(r, g, b)),
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 12)
        };
        panel.Children.Add(preview);

        // RGB sliders
        var rSlider = AddColorSlider(panel, "R", r, textPrimary, textSecondary, controlBrush, accentBrush);
        var gSlider = AddColorSlider(panel, "G", g, textPrimary, textSecondary, controlBrush, accentBrush);
        var bSlider = AddColorSlider(panel, "B", b, textPrimary, textSecondary, controlBrush, accentBrush);

        // Hex input
        var hexPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 12) };
        hexPanel.Children.Add(new TextBlock
        {
            Text = "#", Foreground = textSecondary, FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0)
        });
        var hexBox = new TextBox
        {
            Text = $"{r:X2}{g:X2}{b:X2}",
            Width = 80,
            Background = controlBrush,
            Foreground = textPrimary,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6, 4, 6, 4),
            FontFamily = new FontFamily("Cascadia Code, Consolas"),
            MaxLength = 6
        };
        hexPanel.Children.Add(hexBox);
        panel.Children.Add(hexPanel);

        // Update preview from sliders
        void UpdatePreview()
        {
            var color = Color.FromRgb((byte)rSlider.Value, (byte)gSlider.Value, (byte)bSlider.Value);
            preview.Background = new SolidColorBrush(color);
            hexBox.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        rSlider.ValueChanged += (s, ev) => UpdatePreview();
        gSlider.ValueChanged += (s, ev) => UpdatePreview();
        bSlider.ValueChanged += (s, ev) => UpdatePreview();

        // Hex input → sliders
        hexBox.TextChanged += (s, ev) =>
        {
            if (hexBox.Text.Length == 6)
            {
                try
                {
                    var c = (Color)ColorConverter.ConvertFromString("#" + hexBox.Text);
                    rSlider.Value = c.R;
                    gSlider.Value = c.G;
                    bSlider.Value = c.B;
                }
                catch { }
            }
        };

        // Buttons
        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        string? result = null;

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Style = (Style)FindResource("SecondaryButton"),
            Margin = new Thickness(0, 0, 8, 0)
        };
        cancelBtn.Click += (s, ev) => dialog.Close();

        var okBtn = new Button
        {
            Content = "OK",
            Style = (Style)FindResource("PrimaryButton")
        };
        okBtn.Click += (s, ev) =>
        {
            var c = Color.FromRgb((byte)rSlider.Value, (byte)gSlider.Value, (byte)bSlider.Value);
            result = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            dialog.Close();
        };

        btnPanel.Children.Add(cancelBtn);
        btnPanel.Children.Add(okBtn);
        panel.Children.Add(btnPanel);

        outer.Child = panel;
        dialog.Content = outer;
        dialog.MouseLeftButtonDown += (s, ev) => dialog.DragMove();
        dialog.ShowDialog();

        return result;
    }

    private static Slider AddColorSlider(Panel parent, string label, byte value,
        Brush textPrimary, Brush textSecondary, Brush controlBrush, Brush accentBrush)
    {
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });

        row.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = textSecondary,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });

        var slider = new Slider
        {
            Minimum = 0, Maximum = 255, Value = value,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0),
            Foreground = accentBrush
        };
        Grid.SetColumn(slider, 1);
        row.Children.Add(slider);

        var valText = new TextBlock
        {
            Text = value.ToString(),
            Foreground = textPrimary,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(valText, 2);
        row.Children.Add(valText);

        slider.ValueChanged += (s, e) => valText.Text = ((int)e.NewValue).ToString();

        parent.Children.Add(row);
        return slider;
    }

    // ── Preset Management Popup ──

    private void PresetManageButton_Click(object sender, RoutedEventArgs e)
    {
        var bgBrush = (SolidColorBrush)FindResource("BackgroundBrush");
        var surfaceBrush = (SolidColorBrush)FindResource("SurfaceBrush");
        var controlBrush = (SolidColorBrush)FindResource("ControlBrush");
        var controlHoverBrush = (SolidColorBrush)FindResource("ControlHoverBrush");
        var borderBrush = (SolidColorBrush)FindResource("BorderBrush");
        var textPrimary = (SolidColorBrush)FindResource("TextPrimaryBrush");
        var textSecondary = (SolidColorBrush)FindResource("TextSecondaryBrush");
        var accentBrush = (SolidColorBrush)FindResource("AccentBrush");
        var errorColor = (SolidColorBrush)FindResource("ErrorBrush");

        var popup = new Window
        {
            Title = "Manage Presets",
            Width = 340,
            Height = 360,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Owner = this
        };

        var outerBorder = new Border
        {
            Background = bgBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(8),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black, BlurRadius = 16, ShadowDepth = 2, Opacity = 0.4
            }
        };

        var root = new DockPanel { Margin = new Thickness(0) };

        // ── Header ──
        var header = new DockPanel { Margin = new Thickness(16, 12, 16, 4) };
        header.Children.Add(new TextBlock
        {
            Text = "MANAGE PRESETS",
            Foreground = accentBrush,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Cascadia Code, Consolas"),
            VerticalAlignment = VerticalAlignment.Center
        });
        var closeBtn = new Button
        {
            Content = "×", FontSize = 16, FontWeight = FontWeights.Bold,
            Width = 28, Height = 28, Padding = new Thickness(0),
            Background = Brushes.Transparent, Foreground = textSecondary,
            BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        closeBtn.Click += (s, ev) => popup.Close();
        DockPanel.SetDock(closeBtn, Dock.Right);
        header.Children.Add(closeBtn);
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        // ── Import / Export (top, below header) ──
        var topBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(16, 4, 16, 8)
        };
        var importBtn = new Button
        {
            Content = "＋ Import", Padding = new Thickness(10, 4, 10, 4),
            Style = (Style)FindResource("SecondaryButton"), Margin = new Thickness(0, 0, 6, 0)
        };
        importBtn.Click += (s, ev) => { popup.Close(); _viewModel.ImportPresetCommand.Execute(null); };
        var exportBtn = new Button
        {
            Content = "↗ Export", Padding = new Thickness(10, 4, 10, 4),
            Style = (Style)FindResource("SecondaryButton")
        };
        exportBtn.Click += (s, ev) => { popup.Close(); _viewModel.ExportPresetCommand.Execute(null); };
        topBar.Children.Add(importBtn);
        topBar.Children.Add(exportBtn);
        DockPanel.SetDock(topBar, Dock.Top);
        root.Children.Add(topBar);

        // ── Preset list (each item has name + delete × button) ──
        var listBox = new ListBox
        {
            Background = surfaceBrush,
            Foreground = textPrimary,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(16, 0, 16, 16),
            Padding = new Thickness(0),
            ItemsSource = _viewModel.Presets,
            SelectedItem = _viewModel.SelectedPreset
        };

        // DataTemplate: [Name] ............ [×]
        var itemTemplate = new DataTemplate();
        var panelFactory = new FrameworkElementFactory(typeof(DockPanel));

        // Delete button (docked right)
        var delFactory = new FrameworkElementFactory(typeof(Button));
        delFactory.SetValue(Button.ContentProperty, "×");
        delFactory.SetValue(Button.FontSizeProperty, 13.0);
        delFactory.SetValue(Button.FontWeightProperty, FontWeights.Bold);
        delFactory.SetValue(Button.WidthProperty, 24.0);
        delFactory.SetValue(Button.HeightProperty, 24.0);
        delFactory.SetValue(Button.PaddingProperty, new Thickness(0));
        delFactory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
        delFactory.SetValue(Button.ForegroundProperty, errorColor);
        delFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0.0));
        delFactory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
        delFactory.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
        delFactory.SetValue(DockPanel.DockProperty, Dock.Right);
        delFactory.SetValue(Button.CommandProperty, _viewModel.DeletePresetCommand);
        delFactory.SetValue(Button.CommandParameterProperty, new Binding());
        delFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, ev) =>
        {
            // Prevent selecting the item when clicking delete
            ev.Handled = true;
        }));

        // Name text
        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetValue(TextBlock.TextProperty, new Binding("Name"));
        textFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        textFactory.SetValue(TextBlock.MarginProperty, new Thickness(12, 0, 8, 0));

        panelFactory.AppendChild(delFactory);
        panelFactory.AppendChild(textFactory);
        itemTemplate.VisualTree = panelFactory;
        listBox.ItemTemplate = itemTemplate;

        // Item container style
        listBox.ItemContainerStyle = new Style(typeof(ListBoxItem))
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(8, 6, 8, 6)),
                new Setter(ListBoxItem.ForegroundProperty, textPrimary),
                new Setter(ListBoxItem.BackgroundProperty, Brushes.Transparent),
                new Setter(ListBoxItem.BorderThicknessProperty, new Thickness(0)),
                new Setter(ListBoxItem.TemplateProperty, CreateListBoxItemTemplate(controlBrush, controlHoverBrush))
            }
        };

        root.Children.Add(listBox);

        outerBorder.Child = root;
        popup.Content = outerBorder;
        popup.MouseLeftButtonDown += (s, ev) => popup.DragMove();
        popup.ShowDialog();
    }

    private static ControlTemplate CreateListBoxItemTemplate(Brush bg, Brush hoverBg)
    {
        var template = new ControlTemplate(typeof(ListBoxItem));
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.Name = "itemBorder";
        borderFactory.SetValue(Border.BackgroundProperty, bg);
        borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        borderFactory.SetValue(Border.MarginProperty, new Thickness(0, 1, 0, 1));

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        borderFactory.AppendChild(presenter);

        template.VisualTree = borderFactory;

        var selectedTrigger = new Trigger { Property = ListBoxItem.IsSelectedProperty, Value = true };
        selectedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, hoverBg, "itemBorder"));
        template.Triggers.Add(selectedTrigger);

        return template;
    }

    // ── Window Chrome ──

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // Apply rounded corners on Windows 11+
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int preference = DWMWCP_ROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
        }
        catch
        {
            // Not Windows 11+, ignore
        }
    }

    // ── Title Bar Button Handlers ──

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click title bar → toggle maximize
            ToggleMaximize();
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Hide to tray instead of exiting
        Hide();
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
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
