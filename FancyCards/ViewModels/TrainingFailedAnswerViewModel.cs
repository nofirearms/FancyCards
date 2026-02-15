using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;

namespace FancyCards.ViewModels
{
    public partial class TrainingFailedAnswerViewModel : BaseModalViewModel<object>
    {

        public string Answer { get; }
        public string FrontText { get; }
        public TrainingFailedAnswerViewModel(string answer, string frontText)
        {
            Answer = answer;
            FrontText = frontText;
        }

        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
