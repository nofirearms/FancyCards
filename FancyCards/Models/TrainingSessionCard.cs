using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class TrainingSessionCard : EntityBase
    {

        public int CardId { get; set; }
        /// <summary>
        /// оценка, 0 - неудача, 3 - сложно, 5 - нормально
        /// </summary>
        public int Q { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public TrainingCardResult Result { get; set; }
        public CardState CardState { get; set; }

    }
}
