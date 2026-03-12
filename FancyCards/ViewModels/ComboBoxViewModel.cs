using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;


namespace FancyCards.ViewModels
{

    
    public partial class ComboBoxViewModel : BaseModalViewModel<object>
    {

        public IEnumerable Source { get; }

        [ObservableProperty]
        private object _selectedItem;

        public ComboBoxViewModel(IEnumerable source, object selectedItem, string header)
        {
            Source = source;
            Header = header;
            SelectedItem = selectedItem;
        }

        [RelayCommand]
        private void Accept()
        {
            Close(true, SelectedItem, "Accept");
        }
    }
}
