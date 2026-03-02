using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Extensions;
using FancyCards.Models;
using FancyCards.Services;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class CardDetailViewModel : BaseModalViewModel<Card>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private readonly AudioEngine _audioEngine;

        private Card _card;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))] 
        private string _frontText;
        partial void OnFrontTextChanged(string value)
        {

        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))]
        private string _backText;

        [ObservableProperty]
        private string _prefixText;

        [ObservableProperty]
        private string _suffixText;

        [ObservableProperty]
        private string _commentText;

        [ObservableProperty]
        private string _messageText;
        
        [ObservableProperty]
        private AudioSamplerViewModel _audioSamplerViewModel;

        public IEnumerable<CardState> States => new List<CardState>
        {
            CardState.Learning,
            CardState.Reviewing,
        };
        [ObservableProperty]
        private CardState _selectedState = CardState.Learning;

        [ObservableProperty]
        private DateTime _nextReviewDate = DateTime.Now;

        [ObservableProperty]
        private DateTime _dateCreated;

        public CardAction CardAction { get; } = CardAction.Create;

        public CardDetailViewModel(MainWindowViewModel host, AudioEngine audioEngine, DataService dataService, Card card)
        {
            _host = host;
            _dataService = dataService;
            _audioEngine = audioEngine;

            CardAction = card.Id == default ? CardAction.Create : CardAction.Update;

             if (CardAction == CardAction.Create)
            {
                Header = "Create Card";
                _card = card;
            }
            else
            {
                Header = "Edit Card";
                _card = card.Clone();

                FrontText = _card.FrontText;
                BackText = _card.BackText;
                PrefixText = _card.PrefixText;
                SuffixText = _card.SuffixText;
                CommentText = _card.CommentText;
                MessageText = _card.MessageText;
                DateCreated = _card.DateCreated;
                NextReviewDate = _card.NextReviewDate;
                SelectedState = _card.State;
            }

            _audioSamplerViewModel = new AudioSamplerViewModel(_host, _audioEngine, _card);

            _audioEngine.AudioSourceChanged += (source) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    SaveCardCommand?.NotifyCanExecuteChanged();
                });
            };
            _audioEngine.StateChanged += (state) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    SaveCardCommand?.NotifyCanExecuteChanged();
                });
            };


            //чтобы message box открылся после загрузки
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, async () =>
            {
                if(_audioEngine.Duration == TimeSpan.Zero && CardAction == CardAction.Update)
                {
                    await _host.OpenMessageBox("Audio file not found", ["OK"]);
                }
            });
        }

        private async void InitializeAsync()
        {

        }

        private AsyncRelayCommand _saveCardCommand;
        public IAsyncRelayCommand SaveCardCommand => _saveCardCommand ??= new AsyncRelayCommand(SaveCard, CanSaveCard);
        private async Task SaveCard()
        {
            //create
            if (CardAction == CardAction.Create)
            {
                var card = new Card
                {
                    FrontText = FrontText,
                    BackText = BackText,
                    PrefixText = PrefixText,
                    SuffixText = SuffixText,
                    CommentText = CommentText,
                    MessageText = MessageText,
                    DateCreated = DateTime.Now,
                    NextReviewDate = NextReviewDate.Date,
                    State = SelectedState,
                    //Scores = new CardScores
                    //{
                    //    EF = _host.Deck.Deck.Settings.ReviewProfile.StartEF
                    //}
                };
                var audio_source = new AudioSource
                {
                    Path = $"audio/{card.DateCreated:ddMMyyyy_HHmmss}.mp3",
                    EndPosition = _audioSamplerViewModel.Selection.End,
                    StartPosition = _audioSamplerViewModel.Selection.Start,
                    Tempo = _audioSamplerViewModel.Tempo,
                    Volume = _audioSamplerViewModel.Volume
                };

                card.Audio = audio_source;

                _card = card;

                await _host.StartLoading(false);

                await _dataService.CreateCardAsync(_host.Deck.Id, _card);
                await _audioEngine.RenderToMp3Async(_card.Audio.Path);

                _host.StopLoading();

                Close(true, _card);
            }
            //edit
            else
            {
                _card.FrontText = FrontText;
                _card.BackText = BackText;
                _card.PrefixText = PrefixText;
                _card.SuffixText = SuffixText;
                _card.CommentText = CommentText;
                _card.MessageText = MessageText;
                _card.NextReviewDate = NextReviewDate.Date;
                _card.State = SelectedState;

                _card.Audio.Volume = _audioSamplerViewModel.Volume;
                _card.Audio.StartPosition = _audioSamplerViewModel.Selection.Start;
                _card.Audio.EndPosition = _audioSamplerViewModel.Selection.End;
                _card.Audio.Tempo = _audioSamplerViewModel.Tempo;

                await _host.StartLoading(false);

                await _dataService.UpdateCardAsync(_card);

                if (_audioEngine.AudioChanged)
                {
                    await _audioEngine.RenderToMp3Async(_card.Audio.Path);
                }

                _host.StopLoading();

                Close(true, _card);
            }

            _audioEngine.Dispose();

        }
        private bool CanSaveCard() => !string.IsNullOrEmpty(FrontText)
            && !string.IsNullOrEmpty(BackText)
            && _audioSamplerViewModel.AudioDuration != TimeSpan.Zero
            && _audioSamplerViewModel.AudioSamplerState != State.Initial && _audioSamplerViewModel.AudioSamplerState != State.Recording;


        [RelayCommand]
        private new void Cancel()
        {
            _audioSamplerViewModel.Dispose();
            _audioEngine.Dispose();

            base.Cancel();
        }
    }
}
