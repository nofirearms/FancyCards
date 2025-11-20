using FancyCards.Audio.Providers;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundTouch.Net.NAudioSupport;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace FancyCards.Audio
{
    public class AudioEngine
    {

        private readonly AudioUtilities _utilities;
        private readonly WaveOutEvent _outputDevice;
        private readonly MMDevice _captureDevice;
        private readonly WasapiCapture _captureSource;

        private AudioStateManager _audioStateManager;
        private BufferedWaveProvider _bufferedWaveProvider;

        public State State { get; set; }

        public AudioEngine()
        {
            _utilities = new AudioUtilities();
            _outputDevice = new WaveOutEvent();

            _captureDevice = _utilities.GetDefaultOutputDevice();//_utilities.GetDeviceById(deviceId);
            _captureSource = _utilities.GetWasapiCaptureInstance(_captureDevice);

            _audioStateManager = new AudioStateManager(_captureSource.WaveFormat);
            _bufferedWaveProvider = new BufferedWaveProvider(_captureSource.WaveFormat);

            _outputDevice.PlaybackStopped += OnPlaybackStopped;
        }


        public async Task OpenAudioAsync(string path)
        {
            _audioStateManager.LoadFromAudioFile(path);
        }

        public void StartPlayback(TimeSpan? startPosition = null, TimeSpan? endPosition = null, PlaybackSpeed playbackSpeed = PlaybackSpeed.Full, float volume = 0.4f, float tempo = 1.0f, float targetRMS = 0.2f)
        {         

            if (State == State.Recording || State == State.Initial)
            {
                return;
            }
            else if (State == State.Playing)
            {
                OnPlaybackStopped(null, null);
                StopPlayback();
                StartPlayback();
            }
            else if (State == State.Paused)
            {
                _outputDevice.Play();
                State = State.Playing;
            }
            else if (State == State.Stopped)
            {
                var source = _audioStateManager.CreateWaveProvider() as RawSourceWaveStream;

                var settings = new AudioSettings
                {
                    Volume = volume,
                    SlowMotion = playbackSpeed == PlaybackSpeed.Half,
                    StartTime = startPosition ?? TimeSpan.Zero,
                    EndTime = endPosition ?? source.TotalTime,
                    TargetRMS = targetRMS,
                    Tempo = tempo
                };

                var audio_pipeline = AudioPipeline.Create(source.ToSampleProvider(), settings);

                _outputDevice.Init(audio_pipeline);
                _outputDevice.Play();

                State = State.Playing;
            }
        }

        public void StopPlayback()
        {
            _outputDevice?.Stop();
        }

        public void PausePlayback()
        {
            if (State == State.Playing)
            {
                _outputDevice?.Pause();
                State = State.Paused;
            }
            else if (State == State.Paused)
            {
                _outputDevice?.Play();
                State = State.Playing;
            }
        }

        public void StartRecording()
        {
            if (State == State.Playing)
            {
                return;
            }
            else if (State == State.Paused)
            {
                StopPlayback();
                StartRecording();
            }
            else if (State == State.Recording)
            {
                //StopRecording();
                StartRecording();
            }
            else if (State == State.Stopped || State == State.Initial)
            {
                _bufferedWaveProvider.ClearBuffer();

                _captureSource.DataAvailable += OnCaptureDataAvailable;
                _captureSource.RecordingStopped += OnRecordingStopped;

                _captureSource.StartRecording();

                State = State.Recording;
            }
        }

        public void StopRecording()
        {
            _captureSource.StopRecording();
            State = State.Stopped;
        }

        private void OnCaptureDataAvailable(object? sender, WaveInEventArgs e)
        {
            _bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            //var peak = _audioService.PeakSampleFromBuffer(e.Buffer, e.BytesRecorded);
            //GraphChanged?.Invoke(this, new Point((_writer as WaveFileWriterWithCounter).Counter, peak));
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _captureSource.DataAvailable -= OnCaptureDataAvailable;
            _captureSource.RecordingStopped -= OnRecordingStopped;

            _audioStateManager.SetData(GetBufferedData(_bufferedWaveProvider));

            //добавляем точку чтобы знать где конец списка, для графа
            //GraphChanged?.Invoke(this, new Point(-1, -1));
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            State = State.Stopped;
        }


        public byte[] GetBufferedData(BufferedWaveProvider provider)
        {
            int bytesAvailable = provider.BufferedBytes;
            byte[] buffer = new byte[bytesAvailable];
            provider.Read(buffer, 0, bytesAvailable); // достаёт из очереди
            return buffer;
        }
    }



    public enum PlaybackSpeed
    {
        Full, Half
    }

    public enum State
    {
        Initial, Playing, Recording, Paused, Stopped
    }

}
