using CrosshairPro.Core.Models;

namespace CrosshairPro.Core.Interfaces;

/// <summary>
/// 游戏检测器接口
/// </summary>
public interface IGameDetector
{
    /// <summary>
    /// 当前检测到的游戏
    /// </summary>
    GameInfo? CurrentGame { get; }

    /// <summary>
    /// 开始监控
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// 停止监控
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// 初始化
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 注册游戏配置
    /// </summary>
    void RegisterGameProfile(GameProfile profile);

    /// <summary>
    /// 获取所有注册的游戏
    /// </summary>
    IEnumerable<GameProfile> GetRegisteredGames();

    /// <summary>
    /// 游戏启动事件
    /// </summary>
    event EventHandler<GameDetectedEventArgs>? GameStarted;

    /// <summary>
    /// 游戏退出事件
    /// </summary>
    event EventHandler<GameDetectedEventArgs>? GameExited;
}