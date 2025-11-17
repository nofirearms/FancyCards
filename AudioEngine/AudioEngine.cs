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

        private List<AudioSource> _audioSources;

        public State State { get; set; }

        public AudioEngine()
        {
            _utilities = new AudioUtilities();
            _outputDevice = new WaveOutEvent();

            _captureDevice = _utilities.GetDefaultOutputDevice();//_utilities.GetDeviceById(deviceId);
            _captureSource = _utilities.GetWasapiCaptureInstance(_captureDevice);

            _audioSources = new List<AudioSource>();

            _outputDevice.PlaybackStopped += OnPlaybackStopped;
        }



        public async Task OpenAudioAsync(string path)
        {
            var reader = new AudioFileReader(path);

            var source = new AudioSource(reader.WaveFormat);
            _audioSources.Add(source);

            await reader.CopyToAsync(source.MemoryStream);

            reader.Close();
            reader.Dispose();
        }

        public void StartPlayback(TimeSpan? startPosition = null, TimeSpan? endPosition = null, PlaybackSpeed playbackSpeed = PlaybackSpeed.Full, float volume = 0.4f, double tempo = 1.0, float targetRMS = 0.2f)
        {

            //if (_outputDevice.PlaybackState == PlaybackState.Playing) StopPlayback();
            //if (source is null) return;


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

                var rms = _utilities.GetRMS(_audioSources.Last().RawSource.ToSampleProvider());
                
                var normalized = new NormalizerSampleProvider(_audioSources.Last().RawSource.ToSampleProvider())
                {
                    Ratio = (float)(targetRMS / rms)
                };

                var skip_time = startPosition ?? TimeSpan.Zero;
                var take_time = endPosition ?? _audioSources.Last().RawSource.TotalTime;
                var offset_sp = new OffsetSampleProvider(normalized)
                {
                    SkipOver = skip_time,
                    Take = take_time - skip_time
                };

                var volume_sp = new VolumeSampleProvider(offset_sp)
                {
                    Volume = volume
                };

                var halfspeed_sp = SetHalfSpeed(volume_sp, playbackSpeed == PlaybackSpeed.Half);

                _audioSources.Last().RawSource.Position = 0;

                _outputDevice.Init(halfspeed_sp);
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
                //_writer = new WaveFileWriterWithCounter(path, _captureSource.WaveFormat);
                _audioSources.Add(new AudioSource(_captureSource.WaveFormat));

                _captureSource.DataAvailable += OnCaptureDataAvailable;
                _captureSource.RecordingStopped += OnRecordingStopped;

                _captureSource.StartRecording();

                State = State.Recording;
            }
        }

        public void StopRecording()
        {
            _captureSource.StopRecording();
            _audioSources.Last().RewindStream();
            State = State.Stopped;
        }

        private void OnCaptureDataAvailable(object? sender, WaveInEventArgs e)
        {
            _audioSources.Last().MemoryStream.Write(e.Buffer, 0, e.BytesRecorded);
            //var peak = _audioService.PeakSampleFromBuffer(e.Buffer, e.BytesRecorded);
            //GraphChanged?.Invoke(this, new Point((_writer as WaveFileWriterWithCounter).Counter, peak));
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _captureSource.DataAvailable -= OnCaptureDataAvailable;
            _captureSource.RecordingStopped -= OnRecordingStopped;

            //_rawSourceWaveStream = new RawSourceWaveStream(_memoryStream, _captureSource.WaveFormat);

            //добавляем точку чтобы знать где конец списка, для графа
            //GraphChanged?.Invoke(this, new Point(-1, -1));
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            State = State.Stopped;
        }



        //костыль, так как не ясно как ещё замедлить аудио, алгоритмы стретча работают плохо
        private ISampleProvider SetHalfSpeed(ISampleProvider source, bool enabled)
        {
            if (enabled)
            {
                var st_wp = new SoundTouchWaveProvider(source.ToWaveProvider())
                {
                    RateChange = -50
                };

                var shifter_sp = new SmbPitchShiftingSampleProvider(st_wp.ToSampleProvider())
                {
                    PitchFactor = 2.0f
                };

                return shifter_sp;
            }
            else
            {
                return source;
            }
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

    public class AudioSource
    {
        public MemoryStream MemoryStream { get; set; }
        public RawSourceWaveStream RawSource { get; set; }
        public double RMS { get; set; }

        public AudioSource(WaveFormat waveFormat)
        {
            MemoryStream = new MemoryStream(); 
            RawSource = new RawSourceWaveStream(MemoryStream, waveFormat);
        }

        public void RewindStream() => MemoryStream.Position = 0;
    }
}
