using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Controls.Primitives;

namespace FancyCards.ViewModels
{
    public partial class CardListViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private readonly MainWindowViewModel _host;

        [ObservableProperty]
        private ReadOnlyObservableCollection<Card> _cards;
        
        private SourceCache<Card, int> _sourceCache;

        //[ObservableProperty]
        //private int _learnCount;
        //[ObservableProperty]
        //private int _reviewCount;

        public IEnumerable<CardState> States => new List<CardState>
        {
            CardState.Scheduled,
            CardState.Learning,
            CardState.Reviewing,
            CardState.Mastered
        };

        [ObservableProperty]
        private CardState _selectedState = CardState.Reviewing;
        partial void OnSelectedStateChanged(CardState value)
        {
            UpdateFilter();
        }

        public CardListViewModel(MainWindowViewModel host, DataService dataService, int deckId)
        {
            _dataService = dataService;
            _host = host;

            
            _dataService.CardEvent += OnCardEvent;

            InitializeAsync(deckId);
        }

        private async void InitializeAsync(int deckId)
        {

            //_cards = new ObservableCollection<Card>(_decks.First().Cards ?? new List<Card>());
            _sourceCache = new SourceCache<Card, int>(o => o.Id);

            var cards = await _dataService.GetCardsAsync(deckId);
            _sourceCache.AddOrUpdate(cards ?? new List<Card>());

            _sourceCache.Connect()
            .Filter(CreateFilter())
            .Bind(out _cards)
            .Subscribe();
        }

        private void OnCardEvent(CardsEventArgs args)
        {
            var cards = args.Cards;
            var action = args.Action;

            if (args.Action == CardAction.Create)
            {
                foreach (var card in cards)
                {
                    _sourceCache.AddOrUpdate(card);
                }
            }
            else if (args.Action == CardAction.Remove)
            {
                foreach (var card in cards)
                {
                    _sourceCache.Remove(card);
                }
            }
            else if (args.Action == CardAction.Update) 
            {
                foreach (var card in cards)
                {
                    _sourceCache.AddOrUpdate(card);
                }
            }
        }


        [RelayCommand]
        private async void OpenCardContext(Card card)
        {
            if (card is null) return;
            var result = await _host.OpenContext(new CardContextViewModel(card));
            if (result.Success)
            {
                if(result.ButtonTag == "Edit")
                {
                    var edit_result = await _host.OpenCardModal(card);
                }
                else if(result.ButtonTag == "Remove")
                {
                    var mb_result = await _host.OpenMessageBox("Remove selected card?", ["Yes", "No"]);
                    if(mb_result.ButtonTag == "Yes")
                    {
                        await _host.StartLoading(false);
                        var remove_result = await _dataService.RemoveCardAsync(card);
                        _host.StopLoading();
                        if (!remove_result)
                        {
                            await _host.OpenMessageBox("Failed to remove the file", ["OK"]);
                        }
                    }
                }
            }
        }


        //--------------------------------------------------------------------------------------------FILTER--------------------------------------------------------------
        #region FILTER

        private string _frontTextFilter;
        public string FrontTextFilter
        {
            get => _frontTextFilter;
            set
            {
                SetProperty(ref _frontTextFilter, value);
                UpdateFilter();
            }
        }

        private Func<Card, bool> CreateFilter()
        {
            return item =>
            {
                bool date_pass = SelectedState == CardState.Scheduled 
                    ? item.NextReviewDate > DateTime.Now 
                    : item.NextReviewDate <= DateTime.Now && SelectedState == item.State;

                 var text_pass = string.IsNullOrEmpty(FrontTextFilter) ||
                                  item.FrontText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase) ||
                                  item.BackText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase);

                return date_pass && text_pass; //&& categoryPass && pricePass;
            };
        }

        private void UpdateFilter()
        {
            // DynamicData автоматически применит новый фильтр
            _sourceCache.Refresh(); // Перефильтрует существующие данные
        }

        #endregion
    }
}




