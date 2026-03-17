using FancyCards.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FancyCards.Controls
{
    /// <summary>
    /// Interaction logic for ContextBase.xaml
    /// </summary>
    public partial class ContextBase : UserControl
    {
        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }


        public new static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ContextBase), new PropertyMetadata(null, OnNewContentChanged));

        private static void OnNewContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ContextBase)d;

            //_______________________________
            var target = App.Current.ContextMenuParent;
            var render = new RenderTargetBitmap((int)Math.Ceiling(target.ActualWidth), (int)Math.Ceiling(target.ActualHeight), 96, 96, PixelFormats.Default);
            render.Render(target);

            var point = target.TranslatePoint(new Point(0, 0), App.Current.MainWindow);

            control.ParentImage.Source = render;

            Canvas.SetTop(control.ImageGrid, point.Y);
            Canvas.SetLeft(control.ImageGrid, point.X);

            //TODO переделать чтоб можно было выбирать в xaml
            if(target is AudioGraph)
            {
                control.ImageGrid.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                control.ImageGrid.Background = (Brush)Application.Current.FindResource("MaterialDesign.Brush.Primary.Light");
            }
        }



        private bool _contextContentLoaded = false;

        public ContextBase()
        {
            InitializeComponent();

        }



        private void BackgroundMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = Content as BaseModalViewModel;
            vm.CancelObject();
        }

        private void ContextContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                _contextContentLoaded = true;
            }
        }

        private void ContextContent_LayoutUpdated(object sender, EventArgs e)
        {
            if(VisualTreeHelper.GetChildrenCount(ContextContent) > 0 && _contextContentLoaded) 
            {
                _contextContentLoaded = false;

                ChangeContentPosition();
            }
            else
            {
                
            }
        }

        private void ChangeContentPosition()
        {
            //_______________________________
            var mouse_point = Mouse.GetPosition(this);

            var content = ContextContent;
            var content_h = content.ActualHeight;
            var content_w = content.ActualWidth;

            double content_x = 0;
            double content_y = 0;

            //if (context_view.Position == DialogPosition.MouseCenter)
            //{

            content_x = Math.Clamp(mouse_point.X - (content_w / 2), 5, this.ActualWidth - (content_w) - 15);
            content_y = Math.Clamp(mouse_point.Y - (content_h / 2), 3, this.ActualHeight - (content_h) - 10);

            //content_x = Math.Min(Math.Max(5, mouse_point.X - (content_w / 2)), this.ActualWidth - (content_w) - 5);
            //content_y = Math.Min(Math.Max(5, mouse_point.Y - (content_h / 2)), this.ActualHeight - (content_h) - 5);
            //}
            //var window = control.FindVisualParent<Window>();
            //var window_w = window.ActualWidth;
            //var window_h = window.ActualHeight;

            //content_x = (window_w / 2) - (content_w / 2);
            //content_y = (window_h / 2) - (content_h / 2);



            Canvas.SetTop(ContentBorder, content_y);
            Canvas.SetLeft(ContentBorder, content_x);
        }
    }
}
