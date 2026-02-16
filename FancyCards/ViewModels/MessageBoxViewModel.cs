using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;


namespace FancyCards.ViewModels
{
    public partial class MessageBoxViewModel : BaseModalViewModel<object>
    {
        public string Header { get; }
        public string Message { get; }
        public string[] Buttons { get; }

        public MessageBoxViewModel(string header, string message, string[] buttons)
        {
            Header = header;
            Message = message;
            Buttons = buttons;

            Background = new SolidColorBrush(Colors.PaleGreen);
        }

        [RelayCommand]
        private void ButtonClick(string button)
        {
            Close(true, buttonTag:button);
        }
    }
}
