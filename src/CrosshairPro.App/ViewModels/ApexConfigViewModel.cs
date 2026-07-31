using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// Apex Legends 配置 ViewModel
/// </summary>
public partial class ApexConfigViewModel : ObservableObject
{
    private readonly IGameConfigService _gameConfigService;

    [ObservableProperty]
    private string _launchOptions = string.Empty;

    [ObservableProperty]
    private bool _fullscreen = true;

    [ObservableProperty]
    private int _fpsMax;

    [ObservableProperty]
    private int _refreshRate = 144;

    [ObservableProperty]
    private bool _showPos;

    [ObservableProperty]
    private bool _showFps;

    public ApexConfigViewModel(IGameConfigService gameConfigService)
    {
        _gameConfigService = gameConfigService;
        LoadConfigAsync().ConfigureAwait(false);
    }

    private async Task LoadConfigAsync()
    {
        var config = await _gameConfigService.GetConfigAsync("builtin-apex");
        if (config == null) return;

        LaunchOptions = config.LaunchOptions ?? string.Empty;
        
        if (config.Settings.TryGetValue("fullscreen", out var fullscreen))
            Fullscreen = fullscreen is bool b && b;
        
        if (config.Settings.TryGetValue("fps_max", out var fpsMax))
            FpsMax = fpsMax is int i ? i : 0;
        
        if (config.Settings.TryGetValue("refresh_rate", out var refresh))
            RefreshRate = refresh is int r ? r : 144;
        
        if (config.Settings.TryGetValue("cl_showpos", out var showPos))
            ShowPos = showPos is bool sp && sp;
        
        if (config.Settings.TryGetValue("cl_showfps", out var showFps))
            ShowFps = showFps is bool sf && sf;
    }

    [RelayCommand]
    private async Task Reset()
    {
        await _gameConfigService.ResetToDefaultAsync("builtin-apex");
        await LoadConfigAsync();
    }

    [RelayCommand]
    private async Task Apply()
    {
        var config = await _gameConfigService.GetConfigAsync("builtin-apex");
        if (config == null) return;

        config.LaunchOptions = LaunchOptions;
        config.Settings["fullscreen"] = Fullscreen;
        config.Settings["fps_max"] = FpsMax;
        config.Settings["refresh_rate"] = RefreshRate;
        config.Settings["cl_showpos"] = ShowPos;
        config.Settings["cl_showfps"] = ShowFps;

        await _gameConfigService.SaveConfigAsync(config);
        await _gameConfigService.ApplyConfigAsync("builtin-apex");
    }
}
