using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models.Param
{
    public class TrainingFailedAnswerParameters
    {
        public string Answer { get; set; }
        public string FrontText { get; set; }
        public TrainingFailedAnswerParameters(string answer, string frontText)
        {
            Answer = answer;
            FrontText = frontText;
        }
    }
}
