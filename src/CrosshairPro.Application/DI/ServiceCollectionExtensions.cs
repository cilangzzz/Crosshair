using CrosshairPro.Application.Interfaces;
using CrosshairPro.Application.Services;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Infrastructure.Hotkey;
using CrosshairPro.Services.Configuration;
using CrosshairPro.Services.Crosshair;
using Microsoft.Extensions.DependencyInjection;

namespace CrosshairPro.Application.DI;

/// <summary>
/// 服务注册扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 CrosshairPro 所有服务
    /// </summary>
    public static IServiceCollection AddCrosshairProServices(this IServiceCollection services)
    {
        // Repositories (Singleton - 单例共享状态)
        services.AddSingleton<JsonConfigRepository>();
        services.AddSingleton<IConfigRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());
        services.AddSingleton<IAppStateRepository>(sp => sp.GetRequiredService<JsonConfigRepository>());

        services.AddSingleton<IPresetRepository, JsonPresetRepository>();

        // Application Services
        services.AddSingleton<IPresetService, PresetService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IGameConfigService, GameConfigService>();

        // Apex Legends 配置服务
        services.AddSingleton<IApexConfigService, ApexConfigService>();

        // Infrastructure Services
        services.AddSingleton<IHotkeyManager, HotkeyManager>();
        services.AddSingleton<ICrosshairRenderer, CrosshairRenderer>();

        return services;
    }
}