using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Extensions;
using FancyCards.Models;
using FancyCards.Services;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class CardDetailViewModel : BaseModalViewModel<Card>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private Card _card;


        [ObservableProperty]
        private string _title = "Card";

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

        public IEnumerable<CardState> States => Enum.GetValues<CardState>().Cast<CardState>();
        [ObservableProperty]
        private CardState _selectedState = CardState.Learning;

        [ObservableProperty]
        private DateTime _nextReviewDate = DateTime.Now;

        [ObservableProperty]
        private DateTime _dateCreated;


        public CardAction CardAction { get; } = CardAction.Create;

        public CardDetailViewModel(MainWindowViewModel host, DataService dataService, Card card)
        {
            _host = host;
            _dataService = dataService;

            CardAction = card.Id == default ? CardAction.Create : CardAction.Update;

             if (CardAction == CardAction.Create)
            {
                Title = "Create Card";
                _card = card;
            }
            else
            {
                Title = "Edit Card";
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

            _audioSamplerViewModel = new AudioSamplerViewModel(_host, _card);
            //_audioEngine.AudioDurationChanged += (_) =>
            //{
            //    App.Current.Dispatcher.Invoke(() =>
            //    {
            //        SaveCardCommand?.NotifyCanExecuteChanged();
            //    });
            //};
        }



        [RelayCommand(CanExecute = nameof(CanSaveCard))]
        private async void SaveCard()
        {
            _audioSamplerViewModel.StopPlaybackCommand.Execute(null);

            //create
            if(CardAction == CardAction.Create)
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

                await _dataService.CreateCardAsync(1, _card);
                await _audioSamplerViewModel.RenderAudioToMp3Async(_card.Audio.Path);

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
                _card.Audio.Volume = _audioSamplerViewModel.Volume;
                _card.Audio.StartPosition = _audioSamplerViewModel.Selection.Start;
                _card.Audio.EndPosition = _audioSamplerViewModel.Selection.End;
                _card.Audio.Tempo = _audioSamplerViewModel.Tempo;

                await _dataService.UpdateCardAsync(1, _card);

                if (_audioSamplerViewModel.AudioSourceChanged)
                {
                    await _audioSamplerViewModel.RenderAudioToMp3Async(_card.Audio.Path);
                }

                Close(true, _card);
            }

        }
        private bool CanSaveCard() => !string.IsNullOrEmpty(FrontText) && !string.IsNullOrEmpty(BackText) /*&& _audioSamplerViewModel.AudioDuration != TimeSpan.Zero*/;


        [RelayCommand]
        private new void Cancel()
        {
            _audioSamplerViewModel.StopPlaybackCommand.Execute(null);
            base.Cancel();
        }
    }
}
