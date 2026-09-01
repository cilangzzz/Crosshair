using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CrosshairPro.App.Localization;

/// <summary>
/// 本地化提供者（单例）
/// 管理当前语言文化，提供翻译查询，支持动态语言切换
/// 通过索引器暴露翻译，配合 Binding "{Key}]" 路径实现 XAML 动态更新
/// </summary>
public sealed class LocalizationProvider : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationProvider> _instance = new(() => new LocalizationProvider());
    public static LocalizationProvider Instance => _instance.Value;

    private CultureInfo _currentCulture = new("zh-CN");

    /// <summary>
    /// 当前语言文化
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
            // 关键：通知索引器变化，触发所有 "[Key]" 路径的 Binding 更新
            OnPropertyChanged("Item[]");
        }
    }

    /// <summary>
    /// 通过 key 获取翻译文本（XAML 索引器绑定使用）
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
        new("zh-CN"),
        new("en-US"),
    ];

    /// <summary>
    /// 切换语言（会触发所有 XAML 绑定更新）
    /// </summary>
    public void SetCulture(string cultureName)
    {
        try
        {
            var culture = new CultureInfo(cultureName);
            CurrentCulture = culture;
        }
        catch
        {
            // 无效 culture 名时，忽略
        }
    }

    /// <summary>
    /// 初始化：默认中文，可选指定已保存的语言
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
                _currentCulture = new CultureInfo("zh-CN");
            }
        }
        else
        {
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