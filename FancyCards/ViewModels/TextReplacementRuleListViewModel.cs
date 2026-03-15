
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Data;
using System.Reactive.Linq;

namespace FancyCards.ViewModels
{
    public partial class TextReplacementRuleListViewModel : BaseModalViewModel<object>
    {
        private readonly DataService _dataService;
        private readonly ModalService _modalService;
        private readonly LoadingService _loadingService;

        [ObservableProperty]
        private ReadOnlyObservableCollection<TextReplacementRule> _rules;

        public TextReplacementRuleListViewModel(DataService dataService, ModalService modalService, LoadingService loadingService)
        {
            _dataService = dataService;
            _modalService = modalService;
            _loadingService = loadingService;

            Header = "Rules";

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {

            // 1. Захватываем UI-поток (здесь он еще доступен)
            var uiContext = SynchronizationContext.Current;
            // 1. Создаем поток для текста
            var textChanged = this.WhenPropertyChanged(x => x.TextFilter)
                .Select(_ => CreateFilter());

            _dataService.ConnectRules()
                .Filter(textChanged)
                .DisposeMany()
                .ObserveOn(uiContext)
                .Bind(out _rules)
                .Subscribe();
        }


        [RelayCommand]
        private async void OpenContext(TextReplacementRule rule)
        {
            var result = await _modalService.OpenContext(new GeneralListContextViewModel<TextReplacementRule>(rule));
            if(result.ButtonTag == "Edit")
            {
                var edit_result = await _modalService.OpenTextReplacementRuleDetailModal(rule);
                if (edit_result.Success)
                {

                    var edited = new TextReplacementRule
                    {
                        Id = rule.Id,
                        Original = edit_result.Data.Original,
                        Replacement= edit_result.Data.Replacement
                    };
                    await _loadingService.ShowLoadingAsync(async () =>
                    {
                        await _dataService.AddOrUpdateTextReplacementRulesAsync([edited]);
                    }, true, false);
                }
            }
            else if(result.ButtonTag == "Remove")
            {
                await _loadingService.ShowLoadingAsync(async () =>
                {
                    await _dataService.RemoveTextReplacementRuleAsync(rule);
                }, true, false);
            }
        }

        private AsyncRelayCommand _addCommand;
        public IAsyncRelayCommand AddCommand => _addCommand ??= new AsyncRelayCommand(Add);
        private async Task Add()
        {
            var create_result = await _modalService.OpenTextReplacementRuleDetailModal(null);
            if (create_result.Success)
            {
                await _loadingService.ShowLoadingAsync(async () =>
                {
                    await _dataService.AddOrUpdateTextReplacementRulesAsync([create_result.Data]);
                }, true, false);

            }
        }


        //----------------------------------------------------------------------------- FILTER --------------------------------------------------------------------------

        [ObservableProperty]
        private string _textFilter;

        private Func<TextReplacementRule, bool> CreateFilter()
        {
            return item =>
            {
                var text_pass = string.IsNullOrEmpty(TextFilter) ||
                              item.Original.Contains(TextFilter, StringComparison.OrdinalIgnoreCase) ||
                              (item.Replacement?.Contains(TextFilter, StringComparison.OrdinalIgnoreCase) ?? false);

                return text_pass; //&& categoryPass && pricePass;
            };
        }

    }

}
