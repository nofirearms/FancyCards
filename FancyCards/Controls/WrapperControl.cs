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

    public class WrapperControl : ContentControl
    {


        public new FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(FrameworkElement), typeof(WrapperControl), new PropertyMetadata(null));

        public object Prefix
        {
            get { return (object)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Prefix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register(nameof(Prefix), typeof(object), typeof(WrapperControl), new PropertyMetadata(null));


        public object Suffix
        {
            get { return (object)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Suffix.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register(nameof(Suffix), typeof(object), typeof(WrapperControl), new PropertyMetadata(null));


        public Brush WrapperForeground
        {
            get { return (Brush)GetValue(WrapperForegroundProperty); }
            set { SetValue(WrapperForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WrapperForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapperForegroundProperty =
            DependencyProperty.Register(nameof(WrapperForeground), typeof(Brush), typeof(WrapperControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));




        public Brush WrapperBackground
        {
            get { return (Brush)GetValue(WrapperBackgroundProperty); }
            set { SetValue(WrapperBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WrapperBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapperBackgroundProperty =
            DependencyProperty.Register(nameof(WrapperBackground), typeof(Brush), typeof(WrapperControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));





        public double WrapperFontSize
        {
            get { return (double)GetValue(WrapperFontSizeProperty); }
            set { SetValue(WrapperFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WrapperFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WrapperFontSizeProperty =
            DependencyProperty.Register(nameof(WrapperFontSize), typeof(double), typeof(WrapperControl), new PropertyMetadata(12d));




        static WrapperControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WrapperControl), new FrameworkPropertyMetadata(typeof(WrapperControl)));
        }
    }
}
