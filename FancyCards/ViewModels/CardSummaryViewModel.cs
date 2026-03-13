using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public class CardSummaryViewModel : ModdedObservableObject
    {
        private Card _card;
        public Card Card => _card;

        public int Id => _card.Id;
        public string FrontText => _card.FrontText;
        public string BackText => _card.BackText;

        public CardFilterState State
        {
            get
            {
                var now = DateTime.Now;

                if(_card.NextReviewDate > now)
                {
                    return CardFilterState.Scheduled;
                }
                return _card.State switch
                {
                    CardState.Reviewing => CardFilterState.Reviewing,
                    CardState.Learning => CardFilterState.Learning,
                    CardState.Archived => CardFilterState.Archived
                };
            }
        }

        public CardSummaryViewModel(Card card)
        {
            _card = card;
        }

        public void Update(Card card = null)
        {
            if (card != null) _card = card;

            UpdateProperties();
        }
        
    }
}
