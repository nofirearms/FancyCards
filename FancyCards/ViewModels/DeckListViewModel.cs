using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Xml.Linq;

namespace FancyCards.ViewModels
{
    public partial class DeckListViewModel : BaseModalViewModel<Deck>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private ReadOnlyObservableCollection<DeckSummaryViewModel> _decks;

        [NotifyCanExecuteChangedFor(nameof(SelectCommand))]
        [ObservableProperty]
        
        private DeckSummaryViewModel _selectedDeck;

        private SourceCache<DeckSummaryViewModel, int> _sourceCache;

        public DeckListViewModel(MainWindowViewModel host, DataService dataService, SettingsService settingsService)
        {

            _host = host;
            _dataService = dataService;
            _settingsService = settingsService;

            Header = "Decks";

            _dataService.DeckEvent += OnDeckEvent;

            var _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var db_decks = await _dataService.GetDecksAsync();
            var selected_id = _host.Deck.Id;

            _sourceCache = new SourceCache<DeckSummaryViewModel, int>(o => o.Id);
            _sourceCache.AddOrUpdate(db_decks.Select(d => new DeckSummaryViewModel(d)) ?? new List<DeckSummaryViewModel>());

            _sourceCache.Connect()
                .Filter(CreateFilter())
                .Bind(out _decks)
                .Subscribe();

            SelectedDeck = _decks.FirstOrDefault(d => d.Id == selected_id);
        }

        private void OnDeckEvent(DeckEventArgs args)
        {
            var decks = args.Decks;
            var action = args.Action;

            if (args.Action == DeckAction.Create)
            {
                foreach (var deck in decks)
                {
                    var d = new DeckSummaryViewModel(deck);
                    _sourceCache.AddOrUpdate(d);
                    SelectedDeck = d;
                }
            }
            else if (args.Action == DeckAction.Remove)
            {
                foreach (var deck in decks)
                {
                    var d = Decks.FirstOrDefault(o => o.Deck.Id == deck.Id);
                    if (d != null)
                    {
                        _sourceCache.Remove(d);
                    }

                }
            }
            else if (args.Action == DeckAction.Update)
            {
                foreach (var deck in decks)
                {
                    var d = Decks.FirstOrDefault(o => o.Deck.Id == deck.Id);
                    if (d != null)
                    {
                        //_sourceCache.AddOrUpdate(d);
                        d.Update();
                    }

                }
            }
        }


        [RelayCommand]
        private async void CreateDeck(Deck deck)
        {
            var result = await _host.OpenDeckModal(deck);
            if (result.Success)
            {

            }
        }

        [RelayCommand(CanExecute = nameof(CanSelect))]
        private async void Select()
        {
            Close(true, SelectedDeck.Deck, "Select");
        }
        private bool CanSelect() => SelectedDeck != null;



        //----------------------------------------------------------------------------- FILTER --------------------------------------------------------------------------

        private string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set
            {
                SetProperty(ref _nameFilter, value);
                UpdateFilter();
            }
        }

        private Func<DeckSummaryViewModel, bool> CreateFilter()
        {
            return item =>
            {
                var name_pass = string.IsNullOrEmpty(NameFilter) ||
                              item.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase);

                return name_pass; //&& categoryPass && pricePass;
            };
        }


        private void UpdateFilter()
        {
            // DynamicData автоматически применит новый фильтр
            _sourceCache.Refresh(); // Перефильтрует существующие данные
        }
    }
}
