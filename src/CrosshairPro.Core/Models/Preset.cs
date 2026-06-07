using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 预设配置
/// </summary>
public partial class Preset : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "新预设";

    [ObservableProperty]
    private CrosshairConfig _config = new();

    [ObservableProperty]
    private string? _gameAssociation;

    [ObservableProperty]
    private string? _hotkeyBinding;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.UtcNow;

    [ObservableProperty]
    private bool _isDefault;

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public Preset Clone()
    {
        return new Preset
        {
            Id = Guid.NewGuid().ToString(),
            Name = Name + " (副本)",
            Config = Config.Clone(),
            GameAssociation = GameAssociation,
            HotkeyBinding = HotkeyBinding,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDefault = false
        };
    }
}
