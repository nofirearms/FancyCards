using CommunityToolkit.Mvvm.Input;

namespace FancyCards.ViewModels
{
    public partial class TrainingResultViewModel : BaseModalViewModel<object>
    {

        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public TimeSpan TimeSpent { get; set; }

        public TrainingResultViewModel(IEnumerable<TrainingCardViewModel> cards)
        {
            Header = "Result";

            SuccessCount = cards.Where(c => c.CardStatus == Models.TrainingCardState.Success).Count();
            FailedCount = cards.Where(c => c.CardStatus == Models.TrainingCardState.Failed).Count();
            TimeSpent = TimeSpan.FromSeconds( cards.Select(c => c.SessionTimeSpent.TotalSeconds).Sum());
        }


        [RelayCommand]
        private void Ok() => Close(buttonTag: "Ok");
    }
}
