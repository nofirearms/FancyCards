using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Audio.Common;
using FancyCards.Helpers;
using FancyCards.Models;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FancyCards.ViewModels
{
    public partial class AudioSamplerViewModel : ObservableObject, IDisposable
    {
        private readonly MainWindowViewModel _host;
        private readonly AudioEngine _audioEngine;
        private DispatcherTimer _recordingTimer;
        private Stopwatch _stopwatch;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartPlaybackCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopPlaybackCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
        private State _audioSamplerState;

        [ObservableProperty]
        private double _playbackStartPosition = 0;

        [ObservableProperty]
        private Selection _selection = new Selection(0, 1);

        [ObservableProperty]
        private double _tempo = 1d;
        [ObservableProperty]
        private double _volume = 0.4d;

        [ObservableProperty]
        private TimeSpan _audioDuration = TimeSpan.Zero;
        partial void OnAudioDurationChanged(TimeSpan value)
        {
            
        }

        [ObservableProperty]
        private TimeSpan _playbackCurrentPositionTimeSpan;

        [ObservableProperty]
        private double _playbackCurrentPosition;

        [ObservableProperty]
        private ObservableCollection<double> _points = [];

        private bool _canUndo = false;


        public AudioSamplerViewModel(MainWindowViewModel host, AudioEngine audioEngine, Card card)
        {
            _host = host;
            _audioEngine = audioEngine;

            _audioEngine.StateChanged += OnAudioEngineStateChanged;
            _audioEngine.GraphChanged += OnGraphChanged;
            _audioEngine.PlaybackPositionChanged += OnPlaybackPositionChanged;
            _audioEngine.AudioSourceChanged += OnAudioSourceChanged;

            if(card.Id != default)
            {
                _audioEngine.OpenAudioAsync(card.Audio.Path);
                Selection = new Selection(card.Audio.StartPosition, card.Audio.EndPosition);
                PlaybackStartPosition = Selection.Start;
                Volume = card.Audio.Volume;
                Tempo = card.Audio.Tempo;
                AudioDuration = _audioEngine.Duration;
            }

            _stopwatch = new Stopwatch();
            _recordingTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _recordingTimer.Tick += (s, a) =>
            {
                AudioDuration = _stopwatch.Elapsed;
            };
        }

        private async void OnAudioSourceChanged(AudioSourceChangedArgs args)
        {
            BuildGraph();

            AudioDuration = args.Duration;

            _canUndo = args.CanUndo;
            UndoAudioCommand?.NotifyCanExecuteChanged();

            ResetSelection();
        }


        private async void BuildGraph()
        {
            var points = await _audioEngine.GetWaveformPoints();
            Points = new ObservableCollection<double>(points);
        }

        private void OnPlaybackPositionChanged(PlaybackPositionArgs args)
        {
            PlaybackCurrentPositionTimeSpan = args.PositionTimeSpan;
            PlaybackCurrentPosition = args.PositionPercent;
        }


        private void OnGraphChanged(double obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _points.Add(obj);
            });
            OnPropertyChanged(nameof(Points));
        }

        private void OnAudioEngineStateChanged(State state)
        {
            AudioSamplerState = state;
        }

        [RelayCommand(CanExecute = nameof(CanStartPlayback))]
        private void StartPlayback(PlaybackMode playbackMode)
        {
            if(playbackMode == PlaybackMode.Selected)
            {
                _audioEngine.StartPlayback(startPosition: PlaybackStartPosition, endPosition: Selection.End, tempo: (float)Tempo, volume: (float)Volume);
            }
            else if(playbackMode == PlaybackMode.Full)
            {
                _audioEngine.StartPlayback(tempo: (float)Tempo, volume: (float)Volume);
            }
            else if(playbackMode == PlaybackMode.SelectedSlow)
            {
                _audioEngine.StartPlayback(startPosition: PlaybackStartPosition, endPosition: Selection.End, PlaybackSpeed.Half, tempo: (float)Tempo, volume: (float)Volume);
            }
        }
        private bool CanStartPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;



        [RelayCommand(CanExecute = nameof(CanStopPlayback))]
        private void StopPlayback()
        {
            _audioEngine.StopPlayback();
        }
        private bool CanStopPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;



        [RelayCommand(CanExecute = nameof(CanStartRecording))]
        private async void StartRecording()
        {
            Points.Clear();
            _audioEngine.StartRecording();

            _stopwatch.Reset();
            _stopwatch.Start();
            _recordingTimer.Start();
            
        }
        private bool CanStartRecording() => AudioSamplerState == State.Stopped || AudioSamplerState == State.Initial;



        [RelayCommand(CanExecute = nameof(CanStopRecording))]
        private async void StopRecording()
        {
            _audioEngine.StopRecording();

            _stopwatch.Stop();
            _recordingTimer.Stop();
            
        }
        private bool CanStopRecording() => AudioSamplerState == State.Recording;



        [RelayCommand]
        private void ResetSelection()
        {
            Selection = new Selection(0, 1);
            PlaybackStartPosition = 0;
        }

        [RelayCommand]
        private async void TrimAudio()
        {
            if (Selection.Start == 0 && Selection.End == 1) return;

            _audioEngine.Trim(Selection.Start, Selection.End);
        }

        [RelayCommand]
        private async void CutAudio()
        {
            if (Selection.Start == 0 && Selection.End == 1) return;

            _audioEngine.Cut(Selection.Start, Selection.End);
        }


        [RelayCommand(CanExecute = nameof(CanUndoAudio))]
        private async void UndoAudio()
        {
            _audioEngine.Undo();
        }
        private bool CanUndoAudio() => _canUndo;


        [RelayCommand]
        private async void OpenAudioGraphContext()
        {
            var context_result = await _host.OpenContext(new AudioGraphContextViewModel());
            if(context_result.ButtonTag == "ResetSelection")
            {
                ResetSelection();
            }
            else if(context_result.ButtonTag == "Trim")
            {
                TrimAudio();
            }
            else if(context_result.ButtonTag == "Cut")
            {
                CutAudio();
            }
        }

        public void Dispose()
        {
            _recordingTimer.Stop();
            _stopwatch.Stop();
            _stopwatch = null;
            _recordingTimer = null;
        }
    }
}
