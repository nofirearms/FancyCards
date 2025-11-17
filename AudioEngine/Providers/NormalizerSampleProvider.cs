using NAudio.Wave;

namespace FancyCards.Audio.Providers
{
    class NormalizerSampleProvider : ISampleProvider
    {
        private ISampleProvider _source;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);
            for (int i = 0; i < samplesRead; i++)
            {
                //buffer[i] = buffer[i] * (1 / _ratio);
                buffer[i] *= _ratio;
            }
            return samplesRead;
        }

        private float _ratio = 1;
        public float Ratio
        {
            get => _ratio;
            set => _ratio = value;
        }

        public NormalizerSampleProvider(ISampleProvider source)
        {
            _source = source;
        }

        public NormalizerSampleProvider(ISampleProvider source, float ratio)
        {
            _source = source;
            Ratio = ratio;
        }
    }
}
