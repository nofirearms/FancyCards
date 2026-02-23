using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class Deck : EntityBase
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        public ICollection<Card> Cards { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DeckSettings Settings { get; set; } = new();

    }
}
