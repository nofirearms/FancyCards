using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Extensions;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class DeckDetailViewModel : BaseModalViewModel<Deck>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private Deck _deck;

        public DeckAction DeckAction { get; }


        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        [ObservableProperty]

        private string _name;
        partial void OnNameChanged(string value)
        {
            
        }

        [ObservableProperty]
        private string _description;

        //---------------------------------------------------------------- SETTINGS ----------------------------------------------------------------------------
        [ObservableProperty]
        private int _trainingLearnCards = 5;
        [ObservableProperty]
        private int _trainingReviewCards = 5;
        [ObservableProperty]
        private int _correctAnswersToFinishLearning = 2;
        [ObservableProperty]
        private int _correctAnswersToFinishReviewing = 50;


        public DeckDetailViewModel(MainWindowViewModel host, DataService dataService, Deck deck) 
        {
            _host = host;
            _dataService = dataService;

            DeckAction = deck.Id == default ? DeckAction.Create : DeckAction.Update;

            if (DeckAction == DeckAction.Create)
            {
                Header = "Create Deck";
                _deck = deck;
            }
            else if(DeckAction == DeckAction.Update) 
            {
                Header = "Edit Deck";
                _deck = deck.Clone();

                _name = _deck.Name;
                _description = _deck.Description;

                _trainingLearnCards = _deck.Settings.TrainingLearnCards;
                _trainingReviewCards = _deck.Settings.TrainingReviewCards;
                _correctAnswersToFinishLearning = _deck.Settings.СorrectAnswersToFinishLearning;
                _correctAnswersToFinishReviewing = _deck.Settings.СorrectAnswersToFinishReviewing;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async void Save()
        {

            _deck.Name = Name;
            _deck.Description = Description;
            _deck.Cards = new List<Card>();

            _deck.Settings.TrainingLearnCards = TrainingLearnCards;
            _deck.Settings.TrainingReviewCards = TrainingReviewCards;
            _deck.Settings.СorrectAnswersToFinishLearning = CorrectAnswersToFinishLearning;
            _deck.Settings.СorrectAnswersToFinishReviewing = CorrectAnswersToFinishReviewing;

            await _host.StartLoading(false);

            //сначала сохраняем чтобы получить Id
            await _dataService.AddOrUpdateDecks([_deck], DeckAction);

            _host.StopLoading();

            Close(true, _deck, "Save");
            
        }
        private bool CanSave() => !string.IsNullOrEmpty(_name);
    }
}
