using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FancyCards.Controls
{

    public partial class AnswerTextBoxControl : UserControl
    {



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(AnswerTextBoxControl), new PropertyMetadata(string.Empty));


        public string Prefix
        {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Prefix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(AnswerTextBoxControl), new PropertyMetadata(string.Empty));


        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Suffix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(AnswerTextBoxControl), new PropertyMetadata(string.Empty));




        public AnswerTextBoxControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AnswerTextbox.Focus();
        }

        private void OnMainBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            AnswerTextbox.Focus();
        }

        private void OnMainBorderGotFocus(object sender, RoutedEventArgs e)
        {
            var brush = (Brush)Application.Current.FindResource("MaterialDesign.Brush.Primary");

            TextBlock.SetForeground(SuffixTextBlock, brush);
            TextBlock.SetForeground(PrefixTextBlock, brush);
        }

        private void OnMainBorderLostFocus(object sender, RoutedEventArgs e)
        {
            var brush = (Brush)Application.Current.FindResource("InactiveForeground");

            TextBlock.SetForeground(SuffixTextBlock, brush);
            TextBlock.SetForeground(PrefixTextBlock, brush);
        }
    }
}
