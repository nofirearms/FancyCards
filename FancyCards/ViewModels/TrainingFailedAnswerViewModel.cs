using CommunityToolkit.Mvvm.Input;
using System.Windows.Media;

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

            Header = "Incorrect";

            Background = new SolidColorBrush(Colors.LightPink);
        }

        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
