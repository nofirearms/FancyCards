using FancyCards.Audio;
using FancyCards.Audio.Common;

namespace FancyCards.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DoStuff().Wait();
            Console.ReadLine();
        }

        private static async Task DoStuff()
        {
            var timer = new AudioTimer(100);
            timer.Tick += () =>
            {
                Console.WriteLine(DateTime.Now.ToString());
            };
            timer.Start();
            Console.ReadLine();
            //engine.StartPlayback(playbackSpeed: PlaybackSpeed.Half);

        }
    }
}
