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
    public abstract partial class BaseModalViewModel<TResult> : BaseModalViewModel
    {
        private TaskCompletionSource<ModalResult<TResult>> _completionSource;

        [ObservableProperty]
        private bool _loading = false;

        protected BaseModalViewModel()
        {
            _completionSource = new TaskCompletionSource<ModalResult<TResult>>(); 
        }

        public async Task<ModalResult<TResult>> OpenAsync()
        {
            try
            {
                Loading = true;

                await LoadData();

                Loading = false;

                return await _completionSource.Task;
            }
            finally
            {
                Dispose();
            }

        }

        protected async virtual Task LoadData() { }
        protected void Dispose() { }

        protected async Task Close(bool success = true, TResult data = default, string buttonTag = "Close")
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
