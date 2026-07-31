namespace CrosshairPro.Core.Models;

/// <summary>
/// 配置项类型
/// </summary>
public enum ConfigItemType
{
    /// <summary>布尔开关</summary>
    Bool,

    /// <summary>整数数值</summary>
    Int,

    /// <summary>枚举选择</summary>
    Enum,

    /// <summary>字符串</summary>
    String
}

/// <summary>
/// 配置项定义
/// 描述单个配置项的元数据
/// </summary>
public class ConfigItemDefinition
{
    /// <summary>配置项ID（唯一键）</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>配置项类型</summary>
    public ConfigItemType Type { get; set; }

    /// <summary>默认值</summary>
    public object? DefaultValue { get; set; }

    /// <summary>最小值（Int类型）</summary>
    public int? MinValue { get; set; }

    /// <summary>最大值（Int类型）</summary>
    public int? MaxValue { get; set; }

    /// <summary>枚举选项（Enum类型）</summary>
    public List<string>? Options { get; set; }

    /// <summary>描述说明</summary>
    public string? Description { get; set; }

    /// <summary>是否需要重启游戏生效</summary>
    public bool RequiresRestart { get; set; }
}

/// <summary>
/// 配置分区定义
/// 将配置项按功能分组
/// </summary>
public class ConfigSectionDefinition
{
    /// <summary>分区名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>分区显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>配置项列表</summary>
    public List<ConfigItemDefinition> Items { get; set; } = new();
}

/// <summary>
/// 游戏配置策略定义
/// 定义每个游戏支持的配置项和操作方式
/// </summary>
public class GameConfigStrategy
{
    /// <summary>游戏ID</summary>
    public string GameId { get; set; } = string.Empty;

    /// <summary>是否支持启动项</summary>
    public bool SupportsLaunchOptions { get; set; } = true;

    /// <summary>启动项说明</summary>
    public string? LaunchOptionsDescription { get; set; }

    /// <summary>配置分区列表</summary>
    public List<ConfigSectionDefinition> Sections { get; set; } = new();

    /// <summary>配置文件路径模板（支持环境变量）</summary>
    public string? ConfigFilePath { get; set; }
}