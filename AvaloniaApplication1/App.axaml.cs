/*
 Author: wangchaozhi
 Date: 2026/02/10
*/
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using AvaloniaApplication1.Service;

namespace AvaloniaApplication1;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainWindowViewModel(),
            // };
            desktop.MainWindow = new LoginWindow(); // 将入口改为 LoginWindow
            // {
            //     DataContext = new LoginWindowViewModel(this),
            // };

            // Listen for "activate existing instance" requests.
            // This is invoked when user launches the app again.
            var instanceKey = SingleInstanceManager.InstanceKey;
            if (!string.IsNullOrWhiteSpace(instanceKey))
            {
                SingleInstanceManager.StartListening(instanceKey, () =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var target =
                            desktop.Windows?.FirstOrDefault(w => w.IsActive) ??
                            desktop.Windows?.FirstOrDefault(w => w.IsVisible) ??
                            desktop.MainWindow;

                        WindowActivationService.Activate(target);
                    });
                });

                desktop.Exit += (_, _) => SingleInstanceManager.Shutdown();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}