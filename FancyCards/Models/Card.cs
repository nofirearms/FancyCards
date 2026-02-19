using FancyCards.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class Card : EntityBase
    {
        public string FrontText { get; set; }
        public string BackText { get; set; }
        public string PrefixText { get; set; }
        public string SuffixText { get; set; }
        public string CommentText { get; set; }
        public string MessageText { get; set; }
        public CardState State { get; set; }
        public CardScores Scores { get; set; } = new();
        public AudioSource Audio { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime NextReviewDate { get; set; }
        public DateTime LastReviewDate { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public int DeckId { get; set; }
    }
}
