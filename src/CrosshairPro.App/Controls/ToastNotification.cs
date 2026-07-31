using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrosshairPro.App.Controls;

/// <summary>
/// Toast 通知控件 - 短暂显示后自动消失
/// 使用主题系统，避免硬编码颜色
/// </summary>
public class ToastNotification : Control
{
    #region DependencyProperty

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string),
            typeof(ToastNotification), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(int),
            typeof(ToastNotification), new PropertyMetadata(3)); // 默认3秒

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius),
            typeof(ToastNotification), new PropertyMetadata(new CornerRadius(6)));

    #endregion

    #region Properties

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public int Duration
    {
        get => (int)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    private DispatcherTimer? _timer;
    private Panel? _container;

    static ToastNotification()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToastNotification),
            new FrameworkPropertyMetadata(typeof(ToastNotification)));
    }

    public ToastNotification()
    {
        Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 显示 Toast 通知
    /// </summary>
    public void Show(string message, int duration = 3)
    {
        Message = message;
        Duration = duration;
        Visibility = Visibility.Visible;

        // 启动定时器自动隐藏
        _timer?.Stop();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(duration)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer?.Stop();
        Hide();

        // 从容器中移除
        if (_container != null)
        {
            _container.Children.Remove(this);
        }
    }

    /// <summary>
    /// 隐藏 Toast 通知
    /// </summary>
    public void Hide()
    {
        Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 在指定容器中显示 Toast（自动移除）
    /// </summary>
    public static ToastNotification ShowIn(Panel container, string message, int duration = 3)
    {
        var toast = new ToastNotification();
        toast._container = container;
        container.Children.Add(toast);
        toast.Show(message, duration);
        return toast;
    }
}