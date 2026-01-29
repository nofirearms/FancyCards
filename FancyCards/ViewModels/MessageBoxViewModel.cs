using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        [RelayCommand]
        private void ButtonClick(string button)
        {
            Close(true, buttonTag:button);
        }
    }
}
