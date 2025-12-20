using DialogMaker.Core;
using System.ComponentModel;
using System.Windows.Input;

namespace DialogMaker.Lib.Commands
{
    public class PropertyCommand : Disposable, ICommand
    {
        public PropertyCommand(INotifyPropertyChanged instance, string propertyName, Action<object?> execute, Func<object?, bool>? canExecute)
        {
            ObservingInstance = instance;
            ObservingProperty = propertyName;

            _execute = execute;
            _canExecute = canExecute;

            instance.PropertyChanged += OnInstancePropertyChanged;
        }
        public PropertyCommand(INotifyPropertyChanged instance, string propertyName, Action<object?> execute)
            : this(instance, propertyName, execute, null)
        {
        }

        public event EventHandler? CanExecuteChanged;

        public INotifyPropertyChanged ObservingInstance { get; }
        public string ObservingProperty { get; }

        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        #region Управление

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            ObservingInstance.PropertyChanged -= OnInstancePropertyChanged;
        }

        #endregion

        #region События

        private void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ObservingProperty)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
