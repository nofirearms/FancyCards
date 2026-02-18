


using FancyCards.Models;
using Newtonsoft.Json;

namespace FancyCards.Services
{
    public class SettingsService 
    {
        private readonly DataService _dataService;

        public int TrainingMaxLearnCards { get; set; } = 10;
        public int TrainingMaxReviewCards { get; set; } = 15;

        public SettingsService(DataService dataService)
        {
            _dataService = dataService;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var settings = await _dataService.GetSettingsAsync();

            var json_setting = settings.ToDictionary(s => s.Key, s => s.Value);
            var json = JsonConvert.SerializeObject(json_setting);

            JsonConvert.PopulateObject(json, this);
        }

        public async Task SaveAsync()
        {
            var properties = this.GetType().GetProperties();
            var settings = new List<Setting>();

            foreach (var prop in properties)
            {
                // Получаем значение свойства
                var value = prop.GetValue(this)?.ToString() ?? "";

                // Создаем настройку
                settings.Add(new Setting
                {
                    Key = prop.Name,
                    Value = value,
                    Type = prop.PropertyType.AssemblyQualifiedName
                });
            }

            // Сохраняем все в БД (перезаписываем)
            await _dataService.SaveSettingsAsync(settings);
        }
    }
}
