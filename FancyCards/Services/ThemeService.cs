using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FancyCards.Services
{
    public class ThemeService
    {

        public void SetBaseTheme(BaseTheme baseTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);

            UpdateDictionaries(baseTheme);
        }

        //public void SetBaseTheme()
        //{
        //    var paletteHelper = new PaletteHelper();
        //    var theme = paletteHelper.GetTheme();

        //    theme.Foreground = Colors.White;

        //    paletteHelper.SetTheme(theme);
        //}

        public void UpdateDictionaries(BaseTheme baseTheme)
        {
            // 1. Указываем пути к вашим словарям
            string themeUri = baseTheme switch
            {
                BaseTheme.Light => "Resources/Styles/Brushes/LightBrushes.xaml",
                BaseTheme.Dark => "Resources/Styles/Brushes/DarkBrushes.xaml",
                _ => "Resources/Styles/Brushes/LightBrushes.xaml"
            };
                

            var dictionaries = App.Current.Resources.MergedDictionaries;

            // 2. Ищем уже подключенный словарь (по части имени файла)
            var oldDict = dictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Brushes.xaml"));

            // 3. Заменяем его на новый
            if (oldDict != null)
            {
                int index = dictionaries.IndexOf(oldDict);
                dictionaries[index] = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };
            }
            else
            {
                // Если вдруг словаря еще нет, просто добавляем
                dictionaries.Add(new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) });
            }
        }
    }
}
