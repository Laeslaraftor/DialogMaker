using DialogMaker.Lib.Elements;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class TextInputField : InputField
    {
        public TextInputField()
        {
            Entry = new()
            {
                TextWrapping = TextWrapping.Wrap
            };
            Entry.ConfirmedText += OnViewConfirmedText;
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

                    if (Entry.Placeholder != value)
                    {
                        Entry.Placeholder = value;
                        Entry.ToolTip = value;
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
                    if (!CanConvert(value))
                    {
                        throw new ArgumentException($"Недопустимый тип! Требуется: {ValueType}, получен: {value?.GetType()}", nameof(value));
                    }

                    InvokePropertyChanging(nameof(Value));

                    field = Convert(value);
                    string textValue = ValueToString(value);

                    if (textValue.Equals(Entry.Text) != true)
                    {
                        Entry.Text = textValue;
                    }

                    InvokePropertyChanged(nameof(Value));
                }
            }
        }
        public bool Multiline
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Multiline));
                    field = value;
                    Entry.TextBox.AcceptsReturn = value;
                    InvokePropertyChanged(nameof(Multiline));
                }
            }
        }
        public override FrameworkElement View => Entry;

        protected virtual Type ValueType { get; } = typeof(string);
        protected string Text
        {
            get => Entry.Text;
            set => Entry.Text = value;
        }
        protected Entry Entry { get; }

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Entry.ConfirmedText -= OnViewConfirmedText;
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
        protected virtual bool CanConvert(object? value)
        {
            return value == null || value.GetType() == ValueType;
        }
        protected virtual object? Convert(object? value)
        {
            if (value is string)
            {
                return value;
            }

            return value?.ToString();
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
