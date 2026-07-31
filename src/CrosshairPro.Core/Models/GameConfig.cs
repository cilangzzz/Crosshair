using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 游戏配置模型
/// 存储单个游戏的配置数据
/// </summary>
public partial class GameConfig : ObservableObject
{
    /// <summary>游戏ID（对应 GameProfile.Id）</summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>启动项参数</summary>
    [ObservableProperty]
    private string _launchOptions = string.Empty;

    /// <summary>配置项字典（键为配置项ID，值为配置值）</summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}