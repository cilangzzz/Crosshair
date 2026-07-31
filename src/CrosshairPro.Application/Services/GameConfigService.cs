using System.IO;
using System.Text.Json;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Services;

/// <summary>
/// 游戏配置服务实现
/// </summary>
public class GameConfigService : IGameConfigService
{
    private readonly Dictionary<string, GameConfigStrategy> _strategies;
    private readonly string _configDir;
    private readonly Dictionary<string, GameConfig> _configCache = new();

    public GameConfigService()
    {
        _configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CrosshairPro",
            "gameconfigs");

        Directory.CreateDirectory(_configDir);

        // 初始化游戏策略
        _strategies = InitializeStrategies();
    }

    /// <summary>
    /// 初始化各游戏的配置策略
    /// </summary>
    private Dictionary<string, GameConfigStrategy> InitializeStrategies()
    {
        return new Dictionary<string, GameConfigStrategy>
        {
            ["builtin-cs2"] = CreateCS2Strategy(),
            ["builtin-valorant"] = CreateValorantStrategy(),
            ["builtin-apex"] = CreateApexStrategy(),
            ["builtin-overwatch2"] = CreateOverwatch2Strategy(),
            ["builtin-pubg"] = CreatePUBGStrategy(),
            ["builtin-fortnite"] = CreateFortniteStrategy(),
            ["builtin-r6"] = CreateR6Strategy(),
            ["builtin-csgo"] = CreateCSGOStrategy()
        };
    }

    // ── 游戏策略定义 ───────────────────────────────────────────────────

    private GameConfigStrategy CreateCS2Strategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-cs2",
            SupportsLaunchOptions = true,
            LaunchOptionsDescription = "CS2 启动项参数，如 -high -threads 12 -novid",
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true,
                            Description = "启用全屏模式以获得最佳性能"
                        },
                        new()
                        {
                            Key = "resolution",
                            DisplayName = "分辨率",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "1920x1080",
                            Options = new List<string>
                            {
                                "1920x1080", "1680x1050", "1600x900", "1440x900", "1280x1024", "1280x960", "1280x800", "1280x720"
                            }
                        },
                        new()
                        {
                            Key = "aspect_ratio",
                            DisplayName = "宽高比",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "16:9",
                            Options = new List<string> { "16:9", "16:10", "4:3" }
                        },
                        new()
                        {
                            Key = "refresh_rate",
                            DisplayName = "刷新率 (Hz)",
                            Type = ConfigItemType.Int,
                            DefaultValue = 144,
                            MinValue = 60,
                            MaxValue = 360
                        }
                    }
                },
                new()
                {
                    Name = "game",
                    DisplayName = "游戏设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fps_max",
                            DisplayName = "最大 FPS",
                            Type = ConfigItemType.Int,
                            DefaultValue = 0,
                            MinValue = 0,
                            MaxValue = 999,
                            Description = "0 表示无限制"
                        },
                        new()
                        {
                            Key = "cl_showfps",
                            DisplayName = "显示 FPS",
                            Type = ConfigItemType.Bool,
                            DefaultValue = false
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateValorantStrategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-valorant",
            SupportsLaunchOptions = false, // Valorant 不支持启动项
            LaunchOptionsDescription = "Valorant 不支持自定义启动项",
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "resolution",
                            DisplayName = "分辨率",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "1920x1080",
                            Options = new List<string>
                            {
                                "1920x1080", "1680x1050", "1600x900", "1440x900", "1280x1024", "1280x960", "1280x720"
                            }
                        },
                        new()
                        {
                            Key = "fps_limit",
                            DisplayName = "FPS 限制",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "Unlimited",
                            Options = new List<string> { "Unlimited", "300", "240", "144", "120", "60" }
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateApexStrategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-apex",
            SupportsLaunchOptions = true,
            LaunchOptionsDescription = "Apex 启动项，如 +fps_max unlimited -freq 144",
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "fps_max",
                            DisplayName = "最大 FPS",
                            Type = ConfigItemType.Int,
                            DefaultValue = 0,
                            MinValue = 0,
                            MaxValue = 999,
                            Description = "0 = unlimited"
                        }
                    }
                },
                new()
                {
                    Name = "game",
                    DisplayName = "游戏设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "cl_showpos",
                            DisplayName = "显示位置信息",
                            Type = ConfigItemType.Bool,
                            DefaultValue = false
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateOverwatch2Strategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-overwatch2",
            SupportsLaunchOptions = false,
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "resolution",
                            DisplayName = "分辨率",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "1920x1080",
                            Options = new List<string>
                            {
                                "1920x1080", "1680x1050", "1600x900", "1440x900", "1280x720"
                            }
                        },
                        new()
                        {
                            Key = "fps_limit",
                            DisplayName = "FPS 限制",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "300",
                            Options = new List<string> { "Unlimited", "300", "240", "144", "120", "60" }
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreatePUBGStrategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-pubg",
            SupportsLaunchOptions = true,
            LaunchOptionsDescription = "PUBG 启动项",
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "resolution",
                            DisplayName = "分辨率",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "1920x1080",
                            Options = new List<string>
                            {
                                "1920x1080", "1680x1050", "1600x900", "1280x720"
                            }
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateFortniteStrategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-fortnite",
            SupportsLaunchOptions = false,
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "fps_limit",
                            DisplayName = "FPS 限制",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "Unlimited",
                            Options = new List<string> { "Unlimited", "240", "144", "120", "60" }
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateR6Strategy()
    {
        return new GameConfigStrategy
        {
            GameId = "builtin-r6",
            SupportsLaunchOptions = true,
            LaunchOptionsDescription = "R6 启动项",
            Sections = new List<ConfigSectionDefinition>
            {
                new()
                {
                    Name = "video",
                    DisplayName = "视频设置",
                    Items = new List<ConfigItemDefinition>
                    {
                        new()
                        {
                            Key = "fullscreen",
                            DisplayName = "全屏模式",
                            Type = ConfigItemType.Bool,
                            DefaultValue = true
                        },
                        new()
                        {
                            Key = "resolution",
                            DisplayName = "分辨率",
                            Type = ConfigItemType.Enum,
                            DefaultValue = "1920x1080",
                            Options = new List<string>
                            {
                                "1920x1080", "1680x1050", "1600x900", "1280x720"
                            }
                        }
                    }
                }
            }
        };
    }

    private GameConfigStrategy CreateCSGOStrategy()
    {
        // CS:GO 策略与 CS2 类似
        var strategy = CreateCS2Strategy();
        strategy.GameId = "builtin-csgo";
        return strategy;
    }

    // ── IGameConfigService 实现 ───────────────────────────────────────────────────

    public IReadOnlyList<GameConfigStrategy> GetStrategies()
    {
        return _strategies.Values.ToList().AsReadOnly();
    }

    public GameConfigStrategy? GetStrategy(string gameId)
    {
        return _strategies.TryGetValue(gameId, out var strategy) ? strategy : null;
    }

    public async Task<GameConfig?> GetConfigAsync(string gameId)
    {
        if (_configCache.TryGetValue(gameId, out var cached))
            return cached;

        var filePath = GetConfigFilePath(gameId);
        if (!File.Exists(filePath))
        {
            var defaultConfig = CreateDefaultConfig(gameId);
            _configCache[gameId] = defaultConfig;
            return defaultConfig;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var config = JsonSerializer.Deserialize<GameConfig>(json);
            if (config != null)
            {
                _configCache[gameId] = config;
                return config;
            }
        }
        catch
        {
            // 忽略错误，返回默认配置
        }

        return CreateDefaultConfig(gameId);
    }

    public async Task SaveConfigAsync(GameConfig config)
    {
        _configCache[config.GameId] = config;

        var filePath = GetConfigFilePath(config.GameId);
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ResetToDefaultAsync(string gameId)
    {
        var defaultConfig = CreateDefaultConfig(gameId);
        await SaveConfigAsync(defaultConfig);
    }

    public Task ApplyConfigAsync(string gameId)
    {
        // TODO: 实现写入游戏配置文件的逻辑
        // 这需要根据每个游戏的具体配置文件格式来实现
        // 目前只保存到 CrosshairPro 的配置文件中
        return Task.CompletedTask;
    }

    // ── 辅助方法 ───────────────────────────────────────────────────

    private string GetConfigFilePath(string gameId)
    {
        return Path.Combine(_configDir, $"{gameId}.json");
    }

    private GameConfig CreateDefaultConfig(string gameId)
    {
        var strategy = GetStrategy(gameId);
        var config = new GameConfig { GameId = gameId };

        if (strategy != null)
        {
            foreach (var section in strategy.Sections)
            {
                foreach (var item in section.Items)
                {
                    if (item.DefaultValue != null)
                    {
                        config.Settings[item.Key] = item.DefaultValue;
                    }
                }
            }
        }

        return config;
    }
}