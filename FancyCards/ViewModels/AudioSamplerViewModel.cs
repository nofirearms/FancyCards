using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Audio;
using FancyCards.Audio.Common;
using FancyCards.Helpers;
using FancyCards.Models;
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
        private Selection _selection = new Selection(0, 1);

        [ObservableProperty]
        private double _tempo = 1d;
        [ObservableProperty]
        private double _volume = 1d;

        [ObservableProperty]
        private TimeSpan _audioDuration = TimeSpan.Zero;

        [ObservableProperty]
        private TimeSpan _playbackCurrentPositionTimeSpan;

        [ObservableProperty]
        private double _playbackCurrentPosition;



        [ObservableProperty]
        private ObservableCollection<double> _points = [];

        public AudioSamplerViewModel(Card card)
        {
            _audioEngine = new AudioEngine();

            _audioEngine.StateChanged += OnAudioEngineStateChanged;
            _audioEngine.GraphChanged += OnGraphChanged;
            _audioEngine.AudioDurationChanged += OnDurationChanged;
            _audioEngine.PlaybackPositionChanged += OnPlaybackPositionChanged;

            if(card.Id != default)
            {
                LoadGraph(card.Audio.Path);
                Selection = new Selection(card.Audio.StartPosition, card.Audio.EndPosition);
                PlaybackStartPosition = Selection.Start;
            }
            
        }

        private async void LoadGraph(string path)
        {
            _audioEngine.OpenAudioAsync(path);
            var points = await _audioEngine.GetWaveformPoints(path);
            Points = new ObservableCollection<double>(points);
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
            _audioEngine.StartPlayback(startPosition: PlaybackStartPosition, endPosition: Selection.End); 
        }
        private bool CanStartPlayback() => AudioSamplerState == State.Playing || AudioSamplerState == State.Stopped;


        [RelayCommand(CanExecute = nameof(CanStartSlowMotionPlayback))]
        private void StartSlowMotionPlayback()
        {
            _audioEngine.StartPlayback(startPosition: PlaybackStartPosition, endPosition: Selection.End, playbackSpeed: PlaybackSpeed.Half);
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

        public async Task RenderAudioToMp3Async(string path, int bitrate = 128_000)
        {
            await _audioEngine.RenderToMp3Async(path, bitrate);
        }
    }
}
