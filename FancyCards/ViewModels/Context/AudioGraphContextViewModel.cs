using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;

namespace FancyCards.ViewModels
{
    public partial class AudioGraphContextViewModel : BaseModalViewModel<object>
    {

        [RelayCommand]
        private void ResetSelection() => Close(buttonTag: "ResetSelection");
    }
}
