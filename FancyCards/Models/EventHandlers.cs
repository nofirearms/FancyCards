using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class DeckEventArgs
    {
        public IEnumerable<Deck> Decks { get; set; }
        public DeckAction Action { get; set; }

        public DeckEventArgs(IEnumerable<Deck> decks, DeckAction deckAction) 
        {
            Decks = decks;
            Action = deckAction;
        }
    }

    public class CardsEventArgs
    {
        public IEnumerable<Card> Cards { get; set; }
        public CardAction Action { get; set; }

        public CardsEventArgs(IEnumerable<Card> cards, CardAction cardAction)
        {
            Cards = cards;
            Action = cardAction;
        }
    }

    public class TextReplacementRuleEventArgs
    {
        public IEnumerable<TextReplacementRule> Rules { get; set; }
        public CardAction Action { get; set; }

        public TextReplacementRuleEventArgs(IEnumerable<TextReplacementRule> rules, CardAction action)
        {
            Rules = rules;
            Action = action;
        }
    }
}
