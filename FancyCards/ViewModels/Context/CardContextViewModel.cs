using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;


namespace FancyCards.ViewModels
{
    public partial class CardContextViewModel : BaseModalViewModel<Card>
    {
        private readonly Card _card;

        public CardContextViewModel(Card card)
        {
            _card = card;
        }

        [RelayCommand]
        private void Edit() => Close(true, _card, "Edit");
        [RelayCommand]
        private void Remove() => Close(true, _card, "Remove");
    }
}
