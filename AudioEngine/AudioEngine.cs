using FancyCards.Audio.Common;
using NAudio.CoreAudioApi;
using NAudio.Utils;
using NAudio.Wave;
using System.Linq;
using System.Timers;


namespace FancyCards.Audio
{
    public class AudioEngine
    {
        private readonly AudioUtilities _utilities;
        private readonly AudioWaveform _audioWaveform;

        private readonly WaveOutEvent _outputDevice;
        private readonly MMDevice _captureDevice;
        private readonly WasapiCapture _captureSource;
        private AudioStateManager _audioStateManager;
        private PlaybackSettings _playbackSettings;

        private List<byte[]> _audioChunks;
        private System.Timers.Timer _playbackPositionTimer; 

        public event Action<State> StateChanged;
        public event Action<double> GraphChanged;
        public event Action<TimeSpan> AudioDurationChanged;
        public event Action<PlaybackPositionArgs> PlaybackPositionChanged;

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

        public AudioEngine()
        {
            _utilities = new AudioUtilities();
            _audioWaveform = new AudioWaveform();
            _outputDevice = new WaveOutEvent();

            _captureDevice = _utilities.GetDefaultOutputDevice();//_utilities.GetDeviceById(deviceId);
            _captureSource = _utilities.GetWasapiCaptureInstance(_captureDevice);

            _audioStateManager = new AudioStateManager(_captureSource.WaveFormat);

            _outputDevice.PlaybackStopped += OnPlaybackStopped;

            _playbackPositionTimer = new System.Timers.Timer(10)
            {
                AutoReset = true
            };
            _playbackPositionTimer.Elapsed += OnPlaybackPositionChanged;
        }


        public async void OpenAudioAsync(string path)
        {
            _audioStateManager.LoadFromAudioFile(path);

            var duration = GetDuration(_audioStateManager.CurrentData, _captureSource.WaveFormat);
            AudioDurationChanged?.Invoke(duration);

            State = State.Stopped;
        }

        public async Task<double[]> GetWaveformPoints(string path)
        {
            var points = _audioWaveform.GetPointsFromAudio(path);
            return points;
        }

        //--------------------------------------------------------------------------------------------------PLAYBACK-----------------------------------------------------

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
                    StartTime = startPosition * source.TotalTime,
                    EndTime = endPosition * source.TotalTime,
                    TargetRMS = targetRMS,
                    Tempo = tempo
                };

                var audio_pipeline = AudioPipeline.Create(source.ToSampleProvider(), settings);

                _playbackSettings = new PlaybackSettings
                {
                    Speed = (double)tempo * ((playbackSpeed == PlaybackSpeed.Half) ? 0.5 : 1),
                    TotalLength = source.Length / ((double)tempo * ((playbackSpeed == PlaybackSpeed.Half) ? 0.5 : 1)),
                    StartPosition = (startPosition * source.Length) / ((double)tempo * ((playbackSpeed == PlaybackSpeed.Half) ? 0.5 : 1)),
                };
                _playbackPositionTimer.Start();

                _outputDevice.Init(audio_pipeline); 
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
                PositionPercent = (double)(_outputDevice.GetPosition() + _playbackSettings.StartPosition) / _playbackSettings.TotalLength
                
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
        //----------------------------------------------------------------------------------------------------RECORDING---------------------------------------------------
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
            var duration = GetDuration(_audioChunks, _captureSource.WaveFormat);
            AudioDurationChanged?.Invoke(duration);

            //var peak = _audioService.PeakSampleFromBuffer(e.Buffer, e.BytesRecorded);
            //GraphChanged?.Invoke(this, new Point((_writer as WaveFileWriterWithCounter).Counter, peak));
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            _captureSource.DataAvailable -= OnCaptureDataAvailable;
            _captureSource.RecordingStopped -= OnRecordingStopped;

            _audioStateManager.SetData(GetSoundData(_audioChunks));

            //добавляем точку чтобы знать где конец списка, для графа
            //GraphChanged?.Invoke(this, new Point(-1, -1));
        }



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


        private TimeSpan GetDuration(List<byte[]> chunks, WaveFormat waveFormat)
        {
            var result = chunks.SelectMany(arr => arr).ToArray();

            var seconds = (double)result.Length / waveFormat.AverageBytesPerSecond;

            return TimeSpan.FromSeconds(seconds);
        }

        private TimeSpan GetDuration(byte[] bytes, WaveFormat waveFormat)
        {
            //TODO разобраться почему в 2 раза меньше
            var seconds = (double)bytes.Length * 2 / waveFormat.AverageBytesPerSecond ;

            return TimeSpan.FromSeconds(seconds);
        }

        public async Task RenderToMp3Async(string path, int bitRate = 128000)
        {
            await _audioStateManager.ExportToMp3Async(path, bitRate);
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
