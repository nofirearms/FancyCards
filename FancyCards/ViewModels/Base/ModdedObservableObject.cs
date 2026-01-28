using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.ViewModels
{
    public class ModdedObservableObject : ObservableObject
    {

        public void UpdateProperties()
        {
            var type = this.GetType();
            foreach (var property in type.GetProperties())
            {
                OnPropertyChanged(property.Name);
            }
        }
    }
}
