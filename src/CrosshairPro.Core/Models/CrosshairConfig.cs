using CommunityToolkit.Mvvm.ComponentModel;
using CrosshairPro.Core.Enums;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 准心配置
/// </summary>
public partial class CrosshairConfig : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "默认配置";

    [ObservableProperty]
    private CrosshairStyle _style = CrosshairStyle.Cross;

    [ObservableProperty]
    private int _size = 20;

    [ObservableProperty]
    private int _gap = 4;

    [ObservableProperty]
    private int _thickness = 2;

    [ObservableProperty]
    private string _color = "#00FF00";

    [ObservableProperty]
    private int _opacity = 100;

    [ObservableProperty]
    private int _brightness = 100;

    [ObservableProperty]
    private int _centerSize = 4;

    [ObservableProperty]
    private int _rotation;

    [ObservableProperty]
    private string? _customImagePath;

    [ObservableProperty]
    private EffectsConfig _effects = new();

    [ObservableProperty]
    private DisplayConfig _display = new();

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public CrosshairConfig Clone()
    {
        return new CrosshairConfig
        {
            Id = Guid.NewGuid().ToString(),
            Name = Name,
            Style = Style,
            Size = Size,
            Gap = Gap,
            Thickness = Thickness,
            Color = Color,
            Opacity = Opacity,
            Brightness = Brightness,
            CenterSize = CenterSize,
            Rotation = Rotation,
            CustomImagePath = CustomImagePath,
            Effects = new EffectsConfig
            {
                Outline = new OutlineConfig
                {
                    Enabled = Effects.Outline.Enabled,
                    Color = Effects.Outline.Color,
                    Thickness = Effects.Outline.Thickness
                },
                Shadow = new ShadowConfig
                {
                    Enabled = Effects.Shadow.Enabled,
                    Color = Effects.Shadow.Color,
                    Blur = Effects.Shadow.Blur,
                    OffsetX = Effects.Shadow.OffsetX,
                    OffsetY = Effects.Shadow.OffsetY
                },
                Glow = new GlowConfig
                {
                    Enabled = Effects.Glow.Enabled,
                    Color = Effects.Glow.Color,
                    Intensity = Effects.Glow.Intensity,
                    Range = Effects.Glow.Range
                }
            },
            Display = new DisplayConfig
            {
                Monitor = Display.Monitor,
                ClickThrough = Display.ClickThrough,
                AlwaysOnTop = Display.AlwaysOnTop,
                PositionX = Display.PositionX,
                PositionY = Display.PositionY
            }
        };
    }

    /// <summary>
    /// 从另一个配置复制值
    /// </summary>
    public void CopyFrom(CrosshairConfig other)
    {
        Name = other.Name;
        Style = other.Style;
        Size = other.Size;
        Gap = other.Gap;
        Thickness = other.Thickness;
        Color = other.Color;
        Opacity = other.Opacity;
        Brightness = other.Brightness;
        CenterSize = other.CenterSize;
        Rotation = other.Rotation;
        CustomImagePath = other.CustomImagePath;

        Effects.Outline.Enabled = other.Effects.Outline.Enabled;
        Effects.Outline.Color = other.Effects.Outline.Color;
        Effects.Outline.Thickness = other.Effects.Outline.Thickness;

        Effects.Shadow.Enabled = other.Effects.Shadow.Enabled;
        Effects.Shadow.Color = other.Effects.Shadow.Color;
        Effects.Shadow.Blur = other.Effects.Shadow.Blur;
        Effects.Shadow.OffsetX = other.Effects.Shadow.OffsetX;
        Effects.Shadow.OffsetY = other.Effects.Shadow.OffsetY;

        Effects.Glow.Enabled = other.Effects.Glow.Enabled;
        Effects.Glow.Color = other.Effects.Glow.Color;
        Effects.Glow.Intensity = other.Effects.Glow.Intensity;
        Effects.Glow.Range = other.Effects.Glow.Range;

        Display.Monitor = other.Display.Monitor;
        Display.ClickThrough = other.Display.ClickThrough;
        Display.AlwaysOnTop = other.Display.AlwaysOnTop;
        Display.PositionX = other.Display.PositionX;
        Display.PositionY = other.Display.PositionY;
    }
}

/// <summary>
/// 显示配置
/// </summary>
public partial class DisplayConfig : ObservableObject
{
    [ObservableProperty]
    private string _monitor = "primary";

    [ObservableProperty]
    private bool _clickThrough = true;

    [ObservableProperty]
    private bool _alwaysOnTop = true;

    [ObservableProperty]
    private int _positionX;

    [ObservableProperty]
    private int _positionY;
}
