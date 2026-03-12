using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Models.Param;
using FancyCards.Services;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FancyCards.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ModalService _modalService;
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly OverlayService _overlayService;

        public string Title => "Fancy Cards";

        [ObservableProperty]
        private string _deckName;

        [ObservableProperty]
        private CardListViewModel _cardListViewModel;
        public OverlayViewModel OverlayViewModel { get; }


        public IReadOnlyList<BaseModalViewModel> ActiveModals => _modalService.ActiveModals;
        public bool HasActiveModals => _modalService.ActiveModals.Any();


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ContextMenuOpen))]
        private BaseModalViewModel _contextMenu;

        [ObservableProperty]
        private bool _loading = false;

        [ObservableProperty]
        private bool _showLoadingBackground = false;

        public bool ContextMenuOpen => _contextMenu != null;


        public MainWindowViewModel(ViewModelFactory viewModelFactory, 
            DataService dataService, 
            ModalService modalService, 
            AudioEngine audioEngine, 
            SettingsService settingsService, 
            OverlayService overlayService, 
            OverlayViewModel overlayViewModel)
        {
            _modalService = modalService;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _overlayService = overlayService;

            OverlayViewModel = overlayViewModel;
            CardListViewModel = _viewModelFactory.Create<CardListViewModel>(this);

            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };

            var _ = InitializeAsync();

            _modalService.OnModalOpen += async () =>
            {
                StartLoading(true);
            };
        }


        private async Task InitializeAsync()
        {
            await _dataService.InitializeAsync();

            _dataService.SelectedDeckChanged
                .Where(deck => deck != null)
                .ObserveOn(SynchronizationContext.Current)// Чтобы менять UI-свойства безопасно
                .Subscribe(deck =>
                {
                    DeckName = deck.Name;
                    _ = StoreStartupDeckAsync(deck.Id);
                });

            var selected_deck_id = _settingsService.StartupSelectedDeckId;
            if(selected_deck_id == 0)
            {
                var result = await _modalService.OpenDeckModal(new Deck());
                if (result.Success)
                {
                    _dataService.CurrentDeck = result.Data;
                }
                else
                {
                    InitializeAsync();
                }
            }
            else
            {
                _dataService.CurrentDeck = _dataService.GetDeckById(selected_deck_id);
            }
            
        }

        public async Task<ModalResult<T>> OpenContext<T>(BaseModalViewModel<T> context)
        {
            try
            {
                ContextMenu = context;

                var result = await context.ResultTask;

                ContextMenu = null;

                return result;
            }
            finally
            {
                ////unfreeze ui
                //await Task.Delay(15);
            }
        }

        private async Task StoreStartupDeckAsync(int deckId)
        {
            await StartLoading(false);
            _settingsService.StartupSelectedDeckId = deckId;
            //TODO возможно сохранять только при закрытии программы
            var _ = _settingsService.SaveAsync();
            StopLoading();
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
        public async Task StartLoading(bool showBackground = true)
        {
            Loading = true;
            ShowLoadingBackground = showBackground;

            //unfreeze interface
            //await Task.Delay(20);

            ChangeCursor(Cursors.Wait);
        }
        [RelayCommand]
        public void StopLoading()
        {
            Loading = false;
            ShowLoadingBackground = false;

            ChangeCursor();
        }


        public void ChangeCursor(Cursor cursor = null)
        {
            Mouse.OverrideCursor = cursor;
        }
    }
}
