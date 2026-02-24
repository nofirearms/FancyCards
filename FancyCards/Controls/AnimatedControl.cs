using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FancyCards.Controls
{
    public class AnimatedControl : ContentControl
    {
        static AnimatedControl()
        {

        }

        // Тип анимации появления
        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register("AnimationType", typeof(AnimationType), typeof(AnimatedControl),
                new PropertyMetadata(AnimationType.ScaleAndFade));

        public AnimationType AnimationType
        {
            get => (AnimationType)GetValue(AnimationTypeProperty);
            set => SetValue(AnimationTypeProperty, value);
        }

        // Длительность анимации
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(int), typeof(AnimatedControl),
                new PropertyMetadata(300));

        public int AnimationDuration
        {
            get => (int)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        // Автоматически скрывать через N мс
        public static readonly DependencyProperty AutoHideDelayProperty =
            DependencyProperty.Register("AutoHideDelay", typeof(int), typeof(AnimatedControl),
                new PropertyMetadata(0));

        public int AutoHideDelay
        {
            get => (int)GetValue(AutoHideDelayProperty);
            set => SetValue(AutoHideDelayProperty, value);
        }


        public AnimatedControl()
        {
            Visibility = Visibility.Collapsed;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ShowAsync();
        }

        public override void OnApplyTemplate()
        {

            base.OnApplyTemplate();
        }

        public async Task ShowAsync()
        {
            Visibility = Visibility.Visible;

            switch (AnimationType)
            {
                case AnimationType.ScaleAndFade:
                    await AnimateScaleAndFade();
                    break;
                case AnimationType.FadeOnly:
                    await AnimateFade();
                    break;
                case AnimationType.SlideFromTop:
                    await AnimateSlideFromTop();
                    break;
                case AnimationType.Pop:
                    await AnimatePop();
                    break;
            }

            if (AutoHideDelay > 0)
            {
                await Task.Delay(AutoHideDelay);
                await HideAsync();
            }
        }

        public async Task HideAsync()
        {
            var fadeAnim = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            var tcs = new TaskCompletionSource<bool>();
            fadeAnim.Completed += (s, e) =>
            {
                Visibility = Visibility.Collapsed;
                tcs.SetResult(true);
            };

            BeginAnimation(OpacityProperty, fadeAnim);
            await tcs.Task;
        }

        private async Task AnimateScaleAndFade()
        {
            Opacity = 0;
            RenderTransform = new ScaleTransform(0.3, 0.3);
            RenderTransformOrigin = new Point(0.5, 0.5);

            await Task.Delay(10);

            var story = new Storyboard();

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(AnimationDuration));
            Storyboard.SetTarget(fadeAnim, this);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            var scaleAnim = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(AnimationDuration));
            Storyboard.SetTarget(scaleAnim, this);
            Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));

            story.Children.Add(fadeAnim);
            story.Children.Add(scaleAnim);

            var tcs = new TaskCompletionSource<bool>();
            story.Completed += (s, e) => tcs.SetResult(true);
            story.Begin();

            await tcs.Task;
        }

        private async Task AnimatePop()
        {
            Opacity = 0;
            RenderTransform = new ScaleTransform(0.1, 0.1);
            RenderTransformOrigin = new Point(0.5, 0.5);

            await Task.Delay(10);

            var story = new Storyboard();

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(100));
            Storyboard.SetTarget(fadeAnim, this);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            var scaleAnim = new DoubleAnimation(0.1, 1.2, TimeSpan.FromMilliseconds(150));
            Storyboard.SetTarget(scaleAnim, this);
            Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("RenderTransform.ScaleX"));

            var scaleBackAnim = new DoubleAnimation(1.2, 1, TimeSpan.FromMilliseconds(100));
            scaleBackAnim.BeginTime = TimeSpan.FromMilliseconds(150);
            Storyboard.SetTarget(scaleBackAnim, this);
            Storyboard.SetTargetProperty(scaleBackAnim, new PropertyPath("RenderTransform.ScaleX"));

            story.Children.Add(fadeAnim);
            story.Children.Add(scaleAnim);
            story.Children.Add(scaleBackAnim);

            var tcs = new TaskCompletionSource<bool>();
            story.Completed += (s, e) => tcs.SetResult(true);
            story.Begin();

            await tcs.Task;
        }

        private async Task AnimateSlideFromTop()
        {
            Opacity = 0;
            RenderTransform = new TranslateTransform(0, -50);

            await Task.Delay(10);

            var story = new Storyboard();

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(AnimationDuration));
            Storyboard.SetTarget(fadeAnim, this);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            var slideAnim = new DoubleAnimation(-50, 0, TimeSpan.FromMilliseconds(AnimationDuration));
            Storyboard.SetTarget(slideAnim, this);
            Storyboard.SetTargetProperty(slideAnim, new PropertyPath("RenderTransform.Y"));

            story.Children.Add(fadeAnim);
            story.Children.Add(slideAnim);

            var tcs = new TaskCompletionSource<bool>();
            story.Completed += (s, e) => tcs.SetResult(true);
            story.Begin();

            await tcs.Task;
        }

        private async Task AnimateFade()
        {
            Opacity = 0;

            await Task.Delay(10);

            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(AnimationDuration));

            var tcs = new TaskCompletionSource<bool>();
            fadeAnim.Completed += (s, e) => tcs.SetResult(true);
            BeginAnimation(OpacityProperty, fadeAnim);

            await tcs.Task;
        }
    }

    public enum AnimationType
    {
        ScaleAndFade,
        FadeOnly,
        SlideFromTop,
        Pop
    }
}
