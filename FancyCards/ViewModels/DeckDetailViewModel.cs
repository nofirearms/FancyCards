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
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async void Save()
        {
            //if(DeckAction == DeckAction.Create)
            //{

            //}
            //else if(DeckAction == DeckAction.Update)
            //{

            //}

            _deck.Name = Name;
            _deck.Description = Description;

            await _dataService.AddOrUpdateDecks([_deck], DeckAction);
        }
        private bool CanSave() => !string.IsNullOrEmpty(_deck.Name);
    }
}
