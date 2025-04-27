using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Serilog;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _greeting = "Welcome to Avalonia123";

        [ObservableProperty]
        private string _consoleOutput = "Console initialized.\n";

        public MainWindowViewModel()
        {
            AppendConsoleOutput("Application started.");
        }

        [RelayCommand]
        private void New()
        {
            AppendConsoleOutput("New item created.");
            // TODO: Implement new item logic
        }

        [RelayCommand]
        private void Open()
        {
            AppendConsoleOutput("Opening item.");
            // TODO: Implement open logic
        }

        [RelayCommand]
        private void Save()
        {
            AppendConsoleOutput("Saving item.");
            // TODO: Implement save logic
        }

        [RelayCommand]
        private void Exit()
        {
            AppendConsoleOutput("Exiting application.");
            // Trigger system tray exit (handled in MainWindow.cs)
        }

        [RelayCommand]
        private void Cut()
        {
            AppendConsoleOutput("Cut operation.");
            // TODO: Implement cut logic
        }

        [RelayCommand]
        private void Copy()
        {
            AppendConsoleOutput("Copy operation.");
            // TODO: Implement copy logic
        }

        [RelayCommand]
        private void Paste()
        {
            AppendConsoleOutput("Paste operation.");
            // TODO: Implement paste logic
        }

        [RelayCommand]
        private void About()
        {
            AppendConsoleOutput("About dialog opened.");
            // TODO: Implement about dialog
        }

        internal void AppendConsoleOutput(string message)
        {
            ConsoleOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            Log.Information("Console: {Message}", message);
        }
    }
}