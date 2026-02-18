using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
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

        public string Title => "Fancy Cards";

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


        public async Task<ModalResult<Card>> OpenCardModal(Card card)
        {
            await StartLoading();
            try
            {
                var result = await _modalService.ShowModalAsync(_viewModelFactory.Create<CardDetailViewModel>(card ?? new Card()));

                return result;
            }
            finally
            {

            }

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
    }
}
