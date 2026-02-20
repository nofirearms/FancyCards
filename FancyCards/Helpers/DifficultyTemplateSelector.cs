using FancyCards.Models;
using System.Windows;
using System.Windows.Controls;

namespace FancyCards.Helpers
{
    public class DifficultyTemplateSelector : DataTemplateSelector
    {

        public DataTemplate EasyTemplate { get; set; }
        public DataTemplate NormalTemplate { get; set; }
        public DataTemplate HardTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = (Difficulty)item;

            return type switch
            {
                Difficulty.Easy => EasyTemplate,
                Difficulty.Normal => NormalTemplate,
                Difficulty.Hard => HardTemplate,
                 _ => null
            };
        }
    }
}
