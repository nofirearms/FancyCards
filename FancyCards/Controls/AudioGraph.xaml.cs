using FancyCards.Audio;
using FancyCards.Helpers;
using FancyCards.Models;
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

            if (points.Any())
            {
                control.StretchGraphHorizontally();
                control.StretchGraphVertically();
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


        public Selection Selection
        {
            get { return (Selection)GetValue(SelectionProperty); }
            set { SetValue(SelectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionProperty =
            DependencyProperty.Register(nameof(Selection), typeof(Selection), typeof(AudioGraph), new PropertyMetadata(new Selection(0, 0), OnSelectionChanged));

        private static void OnSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            SelectionOuter.Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
        }

        private void OnStateChanged(State oldState, State newState)
        {
            if(newState == State.Recording)
            {
                InteractionCanvas.IsHitTestVisible = false;
                CurrentPositionLine.Visibility = Visibility.Collapsed;
                StartPlaybackPosition = 0;
                Selection = new Selection(0, 1);
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
                //StretchGraphHorizontally();
                OnRecodringStopped();
            }
        }

        private void UpdateSelection()
        {
            SelectionInner.Rect = new Rect(new Point(Selection.Start * ActualWidth, 0), new Point(Selection.End * ActualWidth, ActualHeight));
        }

        private void UpdateCurrentPlaybackPosition()
        {
            if (SamplerState != State.Playing) return;

            var pos = Math.Clamp(PlaybackCurrentPosition, 0, 1);
            Canvas.SetLeft(CurrentPositionLine, (InteractionCanvas.ActualWidth - CurrentPositionLine.StrokeThickness) * pos);
            //если через стейт делать, то он отображается вконце графа и перескакивает вначало
            CurrentPositionLine.Visibility = Visibility.Visible;
        }

        private void UpdateStartPlaybackPosition()
        {
            var pos = Math.Clamp(StartPlaybackPosition, 0, 1);

            Canvas.SetLeft(StartPlaybackPositionLine, (ActualWidth - StartPlaybackPositionLine.StrokeThickness) * pos);

        }


        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var point_canvas = e.GetPosition(InteractionCanvas);
            if (e.ChangedButton == MouseButton.Left)
            {
                if(e.ClickCount > 1)
                {
                    //micro selection

                    var mouse_canvas = point_canvas.X / InteractionCanvas.ActualWidth;
                    var to_left_edge = Math.Abs(Selection.Start - mouse_canvas);
                    var to_right_edge = Math.Abs(Selection.End - mouse_canvas);

                    var x = Math.Clamp(mouse_canvas, 0, 1);

                    if (to_left_edge < to_right_edge)
                    {
                        Selection = new Selection(x, Selection.End);
                        StartPlaybackPosition = x;
                    }
                    else
                    {
                        Selection = new Selection(Selection.Start, x);
                    }
                }
                else
                {
                    //start selection
                    _startPoint = point_canvas;
                    _isSelecting = true;

                    //start playback position
                    //var x = Math.Clamp(point_canvas.X / InteractionCanvas.ActualWidth, 0, 1);

                    //StartPlaybackPosition = x;
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {

            }
        }


        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;

            var element = (UIElement)sender;
            element.ReleaseMouseCapture();

            // Передаём выбранный диапазон во ViewModel
            var startX = Math.Min(_startPoint.X, e.GetPosition(InteractionCanvas).X);
            var endX = Math.Max(_startPoint.X, e.GetPosition(InteractionCanvas).X);

        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            var canvas = (Canvas)sender;
            var currentPoint = e.GetPosition(canvas);

            if (Math.Abs(currentPoint.X - _startPoint.X) < 3) return;


            canvas.CaptureMouse();

            var x = Math.Clamp(currentPoint.X / canvas.ActualWidth, 0, 1);
            var start_x = _startPoint.X / canvas.ActualWidth;

            if (start_x <= x)
            {
                Selection = new Selection(start_x, x);

                StartPlaybackPosition = start_x;
            }
            else
            {
                Selection = new Selection(x, start_x);

                StartPlaybackPosition = x;
            }
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
            //Selection = new Selection(0, 1);
            InteractionCanvas.IsHitTestVisible = true;
            //StretchGraphVertically();
        }

        private async void ResetGraphScale()
        {
            
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var scale_transform = (ScaleTransform)AudioGraphPolyline.LayoutTransform;
                scale_transform.ScaleX = 1;

                var render_transform = (ScaleTransform)AudioGraphPolyline.RenderTransform;
                render_transform.ScaleY = 1;
            });
        }


    }
}
