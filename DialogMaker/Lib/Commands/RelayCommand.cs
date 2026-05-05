using System.Windows.Input;

namespace DialogMaker.Lib
{
    public class RelayCommand(Action<object?> execute) : ICommand
    {
        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
            : this(execute)
        {
            _canExecuteMethod = canExecute;
        }
        public RelayCommand(Func<object?, bool> canExecute, Action<object?> execute)
            : this(execute, canExecute)
        {
        }

        public event EventHandler? CanExecuteChanged;

        private readonly Func<object?, bool>? _canExecuteMethod;
        private readonly Action<object?> _executeMethod = execute;

        #region Управление

        public bool CanExecute(object? parameter)
        {
            if (_canExecuteMethod == null)
            {
                return true;
            }

            return _canExecuteMethod(parameter);
        }
        public void Execute(object? parameter)
        {
            _executeMethod(parameter);
        }

        public void InvokeCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
