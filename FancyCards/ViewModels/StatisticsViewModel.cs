using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class StatisticsViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;


        [ObservableProperty]
        private List<SessionsDailySummary> _sessions;

        public StatisticsViewModel(MainWindowViewModel host, DataService dataService)
        {
            _host = host;
            _dataService = dataService;

            Header = "Statistics";

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var sessions = await _dataService.GetTrainingSessionsAsync(_host.Deck.Deck.Id);
            var summaries = sessions.GroupBy(s => s.Date.Date).Select(g => new SessionsDailySummary
            {
                Date = g.Key,
                TotalTimeSpent = TimeSpan.FromSeconds(g.Sum(s => s.Duration.TotalSeconds))
            });

            Sessions = summaries.ToList();
        }

    }

    public class SessionsDailySummary
    {
        public TimeSpan TotalTimeSpent { get; set; }
        public DateTime Date { get; set; }
    }
}
