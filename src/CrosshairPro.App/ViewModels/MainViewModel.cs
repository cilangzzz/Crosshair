using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Models;
using CrosshairPro.Services.Configuration;

namespace CrosshairPro.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private CrosshairConfig _config;

    [ObservableProperty]
    private bool _isCrosshairVisible = true;

    [ObservableProperty]
    private string _statusMessage = "准心已启用";

    [ObservableProperty]
    private string _currentPresetName = "默认配置";

    [ObservableProperty]
    private int _selectedStyleIndex;

    private readonly JsonPresetRepository _presetRepo = new();

    public MainViewModel()
    {
        _config = new CrosshairConfig();
        SubscribeConfigEvents(_config);
        LoadPresets();
    }

    // ==================== 事件 ====================

    public event EventHandler? ConfigUpdated;
    public event EventHandler? ToggleCrosshairRequested;

    // ==================== 属性 ====================

    public string[] CrosshairStyleNames { get; } = new[]
    {
        "十字准心", "点状准心", "圆形准心", "T形准心", "X形准心", "自定义图片"
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
        Config.CopyFrom(value.Config);
        CurrentPresetName = value.Name;
        ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    // ==================== 命令 ====================

    [RelayCommand]
    private void SetColor(string color) => Config.Color = color;

    /// <summary>
    /// 选择自定义图片（通知 MainWindow 打开文件对话框）
    /// </summary>
    [RelayCommand]
    private void SelectImage()
    {
        SelectImageRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SelectImageRequested;

    [RelayCommand]
    private void ToggleCrosshair()
    {
        IsCrosshairVisible = !IsCrosshairVisible;
        StatusMessage = IsCrosshairVisible ? "准心已启用" : "准心已禁用";
        ToggleCrosshairRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void ResetConfig()
    {
        Config = new CrosshairConfig();
        SelectedStyleIndex = 0;
        CurrentPresetName = "默认配置";
        SubscribeConfigEvents(Config);
        ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 保存当前配置为预设（通知 MainWindow 弹出命名对话框）
    /// </summary>
    [RelayCommand]
    private void SavePreset()
    {
        SavePresetRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 导入预设（通知 MainWindow 打开文件对话框）
    /// </summary>
    [RelayCommand]
    private void ImportPreset()
    {
        ImportPresetRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 导出当前配置（通知 MainWindow 打开保存对话框）
    /// </summary>
    [RelayCommand]
    private void ExportPreset()
    {
        ExportPresetRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 删除选中的预设
    /// </summary>
    [RelayCommand]
    private async Task DeletePreset(Preset? preset)
    {
        if (preset == null || preset.IsDefault) return;
        try
        {
            await _presetRepo.DeletePresetAsync(preset.Id);
            await LoadPresets();
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to delete preset");
        }
    }

    // ==================== 对话框请求事件 ====================

    public event EventHandler? SavePresetRequested;
    public event EventHandler? ImportPresetRequested;
    public event EventHandler? ExportPresetRequested;

    // ==================== 业务方法 ====================

    /// <summary>
    /// 保存预设（由 MainWindow 调用，传入用户输入的名称）
    /// </summary>
    public async Task SavePresetWithNameAsync(string name)
    {
        var preset = new Preset
        {
            Name = name,
            Config = Config.Clone(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _presetRepo.SavePresetAsync(preset);
        await LoadPresets();

        // 选中新保存的预设
        SelectedPreset = Presets.FirstOrDefault(p => p.Id == preset.Id) ?? Presets.FirstOrDefault();
        CurrentPresetName = name;
        StatusMessage = $"预设 \"{name}\" 已保存";
    }

    /// <summary>
    /// 从文件导入预设
    /// </summary>
    public async Task ImportPresetFromFileAsync(string filePath)
    {
        try
        {
            var preset = await _presetRepo.ImportPresetAsync(filePath);
            await LoadPresets();
            SelectedPreset = preset;
            StatusMessage = $"预设 \"{preset.Name}\" 已导入";
        }
        catch (Exception ex)
        {
            StatusMessage = $"导入失败: {ex.Message}";
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
                Config = Config.Clone()
            };
            await _presetRepo.ExportPresetAsync(preset.Id, filePath);

            // 临时保存再导出，或者直接写文件
            var json = System.Text.Json.JsonSerializer.Serialize(preset, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            await System.IO.File.WriteAllTextAsync(filePath, json);

            StatusMessage = $"配置已导出到 {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 加载所有预设
    /// </summary>
    private async Task LoadPresets()
    {
        try
        {
            var presets = await _presetRepo.LoadPresetsAsync();
            var list = presets.ToList();

            // 始终在最前面放一个默认预设
            var defaultPreset = new Preset
            {
                Id = "default",
                Name = "默认配置",
                Config = new CrosshairConfig(),
                IsDefault = true
            };
            list.Insert(0, defaultPreset);

            Presets = list;

            // 如果当前没有选中预设，或选中的预设已不在列表中，自动选中第一个
            if (SelectedPreset == null || !list.Any(p => p.Id == SelectedPreset.Id))
            {
                SelectedPreset = list.FirstOrDefault();
            }
        }
        catch
        {
            Presets = new List<Preset>
            {
                new()
                {
                    Id = "default",
                    Name = "默认配置",
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
