
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Audio.Common;
using FancyCards.Helpers;
using FancyCards.Models;
using FancyCards.Services;
using FancyPhrases.Models;
using NAudio.CoreAudioApi;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;



namespace FancyCards.ViewModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute { }

    public partial class SettingsViewModel : BaseModalViewModel<object>
    {
        private readonly MainWindowViewModel _host;
        private readonly SettingsService _settingsService;
        private readonly DataService _dataService;

        [ObservableProperty]
        private bool _devicesLoaded = false;

        private string _caputreDeviceId;
        [Setting]
        public string CaptureDeviceId
        {
            get => _caputreDeviceId;
            set
            {
                SetProperty(ref _caputreDeviceId, value);
                if (CaptureDevices.Any()) 
                {
                    CaptureDeviceName = CaptureDevices.Find(o => o.ID == value).Name;
                }
            }
        }

        private string _captureDeviceName;
        [Setting]
        public string CaptureDeviceName
        {
            get => _captureDeviceName;
            set => SetProperty(ref _captureDeviceName, value);
        }

        [ObservableProperty]
        private List<CaptureDeviceSummary> _captureDevices = new();


        [ObservableProperty]
        private string _info;

        public SettingsViewModel(MainWindowViewModel host, SettingsService settingsService, DataService dataService)
        {
            _host = host;
            _settingsService = settingsService;
            _dataService = dataService;

            Header = "Settings";

            LoadSettings();

            var audio_utilities = new AudioUtilities();
            var default_device = audio_utilities.GetDefaultInputDevice();
            CaptureDeviceId = string.IsNullOrEmpty(CaptureDeviceId) ? default_device.ID : CaptureDeviceId;
            CaptureDeviceName = string.IsNullOrEmpty(CaptureDeviceName) ? default_device.FriendlyName : CaptureDeviceName;

            _ = InitializeAsync();

        }

        private async Task InitializeAsync()
        {
            await System.Threading.Tasks.Task.Delay(20);
            await System.Threading.Tasks.Task.Run(() =>
            {
                var audio_utilities = new AudioUtilities();

                CaptureDevices = audio_utilities.GetRecordDevices().Select(d => new CaptureDeviceSummary { Name = d.FriendlyName, ID = d.ID }).ToList();
            });
            DevicesLoaded = true;
        }

        public override void Loaded()
        {

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

        private async Task Script()
        {
            await System.Threading.Tasks.Task.Run(async() =>
            {
                IEnumerable<Change> old_rules = PathHelper.ReadFile<List<Change>>("old_data/Changes.json");
                var old_phrases = PathHelper.ReadFile<List<Phrase>>("old_data/Phrases.json");
                var old_attempts = PathHelper.ReadFile<List<Attempt>>("old_data/Attempts.json");

                int[] intervals  = new int[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181 };
                var deck_id = _dataService.CurrentDeck.Id;
                var cards = new List<Card>();

                var audio_utilities = new AudioUtilities();

                int i = 0;

                foreach(var old_phrase in old_phrases.OrderByDescending(o => o.CreationDate))
                {
                    Info = $"{++i} | {old_phrases.Count}";

                    old_phrase.Sound.Path = Path.Combine("old_data", old_phrase.Sound.Path);

                    var length = audio_utilities.GetLength(old_phrase.Sound.Path);

                    var card = new Card
                    {
                        BackText = old_phrase.Translation,
                        CommentText = old_phrase.Remark,
                        DateCreated = old_phrase.CreationDate,
                        DeckId = deck_id,
                        FrontText = old_phrase.Original,
                        LastReviewDate = old_phrase.ClosestDate.AddDays(old_phrase.Answers.RepeatCorrect == 0 ? 0 : -intervals[old_phrase.Answers.RepeatCorrect - 1]),
                        NextReviewDate = old_phrase.ClosestDate,
                        Difficulty = Difficulty.Normal,
                        MessageText = string.Empty,
                        PrefixText = old_phrase.PreOriginal,
                        SuffixText = old_phrase.PostOriginal,
                        State = old_phrase.State switch
                        {
                            PhraseState.Repeat => CardState.Reviewing,
                            PhraseState.Learn => CardState.Learning,
                            PhraseState.Done => CardState.Archived,
                            _ => CardState.Learning
                        },
                        TotalTimeSpent = TimeSpan.Zero,
                        Audio = new AudioSource
                        {
                            Tempo = old_phrase.Sound.Tempo,
                            Path = Path.Combine("audio", Path.GetFileName(old_phrase.Sound.Path)),
                            Volume = old_phrase.Sound.Volume,
                            StartPosition = (double)old_phrase.Sound.StartPosition / length,
                            EndPosition = (double)(length - old_phrase.Sound.StopPosition) / length
                        },
                        Scores = new CardScores
                        {
                            CorrectCount = old_phrase.State switch
                            {
                                PhraseState.Repeat => old_phrase.Answers.GetCorrectSum - 2,
                                PhraseState.Learn => old_phrase.Answers.LearnCorrect,
                                PhraseState.Done => old_phrase.Answers.GetCorrectSum - 2,
                                _ => 0
                            },
                            TotalCount = old_phrase.Answers.GetTotalSum,
                            I = old_phrase.Answers.RepeatCorrect == 0 ? 0 : intervals[old_phrase.Answers.RepeatCorrect - 1],
                            Reps = old_phrase.Answers.RepeatCorrect,
                            EF = 2.0
                        }


                    };

                    cards.Add(card);
                }

                Info = $"Sesseions progress";
                var sessions = new List<TrainingSession>();
                foreach (var old_attempt in old_attempts) 
                {
                    var session = new TrainingSession
                    {
                        Date = old_attempt.Date,
                        Duration = old_attempt.Duration,
                        
                    };
                    sessions.Add(session);
                }

                Info = $"Rules progress";
                var rules = new List<TextReplacementRule>();
                foreach(var old_rule in old_rules)
                {
                    var rule = new TextReplacementRule
                    {
                        Original = old_rule.Input,
                        Replacement = old_rule.Output
                    };
                    rules.Add(rule);
                }

                await _dataService.AddOrUpdateCardsAsync(cards);
                await _dataService.AddOrUpdateTrainingSessionsAsync(sessions);
                await _dataService.AddOrUpdateTextReplacementRulesAsync(rules);
            });
        }

        [RelayCommand]
        private async void RunScript()
        {
            await _host.StartLoading(false);
            await Script();
            _host.StopLoading();
        }

        private AsyncRelayCommand _exportOldDataCommand;
        public IAsyncRelayCommand ExportOldDataCommand => _exportOldDataCommand ??= new AsyncRelayCommand(ExportOldDataAsync);

        public async Task ExportOldDataAsync()
        {
            await System.Threading.Tasks.Task.Run(async () =>
            {

                var cards = _dataService.GetCardsByDeckId(_dataService.CurrentDeck.Id);

                int[] intervals = new int[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181 };

                var audio_utilities = new AudioUtilities();

                var old_cards = new List<Phrase>();

                var i = 0;

                foreach (var card in cards)
                {
                    i++;
                    Info = $"{Math.Round(((double)i / cards.Count()) * 100)}";
                    var old_card = new Phrase
                    {
                        Id = card.Id,
                        Original = card.FrontText,
                        Translation = card.BackText,
                        CreationDate = card.DateCreated,
                        ClosestDate = card.NextReviewDate,
                        State = card.State switch
                        {
                            CardState.Reviewing => PhraseState.Repeat,
                            CardState.Learning => PhraseState.Learn,
                            CardState.Archived => PhraseState.Done,
                            _ => PhraseState.Done
                        },
                        PreOriginal = card.PrefixText,
                        PostOriginal = card.SuffixText,
                        Remark = card.CommentText,
                        Sound = new Sound
                        {
                            Path = $"Phrases/{Path.GetFileName(card.Audio.Path)}",
                            StartPosition = (long)(audio_utilities.GetLength(card.Audio.Path) * card.Audio.StartPosition),
                            StopPosition = (long)(audio_utilities.GetLength(card.Audio.Path) * (1 - card.Audio.EndPosition)),
                            Tempo = card.Audio.Tempo,
                            Volume = (float)card.Audio.Volume
                        },
                        Answers = new Answers
                        {
                            LearnCorrect = Math.Min(card.Scores.CorrectCount, 2),
                            RepeatCorrect = intervals.Select((value, index) => new { Value = value, Index = index }).OrderBy(x => Math.Abs(x.Value - card.Scores.I)).First().Index + 1,
                            LearnTotal = Math.Min(card.Scores.CorrectCount, 2),
                            RepeatTotal = card.Scores.TotalCount - Math.Min(card.Scores.CorrectCount, 2)
                        }
                    };

                    old_cards.Add(old_card);
                }

                var sessions = _dataService.GetTrainingSessions(_dataService.CurrentDeck.Id);

                var old_sessions = sessions.Select(o => new Attempt { Date = o.Date, Duration = o.Duration });

                PathHelper.CreateDirectory("Export Data/");

                PathHelper.WriteFile(old_cards, Path.Combine("Export Data", "Phrases.json"));
                PathHelper.WriteFile(old_sessions, Path.Combine("Export Data", "Attempts.json"));

            });


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
