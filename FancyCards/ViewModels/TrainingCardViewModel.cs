using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class TrainingCardViewModel : ObservableObject
    {
        private readonly Card _card;

        [ObservableProperty]
        private TrainingCardState _cardStatus = TrainingCardState.Queue;

        [ObservableProperty]
        private bool _hint = false;

        [ObservableProperty]
        private bool _showBack = false;

        public Card Card => _card;

        [ObservableProperty]
        private TimeSpan _totalTimeSpent;

        [ObservableProperty]
        private TimeSpan _sessionTimeSpent;

        [ObservableProperty]
        private string _answer = string.Empty;

        public TrainingCardViewModel(Card card)
        {
            _card = card;

        }

        public void OnTimerTick()
        {
            TotalTimeSpent = TotalTimeSpent.Add(TimeSpan.FromSeconds(1));
            SessionTimeSpent = SessionTimeSpent.Add(TimeSpan.FromSeconds(1));
        }

    }


}
