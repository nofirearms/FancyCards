using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Reflection;
using System.Text;

namespace FancyCards.ViewModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute { }

    public partial class SettingsViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly SettingsService _settingsService;


        private int _trainingMaxLearnCards = 5;
        [Setting]
        public int TrainingMaxLearnCards
        {
            get => _trainingMaxLearnCards;
            set => SetProperty(ref _trainingMaxLearnCards, value);
        }

        private int _trainingMaxReviewCards = 5;
        [Setting]
        public int TrainingMaxReviewCards
        {
            get => _trainingMaxReviewCards;
            set => SetProperty(ref _trainingMaxReviewCards, value);
        }

        public SettingsViewModel(MainWindowViewModel host, SettingsService settingsService)
        {
            _host = host;
            _settingsService = settingsService;

            LoadSettings();
        }

        private void LoadSettings()
        {
            var properties = this.GetType().GetProperties().Where(p => p.GetCustomAttribute<SettingAttribute>() != null);

            foreach (var property in properties)
            {
                var service_property = _settingsService.GetType().GetProperty(property.Name);
                if (service_property == null) continue;

                var value = service_property.GetValue(_settingsService);
                property.SetValue(this, value);

                OnPropertyChanged(nameof(property.Name));
            }
        }

        public async Task SaveSettings()
        {
            var properties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<SettingAttribute>() != null);

            foreach (var property in properties)
            {
                var service_property = _settingsService.GetType().GetProperty(property.Name);
                if (service_property == null || !service_property.CanWrite) continue;

                var value = property.GetValue(this);
                service_property.SetValue(_settingsService, value);
            }

            await _settingsService.SaveAsync();
        }

        [RelayCommand]
        private async void Save()
        {
            await _host.StartLoading(false);

            await SaveSettings();

            _host.StopLoading();

            Close();
        }

        [RelayCommand]
        private new void Cancel() => base.Cancel();
    }
}
