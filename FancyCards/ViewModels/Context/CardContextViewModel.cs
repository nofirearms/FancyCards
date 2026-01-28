using CommunityToolkit.Mvvm.Input;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class CardContextViewModel : BaseModalViewModel<object>
    {

        [RelayCommand]
        private void Test() => Close(true);
    }
}
