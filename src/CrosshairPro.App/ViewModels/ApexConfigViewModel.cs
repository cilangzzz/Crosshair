using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.App.Localization;
using CrosshairPro.Core.Models;
using Microsoft.Win32;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// Apex Legends 配置 ViewModel
/// 管理 videoconfig.txt 和 settings.cfg 配置文件
/// </summary>
public partial class ApexConfigViewModel : ObservableObject
{
    private readonly IApexConfigService _apexService;
    private bool _isInitializing = true;

    // ═══════════════════════════════════════════════════════════
    // 配置数据
    // ═══════════════════════════════════════════════════════════

    [ObservableProperty]
    private ApexVideoConfig _videoConfig = new();

    [ObservableProperty]
    private ApexSettingsConfig _settingsConfig = new();

    [ObservableProperty]
    private string _launchOptions = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // 枚举选项
    // ═══════════════════════════════════════════════════════════

    /// <summary>分辨率选项</summary>
    public ObservableCollection<string> ResolutionOptions { get; } = new()
    {
        "3840x2160", "2560x1600", "2560x1440", "2560x1080",
        "1920x1200", "1920x1080", "1680x1050", "1600x900",
        "1440x900", "1280x1024", "1280x960", "1280x800", "1280x720"
    };

    /// <summary>纹理质量选项</summary>
    public Dictionary<string, int> TextureQualityOptions { get; } = new()
    {
        { LocalizationProvider.Get("Quality.Highest"), -1 },
        { LocalizationProvider.Get("Quality.High"), 0 },
        { LocalizationProvider.Get("Quality.Medium"), 1 },
        { LocalizationProvider.Get("Quality.Low"), 2 }
    };

    /// <summary>窗口模式选项</summary>
    public Dictionary<string, int> WindowModeOptions { get; } = new()
    {
        { LocalizationProvider.Get("WindowMode.Fullscreen"), 0 },
        { LocalizationProvider.Get("WindowMode.Windowed"), 1 },
        { LocalizationProvider.Get("WindowMode.Borderless"), 2 }
    };

    /// <summary>粒子效果选项</summary>
    public Dictionary<string, int> ParticleLevelOptions { get; } = new()
    {
        { LocalizationProvider.Get("Quality.Low"), 0 },
        { LocalizationProvider.Get("Quality.Medium"), 1 },
        { LocalizationProvider.Get("Quality.High"), 2 }
    };

    /// <summary>SSAO 质量选项</summary>
    public Dictionary<string, int> SsaoQualityOptions { get; } = new()
    {
        { LocalizationProvider.Get("Quality.Off"), 0 },
        { LocalizationProvider.Get("Quality.Low"), 1 },
        { LocalizationProvider.Get("Quality.High"), 2 }
    };

    /// <summary>垂直同步选项</summary>
    public Dictionary<string, int> VsyncOptions { get; } = new()
    {
        { LocalizationProvider.Get("Quality.Off"), 0 },
        { LocalizationProvider.Get("Quality.On"), 1 },
        { LocalizationProvider.Get("Quality.TripleBuffered"), 2 }
    };

    /// <summary>抗锯齿选项</summary>
    public Dictionary<string, int> AntialiasOptions { get; } = new()
    {
        { LocalizationProvider.Get("Quality.Off"), 0 },
        { "FXAA", 1 }, { "TXAA", 2 }, { "MSAA 2x", 3 }, { "MSAA 4x", 4 }
    };

    /// <summary>启动选项预设</summary>
    public ObservableCollection<string> LaunchOptionPresets { get; } = new()
    {
        LocalizationProvider.Get("Apex.Preset.Competitive"),
        LocalizationProvider.Get("Apex.Preset.HighQuality"),
        "调试模式",
        LocalizationProvider.Get("Apex.CustomLaunchOptions")
    };

    [ObservableProperty]
    private int _selectedLaunchPresetIndex = 0;

    // ═══════════════════════════════════════════════════════════
    // 状态属性
    // ═══════════════════════════════════════════════════════════

