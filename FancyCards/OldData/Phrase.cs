using System;

namespace FancyPhrases.Models
{
    public class Phrase
    {
        public int Id { get; set; }
        public string PreOriginal { get; set; }
        public string Original { get; set; }
        public string PostOriginal { get; set; }
        public string Translation { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ClosestDate { get; set; }
        public Answers Answers { get; set; }
        public Sound Sound { get; set; }
        public PhraseState State { get; set; }
        public string Remark { get; set; }
        //public int CorrectAnswers { get; set; }
        //public int TotalAnswers { get; set; }
    }
}
