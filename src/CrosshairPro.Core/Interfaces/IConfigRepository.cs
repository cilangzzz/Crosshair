using CrosshairPro.Core.Models;

namespace CrosshairPro.Core.Interfaces;

/// <summary>
/// 配置仓库接口
/// </summary>
public interface IConfigRepository
{
    /// <summary>
    /// 加载主配置
    /// </summary>
    Task<CrosshairConfig> LoadConfigAsync();

    /// <summary>
    /// 保存主配置
    /// </summary>
    Task SaveConfigAsync(CrosshairConfig config);

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    Task<CrosshairConfig> ResetToDefaultAsync();

    /// <summary>
    /// 导出配置
    /// </summary>
    Task ExportConfigAsync(string filePath, CrosshairConfig config);

    /// <summary>
    /// 导入配置
    /// </summary>
    Task<CrosshairConfig> ImportConfigAsync(string filePath);
}

/// <summary>
/// 预设仓库接口
/// </summary>
public interface IPresetRepository
{
    /// <summary>
    /// 加载所有预设
    /// </summary>
    Task<IReadOnlyList<Preset>> LoadPresetsAsync();

    /// <summary>
    /// 保存预设
    /// </summary>
    Task SavePresetAsync(Preset preset);

    /// <summary>
    /// 删除预设
    /// </summary>
    Task DeletePresetAsync(string presetId);

    /// <summary>
    /// 获取预设
    /// </summary>
    Task<Preset?> GetPresetAsync(string presetId);

    /// <summary>
    /// 导出预设
    /// </summary>
    Task ExportPresetAsync(string presetId, string filePath);

    /// <summary>
    /// 导入预设
    /// </summary>
    Task<Preset> ImportPresetAsync(string filePath);
}