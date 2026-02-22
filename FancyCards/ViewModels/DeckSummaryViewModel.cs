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
        public int CardsCount => _deck.Cards.Count;
        public IEnumerable<Card> Cards => _deck.Cards;

        public DeckSummaryViewModel(Deck deck)
        {
            _deck = deck;
        }

        public void Update(Deck deck)
        {
            if (deck != null) _deck = deck;

            UpdateProperties();
        }
    }
}
