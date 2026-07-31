using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Interfaces;

/// <summary>
/// Apex Legends 配置服务接口
/// 管理 videoconfig.txt 和 settings.cfg 配置文件
/// </summary>
public interface IApexConfigService
{
    // ═══════════════════════════════════════════════════════════
    // 配置文件加载和保存
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 加载 videoconfig.txt 配置
    /// </summary>
    /// <returns>视频配置对象，如果文件不存在则返回默认配置</returns>
    Task<ApexVideoConfig> LoadVideoConfigAsync();

    /// <summary>
    /// 加载 settings.cfg 配置
    /// </summary>
    /// <returns>游戏设置配置对象，如果文件不存在则返回默认配置</returns>
    Task<ApexSettingsConfig> LoadSettingsConfigAsync();

    /// <summary>
    /// 保存 videoconfig.txt 配置
    /// </summary>
    /// <param name="config">视频配置对象</param>
    Task SaveVideoConfigAsync(ApexVideoConfig config);

    /// <summary>
    /// 保存 settings.cfg 配置
    /// </summary>
    /// <param name="config">游戏设置配置对象</param>
    Task SaveSettingsConfigAsync(ApexSettingsConfig config);

    // ═══════════════════════════════════════════════════════════
    // 配置文件替换和备份
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 替换 videoconfig.txt 配置文件
    /// 自动备份当前配置文件
    /// </summary>
    /// <param name="filePath">新配置文件路径</param>
    /// <returns>是否替换成功</returns>
    Task<bool> ReplaceVideoConfigAsync(string filePath);

    /// <summary>
    /// 替换 settings.cfg 配置文件
    /// 自动备份当前配置文件
    /// </summary>
    /// <param name="filePath">新配置文件路径</param>
    /// <returns>是否替换成功</returns>
    Task<bool> ReplaceSettingsConfigAsync(string filePath);

    /// <summary>
    /// 备份 videoconfig.txt 配置文件
    /// </summary>
    /// <returns>备份文件路径</returns>
    Task<string> BackupVideoConfigAsync();

    /// <summary>
    /// 备份 settings.cfg 配置文件
    /// </summary>
    /// <returns>备份文件路径</returns>
    Task<string> BackupSettingsConfigAsync();

    // ═══════════════════════════════════════════════════════════
    // 导出功能
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 导出 videoconfig.txt 到指定路径
    /// </summary>
    /// <param name="filePath">目标文件路径</param>
    Task ExportVideoConfigAsync(string filePath);

    /// <summary>
    /// 导出 settings.cfg 到指定路径
    /// </summary>
    /// <param name="filePath">目标文件路径</param>
    Task ExportSettingsConfigAsync(string filePath);

    /// <summary>
    /// 导出启动选项到文件
    /// </summary>
    /// <param name="filePath">目标文件路径</param>
    /// <param name="options">启动选项内容</param>
    Task ExportLaunchOptionsAsync(string filePath, string options);

    // ═══════════════════════════════════════════════════════════
    // 启动选项管理
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 根据当前配置生成启动选项字符串
    /// </summary>
    /// <returns>启动选项字符串</returns>
    string GenerateLaunchOptions();

    /// <summary>
    /// 加载启动选项（从应用配置中）
    /// </summary>
    /// <returns>启动选项字符串</returns>
    Task<string> LoadLaunchOptionsAsync();

    /// <summary>
    /// 保存启动选项（保存到应用配置）
    /// </summary>
    /// <param name="options">启动选项字符串</param>
    Task SaveLaunchOptionsAsync(string options);

    // ═══════════════════════════════════════════════════════════
    // 配置文件路径
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 获取 videoconfig.txt 文件路径
    /// </summary>
    /// <returns>文件路径，如果找不到则返回 null</returns>
    string? GetVideoConfigPath();

    /// <summary>
    /// 获取 settings.cfg 文件路径
    /// </summary>
    /// <returns>文件路径，如果找不到则返回 null</returns>
    string? GetSettingsConfigPath();

    /// <summary>
    /// 检测 Apex Legends 是否已安装
    /// </summary>
    /// <returns>是否已安装</returns>
    bool IsApexInstalled();

    // ═══════════════════════════════════════════════════════════
    // 预设配置
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 获取竞技优化预设
    /// </summary>
    /// <returns>竞技优化视频配置</returns>
    ApexVideoConfig GetCompetitivePreset();

    /// <summary>
    /// 获取高画质预设
    /// </summary>
    /// <returns>高画质视频配置</returns>
    ApexVideoConfig GetHighQualityPreset();

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    /// <returns>默认视频配置</returns>
    ApexVideoConfig GetDefaultPreset();

    // ═══════════════════════════════════════════════════════════
    // 历史版本管理
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// 获取 videoconfig.txt 的所有备份历史
    /// </summary>
    /// <returns>备份文件列表（按时间倒序）</returns>
    List<BackupFileInfo> GetVideoConfigBackups();

    /// <summary>
    /// 获取 settings.cfg 的所有备份历史
    /// </summary>
    /// <returns>备份文件列表（按时间倒序）</returns>
    List<BackupFileInfo> GetSettingsConfigBackups();

    /// <summary>
    /// 从备份恢复 videoconfig.txt
    /// </summary>
    /// <param name="backupPath">备份文件路径</param>
    /// <returns>是否恢复成功</returns>
    Task<bool> RestoreVideoConfigFromBackupAsync(string backupPath);

    /// <summary>
    /// 从备份恢复 settings.cfg
    /// </summary>
    /// <param name="backupPath">备份文件路径</param>
    /// <returns>是否恢复成功</returns>
    Task<bool> RestoreSettingsConfigFromBackupAsync(string backupPath);

    /// <summary>
    /// 删除指定的备份文件
    /// </summary>
    /// <param name="backupPath">备份文件路径</param>
    /// <returns>是否删除成功</returns>
    bool DeleteBackup(string backupPath);
}

/// <summary>
/// 备份文件信息
/// </summary>
public class BackupFileInfo
{
    /// <summary>文件路径</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>文件名</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>备份时间</summary>
    public DateTime BackupTime { get; set; }

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>格式化的备份时间显示</summary>
    public string DisplayTime => BackupTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>格式化的文件大小显示</summary>
    public string DisplaySize => FormatFileSize(FileSize);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}