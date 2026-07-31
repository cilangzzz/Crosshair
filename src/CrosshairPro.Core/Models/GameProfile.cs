using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 游戏配置
/// </summary>
public partial class GameProfile : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _processName = string.Empty;

    [ObservableProperty]
    private int _priority = 100;

    [ObservableProperty]
    private bool _autoSwitch = true;

    [ObservableProperty]
    private string? _presetId;

    [ObservableProperty]
    private bool _fullscreenOnly;

    [ObservableProperty]
    private DateTime? _lastMatchedAt;

    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// 检查是否匹配进程名
    /// </summary>
    public bool Matches(string processName)
    {
        return ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 内置游戏配置
    /// </summary>
    public static class BuiltIn
    {
        public static IEnumerable<GameProfile> GetAll()
        {
            return new[]
            {
                new GameProfile
                {
                    Id = "builtin-cs2",
                    DisplayName = "Counter-Strike 2",
                    ProcessName = "cs2",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-csgo",
                    DisplayName = "CS:GO",
                    ProcessName = "csgo",
                    Priority = 90
                },
                new GameProfile
                {
                    Id = "builtin-valorant",
                    DisplayName = "Valorant",
                    ProcessName = "VALORANT-Win64-Shipping",
                    Priority = 100,
                    FullscreenOnly = true
                },
                new GameProfile
                {
                    Id = "builtin-apex",
                    DisplayName = "Apex Legends",
                    ProcessName = "r5apex",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-overwatch2",
                    DisplayName = "Overwatch 2",
                    ProcessName = "Overwatch",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-pubg",
                    DisplayName = "PUBG",
                    ProcessName = "TslGame",
                    Priority = 90
                },
                new GameProfile
                {
                    Id = "builtin-fortnite",
                    DisplayName = "Fortnite",
                    ProcessName = "FortniteClient-Win64-Shipping",
                    Priority = 90
                },
                new GameProfile
                {
                    Id = "builtin-r6",
                    DisplayName = "Rainbow Six Siege",
                    ProcessName = "RainbowSix",
                    Priority = 90
                }
            };
        }
    }
}