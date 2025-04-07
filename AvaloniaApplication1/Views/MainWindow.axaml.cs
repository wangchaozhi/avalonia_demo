using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CenterWindow();
    }
    

    private void CenterWindow()
    {
        this.Width = 350; 
        this.Height = 350; 
        this.CanResize =false; // 禁用大小调整
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // 自动居中
    }
}