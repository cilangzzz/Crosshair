using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 描边效果配置
/// </summary>
public partial class OutlineConfig : ObservableObject
{
    [ObservableProperty]
    private bool _enabled = true;

    [ObservableProperty]
    private string _color = "#000000";

    [ObservableProperty]
    private int _thickness = 1;
}

/// <summary>
/// 阴影效果配置
/// </summary>
public partial class ShadowConfig : ObservableObject
{
    [ObservableProperty]
    private bool _enabled;

    [ObservableProperty]
    private string _color = "#000000";

    [ObservableProperty]
    private int _blur = 3;

    [ObservableProperty]
    private int _offsetX;

    [ObservableProperty]
    private int _offsetY = 2;
}

/// <summary>
/// 发光效果配置
/// </summary>
public partial class GlowConfig : ObservableObject
{
    [ObservableProperty]
    private bool _enabled;

    [ObservableProperty]
    private string _color = "#00FFFF";

    [ObservableProperty]
    private int _intensity = 50;

    [ObservableProperty]
    private int _range = 10;
}

/// <summary>
/// 效果配置
/// </summary>
public partial class EffectsConfig : ObservableObject
{
    [ObservableProperty]
    private OutlineConfig _outline = new();

    [ObservableProperty]
    private ShadowConfig _shadow = new();

    [ObservableProperty]
    private GlowConfig _glow = new();
}
