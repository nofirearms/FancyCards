using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace FancyCards.Helpers
{
    // Команда-сигнал без логики
    public class SignalCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public event EventHandler Executed;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) { 
            Executed?.Invoke(null, null); } // Пустая реализация

        public void RaiseExecute() => Execute(null);
    }
}
