namespace CrosshairPro.Core.Enums;

/// <summary>
/// 应用状态枚举
/// </summary>
public enum AppState
{
    /// <summary>
    /// 空闲状态
    /// </summary>
    Idle = 0,

    /// <summary>
    /// 游戏模式（检测到游戏运行）
    /// </summary>
    GameMode = 1,

    /// <summary>
    /// 准心显示中
    /// </summary>
    CrosshairVisible = 2,

    /// <summary>
    /// 准心隐藏
    /// </summary>
    CrosshairHidden = 3
}