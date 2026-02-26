
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.PLinq;
using FancyCards.Models;
using FancyCards.Services;
using System.Collections.ObjectModel;
using System.Data;

namespace FancyCards.ViewModels
{
    public partial class TextReplacementRuleListViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly DataService _dataService;


        [ObservableProperty]
        private ReadOnlyObservableCollection<TextReplacementRule> _rules;

        private SourceCache<TextReplacementRule, int> _sourceCache;

        public TextReplacementRuleListViewModel(MainWindowViewModel host, DataService dataService)
        {
            _host = host;
            _dataService = dataService;

            Header = "Rules";

            var _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var rules = await _dataService.GetTextReplacementRules();

            _sourceCache = new SourceCache<TextReplacementRule, int>(o => o.Id);
            _sourceCache.AddOrUpdate(rules ?? new List<TextReplacementRule>());

            _sourceCache.Connect()
                .Filter(CreateFilter())
                .Bind(out _rules)
                .Subscribe();
        }


        [RelayCommand]
        private async void OpenContext(TextReplacementRule rule)
        {
            var result = await _host.OpenContext(new GeneralListContextViewModel<TextReplacementRule>(rule));
            if(result.ButtonTag == "Edit")
            {
                var edit_result = await _host.OpenTextReplacementRuleDetailModal(rule);
                if (edit_result.Success)
                {

                    var edited = new TextReplacementRule
                    {
                        Id = rule.Id,
                        Original = edit_result.Data.Original,
                        Replacement= edit_result.Data.Replacement
                    };

                    await _host.StartLoading(false);

                    _sourceCache.AddOrUpdate(edited);
                    await _dataService.UpdateTextReplacementRuleAsync(rule);

                    _host.StopLoading();
                }
            }
            else if(result.ButtonTag == "Remove")
            {
                await _host.StartLoading(false);

                _sourceCache.Remove(rule);
                await _dataService.RemoveTextReplacementRuleAsync(rule);

                _host.StopLoading();

            }
        }

        [RelayCommand]
        private async void Add()
        {
            var create_result = await _host.OpenTextReplacementRuleDetailModal(null);
            
            if (create_result.Success)
            {

                await _host.StartLoading(false);

                await _dataService.CreateTextReplacementRuleAsync(create_result.Data);

                _sourceCache.AddOrUpdate(create_result.Data);

                _host.StopLoading();
            }
        }

        //[RelayCommand]
        //private void Ok() => Close();

        //----------------------------------------------------------------------------- FILTER --------------------------------------------------------------------------

        private string _textFilter;
        public string TextFilter
        {
            get => _textFilter;
            set
            {
                SetProperty(ref _textFilter, value);
                UpdateFilter();
            }
        }

        private Func<TextReplacementRule, bool> CreateFilter()
        {
            return item =>
            {
                var text_pass = string.IsNullOrEmpty(TextFilter) ||
                              item.Original.Contains(TextFilter, StringComparison.OrdinalIgnoreCase) ||
                              item.Replacement.Contains(TextFilter, StringComparison.OrdinalIgnoreCase);

                return text_pass; //&& categoryPass && pricePass;
            };
        }


        private void UpdateFilter()
        {
            // DynamicData автоматически применит новый фильтр
            _sourceCache.Refresh(); // Перефильтрует существующие данные
        }
    }

}
