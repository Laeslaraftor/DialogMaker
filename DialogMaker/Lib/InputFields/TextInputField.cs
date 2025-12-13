using DialogMaker.Lib.Elements;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class TextInputField : InputField
    {
        public TextInputField()
        {
            _view = new();
            _view.ConfirmedText += OnViewConfirmedText;
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
                    if (value != null && value.GetType() != ValueType)
                    {
                        throw new ArgumentException($"Недопустимый тип! Требуется: {ValueType}, получен: {value.GetType()}", nameof(value));
                    }

                    InvokePropertyChanging(nameof(Value));

                    field = value;
                    string textValue = ValueToString(value);

                    if (textValue.Equals(_view.Text) != true)
                    {
                        _view.Text = textValue;
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public override FrameworkElement View => _view;

        protected virtual Type ValueType { get; } = typeof(string);
        protected string Text
        {
            get => _view.Text;
            set => _view.Text = value;
        }

        private readonly Entry _view;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _view.ConfirmedText -= OnViewConfirmedText;
        }

        protected virtual bool TryHandle(string newValue, [NotNullWhen(true)] out object value)
        {
            value = newValue;
            return true;
        }
        protected virtual string ValueToString(object? value)
        {
            var result = value?.ToString();
            return result ?? string.Empty;
        }

        #endregion

        #region События

        private void OnViewConfirmedText(object? sender, ValueChangedEventArgs<string> e)
        {
            if (TryHandle(e.NewValue, out var value))
            {
                Value = value;
                return;
            }

            Text = e.OldValue;
        }

        #endregion
    }
}
