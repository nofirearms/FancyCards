using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace FancyCards.Models
{
    [Owned]
    public class CardScores
    {
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        /// <summary>
        /// интервал (в днях)
        /// </summary>
        public int I { get; set; } = 0;
        /// <summary>
        /// число успешных повторений подряд
        /// </summary>
        public int Reps { get; set; } = 0;
        /// <summary>
        /// ease factor
        /// </summary>
        public double EF { get; set; } = 2.5; 



    }
}