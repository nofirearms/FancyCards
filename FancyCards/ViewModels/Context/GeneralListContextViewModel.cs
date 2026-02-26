using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FancyCards.Models;


namespace FancyCards.ViewModels
{
    public class GeneralListContextViewModel : BaseModalViewModel<object>
    {
        //public override void CancelObject()
        //{
        //    Cancel();
        //}
    }
    public partial class GeneralListContextViewModel<T> : GeneralListContextViewModel
    {
        private readonly T _item;

        public GeneralListContextViewModel(T item)
        {
            _item = item;
        }

        [RelayCommand]
        private void Edit() => Close(true, _item, "Edit");
        [RelayCommand]
        private void Remove() => Close(true, _item, "Remove");
    }
}
