using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public class Setting : EntityBase
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int DeckId { get; set; }

        public Setting() { }    

        public Setting(string key, string value, string description)
        {
            Key = key;
            Value = value;
            Description = description;
        }
    }
}
