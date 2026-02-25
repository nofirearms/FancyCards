using DynamicData;
using FancyCards.ViewModels;
using FancyCards.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace FancyCards.Services
{
    public class HotkeyService
    {
        private List<VMHotkey> _hotkeys = new();
        private readonly ModalService _modalService;

        public HotkeyService(MainWindow mainWindow, ModalService modalService)
        {
            _modalService = modalService;

            mainWindow.PreviewKeyDown += OnKeyDown;

            (_modalService.ActiveModals as ObservableCollection<BaseModalViewModel>).CollectionChanged += (s, a) =>
            {
                if(a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    var remove = a.OldItems;
                    foreach (var item in remove)
                    {
                        var type = item.GetType();
                        UnregisterHotkeys(type);
                    }
                }

            };
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
           var hotkeys = _hotkeys.Where(h => h.Type == _modalService.ActiveModals.LastOrDefault().GetType());
            if (hotkeys is null) return;
            else
            {
                var hotkey = hotkeys.FirstOrDefault(o => o.Key == e.Key && Keyboard.Modifiers == o.ModifierKeys);
                if(hotkey != null)
                {
                    e.Handled = true;
                    if (hotkey.Command.CanExecute(hotkey.Parameter))
                    {
                        hotkey.Command.Execute(hotkey.Parameter);
                    }
                }
            }
        }

        public void RegisterHotkey<T>(Key key, ModifierKeys modifiers, ICommand command, object parameter = null)
        {
            _hotkeys.Add(new VMHotkey { Type = typeof(T), Key = key, ModifierKeys = modifiers, Command = command, Parameter = parameter}); 
        }

        public void UnregisterHotkeys(Type type)
        {
            var remove = _hotkeys.Where(x => x.Type == type).ToList();
            _hotkeys.RemoveMany(remove);
        }
    }

    public class VMHotkey
    {
        public Type Type { get; set; }
        public Key Key { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
        public ICommand Command { get; set; }
        public object Parameter { get; set; }
    }
}