    [ObservableProperty]
    private bool _isApexInstalled = false;

    [ObservableProperty]
    private string _videoConfigPath = string.Empty;

    [ObservableProperty]
    private string _settingsConfigPath = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // 历史版本管理
    // ═══════════════════════════════════════════════════════════

    [ObservableProperty]
    private ObservableCollection<BackupFileInfo> _videoConfigBackups = new();

    [ObservableProperty]
    private ObservableCollection<BackupFileInfo> _settingsConfigBackups = new();

    [ObservableProperty]
    private bool _showVideoConfigHistory = false;

    [ObservableProperty]
    private bool _showSettingsConfigHistory = false;

    // ═══════════════════════════════════════════════════════════
    // 自定义启动选项
    // ═══════════════════════════════════════════════════════════

    [ObservableProperty]
    private ApexLaunchOptions _customLaunchOptions = new();

    [ObservableProperty]
    private bool _useCustomLaunchOptions = false;

    /// <summary>汇总的启动选项显示</summary>
    public string LaunchOptionsSummary => UseCustomLaunchOptions
        ? CustomLaunchOptions.GenerateOptionsString()
        : LaunchOptions;

    // 事件
    public event EventHandler<string>? ToastRequested;

    public ApexConfigViewModel(IApexConfigService apexService)
    {
        _apexService = apexService;

        // 订阅自定义启动选项的属性变更
        _customLaunchOptions.PropertyChanged += (s, e) =>
        {
            if (UseCustomLaunchOptions)
            {
                OnPropertyChanged(nameof(LaunchOptionsSummary));
            }
        };

        // 异步初始化
        InitializeAsync().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        try
        {
            // 检测 Apex Legends 是否安装
            IsApexInstalled = _apexService.IsApexInstalled();

            if (IsApexInstalled)
            {
                // 加载配置文件路径
                VideoConfigPath = _apexService.GetVideoConfigPath() ?? LocalizationProvider.Get("Path.NotFound");
                SettingsConfigPath = _apexService.GetSettingsConfigPath() ?? LocalizationProvider.Get("Path.NotFound");

                // 加载配置
                VideoConfig = await _apexService.LoadVideoConfigAsync();
                SettingsConfig = await _apexService.LoadSettingsConfigAsync();
                LaunchOptions = await _apexService.LoadLaunchOptionsAsync();
            }
            else
            {
                StatusMessage = LocalizationProvider.Get("Toast.ApexNotInstalled");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = LocalizationProvider.GetFormatted("Toast.SaveFailed", ex.Message);
        }
        finally
        {
            _isInitializing = false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 文件操作命令
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ReplaceVideoConfig()
    {
        var dlg = new OpenFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.VideoConfig"),
            Title = LocalizationProvider.Get("Dialog.SelectVideoConfig")
        };

        if (dlg.ShowDialog() == true)
        {
            var success = await _apexService.ReplaceVideoConfigAsync(dlg.FileName);
            if (success)
            {
                ShowToast(LocalizationProvider.Get("Toast.ConfigReplaced"));
                VideoConfig = await _apexService.LoadVideoConfigAsync();
            }
            else
            {
                ShowToast(LocalizationProvider.Get("Toast.ReplaceFailed"));
            }
        }
    }

    [RelayCommand]
    private async Task ReplaceSettingsConfig()
    {
        var dlg = new OpenFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.SettingsConfig"),
            Title = LocalizationProvider.Get("Dialog.SelectSettingsConfig")
        };

        if (dlg.ShowDialog() == true)
        {
            var success = await _apexService.ReplaceSettingsConfigAsync(dlg.FileName);
            if (success)
            {
                ShowToast(LocalizationProvider.Get("Toast.ConfigReplaced"));
                SettingsConfig = await _apexService.LoadSettingsConfigAsync();
            }
            else
            {
                ShowToast(LocalizationProvider.Get("Toast.ReplaceFailed"));
            }
        }
    }

    [RelayCommand]
    private async Task BackupVideoConfig()
    {
        try
        {
            var backupPath = await _apexService.BackupVideoConfigAsync();
            ShowToast(LocalizationProvider.GetFormatted("Toast.BackedUp", Path.GetFileName(backupPath)));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.BackupFailed", ex.Message));
        }
    }

    [RelayCommand]
    private async Task BackupSettingsConfig()
    {
        try
        {
            var backupPath = await _apexService.BackupSettingsConfigAsync();
            ShowToast(LocalizationProvider.GetFormatted("Toast.BackedUp", Path.GetFileName(backupPath)));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.BackupFailed", ex.Message));
        }
    }

    [RelayCommand]
    private async Task ExportVideoConfig()
    {
        var dlg = new SaveFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.VideoConfig"),
            FileName = "videoconfig.txt",
            Title = LocalizationProvider.Get("Dialog.ExportVideoConfig")
        };

        if (dlg.ShowDialog() == true)
        {
            await _apexService.ExportVideoConfigAsync(dlg.FileName);
            ShowToast(LocalizationProvider.Get("Toast.ConfigExportedShort"));
        }
    }

