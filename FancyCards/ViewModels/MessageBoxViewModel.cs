using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows.Input;
using System.Windows.Media;


namespace FancyCards.ViewModels
{
    public partial class MessageBoxViewModel : BaseModalViewModel<object>
    {
        public string Message { get; }
        public string[] Buttons { get; }

        public MessageBoxViewModel(HotkeyService hotkeyService, MessageBoxParameters parameters)
        {
            Header = parameters.Header;
            Message = parameters.Message;
            Buttons = parameters.Buttons;
            Background = parameters.Background is null ? new SolidColorBrush(Colors.PaleGreen) : parameters.Background;

            hotkeyService.RegisterHotkey<MessageBoxViewModel>(Key.Enter, ModifierKeys.None, ButtonClickCommand, parameters.Buttons[0]);          
        }

        [RelayCommand]
        private void ButtonClick(string button)
        {
            Close(true, buttonTag:button);
        }
    }
}
