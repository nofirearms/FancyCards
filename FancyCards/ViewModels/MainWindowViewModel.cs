using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
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

        public string Title => "Fancy Cards";

        [ObservableProperty]
        private ReadOnlyObservableCollection<DeckSummaryViewModel> _decks;
        [ObservableProperty]
        private DeckSummaryViewModel _selectedDeck;
        private SourceCache<DeckSummaryViewModel, int> _sourceCache;

        public CardListViewModel CardListViewModel { get; set; }

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

        public MainWindowViewModel(ViewModelFactory viewModelFactory, DataService dataService, ModalService modalService, AudioEngine audioEngine, SettingsService settingsService)
        {
            _modalService = modalService;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
           
            


            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };

            _dataService.DeckEvent += OnDeckEvent;



            var _ = InitializeAsync();


        }



        private async Task InitializeAsync()
        {
            var db_decks = await _dataService.GetDecksAsync();


            _sourceCache = new SourceCache<DeckSummaryViewModel, int>(o => o.Id);
            _sourceCache.AddOrUpdate(db_decks.Select(d => new DeckSummaryViewModel(d)) ?? new List<DeckSummaryViewModel>());

            _sourceCache.Connect()
                .Filter(CreateFilter())
                .Bind(out _decks)
                .Subscribe();

            var selected_deck = _decks.FirstOrDefault();
            if (selected_deck != null)
            {
                SelectedDeck = selected_deck;
                CardListViewModel = _viewModelFactory.Create<CardListViewModel>(this, selected_deck.Id);
            }
            else
            {
                await OpenDeckModal(new Deck());
            }



        }

        private void OnDeckEvent(DeckEventArgs args)
        {
            var decks = args.Decks;
            var action = args.Action;

            if (args.Action == DeckAction.Create)
            {
                foreach (var deck in decks)
                {
                    var d = new DeckSummaryViewModel(deck);
                    _sourceCache.AddOrUpdate(d);
                    _selectedDeck = d;
                }
            }
            else if (args.Action == DeckAction.Remove)
            {
                foreach (var deck in decks)
                {
                    var d = _decks.FirstOrDefault(o => o.Deck.Id == deck.Id);
                    if(d != null)
                    {
                        _sourceCache.Remove(d);
                    }
                    
                }
            }
            else if (args.Action == DeckAction.Update)
            {
                foreach (var deck in decks)
                {
                    var d = _decks.FirstOrDefault(o => o.Deck.Id == deck.Id);
                    if (d != null)
                    {
                        _sourceCache.AddOrUpdate(d);
                    }

                }
            }
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
            var result = await _modalService.ShowModalAsync(new MessageBoxViewModel(header, message, buttons, background));
            return result;
        }

        public async Task<ModalResult<object>> OpenFailedAnswer(string answer, string correct)
        {
            var result = await _modalService.ShowModalAsync(new TrainingFailedAnswerViewModel(answer, correct));
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


        [RelayCommand]
        public async Task StartLoading(bool showBackground = true)
        {
            Loading = true;
            ShowLoadingBackground = showBackground;

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


        //----------------------------------------------------------------------------- FILTER --------------------------------------------------------------------------
        private Func<DeckSummaryViewModel, bool> CreateFilter()
        {
            return item => true;
            
            //return item =>
            //{
            //    var front_pass = string.IsNullOrEmpty(FrontTextFilter) ||
            //                  item.FrontText.Contains(FrontTextFilter);

            //    return front_pass; //&& categoryPass && pricePass;
            //};
        }
    }
}
