/*
 Author: wangchaozhi
 Date: 2026/02/10
*/
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace AvaloniaApplication1.ViewModels
{
    public partial class TabViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        public string Key { get; }

        [ObservableProperty]
        private string _header = string.Empty;

        [ObservableProperty]
        private object? _content;

        [ObservableProperty]
        private Window? _detachedWindow;

        public TabViewModel(MainWindowViewModel mainViewModel, string key)
        {
            _mainViewModel = mainViewModel;
            Key = key;
        }
        

        [RelayCommand]
        private void CloseTab()
        {
            _mainViewModel.CloseTab(this);
        }
    }
}