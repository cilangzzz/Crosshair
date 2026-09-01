using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.App.Localization;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// Counter-Strike 2 配置 ViewModel
/// 基于 <see cref="GameConfigStrategy"/>（builtin-cs2）暴露的字段，
/// 渲染游戏内可配置项。CrosshairPro 仅将配置保存到本地，
/// 不直接写入 video.txt / autoexec.cfg，避免与 VAC、Steam 云同步冲突。
/// </summary>
public partial class Cs2ConfigViewModel : ObservableObject
{
    private const string GameId = "builtin-cs2";
    private const int MaxLaunchOptionsLength = 1000;

    private readonly IGameConfigService _gameConfigService;
    private GameConfig _config;

    // ── 分区集合 ────────────────────────────────────────────────

    /// <summary>视频配置分区</summary>
    public ObservableCollection<ConfigSectionWrapper> Sections { get; } = new();

    // ── 启动项 ───────────────────────────────────────────────────

    [ObservableProperty]
    private string _launchOptions = string.Empty;

    /// <summary>启动项字符数（用于 Steam UI 长度限制提示）</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchOptionsSummary))]
    private int _launchOptionsLength;

    public string LaunchOptionsSummary =>
        LocalizationProvider.GetFormatted("Cs2.LaunchOptionsChars", LaunchOptionsLength);

    // ── 状态 ─────────────────────────────────────────────────────

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isCs2Running;

    /// <summary>Toast 通知事件，页面层订阅后渲染</summary>
    public event EventHandler<string>? ToastRequested;

    public Cs2ConfigViewModel(IGameConfigService gameConfigService)
    {
        _gameConfigService = gameConfigService;
        _config = new GameConfig { GameId = GameId };
    }

    /// <summary>
    /// 异步加载配置。
    /// </summary>
    public async Task LoadAsync()
    {
        var strategy = _gameConfigService.GetStrategy(GameId);
        if (strategy == null)
        {
            StatusMessage = LocalizationProvider.Get("Path.NotFound");
            return;
        }

        var loaded = await _gameConfigService.GetConfigAsync(GameId);
        _config = loaded ?? new GameConfig { GameId = GameId };
        var settings = new Dictionary<string, object?>();
        foreach (var kvp in _config.Settings)
        {
            settings[kvp.Key] = kvp.Value;
        }

        Sections.Clear();
        foreach (var section in strategy.Sections)
        {
            Sections.Add(new ConfigSectionWrapper(section, settings));
        }

        LaunchOptions = _config.LaunchOptions ?? string.Empty;
        LaunchOptionsLength = LaunchOptions.Length;
        IsCs2Running = IsProcessRunning("cs2");

        StatusMessage = string.Empty;
    }

    partial void OnLaunchOptionsChanged(string value)
    {
        LaunchOptionsLength = value?.Length ?? 0;
    }

    // ── 命令 ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var config = await _gameConfigService.GetConfigAsync(GameId);
            if (config == null)
            {
                config = new GameConfig { GameId = GameId };
            }

            config.LaunchOptions = LaunchOptions ?? string.Empty;

            // 从 wrapper 收集 settings（item.Value 可能为 null，跳过 null 值）
            foreach (var section in Sections)
            {
                foreach (var item in section.Items)
                {
                    if (item.Value != null)
                    {
                        config.Settings[item.Key] = item.Value;
                    }
                }
            }

            await _gameConfigService.SaveConfigAsync(config);
            _config = config;

            var msg = LocalizationProvider.Get("Cs2.SaveSuccess");
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
        catch (Exception ex)
        {
            var msg = LocalizationProvider.GetFormatted("Cs2.SaveFailed", ex.Message);
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            await _gameConfigService.ResetToDefaultAsync(GameId);
            await LoadAsync();
            var msg = LocalizationProvider.Get("Cs2.ResetSuccess");
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
        catch (Exception ex)
        {
            var msg = LocalizationProvider.GetFormatted("Cs2.SaveFailed", ex.Message);
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
    }

    [RelayCommand]
    private void CopyLaunchOptions()
    {
        try
        {
            System.Windows.Clipboard.SetText(LaunchOptions ?? string.Empty);
            var msg = LocalizationProvider.Get("Cs2.SavedToClipboard");
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            ToastRequested?.Invoke(this, ex.Message);
        }
    }

    [RelayCommand]
    private void RefreshRunningState()
    {
        IsCs2Running = IsProcessRunning("cs2");
    }

    // ── 工具方法 ─────────────────────────────────────────────────

    private static bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        catch
        {
            return false;
        }
    }
}