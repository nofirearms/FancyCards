using FancyCards.Models;
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
            _activeModals.Add(modalViewModel);

            try
            {
                var result = await modalViewModel.Task;
                return result;
            }
            finally
            {
                _activeModals.Remove(modalViewModel);
            }
        }

        public void CloseAll()
        {
            _activeModals.Clear();
        }
    }

}
