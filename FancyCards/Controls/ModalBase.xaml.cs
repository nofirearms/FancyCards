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
    /// <summary>
    /// Interaction logic for ModalBase.xaml
    /// </summary>
    public partial class ModalBase : UserControl
    {


        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public new static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ModalBase), new PropertyMetadata(null));




        public ICommand ContentLoaded
        {
            get { return (ICommand)GetValue(ContentLoadedProperty); }
            set { SetValue(ContentLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentLoadedProperty =
            DependencyProperty.Register(nameof(ContentLoaded), typeof(ICommand), typeof(ModalBase), new PropertyMetadata(null));



        public ICommand ContentLoading
        {
            get { return (ICommand)GetValue(ContentLoadingProperty); }
            set { SetValue(ContentLoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContentLoading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentLoadingProperty =
            DependencyProperty.Register(nameof(ContentLoading), typeof(ICommand), typeof(ModalBase), new PropertyMetadata(null));



        //public Brush ModalBackground
        //{
        //    get { return (Brush)GetValue(ModalBackgroundProperty); }
        //    set { SetValue(ModalBackgroundProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for ModalBackground.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ModalBackgroundProperty =
        //    DependencyProperty.Register(nameof(ModalBackground), typeof(Brush), typeof(ModalBase), new PropertyMetadata((Brush)Application.Current.FindResource("MaterialDesign.Brush.Secondary.Light")));




        //public Color Backdrop
        //{
        //    get { return (Color)GetValue(BackdropProperty); }
        //    set { SetValue(BackdropProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Backdrop.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty BackdropProperty =
        //    DependencyProperty.Register(nameof(Backdrop), typeof(Color), typeof(ModalBase), new PropertyMetadata(Colors.Black));







        public ModalBase()
        {
            InitializeComponent();

            Loaded += ModalBase_Loaded;
        }

        public override void OnApplyTemplate()
        {
            ContentLoading?.Execute(null);
            base.OnApplyTemplate();
        }

        private void ModalBase_Loaded(object sender, RoutedEventArgs e)
        {
            ContentLoaded?.Execute(null);
        }
    }
}
