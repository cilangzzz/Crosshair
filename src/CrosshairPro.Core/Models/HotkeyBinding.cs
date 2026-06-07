using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// 热键绑定
/// </summary>
public partial class HotkeyBinding : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _combo = string.Empty;

    [ObservableProperty]
    private string _defaultCombo = string.Empty;

    [ObservableProperty]
    private bool _enabled = true;

    /// <summary>
    /// 热键动作类型
    /// </summary>
    public HotkeyAction Action { get; set; }

    /// <summary>
    /// 关联的预设ID（用于切换预设）
    /// </summary>
    public string? PresetId { get; set; }
}

/// <summary>
/// 热键动作类型
/// </summary>
public enum HotkeyAction
{
    /// <summary>
    /// 显示/隐藏准心
    /// </summary>
    ToggleCrosshair = 0,

    /// <summary>
    /// 切换预设
    /// </summary>
    SwitchPreset = 1,

    /// <summary>
    /// 重置位置
    /// </summary>
    ResetPosition = 2,

    /// <summary>
    /// 锁定/解锁位置
    /// </summary>
    LockPosition = 3,

    /// <summary>
    /// 增大大小
    /// </summary>
    IncreaseSize = 4,

    /// <summary>
    /// 减小大小
    /// </summary>
    DecreaseSize = 5
}

/// <summary>
/// 按键组合
/// </summary>
public struct KeyCombo
{
    public bool Ctrl;
    public bool Shift;
    public bool Alt;
    public bool Win;
    public string Key;

    /// <summary>
    /// 获取修饰键标志
    /// </summary>
    public int Modifiers =>
        (Ctrl ? 0x0002 : 0) |
        (Shift ? 0x0004 : 0) |
        (Alt ? 0x0001 : 0) |
        (Win ? 0x0008 : 0);

    /// <summary>
    /// 解析热键字符串
    /// </summary>
    public static KeyCombo Parse(string combo)
    {
        if (string.IsNullOrEmpty(combo))
            return new KeyCombo();

        var parts = combo.ToLowerInvariant().Split('+', StringSplitOptions.RemoveEmptyEntries);
        var result = new KeyCombo();

        foreach (var part in parts)
        {
            switch (part)
            {
                case "ctrl":
                    result.Ctrl = true;
                    break;
                case "shift":
                    result.Shift = true;
                    break;
                case "alt":
                    result.Alt = true;
                    break;
                case "win":
                    result.Win = true;
                    break;
                default:
                    result.Key = part;
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();
        if (Ctrl) parts.Add("Ctrl");
        if (Shift) parts.Add("Shift");
        if (Alt) parts.Add("Alt");
        if (Win) parts.Add("Win");
        if (!string.IsNullOrEmpty(Key)) parts.Add(Key.ToUpperInvariant());
        return string.Join("+", parts);
    }
}