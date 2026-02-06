using FancyCards.Services;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public class TrainingViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;

        public TrainingViewModel(MainWindowViewModel host, DataService dataService)
        {
            _host = host;
            _dataService = dataService;

            var random = new Random();

            var cards = _dataService.GetCards(1).ToArray();

            var learning_cards = cards
                .Where(c => c.State == Models.CardState.Learning)
                .Where(c => c.NextReviewDate.Date <=  DateTime.Now)
                .OrderBy(c => random.Next())
                .Take(5)
                .Select(c => new TrainingCardViewModel(c))
                .ToArray();

            var reviewing_cards = cards
                .Where(c => c.State == Models.CardState.Reviewing)
                .Where(c => c.NextReviewDate.Date <= DateTime.Now)
                .OrderBy(c => random.Next())
                .Take(5)
                .Select(c => new TrainingCardViewModel(c))
                .ToArray();

            var training_cards = new List<TrainingCardViewModel>();
            training_cards.AddRange(learning_cards);
            training_cards.AddRange(reviewing_cards);

            training_cards = training_cards.OrderBy(c => random.Next()).ToList();
        }
    }
}
