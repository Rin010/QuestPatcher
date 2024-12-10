using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using QuestPatcher.ViewModels.ModBrowser;

namespace QuestPatcher.Views.ModBrowser
{
    public partial class ExternalModView : UserControl
    {
        public ExternalModView()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ExternalModViewModel viewModel)
            {
                viewModel.ViewClicked();
            }
        }
    }
}

