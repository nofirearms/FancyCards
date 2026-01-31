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


        public CardDetailViewModel(DataService dataService, Card card = null)
        {
            _dataService = dataService;

            _card = card == null ? null : card.Clone();
            

            if(card == null)
            {
                Title = "Create Card";
            }
            else
            {
                Title = "Edit Card";

                FrontText = card.FrontText;
                BackText = card.BackText;
                PrefixText = card.PrefixText;
                SuffixText = card.SuffixText;
                CommentText = card.CommentText;
                MessageText = card.MessageText;
            }

            _audioSamplerViewModel = new AudioSamplerViewModel(card);

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
            //create
            if(_card == null)
            {
                var card = new Card
                {
                    FrontText = FrontText,
                    BackText = BackText,
                    PrefixText = PrefixText,
                    SuffixText = SuffixText,
                    CommentText = CommentText,
                    MessageText = MessageText,
                    DateCreated = DateTime.Now

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
        private bool CanSaveCard() => !string.IsNullOrEmpty(FrontText) && !string.IsNullOrEmpty(BackText) && _audioSamplerViewModel.AudioDuration != TimeSpan.Zero;


        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(Cancel);
    }
}
