using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    [Owned]
    public class DeckSettings
    {
        public int TrainingLearnCards { get; set; } = 6;
        public int TrainingReviewCards { get; set; } = 14;
        public int СorrectAnswersToFinishLearning { get; set; } = 2;
        public int СorrectAnswersToFinishReviewing { get; set; } = 2;
    }
}
