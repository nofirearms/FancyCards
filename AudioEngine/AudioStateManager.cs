using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio
{
    /// <summary>
    /// Класс который хранит состояния аудио для возможности Undo и Redo
    /// </summary>
    public class AudioStateManager : IDisposable
    {
        private byte[] _currentData;
        private readonly WaveFormat _format;

        private readonly Stack<byte[]> _undoStack = new Stack<byte[]>();
        private readonly Stack<byte[]> _redoStack = new Stack<byte[]>();

        private int MaxHistory = 25; // ограничение истории

        public WaveFormat Format => _format;

        public AudioStateManager(WaveFormat format)
        {
            _format = format ?? throw new ArgumentNullException(nameof(format));
            _currentData = Array.Empty<byte>();
        }

        public byte[] CurrentData => _currentData;

        public IWaveProvider CreateWaveProvider()
        {
            return new RawSourceWaveStream(new MemoryStream(_currentData), _format);
        }

        //public TimeSpan GetDuration()
        //{
        //    double seconds = (double)_currentData.Length / _format.AverageBytesPerSecond;
        //    return TimeSpan.FromSeconds(seconds);
        //}

        public byte[] GetDataCopy() => (byte[])_currentData?.Clone();

        public void SetData(byte[] newData, bool createUndoPoint = false)
        {
            if (createUndoPoint) SaveState();
            _currentData = newData ?? Array.Empty<byte>();
            _redoStack.Clear();
        }

        public void SaveState()
        {
            if (_currentData == null) return;

            _undoStack.Push((byte[])_currentData.Clone());
            if (_undoStack.Count > MaxHistory)
                _undoStack.TrimExcess();

            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;

        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo) return;

            _redoStack.Push((byte[])_currentData.Clone());
            _currentData = _undoStack.Pop();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            _undoStack.Push((byte[])_currentData.Clone());
            _currentData = _redoStack.Pop();
        }

        public void LoadFromAudioFile(string path, bool createUndoPoint = false)
        {
            if (!File.Exists(path))
                return;

            using (var reader = new AudioFileReader(path))
            using (var ms = new MemoryStream())
            {
                reader.CopyTo(ms);
                byte[] allBytes = ms.ToArray();
                _currentData = allBytes;
            }

            if (createUndoPoint) SaveState();

            // Очищаем Redo стек
            _redoStack.Clear();
        }
        //--------------------------------------------------------- EXPORT ------------------------------------------
        #region EXPORT

        /// <summary>Экспорт текущего PCM в WAV файл</summary>
        public void ExportToWav(string path)
        {
            using var writer = new WaveFileWriter(path, _format);
            writer.Write(_currentData, 0, _currentData.Length);
        }

        public async Task ExportToMp3Async(string path, int bitRate)
        {
            await Task.Run(() =>
            {
                CreateDirectory(path);
                using (var resampler = new MediaFoundationResampler(this.CreateWaveProvider(), _format))
                {
                    MediaFoundationEncoder.EncodeToMp3(resampler, path, bitRate);
                }
            });
        }

        #endregion


        /// <summary>Обрезать 0 - 1</summary>
        public void Trim(double startPosition, double endPosition)
        {
            int bytesPerSample = _format.BlockAlign;
            int startByte = (int)(startPosition * _currentData.Length);
            int endByte = (int)(endPosition * _currentData.Length);

            startByte = startByte - (startByte % bytesPerSample);
            endByte = endByte - (endByte % bytesPerSample);

            if (_currentData.Length == 0) return;
            SaveState();

            _currentData = _currentData.Skip(startByte).Take(endByte - startByte).ToArray();
        }


        public void Cut(double startPosition, double endPosition)
        {
            int bytesPerSample = _format.BlockAlign;
            int startByte = (int)(startPosition * _currentData.Length);
            int endByte = (int)(endPosition * _currentData.Length);

            startByte = startByte - (startByte % bytesPerSample);
            endByte = endByte - (endByte % bytesPerSample);

            if (_currentData.Length == 0) return;
            SaveState();

            _currentData = _currentData.Take(startByte)
                                       .Concat(_currentData.Skip(endByte))
                                       .ToArray();
        }

        public void CreateDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Dispose()
        {
            _currentData = null;
            _undoStack.Clear();
            _undoStack.Clear();
        }


    }
}
