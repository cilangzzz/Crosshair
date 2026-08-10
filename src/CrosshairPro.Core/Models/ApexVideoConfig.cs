using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// Apex Legends videoconfig.txt 配置模型
/// 对应 videoconfig.txt 的 KeyValues 格式
/// 文件位置: %USERPROFILE%\Saved Games\Respawn\Apex\local\videoconfig.txt
/// </summary>
public partial class ApexVideoConfig : ObservableObject
{
    // ═══════════════════════════════════════════════════════════
    // 显示设置 (Display Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>分辨率宽度 (640-3840)</summary>
    [ObservableProperty]
    private int _defaultRes = 1920;

    /// <summary>分辨率高度 (480-2160)</summary>
    [ObservableProperty]
    private int _defaultResHeight = 1080;

    /// <summary>上次显示宽度</summary>
    [ObservableProperty]
    private int _lastDisplayWidth = 1920;

    /// <summary>上次显示高度</summary>
    [ObservableProperty]
    private int _lastDisplayHeight = 1080;

    /// <summary>刷新率 Hz (60-360)</summary>
    [ObservableProperty]
    private int _refreshRate = 144;

    /// <summary>分辨率缩放百分比 (50-100)</summary>
    [ObservableProperty]
    private int _resolutionScale = 100;

    /// <summary>全屏模式 (0=窗口, 1=全屏)</summary>
    [ObservableProperty]
    private int _fullscreen = 1;

    /// <summary>无边框窗口 (0=有边框, 1=无边框)</summary>
    [ObservableProperty]
    private int _nowindowborder = 1;

    /// <summary>窗口模式 (0=全屏, 1=窗口, 2=无边框)</summary>
    [ObservableProperty]
    private int _windowMode = 0;

    /// <summary>伽马值 (0.5-2.5, 越高越亮)</summary>
    [ObservableProperty]
    private float _gamma = 1.0f;

    /// <summary>显示器伽马 (1.8-2.6)</summary>
    [ObservableProperty]
    private float _monitorGamma = 2.2f;

    // ═══════════════════════════════════════════════════════════
    // 分辨率选择辅助属性
    // ═══════════════════════════════════════════════════════════

    /// <summary>分辨率选项列表</summary>
    public static readonly (int Width, int Height)[] ResolutionOptions = new[]
    {
        (3840, 2160),
        (2560, 1600),
        (2560, 1440),
        (2560, 1080),
        (1920, 1200),
        (1920, 1080),
        (1680, 1050),
        (1600, 900),
        (1440, 900),
        (1280, 1024),
        (1280, 960),
        (1280, 800),
        (1280, 720)
    };

    /// <summary>分辨率索引 (对应 ResolutionOptions)</summary>
    [ObservableProperty]
    private int _resolutionIndex = 5; // 默认 1920x1080

    /// <summary>当分辨率索引改变时，更新 DefaultRes 和 DefaultResHeight</summary>
    partial void OnResolutionIndexChanged(int value)
    {
        if (value >= 0 && value < ResolutionOptions.Length)
        {
            var (w, h) = ResolutionOptions[value];
            DefaultRes = w;
            DefaultResHeight = h;
        }
    }

    /// <summary>根据 DefaultRes 和 DefaultResHeight 计算分辨率索引</summary>
    public void UpdateResolutionIndex()
    {
        for (int i = 0; i < ResolutionOptions.Length; i++)
        {
            if (ResolutionOptions[i].Width == DefaultRes && ResolutionOptions[i].Height == DefaultResHeight)
            {
                ResolutionIndex = i;
                return;
            }
        }
        // 未找到匹配项，默认为 1920x1080
        ResolutionIndex = 5;
    }

    // ═══════════════════════════════════════════════════════════
    // 画质设置 (Quality Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>纹理质量 (-1=最高, 0=高, 1=中, 2=低)</summary>
    [ObservableProperty]
    private int _matPicmip = 0;

    /// <summary>各向异性过滤 (0-16)</summary>
    [ObservableProperty]
    private int _matForceaniso = 1;

    /// <summary>Mipmap 线性过滤 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _matMipLinear = 1;

    /// <summary>流式内存 KB</summary>
    [ObservableProperty]
    private int _streamMemory = 128000;

    /// <summary>阴影总开关 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _shadowEnable = 1;

    /// <summary>最大动态阴影数 (0-8)</summary>
    [ObservableProperty]
    private int _shadowMaxdynamic = 4;

    /// <summary>阴影深度最小维度</summary>
    [ObservableProperty]
    private int _shadowDepthDimenMin = 0;

    /// <summary>阴影上采样因子</summary>
    [ObservableProperty]
    private int _shadowDepthUpresFactorMax = 0;

    /// <summary>级联阴影映射 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _csmEnabled = 1;

