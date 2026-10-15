using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// 电脑优化页：仅提供优化项说明与对应系统管理页的快捷入口，
/// 所有更改均由用户在打开的页面中手动完成，本工具不修改任何系统设置
/// </summary>
public partial class PcOptimizePage : UserControl
{
    public PcOptimizePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private PcOptimizeViewModel? _viewModel;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.Current is App app && app.Services != null)
        {
            _viewModel = app.Services.GetService(typeof(PcOptimizeViewModel)) as PcOptimizeViewModel;
            if (_viewModel != null)
            {
                DataContext = _viewModel;
                _viewModel.ToastRequested += OnToastRequested;
            }
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.ToastRequested -= OnToastRequested;
        }
    }

    private void OnToastRequested(object? sender, string message)
    {
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
