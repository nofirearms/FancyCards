using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace FancyCards.Services
{
    public class LoadingCursorManager : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private bool _cursorSet;
        private bool _operationCompleted;

        public LoadingCursorManager(int delayMs = 10)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(delayMs);
            _timer.Tick += OnTimerTick;
        }

        // Вызываем, когда операция началась
        public void OperationStarted()
        {
            _operationCompleted = false;
            _timer.Start();
        }

        // Вызываем, когда операция завершилась (из event)
        public void OperationCompleted()
        {
            _operationCompleted = true;
            _timer.Stop();

            if (_cursorSet)
            {
                Mouse.OverrideCursor = null;
                _cursorSet = false;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _timer.Stop();

            // Если операция еще не завершилась - ставим курсор
            if (!_operationCompleted)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _cursorSet = true;
            }
        }

        public void Dispose()
        {
            _timer?.Stop();

            if (_cursorSet)
            {
                Mouse.OverrideCursor = null;
            }
        }
    }

}
