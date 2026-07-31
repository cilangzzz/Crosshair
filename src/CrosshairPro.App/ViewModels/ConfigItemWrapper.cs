using CommunityToolkit.Mvvm.ComponentModel;
using CrosshairPro.Core.Models;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 配置项包装类，用于 UI 绑定
/// </summary>
public partial class ConfigItemWrapper : ObservableObject
{
    private readonly Dictionary<string, object?> _settings;

    public ConfigItemDefinition Definition { get; }

    public string Key => Definition.Key;
    public string DisplayName => Definition.DisplayName;
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