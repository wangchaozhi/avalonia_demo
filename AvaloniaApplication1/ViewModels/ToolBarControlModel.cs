using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels
{
    public partial class ToolBarControlModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        public ToolBarControlModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private void New()
        {
            _mainViewModel?.AppendConsoleOutput("New item created.");
        }

        [RelayCommand]
        private void Music()
        {
            _mainViewModel?.AddMusicTab();
        }
    }
}