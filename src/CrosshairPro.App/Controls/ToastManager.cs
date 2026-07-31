using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CrosshairPro.App.Controls;

/// <summary>
/// Toast 通知管理器 - 创建独立的悬浮窗口显示通知
/// 真正的悬浮效果，不影响主窗口布局
/// </summary>
public static class ToastManager
{
    private static readonly List<Window> _activeToasts = new();
    private static readonly object _lock = new();

    /// <summary>
    /// 显示 Toast 通知（悬浮窗口）
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <param name="duration">显示时长（秒）</param>
    /// <param name="owner">所属窗口（用于定位）</param>
    public static void Show(string message, int duration = 3, Window? owner = null)
    {
        var toastWindow = CreateToastWindow(message, owner);

        lock (_lock)
        {
            _activeToasts.Add(toastWindow);
        }

        toastWindow.Loaded += (s, e) =>
        {
            // 启动淡入动画
            StartFadeInAnimation(toastWindow);

            // 启动定时器自动关闭
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(duration)
            };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                CloseToastWithAnimation(toastWindow);
            };
            timer.Start();
        };

        toastWindow.Show();
    }

    /// <summary>
    /// 创建 Toast 窗口
    /// </summary>
    private static Window CreateToastWindow(string message, Window? owner)
    {
        var window = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            ShowInTaskbar = false,
            Topmost = true,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            Owner = owner
        };

        // 获取主题资源
        var bgBrush = GetThemeBrush("SurfaceBrush");
        var borderBrush = GetThemeBrush("BorderBrush");
        var textBrush = GetThemeBrush("TextPrimaryBrush");
        var accentBrush = GetThemeBrush("AccentBrush");
        var fontFamily = GetThemeFontFamily();

        // 创建内容 Border
        var border = new Border
        {
            Background = bgBrush,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(16, 10, 16, 10),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 16,
                ShadowDepth = 2,
                Opacity = 0.4
            }
        };

        // 创建文本
        var textBlock = new TextBlock
        {
            Text = message,
            Foreground = textBrush,
            FontSize = 14,
            FontFamily = fontFamily,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 350
        };

        border.Child = textBlock;
        window.Content = border;

        // 定位到屏幕底部居中
        PositionWindow(window, owner);

        // 窗口关闭时清理
        window.Closed += (s, e) =>
        {
            lock (_lock)
            {
                _activeToasts.Remove(window);
            }
        };

        return window;
    }

    /// <summary>
    /// 定位窗口到底部居中
    /// </summary>
    private static void PositionWindow(Window window, Window? owner)
    {
        window.Loaded += (s, e) =>
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // 如果有 owner，使用 owner 的位置
            if (owner != null && owner.WindowState == WindowState.Normal)
            {
                var ownerCenterX = owner.Left + owner.ActualWidth / 2;
                var ownerBottom = owner.Top + owner.ActualHeight;

                window.Left = ownerCenterX - window.ActualWidth / 2;
                window.Top = ownerBottom - window.ActualHeight - 60;
            }
            else
            {
                // 屏幕底部居中
                window.Left = (screenWidth - window.ActualWidth) / 2;
                window.Top = screenHeight - window.ActualHeight - 80;
            }

            // 确保不超出屏幕
            if (window.Left < 0) window.Left = 10;
            if (window.Left + window.ActualWidth > screenWidth)
                window.Left = screenWidth - window.ActualWidth - 10;
        };
    }

    /// <summary>
    /// 启动淡入动画
    /// </summary>
    private static void StartFadeInAnimation(Window window)
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        window.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    /// <summary>
    /// 带动画关闭 Toast
    /// </summary>
    private static void CloseToastWithAnimation(Window window)
    {
        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        animation.Completed += (s, e) =>
        {
            window.Close();
        };

        window.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    /// <summary>
    /// 关闭所有 Toast
    /// </summary>
    public static void CloseAll()
    {
        lock (_lock)
        {
            foreach (var toast in _activeToasts.ToList())
            {
                toast.Dispatcher.Invoke(() => toast.Close());
            }
            _activeToasts.Clear();
        }
    }

    /// <summary>
    /// 获取主题画刷
    /// </summary>
    private static SolidColorBrush GetThemeBrush(string key)
    {
        try
        {
            return (SolidColorBrush)System.Windows.Application.Current.FindResource(key);
        }
        catch
        {
            return Brushes.Gray;
        }
    }

    /// <summary>
    /// 获取主题字体
    /// </summary>
    private static FontFamily GetThemeFontFamily()
    {
        try
        {
            return (FontFamily)System.Windows.Application.Current.FindResource("FontFamilyMono");
        }
        catch
        {
            return new FontFamily("Consolas");
        }
    }
}