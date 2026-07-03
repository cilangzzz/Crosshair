using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrosshairPro.Application.Interfaces;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;

namespace CrosshairPro.Application.Services;

/// <summary>
/// 预设管理服务实现
/// </summary>
public class PresetService : IPresetService
{
    private readonly IPresetRepository _presetRepository;
    private readonly IAppStateRepository _stateRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public PresetService(
        IPresetRepository presetRepository,
        IAppStateRepository stateRepository)
    {
        _presetRepository = presetRepository;
        _stateRepository = stateRepository;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 加载所有预设（包含默认预设）
    /// </summary>
    public async Task<IReadOnlyList<Preset>> LoadAllPresetsAsync()
    {
        var presets = await _presetRepository.LoadPresetsAsync();
        var list = presets.ToList();

        // 始终在最前面放一个默认预设
        var defaultPreset = new Preset
        {
            Id = "default",
            Name = "默认配置",
            Config = new CrosshairConfig(),
            IsDefault = true
        };
        list.Insert(0, defaultPreset);

        return list;
    }

    /// <summary>
    /// 保存预设
    /// </summary>
    public async Task SavePresetAsync(Preset preset)
    {
        if (preset.IsDefault)
            return; // 默认预设不可保存

        await _presetRepository.SavePresetAsync(preset);
    }

    /// <summary>
    /// 删除预设（默认预设不可删除）
    /// </summary>
    public async Task DeletePresetAsync(string presetId)
    {
        if (presetId == "default")
            return; // 默认预设不可删除

        await _presetRepository.DeletePresetAsync(presetId);
    }

    /// <summary>
    /// 从文件导入预设
    /// </summary>
    public async Task<Preset> ImportPresetAsync(string filePath)
    {
        var preset = await _presetRepository.ImportPresetAsync(filePath);
        return preset;
    }

    /// <summary>
    /// 导出预设到文件
    /// </summary>
    public async Task ExportPresetAsync(Preset preset, string filePath)
    {
        // 直接写入文件（不通过 Repository，因为 Repository 会生成新ID）
        var json = JsonSerializer.Serialize(preset, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 设置当前使用的预设
    /// </summary>
    public async Task SetCurrentPresetAsync(string presetId)
    {
        var state = await _stateRepository.LoadStateAsync();
        state.CurrentPresetId = presetId;
        await _stateRepository.SaveStateAsync(state);
    }

    /// <summary>
    /// 获取当前使用的预设ID
    /// </summary>
    public async Task<string?> GetCurrentPresetIdAsync()
    {
        var state = await _stateRepository.LoadStateAsync();
        return state.CurrentPresetId;
    }
}