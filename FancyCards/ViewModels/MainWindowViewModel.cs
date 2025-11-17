using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;
using FancyCards.Services;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ModalService _modalService;

        public string Title => "Fancy Cards";

        [ObservableProperty]
        private ObservableCollection<Deck> _decks = [];

        [ObservableProperty]
        private ObservableCollection<Card> _cards = [];

        public IReadOnlyList<BaseModalViewModel> ActiveModals => _modalService.ActiveModals;
        public bool HasActiveModals => _modalService.ActiveModals.Any();
        public MainWindowViewModel(DataService dataService, ModalService modalService)
        {
            _modalService = modalService;

            _decks = new ObservableCollection<Deck>(dataService.Decks);
            _cards = new ObservableCollection<Card>(_decks.First().Cards ?? new List<Card>());

            // Подписываемся на изменение коллекции
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };
        }

        private IRelayCommand _openCardModal;
        public IRelayCommand OpenCardModal => _openCardModal ??= new RelayCommand(async() =>
        {
            var result = await _modalService.ShowModalAsync(new CardViewViewModel());
            if (result.Success)
            {
                _cards.Add(result.Data);
            }
        });
    }
}
