using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CrosshairPro.App.Controls;

/// <summary>
/// 对话框基类 - 提供统一的对话框样式和行为
/// 使用主题系统，避免硬编码颜色
/// </summary>
public class DialogBase : Window
{
    #region DependencyProperty

    /// <summary>对话框标题</summary>
    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(nameof(TitleText), typeof(string),
            typeof(DialogBase), new PropertyMetadata(string.Empty));

    /// <summary>是否显示关闭按钮</summary>
    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool),
            typeof(DialogBase), new PropertyMetadata(true));

    /// <summary>是否显示按钮区</summary>
    public static readonly DependencyProperty ShowButtonsProperty =
        DependencyProperty.Register(nameof(ShowButtons), typeof(bool),
            typeof(DialogBase), new PropertyMetadata(true));

    /// <summary>确认按钮文本</summary>
    public static readonly DependencyProperty ConfirmButtonTextProperty =
        DependencyProperty.Register(nameof(ConfirmButtonText), typeof(string),
            typeof(DialogBase), new PropertyMetadata("OK"));

    /// <summary>取消按钮文本</summary>
    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(nameof(CancelButtonText), typeof(string),
            typeof(DialogBase), new PropertyMetadata("Cancel"));

    /// <summary>对话框内容</summary>
    public static readonly DependencyProperty DialogContentProperty =
        DependencyProperty.Register(nameof(DialogContent), typeof(object),
            typeof(DialogBase), new PropertyMetadata(null));

    #endregion

    #region Properties

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public bool ShowButtons
    {
        get => (bool)GetValue(ShowButtonsProperty);
        set => SetValue(ShowButtonsProperty, value);
    }

    public string ConfirmButtonText
    {
        get => (string)GetValue(ConfirmButtonTextProperty);
        set => SetValue(ConfirmButtonTextProperty, value);
    }

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public object DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }

    #endregion

    #region Events

    /// <summary>确认按钮点击事件</summary>
    public event RoutedEventHandler? Confirmed;

    /// <summary>取消按钮点击事件</summary>
    public event RoutedEventHandler? Cancelled;

    #endregion

    static DialogBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBase),
            new FrameworkPropertyMetadata(typeof(DialogBase)));
    }

    public DialogBase()
    {
        // 默认窗口设置
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ShowInTaskbar = false;

        // 默认大小
        Width = 360;
        Height = 185;

        // 拖动支持
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取模板部件并绑定事件
        var closeButton = GetTemplateChild("CloseButton") as Button;
        var confirmButton = GetTemplateChild("ConfirmButton") as Button;
        var cancelButton = GetTemplateChild("CancelButton") as Button;
        var buttonPanel = GetTemplateChild("ButtonPanel") as StackPanel;

        if (closeButton != null)
        {
            closeButton.Click += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogResult = false;
                    Close();
                });
            };
        }

        if (confirmButton != null)
        {
            confirmButton.Click += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                    Confirmed?.Invoke(this, e);
                    Close();
                });
            };
        }

        if (cancelButton != null)
        {
            cancelButton.Click += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogResult = false;
                    Cancelled?.Invoke(this, e);
                    Close();
                });
            };
        }

        if (buttonPanel != null && !ShowButtons)
        {
            buttonPanel.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 显示对话框并返回结果
    /// </summary>
    public bool? ShowDialog(Window owner)
    {
        Owner = owner;
        return ShowDialog();
    }

    /// <summary>
    /// 创建简单输入对话框
    /// </summary>
    public static DialogBase CreateInputDialog(string title, string prompt, string defaultValue = "")
    {
        var dialog = new DialogBase
        {
            TitleText = title,
            Width = 360,
            Height = 185
        };

        // 创建内容面板
        var panel = new StackPanel { Margin = new Thickness(16, 0, 16, 0) };

        // 提示文本
        panel.Children.Add(new TextBlock
        {
            Text = prompt,
            Foreground = Helpers.ThemeHelper.TextSecondaryBrush,
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 6)
        });

        // 输入框
        var textBox = new TextBox
        {
            Text = defaultValue,
            Background = Helpers.ThemeHelper.ControlBrush,
            Foreground = Helpers.ThemeHelper.TextPrimaryBrush,
            CaretBrush = Helpers.ThemeHelper.AccentBrush,
            BorderBrush = Helpers.ThemeHelper.BorderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(8, 5, 0, 0),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 12)
        };

        // Enter/Escape 键处理
        textBox.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        };

        panel.Children.Add(textBox);
        dialog.DialogContent = panel;

        // 存储输入框引用以便获取结果
        dialog.Tag = textBox;

        return dialog;
    }

    /// <summary>
    /// 获取输入对话框的结果文本
    /// </summary>
    public string? GetInputResult()
    {
        if (Tag is TextBox textBox)
        {
            return textBox.Text;
        }
        return null;
    }
}