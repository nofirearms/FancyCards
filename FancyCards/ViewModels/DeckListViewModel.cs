using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Xml.Linq;

namespace FancyCards.ViewModels
{
    public partial class DeckListViewModel : BaseModalViewModel<Deck>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;
        private readonly SettingsService _settingsService;
        private readonly ModalService _modalService;
        [ObservableProperty]
        private ReadOnlyObservableCollection<DeckSummaryViewModel> _decks;

        [NotifyCanExecuteChangedFor(nameof(SelectCommand))]
        [ObservableProperty]
        private DeckSummaryViewModel _selectedDeck;



        public DeckListViewModel(MainWindowViewModel host, DataService dataService, ModalService modalService, SettingsService settingsService)
        {

            _host = host;
            _dataService = dataService;
            _settingsService = settingsService;
            _modalService = modalService;

            Header = "Decks";

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // 1. Захватываем UI-поток (здесь он еще доступен)
            var uiContext = SynchronizationContext.Current;
            // 1. Создаем поток для текста
            var textChanged = this.WhenPropertyChanged(x => x.NameFilter)
                .Select(_ => CreateFilter());
                
            _dataService.ConnectDecks()
                .Filter(textChanged)
                .Transform(d =>
                {
                    var count = _dataService.GetCardsByDeckId(d.Id).Count();
                    return new DeckSummaryViewModel(d, count);
                })
                .DisposeMany()
                .ObserveOn(uiContext)
                .Bind(out _decks)
                .Subscribe();

            SelectedDeck = _decks.FirstOrDefault(d => d.Id == _dataService.CurrentDeck.Id);
        }



        private AsyncRelayCommand<Deck> _createDeckCommand;
        public IAsyncRelayCommand CreateDeckCommand => _createDeckCommand ??= new AsyncRelayCommand<Deck>(CreateDeck);

        private async Task CreateDeck(Deck deck)
        {
            var result = await _modalService.OpenDeckModal(deck);
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

        [ObservableProperty]
        private string _nameFilter;

        private Func<Deck, bool> CreateFilter()
        {
            return item =>
            {
                var name_pass = string.IsNullOrEmpty(NameFilter) ||
                              item.Name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase);

                return name_pass; //&& categoryPass && pricePass;
            };
        }

    }
}
