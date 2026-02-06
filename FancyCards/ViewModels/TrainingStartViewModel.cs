using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class TrainingStartViewModel : BaseModalViewModel<object>
    {

        [RelayCommand]
        private void StartTraining() => Close(buttonTag: "StartTraining");

        [RelayCommand]
        private void CancelTraining() => Cancel();
    }
}
