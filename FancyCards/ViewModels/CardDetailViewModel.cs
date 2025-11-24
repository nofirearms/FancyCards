using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Models;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class CardDetailViewModel : BaseModalViewModel<Card>
    {
        [ObservableProperty]
        private string _title = "Card";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))] 
        private string _frontText;
        partial void OnFrontTextChanged(string value)
        {

        }
        [NotifyCanExecuteChangedFor(nameof(SaveCardCommand))]
        [ObservableProperty]
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


        public CardDetailViewModel(AudioEngine audioEngine)
        {
            _audioSamplerViewModel = new AudioSamplerViewModel(audioEngine);
        }
        [RelayCommand(CanExecute = nameof(CanSaveCard))]
        private void SaveCard()
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
            Close(true, card);
        }
        private bool CanSaveCard() => !string.IsNullOrEmpty(FrontText) && !string.IsNullOrEmpty(BackText);


        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(Cancel);
    }
}
