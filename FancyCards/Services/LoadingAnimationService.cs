using FancyCards.Views;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FancyCards.Services
{
    public class LoadingAnimationService
    {
        private readonly Window _mainWindow;
        private LoadingWindow _loadingWindow = null;
        public LoadingAnimationService()
        {
            _mainWindow = App.Current.MainWindow;
            _mainWindow.LocationChanged += MainWindowLocationChanged;
        }

        private async void MainWindowLocationChanged(object sender, EventArgs e)
        {
            if (_loadingWindow is null) return;

            var point = GetWindowCoordinates();

            _loadingWindow.Dispatcher.Invoke(() =>
            {
                _loadingWindow.Left = point.Left;
                _loadingWindow.Top = point.Top;
                _loadingWindow.Width = point.Width;
                _loadingWindow.Height = point.Height;
            });
        }

        public async void StartLoadingAniamtion()
        {

            // Создаем отдельный поток для UI загрузки
            Thread thread = new Thread(async() =>
            {

                var point = GetWindowCoordinates();
                _loadingWindow = new LoadingWindow
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = point.Left,
                    Top = point.Top,
                    Width = point.Width,
                    Height = point.Height,
                    SizeToContent = SizeToContent.Manual,
                    ShowActivated = false
                };

                // Привязываем положение к текущему окну (опционально)
                _loadingWindow.Loaded += async(s, args) => {
                    
                };

                _loadingWindow.Show();


                // Запускаем цикл обработки сообщений для этого потока
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void StopLoadingAniamtion()
        {
            //_completionSource.TrySetResult();
            //_closeWindow?.Invoke();
            _loadingWindow.Dispatcher.Invoke(() =>
            {
                _loadingWindow.Close();
                _loadingWindow.Dispatcher.InvokeShutdown();
                _loadingWindow = null;
            });

            
            
        }

        private Rect GetWindowCoordinates()
        {
            double main_left = 0;
            double main_top = 0;
            double main_width = 0;
            double main_height = 0;
             _mainWindow.Dispatcher.Invoke(() => 
            {
                main_left = _mainWindow.Left;
                main_top = _mainWindow.Top;
                main_width = _mainWindow.Width;
                main_height = _mainWindow.Height;

                
            });

            var point = new Rect(main_left, main_top, main_width, main_height);

            return point;
        }
    }
}
