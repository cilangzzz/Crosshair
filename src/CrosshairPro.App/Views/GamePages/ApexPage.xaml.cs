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

    // 父子联动之值联动：父开关经 UI 点击关闭时，子开关值同步置为关闭。
    // 用 Click 而非 Checked/Unchecked：仅用户点击时触发，加载配置/应用预设经绑定更新 IsChecked 不会误触发级联
    private void OnEnableShadowsClick(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { IsChecked: false } &&
            DataContext is ApexConfigViewModel { VideoConfig: { } video })
        {
            video.CsmEnabled = 0;
        }
    }

    private void OnVolumetricLightingClick(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { IsChecked: false } &&
            DataContext is ApexConfigViewModel { VideoConfig: { } video })
        {
            video.VolumetricFog = 0;
        }
    }

    // 启动项字符串失焦联动：文本模式手改后回填面板，偏离所选预设时回显切「自定义」（VM 内判断，见 NotifyLaunchOptionsEdited）
    private void OnLaunchOptionsBoxLostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is ApexConfigViewModel viewModel)
        {
            viewModel.NotifyLaunchOptionsEdited();
        }
    }
}