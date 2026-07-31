using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// 管理页面导航和全局状态
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPresetService _presetService;
    private readonly IConfigurationService _configService;

    /// <summary>准心配置页面 ViewModel</summary>
    public CrosshairViewModel CrosshairViewModel { get; }

    /// <summary>游戏配置页面 ViewModel</summary>
    public GamesViewModel GamesViewModel { get; }

    /// <summary>当前页面</summary>
    [ObservableProperty]
    private PageType _currentPage = PageType.Crosshair;

    /// <summary>是否显示准心页面</summary>
    public bool IsCrosshairPage => CurrentPage == PageType.Crosshair;

    /// <summary>是否显示游戏页面</summary>
    public bool IsGamesPage => CurrentPage == PageType.Games;

    public MainViewModel(
        IPresetService presetService,
        IConfigurationService configService,
        CrosshairViewModel crosshairViewModel,
        GamesViewModel gamesViewModel)
    {
        _presetService = presetService;
        _configService = configService;
        CrosshairViewModel = crosshairViewModel;
        GamesViewModel = gamesViewModel;

        // 订阅准心 ViewModel 的事件
        CrosshairViewModel.ToastRequested += (s, msg) => ToastRequested?.Invoke(s, msg);
        CrosshairViewModel.ConfigUpdated += (s, e) => ConfigUpdated?.Invoke(s, e);
        CrosshairViewModel.ToggleCrosshairRequested += (s, e) => ToggleCrosshairRequested?.Invoke(s, e);
        CrosshairViewModel.SelectImageRequested += (s, e) => SelectImageRequested?.Invoke(s, e);
        CrosshairViewModel.SavePresetRequested += (s, e) => SavePresetRequested?.Invoke(s, e);
        CrosshairViewModel.ImportPresetRequested += (s, e) => ImportPresetRequested?.Invoke(s, e);
        CrosshairViewModel.ExportPresetRequested += (s, e) => ExportPresetRequested?.Invoke(s, e);
    }

    // ==================== 事件 ====================

    public event EventHandler? ConfigUpdated;
    public event EventHandler? ToggleCrosshairRequested;
    public event EventHandler? SelectImageRequested;
    public event EventHandler? SavePresetRequested;
    public event EventHandler? ImportPresetRequested;
    public event EventHandler? ExportPresetRequested;
    public event EventHandler<string>? ToastRequested;

    // ==================== 命令 ====================

    /// <summary>导航到准心配置页面</summary>
    [RelayCommand]
    private void NavigateToCrosshair()
    {
        CurrentPage = PageType.Crosshair;
        OnPropertyChanged(nameof(IsCrosshairPage));
        OnPropertyChanged(nameof(IsGamesPage));
    }

    /// <summary>导航到游戏配置页面</summary>
    [RelayCommand]
    private void NavigateToGames()
    {
        CurrentPage = PageType.Games;
        OnPropertyChanged(nameof(IsCrosshairPage));
        OnPropertyChanged(nameof(IsGamesPage));
    }

    // ==================== 业务方法 ====================

    /// <summary>
    /// 保存预设（转发到 CrosshairViewModel）
    /// </summary>
    public async Task SavePresetWithNameAsync(string name)
    {
        await CrosshairViewModel.SavePresetWithNameAsync(name);
    }

    /// <summary>
    /// 导入预设（转发到 CrosshairViewModel）
    /// </summary>
    public async Task ImportPresetFromFileAsync(string filePath)
    {
        await CrosshairViewModel.ImportPresetFromFileAsync(filePath);
    }

    /// <summary>
    /// 导出预设（转发到 CrosshairViewModel）
    /// </summary>
    public async Task ExportPresetToFileAsync(string filePath)
    {
        await CrosshairViewModel.ExportPresetToFileAsync(filePath);
    }
}

/// <summary>
/// 页面类型枚举
/// </summary>
public enum PageType
{
    /// <summary>准心配置页面</summary>
    Crosshair,

    /// <summary>游戏配置页面</summary>
    Games
}
