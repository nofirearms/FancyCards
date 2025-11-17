using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FancyCards.ViewModels
{
    public class CardViewViewModel : BaseModalViewModel<Card>
    {

        private string _frontText;
        public string FrontText
        {
            get => _frontText;
            set
            {
                SetProperty(ref _frontText, value);
            }
        }

        private string _backText;
        public string BackText
        {
            get => _backText;
            set
            {
                SetProperty(ref _backText, value);
            }
        }

        private string _prefixText;
        public string PrefixText
        {
            get => _prefixText;
            set
            {
                SetProperty(ref _prefixText, value);
            }
        }

        private string _suffixText;
        public string SuffixText
        {
            get => _suffixText;
            set
            {
                SetProperty(ref _suffixText, value);
            }
        }

        private string _commentText;
        public string CommentText
        {
            get => _commentText;
            set
            {
                SetProperty(ref _commentText, value);
            }
        }

        public CardViewViewModel()
        {
            
        }

        private RelayCommand _acceptCommand;
        public RelayCommand AcceptCommand => _acceptCommand ??= new RelayCommand(() =>
        {
            var card = new Card
            {
                FrontText = _frontText, 
                BackText = _backText,
                PrefixText = _prefixText,
                SuffixText = _suffixText,
                Comment = _commentText, 
                DateCreated = DateTime.Now
            };
            Close(true, card);
        });
    }
}
