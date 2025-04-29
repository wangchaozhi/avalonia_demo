
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaApplication1.ViewModels;


namespace AvaloniaApplication1.Views;

public partial class LoginWindow : BaseWindow
{
    public LoginWindow()
    {
        InitializeComponent();
        this.CanResize =false; // 禁用大小调整
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // 自动居中
        // 将视图模型与视图关联
        this.DataContext = new LoginWindowViewModel(this);
    }
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            this.BeginMoveDrag(e);
        }
    }
    
    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
    
    

  
}