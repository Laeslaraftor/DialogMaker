using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.InputFields
{
    public class TextInputField : InputField
    {
        public TextInputField()
        {
            _view = new();
            _view.TextChanged += OnTextTextChanged;
        }

        public override string Placeholder
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Placeholder));

                    field = value;

                    if (_view.Placeholder != value)
                    {
                        _view.Placeholder = value;
                        _view.ToolTip = value;
                    }

                    InvokePropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Value));

                    field = value;

                    if (_view.Text?.Equals(value) != true)
                    {
                        _view.Text = value as string;
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly TextBox _view;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _view.TextChanged -= OnTextTextChanged;
        }

        #endregion

        #region События

        private void OnTextTextChanged(object sender, TextChangedEventArgs e)
        {
            Value = _view.Text;
        }

        #endregion
    }
}
