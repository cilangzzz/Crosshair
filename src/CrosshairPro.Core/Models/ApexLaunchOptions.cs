using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// Apex Legends 启动选项模型
/// 每个参数都可以独立配置
/// </summary>
public partial class ApexLaunchOptions : ObservableObject
{
    // 性能参数
    [ObservableProperty]
    private bool _high = false;

    [ObservableProperty]
    private int _threads = 0;

    [ObservableProperty]
    private int _freq = 0;

    [ObservableProperty]
    private bool _novid = false;

    [ObservableProperty]
    private bool _nojoy = false;

    // 窗口参数
    [ObservableProperty]
    private bool _windowed = false;

    [ObservableProperty]
    private bool _noborder = false;

    [ObservableProperty]
    private bool _fullscreen = false;

    [ObservableProperty]
    private int _width = 1920;

    [ObservableProperty]
    private int _height = 1080;

    // 游戏控制参数
    [ObservableProperty]
    private int _fpsMax = 0;

    [ObservableProperty]
    private int _clShowfps = 0;

    [ObservableProperty]
    private bool _clShowpos = false;

    [ObservableProperty]
    private float _clFovScale = 1.27f;

    [ObservableProperty]
    private float _matLetterboxAspectMin = 0.0f;

    [ObservableProperty]
    private bool _mRawinput = false;

    // 网络参数
    [ObservableProperty]
    private float _clInterp = 0.015f;

    [ObservableProperty]
    private int _clInterpRatio = 1;

    [ObservableProperty]
    private int _clCmdrate = 60;

    [ObservableProperty]
    private int _clUpdaterate = 60;

    // 调试参数
    [ObservableProperty]
    private bool _dev = false;

    [ObservableProperty]
    private bool _console = false;

    /// <summary>生成启动选项字符串</summary>
    public string GenerateOptionsString()
    {
        var options = new List<string>();

        if (High) options.Add("-high");
        if (Threads > 0) options.Add($"-threads {Threads}");
        if (Freq > 0) options.Add($"-freq {Freq}");
        if (Novid) options.Add("-novid");
        if (Nojoy) options.Add("-nojoy");

        if (Windowed) options.Add("-windowed");
        if (Noborder) options.Add("-noborder");
        if (Fullscreen) options.Add("-fullscreen");
        if (Windowed || Noborder)
        {
            if (Width > 0) options.Add($"-w {Width}");
            if (Height > 0) options.Add($"-h {Height}");
        }

        options.Add($"+fps_max {FpsMax}");
        if (ClShowfps > 0) options.Add($"+cl_showfps {ClShowfps}");
        if (ClShowpos) options.Add("+cl_showpos 1");
        if (ClFovScale > 0) options.Add($"+cl_fovScale \"{ClFovScale:F2}\"");
        if (MatLetterboxAspectMin > 0) options.Add($"+mat_letterbox_aspect_min {MatLetterboxAspectMin:F1}");
        if (MRawinput) options.Add("+m_rawinput 1");

        if (ClInterp > 0) options.Add($"+cl_interp {ClInterp}");
        if (ClInterpRatio > 0) options.Add($"+cl_interp_ratio {ClInterpRatio}");
        if (ClCmdrate > 0) options.Add($"+cl_cmdrate {ClCmdrate}");
        if (ClUpdaterate > 0) options.Add($"+cl_updaterate {ClUpdaterate}");

        if (Dev) options.Add("-dev");
        if (Console) options.Add("-console");

        return string.Join(" ", options);
    }
}