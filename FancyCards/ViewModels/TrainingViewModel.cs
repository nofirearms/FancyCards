using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class TrainingViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;
        private readonly TextReplacementService _textService;
        private readonly TrainingCardListManager _cardManager;
        private readonly DispatcherTimer _timer;


        [ObservableProperty]
        private TrainingCardViewModel _currentCard;

        [ObservableProperty]
        private double _maxSampleVolume = 0;

        public TrainingViewModel(MainWindowViewModel host, DataService dataService, TextReplacementService textService, AudioEngine audioEngine )
        {
            _host = host;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _textService = textService;

            _audioEngine.MaxSampleVolume += (v) => MaxSampleVolume = v;


            var random = new Random();

            var cards = _dataService.GetCards(1)
                .Where(c => c.NextReviewDate.Date <= DateTime.Now)
                .Where(c => c.State == Models.CardState.Learning || c.State == Models.CardState.Reviewing)
                .OrderBy(c => random.NextDouble())
                .ToArray();

            var learning_cards = cards
                .Where(c => c.State == Models.CardState.Learning)
                .Take(5)
                .ToArray();

            var reviewing_cards = cards
                .Where(c => c.State == Models.CardState.Reviewing)               
                .Take(5)
                .ToArray();

            var training_cards = learning_cards
                .Concat(reviewing_cards)
                .Select(c => new TrainingCardViewModel(c))
                .ToList();

            _cardManager = new TrainingCardListManager(training_cards);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                if (_currentCard != null)
                {
                    _currentCard.OnTimerTick();
                }
            };


            //чтоб аудио воспроизводилось только после загрузки 
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, async () =>
            {
                ShowNextCard();
            });
            

        }

        private async void ShowNextCard()
        {
            

            if (_cardManager.MoveToNextCard())
            {
                CurrentCard = _cardManager.CurrentCard;

                CurrentCard.ShowCount++;

                if(_audioEngine.OpenAudioAsync(_currentCard.Card.Audio.Path, false, true))
                {

                    AudioPlaybackSelected();
                }
                else
                {
                    await _host.OpenMessageBox("Audio file not found\nCard will be removed from list", ["Ok"]);
                    ShowNextCard();
                }
                    
            }
        }


        //---------------------------------------------------------------------- AUDIO ----------------------------------------------------------------
        #region AUDIO
        [RelayCommand]
        private void AudioPlaybackSelected()
        {
            if (_currentCard is null) return;

            _audioEngine.StartPlayback(_currentCard.Card.Audio.StartPosition, _currentCard.Card.Audio.EndPosition, PlaybackSpeed.Full, (float)_currentCard.Card.Audio.Volume, (float)_currentCard.Card.Audio.Tempo);
        }

        [RelayCommand]
        private void AudioPlaybackFull()
        {
            if (_currentCard is null) return;

            _audioEngine.StartPlayback(0, 1, PlaybackSpeed.Full, (float)_currentCard.Card.Audio.Volume, (float)_currentCard.Card.Audio.Tempo);
        }

        [RelayCommand]
        private void AudioPlaybackSlow()
        {
            if (_currentCard is null) return;

            _audioEngine.StartPlayback(_currentCard.Card.Audio.StartPosition, _currentCard.Card.Audio.EndPosition, PlaybackSpeed.Half, (float)_currentCard.Card.Audio.Volume, (float)_currentCard.Card.Audio.Tempo);
        }

        [RelayCommand]
        private void AudioStopPlayback()
        {
            if (_currentCard is null) return;

            _audioEngine.StopPlayback();
        }
        #endregion

        [RelayCommand]
        private async void Accept()
        {
            var answer_result = await _textService.ProcessAndCompareAsync(CurrentCard.Answer, CurrentCard.Card.FrontText);

            if (answer_result)
            {
                CurrentCard.CardStatus = TrainingCardState.Success;
            }
            else
            {
                //из сновного списка
                if(CurrentCard.ShowCount == 1)
                {
                    _cardManager.AddCard(CurrentCard);
                }
                //из списка на повтор
                else
                {
                    if(CurrentCard.Card.State == CardState.Reviewing)
                    {
                        //messagebox с правильным ответом, result = failed
                        CurrentCard.CardStatus = TrainingCardState.Failed;
                        await _host.OpenFailedAnswer(CurrentCard.Answer, CurrentCard.Card.FrontText);
                    }
                    else if(CurrentCard.Card.State == CardState.Learning)
                    {
                        //добавляем ещё раз в список пока нет правильного ответа
                        _cardManager.AddCard(CurrentCard);

                    }
                }
            }

            ShowNextCard();
        }

        [RelayCommand]
        private void CancelTraining()
        {
            _audioEngine.StopPlayback();
            Cancel();
        }
    }
}
