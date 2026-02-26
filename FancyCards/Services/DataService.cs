using FancyCards.Database;
using FancyCards.Helpers;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Policy;
using System.Text;

namespace FancyCards.Services
{
    public class DataService
    {
        public event Action<DeckEventArgs> DeckEvent;
        public event Action<CardsEventArgs> CardEvent;
        public event Action<TextReplacementRuleEventArgs> TextReplacementRuleEvent;

        private readonly Repository _repository;

        public DataService(Repository repository)
        {
            _repository = repository;
        }


        

        //-------------------------------------------------------------------------------- SETTINGS ------------------------------------------------------------------------

        #region SETTINGS

        public async Task<IEnumerable<Setting>> GetSettingsAsync() => (await _repository.GetAllAsync<Setting>());

        public async Task<bool> SaveSettingsAsync(IEnumerable<Setting> settings)
        {
            try
            {
                var db_settings = await GetSettingsAsync();
                foreach (var setting in settings)
                {
                    var db_setting = db_settings.FirstOrDefault(s => s.Key == setting.Key);
                    if (db_setting is null)
                    {

                    }
                    else
                    {
                        setting.Id = db_setting.Id;
                    }
                }

                await _repository.AddOrUpdateAsync(settings);
                return true;
            }
            catch
            {
                return false;
            }



        }

        #endregion

        //---------------------------------------------------------------------------------- DECKS -------------------------------------------------------------------------

        #region DECKS
        public async Task<Deck> GetDeckByIdAsync(int id) => await _repository.GetDeckAsync(id);
        public async Task<IEnumerable<Deck>> GetDecksAsync() => await _repository.GetAllDecksAsync();

        public async Task<IEnumerable<Deck>> AddOrUpdateDecks(IEnumerable<Deck> decks, DeckAction deckAction)
        {
            await _repository.AddOrUpdateAsync(decks);

            DeckEvent?.Invoke(new DeckEventArgs(decks, deckAction));

            return decks;
        }

 
        #endregion

        //---------------------------------------------------------------------------------- CARDS -------------------------------------------------------------------------

        #region CARDS
        public async Task<IEnumerable<Card>> GetCardsAsync(int deckId) => (await GetDeckByIdAsync(deckId)).Cards;

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

        public async Task<IEnumerable<Card>> UpdateCardsAsync(IEnumerable<Card> cards)
        {
            await _repository.AddOrUpdateAsync(cards);

            CardEvent?.Invoke(new CardsEventArgs(cards, CardAction.Update));

            return cards;
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

        #endregion

        //--------------------------------------------------------------------------------- REPLACEMENT TEXT RULES ---------------------------------------------------------

        #region REPLACEMENT TEXT RULES

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

        public async Task<IEnumerable<TextReplacementRule>> AddOrUpdateTextReplacementRuleAsync(IEnumerable<TextReplacementRule> rules)
        {
            await _repository.AddOrUpdateAsync(rules);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs(rules, CardAction.Create));

            return rules;
        }

        public async Task<TextReplacementRule> RemoveTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.RemoveAsync(rule);

            TextReplacementRuleEvent?.Invoke(new TextReplacementRuleEventArgs([rule], CardAction.Remove));
            return rule;
        }

        public async Task<IEnumerable<TextReplacementRule>> GetTextReplacementRules() => await _repository.GetAllAsync<TextReplacementRule>();

        #endregion

        //-------------------------------------------------------------------------------- TRAINING SESSIONS ---------------------------------------------------------------

        #region TRAINING SESSIONS

        public async Task<IEnumerable<TrainingSession>> GetTrainingSessionsAsync() => await _repository.GetAllAsync<TrainingSession>();

        public async Task<TrainingSession> CreateTrainingSessionAsync(TrainingSession session)
        {
            await _repository.AddAsync(session);

            return session;
        }

        #endregion

        //----------------------------------------------------------------------------- TRAINING SESSION CARDS -------------------------------------------------------------

        #region TRAINING SESSION CARDS

        public async Task<IEnumerable<TrainingSessionCard>> GetTrainingSessionCardsAsync() => await _repository.GetAllAsync<TrainingSessionCard>();

        public async Task<IEnumerable<TrainingSessionCard>> GetTrainingSessionCardsAsync(IEnumerable<int> cardIds) => (await _repository.GetAllAsync<TrainingSessionCard>()).Where(c => cardIds.Contains(c.CardId));

        public async Task<TrainingSessionCard> CreateTrainingSessionCardAsync(TrainingSessionCard trainingCard)
        {
            await _repository.AddAsync(trainingCard);

            return trainingCard;
        }

        #endregion
    }
}
