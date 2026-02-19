


using FancyCards.Models;
using Newtonsoft.Json;

namespace FancyCards.Services
{
    public class SettingsService 
    {
        private readonly DataService _dataService;
        private int _deckId;

        public int TrainingLearnCards { get; set; } = 10;
        public int TrainingReviewCards { get; set; } = 15;

        public int СorrectAnswersToFinishLearning { get; set; } = 2;
        //public int СorrectAnswersToFinishReviewing { get; set; } = 2;

        public SettingsService(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadSettingsAsync(int deckId)
        {
            _deckId = deckId;
            var settings = await _dataService.GetSettingsAsync(_deckId);

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
                    Type = prop.PropertyType.AssemblyQualifiedName,
                    DeckId = _deckId,
                });
            }

            // Сохраняем все в БД (перезаписываем)
            await _dataService.SaveSettingsAsync(settings, _deckId);
        }
    }
}
