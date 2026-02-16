using FancyCards.Models;
using FancyCards.ViewModels;
using System.Collections.ObjectModel;

namespace FancyCards.Services
{
    public class ModalService
    {
        private readonly ObservableCollection<BaseModalViewModel> _activeModals;
        public IReadOnlyList<BaseModalViewModel> ActiveModals => _activeModals;

        public ModalService()
        {
            _activeModals = new ObservableCollection<BaseModalViewModel>();
        }

        public async Task<ModalResult<TResult>> ShowModalAsync<TResult>(BaseModalViewModel<TResult> modalViewModel)
        {

            return await App.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    _activeModals.Add(modalViewModel);
                    var result = await modalViewModel.Task;
                    return result;
                }
                finally
                {
                    _activeModals.Remove(modalViewModel);
                }
            });

        }


        public void CloseAll()
        {
            _activeModals.Clear();
        }
    }

}
