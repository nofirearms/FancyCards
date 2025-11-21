using FancyCards.Audio;
using FancyCards.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for AudioGraph.xaml
    /// </summary>
    public partial class AudioGraph : UserControl
    {


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
                                control.StretchGraph();
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

            if(oldState == State.Recording && newState == State.Stopped)
            {
                control.StretchGraph();
            }
        }

        public AudioGraph()
        {
            InitializeComponent(); 
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

        private async void StretchGraph()
        {
            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var scale_transform = (ScaleTransform)AudioGraphPolyline.LayoutTransform;
                scale_transform.ScaleX = AudioGraphGrid.ActualWidth / ((AudioGraphPolyline.Points.Count - 1) / 2);
            });

        }

        private async void ResetGraphScale()
        {

            AudioGraphPolyline.Dispatcher.Invoke(() =>
            {
                var scale_transform = (ScaleTransform)AudioGraphPolyline.LayoutTransform;
                scale_transform.ScaleX = 1;
            });
        }
    }
}
