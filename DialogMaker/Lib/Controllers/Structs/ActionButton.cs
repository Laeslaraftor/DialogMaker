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
                    OnPropertyChanging(nameof(IsVisible));
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
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
                    OnPropertyChanging(nameof(IsEnabled));
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
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
                    OnPropertyChanging(nameof(Icon));
                    field = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                    OnPropertyChanged(nameof(Icon));
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
                    OnPropertyChanging(nameof(Text));
                    field = value;
                    OnPropertyChanged(nameof(Text));
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
                    OnPropertyChanging(nameof(Color));
                    field = value;
                    OnPropertyChanged(nameof(Color));
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
                    OnPropertyChanging(nameof(ToolTip));
                    field = value;
                    OnPropertyChanged(nameof(ToolTip));
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
