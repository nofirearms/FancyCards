using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Threading; // Обязательно для Dispatcher

namespace FancyCards.ViewModels
{
    public partial class CardListViewModel : ObservableObject, IDisposable
    {
        private readonly ModalService _modalService;
        private readonly DataService _dataService;
        private readonly SettingsService _settingsService;
        private readonly LoadingService _loadingService;

        [ObservableProperty]
        private string _deckName;

        [ObservableProperty]
        private ReadOnlyObservableCollection<CardSummaryViewModel> _cards;
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


        public CardListViewModel(DataService dataService, ModalService modalService, SettingsService settingsService, LoadingService loadingService)
        {
            _modalService = modalService;
            _dataService = dataService;
            _settingsService = settingsService;
            _loadingService = loadingService;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _dataService.SelectedDeckChanged
                .Where(deck => deck != null)
                .ObserveOn(SynchronizationContext.Current)// Чтобы менять UI-свойства безопасно
                .Subscribe(deck =>
                {
                    DeckName = deck.Name;
                    _ = StoreStartupDeckAsync(deck.Id);
                });

            InitializeCardList();
        }

        private void InitializeCardList()
        {

            // 1. Захватываем UI-поток (здесь он еще доступен)
            var uiContext = SynchronizationContext.Current;


            var midnightTrigger = GetMidnightTimer()
                .Select(_ => CreateFilter())
                .StartWith(CreateFilter());

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
                .Merge(midnightTrigger)
                .StartWith(CreateFilter()); // Чтобы фильтр применился сразу при загрузке



            // Общий источник данных
            var all_сards = _dataService.ConnectCards()
                .Transform(c => new CardSummaryViewModel(c))
                .DisposeMany()
                .Publish();

            // Для UI с фильтром
            all_сards
                .Filter(filterTrigger)
                .ObserveOn(uiContext)
                .Bind(out _cards)
                .Subscribe();

            // Для счетчиков без фильтра
            all_сards
                .ToCollection()
                .CombineLatest(
                    _dataService.SelectedDeckChanged,
                    midnightTrigger.Select(_ => Unit.Default).StartWith(Unit.Default),
                    (items, deck, _) => new { items, deck })
                .ObserveOn(uiContext)
                .Subscribe(x =>
                {
                    var now = DateTime.Now;
                    var deck_id = x.deck?.Id;

                    var filtered = x.items.Where(o => deck_id == null || o.Card.DeckId == deck_id).ToList();

                    ScheduledCount = filtered.Count(o => o.State == CardFilterState.Scheduled);
                    ReviewingCount = filtered.Count(o => o.State == CardFilterState.Reviewing);
                    LearningCount = filtered.Count(o => o.State == CardFilterState.Learning);
                    ArchivedCount = filtered.Count(o => o.State == CardFilterState.Archived);
                    TotalCount = filtered.Count;
                });

            // Запускаем общий источник
            all_сards.Connect();
        }


        private IObservable<long> GetMidnightTimer()
        {
            return Observable.Create<long>(observer =>
            {
                var now = DateTime.Now;
                var midnight = DateTime.Today.AddDays(1); // следующая полночь
                //test
                //var midnight = DateTime.Now.AddSeconds(20);
                var initialDelay = midnight - now;

                // Таймер, который срабатывает в полночь и затем каждые 24 часа
                return Observable.Timer(initialDelay, TimeSpan.FromDays(1))
                    .Subscribe(observer);
            });
        }

        private async Task StoreStartupDeckAsync(int deckId)
        {
            await _loadingService.ShowLoadingAsync(async() =>
            {
                _settingsService.StartupSelectedDeckId = deckId;
                //TODO возможно сохранять только при закрытии программы
                await _settingsService.SaveAsync();

            }, true, true);
        }


        private AsyncRelayCommand _openMenuCommand;
        public IAsyncRelayCommand OpenMenuCommand => _openMenuCommand ??= new AsyncRelayCommand(OpenMainMenu);
        private async Task OpenMainMenu()
        {
            var result = await _modalService.OpenContext(new MainMenuContextViewModel());
            if (result.Success)
            {
                if (result.ButtonTag == "Settings")
                {
                    OpenSettingsModalCommand?.Execute(null);
                }
                else if (result.ButtonTag == "Statistics")
                {
                    OpenStatisticsModalCommand?.Execute(null);
                }
                else if (result.ButtonTag == "Text rules")
                {
                    OpenTextReplacementRulesModalCommand?.Execute(null);
                }
            }
        }


