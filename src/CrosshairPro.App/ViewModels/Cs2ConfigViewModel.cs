using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.App.Localization;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Models;
using Microsoft.Win32;

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

    /// <summary>本地配置文件路径（gameconfigs/builtin-cs2.json）</summary>
    [ObservableProperty]
    private string _configFilePath = string.Empty;

    /// <summary>Toast 通知事件，页面层订阅后渲染</summary>
    public event EventHandler<string>? ToastRequested;

    public Cs2ConfigViewModel(IGameConfigService gameConfigService)
    {
        _gameConfigService = gameConfigService;
        _config = new GameConfig { GameId = GameId };
        ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrosshairPro", "gameconfigs", $"{GameId}.json");
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

    /// <summary>
    /// 在资源管理器中定位本地配置文件
    /// </summary>
    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            Process.Start("explorer.exe", $"/select,\"{ConfigFilePath}\"");
        }
        catch (Exception ex)
        {
            var msg = LocalizationProvider.GetFormatted("Game.OpenFolderFailed", ex.Message);
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
    }

    /// <summary>
    /// 从当前选项生成 exec 用的 .cfg 配置文件（用户选择保存位置，
    /// 不直接写入游戏目录；仅含安全白名单 cvar，UTF-8 无 BOM）
    /// </summary>
    [RelayCommand]
    private async Task GenerateConfigFileAsync()
    {
        var dlg = new SaveFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.CfgFiles"),
            FileName = "crosshairpro_cs2.cfg",
            Title = LocalizationProvider.Get("Dialog.GenerateCs2Config")
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var content = BuildConfigFileContent();
            await File.WriteAllTextAsync(dlg.FileName, content, new UTF8Encoding(false));

            var msg = LocalizationProvider.GetFormatted("Game.GenerateConfigSuccess", Path.GetFileName(dlg.FileName));
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
        catch (Exception ex)
        {
            var msg = LocalizationProvider.GetFormatted("Game.GenerateConfigFailed", ex.Message);
            StatusMessage = msg;
            ToastRequested?.Invoke(this, msg);
        }
    }

    // ── 工具方法 ─────────────────────────────────────────────────

    /// <summary>按 Key 读取当前 UI 上的配置值（未保存的修改也包含在内）；bool 渲染为 1/0（cvar 数值约定）</summary>
    private string GetSetting(string key)
    {
        var value = Sections.SelectMany(s => s.Items)
            .FirstOrDefault(i => i.Key == key)?.Value;
        if (value is bool b)
        {
            return b ? "1" : "0";
        }
        return value?.ToString() ?? string.Empty;
    }

    private string BuildConfigFileContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// ============================================================");
        sb.AppendLine("// CrosshairPro 生成的 CS2 配置文件");
        sb.AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("//");
        sb.AppendLine("// 使用方法:");
        sb.AppendLine("//   1. 将本文件放入 Steam 库目录:");
        sb.AppendLine("//      SteamLibrary/steamapps/common/Counter-Strike 2/game/csgo/cfg/");
        sb.AppendLine("//   2. 在 autoexec.cfg（或控制台）中执行: exec crosshairpro_cs2");
        sb.AppendLine("// ============================================================");
        sb.AppendLine();
        sb.AppendLine("// ── 游戏设置 (cvar) ──");
        sb.AppendLine($"fps_max \"{GetSetting("fps_max")}\"");
        sb.AppendLine($"cl_showfps \"{GetSetting("cl_showfps")}\"");
        sb.AppendLine();
        sb.AppendLine("// ── Steam 启动项（填入 Steam 启动参数，勿加入本文件执行）──");
        var launch = LaunchOptions?.Trim() ?? string.Empty;
        if (launch.Length > 0)
        {
            foreach (var line in launch.Split('\n'))
            {
                sb.AppendLine($"// {line.TrimEnd('\r')}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("// ── 视频设置（非 cvar，请在游戏内设置或 video.txt 中修改）──");
        sb.AppendLine($"// fullscreen: {GetSetting("fullscreen")}");
        sb.AppendLine($"// resolution: {GetSetting("resolution")}");
        sb.AppendLine($"// aspect_ratio: {GetSetting("aspect_ratio")}");
        sb.AppendLine($"// refresh_rate: {GetSetting("refresh_rate")}");
        return sb.ToString();
    }

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