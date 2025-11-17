using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Models
{
    public enum CardState
    {
        Learning,   // В процессе изучения
        Reviewing,  // На повторении
        Mastered    // Выучена
    }

    public enum DeckAction
    {
        Create, Remove, Update
    }

    public enum CardAction
    {
        Create, Remove, Update
    }
}
