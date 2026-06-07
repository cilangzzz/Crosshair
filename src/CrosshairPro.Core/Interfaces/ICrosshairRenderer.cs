using CrosshairPro.Core.Models;

namespace CrosshairPro.Core.Interfaces;

/// <summary>
/// 准心渲染器接口
/// </summary>
public interface ICrosshairRenderer
{
    /// <summary>
    /// 渲染准心
    /// </summary>
    void Render(object drawingContext, CrosshairConfig config, double width, double height);

    /// <summary>
    /// 渲染完成事件
    /// </summary>
    event EventHandler<RenderCompletedEventArgs>? RenderCompleted;
}

/// <summary>
/// 渲染完成事件参数
/// </summary>
public class RenderCompletedEventArgs : EventArgs
{
    public double RenderTimeMs { get; set; }
    public bool Success { get; set; }
}