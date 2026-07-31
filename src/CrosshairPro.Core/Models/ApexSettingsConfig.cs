using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosshairPro.Core.Models;

/// <summary>
/// Apex Legends settings.cfg 配置模型
/// 对应 settings.cfg 的 Valve CFG 格式
/// 文件位置: %USERPROFILE%\Saved Games\Respawn\Apex\local\settings.cfg
/// </summary>
public partial class ApexSettingsConfig : ObservableObject
{
    // ═══════════════════════════════════════════════════════════
    // 鼠标设置 (Mouse Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>鼠标灵敏度 (0.1-10.0)</summary>
    [ObservableProperty]
    private float _mouseSensitivity = 1.0f;

    /// <summary>鼠标加速 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _mAcceleration = 0;

    /// <summary>鼠标限制在窗口 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _mClampToWindow = 0;

    // ═══════════════════════════════════════════════════════════
    // 瞄准镜灵敏度 (Scope Sensitivity)
    // ═══════════════════════════════════════════════════════════

    /// <summary>使用独立瞄准镜灵敏度 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _mouseUsePerScopeSensitivityScalars = 0;

    /// <summary>1倍镜灵敏度 (红点/全息)</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar0 = 1.0f;

    /// <summary>2倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar1 = 1.0f;

    /// <summary>3倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar2 = 1.0f;

    /// <summary>4倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar3 = 1.0f;

    /// <summary>6倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar4 = 1.0f;

    /// <summary>8倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar5 = 1.0f;

    /// <summary>10倍镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar6 = 1.0f;

    /// <summary>变焦镜灵敏度</summary>
    [ObservableProperty]
    private float _mouseZoomedSensitivityScalar7 = 1.0f;

    // ═══════════════════════════════════════════════════════════
    // 帧率设置 (FPS Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>最大帧率 (0=无限制)</summary>
    [ObservableProperty]
    private int _fpsMax = 0;

    /// <summary>菜单帧率限制</summary>
    [ObservableProperty]
    private int _fpsMaxMenu = 60;

    /// <summary>显示FPS (0=关, 1=简单, 2=详细)</summary>
    [ObservableProperty]
    private int _clShowfps = 0;

    /// <summary>显示位置信息 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _clShowpos = 0;

    // ═══════════════════════════════════════════════════════════
    // 视野设置 (FOV Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>FOV缩放 (1.0-1.35, 约90-120度)</summary>
    [ObservableProperty]
    private float _clFovScale = 1.27f;

    /// <summary>垂直视角缩放</summary>
    [ObservableProperty]
    private float _clViewpitchscale = 0.95f;

    // ═══════════════════════════════════════════════════════════
    // 网络设置 (Network Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>网络插值</summary>
    [ObservableProperty]
    private float _clInterp = 0.015f;

    /// <summary>插值比率</summary>
    [ObservableProperty]
    private int _clInterpRatio = 1;

    /// <summary>命令速率</summary>
    [ObservableProperty]
    private int _clCmdrate = 60;

    /// <summary>更新速率</summary>
    [ObservableProperty]
    private int _clUpdaterate = 60;

    // ═══════════════════════════════════════════════════════════
    // 音频设置 (Audio Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>扬声器数量 (2/6/8)</summary>
    [ObservableProperty]
    private int _soundNumSpeakers = 2;

    /// <summary>语音音量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _soundVolumeVoice = 1.0f;

    /// <summary>音频通道</summary>
    [ObservableProperty]
    private int _milesChannels = 0;

    /// <summary>输出设备</summary>
    [ObservableProperty]
    private string _milesOutputDevice = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // 语音设置 (Voice Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>语音聊天模式</summary>
    [ObservableProperty]
    private int _voiceChatMode = 0;

    /// <summary>强制麦克风录音 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _voiceForcemicrecord = 1;

    /// <summary>输入设备</summary>
    [ObservableProperty]
    private string _voiceInputDevice = string.Empty;

    /// <summary>麦克风增益</summary>
    [ObservableProperty]
    private int _voiceMixerBoost = 0;

    /// <summary>麦克风静音 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _voiceMixerMute = 0;

    /// <summary>麦克风音量 (0.0-1.0)</summary>
    [ObservableProperty]
    private float _voiceMixerVolume = 1.0f;

    /// <summary>语音调制启用 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _voiceModenable = 1;

    /// <summary>语音缩放</summary>
    [ObservableProperty]
    private float _voiceScale = 1.0f;

    /// <summary>VOX 模式 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _voiceVox = 1;

    // ═══════════════════════════════════════════════════════════
    // 图形设置 (Graphics Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>AMD 低延迟模式 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _gfxAmdUseLowLatency = 1;

    /// <summary>NVIDIA 低延迟模式 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _gfxNvnUseLowLatency = 0;

    /// <summary>NVIDIA 低延迟增强 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _gfxNvnUseLowLatencyBoost = 0;

    /// <summary>Razer Chroma 灯光 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _chromaEnable = 0;

    // ═══════════════════════════════════════════════════════════
    // 其他设置 (Other Settings)
    // ═══════════════════════════════════════════════════════════

    /// <summary>玩家名称</summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>UI 布局模式</summary>
    [ObservableProperty]
    private int _uiLayoutMode = 0;

    /// <summary>语音启用 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _svVoiceenable = 1;

    /// <summary>观战加速</summary>
    [ObservableProperty]
    private float _svSpecaccelerate = 1000.0f;

    /// <summary>观战穿墙 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _svSpecnoclip = 1;

    /// <summary>观战速度</summary>
    [ObservableProperty]
    private float _svSpecspeed = 5.0f;

    /// <summary>闭 captions 持续时间</summary>
    [ObservableProperty]
    private float _ccLingerTime = 1.0f;

    /// <summary>预显示时间</summary>
    [ObservableProperty]
    private float _ccPredisplayTime = 0.25f;

    /// <summary>最大破碎碎片数</summary>
    [ObservableProperty]
    private int _funcBreakMaxPieces = 15;

    /// <summary>自动回正 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _lookspring = 0;

    /// <summary>平视移动 (0=关, 1=开)</summary>
    [ObservableProperty]
    private int _lookstrafe = 0;

    /// <summary>HDR 截图目录</summary>
    [ObservableProperty]
    private string _hdrScreenshotDirectory = string.Empty;
}