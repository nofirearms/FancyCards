using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using FancyCards.ViewModels.Modal;
using NAudio.Wave.Compression;
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
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;

        public string Title => "Fancy Cards";

        [ObservableProperty]
        private ObservableCollection<Deck> _decks = [];

        public CardListViewModel CardListViewModel { get; set; }

        public IReadOnlyList<BaseModalViewModel> ActiveModals => _modalService.ActiveModals;
        public bool HasActiveModals => _modalService.ActiveModals.Any();





        public MainWindowViewModel(DataService dataService, ModalService modalService, AudioEngine audioEngine)
        {
            _modalService = modalService;
            _dataService = dataService;
            _audioEngine = audioEngine;

            _decks = new ObservableCollection<Deck>(dataService.Decks);

            CardListViewModel = new CardListViewModel(dataService);

            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };


        }


        private IRelayCommand _openCardModal;
        public IRelayCommand OpenCardModal => _openCardModal ??= new RelayCommand(async() =>
        {
            var result = await _modalService.ShowModalAsync(new CardDetailViewModel(_audioEngine));
            if (result.Success)
            {
                await _dataService.CreateCardAsync(1, result.Data);
            }
        });
    }
}
