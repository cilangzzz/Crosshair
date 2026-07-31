using System.Windows;
using CrosshairPro.Application.DI;
using CrosshairPro.App.ViewModels;
using CrosshairPro.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CrosshairPro.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 配置依赖注入容器
        _services = new ServiceCollection()
            .AddCrosshairProServices()
            .AddSingleton<CrosshairViewModel>()
            .AddSingleton<GamesViewModel>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<OverlayWindow>()
            .AddTransient<MainWindow>()
            .BuildServiceProvider();

        // 从容器获取主窗口并显示
        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

