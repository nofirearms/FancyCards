using FancyCards.Database;
using FancyCards.Helpers;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FancyCards.Services
{
    public class DataService
    {
        public event Action<DeckEventArgs> DeckEvent;
        public event Action<CardsEventArgs> CardEvent;
        public event Action<TextReplacementRuleEventArgs> TextReplacementRuleEvent;

        private readonly Repository _repository;

        private List<Deck> _decks = [];
        public List<Deck> Decks => _decks;
        public DataService(Repository repository)
        {
            _repository = repository;

            LoadData();
        }

        private async void LoadData()
        {
            _decks = [.. await _repository.GetAllDecksAsync()];
        }

        public async Task<Card> CreateCardAsync(int deckId, Card card)
        {
            await _repository.AddCardToDeckAsync(deckId, card); 

            CardEvent?.Invoke(new CardsEventArgs([card], CardAction.Create));

            return card;
        }

        public async Task<Card> UpdateCardAsync(int deckId, Card card)
        {
            await _repository.UpdateCardAsync(card);

            CardEvent?.Invoke(new CardsEventArgs([card], CardAction.Update));

            return card;
        }

        public async Task<bool> RemoveCardAsync(int deckId, Card card)
        {
            bool output = true;
            var result = await _repository.RemoveCardFromDeckAsync(deckId, card.Id);
            if (result)
            {
                if(card.Audio != null)
                {
                    var delete_result = PathHelper.DeleteFile(card.Audio.Path);
                    if (!delete_result) output = false;
                }

            }

            CardEvent?.Invoke(new CardsEventArgs([card], CardAction.Remove));
            return result;
        }


        //--------------------------------------------------------------------------------- REPLACEMENT TEXT RULES -------------------------------------------------------

        public async Task<TextReplacementRule> CreateTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.AddTextReplacementRuleAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Create));

            return rule;
        }

        public async Task<TextReplacementRule> UpdateTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.UpdateTextReplacementRuleAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Update));

            return rule;
        }

        public async Task<TextReplacementRule> RemoveTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.RemoveTextReplacementRuleAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Remove));
            return rule;
        }


        public Deck GetDeckById(int id) => _decks.FirstOrDefault(d => d.Id == id);
        public IEnumerable<Card> GetCards(int deckId) => GetDeckById(deckId).Cards;
    }
}
