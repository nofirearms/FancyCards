using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Windows.Media;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class TrainingStartViewModel : BaseModalViewModel<IEnumerable<Card>>
    {
        private readonly DataService _dataService;
        private readonly SettingsService _settingsService;
        private readonly MainWindowViewModel _host;
        private readonly ModalService _modalService;

        private IEnumerable<Card> _dbCardsOnDate;

        [ObservableProperty]
        private int _learnCardsCount = 0;
        partial void OnLearnCardsCountChanged(int value)
        {
            StartTrainingCommand?.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private int _reviewCardsCount = 0;
        partial void OnReviewCardsCountChanged(int value)
        {
            StartTrainingCommand?.NotifyCanExecuteChanged();
        }

        //максимум карт возможных на текущую дату
        [ObservableProperty]
        private int _maxLearnCardsCount = 0;

        //максимум карт возможных на текущую дату
        [ObservableProperty]
        private int _maxReviewCardsCount = 0;

        //дефолтное значение, при даблклике
        [ObservableProperty]
        public int _defaultReviewCardsCount = 0;
        [ObservableProperty]
        public int _defaultLearnCardsCount = 0;

        public TrainingStartViewModel(MainWindowViewModel host, DataService dataService, ModalService modalService, SettingsService settingsService)
        {
            _host = host;
            _modalService = modalService;

            _dataService = dataService;
            _settingsService = settingsService;

            Header = "Start Training";

            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var cards = _dataService.GetCardsByDeckId(_dataService.CurrentDeck.Id);
            _dbCardsOnDate = cards.Where(c => c.NextReviewDate.Date <= DateTime.Now);

            MaxReviewCardsCount = _dbCardsOnDate.Where(c => c.State == CardState.Reviewing).Count();
            MaxLearnCardsCount = _dbCardsOnDate.Where(c => c.State == CardState.Learning).Count();

            ReviewCardsCount = Math.Min(_dataService.CurrentDeck.Settings.TrainingReviewCards, MaxReviewCardsCount);
            LearnCardsCount = Math.Min(_dataService.CurrentDeck.Settings.TrainingLearnCards, MaxLearnCardsCount);

            DefaultReviewCardsCount = Math.Min(_dataService.CurrentDeck.Settings.TrainingReviewCards, MaxReviewCardsCount);
            DefaultLearnCardsCount = Math.Min(_dataService.CurrentDeck.Settings.TrainingLearnCards, MaxLearnCardsCount);
        }

        [RelayCommand(CanExecute = nameof(CanStartTraining))]

        private async void StartTraining()
        {
            var random = new Random();

            var cards = _dbCardsOnDate
                .Where(c => c.State == CardState.Learning || c.State == CardState.Reviewing)
                .OrderBy(c => random.NextDouble())
                .ToArray();

            var learning_cards = cards
                .Where(c => c.State == CardState.Learning)
                .Take(LearnCardsCount)
                .ToArray();

            var reviewing_cards = cards
                .Where(c => c.State == CardState.Reviewing)
                .Take(ReviewCardsCount)
                .ToArray();

            var training_cards = learning_cards
                .Concat(reviewing_cards)
                .OrderBy(c => random.NextDouble())
                .ToList();

            if (!training_cards.Any())
            {
                //чтобы message box открылся после загрузки
                await App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, async () =>
                {
                    await _modalService.OpenMessageBox("No cards available for training", ["Ok"], background: new SolidColorBrush(Colors.LightPink));
                    return;
                });

            }

            Close(buttonTag: "StartTraining", data: training_cards);
        }
        private bool CanStartTraining() => ReviewCardsCount > 0 || LearnCardsCount > 0;


        [RelayCommand]
        private void CancelTraining() => Cancel();
    }
}
