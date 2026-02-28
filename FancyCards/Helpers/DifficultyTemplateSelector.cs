using FancyCards.Models;
using FancyCards.ViewModels;
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

    public class CardStateTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReviewingTemplate { get; set; }
        public DataTemplate LearningTemplate { get; set; }
        public DataTemplate ArchivedTemplate { get; set; }
        public DataTemplate ScheduledTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = (CardState)item;

            return type switch
            {
                CardState.Reviewing => ReviewingTemplate,
                CardState.Learning => LearningTemplate,
                CardState.Archived => ArchivedTemplate,
                CardState.Scheduled => ScheduledTemplate,
                _ => null
            };
        }
    }
}
