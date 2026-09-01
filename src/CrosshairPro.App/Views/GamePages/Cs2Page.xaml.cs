using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views.GamePages;

/// <summary>
/// Counter-Strike 2 配置页面
/// </summary>
public partial class Cs2Page : UserControl
{
    public Cs2Page()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private Cs2ConfigViewModel? _viewModel;

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (App.Current is App app && app.Services != null)
        {
            _viewModel = app.Services.GetService(typeof(Cs2ConfigViewModel)) as Cs2ConfigViewModel;
            if (_viewModel != null)
            {
                DataContext = _viewModel;
                _viewModel.ToastRequested += OnToastRequested;
                await _viewModel.LoadAsync();
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