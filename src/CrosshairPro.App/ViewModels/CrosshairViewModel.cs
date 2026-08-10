using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.App.Localization;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 准心配置页面 ViewModel
/// 管理准心配置、预设、预览渲染
/// </summary>
public partial class CrosshairViewModel : ObservableObject
{
    [ObservableProperty]
    private CrosshairConfig _config;

    [ObservableProperty]
    private bool _isCrosshairVisible = true;

    [ObservableProperty]
    private string _statusMessage = LocalizationProvider.Instance["Status.CrosshairEnabled"];

    [ObservableProperty]
    private string _currentPresetName = LocalizationProvider.Instance["Preset.Default"];

    [ObservableProperty]
    private int _selectedStyleIndex;

    private readonly IPresetService _presetService;
    private readonly IConfigurationService _configService;
    private string? _currentPresetId;
    private bool _isInitializing = true;

    public CrosshairViewModel(
        IPresetService presetService,
        IConfigurationService configService)
    {
        _presetService = presetService;
        _configService = configService;

        _config = _configService.GetCurrentConfig();
        SubscribeConfigEvents(_config);

        InitializeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 异步初始化：加载预设并恢复上次使用的配置
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            // 1. 加载应用状态
            _currentPresetId = await _presetService.GetCurrentPresetIdAsync();
            var savedPresetId = _currentPresetId;

            // 2. 加载所有预设
            await LoadPresets();

            // 3. 尝试恢复上次使用的预设
            if (!string.IsNullOrEmpty(savedPresetId))
            {
                var targetPreset = Presets.FirstOrDefault(p => p.Id == savedPresetId);
                if (targetPreset != null)
                {
                    SelectedPreset = targetPreset;
                    _configService.CopyConfig(targetPreset.Config, Config);
                    CurrentPresetName = targetPreset.Name;
                    ShowToast(LocalizationProvider.GetFormatted("Toast.PresetRestored", targetPreset.Name));
                }
                else
                {
                    _currentPresetId = "default";
                    SelectedPreset = Presets.FirstOrDefault();
                    ShowToast(LocalizationProvider.Get("Toast.PresetNotFound"));
                }
            }
            else
            {
                _currentPresetId = "default";
                SelectedPreset = Presets.FirstOrDefault();
            }

            _isInitializing = false;

            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to initialize CrosshairViewModel");
            _isInitializing = false;
            SelectedPreset = Presets.FirstOrDefault();
            ShowToast(LocalizationProvider.Get("Toast.ConfigLoadFailed"));
        }
    }

    // ==================== 事件 ====================

    public event EventHandler? ConfigUpdated;
    public event EventHandler? ToggleCrosshairRequested;
    public event EventHandler? SelectImageRequested;
    public event EventHandler? SavePresetRequested;
    public event EventHandler? ImportPresetRequested;
    public event EventHandler? ExportPresetRequested;
    public event EventHandler<string>? ToastRequested;

    /// <summary>
    /// 显示悬浮提示
    /// </summary>
    private void ShowToast(string message)
    {
        ToastRequested?.Invoke(this, message);
    }

    // ==================== 属性 ====================

    public string[] CrosshairStyleNames { get; } = new[]
    {
        LocalizationProvider.Get("CrosshairStyle.Cross"),
        LocalizationProvider.Get("CrosshairStyle.Dot"),
        LocalizationProvider.Get("CrosshairStyle.Circle"),
        LocalizationProvider.Get("CrosshairStyle.TShape"),
        LocalizationProvider.Get("CrosshairStyle.XShape"),
        LocalizationProvider.Get("CrosshairStyle.CustomImage")
    };

    public string[] PresetColors { get; } = new[]
    {
        "#00FF00", "#00FFFF", "#FFFF00", "#FF0000",
        "#FF00FF", "#FFA500", "#FFFFFF", "#000000"
    };

    /// <summary>预设列表</summary>
    [ObservableProperty]
    private List<Preset> _presets = new();

    /// <summary>当前选中的预设</summary>
    [ObservableProperty]
    private Preset? _selectedPreset;

    partial void OnSelectedStyleIndexChanged(int value)
    {
        if (value >= 0 && value < Enum.GetValues(typeof(CrosshairStyle)).Length)
            Config.Style = (CrosshairStyle)value;
    }

    partial void OnSelectedPresetChanged(Preset? value)
    {
        if (value == null) return;
        _configService.CopyConfig(value.Config, Config);
        CurrentPresetName = value.Name;
        _currentPresetId = value.Id;

        if (!_isInitializing)
        {
            _ = SaveCurrentStateAsync();
        }

        ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 保存当前状态（当前使用的预设ID）
    /// </summary>
    private async Task SaveCurrentStateAsync()
    {
        try
        {
            await _presetService.SetCurrentPresetAsync(_currentPresetId ?? "default");
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to save app state");
        }
    }

    // ==================== 命令 ====================

    [RelayCommand]
    private void SetColor(string color) => Config.Color = color;

    [RelayCommand]
    private void SelectImage() => SelectImageRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ToggleCrosshair()
    {
        IsCrosshairVisible = !IsCrosshairVisible;
        StatusMessage = IsCrosshairVisible
            ? LocalizationProvider.Get("Status.CrosshairEnabled")
            : LocalizationProvider.Get("Status.CrosshairDisabled");
        ToggleCrosshairRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ResetConfig()
    {
        var defaultConfig = _configService.CreateDefaultConfig();
        _configService.CopyConfig(defaultConfig, Config);
        SelectedStyleIndex = 0;
        CurrentPresetName = LocalizationProvider.Get("Preset.Default");
        _currentPresetId = "default";
        SubscribeConfigEvents(Config);

        _ = SaveCurrentStateAsync();

        ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SavePreset() => SavePresetRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ImportPreset() => ImportPresetRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void ExportPreset() => ExportPresetRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task DeletePreset(Preset? preset)
    {
        if (preset == null || preset.IsDefault) return;
        try
        {
            await _presetService.DeletePresetAsync(preset.Id);
            await LoadPresets();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to delete preset");
        }
    }

    // ==================== 业务方法 ====================

    /// <summary>
    /// 保存预设（由 MainWindow 调用，传入用户输入的名称）
    /// </summary>
    public async Task SavePresetWithNameAsync(string name)
    {
        var preset = new Preset
        {
            Name = name,
            Config = _configService.CloneConfig(Config),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _presetService.SavePresetAsync(preset);
        await LoadPresets();

        SelectedPreset = Presets.FirstOrDefault(p => p.Id == preset.Id) ?? Presets.FirstOrDefault();
        CurrentPresetName = name;
        _currentPresetId = preset.Id;

        await SaveCurrentStateAsync();

        ShowToast(LocalizationProvider.GetFormatted("Toast.PresetSaved", name));
    }

    /// <summary>
    /// 从文件导入预设
    /// </summary>
    public async Task ImportPresetFromFileAsync(string filePath)
    {
        try
        {
            var preset = await _presetService.ImportPresetAsync(filePath);
            await LoadPresets();
            SelectedPreset = preset;
            _currentPresetId = preset.Id;
            await SaveCurrentStateAsync();
            ShowToast(LocalizationProvider.GetFormatted("Toast.PresetImported", preset.Name));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.ImportFailed", ex.Message));
        }
    }

    /// <summary>
    /// 导出当前配置到文件
    /// </summary>
    public async Task ExportPresetToFileAsync(string filePath)
    {
        try
        {
            var preset = new Preset
            {
                Name = CurrentPresetName,
                Config = _configService.CloneConfig(Config)
            };
            await _presetService.ExportPresetAsync(preset, filePath);
            ShowToast(LocalizationProvider.GetFormatted("Toast.ConfigExported", System.IO.Path.GetFileName(filePath)));
        }
        catch (Exception ex)
        {
            ShowToast(LocalizationProvider.GetFormatted("Toast.ExportFailed", ex.Message));
        }
    }

    /// <summary>
    /// 加载所有预设
    /// </summary>
    private async Task LoadPresets()
    {
        try
        {
            var presets = await _presetService.LoadAllPresetsAsync();
            Presets = presets.ToList();

            if (!_isInitializing && (SelectedPreset == null || !Presets.Any(p => p.Id == SelectedPreset.Id)))
            {
                SelectedPreset = Presets.FirstOrDefault();
            }
        }
        catch
        {
            Presets = new List<Preset>
            {
                new()
                {
                    Id = "default",
                    Name = LocalizationProvider.Get("Preset.Default"),
                    Config = new CrosshairConfig(),
                    IsDefault = true
                }
            };
        }
    }

    // ==================== 内部方法 ====================

    private void SubscribeConfigEvents(CrosshairConfig config)
    {
        config.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
        config.Effects.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
        config.Effects.Outline.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
        config.Effects.Shadow.PropertyChanged += (s, e) => { OnPropertyChanged(nameof(Config)); ConfigUpdated?.Invoke(this, EventArgs.Empty); };
    }
}