using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Interfaces;

/// <summary>
/// 预设管理服务接口
/// </summary>
public interface IPresetService
{
    /// <summary>
    /// 加载所有预设（包含默认预设）
    /// </summary>
    Task<IReadOnlyList<Preset>> LoadAllPresetsAsync();

    /// <summary>
    /// 保存预设
    /// </summary>
    Task SavePresetAsync(Preset preset);

    /// <summary>
    /// 删除预设（默认预设不可删除）
    /// </summary>
    Task DeletePresetAsync(string presetId);

    /// <summary>
    /// 从文件导入预设
    /// </summary>
    Task<Preset> ImportPresetAsync(string filePath);

    /// <summary>
    /// 导出预设到文件
    /// </summary>
    Task ExportPresetAsync(Preset preset, string filePath);

    /// <summary>
    /// 设置当前使用的预设
    /// </summary>
    Task SetCurrentPresetAsync(string presetId);

    /// <summary>
    /// 获取当前使用的预设ID
    /// </summary>
    Task<string?> GetCurrentPresetIdAsync();
}