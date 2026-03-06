using FancyCards.Audio.Common;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;

namespace FancyCards.Audio
{
    /// <summary>
    /// Класс который хранит состояния аудио для возможности Undo и Redo
    /// </summary>
    public class AudioStateManager : IDisposable
    {
        public event Action<AudioSourceChangedArgs> SourceChanged;

        
        private WaveFormat _format;

        private readonly Stack<byte[]> _undoStack = new Stack<byte[]>();
        private readonly Stack<byte[]> _redoStack = new Stack<byte[]>();

        private int MaxHistory = 25; // ограничение истории

        public WaveFormat Format => _format; 

        public AudioStateManager(WaveFormat format)
        {
            _format = format ?? throw new ArgumentNullException(nameof(format));
            _currentData = Array.Empty<byte>(); 
        }

        private byte[] _currentData;
        public byte[] CurrentData
        {
            get => _currentData;
            set
            {
                _currentData = value;
                if(_currentData != null)
                {
                    SourceChanged?.Invoke(new AudioSourceChangedArgs
                    {
                        CanUndo = CanUndo,
                        Duration = GetDuration()
                    });
                }
                
            }
        }

        public IWaveProvider CreateWaveProvider()
        {
            return new RawSourceWaveStream(new MemoryStream(CurrentData), _format);
        }

        public byte[] GetDataCopy() => (byte[])CurrentData?.Clone();

        public void SetData(byte[] newData, bool createUndoPoint = false)
        {
            if (createUndoPoint) SaveState();
            CurrentData = newData ?? Array.Empty<byte>();
            _redoStack.Clear();
        }

        public void SaveState()
        {
            if (CurrentData == null) return;

            _undoStack.Push((byte[])CurrentData.Clone());
            if (_undoStack.Count > MaxHistory)
                _undoStack.TrimExcess();

            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;

        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo) return;

            _redoStack.Push((byte[])CurrentData.Clone());
            CurrentData = _undoStack.Pop();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            _undoStack.Push((byte[])CurrentData.Clone());
            CurrentData = _redoStack.Pop();
        }

        public bool LoadFromAudioFile(string path, bool createUndoPoint = false, bool clearHistory = false)
        {
            try
            {
                if (!File.Exists(path))
                {
                    CurrentData = Array.Empty<byte>();
                    return false;
                }
                    

                using (var reader = new AudioFileReader(path))
                using (var ms = new MemoryStream())
                {
                    reader.CopyTo(ms);
                    byte[] allBytes = ms.ToArray();

                    if (clearHistory) _undoStack.Clear();

                    SetData(allBytes, createUndoPoint);
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool LoadFromStream(Stream stream, bool createUndoPoint = false, bool clearHistory = false)
        {
            try
            {
                if(stream is null) return false;

                using (var reader = new Mp3FileReader(stream))
                using (var ms = new MemoryStream())
                {

                    reader.CopyTo(ms);
                    byte[] allBytes = ms.ToArray();

                    if (clearHistory) _undoStack.Clear();

                    SetData(allBytes, createUndoPoint);

                    _format = reader.WaveFormat;

                }
                return true;
            }
            
            catch
            {
                return false;
            }

        }


        public TimeSpan GetDuration()
        {   
            double seconds = (double)CurrentData.Length / _format.AverageBytesPerSecond;
            return TimeSpan.FromSeconds(seconds);
        }

        public void Dispose()
        {
            CurrentData = null;
            _undoStack.Clear();
            _undoStack.Clear();
        }


    }
}
