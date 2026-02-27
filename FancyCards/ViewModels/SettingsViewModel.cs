
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Helpers;
using FancyCards.Models;
using FancyCards.Services;
using FancyPhrases.Models;

using System.IO;
using System.Reflection;



namespace FancyCards.ViewModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute { }

    public partial class SettingsViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly SettingsService _settingsService;




        //private int _correctAnswersToFinishReviewing = 10;
        //[Setting]
        //public int СorrectAnswersToFinishReviewing
        //{
        //    get => _correctAnswersToFinishReviewing;
        //    set => SetProperty(ref _correctAnswersToFinishReviewing, value);
        //}


        public SettingsViewModel(MainWindowViewModel host, SettingsService settingsService)
        {
            _host = host;
            _settingsService = settingsService;

            Header = "Settings";

            var _ = LoadSettings();
        }

        private async Task LoadSettings()
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

        private async Task Script()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                IEnumerable<Change> old_rules = PathHelper.ReadFile<List<Change>>("old_data/Changes.json");
                var old_phrases = PathHelper.ReadFile<List<Phrase>>("old_data/Phrases.json");
                var old_attempts = PathHelper.ReadFile<List<Attempt>>("old_data/Attempts.json");

                int[] intervals  = new int[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181 };
                var deck_id = _host.Deck.Id;
                var cards = new List<Card>();

                var audio_utilities = new AudioUtilities();

                foreach(var old_phrase in old_phrases)
                {
                    old_phrase.Sound.Path = Path.Combine("old_data", old_phrase.Sound.Path);

                    var card = new Card
                    {
                        BackText = old_phrase.Translation,
                        CommentText = old_phrase.Remark,
                        DateCreated = old_phrase.CreationDate,
                        DeckId = deck_id,
                        FrontText = old_phrase.Translation,
                        LastReviewDate = old_phrase.ClosestDate.AddDays(-intervals[old_phrase.Answers.RepeatCorrect - 1]),
                        NextReviewDate = old_phrase.ClosestDate,
                        Difficulty = Difficulty.Normal,
                        MessageText = string.Empty,
                        PrefixText = old_phrase.PreOriginal,
                        SuffixText = old_phrase.PostOriginal,
                        State = old_phrase.State switch
                        {
                            PhraseState.Repeat => CardState.Reviewing,
                            PhraseState.Learn => CardState.Learning,
                            PhraseState.Done => CardState.Mastered,
                            _ => CardState.Learning
                        },
                        TimeSpent = TimeSpan.Zero,
                        Audio = new AudioSource
                        {
                            Tempo = old_phrase.Sound.Tempo,
                            Path = Path.Combine("audio", Path.GetFileName(old_phrase.Sound.Path)),
                            Volume = old_phrase.Sound.Volume,
                            StartPosition = old_phrase.Sound.StartPosition / audio_utilities.GetLength(old_phrase.Sound.Path),
                            EndPosition = old_phrase.Sound.StopPosition / audio_utilities.GetLength(old_phrase.Sound.Path)
                        },
                        Scores = new CardScores
                        {
                            CorrectCount = old_phrase.Answers.GetCorrectSum,
                            TotalCount = old_phrase.Answers.GetTotalSum,
                            I = intervals[old_phrase.Answers.RepeatCorrect - 1],
                            Reps = old_phrase.Answers.RepeatCorrect,
                            EF = 2.0
                        }
                         

                    };

                }


            });
        }

        [RelayCommand]
        private async void RunScript()
        {
            await _host.StartLoading(false);
            await Script();
            _host.StopLoading();
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
