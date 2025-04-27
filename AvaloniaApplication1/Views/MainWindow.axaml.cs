using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Serilog;
using System;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaApplication1.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : Window
    {
        private TrayIcon? _trayIcon;
        private NativeMenu _trayMenu = new NativeMenu();
    

        public MainWindow()
        {
            InitializeComponent();
            CenterWindow();
            SetupSystemTray();
        }

        private void CenterWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen; // Center window
        }

        private void SetupSystemTray()
        {
            try
            {
                // Determine resource URI based on platform
                string resourceUri = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "avares://AvaloniaApplication1/Assets/avalonia-logo.png" // PNG for macOS/Linux
                    : "avares://AvaloniaApplication1/Assets/avalonia-logo.ico"; // ICO for Windows

                // Load resource stream
                using var stream = AssetLoader.Open(new Uri(resourceUri));
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(stream),
                    ToolTipText = "AvaloniaApplication1"
                };

                // Create context menu
                _trayMenu.Add(new NativeMenuItem
                {
                    Header = "显示",
                    Command = new AvaloniaCommand(() => ShowWindow())
                });
                _trayMenu.Add(new NativeMenuItem
                {
                    Header = "隐藏",
                    Command = new AvaloniaCommand(() => HideWindow())
                });
                _trayMenu.Add(new NativeMenuItem
                {
                    Header = "退出",
                    Command = new AvaloniaCommand(() => ExitApplication())
                });

                _trayIcon.Menu = _trayMenu;
                _trayIcon.Clicked += (s, e) =>
                {
                    if (this.IsVisible)
                        HideWindow();
                    else
                        ShowWindow();
                };

                Log.Information("System tray initialized with resource: {ResourceUri}", resourceUri);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize system tray");
            }

            // Prevent window close from exiting the app
            this.Closing += (s, e) =>
            {
                e.Cancel = true; // Cancel close
                HideWindow();
            };
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            Log.Information("MainWindow shown via system tray");
        }

        private void HideWindow()
        {
            this.Hide();
            Log.Information("MainWindow hidden to system tray");
        }

        private void ExitApplication()
        {
            try
            {
                // Dispose tray icon and close the application
                _trayIcon?.Dispose();
                _trayIcon = null;
                this.Closing -= (s, e) => e.Cancel = true; // Allow close
                this.Close();
                (Application.Current?.TryGetFeature(typeof(IClassicDesktopStyleApplicationLifetime)) as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
                Log.Information("Application exited via system tray");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exiting application");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Ensure tray icon is disposed if window is closed unexpectedly
            _trayIcon?.Dispose();
            _trayIcon = null;
        }

        private class AvaloniaCommand : System.Windows.Input.ICommand
        {
            private readonly Action _execute;

            public AvaloniaCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged; // Required by ICommand

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter) => _execute();
        }
    }
}