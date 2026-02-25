using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows.Input;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class TrainingViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;
        private readonly TextReplacementService _textService;
        private readonly SettingsService _settingsService;
        private readonly OverlayService _overlayService;

        private TrainingCardListManager _cardManager;
        private DispatcherTimer _timer;

        [ObservableProperty]
        private int _totalCards;

        [ObservableProperty]
        private int _currentCardIndex;


        [ObservableProperty]
        private TrainingCardViewModel _currentCard;

        [ObservableProperty]
        private TimeSpan _trainingTime = TimeSpan.Zero;

        [ObservableProperty]
        private double _maxSampleVolume = 0;

        public IEnumerable<Difficulty> Difficulties => Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>();

        public TrainingViewModel(MainWindowViewModel host,
            DataService dataService,
            TextReplacementService textService,
            AudioEngine audioEngine,
            SettingsService settingsService,
            HotkeyService hotkeyService,
            OverlayService overlayService,
            IEnumerable<Card> cards )
        {
            _host = host;
            _dataService = dataService;
            _audioEngine = audioEngine;
            _textService = textService;
            _settingsService = settingsService;
            _overlayService = overlayService;

            Header = "Training";

            _audioEngine.MaxSampleVolume += (v) => MaxSampleVolume = v;

            _cardManager = new TrainingCardListManager(cards.Select(c => new TrainingCardViewModel(c)));

            hotkeyService.RegisterHotkey<TrainingViewModel>(Key.Enter, ModifierKeys.None, AcceptCommand);
            hotkeyService.RegisterHotkey<TrainingViewModel>(Key.F1, ModifierKeys.None, AudioPlaybackSelectedCommand);
            hotkeyService.RegisterHotkey<TrainingViewModel>(Key.F2, ModifierKeys.None, AudioPlaybackFullCommand);
            hotkeyService.RegisterHotkey<TrainingViewModel>(Key.F3, ModifierKeys.None, AudioPlaybackSlowCommand);
            hotkeyService.RegisterHotkey<TrainingViewModel>(Key.F4, ModifierKeys.None, AudioStopPlaybackCommand);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                if (_currentCard != null)
                {
                    _currentCard.OnTimerTick();
                    TrainingTime = TrainingTime.Add(TimeSpan.FromSeconds(1));
                }
            };

            //чтоб аудио воспроизводилось только после загрузки 
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, async () =>
            {
                ShowNextCard();
                _timer.Start();
            });

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            
        }

        private async void ShowNextCard()
        {
            

            if (_cardManager.MoveToNextCard())
            {
                CurrentCard = _cardManager.CurrentCard;

                CurrentCard.ShowCount++;

                CurrentCardIndex = _cardManager.CardsShown;
                TotalCards = _cardManager.TotalCards;

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
            else
            {
                await FinishTraining();
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
            if (string.IsNullOrEmpty(CurrentCard.Answer)) return;

            var answer_result = await _textService.ProcessAndCompareAsync(CurrentCard.Answer, CurrentCard.Card.FrontText);

            if (answer_result)
            {
                await _overlayService.ShowAndHideAsync(OverlayType.Success, 500);
                if (!CurrentCard.Hint)
                {
                    CurrentCard.CardStatus = TrainingCardState.Success;
                }
                else
                {
                    CurrentCard.CardStatus = TrainingCardState.Failed;
                }
            }
            else
            {
                await _overlayService.ShowAndHideAsync(OverlayType.Error, 500);
                //из сновного списка
                if (CurrentCard.ShowCount == 1)
                {
                    _cardManager.AddCard(CurrentCard);
                }
                //из списка на повтор
                else
                {
                    CurrentCard.CardStatus = TrainingCardState.Failed;
                    await _host.OpenFailedAnswer(CurrentCard.Answer, CurrentCard.Card.FrontText);
                    //if(CurrentCard.Card.State == CardState.Reviewing)
                    //{
                    //    //messagebox с правильным ответом, result = failed
                    //    CurrentCard.CardStatus = TrainingCardState.Failed;
                    //    await _host.OpenFailedAnswer(CurrentCard.Answer, CurrentCard.Card.FrontText);
                    //}
                    //else if(CurrentCard.Card.State == CardState.Learning)
                    //{
                    //    //добавляем ещё раз в список пока нет правильного ответа
                    //    _cardManager.AddCard(CurrentCard);

                    //}
                }
            }

            ShowNextCard();
        }

        private async Task FinishTraining()
        {
            StopTraining();

            var result_cards = _cardManager.BaseCards;

            var session_cards = new List<TrainingSessionCard>();

            //var card_trainings = await _dataService.GetTrainingSessionCardsAsync(result_cards.Select(o => o.Card.Id));

            foreach (var card in result_cards)
            {
                card.Card.Scores.TotalCount++;
                card.Card.LastReviewDate = DateTime.Now;
                card.Card.TimeSpent = card.TotalTimeSpent;
                card.Card.Difficulty = card.Difficulty;

                if (card.CardStatus == TrainingCardState.Success)
                {
                    card.Card.Scores.CorrectCount++;

                    if (card.Card.State == CardState.Learning)
                    {
                        if (card.Card.Scores.CorrectCount >= _host.Deck.Deck.Settings.СorrectAnswersToFinishLearning)
                        {
                            card.Card.State = CardState.Reviewing;
                            ProcessScore(card);
                            card.Card.NextReviewDate = DateTime.Now.Date.AddDays(card.Card.Scores.I);
                        }
                        else
                        {
                            card.Card.NextReviewDate = DateTime.Now.Date;
                        }
                    }
                    else if (card.Card.State == CardState.Reviewing)
                    {
                        ProcessScore(card);
                        card.Card.NextReviewDate = DateTime.Now.Date.AddDays(card.Card.Scores.I);
                    }
                }
                else if (card.CardStatus == TrainingCardState.Failed)
                {

                    if (card.Card.State == CardState.Learning)
                    {

                    }
                    else if (card.Card.State == CardState.Reviewing)
                    {
                        ProcessScore(card);
                        card.Card.NextReviewDate = DateTime.Now.Date.AddDays(card.Card.Scores.I);
                    }
                }

                session_cards.Add(new TrainingSessionCard()
                {
                    CardId = card.Card.Id,
                    CardState = card.InitialState,
                    Date = DateTime.Now,
                    Difficulty = card.Difficulty,
                    Result = card.CardStatus == TrainingCardState.Success ? TrainingCardResult.Success : TrainingCardResult.Failed,
                    TimeSpent = card.SessionTimeSpent,
                });

            }

            var training_session = new TrainingSession
            {
                Cards = session_cards,
                Date = DateTime.Now,
                TimeSpent = TrainingTime
            };

            await _host.StartLoading(false);

            await _dataService.CreateTrainingSessionAsync(training_session);

            await _dataService.UpdateCardsAsync(result_cards.Select(r => r.Card));

            _host.StopLoading();

            await _host.OpenTrainingResult(result_cards);

            Close();

        }

        private void ProcessScore(TrainingCardViewModel card)
        {
            if(card.CardStatus == TrainingCardState.Failed)
            {
                card.Card.Scores.Reps = 0;
                card.Card.Scores.I = 1;

                card.Card.Scores.EF = Math.Clamp(card.Card.Scores.EF - 0.25, 1.3, 2.8);
            }
            else
            {
                if(card.Difficulty == Difficulty.Hard)
                {
                    card.Card.Scores.EF = Math.Clamp(card.Card.Scores.EF - 0.15, 1.3, 2.8);
                }
                else if(card.Difficulty == Difficulty.Normal)
                {
                    card.Card.Scores.EF = Math.Clamp(card.Card.Scores.EF - 0.05 , 1.3, 2.8);
                }
                else if (card.Difficulty == Difficulty.Easy)
                {
                    card.Card.Scores.EF = Math.Clamp(card.Card.Scores.EF + 0.08, 1.3, 2.8);
                }

                if (card.Card.Scores.Reps == 0)
                {
                    card.Card.Scores.I = 1;
                }
                else if (card.Card.Scores.Reps == 1)
                {
                    card.Card.Scores.I = 6;
                }
                else if (card.Card.Scores.Reps >= 2)
                {
                    card.Card.Scores.I = (int)Math.Round(card.Card.Scores.I * card.Card.Scores.EF, MidpointRounding.AwayFromZero);
                }

                card.Card.Scores.Reps++;
            }
        }



        protected override async void Cancel()
        {
            var result = await _host.OpenMessageBox("Exit training? Progress won't be saved.", ["Yes", "No"]);
            if(result.ButtonTag == "Yes")
            {
                StopTraining();
                base.Cancel();
            }
        }

        private void StopTraining()
        {
            _audioEngine.StopPlayback();
            _timer.Stop();
        }
    }
}
