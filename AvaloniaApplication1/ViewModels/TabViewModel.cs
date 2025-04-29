using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels
{
    public partial class TabViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        [ObservableProperty]
        private string _header;

        [ObservableProperty]
        private object _content;

        public TabViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
        

        [RelayCommand]
        private void CloseTab()
        {
            _mainViewModel?.Tabs.Remove(this);
            _mainViewModel?.AppendConsoleOutput($"Closed tab: {Header}");
        }
    }
}