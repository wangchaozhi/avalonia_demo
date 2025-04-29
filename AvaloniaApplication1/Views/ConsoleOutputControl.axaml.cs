using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.ViewModels;
using System.ComponentModel;
using System.Linq;
using Avalonia.VisualTree;
using Avalonia;

namespace AvaloniaApplication1.Views
{
    public partial class ConsoleOutputControl : UserControl
    {
        private readonly TextBox _consoleTextBox;
        private ScrollViewer? _scrollViewer;

        public ConsoleOutputControl()
        {
            InitializeComponent();
            _consoleTextBox = this.FindControl<TextBox>("ConsoleTextBox")!;
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            // Find ScrollViewer after layout is complete
            this.LayoutUpdated += (s, e) => FindScrollViewer();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void FindScrollViewer()
        {
            if (_scrollViewer == null)
            {
                _scrollViewer = _consoleTextBox.GetVisualDescendants()
                    .OfType<ScrollViewer>()
                    .FirstOrDefault();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.ConsoleOutput) && _scrollViewer != null)
            {
                // Scroll to the bottom
                _scrollViewer.Offset = new Vector(0, _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height);
            }
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MainWindowViewModel newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
    }
}