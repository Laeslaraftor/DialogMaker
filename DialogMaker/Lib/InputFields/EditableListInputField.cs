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
                if (field != value)
                {
                    OnPropertyChanging(nameof(Placeholder));
                    field = value;

                    if (_view.Placeholder?.Equals(value) != true)
                    {
                        _view.Placeholder = value;
                    }

                    OnPropertyChanged(nameof(Placeholder));
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

                    OnPropertyChanging(nameof(Placeholder));
                    field = value;

                    if (_view.EditableList?.Equals(value) != true)
                    {
                        _view.EditableList = list;
                    }

                    OnPropertyChanged(nameof(Placeholder));
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
                    OnPropertyChanging(nameof(InputFieldHandler));
                    field = value;

                    if (_view.InputFieldHandler != value)
                    {
                        _view.InputFieldHandler = value;
                    }

                    OnPropertyChanged(nameof(InputFieldHandler));
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
