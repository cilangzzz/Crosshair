using CrosshairPro.Core.Models;

namespace CrosshairPro.Core.Interfaces;

/// <summary>
/// 热键管理器接口
/// </summary>
public interface IHotkeyManager : IDisposable
{
    /// <summary>
    /// 注册热键
    /// </summary>
    bool RegisterHotkey(HotkeyBinding binding);

    /// <summary>
    /// 注销热键
    /// </summary>
    bool UnregisterHotkey(string bindingId);

    /// <summary>
    /// 注销所有热键
    /// </summary>
    void UnregisterAll();

    /// <summary>
    /// 热键触发事件
    /// </summary>
    event EventHandler<HotkeyTriggeredEventArgs>? HotkeyTriggered;
}

/// <summary>
/// 热键触发事件参数
/// </summary>
public class HotkeyTriggeredEventArgs : EventArgs
{
    public HotkeyBinding Binding { get; }

    public HotkeyTriggeredEventArgs(HotkeyBinding binding)
    {
        Binding = binding;
    }
}