using FancyCards.Extensions;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace FancyCards.Behaviors
{
    public class OpenContextMenuBehavior : Behavior<FrameworkElement>
    {


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(OpenContextMenuBehavior), new PropertyMetadata(null));


        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Parameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(OpenContextMenuBehavior), new PropertyMetadata(null));




        public MouseButton Button
        {
            get { return (MouseButton)GetValue(ButtonProperty); }
            set { SetValue(ButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Button.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register("Button", typeof(MouseButton), typeof(OpenContextMenuBehavior), new PropertyMetadata(MouseButton.Right));




        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += OnMouseDown;
            
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            AssociatedObject.PreviewMouseUp -= OnMouseUp;
            AssociatedObject.MouseLeave -= OnLeave;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            AssociatedObject.PreviewMouseUp -= OnMouseUp;
            AssociatedObject.MouseLeave -= OnLeave;



            //TODO подумать как переделать
            App.Current.ContextMenuParent = AssociatedObject;

            Command?.Execute(Parameter);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject is null) return;

            if(e.ChangedButton == Button)
            {
                //e.Handled = true;

                AssociatedObject.PreviewMouseUp += OnMouseUp;
                AssociatedObject.MouseLeave += OnLeave;
            }
                
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseDown -= OnMouseDown;
        }
    }
}
