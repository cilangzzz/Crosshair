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

    // 进入自定义模式解析既有字符串期间抑制生成回写（避免逐属性触发覆盖）
    private bool _isLoadingOptionsFromString;

    // 事件
    public event EventHandler<string>? ToastRequested;

    public ApexConfigViewModel(IApexConfigService apexService)
    {
        _apexService = apexService;

        // 订阅自定义启动选项的属性变更：启动参数面板常显，任意模式下编辑都实时回写启动项字符串（启动参数 → 启动项联动）；
        // 面板内容偏离所选预设时，预设回显切到「自定义」（guard 包裹置索引，面板即源头无需回读解析）
        _customLaunchOptions.PropertyChanged += (s, e) =>
        {
            if (_isLoadingOptionsFromString)
            {
                return;
            }

            LaunchOptions = CustomLaunchOptions.GenerateOptionsString();

            if (!_isInitializing && SelectedLaunchPresetIndex != 3)
            {
                _isLoadingOptionsFromString = true;
                try
                {
                    SelectedLaunchPresetIndex = 3;
                }
                finally
                {
                    _isLoadingOptionsFromString = false;
                }
            }
        };

        // 异步初始化（continuation 回 UI 线程：后续 LoadFromString 会在后台线程改绑定属性，与 UI 面板编辑构成竞态）
        InitializeAsync();
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

                // 面板常显：加载后立即回填面板，避免静止状态下面板（全 0）与字符串（文件内容）失同步的错觉。
                // 必须经 guard 包裹：否则逐属性赋值触发钩子把字符串重新生成，launch_options.txt 内容被静默改写
                // （未知令牌如 -condebug、显式 +fps_max 0 丢失）。此间钩子的预设回显切换由 _isInitializing 拦下
                SeedPanelFromLaunchOptions();
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
            var textToCopy = LaunchOptions;

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

    /// <summary>
    /// 将当前启动项导出为文本文件（用户选择保存位置）
    /// </summary>
    [RelayCommand]
    private async Task GenerateLaunchOptionsFile()
    {
        var dlg = new SaveFileDialog
        {
            Filter = LocalizationProvider.Get("Filter.TextFiles"),
            FileName = "apex_launch_options.txt",
            Title = LocalizationProvider.Get("Dialog.GenerateApexLaunchOptions")
        };

        if (dlg.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var options = LaunchOptions;
            await _apexService.ExportLaunchOptionsAsync(dlg.FileName, options ?? string.Empty);
            ShowToast(LocalizationProvider.GetFormatted("Game.GenerateConfigSuccess", Path.GetFileName(dlg.FileName)));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Game.GenerateConfigFailed", ex.Message));
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

        // 回显同步到预设 0（默认模板与预设 0 内容一致）；索引切换级联写串+回填面板，
        // 已是 0 时 setter 幂等跳过，由显式重置的字符串 + else 分支回填收敛
        if (SelectedLaunchPresetIndex != 0)
        {
            SelectedLaunchPresetIndex = 0;
        }
        else
        {
            SeedPanelFromLaunchOptions();
        }

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
            var preset = PresetStringFor(value);
            if (preset != null)
            {
                LaunchOptions = preset;
            }

            // 启动参数面板常显：选预设后同步回填面板，避免面板残留上一次的值
            SeedPanelFromLaunchOptions();
        }
    }

    /// <summary>内置预设字符串；非预设索引返回 null（调用方保持原字符串不变）</summary>
    private static string? PresetStringFor(int index) => index switch
    {
        0 => "-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -novid",
        1 => "-dev +fps_max 279 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -w 2560 -h 1600 -novid",
        2 => "-console -dev -condebug +fps_max 0",
        _ => null
    };

    /// <summary>
    /// 文本模式手改启动项字符串失焦时由视图调用（面板常显，须与字符串保持一致）：
    /// 字符串已偏离所选预设时回显切「自定义」并级联回填面板，此后面板编辑从手改内容继续生成
    /// </summary>
    public void NotifyLaunchOptionsEdited()
    {
        if (_isInitializing || _isLoadingOptionsFromString)
        {
            return;
        }

        if (SelectedLaunchPresetIndex is >= 0 and <= 2)
        {
            if (LaunchOptions == PresetStringFor(SelectedLaunchPresetIndex))
            {
                // 未偏离预设：仅回填面板，回显保持所选预设
                SeedPanelFromLaunchOptions();
                return;
            }

            if (SelectedLaunchPresetIndex != 3)
            {
                SelectedLaunchPresetIndex = 3; // 偏离预设：切「自定义」回显（级联触发 OnUseCustomLaunchOptionsChanged 回填面板）
                return;
            }
        }

        SeedPanelFromLaunchOptions();
    }

    partial void OnUseCustomLaunchOptionsChanged(bool value)
    {
        if (!value || _isLoadingOptionsFromString)
        {
            return;
        }

        // 进入自定义模式：把当前启动项字符串解析回结构化选项（启动项 → 启动参数联动），
        // 未知令牌被忽略；此后改动结构化选项会重新生成字符串（未知令牌丢失，见 GameConfig/Apex/pitfalls.md）
        SeedPanelFromLaunchOptions();
    }

    /// <summary>
    /// 把当前启动项字符串解析回结构化面板（启动项 → 启动参数联动）。
    /// 解析先重置后填充，保证面板与字符串内容严格一致；解析期间抑制生成回写（防回环）
    /// </summary>
    private void SeedPanelFromLaunchOptions()
    {
        _isLoadingOptionsFromString = true;
        try
        {
            CustomLaunchOptions.LoadFromString(LaunchOptions);
        }
        finally
        {
            _isLoadingOptionsFromString = false;
        }
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