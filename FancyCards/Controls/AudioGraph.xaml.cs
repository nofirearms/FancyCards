using FancyCards.Audio;
using FancyCards.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// <summary>
    /// Interaction logic for AudioGraph.xaml
    /// </summary>
    public partial class AudioGraph : UserControl
    {

        private Point _startPoint;
        private bool _isSelecting;

        public ObservableCollection<double> Points
        {
            get { return (ObservableCollection<double>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(ObservableCollection<double>), typeof(AudioGraph), new PropertyMetadata(default, OnPointsChanged));

        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;
            var polyline = control.AudioGraphPolyline;
            var points = (ObservableCollection<double>)e.NewValue;

            if (points is null) return;

            polyline.Points.Clear();

            foreach (var point in points)
            {
                ////если конец списка, растягиваем
                //if (point.X == -1)
                //{
                //    behavior.StretchGraph(polyline);
                //    break;
                //}
                control.AddGraphPoint(point);
            }


            points.CollectionChanged += (s, a) =>
            {
                polyline.Dispatcher.Invoke(() =>
                {
                    if (a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        foreach (double point in a.NewItems)
                        {
                            ////если конец списка, растягиваем
                            //if (point.X == -1)
                            //{
                            //    behavior.StretchGraph(polyline);
                            //    return;
                            //}
                            control.AddGraphPoint(point);


                            var x = polyline.Points.Count / 2;

                            if (x >= control.AudioGraphGrid.ActualWidth)
                            {
                                control.StretchGraphHorizontally();
                            }
                            else
                            {
                                control.ResetGraphScale();
                            }
                        }
                    }
                    else if (a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                    {
                        polyline.Points.Clear();
                    }
                });

            };

        }

        public State SamplerState
        {
            get { return (State)GetValue(SamplerStateProperty); }
            set { SetValue(SamplerStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SamplerState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SamplerStateProperty =
            DependencyProperty.Register(nameof(SamplerState), typeof(State), typeof(AudioGraph), new PropertyMetadata(State.Initial, OnSamplerStateChanged));

        private static void OnSamplerStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;
            var oldState = (State)e.OldValue;
            var newState = (State)e.NewValue;

            control.OnStateChanged(oldState, newState);
        }

        public double StartSelection
        {
            get { return (double)GetValue(StartSelectionProperty); }
            set { SetValue(StartSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartSelectionProperty =
            DependencyProperty.Register(nameof(StartSelection), typeof(double), typeof(AudioGraph), new PropertyMetadata(0d, OnStartSelectionChanged));

        private static void OnStartSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;

            control.UpdateSelection();
        }

        public double EndSelection
        {
            get { return (double)GetValue(EndSelectionProperty); }
            set { SetValue(EndSelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndSelection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndSelectionProperty =
            DependencyProperty.Register(nameof(EndSelection), typeof(double), typeof(AudioGraph), new PropertyMetadata(1d, OnEndSelectionChanged));

        private static void OnEndSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;
            control.UpdateSelection();
        }

        public double StartPlaybackPosition
        {
            get { return (double)GetValue(StartPlaybackPositionProperty); }
            set { SetValue(StartPlaybackPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartPlaybackPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartPlaybackPositionProperty =
            DependencyProperty.Register(nameof(StartPlaybackPosition), typeof(double), typeof(AudioGraph), new PropertyMetadata(0d, OnStartPlaybackPositionChanged));

        private static void OnStartPlaybackPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;

            control.UpdateStartPlaybackPosition();
        }


        public double PlaybackCurrentPosition
        {
            get { return (double)GetValue(PlaybackCurrentPositionProperty); }
            set { SetValue(PlaybackCurrentPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaybackCurrentPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaybackCurrentPositionProperty =
            DependencyProperty.Register(nameof(PlaybackCurrentPosition), typeof(double), typeof(AudioGraph), new PropertyMetadata(0d, OnPlaybackCurrentPositionChanged));

        private static void OnPlaybackCurrentPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AudioGraph)d;

            control.UpdateCurrentPlaybackPosition();
        }

        public AudioGraph()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SelectionRect.Width = 0;
            SelectionRect.Height = SelectionCanvas.ActualHeight;
        }

        private void OnStateChanged(State oldState, State newState)
        {
            if(newState == State.Recording)
            {
                CurrentPositionLine.Visibility = Visibility.Collapsed;
            }
            if(newState == State.Playing)
            {
                
            }
            if (newState == State.Stopped)
            {
                CurrentPositionLine.Visibility = Visibility.Collapsed;
            }

            if (oldState == State.Recording && newState == State.Stopped)
            {
                StretchGraphHorizontally();
                OnRecodringStopped();
            }
        }

        /// <summary>
        /// position 0-1
        /// </summary>
        /// <param name="start"></param>
        private void UpdateSelection()
        {
            Canvas.SetLeft(SelectionRect, Math.Max(0, StartSelection * ActualWidth));
            SelectionRect.Width = Math.Abs(EndSelection * ActualWidth - StartSelection * ActualWidth);
        }

        private void UpdateCurrentPlaybackPosition()
        {
            if (SamplerState != State.Playing) return;

            var pos = Math.Clamp(PlaybackCurrentPosition, 0, 1);
            Canvas.SetLeft(CurrentPositionLine, (SelectionCanvas.ActualWidth - CurrentPositionLine.StrokeThickness) * pos);
            //если через стейт делать, то он отображается вконце графа и перескакивает вначало
            CurrentPositionLine.Visibility = Visibility.Visible;
        }

        private void UpdateStartPlaybackPosition()
        {
            var pos = Math.Clamp(StartPlaybackPosition, 0, 1);

            Canvas.SetLeft(StartPlaybackPositionLine, (SelectionCanvas.ActualWidth - StartPlaybackPositionLine.StrokeThickness) * pos);

        }


        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(SelectionCanvas);
            _isSelecting = true;

            SelectionRect.Visibility = Visibility.Visible;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            var canvas = (Canvas)sender;
            var currentPoint = e.GetPosition(canvas);
            canvas.CaptureMouse();

            var x = Math.Clamp(currentPoint.X / canvas.ActualWidth, 0, 1);
            var start_x = _startPoint.X / canvas.ActualWidth;

            if (start_x <= x)
            {
                StartSelection = start_x;
                EndSelection = x;

                StartPlaybackPosition = start_x;
            }
            else
            {
                StartSelection = x;
                EndSelection = start_x;

                StartPlaybackPosition = x;
            }
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;

            var element = (UIElement)sender;
            element.ReleaseMouseCapture();

            // Передаём выбранный диапазон во ViewModel
            var startX = Math.Min(_startPoint.X, e.GetPosition(SelectionCanvas).X);
            var endX = Math.Max(_startPoint.X, e.GetPosition(SelectionCanvas).X);

        }


        private void ManipulationCanvasMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ManipulationCanvas);
            StartPlaybackPosition = position.X / ManipulationCanvas.ActualWidth;
        }

        private void AddGraphPoint(double y)
        {
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var x = AudioGraphPolyline.Points.Count / 2;
                var height = AudioGraphPolyline.Height / 2;

                var top_height = height - height * y;
                var bottom_height = height + height * y;

                var top_point = new Point(x, top_height);
                var bottom_point = new Point(x, bottom_height);

                AudioGraphPolyline.Points.Add(top_point);
                AudioGraphPolyline.Points.Insert(0, bottom_point);
            });
        }

        private async void StretchGraphHorizontally()
        {
            
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var layout_transform = (ScaleTransform)AudioGraphPolyline.LayoutTransform;
                layout_transform.ScaleX = AudioGraphGrid.ActualWidth / ((AudioGraphPolyline.Points.Count - 1) / 2);
            });
        }
        private async void StretchGraphVertically()
        {
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var render_transform = (ScaleTransform)AudioGraphPolyline.RenderTransform;
                var max_y = Points.Max();
                if (max_y > 0)
                {
                    var y_ratio = 1 / max_y;
                    render_transform.ScaleY = y_ratio;
                }
            });

        }

        private void OnRecodringStopped()
        {
            StartSelection = 0;
            EndSelection = 1;
            StretchGraphVertically();
            UpdateSelection();           
            SelectionRect.Visibility = Visibility.Visible;
        }

        private async void ResetGraphScale()
        {
            
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                SelectionRect.Visibility = Visibility.Collapsed;

                var scale_transform = (ScaleTransform)AudioGraphPolyline.LayoutTransform;
                scale_transform.ScaleX = 1;

                var render_transform = (ScaleTransform)AudioGraphPolyline.RenderTransform;
                render_transform.ScaleY = 1;
            });
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var canvas = (Canvas)sender;
            var currentPoint = e.GetPosition(canvas);

            var x = Math.Clamp(currentPoint.X / canvas.ActualWidth, 0, 1);

            StartPlaybackPosition = x;
        }
    }
}
