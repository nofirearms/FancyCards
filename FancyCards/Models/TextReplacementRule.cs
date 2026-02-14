using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class TextReplacementRule : EntityBase
    {
        public string Original { get; set; } 
        public string Replacement { get; set; }

        public TextReplacementRule(string original, string replacement = "") 
        {
            Original = original;
            Replacement = replacement;
        }
    }
}
