using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CrosshairPro.App.Localization;

/// <summary>
/// 本地化提供者（单例）
/// 管理当前语言文化，提供翻译查询，支持动态语言切换
/// </summary>
public sealed class LocalizationProvider : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationProvider> _instance = new(() => new LocalizationProvider());
    public static LocalizationProvider Instance => _instance.Value;

    private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

    /// <summary>
    /// 当前语言文化，设置后触发所有绑定更新
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (_currentCulture.Equals(value)) return;
            _currentCulture = value;
            CultureInfo.CurrentUICulture = value;
            CultureInfo.CurrentCulture = value;
            OnPropertyChanged();
            // 索引器变化 → 通知所有绑定更新
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// 通过 key 获取翻译文本
    /// 用法：LocalizationProvider.Instance["Key"]
    /// </summary>
    public string this[string key] =>
        Strings.ResourceManager.GetString(key, _currentCulture) ?? key;

    /// <summary>
    /// 获取翻译文本（静态便捷方法）
    /// </summary>
    public static string Get(string key) => Instance[key];

    /// <summary>
    /// 获取翻译文本并格式化（支持 {0}, {1} 等占位符）
    /// </summary>
    public static string GetFormatted(string key, params object[] args)
    {
        var format = Instance[key];
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }

    /// <summary>
    /// 支持的语言列表
    /// </summary>
    public static CultureInfo[] SupportedCultures { get; } =
    [
        new("zh-CN"),  // 简体中文（默认）
        new("en-US"),  // 英文
    ];

    /// <summary>
    /// 切换语言
    /// </summary>
    public void SetCulture(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        CurrentCulture = culture;
    }

    /// <summary>
    /// 初始化：根据保存的偏好设置初始语言，如果没有保存则默认使用中文
    /// </summary>
    public void Initialize(string? savedLanguage = null)
    {
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            try
            {
                _currentCulture = new CultureInfo(savedLanguage);
            }
            catch
            {
                _currentCulture = new CultureInfo("zh-CN"); // 默认中文
            }
        }
        else
        {
            // 默认使用中文（强制）
            _currentCulture = new CultureInfo("zh-CN");
        }

        CultureInfo.CurrentUICulture = _currentCulture;
        CultureInfo.CurrentCulture = _currentCulture;
    }

    // ── INotifyPropertyChanged ──

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
