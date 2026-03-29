using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;
using System.Windows.Media;


namespace FancyCards.ViewModels
{
    public abstract class BaseModalViewModel : ObservableObject
    {
        public event Action OnLoaded;
        protected void RaiseOnLoaded()
        {
            OnLoaded?.Invoke();
        }
        public abstract void Loaded();
        public abstract void CancelObject();
    }
    public abstract partial class BaseModalViewModel<TResult> : BaseModalViewModel
    {

        [ObservableProperty]
        private string _header = ""; 

        public Brush Background { get; set; } = (Brush)App.Current.FindResource("MaterialDesign.Brush.Secondary.Light");

        public Brush Backdrop { get; set; } = new SolidColorBrush(Colors.Black);


        private TaskCompletionSource<ModalResult<TResult>> _completionSource;

        public Task<ModalResult<TResult>> ResultTask => _completionSource.Task;

        protected BaseModalViewModel()
        {
            _completionSource = new TaskCompletionSource<ModalResult<TResult>>();  
        }

        protected virtual async void Close(bool success = true, TResult data = default, string buttonTag = "Close")
        {
            
            _completionSource.TrySetResult(new ModalResult<TResult>
            { 
                Success = success,
                Data = data,
                ButtonTag = buttonTag
            });
        }

        protected virtual void Cancel()
        {
            _completionSource.TrySetResult(new ModalResult<TResult>
            {
                Success = false,
                Data = default,
                ButtonTag = "Cancel"
            });
        }

        public override void Loaded()
        {
            RaiseOnLoaded();
        }

        public override void CancelObject()
        {
            Cancel();
        }

        [RelayCommand]
        private void CloseModal() => Cancel();

        //public abstract void Dispose();
    }
}
