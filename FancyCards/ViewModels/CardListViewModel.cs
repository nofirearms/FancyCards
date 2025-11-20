using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using FancyCards.Models;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class CardListViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        [ObservableProperty]
        private ReadOnlyObservableCollection<Card> _cards;
        
        private SourceCache<Card, int> _sourceCache;
        public CardListViewModel(DataService dataService)
        {
            _dataService = dataService;
            //_cards = new ObservableCollection<Card>(_decks.First().Cards ?? new List<Card>());
            _sourceCache = new SourceCache<Card, int>(o => o.Id);
            _sourceCache.AddOrUpdate(_dataService.GetCards(1) ?? new List<Card>());

            _sourceCache.Connect()
            .Filter(CreateFilter())
            .Bind(out _cards)
            .Subscribe();
            
            _dataService.CardEvent += OnCardEvent; 
        }

        private void OnCardEvent(CardsEventArgs args)
        {
            var cards = args.Cards;
            var action = args.Action;

            if(args.Action == CardAction.Create)
            {
                foreach(var card in cards)
                {
                    _sourceCache.AddOrUpdate(card);
                }
            }
        }


        //--------------------------------------------------------------------------------------------FILTER--------------------------------------------------------------
        #region FILTER

        private string _frontTextFilter;
        public string FrontTextFilter
        {
            get => _frontTextFilter;
            set
            {
                SetProperty(ref _frontTextFilter, value);
                UpdateFilter();
            }
        }

        private Func<Card, bool> CreateFilter()
        {
            return item =>
            {
                var front_pass = string.IsNullOrEmpty(FrontTextFilter) ||
                              item.FrontText.Contains(FrontTextFilter);

                return front_pass; //&& categoryPass && pricePass;
            };
        }

        private void UpdateFilter()
        {
            // DynamicData автоматически применит новый фильтр
            _sourceCache.Refresh(); // Перефильтрует существующие данные
        }

        #endregion
    }
}




