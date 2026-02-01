using FancyCards.Models;
using FancyCards.ViewModels;
using FancyCards.ViewModels.Modal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
                    var result = await modalViewModel.OpenAsync();
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
