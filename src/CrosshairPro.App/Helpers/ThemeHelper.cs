using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CrosshairPro.App.Helpers;

/// <summary>
/// 主题资源访问助手 - 从 Application.Resources 获取主题颜色和样式
/// 提供类型安全的访问方式，避免代码中硬编码颜色值
/// </summary>
public static class ThemeHelper
{
    // ═══════════════════════════════════════════════════════════
    // COLORS
    // ═══════════════════════════════════════════════════════════

    /// <summary>获取颜色值</summary>
    public static Color GetColor(string key)
    {
        var color = (Color)System.Windows.Application.Current.FindResource(key);
        return color;
    }

    /// <summary>背景色</summary>
    public static Color BackgroundColor => GetColor("BackgroundColor");

    /// <summary>表面色</summary>
    public static Color SurfaceColor => GetColor("SurfaceColor");

    /// <summary>控件色</summary>
    public static Color ControlColor => GetColor("ControlColor");

    /// <summary>边框色</summary>
    public static Color BorderColor => GetColor("BorderColor");

    /// <summary>强调色</summary>
    public static Color AccentColor => GetColor("AccentColor");

    /// <summary>错误色</summary>
    public static Color ErrorColor => GetColor("ErrorColor");

    /// <summary>阴影色</summary>
    public static Color ShadowColor => GetColor("ShadowColor");

    /// <summary>网格线颜色</summary>
    public static Color GridLineColor => GetColor("GridLineColor");

    /// <summary>网格中心线颜色</summary>
    public static Color GridCenterLineColor => GetColor("GridCenterLineColor");

    // ═══════════════════════════════════════════════════════════
    // BRUSHES
    // ═══════════════════════════════════════════════════════════

    /// <summary>获取画刷</summary>
    public static SolidColorBrush GetBrush(string key)
    {
        var brush = (SolidColorBrush)System.Windows.Application.Current.FindResource(key);
        return brush;
    }

    /// <summary>背景画刷</summary>
    public static SolidColorBrush BackgroundBrush => GetBrush("BackgroundBrush");

    /// <summary>表面画刷</summary>
    public static SolidColorBrush SurfaceBrush => GetBrush("SurfaceBrush");

    /// <summary>控件画刷</summary>
    public static SolidColorBrush ControlBrush => GetBrush("ControlBrush");

    /// <summary>控件悬停画刷</summary>
    public static SolidColorBrush ControlHoverBrush => GetBrush("ControlHoverBrush");

    /// <summary>边框画刷</summary>
    public static SolidColorBrush BorderBrush => GetBrush("BorderBrush");

    /// <summary>文本主色画刷</summary>
    public static SolidColorBrush TextPrimaryBrush => GetBrush("TextPrimaryBrush");

    /// <summary>文本次要画刷</summary>
    public static SolidColorBrush TextSecondaryBrush => GetBrush("TextSecondaryBrush");

    /// <summary>强调画刷</summary>
    public static SolidColorBrush AccentBrush => GetBrush("AccentBrush");

    /// <summary>错误画刷</summary>
    public static SolidColorBrush ErrorBrush => GetBrush("ErrorBrush");

    /// <summary>网格线画刷</summary>
    public static SolidColorBrush GridLineBrush => GetBrush("GridLineBrush");

    /// <summary>网格中心线画刷</summary>
    public static SolidColorBrush GridCenterLineBrush => GetBrush("GridCenterLineBrush");

    // ═══════════════════════════════════════════════════════════
    // EFFECTS
    // ═══════════════════════════════════════════════════════════

    /// <summary>对话框阴影效果</summary>
    public static DropShadowEffect DialogShadowEffect
        => (DropShadowEffect)System.Windows.Application.Current.FindResource("DialogShadowEffect");

    /// <summary>Toast阴影效果</summary>
    public static DropShadowEffect ToastShadowEffect
        => (DropShadowEffect)System.Windows.Application.Current.FindResource("ToastShadowEffect");

    // ═══════════════════════════════════════════════════════════
    // TYPOGRAPHY
    // ═══════════════════════════════════════════════════════════

    /// <summary>主字体</summary>
    public static FontFamily FontFamilyPrimary
        => (FontFamily)System.Windows.Application.Current.FindResource("FontFamilyPrimary");

    /// <summary>等宽字体</summary>
    public static FontFamily FontFamilyMono
        => (FontFamily)System.Windows.Application.Current.FindResource("FontFamilyMono");

    /// <summary>标题字体大小</summary>
    public static double FontSizeHeading
        => (double)System.Windows.Application.Current.FindResource("FontSizeHeading");

    /// <summary>正文字体大小</summary>
    public static double FontSizeBody
        => (double)System.Windows.Application.Current.FindResource("FontSizeBody");

    /// <summary>说明字体大小</summary>
    public static double FontSizeCaption
        => (double)System.Windows.Application.Current.FindResource("FontSizeCaption");

    // ═══════════════════════════════════════════════════════════
    // RADII
    // ═══════════════════════════════════════════════════════════

    /// <summary>大圆角</summary>
    public static CornerRadius RadiusLg
        => (CornerRadius)System.Windows.Application.Current.FindResource("RadiusLg");

    /// <summary>中圆角</summary>
    public static CornerRadius RadiusMd
        => (CornerRadius)System.Windows.Application.Current.FindResource("RadiusMd");

    /// <summary>小圆角</summary>
    public static CornerRadius RadiusSm
        => (CornerRadius)System.Windows.Application.Current.FindResource("RadiusSm");
}