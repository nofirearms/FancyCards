using FancyCards.Audio.Common;
using FancyCards.Audio.Providers;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundTouch.Net.NAudioSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio
{
    public class AudioPipeline
    {
        private RawSourceWaveStream _source;

        public double PlaybackLength { get; private set; }
        public double PlaybackStartPosition { get; private set;  }

        public event Action<StreamVolumeEventArgs> StreamVolume;

        public ISampleProvider Build(RawSourceWaveStream source, AudioSettings settings)
        {
            _source = source;
            var output = source.ToSampleProvider();

            output = ApplyRMSNormalizer(output, settings.TargetRMS);
            output = ApplyOffset(output, settings.StartPosition * source.TotalTime, settings.EndPosition * source.TotalTime);
            output = ApplySlowMotion(output, settings.SlowMotion);
            output = ApplyTempo(output, settings.Tempo);
            output = ApplyVolume(output, settings.Volume);
            output = ApplyMetering(output);

            //для отображения current playback position
            PlaybackLength = source.Length / ((double)settings.Tempo * ((settings.SlowMotion) ? 0.5 : 1));
            PlaybackStartPosition = (settings.StartPosition * source.Length) / ((double)settings.Tempo * ((settings.SlowMotion) ? 0.5 : 1));
            //Speed = (double)tempo * ((playbackSpeed == PlaybackSpeed.Half) ? 0.5 : 1),

            return output;
        }
        private ISampleProvider ApplyRMSNormalizer(ISampleProvider source, float targetRms, bool enabled = true)
        {
            //TODO переделать, чтоб кешировалось только один раз при открытии файла
            var cached_sp = new CachedSampleProvider(source);
            var rms = cached_sp.RMS;
            cached_sp.Reset();

            var normalized = new NormalizerSampleProvider(cached_sp)
            {
                Ratio = (float)(targetRms / rms)
            };

            return normalized;
        }
        private ISampleProvider ApplySlowMotion(ISampleProvider source, bool enabled)
        {
            if (!enabled) return source;

            var timeStretch = new SoundTouchWaveProvider(source.ToWaveProvider())
            { RateChange = -50 };

            return new SmbPitchShiftingSampleProvider(timeStretch.ToSampleProvider())
            { PitchFactor = 2.0f };
        }

        private ISampleProvider ApplyTempo(ISampleProvider source, float tempo = 1.0f)
        {
            var rate_provider = new SoundTouchWaveProvider(source.ToWaveProvider())
            {
                Tempo = tempo
            };
            return rate_provider.ToSampleProvider();
        }

        private ISampleProvider ApplyOffset(ISampleProvider source, TimeSpan start, TimeSpan end)
        {
            var offset_sp = new OffsetSampleProvider(source)
            {
                SkipOver = start,
                Take = end - start
            };

            return offset_sp;
        }

        private ISampleProvider ApplyVolume(ISampleProvider source, float volume)
        {
            return new VolumeSampleProvider(source) { Volume = volume };
        }

        private ISampleProvider ApplyMetering(ISampleProvider source)
        {
            var metering = new MeteringSampleProvider(source);
            metering.StreamVolume += (_, v) => StreamVolume?.Invoke(v);

            return metering;
        }

        //private ISampleProvider ApplyMetering(ISampleProvider source)
        //{
        //    var metering = new MeteringSampleProvider(source);
        //    metering.StreamVolume += (_, v) => StreamVolume?.Invoke(v);

        //    return metering;
        //}

    }
    
    public class AudioSettings
    {
        public float TargetRMS { get; set; }
        public bool SlowMotion { get; set; }
        public float Volume { get; set; } = 1.0f;
        public float Tempo { get; set; } = 1.0f;
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }

    }


}
