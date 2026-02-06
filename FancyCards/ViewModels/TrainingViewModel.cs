using CommunityToolkit.Mvvm.Input;
using FancyCards.Services;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class TrainingViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;

        public TrainingViewModel(MainWindowViewModel host, DataService dataService)
        {
            _host = host;
            _dataService = dataService;

            var random = new Random();

            var cards = _dataService.GetCards(1)
                .Where(c => c.NextReviewDate.Date <= DateTime.Now)
                .Where(c => c.State == Models.CardState.Learning || c.State == Models.CardState.Reviewing)
                .OrderBy(c => random.NextDouble())
                .ToArray();

            var learning_cards = cards
                .Where(c => c.State == Models.CardState.Learning)
                .Take(5)
                .ToArray();

            var reviewing_cards = cards
                .Where(c => c.State == Models.CardState.Reviewing)               
                .Take(5)
                .ToArray();

            var training_cards = learning_cards
                .Concat(reviewing_cards)
                .Select(c => new TrainingCardViewModel(c))
                .ToList();

        }


        [RelayCommand]
        private void CancelTraining() => Cancel();
    }
}
