using CommunityToolkit.Mvvm.Input;

namespace FancyCards.ViewModels
{ 
    public partial class MainMenuContextViewModel : BaseModalViewModel<object>
    {

        [RelayCommand]
        public void ButtonClick(string parameter) => Close(true, null, parameter);
    }
}
