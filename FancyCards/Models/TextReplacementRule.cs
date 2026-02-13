using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    internal class TextReplacementRule : EntityBase
    {
        public string Original { get; set; } 
        public string Replacement { get; set; }
    }
}
