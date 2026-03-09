using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public class DeckSummaryViewModel : ModdedObservableObject
    {
        private Deck _deck;
        public Deck Deck => _deck;

        public int Id => _deck.Id;
        public string Name => _deck.Name;
        public int CardsCount { get; } 

        public DeckSummaryViewModel(Deck deck, int cardsCount)
        {
            _deck = deck;
            CardsCount = cardsCount;
        }

        public void Update(Deck deck = null)
        {
            if (deck != null) _deck = deck;

            UpdateProperties();
        }
    }
}
