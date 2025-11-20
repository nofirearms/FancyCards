using FancyCards.Audio.Providers;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundTouch.Net.NAudioSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio
{
    public static class AudioPipeline
    {
        
        public static ISampleProvider Create(ISampleProvider source, AudioSettings settings)
        {
            return source
                .ApplyRMSNormalizer(settings.TargetRMS)
                .ApplyOffset(settings.StartTime, settings.EndTime)
                .ApplySlowMotion(settings.SlowMotion)
                .ApplyTempo(settings.Tempo)
                .ApplyVolume(settings.Volume);
        }
        private static ISampleProvider ApplyRMSNormalizer(this ISampleProvider source, float targetRms, bool enabled = true)
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
        private static ISampleProvider ApplySlowMotion(this ISampleProvider source, bool enabled)
        {
            if (!enabled) return source;

            var timeStretch = new SoundTouchWaveProvider(source.ToWaveProvider())
            { RateChange = -50 };

            return new SmbPitchShiftingSampleProvider(timeStretch.ToSampleProvider())
            { PitchFactor = 2.0f };
        }

        private static ISampleProvider ApplyTempo(this ISampleProvider source, float tempo = 1.0f)
        {
            var rate_provider = new SoundTouchWaveProvider(source.ToWaveProvider())
            {
                Tempo = tempo
            };
            return rate_provider.ToSampleProvider();
        }

        private static ISampleProvider ApplyOffset(this ISampleProvider source, TimeSpan start, TimeSpan end)
        {
            var offset_sp = new OffsetSampleProvider(source)
            {
                SkipOver = start,
                Take = end - start
            };

            return offset_sp;
        }

        private static ISampleProvider ApplyVolume(this ISampleProvider source, float volume)
        {
            return new VolumeSampleProvider(source) { Volume = volume };
        }
    }
    
    public class AudioSettings
    {
        public float TargetRMS { get; set; }
        public bool SlowMotion { get; set; }
        public float Volume { get; set; } = 1.0f;
        public float Tempo { get; set; } = 1.0f;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

    }


}
