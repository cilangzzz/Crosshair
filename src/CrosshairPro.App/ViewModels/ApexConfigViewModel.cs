using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;
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
        { "最高", -1 }, { "高", 0 }, { "中", 1 }, { "低", 2 }
    };

    /// <summary>窗口模式选项</summary>
    public Dictionary<string, int> WindowModeOptions { get; } = new()
    {
        { "全屏", 0 }, { "窗口化", 1 }, { "无边框窗口", 2 }
    };

    /// <summary>粒子效果选项</summary>
    public Dictionary<string, int> ParticleLevelOptions { get; } = new()
    {
        { "低", 0 }, { "中", 1 }, { "高", 2 }
    };

    /// <summary>SSAO 质量选项</summary>
    public Dictionary<string, int> SsaoQualityOptions { get; } = new()
    {
        { "关闭", 0 }, { "低", 1 }, { "高", 2 }
    };

    /// <summary>垂直同步选项</summary>
    public Dictionary<string, int> VsyncOptions { get; } = new()
    {
        { "关闭", 0 }, { "开启", 1 }, { "三重缓冲", 2 }
    };

    /// <summary>抗锯齿选项</summary>
    public Dictionary<string, int> AntialiasOptions { get; } = new()
    {
        { "关闭", 0 }, { "FXAA", 1 }, { "TXAA", 2 }, { "MSAA 2x", 3 }, { "MSAA 4x", 4 }
    };

    /// <summary>启动选项预设</summary>
    public ObservableCollection<string> LaunchOptionPresets { get; } = new()
    {
        "竞技优化",
        "高帧率",
        "调试模式",
        "自定义"
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

    // 事件
    public event EventHandler<string>? ToastRequested;

    public ApexConfigViewModel(IApexConfigService apexService)
    {
        _apexService = apexService;

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
                VideoConfigPath = _apexService.GetVideoConfigPath() ?? "未找到";
                SettingsConfigPath = _apexService.GetSettingsConfigPath() ?? "未找到";

                // 加载配置
                VideoConfig = await _apexService.LoadVideoConfigAsync();
                SettingsConfig = await _apexService.LoadSettingsConfigAsync();
                LaunchOptions = await _apexService.LoadLaunchOptionsAsync();
            }
            else
            {
                StatusMessage = "未检测到 Apex Legends 安装";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载配置失败: {ex.Message}";
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
            Filter = "Video Config Files|videoconfig.txt|All Files|*.*",
            Title = "选择 videoconfig.txt 文件"
        };

        if (dlg.ShowDialog() == true)
        {
            var success = await _apexService.ReplaceVideoConfigAsync(dlg.FileName);
            if (success)
            {
                ShowToast("配置文件已替换并备份");
                VideoConfig = await _apexService.LoadVideoConfigAsync();
            }
            else
            {
                ShowToast("替换失败：文件格式无效");
            }
        }
    }

    [RelayCommand]
    private async Task ReplaceSettingsConfig()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Settings Files|settings.cfg|All Files|*.*",
            Title = "选择 settings.cfg 文件"
        };

        if (dlg.ShowDialog() == true)
        {
            var success = await _apexService.ReplaceSettingsConfigAsync(dlg.FileName);
            if (success)
            {
                ShowToast("配置文件已替换并备份");
                SettingsConfig = await _apexService.LoadSettingsConfigAsync();
            }
            else
            {
                ShowToast("替换失败：文件格式无效");
            }
        }
    }

    [RelayCommand]
    private async Task BackupVideoConfig()
    {
        try
        {
            var backupPath = await _apexService.BackupVideoConfigAsync();
            ShowToast($"已备份到: {Path.GetFileName(backupPath)}");
        }
        catch (Exception ex)
        {
            ShowToast($"备份失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BackupSettingsConfig()
    {
        try
        {
            var backupPath = await _apexService.BackupSettingsConfigAsync();
            ShowToast($"已备份到: {Path.GetFileName(backupPath)}");
        }
        catch (Exception ex)
        {
            ShowToast($"备份失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ExportVideoConfig()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Video Config Files|videoconfig.txt|All Files|*.*",
            FileName = "videoconfig.txt",
            Title = "导出 videoconfig.txt"
        };

        if (dlg.ShowDialog() == true)
        {
            await _apexService.ExportVideoConfigAsync(dlg.FileName);
            ShowToast("配置已导出");
        }
    }

    [RelayCommand]
    private async Task ExportSettingsConfig()
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Settings Files|settings.cfg|All Files|*.*",
            FileName = "settings.cfg",
            Title = "导出 settings.cfg"
        };

        if (dlg.ShowDialog() == true)
        {
            await _apexService.ExportSettingsConfigAsync(dlg.FileName);
            ShowToast("配置已导出");
        }
    }

    [RelayCommand]
    private void CopyLaunchOptions()
    {
        if (!string.IsNullOrWhiteSpace(LaunchOptions))
        {
            Clipboard.SetText(LaunchOptions);
            ShowToast("启动选项已复制到剪贴板");
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
            "竞技优化" => _apexService.GetCompetitivePreset(),
            "高画质" => _apexService.GetHighQualityPreset(),
            _ => _apexService.GetDefaultPreset()
        };

        ShowToast($"已应用 {presetName} 预设");
    }

    // ═══════════════════════════════════════════════════════════
    // 保存和重置
    // ═══════════════════════════════════════════════════════════

    [RelayCommand]
    private async Task Save()
    {
        if (!IsApexInstalled)
        {
            ShowToast("未检测到 Apex Legends 安装");
            return;
        }

        try
        {
            await _apexService.SaveVideoConfigAsync(VideoConfig);
            await _apexService.SaveSettingsConfigAsync(SettingsConfig);
            await _apexService.SaveLaunchOptionsAsync(LaunchOptions);

            ShowToast("配置已保存");
        }
        catch (Exception ex)
        {
            ShowToast($"保存失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Reset()
    {
        VideoConfig = _apexService.GetDefaultPreset();
        SettingsConfig = new ApexSettingsConfig();
        LaunchOptions = _apexService.GenerateLaunchOptions();

        await Save();
        ShowToast("已重置为默认配置");
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

        LaunchOptions = value switch
        {
            0 => "-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -novid",
            1 => "-dev +fps_max 279 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -w 2560 -h 1600 -novid",
            2 => "-console -dev -condebug +fps_max 0",
            _ => LaunchOptions
        };
    }
}