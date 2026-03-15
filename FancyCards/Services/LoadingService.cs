using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FancyCards.Services
{
    public class LoadingService
    {
        public event Action<LoadingArgs> OnLoadingChanged;

        public LoadingService()
        {

        }

        public void ShowLoading(bool showWaitCursor = true, bool showBackground = true)
        {
            OnLoadingChanged?.Invoke(new LoadingArgs { ShowBackground = showBackground, ShowLoadingCursor = showWaitCursor });
        }

        public async Task ShowLoadingAsync(
            Func<Task> action,
            bool showWaitCursor = true,
            bool showBackground = true,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                OnLoadingChanged?.Invoke(new LoadingArgs { ShowBackground = showBackground, ShowLoadingCursor = showWaitCursor });
                await action().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Не показываем ошибку при отмене
                throw;
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Debug.WriteLine($"Error in ShowLoading: {ex}");
                throw;
            }
            finally
            {
                OnLoadingChanged?.Invoke(new LoadingArgs { ShowBackground = false, ShowLoadingCursor = false });
            }
        }



        public async Task<T> ShowLoadingAsync<T>(
            Func<Task<T>> action, bool showWaitCursor = true, bool showBackground = true, CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                OnLoadingChanged?.Invoke(new LoadingArgs { ShowBackground = showBackground, ShowLoadingCursor = showWaitCursor });
                return await action().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Не показываем ошибку при отмене
                throw;
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Debug.WriteLine($"Error in ShowLoading: {ex}");
                throw;
            }
            finally
            {
                OnLoadingChanged?.Invoke(new LoadingArgs { ShowBackground = false, ShowLoadingCursor = false });
            }
        }

    }





    public class LoadingArgs
    {
        public bool ShowLoadingCursor { get; set; }
        public bool ShowBackground { get; set; }
    }
}
