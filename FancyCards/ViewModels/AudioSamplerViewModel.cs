using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Audio.Common;
using FancyCards.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FancyCards.ViewModels
{
    public partial class AudioSamplerViewModel : ObservableObject
    {
        private readonly AudioEngine _audioEngine;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartPlaybackCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopPlaybackCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartRecordingCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartSlowMotionPlaybackCommand))]
        private State _audioSamplerState;

        [ObservableProperty]
        private double _playbackStartPosition = 0;
        [ObservableProperty]
        private double _startSelection = 0;
        [ObservableProperty]
        private double _endSelection = 1;

        [ObservableProperty]
        private TimeSpan _audioDuration = TimeSpan.Zero;

        [ObservableProperty]
        private TimeSpan _playbackCurrentPositionTimeSpan;
        [ObservableProperty]
        private double _playbackCurrentPosition;

        [ObservableProperty]
        private ObservableCollection<double> _points = [];

        public AudioSamplerViewModel(AudioEngine audioEngine)
        {
            _audioEngine = audioEngine;

            _audioEngine.StateChanged += OnAudioEngineStateChanged;
            _audioEngine.GraphChanged += OnGraphChanged;
            _audioEngine.AudioDurationChanged += OnDurationChanged;
            _audioEngine.PlaybackPositionChanged += OnPlaybackPositionChanged;
        }

        private void OnPlaybackPositionChanged(PlaybackPositionArgs args)
        {
            PlaybackCurrentPositionTimeSpan = args.PositionTimeSpan;
            PlaybackCurrentPosition = args.PositionPercent;
        }

        private void OnDurationChanged(TimeSpan span)
        {
            AudioDuration = span;
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
        private void StartPlayback()
        {
            _audioEngine.StartPlayback(); 
        }
        private bool CanStartPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;


        [RelayCommand(CanExecute = nameof(CanStartSlowMotionPlayback))]
        private void StartSlowMotionPlayback()
        {
            _audioEngine.StartPlayback(playbackSpeed: PlaybackSpeed.Half);
        }
        private bool CanStartSlowMotionPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;


        [RelayCommand(CanExecute = nameof(CanStopPlayback))]
        private void StopPlayback()
        {
            _audioEngine.StopPlayback();
        }
        private bool CanStopPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;


        [RelayCommand(CanExecute = nameof(CanStartRecording))]
        private void StartRecording()
        {
            Points.Clear();
            _audioEngine.StartRecording();
        }
        private bool CanStartRecording() => AudioSamplerState == State.Stopped || AudioSamplerState == State.Initial;

        [RelayCommand(CanExecute = nameof(CanStopRecording))]
        private void StopRecording()
        {
            _audioEngine.StopRecording();
        }
        private bool CanStopRecording() => AudioSamplerState == State.Recording;
    }
}
