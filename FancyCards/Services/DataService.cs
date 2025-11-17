using FancyCards.Database;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Services
{
    public class DataService
    {
        public EventHandler<DeckEventArgs> DeckEvent;
        public EventHandler<CardsEventArgs> CardEvent;

        private readonly Repository _repository;

        private List<Deck> _decks = [];
        public List<Deck> Decks => _decks;
        public DataService(Repository repository)
        {
            _repository = repository;
            _decks = _repository.GetAll<Deck>().ToList();
        }
    }
}
