using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio.Providers
{
    public class CachedSampleProvider : ISampleProvider
    {
        private readonly float[] _cachedSamples;
        private int _position;

        public WaveFormat WaveFormat { get; }
        public double RMS { get; }

        public CachedSampleProvider(ISampleProvider source)
        {
            WaveFormat = source.WaveFormat;

            var samples = new List<float>();
            float[] buffer = new float[1024]; // Уменьшил размер буфера
            int read;

            double sumSq = 0;
            long sampleCount = 0;

            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    samples.Add(buffer[i]);
                    sumSq += buffer[i] * buffer[i];
                }
                sampleCount += read;
            }

            _cachedSamples = samples.ToArray();
            RMS = sampleCount > 0 ? Math.Sqrt(sumSq / sampleCount) : 1e-9;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (_position >= _cachedSamples.Length)
                return 0; // Конец данных

            int samplesToCopy = Math.Min(count, _cachedSamples.Length - _position);

            // Простое копирование float[] в float[]
            for (int i = 0; i < samplesToCopy; i++)
            {
                buffer[offset + i] = _cachedSamples[_position + i];
            }

            _position += samplesToCopy;
            return samplesToCopy;
        }

        public void Reset() => _position = 0;
    }
}
