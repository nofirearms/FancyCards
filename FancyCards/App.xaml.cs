using FancyCards.Audio;
using FancyCards.Database;
using FancyCards.Services;
using FancyCards.ViewModels;
using FancyCards.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FancyCards
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices(); 
        }

        public static App Current => (App)Application.Current;

        public FrameworkElement ContextMenuParent { get; set; }

        public IServiceProvider Services { get; }
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data source=data.db"));

            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<CardListViewModel>();
            services.AddSingleton<OverlayViewModel>();

            services.AddSingleton<Repository>();
            services.AddSingleton<DataService>();
            services.AddSingleton<ModalService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<TextReplacementService>();
            services.AddSingleton<OverlayService>();
            services.AddSingleton<HotkeyService>();
            services.AddSingleton<NotificationService>();

            services.AddSingleton<ViewModelFactory>();

            services.AddTransient<AudioEngine>();
            services.AddTransient<ReviewIntervalEngine>();



            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = Services.GetRequiredService<MainWindow>();
            window.Loaded += OnWindowLoaded;


            // Принудительная загрузка контрола из ресурсов
            var control = (UserControl)Resources["CardDetailView"];

            // Триггерим инициализацию
            control.Measure(new Size(0, 0));
            control.Arrange(new Rect(0, 0, 0, 0));



            MainWindow = window;
            MainWindow.Show();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow.PreviewMouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }

}
