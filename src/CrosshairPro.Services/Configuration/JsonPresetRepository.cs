using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Services.Configuration;

/// <summary>
/// JSON预设仓库实现
/// </summary>
public class JsonPresetRepository : IPresetRepository
{
    private readonly string _presetsDirectory;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonPresetRepository()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appPath = Path.Combine(appDataPath, "CrosshairPro");
        _presetsDirectory = Path.Combine(appPath, "presets");
        Directory.CreateDirectory(_presetsDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 加载所有预设
    /// </summary>
    public async Task<IReadOnlyList<Preset>> LoadPresetsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var presets = new List<Preset>();

            if (!Directory.Exists(_presetsDirectory))
            {
                Directory.CreateDirectory(_presetsDirectory);
                return presets;
            }

            foreach (var file in Directory.GetFiles(_presetsDirectory, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions);
                    if (preset != null)
                    {
                        presets.Add(preset);
                    }
                }
                catch
                {
                    // 忽略解析错误的文件
                }
            }

            return presets;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 保存预设
    /// </summary>
    public async Task SavePresetAsync(Preset preset)
    {
        await _fileLock.WaitAsync();
        try
        {
            preset.UpdatedAt = DateTime.UtcNow;
            var filePath = GetPresetFilePath(preset.Id);
            var json = JsonSerializer.Serialize(preset, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 删除预设
    /// </summary>
    public async Task DeletePresetAsync(string presetId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var filePath = GetPresetFilePath(presetId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 获取预设
    /// </summary>
    public async Task<Preset?> GetPresetAsync(string presetId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var filePath = GetPresetFilePath(presetId);
            if (!File.Exists(filePath))
                return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<Preset>(json, _jsonOptions);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 导出预设
    /// </summary>
    public async Task ExportPresetAsync(string presetId, string filePath)
    {
        var preset = await GetPresetAsync(presetId);
        if (preset == null)
            throw new InvalidOperationException($"预设 {presetId} 不存在");

        var json = JsonSerializer.Serialize(preset, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 导入预设
    /// </summary>
    public async Task<Preset> ImportPresetAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var preset = JsonSerializer.Deserialize<Preset>(json, _jsonOptions)
            ?? throw new InvalidOperationException("预设文件格式错误");

        // 生成新ID避免冲突
        preset.Id = Guid.NewGuid().ToString();
        preset.CreatedAt = DateTime.UtcNow;
        preset.UpdatedAt = DateTime.UtcNow;

        await SavePresetAsync(preset);
        return preset;
    }

    private string GetPresetFilePath(string presetId)
    {
        return Path.Combine(_presetsDirectory, $"{presetId}.json");
    }
}
