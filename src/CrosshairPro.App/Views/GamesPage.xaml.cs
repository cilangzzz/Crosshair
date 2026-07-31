using System.Windows;
using System.Windows.Controls;
using CrosshairPro.App.ViewModels;

namespace CrosshairPro.App.Views;

/// <summary>
/// 游戏配置页面
/// </summary>
public partial class GamesPage : UserControl
{
    public GamesPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 设置 ViewModel
    /// </summary>
    public void SetViewModel(GamesViewModel viewModel)
    {
        DataContext = viewModel;
    }
}