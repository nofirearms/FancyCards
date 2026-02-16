using CommunityToolkit.Mvvm.Input;

namespace FancyCards.ViewModels
{
    public partial class TrainingStartViewModel : BaseModalViewModel<object>
    {

        [RelayCommand]
        private void StartTraining() => Close(buttonTag: "StartTraining");

        [RelayCommand]
        private void CancelTraining() => Cancel();
    }
}
