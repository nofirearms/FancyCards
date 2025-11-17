using FancyCards.Audio; 

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
            var engine = new AudioEngine();
            //await engine.OpenAudioAsync("D:\\downloads\\Sound20210923_141107.mp3");

            Console.WriteLine("Press enter to record");
            Console.ReadLine();
            engine.StartRecording();
            await Task.Delay(3000);
            engine.StopRecording();

            Console.WriteLine("Recorded 3 secs, press enter to play");
            Console.ReadLine();
            engine.StartPlayback();
            Console.WriteLine("Playback");
            Console.ReadLine();
            //engine.StartPlayback(playbackSpeed: PlaybackSpeed.Half);

        }
    }
}
