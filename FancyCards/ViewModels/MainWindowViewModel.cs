using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Models.Param;
using FancyCards.Services;
using System.Collections.Specialized;
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
        private DeckSummaryViewModel _deck;
        partial void OnDeckChanged(DeckSummaryViewModel value)
        {
            if(value != null)
            {
                LoadCards(value.Id);
                var _ = StoreStartupDeckAsync(value.Id);
            }
        }

        [ObservableProperty]
        private CardListViewModel _cardListViewModel;

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

        public OverlayViewModel OverlayViewModel { get; }

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
            CardListViewModel = _viewModelFactory.Create<CardListViewModel>(this, 0);

            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };

            var _ = InitializeAsync();

        }



        private async Task InitializeAsync()
        {

            var selected_deck_id = _settingsService.StartupSelectedDeckId;
            if(selected_deck_id == 0)
            {
                var result = await OpenDeckModal(new Deck());
                if (result.Success)
                {
                    Deck = new DeckSummaryViewModel(result.Data);
                }
                else
                {
                    InitializeAsync();
                }
            }
            else
            {
                Deck = new DeckSummaryViewModel(await _dataService.GetDeckByIdAsync(selected_deck_id));
            }
            
        }

        private async void LoadCards(int deckId)
        {
            await StartLoading(false);
            CardListViewModel.Dispose();
            CardListViewModel = _viewModelFactory.Create<CardListViewModel>(this, deckId);
            StopLoading();
        }

        public async Task<ModalResult<T>> OpenContext<T>(BaseModalViewModel<T> context)
        {
            try
            {
                ContextMenu = context;

                var result = await context.Task;

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

        [RelayCommand]
        private async void OpenDeckList()
        {
            var result = await OpenDeckListModal();
            if (result.Success)
            {
                Deck = new DeckSummaryViewModel(result.Data);
            }
        }

        [RelayCommand]
        private async void CreateCard()
        {
            await OpenCardModal(null);
        }

        [RelayCommand]
        private async void StartTraining()
        {
            await StartLoading();
            var start_view_result = await _modalService.ShowModalAsync(_viewModelFactory.Create<TrainingStartViewModel>());
            if(start_view_result.ButtonTag == "StartTraining")
            {
                await StartLoading();
                await _modalService.ShowModalAsync(_viewModelFactory.Create<TrainingViewModel>(start_view_result.Data));
            }
        }

        [RelayCommand]
        private async void OpenSettings()
        {
            await OpenSettingsModal();
        }

        [RelayCommand]
        private async void OpenTextReplacementRules()
        {
            await OpenTextReplacementRuleListModal();
        }

        [RelayCommand]
        private async void OpenStatistics()
        {
            await OpenStatisticsModal();
        }

        public async Task<ModalResult<Deck>> OpenDeckModal(Deck deck)
        {
            await StartLoading();
            var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<DeckDetailViewModel>(deck ?? new Deck()));

            return result;
        }

        public async Task<ModalResult<Card>> OpenCardModal(Card card)
        {
            await StartLoading();
            var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<CardDetailViewModel>(card ?? new Card()));

            return result;
        }

        public async Task<ModalResult<object>> OpenMessageBox(string message, string[] buttons, string header = "Attention!", Brush background = null)
        {
            var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<MessageBoxViewModel>(new MessageBoxParameters(header, message, buttons, background)));
            return result;
        }

        public async Task<ModalResult<object>> OpenFailedAnswer(string answer, string correct)
        {
            var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<TrainingFailedAnswerViewModel>(new TrainingFailedAnswerParameters(answer, correct)));
            return result;
        }

        public async Task<ModalResult<object>> OpenTrainingResult(IEnumerable<TrainingCardViewModel> cards)
        {
            var result = await _modalService.ShowModalAsync(new TrainingResultViewModel(cards));
            return result;
        }

        public async Task<ModalResult<object>> OpenSettingsModal()
        {
            await StartLoading();
            var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<SettingsViewModel>());
            return result;
        }

        public async Task<ModalResult<Deck>> OpenDeckListModal()
        {
            await StartLoading();
            return await _modalService.ShowModalAsync(_viewModelFactory.Create<DeckListViewModel>());
        }

        public async Task<ModalResult<object>> OpenTextReplacementRuleListModal()
        {
            await StartLoading();
            return await _modalService.ShowModalAsync(_viewModelFactory.Create<TextReplacementRuleListViewModel>());
        }

        public async Task<ModalResult<TextReplacementRule>> OpenTextReplacementRuleDetailModal(TextReplacementRule rule)
        {
            await StartLoading();
            return await _modalService.ShowModalAsync(_viewModelFactory.Create<TextReplacementRuleDetailViewModel>(rule ?? new TextReplacementRule("")));
        }

        public async Task<ModalResult<object>> OpenStatisticsModal()
        {
            await StartLoading();
            return await _modalService.ShowModalAsync(_viewModelFactory.Create<StatisticsViewModel>());
        }

        [RelayCommand]
        public async Task StartLoading(bool showBackground = true)
        {
            Loading = true;
            ShowLoadingBackground = showBackground;

            //unfreeze interface
            await Task.Delay(20);

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
