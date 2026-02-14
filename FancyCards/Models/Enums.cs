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

    public enum TrainingCardState
    {
        Queue, Success, Failed
    }


    public enum PlaybackMode
    {
        Selected, Full, SelectedSlow
    }

    public struct Selection
    {
        public double Start { get; set; }
        public double End { get; set; }

        public Selection(double start, double end)
        {
            Start = start;
            End = end;
        }

        public Selection()
        {
            
        }
    }
}
