using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class TrainingSession : EntityBase
    {
        public DateTime Date { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public IEnumerable<TrainingSessionCard> Cards { get; set; }
    }
}
