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
using System.Windows.Controls;

namespace FancyCards.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ModalService _modalService;
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;
        private readonly ViewModelFactory _viewModelFactory;

        public string Title => "Fancy Cards";

        [ObservableProperty]
        private ObservableCollection<Deck> _decks = [];

        public CardListViewModel CardListViewModel { get; set; }

        public IReadOnlyList<BaseModalViewModel> ActiveModals => _modalService.ActiveModals;
        public bool HasActiveModals => _modalService.ActiveModals.Any();


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContextMenuOpen))]
        private BaseModalViewModel _contextMenu;

        [ObservableProperty]
        private bool _loading = false;

        public bool ContextMenuOpen => _contextMenu != null;

        public MainWindowViewModel(ViewModelFactory viewModelFactory, DataService dataService, ModalService modalService, AudioEngine audioEngine)
        {
            _modalService = modalService;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _viewModelFactory = viewModelFactory;

            

            _decks = new ObservableCollection<Deck>(dataService.Decks);

            CardListViewModel = _viewModelFactory.Create<CardListViewModel>(this);


            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };

        }

        
        public async Task<ModalResult<T>> OpenContext<T>(BaseModalViewModel<T> context)
        {
            try
            {
                ContextMenu = context;

                var result = await context.OpenAsync();

                ContextMenu = null;

                return result;
            }
            finally
            {
                ////unfreeze ui
                //await Task.Delay(15);
            }

        }
        [RelayCommand]
        private void CreateCard()
        {
            OpenCardModal(null);
        }

        public async Task<ModalResult<Card>> OpenCardModal(Card card)
        {
            await ModalLoading();
            try
            {
                var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<CardDetailViewModel>(card ?? new Card()));

                return result;
            }
            finally
            {

            }

        }

        public async Task<ModalResult<object>> OpenMessageBox(string message, string[] buttons, string header = "Attention!")
        {
            var result = await _modalService.ShowModalAsync(new MessageBoxViewModel(header, message, buttons));
            return result;
        }


        [RelayCommand]
        private async Task ModalLoading()
        {
            Loading = true;
            await Task.Delay(30);
        }
        [RelayCommand]
        private void ModalLoaded()
        {
            Loading = false;
        }
    }
}
