using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;
using CrosshairPro.Infrastructure.Win32;
using System.Runtime.InteropServices;

namespace CrosshairPro.Infrastructure.Hotkey;

/// <summary>
/// Windows热键提供者
/// </summary>
public sealed class WinHotkeyProvider : IDisposable
{
    private readonly Dictionary<int, KeyCombo> _registeredKeys = new();
    private readonly Dictionary<int, string> _idToBindingMap = new();
    private int _nextId = 1;
    private IntPtr _hwnd;
    private bool _disposed;
    private WndProcDelegate? _wndProcDelegate;

    public event EventHandler<int>? HotkeyPressed;

    // 委托类型定义
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public WinHotkeyProvider()
    {
        _hwnd = CreateMessageWindow();
    }

    /// <summary>
    /// 注册热键
    /// </summary>
    public bool Register(int id, KeyCombo combo)
    {
        var virtualKey = Win32Methods.GetVirtualKeyCode(combo.Key);
        if (virtualKey == 0)
            return false;

        var modifiers = combo.Modifiers;

        if (!Win32Methods.RegisterHotKey(_hwnd, id, modifiers, virtualKey))
        {
            return false;
        }

        _registeredKeys[id] = combo;
        return true;
    }

    /// <summary>
    /// 注销热键
    /// </summary>
    public bool Unregister(int id)
    {
        if (!_registeredKeys.ContainsKey(id))
            return false;

        if (!Win32Methods.UnregisterHotKey(_hwnd, id))
            return false;

        _registeredKeys.Remove(id);
        _idToBindingMap.Remove(id);
        return true;
    }

    /// <summary>
    /// 注销所有热键
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var id in _registeredKeys.Keys.ToList())
        {
            Win32Methods.UnregisterHotKey(_hwnd, id);
        }
        _registeredKeys.Clear();
        _idToBindingMap.Clear();
    }

    /// <summary>
    /// 获取下一个热键ID
    /// </summary>
    public int GetNextId()
    {
        return _nextId++;
    }

    /// <summary>
    /// 创建消息窗口用于接收热键消息
    /// </summary>
    private IntPtr CreateMessageWindow()
    {
        // 保持委托引用防止被GC
        _wndProcDelegate = WndProc;

        var wndClass = new Win32Methods.WNDCLASS
        {
            style = 0,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = IntPtr.Zero,
            lpszClassName = "CrosshairProHotkeyWindow"
        };

        Win32Methods.RegisterClass(ref wndClass);

        return Win32Methods.CreateWindowEx(
            0,
            wndClass.lpszClassName,
            "",
            0,
            0, 0, 0, 0,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);
    }

    /// <summary>
    /// 窗口过程（处理热键消息）
    /// </summary>
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == Win32Constants.WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            HotkeyPressed?.Invoke(this, id);
            return IntPtr.Zero;
        }

        return Win32Methods.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        UnregisterAll();

        if (_hwnd != IntPtr.Zero)
        {
            Win32Methods.DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        _disposed = true;
    }
}

/// <summary>
/// 热键管理器实现
/// </summary>
public sealed class HotkeyManager : IHotkeyManager, IDisposable
{
    private readonly WinHotkeyProvider _provider;
    private readonly Dictionary<string, HotkeyBinding> _bindings = new();
    private readonly Dictionary<int, string> _hotkeyIdToBindingId = new();
    private bool _disposed;

    public event EventHandler<HotkeyTriggeredEventArgs>? HotkeyTriggered;

    public HotkeyManager()
    {
        _provider = new WinHotkeyProvider();
        _provider.HotkeyPressed += OnHotkeyPressed;
    }

    /// <summary>
    /// 注册热键
    /// </summary>
    public bool RegisterHotkey(HotkeyBinding binding)
    {
        if (binding == null || string.IsNullOrEmpty(binding.Id) || string.IsNullOrEmpty(binding.Combo))
            return false;

        // 解析组合键
        var combo = KeyCombo.Parse(binding.Combo);

        // 获取新ID
        var hotkeyId = _provider.GetNextId();

        // 注册到系统
        if (!_provider.Register(hotkeyId, combo))
            return false;

        // 保存绑定
        _bindings[binding.Id] = binding;
        _hotkeyIdToBindingId[hotkeyId] = binding.Id;

        return true;
    }

    /// <summary>
    /// 注销热键
    /// </summary>
    public bool UnregisterHotkey(string bindingId)
    {
        if (!_bindings.TryGetValue(bindingId, out var binding))
            return false;

        var entry = _hotkeyIdToBindingId.FirstOrDefault(x => x.Value == bindingId);
        if (entry.Key == 0)
            return false;

        if (!_provider.Unregister(entry.Key))
            return false;

        _hotkeyIdToBindingId.Remove(entry.Key);
        _bindings.Remove(bindingId);

        return true;
    }

    /// <summary>
    /// 注销所有热键
    /// </summary>
    public void UnregisterAll()
    {
        _provider.UnregisterAll();
        _bindings.Clear();
        _hotkeyIdToBindingId.Clear();
    }

    /// <summary>
    /// 热键按下事件处理
    /// </summary>
    private void OnHotkeyPressed(object? sender, int hotkeyId)
    {
        if (!_hotkeyIdToBindingId.TryGetValue(hotkeyId, out var bindingId))
            return;

        if (!_bindings.TryGetValue(bindingId, out var binding))
            return;

        HotkeyTriggered?.Invoke(this, new HotkeyTriggeredEventArgs(binding));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _provider.Dispose();
        _bindings.Clear();
        _hotkeyIdToBindingId.Clear();
        _disposed = true;
    }
}