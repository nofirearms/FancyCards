using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio
{
    /// <summary>
    /// Класс который хранит состояния аудио для возможности Undo и Redo
    /// </summary>
    public class AudioStateManager
    {
        private byte[] _currentData;
        private readonly WaveFormat _format;

        private readonly Stack<byte[]> _undoStack = new Stack<byte[]>();
        private readonly Stack<byte[]> _redoStack = new Stack<byte[]>();

        public int MaxHistory = 25; // ограничение истории

        public AudioStateManager(WaveFormat format)
        {
            _format = format ?? throw new ArgumentNullException(nameof(format));
            _currentData = Array.Empty<byte>();
        }

        public IWaveProvider CreateWaveProvider()
        {
            return new RawSourceWaveStream(new MemoryStream(_currentData), _format);
        }

        public TimeSpan GetDuration()
        {
            double seconds = (double)_currentData.Length / _format.AverageBytesPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>Получить текущее PCM-состояние (копию)</summary>
        public byte[] GetDataCopy() => (byte[])_currentData?.Clone();

        /// <summary>Установить новое PCM-состояние (сбрасывает Redo)</summary>
        public void SetData(byte[] newData, bool createUndoPoint = false)
        {
            if (createUndoPoint) SaveState();
            _currentData = newData ?? Array.Empty<byte>();
            _redoStack.Clear();
        }

        /// <summary>Сохранить состояние перед модификацией</summary>
        public void SaveState()
        {
            if (_currentData == null) return;

            _undoStack.Push((byte[])_currentData.Clone());
            if (_undoStack.Count > MaxHistory)
                _undoStack.TrimExcess();

            _redoStack.Clear();
        }

        /// <summary>Есть ли Undo?</summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>Есть ли Redo?</summary>
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
                throw new FileNotFoundException("Audio file not found", path);

            using var reader = new AudioFileReader(path);

            // читаем все float сэмплы
            var floatSamples = new List<float>(reader.WaveFormat.SampleRate * reader.WaveFormat.Channels * 10);
            float[] tempBuffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
            int read;

            while ((read = reader.Read(tempBuffer, 0, tempBuffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                    floatSamples.Add(tempBuffer[i]);
            }

            // конвертация в PCM16
            byte[] pcmData = FloatArrayToPCM16(floatSamples.ToArray());

            // Обновляем формат менеджера
            _currentData = pcmData;
            typeof(AudioStateManager)
                .GetField("_format", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, new WaveFormat(reader.WaveFormat.SampleRate, 16, reader.WaveFormat.Channels));

            if (createUndoPoint) SaveState();

            // Очищаем Redo стек
            _redoStack.Clear();
        }

        /// <summary> Конвертация float → PCM16 </summary>
        private byte[] FloatArrayToPCM16(float[] samples)
        {
            byte[] result = new byte[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                float f = Math.Clamp(samples[i], -1f, 1f);
                short val = (short)(f * short.MaxValue);
                result[i * 2] = (byte)(val & 0xFF);
                result[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }

            return result;
        }

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

        // ========== AUDIO OPERATIONS EXAMPLES ========== //

        /// <summary>Обрезать по сэмплам</summary>
        public void Trim(int startSample, int endSample)
        {
            if (_currentData.Length == 0) return;
            SaveState();

            int bytesPerSample = _format.BitsPerSample / 8 * _format.Channels;
            int startByte = startSample * bytesPerSample;
            int endByte = endSample * bytesPerSample;

            startByte = Math.Clamp(startByte, 0, _currentData.Length);
            endByte = Math.Clamp(endByte, startByte, _currentData.Length);

            _currentData = _currentData.Skip(startByte).Take(endByte - startByte).ToArray();
        }

        /// <summary>Нормализация 16-bit PCM</summary>
        public void Normalize()
        {
            if (_currentData.Length == 0 || _format.BitsPerSample != 16) return;
            SaveState();

            short max = 0;
            for (int i = 0; i < _currentData.Length; i += 2)
            {
                short sample = BitConverter.ToInt16(_currentData, i);
                if (Math.Abs(sample) > max)
                    max = Math.Abs(sample);
            }

            if (max == 0) return;
            float gain = 32767f / max;

            for (int i = 0; i < _currentData.Length; i += 2)
            {
                short sample = BitConverter.ToInt16(_currentData, i);
                int newSample = (int)(sample * gain);
                newSample = Math.Clamp(newSample, short.MinValue, short.MaxValue);
                byte[] b = BitConverter.GetBytes((short)newSample);
                _currentData[i] = b[0];
                _currentData[i + 1] = b[1];
            }
        }


        public void CreateDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
