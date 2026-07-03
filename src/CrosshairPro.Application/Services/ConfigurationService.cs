using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Enums;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Services;

/// <summary>
/// 配置管理服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigRepository _configRepository;
    private CrosshairConfig _currentConfig;

    public ConfigurationService(IConfigRepository configRepository)
    {
        _configRepository = configRepository;
        _currentConfig = CreateDefaultConfig();
    }

    /// <summary>
    /// 获取当前配置
    /// </summary>
    public CrosshairConfig GetCurrentConfig() => _currentConfig;

    /// <summary>
    /// 从持久化加载配置
    /// </summary>
    public async Task LoadConfigAsync()
    {
        _currentConfig = await _configRepository.LoadConfigAsync();
    }

    /// <summary>
    /// 保存配置到持久化
    /// </summary>
    public async Task SaveConfigAsync()
    {
        await _configRepository.SaveConfigAsync(_currentConfig);
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public async Task ResetToDefaultAsync()
    {
        _currentConfig = CreateDefaultConfig();
        await _configRepository.SaveConfigAsync(_currentConfig);
    }

    /// <summary>
    /// 克隆配置（深拷贝）
    /// </summary>
    public CrosshairConfig CloneConfig(CrosshairConfig source)
    {
        return new CrosshairConfig
        {
            Id = Guid.NewGuid().ToString(),
            Name = source.Name,
            Style = source.Style,
            Size = source.Size,
            Gap = source.Gap,
            Thickness = source.Thickness,
            Color = source.Color,
            Opacity = source.Opacity,
            Brightness = source.Brightness,
            CenterSize = source.CenterSize,
            Rotation = source.Rotation,
            CustomImagePath = source.CustomImagePath,
            Effects = new EffectsConfig
            {
                Outline = new OutlineConfig
                {
                    Enabled = source.Effects.Outline.Enabled,
                    Color = source.Effects.Outline.Color,
                    Thickness = source.Effects.Outline.Thickness
                },
                Shadow = new ShadowConfig
                {
                    Enabled = source.Effects.Shadow.Enabled,
                    Color = source.Effects.Shadow.Color,
                    Blur = source.Effects.Shadow.Blur,
                    OffsetX = source.Effects.Shadow.OffsetX,
                    OffsetY = source.Effects.Shadow.OffsetY
                },
                Glow = new GlowConfig
                {
                    Enabled = source.Effects.Glow.Enabled,
                    Color = source.Effects.Glow.Color,
                    Intensity = source.Effects.Glow.Intensity,
                    Range = source.Effects.Glow.Range
                }
            },
            Display = new DisplayConfig
            {
                Monitor = source.Display.Monitor,
                ClickThrough = source.Display.ClickThrough,
                AlwaysOnTop = source.Display.AlwaysOnTop,
                PositionX = source.Display.PositionX,
                PositionY = source.Display.PositionY
            }
        };
    }

    /// <summary>
    /// 复制配置（从源到目标）
    /// </summary>
    public void CopyConfig(CrosshairConfig source, CrosshairConfig target)
    {
        target.Name = source.Name;
        target.Style = source.Style;
        target.Size = source.Size;
        target.Gap = source.Gap;
        target.Thickness = source.Thickness;
        target.Color = source.Color;
        target.Opacity = source.Opacity;
        target.Brightness = source.Brightness;
        target.CenterSize = source.CenterSize;
        target.Rotation = source.Rotation;
        target.CustomImagePath = source.CustomImagePath;

        target.Effects.Outline.Enabled = source.Effects.Outline.Enabled;
        target.Effects.Outline.Color = source.Effects.Outline.Color;
        target.Effects.Outline.Thickness = source.Effects.Outline.Thickness;

        target.Effects.Shadow.Enabled = source.Effects.Shadow.Enabled;
        target.Effects.Shadow.Color = source.Effects.Shadow.Color;
        target.Effects.Shadow.Blur = source.Effects.Shadow.Blur;
        target.Effects.Shadow.OffsetX = source.Effects.Shadow.OffsetX;
        target.Effects.Shadow.OffsetY = source.Effects.Shadow.OffsetY;

        target.Effects.Glow.Enabled = source.Effects.Glow.Enabled;
        target.Effects.Glow.Color = source.Effects.Glow.Color;
        target.Effects.Glow.Intensity = source.Effects.Glow.Intensity;
        target.Effects.Glow.Range = source.Effects.Glow.Range;

        target.Display.Monitor = source.Display.Monitor;
        target.Display.ClickThrough = source.Display.ClickThrough;
        target.Display.AlwaysOnTop = source.Display.AlwaysOnTop;
        target.Display.PositionX = source.Display.PositionX;
        target.Display.PositionY = source.Display.PositionY;
    }

    /// <summary>
    /// 创建默认配置
    /// </summary>
    public CrosshairConfig CreateDefaultConfig()
    {
        return new CrosshairConfig
        {
            Name = "默认配置",
            Style = CrosshairStyle.Cross,
            Size = 20,
            Gap = 4,
            Thickness = 2,
            Color = "#00FF00",
            Opacity = 100,
            CenterSize = 4
        };
    }
}