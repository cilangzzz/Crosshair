using CommunityToolkit.Mvvm.ComponentModel;
using CrosshairPro.App.Localization;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 配置项包装类，用于 UI 绑定
/// </summary>
public partial class ConfigItemWrapper : ObservableObject
{
    private readonly Dictionary<string, object?> _settings;

    /// <summary>项 Key → 本地化键映射（策略 DisplayName 为硬编码中文，英文界面需走 resx）</summary>
    private static readonly Dictionary<string, string> LocKeyByItemKey = new()
    {
        ["fullscreen"] = "Cs2.Fullscreen",
        ["resolution"] = "Cs2.Resolution",
        ["aspect_ratio"] = "Cs2.AspectRatio",
        ["refresh_rate"] = "Cs2.RefreshRate",
        ["fps_max"] = "Cs2.MaxFps",
        ["cl_showfps"] = "Cs2.ShowFps",
    };

    public ConfigItemDefinition Definition { get; }

    public string Key => Definition.Key;

    /// <summary>
    /// 显示名：已映射的项 Key 取本地化文本（随语言切换在下次构建 Sections 时刷新），
    /// 未映射的项回退策略定义的 DisplayName
    /// </summary>
    public string DisplayName =>
        LocKeyByItemKey.TryGetValue(Definition.Key, out var locKey)
            ? LocalizationProvider.Get(locKey)
            : Definition.DisplayName;

    public string? Description => Definition.Description;
    public ConfigItemType Type => Definition.Type;
    public int? MinValue => Definition.MinValue;
    public int? MaxValue => Definition.MaxValue;
    public List<string>? Options => Definition.Options;

    /// <summary>
    /// 配置项的值
    /// </summary>
    [ObservableProperty]
    private object? _value;

    public ConfigItemWrapper(ConfigItemDefinition definition, Dictionary<string, object?> settings)
    {
        Definition = definition;
        _settings = settings;

        // 从设置中获取当前值
        if (settings.TryGetValue(definition.Key, out var value))
        {
            _value = value;
        }
        else
        {
            _value = definition.DefaultValue;
        }
    }

    partial void OnValueChanged(object? value)
    {
        _settings[Key] = value;
    }
}

/// <summary>
/// 配置分区包装类
/// </summary>
public class ConfigSectionWrapper
{
    public string DisplayName { get; }
    public List<ConfigItemWrapper> Items { get; }

    public ConfigSectionWrapper(ConfigSectionDefinition section, Dictionary<string, object?> settings)
    {
        DisplayName = section.DisplayName;
        Items = section.Items.Select(i => new ConfigItemWrapper(i, settings)).ToList();
    }
}