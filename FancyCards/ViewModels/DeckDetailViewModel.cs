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
        private readonly DataService _dataService;
        private readonly LoadingService _loadingService;

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

        [ObservableProperty]
        private IEnumerable<ReviewProfile> _profiles;

        [ObservableProperty]
        private ReviewProfile _selectedProfile;

        //---------------------------------------------------------------- SETTINGS ----------------------------------------------------------------------------
        [ObservableProperty]
        private int _trainingLearnCards = 5;
        [ObservableProperty]
        private int _trainingReviewCards = 5;
        [ObservableProperty]
        private int _correctAnswersToFinishLearning = 2;
        [ObservableProperty]
        private int _maxIntervalDays = 90;


        public DeckDetailViewModel(DataService dataService, LoadingService loadingService, Deck deck) 
        {
            _dataService = dataService;
            _loadingService = loadingService;

            DeckAction = deck.Id == default ? DeckAction.Create : DeckAction.Update;

            Profiles = _dataService.GetReivewProfiles();

            if (DeckAction == DeckAction.Create)
            {
                Header = "Create Deck";
                _deck = deck;

                _selectedProfile = Profiles.FirstOrDefault();
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
                _maxIntervalDays = _deck.Settings.MaxIntervalDays;
                _selectedProfile = Profiles.FirstOrDefault(p => p.Id == _deck.Settings.ReviewProfileId);
            }

            var _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            
        }

        private AsyncRelayCommand _saveCommand;
        public IAsyncRelayCommand SaveCommand => _saveCommand ??= new AsyncRelayCommand(Save, CanSave);
        private async Task Save()
        {
            _deck.Name = Name;
            _deck.Description = Description;

            _deck.Settings.TrainingLearnCards = TrainingLearnCards;
            _deck.Settings.TrainingReviewCards = TrainingReviewCards;
            _deck.Settings.СorrectAnswersToFinishLearning = CorrectAnswersToFinishLearning;
            _deck.Settings.MaxIntervalDays = MaxIntervalDays;

            _deck.Settings.ReviewProfileId = SelectedProfile.Id;

            await _loadingService.ShowLoadingAsync(async () =>
            {
                await _dataService.AddOrUpdateDecksAsync([_deck]);
            }, true, false);

            Close(true, _deck, "Save");
            
        }
        private bool CanSave() => !string.IsNullOrEmpty(_name);
    }
}
