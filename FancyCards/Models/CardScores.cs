using Microsoft.EntityFrameworkCore;

namespace FancyCards.Models
{
    [Owned]
    public class CardScores
    {
        public int LearningScore { get; set; }
        public int ReviewingScore { get; set; }
    }
}