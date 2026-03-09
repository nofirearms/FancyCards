using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows.Threading; // Обязательно для Dispatcher
using System.Reactive.Concurrency;

using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace FancyCards.ViewModels
{
    public partial class CardListViewModel : ObservableObject, IDisposable
    {
        private readonly ModalService _modalService;
        private readonly DataService _dataService;
        private readonly MainWindowViewModel _host;

        [ObservableProperty]
        private ReadOnlyObservableCollection<Card> _cards;
        private Deck _currentDeck => _dataService.CurrentDeck;

        [ObservableProperty]
        private int _scheduledCount;
        [ObservableProperty]
        private int _reviewingCount;
        [ObservableProperty]
        private int _learningCount;
        [ObservableProperty]
        private int _archivedCount;
        [ObservableProperty]
        private int _totalCount;


        public CardListViewModel(MainWindowViewModel host, DataService dataService, ModalService modalService)
        {
            _modalService = modalService;
            _dataService = dataService;
            _host = host;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {

            // 1. Захватываем UI-поток (здесь он еще доступен)
            var uiContext = SynchronizationContext.Current;

            // 1. Создаем поток для текста
            var textChanged = this.WhenPropertyChanged(x => x.FrontTextFilter)
                .Select(_ => CreateFilter());

            // 2. Создаем поток для стейта
            var stateChanged = this.WhenPropertyChanged(x => x.SelectedState)
                .Select(_ => CreateFilter());

            // 3. Поток для смены колоды из DataService
            var deckChanged = _dataService.SelectedDeckChanged
                .Select(_ => CreateFilter());

            // 4. Объединяем их (теперь оба потока возвращают Func<Card, bool>)
            var filterTrigger = textChanged
                .Merge(stateChanged)
                .Merge(deckChanged)
                .StartWith(CreateFilter()); // Чтобы фильтр применился сразу при загрузке



            _dataService.ConnectCards()
                .Filter(filterTrigger)
                .DisposeMany()
                .ObserveOn(uiContext)
                .Bind(out _cards)
                .Subscribe();


            _dataService.ConnectCards()
                .ToCollection()
                // Объединяем поток изменений карт с потоком выбора колоды
                .CombineLatest(_dataService.SelectedDeckChanged, (items, deck) => new { items, deck })
                .Subscribe(x =>
                {
                    var now = DateTime.Now;
                    var deck_id = x.deck?.Id;

                    var filtered = x.items.Where(o => deck_id == null || o.DeckId == deck_id).ToList();

                    ScheduledCount = filtered.Count(o => o.NextReviewDate > now);
                    ReviewingCount = filtered.Count(o => o.State == CardState.Reviewing && o.NextReviewDate <= now);
                    LearningCount = filtered.Count(o => o.State == CardState.Learning && o.NextReviewDate <= now);
                    ArchivedCount = filtered.Count(o => o.State == CardState.Archived);
                    TotalCount = filtered.Count;
                });
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
                    var edit_result = await _modalService.OpenCardModal(card);
                }
                else if(result.ButtonTag == "Remove")
                {
                    var mb_result = await _modalService.OpenMessageBox("Remove selected card?", ["Yes", "No"]);
                    if(mb_result.ButtonTag == "Yes")
                    {
                        await _host.StartLoading(false);
                        var remove_result = await _dataService.RemoveCardAsync(card);
                        _host.StopLoading();
                        if (!remove_result)
                        {
                            await _modalService.OpenMessageBox("Failed to remove the file", ["OK"]);
                        }
                    }
                }
            }
        }


        //--------------------------------------------------------------------------------------------FILTER--------------------------------------------------------------
        #region FILTER

        [ObservableProperty]
        private string _frontTextFilter;

        [ObservableProperty]
        private CardState _selectedState = CardState.Reviewing;

        public List<CardState> States => new List<CardState>
        {
            CardState.Scheduled,
            CardState.Learning,
            CardState.Reviewing,
            CardState.Archived
        };



        private Func<Card, bool> CreateFilter()
        {


            return item =>
            {
                // 1. Фильтр по колоде (если колода не выбрана - показываем все или ничего, как захочешь)
                if (_currentDeck != null && item.DeckId != _currentDeck.Id)
                    return false;

                bool date_pass = SelectedState == CardState.Scheduled 
                    ? item.NextReviewDate > DateTime.Now && item.State != CardState.Archived
                    : item.NextReviewDate <= DateTime.Now && SelectedState == item.State;

                 var text_pass = string.IsNullOrEmpty(FrontTextFilter) ||
                                  item.FrontText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase) ||
                                  item.BackText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase);

                return date_pass && text_pass; //&& categoryPass && pricePass;
            };
        }


        #endregion

        public void Dispose()
        {
            _cards = null;
        }
    }
}




