using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class TrainingSessionCard : EntityBase
    {

        public int CardId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public int TrainingSessionId { get; set;  }


    }
}