        private AsyncRelayCommand _openDeckListModalCommand;
        public IAsyncRelayCommand OpenDeckListModalCommand => _openDeckListModalCommand ??= new AsyncRelayCommand(OpenDeckList);
        private async Task OpenDeckList()
        {
            var result = await _modalService.OpenDeckListModal();
            if (result.Success)
            {
                _dataService.CurrentDeck = result.Data;
            }
        }
        private AsyncRelayCommand _openCardDetailModalCommand;
        public IAsyncRelayCommand OpenCardDetailModalCommand => _openCardDetailModalCommand ??= new AsyncRelayCommand(CreateCard);
        private async Task CreateCard()
        {
            await _modalService.OpenCardModal(null);
        }


        private AsyncRelayCommand _openStartTrainingModalCommand;
        public IAsyncRelayCommand OpenStartTrainingModalCommand => _openStartTrainingModalCommand ??= new AsyncRelayCommand(StartTraining);
        private async Task StartTraining()
        {
            var start_view_result = await _modalService.OpenTrainingStart();
            if (start_view_result.ButtonTag == "StartTraining")
            {
                await _modalService.OpenTraining(start_view_result.Data);
            }
        }

        private AsyncRelayCommand _openSettingsModalCommand;
        public IAsyncRelayCommand OpenSettingsModalCommand => _openSettingsModalCommand ??= new AsyncRelayCommand(OpenSettings);
        private async Task OpenSettings()
        {
            await _modalService.OpenSettingsModal();
        }

        private AsyncRelayCommand _openTextReplacementRulesModalCommand;
        public IAsyncRelayCommand OpenTextReplacementRulesModalCommand => _openTextReplacementRulesModalCommand ??= new AsyncRelayCommand(OpenTextReplacementRules);
        private async Task OpenTextReplacementRules()
        {
            await _modalService.OpenTextReplacementRuleListModal();
        }

        private AsyncRelayCommand _openStatisticsModalCommand;
        public IAsyncRelayCommand OpenStatisticsModalCommand => _openStatisticsModalCommand ??= new AsyncRelayCommand(OpenStatistics);
        private async Task OpenStatistics()
        {
            await _modalService.OpenStatisticsModal();
        }




        [RelayCommand]
        private async void OpenCardContext(CardSummaryViewModel cardVM)
        {
            if (cardVM is null) return;

            //await _host.OpenMessageBox($"I:{card.Scores.I}; Last Review:{card.LastReviewDate}", ["Ok"], background: new SolidColorBrush(Colors.Azure));

            //return;
            var card = cardVM.Card;
            var result = await _modalService.OpenContext(new CardContextViewModel(card));
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

                        var remove_result = await _loadingService.ShowLoadingAsync(async () =>
                        {
                             return await _dataService.RemoveCardAsync(card);
                        }, true, false);


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
        private CardFilterState _selectedState = CardFilterState.Reviewing;

        public List<CardFilterState> States => new List<CardFilterState>
        {
            CardFilterState.Scheduled,
            CardFilterState.Learning,
            CardFilterState.Reviewing,
            CardFilterState.Archived,
            CardFilterState.Total
        };



        private Func<CardSummaryViewModel, bool> CreateFilter()
        {


            return item =>
            {
                // 1. Фильтр по колоде (если колода не выбрана - показываем все или ничего, как захочешь)
                if (_currentDeck != null && item.Card.DeckId != _currentDeck.Id)
                    return false;

                bool state_pass = SelectedState == CardFilterState.Total
                    ? true
                    : SelectedState == item.State;

                 var text_pass = string.IsNullOrEmpty(FrontTextFilter) ||
                                  item.FrontText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase) ||
                                  item.BackText.Contains(FrontTextFilter, StringComparison.OrdinalIgnoreCase);

                return state_pass && text_pass; //&& categoryPass && pricePass;
            };
        }


        #endregion

        public void Dispose()
        {
            _cards = null;
        }
    }
}




