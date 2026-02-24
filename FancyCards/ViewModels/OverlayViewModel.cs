using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public partial class OverlayViewModel : ObservableObject
    {

        [ObservableProperty]
        private OverlayType _type = OverlayType.None;

        [ObservableProperty]
        private bool _isVisible = false;

        public OverlayViewModel()
        {
            
        }
    }
}
