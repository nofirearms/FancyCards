using FancyCards.Database;
using FancyCards.Services;
using FancyCards.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

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

        public IServiceProvider Services { get; }
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data source=data.db"));
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<Repository>();
            services.AddSingleton<DataService>();
            services.AddSingleton<ModalService>();


            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

}
