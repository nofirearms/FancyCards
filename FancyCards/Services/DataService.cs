using FancyCards.Database;
using FancyCards.Helpers;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Services
{
    public class DataService
    {
        public event Action<DeckEventArgs> DeckEvent;
        public event Action<CardsEventArgs> CardEvent;

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

        public Deck GetDeckById(int id) => _decks.FirstOrDefault(d => d.Id == id);
        public IEnumerable<Card> GetCards(int deckId) => GetDeckById(deckId).Cards;
    }
}
