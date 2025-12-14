using Acly;
using DialogMaker.Lib.Elements;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public class EditableListInputField : InputField
    {
        public override string Placeholder
        {
            get => field ?? string.Empty;
            set
            {
                if(field != value)
                {
                    InvokePropertyChanging(nameof(Placeholder));
                    field = value;

                    if (_view.Placeholder?.Equals(value) != true)
                    {
                        _view.Placeholder = value;
                    }

                    InvokePropertyChanged(nameof(Placeholder));
                }
            }
        }
        public override object? Value
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    if (value is not IEditableList list)
                    {
                        throw new ArgumentException($"Недопустимое значение. Требуется: {typeof(IEditableList)}, получено: {value?.GetType()}", nameof(value));
                    }

                    InvokePropertyChanging(nameof(Placeholder));
                    field = value;

                    if (_view.EditableList?.Equals(value) != true)
                    {
                        _view.EditableList = list;
                    }

                    InvokePropertyChanged(nameof(Placeholder));
                }
            }
        }
        public Action<InputField>? InputFieldHandler
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(InputFieldHandler));
                    field = value;

                    if (_view.InputFieldHandler != value)
                    {
                        _view.InputFieldHandler = value;
                    }

                    InvokePropertyChanged(nameof(InputFieldHandler));
                }
            }
        }

        public override FrameworkElement View => _view;

        private readonly ListEditView _view = new();

        #region Управление

        protected override void SetEnabled(bool value)
        {
        }

        #endregion
    }
}
