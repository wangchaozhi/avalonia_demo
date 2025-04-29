using Avalonia.Controls;
using Avalonia.Platform;
using Serilog;
using System;
using System.Runtime.InteropServices;

namespace AvaloniaApplication1.Views
{
    public abstract class BaseWindow : Window
    {
        protected BaseWindow()
        {
            SetWindowIcon();
            // this.Closing += Window_Closing;
        }

        protected virtual void SetWindowIcon()
        {
            try
            {
                string resourceUri = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "avares://AvaloniaApplication1/Assets/avalonia-logo.png"
                    : "avares://AvaloniaApplication1/Assets/avalonia-logo.ico";

                using var stream = AssetLoader.Open(new Uri(resourceUri));
                this.Icon = new WindowIcon(stream);
                Log.Information("{Window} icon set to: {ResourceUri}", GetType().Name, resourceUri);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to set icon for {Window}", GetType().Name);
            }
        }

        // protected virtual void Window_Closing(object? sender, WindowClosingEventArgs e)
        // {
        //     e.Cancel = true;
        //     this.Hide();
        //     Log.Information("{Window} hidden to system tray", GetType().Name);
        // }
    }
}