    [RelayCommand]
    private async Task ExportSettingsConfig()
    {
        var dlg = new SaveFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.SettingsConfig"),
            FileName = "settings.cfg",
            Title = LocalizationProvider.Get("Dialog.ExportSettingsConfig")
        };

        if (dlg.ShowDialog() == true)
        {
            await _apexService.ExportSettingsConfigAsync(dlg.FileName);
            ShowToast(LocalizationProvider.Get("Toast.ConfigExportedShort"));
        }
    }

    [RelayCommand]
    private void CopyLaunchOptions()
    {
        try
        {
            var textToCopy = UseCustomLaunchOptions ? LaunchOptionsSummary : LaunchOptions;

            if (!string.IsNullOrWhiteSpace(textToCopy))
            {
                // 在 UI 线程上执行剪贴板操作
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Clipboard.SetText(textToCopy);
                });
                ShowToast(LocalizationProvider.Get("Toast.LaunchOptionsCopied"));
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.CopyFailed", ex.Message));
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 预设命令
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task ApplyPreset(string presetName)
    {
        VideoConfig = presetName switch
        {
            _ when presetName == LocalizationProvider.Get("Apex.Preset.Competitive") => _apexService.GetCompetitivePreset(),
            _ when presetName == LocalizationProvider.Get("Apex.Preset.HighQuality") => _apexService.GetHighQualityPreset(),
            _ => _apexService.GetDefaultPreset()
        };

        ShowToast(LocalizationProvider.GetFormatted("Toast.PresetApplied", presetName));
    }

    // ═══════════════════════════════════════════════════════════
    // 保存和重置
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task Save()
    {
        if (!IsApexInstalled)
        {
            ShowToast(LocalizationProvider.Get("Toast.ApexNotInstalled"));
            return;
        }

        try
        {
            await _apexService.SaveVideoConfigAsync(VideoConfig);
            await _apexService.SaveSettingsConfigAsync(SettingsConfig);
            await _apexService.SaveLaunchOptionsAsync(LaunchOptions);

            ShowToast(LocalizationProvider.Get("Toast.ConfigSaved"));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.SaveFailed", ex.Message));
        }
    }

    [RelayCommand]
    private async Task Reset()
    {
        VideoConfig = _apexService.GetDefaultPreset();
        SettingsConfig = new ApexSettingsConfig();
        LaunchOptions = _apexService.GenerateLaunchOptions();

        await Save();
        ShowToast(LocalizationProvider.Get("Toast.ResetToDefault"));
    }

    // ═══════════════════════════════════════════════════════════
    // 辅助方法
    // ═══════════════════════════════════════════════════════════

    private void ShowToast(string message)
    {
        ToastRequested?.Invoke(this, message);
    }

    // 属性变更处理
    partial void OnSelectedLaunchPresetIndexChanged(int value)
    {
        if (_isInitializing) return;

        UseCustomLaunchOptions = value == 3; // "自定义" 选项

        if (!UseCustomLaunchOptions)
        {
            LaunchOptions = value switch
            {
                0 => "-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -novid",
                1 => "-dev +fps_max 279 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -w 2560 -h 1600 -novid",
                2 => "-console -dev -condebug +fps_max 0",
                _ => LaunchOptions
            };
        }
    }

    partial void OnUseCustomLaunchOptionsChanged(bool value)
    {
        OnPropertyChanged(nameof(LaunchOptionsSummary));
    }

    partial void OnCustomLaunchOptionsChanged(ApexLaunchOptions value)
    {
        OnPropertyChanged(nameof(LaunchOptionsSummary));
    }

    // ═══════════════════════════════════════════════════════════
    // 历史版本管理命令
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private void ToggleVideoConfigHistory()
    {
        ShowVideoConfigHistory = !ShowVideoConfigHistory;
        if (ShowVideoConfigHistory)
        {
            LoadVideoConfigBackups();
        }
    }

    [RelayCommand]
    private void ToggleSettingsConfigHistory()
    {
        ShowSettingsConfigHistory = !ShowSettingsConfigHistory;
        if (ShowSettingsConfigHistory)
        {
            LoadSettingsConfigBackups();
        }
    }

    private void LoadVideoConfigBackups()
    {
        try
        {
            var backups = _apexService.GetVideoConfigBackups();
            VideoConfigBackups.Clear();
            foreach (var backup in backups)
            {
                VideoConfigBackups.Add(backup);
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.LoadHistoryFailed", ex.Message));
        }
    }

    private void LoadSettingsConfigBackups()
    {
        try
        {
            var backups = _apexService.GetSettingsConfigBackups();
            SettingsConfigBackups.Clear();
            foreach (var backup in backups)
            {
                SettingsConfigBackups.Add(backup);
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.LoadHistoryFailed", ex.Message));
        }
    }

    [RelayCommand]
    private async Task RestoreVideoConfigBackup(BackupFileInfo? backup)
    {
        if (backup == null) return;

        try
        {
            var success = await _apexService.RestoreVideoConfigFromBackupAsync(backup.FilePath);
            if (success)
            {
                ShowToast(LocalizationProvider.GetFormatted("Toast.RestoredTo", backup.DisplayTime));
                VideoConfig = await _apexService.LoadVideoConfigAsync();
                LoadVideoConfigBackups(); // 刷新历史列表
            }
            else
            {
                ShowToast(LocalizationProvider.Get("Toast.RestoreFailed"));
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.RestoreFailedDetail", ex.Message));
        }
    }

    [RelayCommand]
    private async Task RestoreSettingsConfigBackup(BackupFileInfo? backup)
    {
        if (backup == null) return;

        try
        {
            var success = await _apexService.RestoreSettingsConfigFromBackupAsync(backup.FilePath);
            if (success)
            {
                ShowToast(LocalizationProvider.GetFormatted("Toast.RestoredTo", backup.DisplayTime));
                SettingsConfig = await _apexService.LoadSettingsConfigAsync();
                LoadSettingsConfigBackups();
            }
            else
            {
                ShowToast(LocalizationProvider.Get("Toast.RestoreFailed"));
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.RestoreFailedDetail", ex.Message));
        }
    }

    [RelayCommand]
    private void DeleteBackup(BackupFileInfo? backup)
    {
        if (backup == null) return;

        try
        {
            var success = _apexService.DeleteBackup(backup.FilePath);
            if (success)
            {
                ShowToast(LocalizationProvider.Get("Toast.BackupDeleted"));
                // 刷新列表
                if (ShowVideoConfigHistory)
                    LoadVideoConfigBackups();
                if (ShowSettingsConfigHistory)
                    LoadSettingsConfigBackups();
            }
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.DeleteFailed", ex.Message));
        }
    }
}