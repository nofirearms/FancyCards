using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Text;

namespace FancyCards.Audio
{

    public class AudioWaveform
    {
        /// <summary>
        /// Generates graph-based visualizations from audio input data
        /// </summary>
        public AudioWaveform()
        {

        }


        public double GetAmplitude(byte[] audioData, int bytesRecorded)
        {
            float[] samples = ConvertToSamples(audioData, bytesRecorded);
            double amplitude = CalculateAmplitude(samples);

            return amplitude;
        }


        /// <summary>
        /// Get points collection from stream to create a graph
        /// </summary>
        public double[] GetPointsFromAudio(string path)
        {
            var points = new List<double>();
            //todo если файл не существует
            using (var reader = new AudioFileReader(path))
            {
                //TODO переделать под float
                reader.Position = 0;
                
                var length = reader.Length / 200;
                var buffer = new byte[length - length % reader.WaveFormat.BlockAlign];
                while (reader.Read(buffer, 0, buffer.Length) > 0)
                {
                    var max = GetAmplitude(buffer, buffer.Length);
                    points.Add(max);
                }
            }

            return points.ToArray();
        }


        public double[] GetPointsFromBytes(byte[] source, WaveFormat waveFormat, int resolution = 256)
        {
            var result = new double[resolution];

            int bytesPerSample = waveFormat.BlockAlign;
            int totalBytes = source.Length;
            int bytesPerPoint = totalBytes / resolution;

            // Выравниваем по границе семпла
            bytesPerPoint = bytesPerPoint - (bytesPerPoint % bytesPerSample);
            if (bytesPerPoint < bytesPerSample) bytesPerPoint = bytesPerSample;

            for (int i = 0; i < resolution; i++)
            {
                int offset = i * bytesPerPoint;
                if (offset >= totalBytes) break;

                //int length = Math.Min(bytesPerPoint, totalBytes - offset);
                var chunk = source.Skip(offset).Take(bytesPerPoint).ToArray();

                result[i] = GetAmplitude(chunk, chunk.Length);
            }

            return result;
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
    }
}
