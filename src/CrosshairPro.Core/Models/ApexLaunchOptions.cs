using System.Globalization;
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

    // 窗口参数（数值 0 = 未设置，不输出对应令牌；默认值保持"无"，避免解析回填后把字符串中没有的 -w/-h 注入回去）
    [ObservableProperty]
    private bool _windowed = false;

    [ObservableProperty]
    private bool _noborder = false;

    [ObservableProperty]
    private bool _fullscreen = false;

    [ObservableProperty]
    private int _width = 0;

    [ObservableProperty]
    private int _height = 0;

    // 游戏控制参数（数值 0 = 未设置，不输出对应令牌）
    [ObservableProperty]
    private int _fpsMax = 0;

    [ObservableProperty]
    private int _clShowfps = 0;

    [ObservableProperty]
    private bool _clShowpos = false;

    [ObservableProperty]
    private float _clFovScale = 0f;

    [ObservableProperty]
    private float _matLetterboxAspectMin = 0.0f;

    [ObservableProperty]
    private bool _mRawinput = false;

    // 网络参数（数值 0 = 未设置，不输出对应令牌）
    [ObservableProperty]
    private float _clInterp = 0f;

    [ObservableProperty]
    private int _clInterpRatio = 0;

    [ObservableProperty]
    private int _clCmdrate = 0;

    [ObservableProperty]
    private int _clUpdaterate = 0;

    // 调试参数
    [ObservableProperty]
    private bool _dev = false;

    [ObservableProperty]
    private bool _console = false;

    /// <summary>生成启动选项字符串。数值参数仅在大于 0 时输出（0 = 未设置），布尔参数为 true 时输出</summary>
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
        // -w/-h 独立于窗口模式输出（值 > 0 即输出），单独设置也生效
        if (Width > 0) options.Add($"-w {Width}");
        if (Height > 0) options.Add($"-h {Height}");

        // 游戏控制参数（游戏参数组）：显式配置时输出；cl_fovScale 超出 1.35 需配合信箱比例。
        // float 一律 InvariantCulture 格式化（小数逗号区域下 CurrentCulture 会产出 "1,27"，游戏无法识别，见 pitfalls §5）
        if (FpsMax > 0) options.Add($"+fps_max {FpsMax}");
        if (ClShowfps > 0) options.Add($"+cl_showfps {ClShowfps}");
        if (ClShowpos) options.Add("+cl_showpos 1");
        if (ClFovScale > 0) options.Add($"+cl_fovScale \"{ClFovScale.ToString("F2", CultureInfo.InvariantCulture)}\"");
        if (MatLetterboxAspectMin > 0) options.Add($"+mat_letterbox_aspect_min {MatLetterboxAspectMin.ToString("F1", CultureInfo.InvariantCulture)}");
        if (MRawinput) options.Add("+m_rawinput 1");

        if (ClInterp > 0) options.Add($"+cl_interp {ClInterp.ToString(CultureInfo.InvariantCulture)}");
        if (ClInterpRatio > 0) options.Add($"+cl_interp_ratio {ClInterpRatio}");
        if (ClCmdrate > 0) options.Add($"+cl_cmdrate {ClCmdrate}");
        if (ClUpdaterate > 0) options.Add($"+cl_updaterate {ClUpdaterate}");

        if (Dev) options.Add("-dev");
        if (Console) options.Add("-console");

        return string.Join(" ", options);
    }

    /// <summary>
    /// 从启动项字符串反向解析填充选项（双向联动：进入自定义模式时把已有字符串还原为结构化编辑器）。
    /// 先将全部字段重置为默认值再解析，保证解析后选项与字符串内容严格一致；
    /// 仅识别本模型支持的令牌（带引号的值剥除引号，如 +cl_fovScale "2"），未知令牌忽略；
    /// 注意此后经 <see cref="GenerateOptionsString"/> 重新生成时未知令牌会丢失。
    /// </summary>
    public void LoadFromString(string? options)
    {
        ResetToDefaults();

        if (string.IsNullOrWhiteSpace(options))
        {
            return;
        }

        var tokens = Tokenize(options);
        for (var i = 0; i < tokens.Count; i++)
        {
            // 令牌大小写不敏感（游戏侧 cvar/命令不区分大小写）；重新生成时输出规范大小写
            switch (tokens[i].ToLowerInvariant())
            {
                case "-high": High = true; break;
                case "-novid": Novid = true; break;
                case "-nojoy": Nojoy = true; break;
                case "-windowed": Windowed = true; break;
                case "-noborder": Noborder = true; break;
                case "-fullscreen": Fullscreen = true; break;
                case "-dev": Dev = true; break;
                case "-console": Console = true; break;
                case "-threads":
                    if (NextInt(tokens, i, out var threads)) { Threads = threads; i++; }
                    break;
                case "-freq":
                case "-refresh":
                    if (NextInt(tokens, i, out var freq)) { Freq = freq; i++; }
                    break;
                case "-w":
                    if (NextInt(tokens, i, out var w)) { Width = w; i++; }
                    break;
                case "-h":
                    if (NextInt(tokens, i, out var h)) { Height = h; i++; }
                    break;
                case "+fps_max":
                    if (NextInt(tokens, i, out var fpsMax)) { FpsMax = fpsMax; i++; }
                    break;
                case "+cl_showfps":
                    if (NextInt(tokens, i, out var showfps)) { ClShowfps = showfps; i++; }
                    break;
                case "+cl_showpos":
                    if (NextInt(tokens, i, out var showpos)) { ClShowpos = showpos != 0; i++; }
                    break;
                case "+m_rawinput":
                    if (NextInt(tokens, i, out var rawinput)) { MRawinput = rawinput != 0; i++; }
                    break;
                case "+cl_fovscale":
                    if (NextFloat(tokens, i, out var fov)) { ClFovScale = fov; i++; }
                    break;
                case "+mat_letterbox_aspect_min":
                    if (NextFloat(tokens, i, out var letterbox)) { MatLetterboxAspectMin = letterbox; i++; }
                    break;
                case "+cl_interp":
                    if (NextFloat(tokens, i, out var interp)) { ClInterp = interp; i++; }
                    break;
                case "+cl_interp_ratio":
                    if (NextInt(tokens, i, out var interpRatio)) { ClInterpRatio = interpRatio; i++; }
                    break;
                case "+cl_cmdrate":
                    if (NextInt(tokens, i, out var cmdrate)) { ClCmdrate = cmdrate; i++; }
                    break;
                case "+cl_updaterate":
                    if (NextInt(tokens, i, out var updaterate)) { ClUpdaterate = updaterate; i++; }
                    break;
            }
        }
    }

    private void ResetToDefaults()
    {
        High = false;
        Threads = 0;
        Freq = 0;
        Novid = false;
        Nojoy = false;
        Windowed = false;
        Noborder = false;
        Fullscreen = false;
        Width = 0;
        Height = 0;
        FpsMax = 0;
        ClShowfps = 0;
        ClShowpos = false;
        ClFovScale = 0f;
        MatLetterboxAspectMin = 0.0f;
        MRawinput = false;
        ClInterp = 0f;
        ClInterpRatio = 0;
        ClCmdrate = 0;
        ClUpdaterate = 0;
        Dev = false;
        Console = false;
    }

    private static bool NextInt(List<string> tokens, int index, out int value)
    {
        value = 0;
        return index + 1 < tokens.Count
            && int.TryParse(tokens[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool NextFloat(List<string> tokens, int index, out float value)
    {
        value = 0;
        return index + 1 < tokens.Count
            && float.TryParse(tokens[index + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>按空白拆分令牌；引号内的空白不拆分、引号本身剥除（如 +cl_fovScale "2" → 值 2）</summary>
    private static List<string> Tokenize(string options)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        foreach (var c in options)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }
        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }
        return tokens;
    }
}