using FancyCards.ViewModels;


namespace FancyCards.Services
{
    public class TrainingCardListManager
    {
        private List<TrainingCardViewModel> _baseCards = new();
        private List<TrainingCardViewModel> _sessionCards = new();

        private int _currentIndex = -1;

        public TrainingCardViewModel CurrentCard => (_currentIndex >= 0 && _currentIndex < _sessionCards.Count) ? _sessionCards[_currentIndex] : null;

        public TrainingCardListManager(IEnumerable<TrainingCardViewModel> cards) 
        {
            _baseCards = cards?.ToList() ?? new List<TrainingCardViewModel>();
            _sessionCards = cards?.ToList() ?? new List<TrainingCardViewModel>();

            _currentIndex = -1;
        }

        public void AddCard(TrainingCardViewModel card)
        {
            _sessionCards.Add(card);
        }

        public bool MoveToNextCard()
        {
            if (_currentIndex + 1 >= _sessionCards.Count)
                return false;

            _currentIndex++;
            return true;
        }
        public IEnumerable<TrainingCardViewModel> BaseCards => _baseCards;
        public bool HasMoreCards => _currentIndex + 1 < _sessionCards.Count;
        public int TotalCards => _sessionCards.Count;
        public int CardsShown => _currentIndex + 1;
        public int CardsRemaining => Math.Max(0, _sessionCards.Count - (_currentIndex + 1));
    }
}
