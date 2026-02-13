using System;
using System.Collections.Generic;
using System.Text;

namespace FancyCards.Audio.Common
{
    public class PlaybackPositionArgs
    {
        public TimeSpan PositionTimeSpan { get; set; }
        public double PositionPercent { get; set; }
    }

    public class AudioSourceChangedArgs
    {
        public TimeSpan Duration { get; set; }
        public bool CanUndo { get; set; }
    }
}
