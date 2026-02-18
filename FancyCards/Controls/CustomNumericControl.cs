
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FancyCards.Controls
{
    public class CustomNumericControl : ContentControl
    {

        private double _initY;
        private int _initialValue;
        private bool _ctrlPressed = false;


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CustomNumericControl), new PropertyMetadata(default(CornerRadius)));





        public Brush MouseOverOverlay
        {
            get { return (Brush)GetValue(MouseOverOverlayProperty); }
            set { SetValue(MouseOverOverlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverOverlay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverOverlayProperty =
            DependencyProperty.Register(nameof(MouseOverOverlay), typeof(Brush), typeof(CustomNumericControl), new PropertyMetadata((Brush)Application.Current.FindResource("MaterialDesign.Brush.Primary")));




        public Visibility MouseOverBackgroundVisibility
        {
            get { return (Visibility)GetValue(MouseOverBackgroundVisibilityProperty); }
            set { SetValue(MouseOverBackgroundVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverBackgroundVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverBackgroundVisibilityProperty =
            DependencyProperty.Register("MouseOverBackgroundVisibility", typeof(Visibility), typeof(CustomNumericControl), new PropertyMetadata(Visibility.Visible));




        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(int.MinValue));




        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(int.MaxValue));




        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(0));




        public int Frequency
        {
            get { return (int)GetValue(FrequencyProperty); }
            set { SetValue(FrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Frequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FrequencyProperty =
            DependencyProperty.Register("Frequency", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(1));


        public int AlternativeFrequency
        {
            get { return (int)GetValue(AlternativeFrequencyProperty); }
            set { SetValue(AlternativeFrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Frequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlternativeFrequencyProperty =
            DependencyProperty.Register("AlternativeFrequency", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(10));


        public int Multiplier
        {
            get { return (int)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Multiplier.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MultiplierProperty =
            DependencyProperty.Register("Multiplier", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(5));




        public int DefaultValue
        {
            get { return (int)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(int), typeof(CustomNumericControl), new PropertyMetadata(0, OnDefaultValueChanged));

        private static void OnDefaultValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        public MouseButton MouseButton
        {
            get { return (MouseButton)GetValue(MouseButtonProperty); }
            set { SetValue(MouseButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseButtonProperty =
            DependencyProperty.Register("MouseButton", typeof(MouseButton), typeof(CustomNumericControl), new PropertyMetadata(MouseButton.Left));




        static CustomNumericControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomNumericControl), new FrameworkPropertyMetadata(typeof(CustomNumericControl)));     
            
        }

        public CustomNumericControl()
        {
            this.PreviewMouseDown += OnMouseDown;
            this.PreviewMouseDoubleClick += OnMouseDoubleClick;
            this.PreviewMouseWheel += OnMouseWheel;
            App.Current.MainWindow.KeyUp += OnKeyUp;
            App.Current.MainWindow.KeyDown += OnKeyDown;
        }

        
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) _ctrlPressed = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) _ctrlPressed = true;
        }


        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton)
            {
                Value = GetValue(DefaultValue);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton)
            {
                e.Handled = true;

                _initY = e.GetPosition(this).Y;
                _initialValue = Value;


                this.CaptureMouse();
                this.PreviewMouseMove += OnMouseMove;
                this.PreviewMouseUp += OnMouseUp;

                this.Cursor = Cursors.Hand;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton)
            {
                this.PreviewMouseMove -= OnMouseMove;
                this.PreviewMouseUp -= OnMouseUp;

                this.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }               
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.SizeNS;

            var freq = _ctrlPressed ? AlternativeFrequency : Frequency;

            var delta = (int)((_initY - e.GetPosition(this).Y) / 20);

            Value = GetValue(_initialValue + delta * freq);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var freq = _ctrlPressed ? AlternativeFrequency : Frequency;

            if (e.Delta > 0)
            {
                Value = GetValue(Value + freq);
            }
            else
            {
                Value = GetValue(Value - freq);
            }
        }

        private int GetValue(int parameter)
        {
            return Math.Clamp(parameter, MinValue, MaxValue);
            //return (int)Math.Max(MinValue, Math.Min(parameter, MaxValue));
        }

    }
}
