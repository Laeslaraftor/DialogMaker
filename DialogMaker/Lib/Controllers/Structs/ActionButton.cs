using DialogMaker.Core;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogMaker.Lib.Controllers
{
    public class ActionButton : ObservableObject, ICommand
    {
        public event EventHandler<object?>? Clicked;
        public event EventHandler? CanExecuteChanged;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    InvokePropertyChanging(nameof(IsVisible));
                    _isVisible = value;
                    InvokePropertyChanged(nameof(IsVisible));
                }
            }
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    InvokePropertyChanging(nameof(IsEnabled));
                    _isEnabled = value;
                    InvokePropertyChanged(nameof(IsEnabled));
                }
            }
        }
        public string? Icon
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Icon));
                    field = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    InvokePropertyChanged(nameof(Icon));
                }
            }
        }
        public string? Text
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Text));
                    field = value;
                    InvokePropertyChanged(nameof(Text));
                }
            }
        }
        public Brush Color
        {
            get => field ?? App.TextBrush;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Color));  
                    field = value;
                    InvokePropertyChanged(nameof(Color));
                }
            }
        }
        public string? ToolTip
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(ToolTip));
                    field = value;
                    InvokePropertyChanged(nameof(ToolTip));
                }
            }
        }

        private bool _isVisible = true;
        private bool _isEnabled = true;

        #region Управление

        public bool CanExecute(object? parameter)
        {
            return _isEnabled;
        }
        public void Execute(object? parameter)
        {
            Clicked?.Invoke(this, parameter);
        }

        #endregion
    }
}