    /// <summary>CSM 覆盖范围 (0-1)</summary>
    [ObservableProperty]
    private int _csmCoverage = 1;

    /// <summary>CSM 级联分辨率 (256/512/1024)</summary>
    [ObservableProperty]
    private int _csmCascadeRes = 512;

    /// <summary>新阴影设置标记</summary>
    [ObservableProperty]
    private int _newShadowSettings = 1;

    /// <summary>粒子CPU等级 (0=低, 1=中, 2=高)</summary>
    [ObservableProperty]
    private int _particleCpuLevel = 1;

    /// <summary>粒子回退基数 (0-3)</summary>
    [ObservableProperty]
    private int _clParticleFallbackBase = 3;

    /// <summary>粒子回退乘数 (0-2)</summary>
    [ObservableProperty]
    private int _clParticleFallbackMultiplier = 2;

    /// <summary>碎片效果 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _clGibAllow = 1;

    /// <summary>布娃娃物理数量 (0-16)</summary>
    [ObservableProperty]
    private int _clRagdollMaxcount = 8;

    /// <summary>布娃娃自碰撞 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _clRagdollSelfCollision = 1;

    /// <summary>贴花数量 (0-256)</summary>
    [ObservableProperty]
    private int _rDecals = 256;

    /// <summary>模型贴花 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _rCreatemodeldecals = 1;

    /// <summary>LOD切换距离 (0.35-1.0)</summary>
    [ObservableProperty]
    private float _rLodSwitchScale = 0.6f;

    /// <summary>消失距离缩放 (0.5-2.0)</summary>
    [ObservableProperty]
    private float _fadeDistScale = 1.0f;

    /// <summary>地图细节等级 (0=低, 1=中, 2=高)</summary>
    [ObservableProperty]
    private int _mapDetailLevel = 1;

    /// <summary>体积光 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _volumetricLighting = 0;

    /// <summary>体积雾 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _volumetricFog = 0;

    /// <summary>SSAO质量 (0=关, 1=低, 2=高)</summary>
    [ObservableProperty]
    private int _ssaoQuality = 1;

    // ═══════════════════════════════════════════════════════════
    // 性能设置 (Performance Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>垂直同步 (0=关, 1=开, 2=三重缓冲)</summary>
    [ObservableProperty]
    private int _matVsyncMode = 0;

    /// <summary>后缓冲区数量 (1-2)</summary>
    [ObservableProperty]
    private int _matBackbufferCount = 1;

    /// <summary>抗锯齿模式 (0=关, 1=FXAA, 2=TXAA, 3=MSAA 2x, 4=MSAA 4x)</summary>
    [ObservableProperty]
    private int _matAntialiasMode = 0;

    /// <summary>动态分辨率缩放 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _dvsEnable = 0;

    /// <summary>GPU帧时间最小值 (微秒)</summary>
    [ObservableProperty]
    private int _dvsGpuframetimeMin = 15000;

    /// <summary>GPU帧时间最大值 (微秒)</summary>
    [ObservableProperty]
    private int _dvsGpuframetimeMax = 16500;

    /// <summary>动态流预算</summary>
    [ObservableProperty]
    private int _dynamicStreamingBudget = 0;

    // ═══════════════════════════════════════════════════════════
    // 高级画质选项 (Advanced Quality)
    // ═══════════════════════════════════════════════════════════

    /// <summary>高光强度 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFSpecularHighlight = 1.0f;

    /// <summary>动态贴花质量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFDynamicDecals = 1.0f;

    /// <summary>动态光照质量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFDynamicLights = 1.0f;

    /// <summary>阴影质量乘数 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFShadows = 1.0f;

    /// <summary>贴花质量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFDecals = 1.0f;

    /// <summary>SSAO强度 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFSSAO = 1.0f;

    /// <summary>阳光阴影过滤</summary>
    [ObservableProperty]
    private float _mFSunShadowFilter = 1.0f;

    /// <summary>阳光阴影分辨率</summary>
    [ObservableProperty]
    private float _mFSunShadowResolution = 1.0f;

    /// <summary>聚光阴影分辨率</summary>
    [ObservableProperty]
    private float _mFSpotShadowResolution = 1.0f;

    /// <summary>暗角效果 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _mFVignetteEnable = 0;

    /// <summary>景深质量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFDepthOfField = 1.0f;

    /// <summary>动态模糊强度 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _mFMotionBlur = 0.0f;

    // ═══════════════════════════════════════════════════════════
    // 系统字段 (System)
    // ═══════════════════════════════════════════════════════════

    /// <summary>配置版本号 (当前: 10)</summary>
    [ObservableProperty]
    private int _configVersion = 10;

    /// <summary>音量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _soundVolume = 1.0f;
}
