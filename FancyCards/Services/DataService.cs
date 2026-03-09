using DynamicData;
using FancyCards.Database;
using FancyCards.Helpers;
using FancyCards.Models;
using System.Data;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FancyCards.Services
{
    public class DataService
    {
        private readonly Repository _repository;

        private readonly SourceCache<Card, int> _cardsCache = new(c => c.Id);
        public IObservable<IChangeSet<Card, int>> ConnectCards() => _cardsCache.Connect();

        private readonly SourceCache<Deck, int> _decksCache = new(c => c.Id);
        public IObservable<IChangeSet<Deck, int>> ConnectDecks() => _decksCache.Connect();

        private readonly SourceCache<TextReplacementRule, int> _rulesCache = new(c => c.Id);
        public IObservable<IChangeSet<TextReplacementRule, int>> ConnectRules() => _rulesCache.Connect();

        private List<TrainingSession> _sessions = new List<TrainingSession>();
        private List<TrainingSessionCard> _sessionCards = new List<TrainingSessionCard>();
        private List<ReviewProfile> _reviewProfiles = new List<ReviewProfile>();


        private readonly BehaviorSubject<Deck?> _selectedDeck = new(null);

        // Поток для подписки (только для чтения)
        public IObservable<Deck?> SelectedDeckChanged => _selectedDeck.AsObservable();

        // Свойство для получения/установки
        public Deck? CurrentDeck
        {
            get => _selectedDeck.Value;
            set => _selectedDeck.OnNext(value);
        }

        public DataService(Repository repository)
        {
            _repository = repository;
        }

        public async Task InitializeAsync()
        {
            var cards = await _repository.GetAllAsync<Card>();
            var decks = await _repository.GetAllAsync<Deck>();
            var rules = await _repository.GetAllAsync<TextReplacementRule>();
            var session_cards = await _repository.GetAllAsync<TrainingSessionCard>();
            var sessions = await _repository.GetAllAsync<TrainingSession>();
            var review_profiles = await _repository.GetAllAsync<ReviewProfile>();

            _cardsCache.AddOrUpdate(cards);
            _decksCache.AddOrUpdate(decks);
            _rulesCache.AddOrUpdate(rules);
            _sessions = sessions.ToList() ;
            _sessionCards = session_cards.ToList();
            _reviewProfiles = review_profiles.ToList();

        }


        //-------------------------------------------------------------------------------- SETTINGS ------------------------------------------------------------------------

        #region SETTINGS

        public async Task<IEnumerable<Setting>> GetSettingsAsync() => await _repository.GetAllAsync<Setting>();

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
        public Deck GetDeckById(int id)
        {
            var optional = _decksCache.Lookup(id);
            return optional.HasValue ? optional.Value : null;
        }
        public IEnumerable<Deck> GetDecks() => _decksCache.Items;

        public async Task AddOrUpdateDecksAsync(IEnumerable<Deck> decks)
        {
            await _repository.AddOrUpdateAsync(decks);

            _decksCache.AddOrUpdate(decks);

        }


        #endregion

        //---------------------------------------------------------------------------------- CARDS -------------------------------------------------------------------------

        #region CARDS
        public IEnumerable<Card> GetCardsByDeckId(int deckId) => _cardsCache.Items.Where(c => c.DeckId == deckId).ToList();

        public async Task AddOrUpdateCardsAsync(IEnumerable<Card> cards)
        {
            await _repository.AddOrUpdateAsync(cards);

            _cardsCache.AddOrUpdate(cards);
        }

        public async Task<bool> RemoveCardAsync(Card card)
        {

            _cardsCache.Remove(card);

            var result = await _repository.RemoveAsync(card);
            if (result)
            {
                if (card.Audio != null)
                {
                    var delete_result = PathHelper.DeleteFile(card.Audio.Path);
                    if (!delete_result) result = false;
                }

            }

            return result;
        }

        #endregion

        //--------------------------------------------------------------------------------- REPLACEMENT TEXT RULES ---------------------------------------------------------

        #region REPLACEMENT TEXT RULES

        public IEnumerable<TextReplacementRule> GetTextReplacementRules() => _rulesCache.Items;

        public async Task AddOrUpdateTextReplacementRulesAsync(IEnumerable<TextReplacementRule> rules)
        {
            await _repository.AddOrUpdateAsync(rules);

            _rulesCache.AddOrUpdate(rules);
        }

        public async Task RemoveTextReplacementRuleAsync(TextReplacementRule rule)
        {
            await _repository.RemoveAsync(rule);

            _rulesCache.Remove(rule);           
        }


        #endregion

        //-------------------------------------------------------------------------------- TRAINING SESSIONS ---------------------------------------------------------------

        #region TRAINING SESSIONS

        public IEnumerable<TrainingSession> GetTrainingSessions(int deckId) => _sessions.Where(s => s.DeckId == deckId).ToList();

        public async Task AddOrUpdateTrainingSessionsAsync(IEnumerable<TrainingSession> sessions)
        {
            await _repository.AddOrUpdateAsync(sessions);

            _sessions.AddRange(sessions);        
        }

        #endregion

        //----------------------------------------------------------------------------- TRAINING SESSION CARDS -------------------------------------------------------------

        #region TRAINING SESSION CARDS

        public IEnumerable<TrainingSessionCard> GetTrainingSessionCards() => _sessionCards;


        public async Task AddOrUpdateTrainingSessionCardsAsync(IEnumerable<TrainingSessionCard> trainingCards)
        {
            await _repository.AddOrUpdateAsync(trainingCards);

            _sessionCards.AddRange(trainingCards);
        }

        #endregion

        //--------------------------------------------------------------------------------- REVIEW PROFILES -----------------------------------------------------------------

        #region REVIEW PROFILES

        public IEnumerable<ReviewProfile> GetReivewProfiles() => _reviewProfiles;

        public ReviewProfile GetReivewProfileById(int id) => _reviewProfiles.FirstOrDefault(p => p.Id == id);

        public async Task AddOrUpdateReviewProfileAsync(IEnumerable<ReviewProfile> reviewProfile)
        {
            await _repository.AddOrUpdateAsync(reviewProfile);

            _reviewProfiles.AddRange(reviewProfile);
        }

        #endregion
    }
}
