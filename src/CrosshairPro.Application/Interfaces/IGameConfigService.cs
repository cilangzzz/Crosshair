using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Interfaces;

/// <summary>
/// 游戏配置服务接口
/// 管理各游戏的配置数据
/// </summary>
public interface IGameConfigService
{
    /// <summary>
    /// 获取所有游戏配置策略
    /// </summary>
    IReadOnlyList<GameConfigStrategy> GetStrategies();

    /// <summary>
    /// 获取指定游戏的配置策略
    /// </summary>
    GameConfigStrategy? GetStrategy(string gameId);

    /// <summary>
    /// 获取指定游戏的配置
    /// </summary>
    Task<GameConfig?> GetConfigAsync(string gameId);

    /// <summary>
    /// 保存游戏配置
    /// </summary>
    Task SaveConfigAsync(GameConfig config);

    /// <summary>
    /// 重置游戏配置到默认值
    /// </summary>
    Task ResetToDefaultAsync(string gameId);

    /// <summary>
    /// 应用配置到游戏（写入配置文件）
    /// </summary>
    Task ApplyConfigAsync(string gameId);
}