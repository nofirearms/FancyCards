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
        private readonly MainWindowViewModel _host;

        [ObservableProperty]
        private int _learnCards = 0;

        [ObservableProperty]
        private int _reviewCards = 0;

        [ObservableProperty]
        private int _maxLearnCards = 0;

        [ObservableProperty]
        private int _maxReviewCards = 0;

        public TrainingStartViewModel(MainWindowViewModel host, DataService dataService)
        {
            _dataService = dataService;
            _host = host;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var db_cards = await _dataService.GetCardsAsync(1);

            var random = new Random();

            var cards = db_cards
                .Where(c => c.NextReviewDate.Date <= DateTime.Now)
                .Where(c => c.State == CardState.Learning || c.State == Models.CardState.Reviewing)
                .OrderBy(c => random.NextDouble())
                .ToArray();

            var learning_cards = cards
                .Where(c => c.State == CardState.Learning)
                .Take(5)
                .ToArray();

            var reviewing_cards = cards
                .Where(c => c.State == CardState.Reviewing)
                .Take(5)
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
                    await _host.OpenMessageBox("No cards available for training", ["Ok"], background: new SolidColorBrush(Colors.LightPink));
                    return;
                });

            }
        }

        [RelayCommand]
        private void StartTraining() => Close(buttonTag: "StartTraining");

        [RelayCommand]
        private void CancelTraining() => Cancel();
    }
}
