using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FancyCards.ViewModels.Modal
{
    public abstract class BaseModalViewModel : ObservableObject
    {
        public abstract void CancelObject();
    }
    public abstract class BaseModalViewModel<TResult> : BaseModalViewModel
    {
        private TaskCompletionSource<ModalResult<TResult>> _completionSource;

        public Task<ModalResult<TResult>> Task => _completionSource.Task;

        protected BaseModalViewModel()
        {
            _completionSource = new TaskCompletionSource<ModalResult<TResult>>(); 
        }

        protected void Close(bool success = true, TResult data = default)
        {
            _completionSource.TrySetResult(new ModalResult<TResult>
            { 
                Success = success,
                Data = data
            });
        }

        protected void Cancel()
        {
            _completionSource.TrySetResult(new ModalResult<TResult>
            {
                Success = false,
                Data = default
            });
        }

        public override void CancelObject()
        {
            Cancel();
        }
    }
}
