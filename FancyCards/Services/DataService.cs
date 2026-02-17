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


        public Deck GetDeckById(int id) => _decks.FirstOrDefault(d => d.Id == id);

        //-------------------------------------------------------------------------------------------------- CARDS ----------------------------------------------------------
        public IEnumerable<Card> GetCards(int deckId) => GetDeckById(deckId).Cards;

        public async Task<Card> CreateCardAsync(int deckId, Card card)
        {
            card.DeckId = deckId;
            await _repository.AddAsync(card); 

            CardEvent?.Invoke(new CardsEventArgs([card], CardAction.Create));

            return card;
        }

        public async Task<Card> UpdateCardAsync(Card card)
        {
            await _repository.UpdateAsync(card);

            CardEvent?.Invoke(new CardsEventArgs([card], CardAction.Update));

            return card;
        }

        public async Task<bool> RemoveCardAsync(Card card)
        {
            bool output = true;
            var result = await _repository.RemoveAsync(card);
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
            await _repository.AddAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Create));

            return rule;
        }

        public async Task<TextReplacementRule> UpdateTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.UpdateAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Update));

            return rule;
        }

        public async Task<TextReplacementRule> RemoveTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.RemoveAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Remove));
            return rule;
        }


        public async Task<IEnumerable<TextReplacementRule>> GetTextReplacementRules() => await _repository.GetAllAsync<TextReplacementRule>();

        //-------------------------------------------------------------------------------- TRAINING SESSIONS -------------------------------------------

        public async Task<IEnumerable<TrainingSession>> GetTrainingSessionsAsync() => await _repository.GetAllAsync<TrainingSession>();

        public async Task<TrainingSession> CreateTrainingSessionAsync(TrainingSession session)
        {
            await _repository.AddAsync(session);

            return session;
        }

        //----------------------------------------------------------------------------- TRAINING SESSION CARDS -------------------------------------------

        public async Task<IEnumerable<TrainingSessionCard>> GetTrainingSessionCardsAsync() => await _repository.GetAllAsync<TrainingSessionCard>();

        public async Task<TrainingSessionCard> CreateTrainingSessionCardAsync(TrainingSessionCard trainingCard)
        {
            await _repository.AddAsync(trainingCard);

            return trainingCard;
        }
    }
}
