using System.IO;
using System.Text.Json;
using System.Windows;
using CrosshairPro.Application.DI;
using CrosshairPro.App.Localization;
using CrosshairPro.App.ViewModels;
using CrosshairPro.App.Views;
using CrosshairPro.Core;
using CrosshairPro.Core.Interfaces;
using CrosshairPro.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CrosshairPro.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>单实例互斥体名称（Local 命名空间：同一登录会话内唯一）</summary>
    private const string SingleInstanceMutexName = @"Local\CrosshairPro.SingleInstance";

    private Mutex? _instanceMutex;

    private IServiceProvider? _services;

    /// <summary>
    /// 服务提供者，用于获取服务实例
    /// </summary>
    public IServiceProvider? Services => _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 单实例校验：重复启动时提示并退出，避免出现多个准心叠加层（以及全局热键注册冲突）
        bool createdNew;
        try
        {
            _instanceMutex = new Mutex(true, SingleInstanceMutexName, out createdNew);
        }
        catch (UnauthorizedAccessException)
        {
            // 同名互斥体已存在但当前用户无权打开（如同会话其他账户/管理员实例先启动、或名称被其他程序占用），
            // 无法安全成为单实例，按"已在运行"处理优雅退出
            createdNew = false;
        }

        if (!createdNew)
        {
            ExitAsAlreadyRunning();
            return;
        }

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
                .AddSingleton<Cs2ConfigViewModel>()
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

    /// <summary>
    /// 按"已在运行"提示后退出（重复启动与互斥体不可访问共用的优雅退出路径）
    /// </summary>
    private void ExitAsAlreadyRunning()
    {
        // 此时语言尚未初始化，先按已保存的语言偏好恢复，再弹出提示
        var savedLanguage = ReadSavedLanguage();
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            LocalizationProvider.Instance.Initialize(savedLanguage);
        }

        System.Windows.MessageBox.Show(
            LocalizationProvider.Instance["App.AlreadyRunning"],
            LocalizationProvider.Instance["App.Title"],
            MessageBoxButton.OK, MessageBoxImage.Information);

        // 释放未获得所有权的句柄（构造抛异常时为 null，空条件安全）
        _instanceMutex?.Dispose();
        _instanceMutex = null;

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 主动释放单实例互斥体（进程异常退出时系统会自动回收，不会残留锁）
        try
        {
            _instanceMutex?.ReleaseMutex();
        }
        catch
        {
            // 未持有所有权时忽略（重复启动路径已提前置空）
        }
        _instanceMutex?.Dispose();
        _instanceMutex = null;

        base.OnExit(e);
    }

    /// <summary>
    /// 同步读取已保存的语言偏好（单实例提示发生在语言初始化之前；
    /// 不能走 LoadStateAsync 的 sync-over-async，存在 UI 线程死锁风险）
    /// </summary>
    private static string? ReadSavedLanguage()
    {
        try
        {
            // 路径来自 Core.CrosshairProPaths（与 JsonConfigRepository 同源，避免存储契约漂移）
            var stateFile = CrosshairProPaths.AppStateFile;
            if (!File.Exists(stateFile)) return null;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var state = JsonSerializer.Deserialize<AppPersistedState>(File.ReadAllText(stateFile), options);
            return string.IsNullOrEmpty(state?.Language) ? null : state.Language;
        }
        catch
        {
            // 读取失败回退默认语言（zh-CN）
            return null;
        }
    }
}
