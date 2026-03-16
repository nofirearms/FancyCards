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
        private readonly ThemeService _themeService;

        public string Title => "Fancy Cards";


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
            OverlayViewModel overlayViewModel,
            ThemeService themeService)
        {
            _modalService = modalService;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _overlayService = overlayService;
            _themeService = themeService;

            OverlayViewModel = overlayViewModel;
            CardListViewModel = _viewModelFactory.Create<CardListViewModel>();

            // Подписываемся на изменение коллекции модальных окон
            ((INotifyCollectionChanged)_modalService.ActiveModals).CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(HasActiveModals));
                OnPropertyChanged(nameof(ActiveModals));
            };

            _modalService.OnModalOpen += async () =>
            {
                StartLoading(true);
            };

            _modalService.OnContextChanged += (context) =>
            {
                ContextMenu = context;
            };

            _ = InitializeAsync();

            
        }


        private async Task InitializeAsync()
        {
            _themeService.SetBaseTheme(_settingsService.Theme);

            await _dataService.InitializeAsync();

            await LoadDeckAsync();
        }

        //загружаем деку по id из настроек, если в настройках нет деки, то через модалку создаём
        private async Task LoadDeckAsync()
        {
            var selected_deck_id = _settingsService.StartupSelectedDeckId;
            if (selected_deck_id == 0)
            {
                var result = await _modalService.OpenDeckModal(new Deck());
                if (result.Success)
                {
                    _dataService.CurrentDeck = result.Data;
                }
                else
                {
                    LoadDeckAsync();
                }
            }
            else
            {
                _dataService.CurrentDeck = _dataService.GetDeckById(selected_deck_id);
            }
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
