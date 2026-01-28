using FancyCards.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;


namespace FancyCards.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<MainWindowViewModel>(); 
        }
    }
}