using System.ComponentModel;
using System.Windows.Input;
using System;
using AvaloniaApplication1.Views;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels
{
    public class LoginWindowViewModel : INotifyPropertyChanged
    {
        private readonly LoginWindow _loginWindow;
        private string _username = string.Empty;
        private string _password = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        // 登录成功的事件
        public event Action? LoginSuccess;


        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                // 每次更新用户名或密码时，都会触发 CanLogin 重新评估
                (LoginCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                
                // 每次更新用户名或密码时，都会触发 CanLogin 重新评估
                (LoginCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public ICommand LoginCommand { get; }

        public LoginWindowViewModel(LoginWindow loginWindow)
        {
            _loginWindow = loginWindow;
            LoginCommand = new RelayCommand(Login, CanLogin);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private void Login()
        {
            if (Username == "admin" && Password == "123456")
            {
                var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                mainWindow.Show();
                _loginWindow.Close(); // 登录成功后关闭登录窗口
            }
            else
            {
                // 登录失败，显示错误提示窗口
                var errorWindow = new Views.ErrorWindow("Login failed: Invalid credentials");
                errorWindow.ShowDialog(_loginWindow); // 传递父窗口
                Console.WriteLine("Login failed!");
                // 这里可以添加失败提示，比如弹窗
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}