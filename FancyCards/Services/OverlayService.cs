using FancyCards.Models;
using FancyCards.ViewModels;

namespace FancyCards.Services
{
    public class OverlayService
    {
        private readonly OverlayViewModel _vm;

        public OverlayService(OverlayViewModel overlayViewModel)
        {
            _vm = overlayViewModel;
        }

        public void Show(OverlayType type)
        {
            _vm.Type = type;
            _vm.IsVisible = true;
        }

        public void Hide()
        {
            _vm.IsVisible = false;
            _vm.Type = OverlayType.None;
        }

        public async Task ShowAndHideAsync(OverlayType type, int durationMs)
        {
            
            Show(type);
            await Task.Delay(TimeSpan.FromMilliseconds(durationMs));
            Hide();
        }
    }
}
