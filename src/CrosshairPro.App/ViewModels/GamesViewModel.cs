using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 游戏配置页面 ViewModel
/// </summary>
public partial class GamesViewModel : ObservableObject
{
    /// <summary>游戏列表</summary>
    public ObservableCollection<GameProfile> Games { get; }

    /// <summary>当前选中的游戏ID</summary>
    [ObservableProperty]
    private string? _currentGameId;

    /// <summary>当前选中的游戏</summary>
    [ObservableProperty]
    private GameProfile? _selectedGame;

    public GamesViewModel()
    {
        // 加载游戏列表
        Games = new ObservableCollection<GameProfile>(GameProfile.BuiltIn.GetAll());

        // 默认选中 Apex Legends
        var apex = Games.FirstOrDefault(g => g.Id == "builtin-apex");
        if (apex != null)
        {
            SelectGameInternal(apex);
        }
    }

    /// <summary>
    /// 当选中游戏改变时
    /// </summary>
    partial void OnSelectedGameChanged(GameProfile? oldValue, GameProfile? newValue)
    {
        // 清除旧游戏的选中状态
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
        }

        // 设置新游戏的选中状态
        if (newValue != null)
        {
            newValue.IsSelected = true;
            CurrentGameId = newValue.Id;
        }
        else
        {
            CurrentGameId = null;
        }
    }

    /// <summary>
    /// 选择游戏（内部方法）
    /// </summary>
    public void SelectGameInternal(GameProfile game)
    {
        SelectedGame = game;
    }

    // ── 命令 ───────────────────────────────────────────────────

    [RelayCommand]
    private void SelectGame(GameProfile? game)
    {
        if (game != null)
        {
            SelectGameInternal(game);
        }
    }
}
