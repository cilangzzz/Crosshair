using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Interfaces;

/// <summary>
/// 配置管理服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 获取当前配置
    /// </summary>
    CrosshairConfig GetCurrentConfig();

    /// <summary>
    /// 从持久化加载配置
    /// </summary>
    Task LoadConfigAsync();

    /// <summary>
    /// 保存配置到持久化
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    Task ResetToDefaultAsync();

    /// <summary>
    /// 克隆配置（深拷贝）
    /// </summary>
    CrosshairConfig CloneConfig(CrosshairConfig source);

    /// <summary>
    /// 复制配置（从源到目标）
    /// </summary>
    void CopyConfig(CrosshairConfig source, CrosshairConfig target);

    /// <summary>
    /// 创建默认配置
    /// </summary>
    CrosshairConfig CreateDefaultConfig();
}