using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio
{
    public class UniversalWaveformRenderer
    {
        private readonly double[] _points;
        private readonly int _width;
        private int _currentIndex;

        public UniversalWaveformRenderer(int width = 800)
        {
            _width = width;
            _points = new double[width];
        }

        // ДЛЯ РЕАЛЬНОГО ВРЕМЕНИ (запись)
        public void ProcessAudioChunk(byte[] audioData, int bytesRecorded)
        {
            float[] samples = ConvertToSamples(audioData, bytesRecorded);
            double amplitude = CalculateAmplitude(samples);

            AddPoint(amplitude);
        }

        // ДЛЯ ФАЙЛА (открытие)
        public void LoadFromSamples(float[] allSamples)
        {
            int samplesPerPoint = allSamples.Length / _width;

            for (int i = 0; i < _width; i++)
            {
                int start = i * samplesPerPoint;
                int end = Math.Min(start + samplesPerPoint, allSamples.Length);

                double amplitude = CalculateAmplitude(allSamples, start, end);
                _points[i] = amplitude;
            }

            UpdateWaveform?.Invoke(_points);
        }

        private void AddPoint(double amplitude)
        {
            _points[_currentIndex] = amplitude;
            _currentIndex = (_currentIndex + 1) % _width;
            UpdateWaveform?.Invoke(_points);
        }

        private double CalculateAmplitude(float[] samples, int start = 0, int end = -1)
        {
            if (end == -1) end = samples.Length;

            double max = 0;
            for (int i = start; i < end; i++)
            {
                double abs = Math.Abs(samples[i]);
                if (abs > max) max = abs;
            }
            return max;
        }

        private float[] ConvertToSamples(byte[] audioData, int bytesRecorded)
        {
            int sampleCount = bytesRecorded / 4;
            float[] samples = new float[sampleCount];
            Buffer.BlockCopy(audioData, 0, samples, 0, bytesRecorded);
            return samples;
        }

        public event Action<double[]> UpdateWaveform;
    }
}
