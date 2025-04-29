using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace AvaloniaApplication1.ViewModels
{
    public partial class MusicPageViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainViewModel;

        public MusicPageViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private void PlayMusic()
        {
            _mainViewModel?.AppendConsoleOutput("Playing music...");
            Log.Information("Music page: Play music clicked");
        }
    }
}