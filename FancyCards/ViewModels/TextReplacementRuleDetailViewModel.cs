using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class TextReplacementRuleDetailViewModel : BaseModalViewModel<TextReplacementRule>
    {

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string _original = string.Empty;
        [ObservableProperty]
        private string _replacement = string.Empty;
        public TextReplacementRuleDetailViewModel(TextReplacementRule rule)
        {
            

            if(rule.Id != default)
            {
                Header = "Edit";

                _original = rule.Original;
                _replacement = rule.Replacement;
            }
            else
            {
                Header = "Create";
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save() => Close(true, new TextReplacementRule(Original, Replacement), "Save");
        private bool CanSave() => !string.IsNullOrEmpty(Original);
    }
}
