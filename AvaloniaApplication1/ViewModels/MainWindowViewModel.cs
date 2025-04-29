using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaloniaApplication1.Views;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _consoleOutput = "Console initialized.\n";

        [ObservableProperty]
        private ObservableCollection<TabViewModel> _tabs = new ObservableCollection<TabViewModel>();

        [ObservableProperty]
        private int _selectedTabIndex = 0;

        private readonly ToolBarControlModel _toolBarControlModel;

        public MainWindowViewModel()
        {
            _toolBarControlModel = new ToolBarControlModel(this);
            AddDefaultTab(); // Initialize with a default tab
        }

        public ToolBarControlModel ToolBarControlModel => _toolBarControlModel;

        internal void AppendConsoleOutput(string message)
        {
            ConsoleOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            Log.Information("Console: {Message}", message);
        }

        public void AddMusicTab()
        {
            var musicViewModel = new MusicPageViewModel(this);
            var musicControl = new MusicPageControl { DataContext = musicViewModel };
            var tab = new TabViewModel(this) // Pass 'this' to constructor
            {
                Header = $"Music Tab {Tabs.Count + 1}",
                Content = musicControl
            };
            Tabs.Add(tab);
            SelectedTabIndex = Tabs.Count - 1;
            AppendConsoleOutput($"Opened new music tab: {tab.Header}");
        }

        private void AddDefaultTab()
        {
            var defaultControl = new UserControl
            {
                Content = new TextBlock { Text = "Welcome to the application!", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center }
            };
            var tab = new TabViewModel(this) // Pass 'this' to constructor
            {
                Header = "Welcome",
                Content = defaultControl
            };
            Tabs.Add(tab);
            AppendConsoleOutput("Default tab opened.");
        }
    }
}