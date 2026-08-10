using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 支持平滑滚动动画的 ScrollViewer
/// 使用 DispatcherTimer 驱动平滑滚动，避免动画回弹
/// </summary>
public class SmoothScrollViewer : System.Windows.Controls.ScrollViewer
{
    #region Dependency Properties

    /// <summary>滚动动画持续时间（毫秒）</summary>
    public static readonly DependencyProperty ScrollDurationProperty =
        DependencyProperty.Register(nameof(ScrollDuration), typeof(int),
            typeof(SmoothScrollViewer), new PropertyMetadata(250));

    /// <summary>每次滚动的像素量（控制滚动速度）</summary>
    public static readonly DependencyProperty ScrollStepProperty =
        DependencyProperty.Register(nameof(ScrollStep), typeof(double),
            typeof(SmoothScrollViewer), new PropertyMetadata(32.0));

    /// <summary>缓动函数类型</summary>
    public static readonly DependencyProperty EasingModeProperty =
        DependencyProperty.Register(nameof(EasingMode), typeof(EasingMode),
            typeof(SmoothScrollViewer), new PropertyMetadata(EasingMode.EaseOut));

    /// <summary>是否启用平滑滚动</summary>
    public static readonly DependencyProperty IsSmoothScrollEnabledProperty =
        DependencyProperty.Register(nameof(IsSmoothScrollEnabled), typeof(bool),
            typeof(SmoothScrollViewer), new PropertyMetadata(true));

    #endregion

    #region Properties

    public int ScrollDuration
    {
        get => (int)GetValue(ScrollDurationProperty);
        set => SetValue(ScrollDurationProperty, value);
    }

    public double ScrollStep
    {
        get => (double)GetValue(ScrollStepProperty);
        set => SetValue(ScrollStepProperty, value);
    }

    public EasingMode EasingMode
    {
        get => (EasingMode)GetValue(EasingModeProperty);
        set => SetValue(EasingModeProperty, value);
    }

    public bool IsSmoothScrollEnabled
    {
        get => (bool)GetValue(IsSmoothScrollEnabledProperty);
        set => SetValue(IsSmoothScrollEnabledProperty, value);
    }

    #endregion

    #region Private Fields

    private double _startOffset;
    private double _targetOffset;
    private long _animationStartTime;
    private int _animationDuration;
    private bool _isAnimating;
    private CubicEase _easingFunction;

    #endregion

    #region Constructor

    public SmoothScrollViewer()
    {
        CanContentScroll = false;
        _easingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
        _isAnimating = false;
    }

    #endregion

    #region Overrides

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsSmoothScrollEnabled)
        {
            base.OnMouseWheel(e);
            return;
        }

        if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled)
        {
            base.OnMouseWheel(e);
            return;
        }

        e.Handled = true;

        // 计算目标偏移量
        double delta = e.Delta > 0 ? -ScrollStep : ScrollStep;
        double newTarget = Math.Max(0, Math.Min(ScrollableHeight, VerticalOffset + delta));

        // 如果目标相同，不处理
        if (Math.Abs(newTarget - VerticalOffset) < 0.1)
            return;

        // 开始新的滚动动画
        _startOffset = VerticalOffset;
        _targetOffset = newTarget;
        _animationStartTime = 0;
        _animationDuration = ScrollDuration;
        _isAnimating = true;

        // 启动渲染回调
        CompositionTarget.Rendering -= OnRendering;
        CompositionTarget.Rendering += OnRendering;
    }

    #endregion

    #region Private Methods

    private void OnRendering(object sender, EventArgs e)
    {
        if (!_isAnimating)
        {
            CompositionTarget.Rendering -= OnRendering;
            return;
        }

        // 获取当前时间（毫秒）
        var currentTime = Environment.TickCount;
        
        // 首次调用时初始化开始时间
        if (_animationStartTime == 0)
        {
            _animationStartTime = currentTime;
        }

        // 计算动画进度
        double elapsed = currentTime - _animationStartTime;
        double progress = Math.Min(1.0, elapsed / _animationDuration);

        // 应用缓动函数
        double easedProgress = _easingFunction.Ease(progress);

        // 计算当前偏移量
        double currentOffset = _startOffset + (_targetOffset - _startOffset) * easedProgress;

        // 设置滚动位置
        ScrollToVerticalOffset(currentOffset);

        // 检查是否完成
        if (progress >= 1.0)
        {
            _isAnimating = false;
            CompositionTarget.Rendering -= OnRendering;
            
            // 确保最终位置准确
            ScrollToVerticalOffset(_targetOffset);
        }
    }

    #endregion
}
