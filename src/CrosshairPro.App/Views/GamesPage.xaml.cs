using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views;

/// <summary>
/// 游戏配置页面
/// </summary>
public partial class GamesPage : UserControl
{
    private GamesViewModel? _viewModel;

    public GamesPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 从 Window 的 DataContext 获取 MainViewModel，然后获取 GamesViewModel
        var window = Window.GetWindow(this);
        if (window?.DataContext is MainViewModel mainVm)
        {
            SetViewModel(mainVm.GamesViewModel);
        }
    }

    /// <summary>
    /// 设置 ViewModel
    /// </summary>
    public void SetViewModel(GamesViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = viewModel;
    }
}
