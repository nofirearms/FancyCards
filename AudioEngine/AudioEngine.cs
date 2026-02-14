using FancyCards.Audio.Common;
using NAudio.CoreAudioApi;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Timers;


namespace FancyCards.Audio
{
    public class AudioEngine : IDisposable
    {
        private readonly AudioUtilities _utilities;
        private readonly AudioWaveform _audioWaveform;

        private readonly WaveOutEvent _outputDevice;
        private readonly AudioPipeline _audioPipeline;
        private readonly MMDevice _captureDevice;
        private readonly WasapiCapture _captureSource;
        private AudioStateManager _audioStateManager;

        private List<byte[]> _audioChunks;
        private System.Timers.Timer _playbackPositionTimer; 

        public event Action<State> StateChanged;
        public event Action<double> GraphChanged;
        public event Action<PlaybackPositionArgs> PlaybackPositionChanged;
        public event Action<double> MaxSampleVolume;
        public event Action<AudioSourceChangedArgs> AudioSourceChanged;

        private State _state = State.Initial;


        public State State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(value);
            }
        }



        public TimeSpan Duration => GetDuration(_audioStateManager.CurrentData ?? Array.Empty<byte>(), _audioStateManager.Format);

        public bool AudioChanged => _audioStateManager.CanUndo;

        public AudioEngine()
        {
            _utilities = new AudioUtilities();
            _audioWaveform = new AudioWaveform();
            _outputDevice = new WaveOutEvent();
            _audioPipeline = new AudioPipeline();

            _captureDevice = _utilities.GetDefaultOutputDevice();//_utilities.GetDeviceById(deviceId);
            _captureSource = _utilities.GetWasapiCaptureInstance(_captureDevice);

            _audioStateManager = new AudioStateManager(_captureSource.WaveFormat);

            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _playbackPositionTimer = new System.Timers.Timer(50)
            {
                AutoReset = true
            };
            _playbackPositionTimer.Elapsed += OnPlaybackPositionChanged;
            _audioPipeline.StreamVolume += OnStreamVolume;

            _audioStateManager.SourceChanged += (source) => AudioSourceChanged?.Invoke(source);
        }


        //createUndoPoint = true, чтобы определять было ли изменение аудио, чтобы экспортировать
        public bool OpenAudioAsync(string path, bool createUndoPoint = false, bool clearHistory = false)
        {
            StopPlayback();

            var result = _audioStateManager.LoadFromAudioFile(path, createUndoPoint, clearHistory);

            State = State.Stopped;

            return result;
        }



        //----------------------------------------------------------------------------------------------------- PLAYBACK ---------------------------------------------------
        #region PLAYBACK
        public void StartPlayback(double startPosition = 0, double endPosition = 1, PlaybackSpeed playbackSpeed = PlaybackSpeed.Full, float volume = 0.4f, float tempo = 1.0f, float targetRMS = 0.2f)
        {         

            if (State == State.Recording || State == State.Initial)
            {
                return;
            }
            else if (State == State.Playing)
            {
                StopPlayback();
                StartPlayback(startPosition, endPosition, playbackSpeed, volume, tempo, targetRMS);
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
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    TargetRMS = targetRMS,
                    Tempo = tempo,

                };

                var output = _audioPipeline.Build(source, settings);

                _playbackPositionTimer.Start();

                _outputDevice.Init(output); 
                _outputDevice.Play();

                State = State.Playing;
            }
        }

        public void StopPlayback()
        {
            State = State.Stopped;
            _playbackPositionTimer.Stop();
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

        private void OnPlaybackPositionChanged(object? sender, ElapsedEventArgs e)
        {
            if (_outputDevice.PlaybackState == PlaybackState.Stopped) return;

            //var seconds = (double)_outputDevice.GetPosition() / _outputDevice.OutputWaveFormat.AverageBytesPerSecond;
            //var time = TimeSpan.FromSeconds(seconds);

            PlaybackPositionChanged?.Invoke(new PlaybackPositionArgs
            {
                PositionTimeSpan = _outputDevice.GetPositionTimeSpan(),
                PositionPercent = (double)(_outputDevice.GetPosition() + _audioPipeline.PlaybackStartPosition) / _audioPipeline.PlaybackLength
            });
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            //если остановка произошла по окончанию воспроизведения файла
            if ((sender as WaveOutEvent).PlaybackState == PlaybackState.Stopped)
            {
                State = State.Stopped;
                _playbackPositionTimer.Stop();
            }

        }

        private void OnStreamVolume(StreamVolumeEventArgs args)
        {
            MaxSampleVolume?.Invoke(Math.Clamp(args.MaxSampleValues.Max(), 0, 1));
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------- RECORDING --------------------------------------------------
        #region RECORDING
        public async Task StartRecording()
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
                StopRecording();
                StartRecording();
            }
            else if (State == State.Stopped || State == State.Initial)
            {
                _audioChunks = new List<byte[]>();

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
            var chunk = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, 0, chunk, 0, e.BytesRecorded);
            _audioChunks.Add(chunk);

            var peak = _audioWaveform.GetAmplitude(e.Buffer, e.BytesRecorded);
            GraphChanged?.Invoke(peak);

            //TODO возможно лучше переделать через таймер
            //var duration = GetDuration(_audioChunks, _captureSource.WaveFormat);
            //AudioDurationChanged?.Invoke(duration);

            //var peak = _audioService.PeakSampleFromBuffer(e.Buffer, e.BytesRecorded);
            //GraphChanged?.Invoke(this, new Point((_writer as WaveFileWriterWithCounter).Counter, peak));
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _captureSource.DataAvailable -= OnCaptureDataAvailable;
            _captureSource.RecordingStopped -= OnRecordingStopped;

            if (_audioStateManager is null) return;

            //если _audioStateManager.CurrentData == null значит 
            _audioStateManager.SetData(GetSoundData(_audioChunks), true);

            //RecordingStopped?.Invoke(true);

            //добавляем точку чтобы знать где конец списка, для графа
            //GraphChanged?.Invoke(this, new Point(-1, -1));
        }

        #endregion

        //------------------------------------------------------------------------------------------------------ GRAPH -----------------------------------------------------
        #region Graph
        public async Task<double[]> GetWaveformPoints(string path)
        {
            var points = _audioWaveform.GetPointsFromAudio(path);
            return points;
        }

        public async Task<double[]> GetWaveformPoints()
        {
            var points = _audioWaveform.GetPointsFromBytes(_audioStateManager.CurrentData, _audioStateManager.Format);
            return points;
        }

        #endregion

        public byte[] GetSoundData(List<byte[]> chunks)
        {
            // Объединяем все чанки в один массив
            int totalSize = chunks.Sum(chunk => chunk.Length);
            byte[] result = new byte[totalSize];

            int offset = 0;
            foreach (byte[] chunk in chunks)
            {
                Buffer.BlockCopy(chunk, 0, result, offset, chunk.Length);
                offset += chunk.Length;
            }
            return result;
        }




        public void Trim(double startPosition = 0, double endPosition = 1)
        {
            StopPlayback();

            var trim = _utilities.Trim(_audioStateManager.CurrentData, startPosition, endPosition, _audioStateManager.Format);

            _audioStateManager.SetData(trim, true);

            State = State.Stopped;
        }

        public void Cut(double startPosition = 0, double endPosition = 1)
        {
            StopPlayback();

            var cut = _utilities.Cut(_audioStateManager.CurrentData, startPosition, endPosition, _audioStateManager.Format);

            _audioStateManager.SetData(cut, true);

            State = State.Stopped;
        }

        public void Undo()
        {
            StopPlayback();

            _audioStateManager.Undo();

            State = State.Stopped;
        }

        public void Redo()
        {
            StopPlayback();

            _audioStateManager.Redo();

            State = State.Stopped;
        }

        private TimeSpan GetDuration(List<byte[]> chunks, WaveFormat waveFormat)
        {
            var result = chunks.SelectMany(arr => arr).ToArray();

            var seconds = (double)result.Length / waveFormat.AverageBytesPerSecond;

            return TimeSpan.FromSeconds(seconds);
        }

        private TimeSpan GetDuration(byte[] bytes, WaveFormat waveFormat)
        {
            var seconds = (double)bytes.Length / waveFormat.AverageBytesPerSecond ;

            return TimeSpan.FromSeconds(seconds);
        }

        public async Task RenderToMp3Async(string path, int bitRate = 128000)
        {
            await _utilities.ExportToMp3Async(path, _audioStateManager.CurrentData, _audioStateManager.Format, bitRate);
        }



        public void Dispose()
        {
            if (State == State.Recording) StopRecording();
            else if (State == State.Playing) StopPlayback();

            _outputDevice.PlaybackStopped -= OnPlaybackStopped;
            _playbackPositionTimer.Elapsed -= OnPlaybackPositionChanged;
            _playbackPositionTimer.Dispose();
            _audioPipeline.StreamVolume -= OnStreamVolume;

            _audioStateManager.Dispose();
            _audioStateManager = null;
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
