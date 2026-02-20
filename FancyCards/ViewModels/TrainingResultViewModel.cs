using CommunityToolkit.Mvvm.Input;

namespace FancyCards.ViewModels
{
    public partial class TrainingResultViewModel : BaseModalViewModel<object>
    {


        public TrainingResultViewModel()
        {
            Header = "Result";
        }


        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
