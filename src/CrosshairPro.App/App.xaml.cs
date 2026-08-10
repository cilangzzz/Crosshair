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

        // 全局异常处理
        DispatcherUnhandledException += (s, args) =>
        {
            System.Windows.MessageBox.Show($"Unhandled exception: {args.Exception}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
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

            // 初始化语言设置（使用默认，等待主窗口加载）
            LocalizationProvider.Instance.Initialize();

            // 从容器获取主窗口并显示
            var mainWindow = _services.GetRequiredService<MainWindow>();

            // 启动后异步加载语言设置
            mainWindow.Loaded += async (s, args) =>
            {
                try
                {
                    var stateRepo = _services?.GetService<IAppStateRepository>();
                    if (stateRepo != null)
                    {
                        var state = await stateRepo.LoadStateAsync();
                        if (!string.IsNullOrEmpty(state.Language))
                        {
                            LocalizationProvider.Instance.Initialize(state.Language);
                        }
                    }
                }
                catch { }
            };

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Startup error: {ex}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
