using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FancyCards.ViewModels
{
    public partial class CardListViewModel : ObservableObject, IDisposable
    {
        private readonly DataService _dataService;
        private readonly MainWindowViewModel _host;

        [ObservableProperty]
        private ReadOnlyObservableCollection<Card> _cards;
        
        private SourceCache<Card, int> _sourceCache;
        private IDisposable _cleanUp;


        //[ObservableProperty]
        //private int _learnCount;
        //[ObservableProperty]
        //private int _reviewCount;

        [ObservableProperty]
        private int _scheduledCount;
        [ObservableProperty]
        private int _reviewingCount;
        [ObservableProperty]
        private int _learningCount;
        [ObservableProperty]
        private int _archivedCount;



        public CardListViewModel(MainWindowViewModel host, DataService dataService, int deckId)
        {
            _dataService = dataService;
            _host = host;

            
            _dataService.CardEvent += OnCardEvent;

            _ = InitializeAsync(deckId);
        }

        private async Task InitializeAsync(int deckId)
        {
            if (deckId == 0) return;
            //_cards = new ObservableCollection<Card>(_decks.First().Cards ?? new List<Card>());
            _sourceCache = new SourceCache<Card, int>(o => o.Id);

            var cards = await _dataService.GetCardsAsync(deckId);
            _sourceCache.AddOrUpdate(cards ?? new List<Card>());

            // Подписываемся на изменения и пересчитываем свойства
            _cleanUp = _sourceCache.Connect()
                .Subscribe(cards =>
                {
                    ScheduledCount = _sourceCache.Items.Count(o => o.NextReviewDate > DateTime.Now);
                    ReviewingCount = _sourceCache.Items.Count(o => o.State == CardState.Reviewing && o.NextReviewDate <= DateTime.Now);
                    LearningCount = _sourceCache.Items.Count(o => o.State == CardState.Learning && o.NextReviewDate <= DateTime.Now);
                    ArchivedCount = _sourceCache.Items.Count(o => o.State == CardState.Archived);

                });

            _sourceCache.Connect()
            .Filter(CreateFilter())
            .Bind(out _cards)
            .Subscribe();


        }

        private async void OnCardEvent(CardsEventArgs args)
        {
            App.Current.Dispatcher.Invoke(() => 
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


            }, System.Windows.Threading.DispatcherPriority.Background);
        }


        [RelayCommand]
        private async void OpenCardContext(Card card)
        {
            if (card is null) return;

            //await _host.OpenMessageBox($"I:{card.Scores.I}; Last Review:{card.LastReviewDate}", ["Ok"], background: new SolidColorBrush(Colors.Azure));

            //return;
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

        public List<CardState> States => new List<CardState>
        {
            CardState.Scheduled,
            CardState.Learning,
            CardState.Reviewing,
            CardState.Archived
        };

        [ObservableProperty]
        private CardState _selectedState = CardState.Reviewing;

        partial void OnSelectedStateChanged(CardState value)
        {
            UpdateFilter();
        }


        private Func<Card, bool> CreateFilter()
        {
            return item =>
            {
                bool date_pass = SelectedState == CardState.Scheduled 
                    ? item.NextReviewDate > DateTime.Now && item.State != CardState.Archived
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

        public void Dispose()
        {
            _cleanUp?.Dispose();
            _sourceCache?.Dispose();
            _sourceCache = null;
            _cleanUp = null;
            _dataService.CardEvent -= OnCardEvent;

        }
    }
}




