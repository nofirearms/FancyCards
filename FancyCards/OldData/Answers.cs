using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FancyPhrases.Models
{
    public class Answers
    {
        public int RepeatTotal { get; set; }
        public int RepeatCorrect { get; set; }

        public int LearnTotal { get; set; }
        public int LearnCorrect { get; set; }

        public int GetCorrectSum => RepeatCorrect + LearnCorrect;
        public int GetTotalSum => RepeatTotal + LearnTotal;
    }
}
