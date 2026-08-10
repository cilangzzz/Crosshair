using System.Windows;
using CrosshairPro.Application.DI;
using CrosshairPro.App.Localization;
using CrosshairPro.App.ViewModels;
using CrosshairPro.App.Views;
using CrosshairPro.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CrosshairPro.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider? _services;

    /// <summary>
    /// 服务提供者，用于获取服务实例
    /// </summary>
    public IServiceProvider? Services => _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置依赖注入容器
        _services = new ServiceCollection()
            .AddCrosshairProServices()
            .AddSingleton<CrosshairViewModel>()
            .AddSingleton<GamesViewModel>()
            .AddSingleton<ApexConfigViewModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<OverlayWindow>()
            .AddTransient<MainWindow>()
            .BuildServiceProvider();

        // 初始化语言设置
        InitializeLocalization();

        // 从容器获取主窗口并显示
        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void InitializeLocalization()
    {
        try
        {
            var stateRepo = _services?.GetService<IAppStateRepository>();
            if (stateRepo != null)
            {
                var state = stateRepo.LoadStateAsync().GetAwaiter().GetResult();
                LocalizationProvider.Instance.Initialize(state.Language);
            }
            else
            {
                LocalizationProvider.Instance.Initialize();
            }
        }
        catch
        {
            LocalizationProvider.Instance.Initialize();
        }
    }
}
