using System.ComponentModel;
using System.Windows.Input;
using System;
using System.IO;
using AvaloniaApplication1.Views;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using AvaloniaApplication1.Service;

namespace AvaloniaApplication1.ViewModels
{
    public class LoginWindowViewModel : INotifyPropertyChanged
    {
        private readonly LoginWindow _loginWindow;
        private readonly DatabaseManager _databaseManager;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberPassword;
        // Hardcoded AES key and IV (for demo purposes; use secure key management in production)
        private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("Your16ByteSecretKey1234567890!@#"); // 32 bytes for AES-256
        private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("Your16ByteIV1234"); // 16 bytes for AES

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? LoginSuccess;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
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
                (LoginCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public bool RememberPassword
        {
            get => _rememberPassword;
            set
            {
                _rememberPassword = value;
                OnPropertyChanged(nameof(RememberPassword));
                if (!value)
                {
                    ClearSavedCredentials(); // Clear credentials if unchecked
                }
            }
        }

        public ICommand LoginCommand { get; }

        public LoginWindowViewModel(LoginWindow loginWindow)
        {
            _loginWindow = loginWindow;
            _databaseManager = new DatabaseManager();
            LoginCommand = new RelayCommand(Login, CanLogin);
            LoadSavedCredentials(); // Load saved credentials on startup
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private void Login()
        {
            try
            {
                bool isAuthenticated = _databaseManager.AuthenticateUser(Username, Password);

                if (isAuthenticated)
                {
                    _databaseManager.LogLoginActivity(Username);
                    Log.Information("User {Username} logged in successfully", Username);

                    // Save credentials if RememberPassword is checked
                    if (RememberPassword)
                    {
                        SaveCredentials();
                    }
                    else
                    {
                        ClearSavedCredentials(); // Clear credentials if not remembering
                    }

                    var mainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel()
                    };
                    mainWindow.Show();
                    _loginWindow.Close();
                }
                else
                {
                    var errorWindow = new Views.ErrorWindow("Login failed: Invalid credentials");
                    errorWindow.ShowDialog(_loginWindow);
                    Log.Warning("Login failed for {Username}: Invalid credentials", Username);
                }
            }
            catch (Exception ex)
            {
                var errorWindow = new Views.ErrorWindow($"Error: {ex.Message}");
                errorWindow.ShowDialog(_loginWindow);
                Log.Error(ex, "Login error for {Username}", Username);
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
                var credentials = _databaseManager.LoadRememberedCredentials();
                if (credentials.HasValue)
                {
                    Username = credentials.Value.Username;
                    if (!string.IsNullOrEmpty(credentials.Value.EncryptedPassword))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(credentials.Value.EncryptedPassword);
                        byte[] decryptedBytes = Decrypt(encryptedBytes, AesKey, AesIV);
                        Password = Encoding.UTF8.GetString(decryptedBytes);
                    }
                    RememberPassword = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load saved credentials");
            }
        }

        private void SaveCredentials()
        {
            try
            {
                string encryptedPassword = null;
                if (!string.IsNullOrEmpty(Password))
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
                    byte[] encryptedBytes = Encrypt(passwordBytes, AesKey, AesIV);
                    encryptedPassword = Convert.ToBase64String(encryptedBytes);
                }
                _databaseManager.SaveRememberedCredentials(Username, encryptedPassword);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save credentials for {Username}", Username);
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                _databaseManager.ClearRememberedCredentials();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clear saved credentials");
            }
        }

        private static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        private static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}