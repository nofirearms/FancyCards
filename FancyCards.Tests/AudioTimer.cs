using System;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX;

namespace FancyCards.Audio.Common
{


    public class AudioTimer : IDisposable
    {
        private readonly XAudio2 _xaudio;
        private readonly MasteringVoice _master;
        private readonly SourceVoice _voice;
        private readonly AudioBuffer _buffer;

        public event Action Tick;

        public AudioTimer(int intervalMs)
        {
            _xaudio = new XAudio2();
            _master = new MasteringVoice(_xaudio);

            // Формат аудио (можно любой)
            var format = new WaveFormat(44_100, 16, 1); // 48kHz mono

            _voice = new SourceVoice(_xaudio, format, true);

            // Длина интервала в сэмплах
            int samples = format.SampleRate * intervalMs / 1000;

            // Создаём тихий буфер нужной длины
            byte[] silence = new byte[samples * format.BlockAlign];

            _buffer = new AudioBuffer
            { 
                 Stream = DataStream.Create(silence, true, false),
                AudioBytes = silence.Length,
                Flags = BufferFlags.EndOfStream
            };

            _voice.BufferEnd += OnBufferEnd;
        }

        public void Start()
        {
            // Закидываем буфер и играем в кольце
            _voice.SubmitSourceBuffer(_buffer, null);
            _voice.Start();
        }

        public void Stop()
        {
            _voice.Stop();
        }

        public void Dispose()
        {
            _voice.DestroyVoice();
            _voice.Dispose();
            _master.Dispose();
            _xaudio.Dispose();
        }

        // -------- Voice callback --------

        public void OnBufferEnd(IntPtr pBufferContext)
        {
            Tick?.Invoke();

            // Зацикливаем
            _voice.SubmitSourceBuffer(_buffer, null);
        }

        public void OnVoiceProcessingPassStart(int bytesRequired) { }
        public void OnVoiceProcessingPassEnd() { }
        public void OnStreamEnd() { }
        public void OnLoopEnd(IntPtr bufferContext) { }
        public void OnBufferStart(IntPtr bufferContext) { }
        public void OnVoiceError(IntPtr bufferContext, SharpDX.Result error) { }
    }

}
