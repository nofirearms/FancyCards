using System.Windows.Media;

namespace FancyCards.Models
{
    public class MessageBoxParameters
    {
        public string Header { get; set; }
        public string Message { get; set; }
        public string[] Buttons { get; set; }
        public Brush Background { get; set; }

        public MessageBoxParameters(string header, string message, string[] buttons, Brush background = null)
        {
            Header = header;
            Message = message;
            Buttons = buttons;
            Background = background;
        }
    }
}
