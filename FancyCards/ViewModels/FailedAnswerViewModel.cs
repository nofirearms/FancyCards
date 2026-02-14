using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class FailedAnswerViewModel : BaseModalViewModel<object>
    {

        public string Answer { get; }
        public string FrontText { get; }
        public FailedAnswerViewModel(string answer, string frontText)
        {
            Answer = answer;
            FrontText = frontText;
        }

        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
