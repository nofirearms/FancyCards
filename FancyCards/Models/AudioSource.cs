using Microsoft.EntityFrameworkCore;

namespace FancyCards.Models
{
    [Owned]
    public class AudioSource
    {
        public string Path { get; set; }
        public double StartPosition { get; set; }
        public double EndPosition { get; set; }
        public double Volume { get; set; } = 0.4;
        public double Tempo { get; set; } = 1.0;
    }
}