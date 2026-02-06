using CommunityToolkit.Mvvm.ComponentModel;
using FancyCards.Models;


namespace FancyCards.ViewModels.Modal
{
    public abstract class BaseModalViewModel : ObservableObject
    {
        public abstract void CancelObject();
    }
    public abstract partial class BaseModalViewModel<TResult> : BaseModalViewModel
    {
        private TaskCompletionSource<ModalResult<TResult>> _completionSource;

        public Task<ModalResult<TResult>> Task => _completionSource.Task;

        protected BaseModalViewModel()
        {
            _completionSource = new TaskCompletionSource<ModalResult<TResult>>(); 
        }

        protected async void Close(bool success = true, TResult data = default, string buttonTag = "Close")
        {
            
            _completionSource.TrySetResult(new ModalResult<TResult>
            { 
                Success = success,
                Data = data,
                ButtonTag = buttonTag
            });
            
        }

        protected void Cancel()
        {
            _completionSource.TrySetResult(new ModalResult<TResult>
            {
                Success = false,
                Data = default,
                ButtonTag = "Cancel"
            });
        }

        public override void CancelObject()
        {
            Cancel();
        }
    }
}
