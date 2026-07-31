using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 游戏配置页面 ViewModel
/// </summary>
public partial class GamesViewModel : ObservableObject
{
    private readonly IGameConfigService _gameConfigService;

    /// <summary>游戏列表</summary>
    public ObservableCollection<GameProfile> Games { get; }

    /// <summary>当前选中的游戏</summary>
    [ObservableProperty]
    private GameProfile? _selectedGame;

    /// <summary>当前游戏的配置策略</summary>
    [ObservableProperty]
    private GameConfigStrategy? _currentStrategy;

    /// <summary>当前游戏的配置</summary>
    [ObservableProperty]
    private GameConfig? _currentConfig;

    /// <summary>是否有未保存的更改</summary>
    [ObservableProperty]
    private bool _hasUnsavedChanges;

    public GamesViewModel(IGameConfigService gameConfigService)
    {
        _gameConfigService = gameConfigService;

        // 加载游戏列表
        Games = new ObservableCollection<GameProfile>(GameProfile.BuiltIn.GetAll());
    }

    /// <summary>
    /// 当选中游戏改变时加载配置
    /// </summary>
    partial void OnSelectedGameChanged(GameProfile? value)
    {
        if (value == null)
        {
            CurrentStrategy = null;
            CurrentConfig = null;
            return;
        }

        // 加载策略和配置
        CurrentStrategy = _gameConfigService.GetStrategy(value.Id);
        LoadConfigAsync(value.Id).ConfigureAwait(false);
    }

    private async Task LoadConfigAsync(string gameId)
    {
        var config = await _gameConfigService.GetConfigAsync(gameId);
        CurrentConfig = config;
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// 获取配置项的值
    /// </summary>
    public object? GetSettingValue(string key)
    {
        if (CurrentConfig == null) return null;
        return CurrentConfig.Settings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// 设置配置项的值
    /// </summary>
    public void SetSettingValue(string key, object value)
    {
        if (CurrentConfig == null) return;

        CurrentConfig.Settings[key] = value;
        HasUnsavedChanges = true;
        OnPropertyChanged(nameof(CurrentConfig));
    }

    // ── 命令 ───────────────────────────────────────────────────

    [RelayCommand]
    private async Task SaveConfig()
    {
        if (CurrentConfig == null) return;

        await _gameConfigService.SaveConfigAsync(CurrentConfig);
        HasUnsavedChanges = false;
        ToastRequested?.Invoke(this, "配置已保存");
    }

    [RelayCommand]
    private async Task ResetConfig()
    {
        if (SelectedGame == null) return;

        await _gameConfigService.ResetToDefaultAsync(SelectedGame.Id);
        await LoadConfigAsync(SelectedGame.Id);
        ToastRequested?.Invoke(this, "已重置为默认配置");
    }

    [RelayCommand]
    private async Task ApplyConfig()
    {
        if (SelectedGame == null) return;

        // 先保存
        if (HasUnsavedChanges && CurrentConfig != null)
        {
            await _gameConfigService.SaveConfigAsync(CurrentConfig);
        }

        // 应用到游戏
        await _gameConfigService.ApplyConfigAsync(SelectedGame.Id);
        ToastRequested?.Invoke(this, "配置已应用");
    }

    // ── 事件 ───────────────────────────────────────────────────

    public event EventHandler<string>? ToastRequested;
}