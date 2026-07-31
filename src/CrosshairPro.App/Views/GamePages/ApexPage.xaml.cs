using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// Apex Legends 配置页面
/// </summary>
public partial class ApexPage : UserControl
{
    public ApexPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 从 App 获取服务
        if (App.Current is App app && app.Services != null)
        {
            var vm = app.Services.GetService(typeof(ApexConfigViewModel)) as ApexConfigViewModel;
            if (vm != null)
            {
                DataContext = vm;

                // 订阅 Toast 事件
                vm.ToastRequested += OnToastRequested;
            }
        }
    }

    private void OnToastRequested(object? sender, string message)
    {
        // 在当前窗口中显示 Toast
        // 查找合适的 Panel 容器
        if (Parent is Panel panel)
        {
            ToastNotification.ShowIn(panel, message);
        }
        else if (Window.GetWindow(this)?.Content is Panel windowPanel)
        {
            ToastNotification.ShowIn(windowPanel, message);
        }
    }
}