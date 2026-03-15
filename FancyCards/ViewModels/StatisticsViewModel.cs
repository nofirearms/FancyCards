using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using FancyCards.Services;


namespace FancyCards.ViewModels
{
    public partial class StatisticsViewModel : BaseModalViewModel<object>
    {
        private readonly DataService _dataService;


        [ObservableProperty]
        private List<SessionsDailySummary> _sessions;

        public StatisticsViewModel(DataService dataService)
        {
            _dataService = dataService;

            Header = "Statistics";

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var sessions = _dataService.GetTrainingSessions(_dataService.CurrentDeck.Id);
            var session_cards = _dataService.GetTrainingSessionCards();
            var summaries = sessions.GroupBy(s => s.Date.Date).Select(g => new SessionsDailySummary
            {
                Date = g.Key,
                TotalTimeSpent = TimeSpan.FromSeconds(g.Sum(s => s.Duration.TotalSeconds)),
                CardsCount = session_cards
                    .Where(sc => g.Select(s => s.Id).Contains(sc.TrainingSessionId))
                    .Select(sc => sc.CardId)
                    .Distinct()
                    .Count(),
                Attempts = g.Count(),
                
            });

            Sessions = summaries.OrderByDescending(s => s.Date).ToList();
        }

    }

    public class SessionsDailySummary
    {
        public TimeSpan TotalTimeSpent { get; set; }
        public DateTime Date { get; set; }
        public int CardsCount { get; set; }
        public int Attempts { get; set; }
    }
}
