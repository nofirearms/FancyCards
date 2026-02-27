namespace FancyPhrases.Models
{
    public  class Sound
    {
        public string Path { get; set; }
        public long StartPosition { get; set; }
        public long StopPosition { get; set; }
        public float Volume { get; set; } = 0.4f;

        public double Tempo { get; set; } = 1.0;
    }
}
