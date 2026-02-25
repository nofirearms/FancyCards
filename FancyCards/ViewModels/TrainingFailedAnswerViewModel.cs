using CommunityToolkit.Mvvm.Input;
using FancyCards.Models.Param;
using FancyCards.Services;
using System.Windows.Input;
using System.Windows.Media;

namespace FancyCards.ViewModels
{
    public partial class TrainingFailedAnswerViewModel : BaseModalViewModel<object>
    {

        public string Answer { get; }
        public string FrontText { get; }
        
        public TrainingFailedAnswerViewModel(HotkeyService hotkeyService, TrainingFailedAnswerParameters parameters)
        {
            Answer = parameters.Answer;
            FrontText = parameters.FrontText;
            Header = "Incorrect";
            Background = new SolidColorBrush(Colors.LightPink);

            hotkeyService.RegisterHotkey<TrainingFailedAnswerViewModel>(Key.Enter, ModifierKeys.None, OkCommand);
        }

        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
