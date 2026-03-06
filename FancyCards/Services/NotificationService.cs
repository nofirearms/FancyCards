using FancyCards.Audio;
using FancyPhrases.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Text;

namespace FancyCards.Services
{
    public class NotificationService
    {
        private readonly AudioEngine _audioEngine;

        public NotificationService(AudioEngine audioEngine)
        {
            _audioEngine = audioEngine;
        }

        public void Play(Notification notification)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var a = assembly.GetName().Name;

            string path = notification switch
            {
                Notification.Success => "FancyCards.Resources.Sounds.SuccessSound.mp3",
                Notification.Failure => "FancyCards.Resources.Sounds.FailureSound.mp3"
            };

            var stream = assembly.GetManifestResourceStream(path);

            _audioEngine.OpenAudioResourceStreamAsync(stream);
            
            _audioEngine.StartPlayback(volume:0.2f);
        }
    }

    public enum Notification
    {
        Success, Failure, CardArchived
    }
}
