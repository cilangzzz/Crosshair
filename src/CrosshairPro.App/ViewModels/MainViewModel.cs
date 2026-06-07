using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 主窗口ViewModel
/// </summary>
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

    public MainViewModel()
    {
        _config = new CrosshairConfig();

        // Config 内部任何属性变化 → 触发预览更新和外部通知
        _config.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        _config.Effects.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        _config.Effects.Outline.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        _config.Effects.Shadow.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
    }

    /// <summary>
    /// 配置更新事件 - MainWindow 订阅此事件同步到覆盖窗口
    /// </summary>
    public event EventHandler? ConfigUpdated;

    /// <summary>
    /// 准心样式名称列表
    /// </summary>
    public string[] CrosshairStyleNames { get; } = new[]
    {
        "十字准心",
        "点状准心",
        "圆形准心",
        "T形准心",
        "X形准心",
        "自定义图片"
    };

    /// <summary>
    /// 预设颜色列表
    /// </summary>
    public string[] PresetColors { get; } = new[]
    {
        "#00FF00", "#00FFFF", "#FFFF00", "#FF0000",
        "#FF00FF", "#FFA500", "#FFFFFF", "#000000"
    };

    /// <summary>
    /// 样式索引变化 → 同步到 Config.Style
    /// </summary>
    partial void OnSelectedStyleIndexChanged(int value)
    {
        if (value >= 0 && value < Enum.GetValues(typeof(CrosshairStyle)).Length)
        {
            Config.Style = (CrosshairStyle)value;
        }
    }

    /// <summary>
    /// 设置颜色
    /// </summary>
    [RelayCommand]
    private void SetColor(string color)
    {
        Config.Color = color;
    }

    /// <summary>
    /// 切换准心显示
    /// </summary>
    [RelayCommand]
    private void ToggleCrosshair()
    {
        IsCrosshairVisible = !IsCrosshairVisible;
        StatusMessage = IsCrosshairVisible ? "准心已启用" : "准心已禁用";
    }

    /// <summary>
    /// 重置配置
    /// </summary>
    [RelayCommand]
    private void ResetConfig()
    {
        Config = new CrosshairConfig();
        SelectedStyleIndex = 0;
        // 重新订阅新 Config 的属性变化
        SubscribeConfigChanges();
        ConfigUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void SubscribeConfigChanges()
    {
        Config.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        Config.Effects.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        Config.Effects.Outline.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
        Config.Effects.Shadow.PropertyChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Config));
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        };
    }
}
