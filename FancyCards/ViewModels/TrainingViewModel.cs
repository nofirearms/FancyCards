using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        private const double TOLERANCE = 0.13;
        private const double PUNISH_RATIO = 3;

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

        //----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private AsyncRelayCommand _acceptCommand;
        public IAsyncRelayCommand AcceptCommand => _acceptCommand ??= new AsyncRelayCommand(Accept);
        private async Task Accept()
        {
            if (string.IsNullOrEmpty(CurrentCard.Answer)) return;

            var answer_result = await _textService.ProcessAndCompareAsync(CurrentCard.Answer, CurrentCard.Card.FrontText);

            if (answer_result)
            {
                await _overlayService.ShowAndHideAsync(OverlayType.Success, 500);
                if (!CurrentCard.Hint)
                {
                    CurrentCard.CardStatus = TrainingCardState.Success;
                    if (CurrentCard.Card.Scores.I >= _host.Deck.Deck.Settings.MaxIntervalDays)
                    {
                        await _overlayService.ShowAndHideAsync(OverlayType.Archived, 1000);
                        //await _host.OpenMessageBox("Card moved to archive!", ["Ok"], "Congratulations!", new SolidColorBrush(Colors.GreenYellow));
                    }


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
                card.Card.TotalTimeSpent = card.TotalTimeSpent;
                card.Card.Difficulty = card.Difficulty;

                if (card.CardStatus == TrainingCardState.Success)
                {
                    card.Card.Scores.CorrectCount++;

                    if (card.Card.State == CardState.Learning)
                    {
                        //learning -> reviewing
                        if (card.Card.Scores.CorrectCount >= _host.Deck.Deck.Settings.СorrectAnswersToFinishLearning)
                        {
                            card.Card.State = CardState.Reviewing;
                            ProcessScore(card);
                        }
                        else
                        {
                            card.Card.NextReviewDate = DateTime.Now.Date;
                        }
                    }
                    else if (card.Card.State == CardState.Reviewing)
                    {
                        //считается выученной
                        if(card.Card.Scores.I >= _host.Deck.Deck.Settings.MaxIntervalDays)
                        {
                            card.Card.State = CardState.Archived;
                        }
                        else
                        {
                            ProcessScore(card);
                        }

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
                    }
                }

                session_cards.Add(new TrainingSessionCard()
                {
                    CardId = card.Card.Id,
                    CardState = card.InitialState,
                    Date = DateTime.Now,
                    Difficulty = card.Difficulty,
                    Result = card.CardStatus == TrainingCardState.Success ? TrainingCardResult.Success : TrainingCardResult.Failed,
                    Duration = card.SessionDuration,
                });

            }

            var training_session = new TrainingSession
            {
                Cards = session_cards,
                Date = DateTime.Now,
                Duration = TrainingTime
            };

            await _host.StartLoading(false);

            //TrainingSessionCards записываются автоматом через сессию
            await _dataService.CreateTrainingSessionAsync(training_session);

            await _dataService.UpdateCardsAsync(result_cards.Select(r => r.Card));

            _host.StopLoading();

            await _host.OpenTrainingResult(result_cards);

            Close();

        }

        //При ошибке или подсказке повторяем карточку на следующий день, I сбрасывается на четверть
        //При правильном ответе следующий повтор через I дней
        private void ProcessScore(TrainingCardViewModel card)
        {
            var profile = _host.Deck.Deck.Settings.ReviewProfile;

            double ef = 0;
            int second_interval = 0;

            if (card.Difficulty == Difficulty.Hard)
            {
                ef = profile.HardEF;
                second_interval = profile.HardSecondRepetitionInterval;
            }
            else if (card.Difficulty == Difficulty.Normal)
            {
                ef = profile.NormalEF;
                second_interval = profile.NormalSecondRepetitionInterval;
            }
            else if (card.Difficulty == Difficulty.Easy)
            {
                ef = profile.EasyEF;
                second_interval = profile.EasySecondRepetitionInterval;
            }

            if (card.CardStatus == TrainingCardState.Failed)
            {

                //card.Card.Scores.I = Math.Max(1, card.Card.Scores.I - (int)Math.Ceiling(card.Card.Scores.I / PUNISH_RATIO));

               //card.Card.Scores.I = Math.Max(1, card.Card.Scores.I / 2);

               //откат на 2 шага
                card.Card.Scores.I = (int)Math.Max(1, Math.Ceiling(Math.Ceiling(card.Card.Scores.I / ef)) / ef);
                card.Card.Scores.Error = true;
                //повторяем на следующий день
                card.Card.NextReviewDate = DateTime.Now.Date.AddDays(1);
            }
            else
            {

                if (card.Card.Scores.I == 0)
                {
                    card.Card.Scores.I = 1;
                }
                else 
                {
                    //суть алгоритма: При штрафах, смене сложности или смене алгоритма могут появляться интервалы дней, которые не соответствуют формулам,
                    //поэтому мы ищем ближайшее число из формулы дат, которое меньше I и уже дальше его прогоняем через основную формулу рачёта дат
                    var i = second_interval;
                    while ((int)Math.Ceiling(i * ef) <= card.Card.Scores.I)
                    {
                        i = (int)Math.Ceiling(i * ef);
                    }
                    //делаем расчёт по формулам, только если не было ошибки в предыдущей попытке
                    if (!card.Card.Scores.Error)
                    {
                        i = (int)Math.Ceiling(i * ef);
                    }

                    card.Card.Scores.I = i;
                    card.Card.Scores.Error = false;
                    //после этого карточка будет выучена
                    //делаем допуск - _host.Deck.Deck.Settings.MaxIntervalDays * TOLERANCE, чтобы если значение рядом, то тоже засчитывалось
                    if (card.Card.Scores.I > _host.Deck.Deck.Settings.MaxIntervalDays - _host.Deck.Deck.Settings.MaxIntervalDays * TOLERANCE)
                        card.Card.Scores.I = _host.Deck.Deck.Settings.MaxIntervalDays;
                }

                card.Card.Scores.Reps++;

                card.Card.NextReviewDate = DateTime.Now.Date.AddDays(card.Card.Scores.I);
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
