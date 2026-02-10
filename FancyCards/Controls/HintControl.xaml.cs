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
    
    public partial class HintControl : UserControl
    {




        public new Control Content
        {
            get { return (Control)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly new DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(Control), typeof(HintControl), new PropertyMetadata(null, OnNewContentChanged));

        private static void OnNewContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HintControl)d;
            var content = e.NewValue as Control;

            if (content is null) return;

            content.GotFocus += (_, _) =>
            {
                var brush = (Brush)Application.Current.FindResource("MaterialDesign.Brush.Primary");
                control.HintTextBlock.Foreground = brush;
                control.SuffixTextBlock.Foreground = brush;
            };

            content.LostFocus += (_, _) =>
            {
                var brush = new SolidColorBrush(Colors.Black);
                control.HintTextBlock.Foreground = brush;
                control.SuffixTextBlock.Foreground = brush;
            };

            content.GotKeyboardFocus += (_, _) =>
            {
                var brush = (Brush)Application.Current.FindResource("MaterialDesign.Brush.Primary");
                control.HintTextBlock.Foreground = brush;
                control.SuffixTextBlock.Foreground = brush;
            };

            content.LostKeyboardFocus += (_, _) =>
            {
                var brush = new SolidColorBrush(Colors.Black);
                control.HintTextBlock.Foreground = brush;
                control.SuffixTextBlock.Foreground = brush;
            };

            content.PreviewGotKeyboardFocus += (_, _) =>
            {

            };
        }


        public Brush HintBackground
        {
            get { return (Brush)GetValue(HintBackgroundProperty); }
            set { SetValue(HintBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintBackgroundProperty =
            DependencyProperty.Register(nameof(HintBackground), typeof(Brush), typeof(HintControl), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register(nameof(HintText), typeof(string), typeof(HintControl), new PropertyMetadata(string.Empty, OnHintTextChanged));

        private static void OnHintTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HintControl)d;
            var hint = e.NewValue as string;

            if (string.IsNullOrEmpty(hint) || hint == "0")
            {
                control.HintTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.HintTextBlock.Visibility = Visibility.Visible;
            }
        }

        public string SuffixText
        {
            get { return (string)GetValue(SuffixTextProperty); }
            set { SetValue(SuffixTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuffixText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuffixTextProperty =
            DependencyProperty.Register(nameof(SuffixText), typeof(string), typeof(HintControl), new PropertyMetadata(string.Empty, OnSuffixTextChanged));

        private static void OnSuffixTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HintControl)d;
            var suffix = e.NewValue as string;

            if (string.IsNullOrEmpty(suffix) || suffix == "0")
            {
                control.SuffixTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.SuffixTextBlock.Visibility = Visibility.Visible;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        public HintControl()
        {
            InitializeComponent();
        }
    }
}
