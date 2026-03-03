using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using FancyCards.Services;
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
        private int _showCount = 0;

        [ObservableProperty]
        private bool _hint = false;

        [ObservableProperty]
        private bool _showBack = false;

        public Card Card => _card;

        [ObservableProperty]
        private TimeSpan _totalTimeSpent;

        [ObservableProperty]
        private TimeSpan _sessionDuration;

        [ObservableProperty]
        private string _answer = string.Empty;

        [ObservableProperty]
        private Difficulty _difficulty;

        public CardState InitialState { get; }
        public Difficulty InitialDifficulty { get; }

        public TrainingCardViewModel(Card card)
        {
            _card = card;

            _totalTimeSpent = _card.TotalTimeSpent;
            InitialState = _card.State;
            InitialDifficulty = _card.Difficulty;
            Difficulty = _card.Difficulty;
        }

        public void OnTimerTick()
        {
            TotalTimeSpent = TotalTimeSpent.Add(TimeSpan.FromSeconds(1));
            SessionDuration = SessionDuration.Add(TimeSpan.FromSeconds(1));
        }





    }


}
