using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaApplication1.Views
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        public ErrorWindow(string message)
            : this()
        {
            this.FindControl<TextBlock>("ErrorMessage")!.Text = message; // 设置错误信息
            // this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // 自动居中
        }

        private void OnOkButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}