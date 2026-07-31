using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Services;

/// <summary>
/// Apex Legends 配置服务实现
/// 管理 videoconfig.txt 和 settings.cfg 配置文件
/// </summary>
public class ApexConfigService : IApexConfigService
{
    private readonly string _apexConfigDir;
    private readonly string _backupDir;
    private readonly string _appConfigDir;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    // 配置文件名称
    private const string VideoConfigFile = "videoconfig.txt";
    private const string SettingsConfigFile = "settings.cfg";
    private const string LaunchOptionsFile = "launch_options.txt";

    public ApexConfigService()
    {
        // Apex Legends 配置目录
        _apexConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Saved Games",
            "Respawn",
            "Apex",
            "local");

        // 备份目录
        _backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrosshairPro",
            "apex-backups");

        // 应用配置目录（存储启动选项等）
        _appConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrosshairPro");

        // 确保目录存在
        Directory.CreateDirectory(_backupDir);
        Directory.CreateDirectory(_appConfigDir);
    }

    // ═══════════════════════════════════════════════════════════
    // 配置文件加载和保存
    // ═══════════════════════════════════════════════════════════

    public async Task<ApexVideoConfig> LoadVideoConfigAsync()
    {
        var filePath = GetVideoConfigPath();
        if (filePath == null || !File.Exists(filePath))
        {
            return GetDefaultPreset();
        }

        await _fileLock.WaitAsync();
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            return ParseVideoConfig(content);
        }
        catch
        {
            return GetDefaultPreset();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<ApexSettingsConfig> LoadSettingsConfigAsync()
    {
        var filePath = GetSettingsConfigPath();
        if (filePath == null || !File.Exists(filePath))
        {
            return new ApexSettingsConfig();
        }

        await _fileLock.WaitAsync();
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            return ParseSettingsConfig(content);
        }
        catch
        {
            return new ApexSettingsConfig();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveVideoConfigAsync(ApexVideoConfig config)
    {
        var filePath = GetVideoConfigPath();
        if (filePath == null)
        {
            throw new InvalidOperationException("无法找到 Apex Legends 配置文件目录");
        }

        var content = SerializeVideoConfig(config);

        await _fileLock.WaitAsync();
        try
        {
            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, content);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveSettingsConfigAsync(ApexSettingsConfig config)
    {
        var filePath = GetSettingsConfigPath();
        if (filePath == null)
        {
            throw new InvalidOperationException("无法找到 Apex Legends 配置文件目录");
        }

        var content = SerializeSettingsConfig(config);

        await _fileLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, content);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 配置文件替换和备份
    // ═══════════════════════════════════════════════════════════

    public async Task<bool> ReplaceVideoConfigAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            // 验证文件格式
            var content = await File.ReadAllTextAsync(filePath);
            if (!ValidateVideoConfigFile(content))
                return false;

            // 备份当前配置
            await BackupVideoConfigAsync();

            // 复制新配置
            var targetPath = GetVideoConfigPath();
            if (targetPath == null)
                return false;

            await _fileLock.WaitAsync();
            try
            {
                File.Copy(filePath, targetPath, overwrite: true);
                return true;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReplaceSettingsConfigAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            if (!ValidateSettingsConfigFile(content))
                return false;

            await BackupSettingsConfigAsync();

            var targetPath = GetSettingsConfigPath();
            if (targetPath == null)
                return false;

            await _fileLock.WaitAsync();
            try
            {
                File.Copy(filePath, targetPath, overwrite: true);
                return true;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> BackupVideoConfigAsync()
    {
        var sourcePath = GetVideoConfigPath();
        if (sourcePath == null || !File.Exists(sourcePath))
        {
            throw new FileNotFoundException("videoconfig.txt 文件不存在");
        }

        var backupFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_videoconfig.txt";
        var backupPath = Path.Combine(_backupDir, "videoconfig", backupFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

        await _fileLock.WaitAsync();
        try
        {
            File.Copy(sourcePath, backupPath, overwrite: true);
            return backupPath;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<string> BackupSettingsConfigAsync()
    {
        var sourcePath = GetSettingsConfigPath();
        if (sourcePath == null || !File.Exists(sourcePath))
        {
            throw new FileNotFoundException("settings.cfg 文件不存在");
        }

        var backupFileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_settings.cfg";
        var backupPath = Path.Combine(_backupDir, "settings", backupFileName);

        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

        await _fileLock.WaitAsync();
        try
        {
            File.Copy(sourcePath, backupPath, overwrite: true);
            return backupPath;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // 导出功能
    // ═══════════════════════════════════════════════════════════

    public async Task ExportVideoConfigAsync(string filePath)
    {
        var config = await LoadVideoConfigAsync();
        var content = SerializeVideoConfig(config);
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task ExportSettingsConfigAsync(string filePath)
    {
        var config = await LoadSettingsConfigAsync();
        var content = SerializeSettingsConfig(config);
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task ExportLaunchOptionsAsync(string filePath, string options)
    {
        await File.WriteAllTextAsync(filePath, options);
    }

    // ═══════════════════════════════════════════════════════════
    // 启动选项管理
    // ═══════════════════════════════════════════════════════════

    public string GenerateLaunchOptions()
    {
        // 默认启动选项模板
        return "-dev +fps_max 0 +cl_showpos 1 +mat_letterbox_aspect_min 1.0 +cl_fovScale \"2\" -novid";
    }

    public async Task<string> LoadLaunchOptionsAsync()
    {
        var filePath = Path.Combine(_appConfigDir, LaunchOptionsFile);
        if (!File.Exists(filePath))
        {
            return GenerateLaunchOptions();
        }

        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch
        {
            return GenerateLaunchOptions();
        }
    }

    public async Task SaveLaunchOptionsAsync(string options)
    {
        var filePath = Path.Combine(_appConfigDir, LaunchOptionsFile);
        await File.WriteAllTextAsync(filePath, options);
    }

    // ═══════════════════════════════════════════════════════════
    // 配置文件路径
    // ═══════════════════════════════════════════════════════════

    public string? GetVideoConfigPath()
    {
        var path = Path.Combine(_apexConfigDir, VideoConfigFile);
        return Directory.Exists(_apexConfigDir) ? path : null;
    }

    public string? GetSettingsConfigPath()
    {
        var path = Path.Combine(_apexConfigDir, SettingsConfigFile);
        return Directory.Exists(_apexConfigDir) ? path : null;
    }

    public bool IsApexInstalled()
    {
        return Directory.Exists(_apexConfigDir);
    }

    // ═══════════════════════════════════════════════════════════
    // 预设配置
    // ═══════════════════════════════════════════════════════════

    public ApexVideoConfig GetCompetitivePreset()
    {
        return new ApexVideoConfig
        {
            // 显示设置
            DefaultRes = 1920,
            DefaultResHeight = 1080,
            RefreshRate = 144,
            Fullscreen = 1,
            WindowMode = 0,
            Gamma = 1.0f,

            // 画质设置 - 竞技优化
            MatPicmip = 0,
            MatForceaniso = 1,
            ShadowEnable = 0,
            CsmEnabled = 0,
            ParticleCpuLevel = 0,
            SsaoQuality = 0,
            VolumetricLighting = 0,
            VolumetricFog = 0,

            // 性能设置
            MatVsyncMode = 0,
            MatAntialiasMode = 0,
            DvsEnable = 0,

            // 物理
            ClGibAllow = 0,
            ClRagdollMaxcount = 0,
            ClRagdollSelfCollision = 0
        };
    }

    public ApexVideoConfig GetHighQualityPreset()
    {
        return new ApexVideoConfig
        {
            // 显示设置
            DefaultRes = 1920,
            DefaultResHeight = 1080,
            RefreshRate = 144,
            Fullscreen = 1,
            WindowMode = 0,
            Gamma = 1.0f,

            // 画质设置 - 高画质
            MatPicmip = -1,
            MatForceaniso = 16,
            MatMipLinear = 1,
            ShadowEnable = 1,
            CsmEnabled = 1,
            CsmCascadeRes = 1024,
            ParticleCpuLevel = 2,
            SsaoQuality = 2,
            VolumetricLighting = 1,
            VolumetricFog = 1,

            // 性能设置
            MatVsyncMode = 0,
            MatAntialiasMode = 4,
            DvsEnable = 0,

            // 物理
            ClGibAllow = 1,
            ClRagdollMaxcount = 8,
            ClRagdollSelfCollision = 1
        };
    }

    public ApexVideoConfig GetDefaultPreset()
    {
        return new ApexVideoConfig();
    }

    // ═══════════════════════════════════════════════════════════
    // 解析和序列化
    // ═══════════════════════════════════════════════════════════

    private ApexVideoConfig ParseVideoConfig(string content)
    {
        var config = new ApexVideoConfig();

        // KeyValues 格式解析
        // "VideoConfig"
        // {
        //     "setting.key"    "value"
        // }

        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"""setting\.(\w+)""\s+""(.+)""");
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                ApplyVideoConfigValue(config, key, value);
            }
        }

        return config;
    }

    private void ApplyVideoConfigValue(ApexVideoConfig config, string key, string value)
    {
        // 根据键名设置对应的属性
        switch (key)
        {
            // 显示设置
            case "defaultres":
                if (int.TryParse(value, out var defaultRes))
                    config.DefaultRes = defaultRes;
                break;
            case "defaultresheight":
                if (int.TryParse(value, out var defaultResHeight))
                    config.DefaultResHeight = defaultResHeight;
                break;
            case "m_nRefreshRate":
                if (int.TryParse(value, out var refreshRate))
                    config.RefreshRate = refreshRate;
                break;
            case "fullscreen":
                if (int.TryParse(value, out var fullscreen))
                    config.Fullscreen = fullscreen;
                break;
            case "m_nWindowMode":
                if (int.TryParse(value, out var windowMode))
                    config.WindowMode = windowMode;
                break;
            case "gamma":
                if (float.TryParse(value, out var gamma))
                    config.Gamma = gamma;
                break;

            // 画质设置
            case "mat_picmip":
                if (int.TryParse(value, out var matPicmip))
                    config.MatPicmip = matPicmip;
                break;
            case "mat_forceaniso":
                if (int.TryParse(value, out var matForceaniso))
                    config.MatForceaniso = matForceaniso;
                break;
            case "shadow_enable":
                if (int.TryParse(value, out var shadowEnable))
                    config.ShadowEnable = shadowEnable;
                break;
            case "csm_enabled":
                if (int.TryParse(value, out var csmEnabled))
                    config.CsmEnabled = csmEnabled;
                break;
            case "particle_cpu_level":
                if (int.TryParse(value, out var particleCpuLevel))
                    config.ParticleCpuLevel = particleCpuLevel;
                break;
            case "ssao_quality":
                if (int.TryParse(value, out var ssaoQuality))
                    config.SsaoQuality = ssaoQuality;
                break;
            case "volumetric_lighting":
                if (int.TryParse(value, out var volumetricLighting))
                    config.VolumetricLighting = volumetricLighting;
                break;
            case "volumetric_fog":
                if (int.TryParse(value, out var volumetricFog))
                    config.VolumetricFog = volumetricFog;
                break;

            // 性能设置
            case "mat_vsync_mode":
                if (int.TryParse(value, out var matVsyncMode))
                    config.MatVsyncMode = matVsyncMode;
                break;
            case "mat_antialias_mode":
                if (int.TryParse(value, out var matAntialiasMode))
                    config.MatAntialiasMode = matAntialiasMode;
                break;

            // 物理
            case "cl_gib_allow":
                if (int.TryParse(value, out var clGibAllow))
                    config.ClGibAllow = clGibAllow;
                break;
            case "cl_ragdoll_maxcount":
                if (int.TryParse(value, out var clRagdollMaxcount))
                    config.ClRagdollMaxcount = clRagdollMaxcount;
                break;

            // 系统字段
            case "configversion":
                if (int.TryParse(value, out var configVersion))
                    config.ConfigVersion = configVersion;
                break;
        }
    }

    private string SerializeVideoConfig(ApexVideoConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\"VideoConfig\"");
        sb.AppendLine("{");

        // 显示设置
        sb.AppendLine($"    \"setting.defaultres\"    \"{config.DefaultRes}\"");
        sb.AppendLine($"    \"setting.defaultresheight\"    \"{config.DefaultResHeight}\"");
        sb.AppendLine($"    \"setting.m_nRefreshRate\"    \"{config.RefreshRate}\"");
        sb.AppendLine($"    \"setting.fullscreen\"    \"{config.Fullscreen}\"");
        sb.AppendLine($"    \"setting.m_nWindowMode\"    \"{config.WindowMode}\"");
        sb.AppendLine($"    \"setting.gamma\"    \"{config.Gamma:F6}\"");

        // 画质设置
        sb.AppendLine($"    \"setting.mat_picmip\"    \"{config.MatPicmip}\"");
        sb.AppendLine($"    \"setting.mat_forceaniso\"    \"{config.MatForceaniso}\"");
        sb.AppendLine($"    \"setting.mat_mip_linear\"    \"{config.MatMipLinear}\"");
        sb.AppendLine($"    \"setting.shadow_enable\"    \"{config.ShadowEnable}\"");
        sb.AppendLine($"    \"setting.csm_enabled\"    \"{config.CsmEnabled}\"");
        sb.AppendLine($"    \"setting.csm_cascade_res\"    \"{config.CsmCascadeRes}\"");
        sb.AppendLine($"    \"setting.particle_cpu_level\"    \"{config.ParticleCpuLevel}\"");
        sb.AppendLine($"    \"setting.ssao_quality\"    \"{config.SsaoQuality}\"");
        sb.AppendLine($"    \"setting.volumetric_lighting\"    \"{config.VolumetricLighting}\"");
        sb.AppendLine($"    \"setting.volumetric_fog\"    \"{config.VolumetricFog}\"");

        // 性能设置
        sb.AppendLine($"    \"setting.mat_vsync_mode\"    \"{config.MatVsyncMode}\"");
        sb.AppendLine($"    \"setting.mat_antialias_mode\"    \"{config.MatAntialiasMode}\"");
        sb.AppendLine($"    \"setting.dvs_enable\"    \"{config.DvsEnable}\"");

        // 物理
        sb.AppendLine($"    \"setting.cl_gib_allow\"    \"{config.ClGibAllow}\"");
        sb.AppendLine($"    \"setting.cl_ragdoll_maxcount\"    \"{config.ClRagdollMaxcount}\"");
        sb.AppendLine($"    \"setting.cl_ragdoll_self_collision\"    \"{config.ClRagdollSelfCollision}\"");

        // 系统字段
        sb.AppendLine($"    \"setting.configversion\"    \"{config.ConfigVersion}\"");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private ApexSettingsConfig ParseSettingsConfig(string content)
    {
        var config = new ApexSettingsConfig();

        // Valve CFG 格式解析
        // setting_name "value"
        // bind_US_standard "key" "command" flags

        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // 跳过注释和空行
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;

            // 解析设置项
            var settingMatch = Regex.Match(trimmedLine, @"^(\w+)\s+""(.+)""");
            if (settingMatch.Success)
            {
                var key = settingMatch.Groups[1].Value;
                var value = settingMatch.Groups[2].Value;

                ApplySettingsConfigValue(config, key, value);
            }
        }

        return config;
    }

    private void ApplySettingsConfigValue(ApexSettingsConfig config, string key, string value)
    {
        switch (key)
        {
            // 鼠标设置
            case "mouse_sensitivity":
                if (float.TryParse(value, out var mouseSensitivity))
                    config.MouseSensitivity = mouseSensitivity;
                break;
            case "m_acceleration":
                if (int.TryParse(value, out var mAcceleration))
                    config.MAcceleration = mAcceleration;
                break;

            // 瞄准镜灵敏度
            case "mouse_use_per_scope_sensitivity_scalars":
                if (int.TryParse(value, out var usePerScope))
                    config.MouseUsePerScopeSensitivityScalars = usePerScope;
                break;
            case "mouse_zoomed_sensitivity_scalar_0":
                if (float.TryParse(value, out var scalar0))
                    config.MouseZoomedSensitivityScalar0 = scalar0;
                break;
            case "mouse_zoomed_sensitivity_scalar_1":
                if (float.TryParse(value, out var scalar1))
                    config.MouseZoomedSensitivityScalar1 = scalar1;
                break;
            case "mouse_zoomed_sensitivity_scalar_2":
                if (float.TryParse(value, out var scalar2))
                    config.MouseZoomedSensitivityScalar2 = scalar2;
                break;
            case "mouse_zoomed_sensitivity_scalar_3":
                if (float.TryParse(value, out var scalar3))
                    config.MouseZoomedSensitivityScalar3 = scalar3;
                break;
            case "mouse_zoomed_sensitivity_scalar_4":
                if (float.TryParse(value, out var scalar4))
                    config.MouseZoomedSensitivityScalar4 = scalar4;
                break;
            case "mouse_zoomed_sensitivity_scalar_5":
                if (float.TryParse(value, out var scalar5))
                    config.MouseZoomedSensitivityScalar5 = scalar5;
                break;
            case "mouse_zoomed_sensitivity_scalar_6":
                if (float.TryParse(value, out var scalar6))
                    config.MouseZoomedSensitivityScalar6 = scalar6;
                break;
            case "mouse_zoomed_sensitivity_scalar_7":
                if (float.TryParse(value, out var scalar7))
                    config.MouseZoomedSensitivityScalar7 = scalar7;
                break;

            // 帧率设置
            case "fps_max":
                if (int.TryParse(value, out var fpsMax))
                    config.FpsMax = fpsMax;
                break;
            case "cl_showfps":
                if (int.TryParse(value, out var clShowfps))
                    config.ClShowfps = clShowfps;
                break;
            case "cl_showpos":
                if (int.TryParse(value, out var clShowpos))
                    config.ClShowpos = clShowpos;
                break;

            // 视野设置
            case "cl_fovScale":
                if (float.TryParse(value, out var clFovScale))
                    config.ClFovScale = clFovScale;
                break;

            // 网络设置
            case "cl_interp":
                if (float.TryParse(value, out var clInterp))
                    config.ClInterp = clInterp;
                break;
            case "cl_interp_ratio":
                if (int.TryParse(value, out var clInterpRatio))
                    config.ClInterpRatio = clInterpRatio;
                break;
        }
    }

    private string SerializeSettingsConfig(ApexSettingsConfig config)
    {
        var sb = new StringBuilder();

        // 鼠标设置
        sb.AppendLine($"mouse_sensitivity \"{config.MouseSensitivity}\"");
        sb.AppendLine($"m_acceleration \"{config.MAcceleration}\"");
        sb.AppendLine();

        // 瞄准镜灵敏度
        sb.AppendLine($"mouse_use_per_scope_sensitivity_scalars \"{config.MouseUsePerScopeSensitivityScalars}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_0 \"{config.MouseZoomedSensitivityScalar0}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_1 \"{config.MouseZoomedSensitivityScalar1}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_2 \"{config.MouseZoomedSensitivityScalar2}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_3 \"{config.MouseZoomedSensitivityScalar3}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_4 \"{config.MouseZoomedSensitivityScalar4}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_5 \"{config.MouseZoomedSensitivityScalar5}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_6 \"{config.MouseZoomedSensitivityScalar6}\"");
        sb.AppendLine($"mouse_zoomed_sensitivity_scalar_7 \"{config.MouseZoomedSensitivityScalar7}\"");
        sb.AppendLine();

        // 帧率设置
        sb.AppendLine($"fps_max \"{config.FpsMax}\"");
        sb.AppendLine($"cl_showfps \"{config.ClShowfps}\"");
        sb.AppendLine($"cl_showpos \"{config.ClShowpos}\"");
        sb.AppendLine();

        // 视野设置
        sb.AppendLine($"cl_fovScale \"{config.ClFovScale}\"");
        sb.AppendLine();

        // 网络设置
        sb.AppendLine($"cl_interp \"{config.ClInterp}\"");
        sb.AppendLine($"cl_interp_ratio \"{config.ClInterpRatio}\"");

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════
    // 文件验证
    // ═══════════════════════════════════════════════════════════

    private bool ValidateVideoConfigFile(string content)
    {
        // 检查是否包含 VideoConfig 标记
        return content.Contains("\"VideoConfig\"");
    }

    private bool ValidateSettingsConfigFile(string content)
    {
        // 检查是否包含常见的设置项
        return content.Contains("mouse_sensitivity") || content.Contains("fps_max");
    }

    // ═══════════════════════════════════════════════════════════
    // 历史版本管理
    // ═══════════════════════════════════════════════════════════

    public List<BackupFileInfo> GetVideoConfigBackups()
    {
        var backupDir = Path.Combine(_backupDir, "videoconfig");
        if (!Directory.Exists(backupDir))
            return new List<BackupFileInfo>();

        return Directory.GetFiles(backupDir, "*.txt")
            .Select(filePath => new BackupFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                BackupTime = ParseBackupTime(Path.GetFileName(filePath)),
                FileSize = new FileInfo(filePath).Length
            })
            .OrderByDescending(b => b.BackupTime)
            .ToList();
    }

    public List<BackupFileInfo> GetSettingsConfigBackups()
    {
        var backupDir = Path.Combine(_backupDir, "settings");
        if (!Directory.Exists(backupDir))
            return new List<BackupFileInfo>();

        return Directory.GetFiles(backupDir, "*.cfg")
            .Select(filePath => new BackupFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                BackupTime = ParseBackupTime(Path.GetFileName(filePath)),
                FileSize = new FileInfo(filePath).Length
            })
            .OrderByDescending(b => b.BackupTime)
            .ToList();
    }

    public async Task<bool> RestoreVideoConfigFromBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
            return false;

        try
        {
            var targetPath = GetVideoConfigPath();
            if (targetPath == null)
                return false;

            // 先备份当前配置
            await BackupVideoConfigAsync();

            await _fileLock.WaitAsync();
            try
            {
                File.Copy(backupPath, targetPath, overwrite: true);
                return true;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreSettingsConfigFromBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
            return false;

        try
        {
            var targetPath = GetSettingsConfigPath();
            if (targetPath == null)
                return false;

            // 先备份当前配置
            await BackupSettingsConfigAsync();

            await _fileLock.WaitAsync();
            try
            {
                File.Copy(backupPath, targetPath, overwrite: true);
                return true;
            }
            finally
            {
                _fileLock.Release();
            }
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteBackup(string backupPath)
    {
        try
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private DateTime ParseBackupTime(string fileName)
    {
        // 格式: yyyy-MM-dd_HH-mm-ss_videoconfig.txt
        var match = Regex.Match(fileName, @"(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})");
        if (match.Success)
        {
            var dateStr = match.Groups[1].Value;
            if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd_HH-mm-ss", null,
                System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        return File.GetCreationTime(Path.Combine(_backupDir, fileName));
    }
}