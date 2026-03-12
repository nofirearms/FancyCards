using FancyCards.Models;
using FancyCards.Models.Param;
using FancyCards.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace FancyCards.Services
{
    public class ModalService
    {
        private readonly ViewModelFactory _factory;
        private readonly ObservableCollection<BaseModalViewModel> _activeModals;
        public IReadOnlyList<BaseModalViewModel> ActiveModals => _activeModals;

        public event Action OnModalOpen;

        public ModalService(ViewModelFactory factory)
        {
            _factory = factory;
            _activeModals = new ObservableCollection<BaseModalViewModel>();
        }

        public async Task<ModalResult<TResult>> ShowModalAsync<TResult>(BaseModalViewModel<TResult> modalViewModel)
        {

            return await App.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    _activeModals.Add(modalViewModel);
                    var result = await modalViewModel.ResultTask;
                    return result;
                }
                finally
                {
                    _activeModals.Remove(modalViewModel);
                }
            });

        }


        public async Task<ModalResult<Deck>> OpenDeckModal(Deck deck)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<DeckDetailViewModel>(deck ?? new Deck()));
        }

        public async Task<ModalResult<Card>> OpenCardModal(Card card)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<CardDetailViewModel>(card ?? new Card()));
        }

        public async Task<ModalResult<object>> OpenMessageBox(string message, string[] buttons, string header = "Attention!", Brush background = null)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<MessageBoxViewModel>(new MessageBoxParameters(header, message, buttons, background)));
        }

        public async Task<ModalResult<object>> OpenFailedAnswer(string answer, string correct)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<TrainingFailedAnswerViewModel>(new TrainingFailedAnswerParameters(answer, correct)));
        }

        public async Task<ModalResult<object>> OpenTraining(IEnumerable<Card> cards)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<TrainingViewModel>(cards));
        }

        public async Task<ModalResult<IEnumerable<Card>>> OpenTrainingStart()
        {
            OnModalOpen?.Invoke();
            await Task.Delay(30);
            return await ShowModalAsync(_factory.Create<TrainingStartViewModel>());
        }

        public async Task<ModalResult<object>> OpenTrainingResult(IEnumerable<TrainingCardViewModel> cards)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(new TrainingResultViewModel(cards));
        }

        public async Task<ModalResult<object>> OpenSettingsModal()
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<SettingsViewModel>());
        }

        public async Task<ModalResult<Deck>> OpenDeckListModal()
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<DeckListViewModel>());
        }

        public async Task<ModalResult<object>> OpenTextReplacementRuleListModal()
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<TextReplacementRuleListViewModel>());
        }

        public async Task<ModalResult<TextReplacementRule>> OpenTextReplacementRuleDetailModal(TextReplacementRule rule)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<TextReplacementRuleDetailViewModel>(rule ?? new TextReplacementRule("")));
        }

        public async Task<ModalResult<object>> OpenStatisticsModal()
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<StatisticsViewModel>());
        }

        public async Task<ModalResult<object>> OpenComboBoxModal(IEnumerable source, object selectedItem, string header)
        {
            OnModalOpen?.Invoke();
            await Task.Delay(20);
            return await ShowModalAsync(_factory.Create<ComboBoxViewModel>(source, selectedItem, header));
        }

        public void CloseAll()
        {
            _activeModals.Clear();
        }
    }

}
