﻿
using Avalonia.Controls;
using AvaloniaApplication1.ViewModels;


namespace AvaloniaApplication1.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        this.CanResize =false; // 禁用大小调整
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // 自动居中
        // 将视图模型与视图关联
        this.DataContext = new LoginWindowViewModel(this);
    }
    
    

  
}