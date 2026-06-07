namespace CrosshairPro.Core.Models;

/// <summary>
/// 游戏信息
/// </summary>
public record GameInfo(
    string ProcessName,
    string DisplayName,
    int ProcessId,
    DateTime StartTime);

/// <summary>
/// 游戏检测事件参数
/// </summary>
public class GameDetectedEventArgs : EventArgs
{
    public GameInfo Game { get; }
    public bool IsStarting { get; }

    public GameDetectedEventArgs(GameInfo game, bool isStarting)
    {
        Game = game;
        IsStarting = isStarting;
    }
}