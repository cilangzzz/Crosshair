using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Services.Configuration;

/// <summary>
/// JSON配置仓库实现
/// </summary>
public class JsonConfigRepository : IConfigRepository
{
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonConfigRepository()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appPath = Path.Combine(appDataPath, "CrosshairPro");
        Directory.CreateDirectory(appPath);
        _configFilePath = Path.Combine(appPath, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 加载主配置
    /// </summary>
    public async Task<CrosshairConfig> LoadConfigAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(_configFilePath))
            {
                return CreateDefaultConfig();
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<CrosshairConfig>(json, _jsonOptions);
            return config ?? CreateDefaultConfig();
        }
        catch
        {
            return CreateDefaultConfig();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 保存主配置
    /// </summary>
    public async Task SaveConfigAsync(CrosshairConfig config)
    {
        await _fileLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public Task<CrosshairConfig> ResetToDefaultAsync()
    {
        return Task.FromResult(CreateDefaultConfig());
    }

    /// <summary>
    /// 导出配置
    /// </summary>
    public async Task ExportConfigAsync(string filePath, CrosshairConfig config)
    {
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 导入配置
    /// </summary>
    public async Task<CrosshairConfig> ImportConfigAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var config = JsonSerializer.Deserialize<CrosshairConfig>(json, _jsonOptions);
        return config ?? CreateDefaultConfig();
    }

    /// <summary>
    /// 创建默认配置
    /// </summary>
    private static CrosshairConfig CreateDefaultConfig()
    {
        return new CrosshairConfig
        {
            Name = "默认配置",
            Style = Core.Enums.CrosshairStyle.Cross,
            Size = 20,
            Gap = 4,
            Thickness = 2,
            Color = "#00FF00",
            Opacity = 100,
            CenterSize = 4
        };
    }
}
