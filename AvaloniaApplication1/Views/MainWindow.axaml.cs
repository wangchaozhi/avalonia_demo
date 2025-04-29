using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Serilog;
using System;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : BaseWindow
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
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void SetupSystemTray()
        {
            try
            {
                string resourceUri = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "avares://AvaloniaApplication1/Assets/avalonia-logo.png"
                    : "avares://AvaloniaApplication1/Assets/avalonia-logo.ico";

                using var stream = AssetLoader.Open(new Uri(resourceUri));
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(stream),
                    ToolTipText = "AvaloniaApplication1"
                };

                _trayMenu.Add(new NativeMenuItem { Header = "显示", Command = new AvaloniaCommand(() => ShowWindow()) });
                _trayMenu.Add(new NativeMenuItem { Header = "隐藏", Command = new AvaloniaCommand(() => HideWindow()) });
                _trayMenu.Add(new NativeMenuItem { Header = "退出", Command = new AvaloniaCommand(() => ExitApplication()) });

                _trayIcon.Menu = _trayMenu;
                _trayIcon.Clicked += TrayIcon_Clicked;

                Log.Information("System tray initialized on {OS} with resource: {ResourceUri}", RuntimeInformation.OSDescription, resourceUri);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize system tray on {OS}", RuntimeInformation.OSDescription);
            }

            this.Closing += Window_Closing;
        }

        private void TrayIcon_Clicked(object? sender, EventArgs e)
        {
            if (this.IsVisible)
                HideWindow();
            else
                ShowWindow();
        }

        private void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            HideWindow();
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
                if (_trayIcon != null)
                {
                    _trayIcon.Clicked -= TrayIcon_Clicked;
                    _trayIcon.Menu = null;
                    _trayIcon.Dispose();
                    _trayIcon = null;
                }

                this.Closing -= Window_Closing;
                this.Close();

                if (Application.Current?.TryGetFeature(typeof(IClassicDesktopStyleApplicationLifetime)) is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.Shutdown();
                }

                Log.Information("Application exited via system tray");
                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error exiting application");
                Log.CloseAndFlush();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try
            {
                if (_trayIcon != null)
                {
                    _trayIcon.Clicked -= TrayIcon_Clicked;
                    _trayIcon.Menu = null;
                    _trayIcon.Dispose();
                    _trayIcon = null;
                }

                Log.CloseAndFlush();

                if (Application.Current?.TryGetFeature(typeof(IClassicDesktopStyleApplicationLifetime)) is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during window close");
            }
        }

        private class AvaloniaCommand : System.Windows.Input.ICommand
        {
            private readonly Action _execute;

            public AvaloniaCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter) => _execute();
        }
    }
}