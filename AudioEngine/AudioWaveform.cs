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
