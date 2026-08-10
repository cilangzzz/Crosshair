using System.Globalization;
using System.Reflection;
using System.Resources;

namespace CrosshairPro.App.Localization;

/// <summary>
/// 本地化资源访问器
/// 从嵌入的 .resx 资源文件中读取翻译文本
/// </summary>
public static class Strings
{
    private static ResourceManager? _resourceManager;

    /// <summary>
    /// 资源管理器实例
    /// </summary>
    public static ResourceManager ResourceManager =>
        _resourceManager ??= new ResourceManager(
            "CrosshairPro.App.Localization.Strings",
            Assembly.GetExecutingAssembly());

    /// <summary>
    /// 根据当前语言获取翻译文本
    /// </summary>
    public static string Get(string key)
    {
        return ResourceManager.GetString(key, LocalizationProvider.Instance.CurrentCulture) ?? key;
    }

    /// <summary>
    /// 根据指定语言获取翻译文本
    /// </summary>
    public static string Get(string key, CultureInfo culture)
    {
        return ResourceManager.GetString(key, culture) ?? key;
    }

    /// <summary>
    /// 获取格式化后的翻译文本
    /// </summary>
    public static string GetFormatted(string key, params object[] args)
    {
        var format = Get(key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }
}
