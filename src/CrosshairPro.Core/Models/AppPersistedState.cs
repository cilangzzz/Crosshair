namespace CrosshairPro.Core.Models;

/// <summary>
/// 应用持久化状态（记录当前使用的预设ID等）
/// </summary>
public class AppPersistedState
{
    /// <summary>当前使用的预设ID（null或空表示使用默认配置）</summary>
    public string? CurrentPresetId { get; set; }

    /// <summary>当前配置是否已修改（未保存到预设）</summary>
    public bool IsConfigModified { get; set; }

    /// <summary>用户选择的语言（如 "zh-CN"、"en-US"），null 表示跟随系统</summary>
    public string? Language { get; set; }
}
