using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public abstract class InputField : Disposable
    {
        public abstract string Placeholder { get; set; }
        public abstract object? Value { get; set; }
        public abstract FrameworkElement View { get; }
        public bool CanEdit
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CanEdit));
                    field = value;
                    SetEnabled(true);
                    InvokePropertyChanged(nameof(CanEdit));
                }
            }
        }

        #region Управление

        protected virtual void SetEnabled(bool value)
        {
            View.IsEnabled = value;
        }

        #endregion

        #region Статика

        public static Type? GetFieldType(object? obj)
        {
            return GetFieldType(obj?.GetType());
        }
        public static Type? GetFieldType(Type? type)
        {
            if (type == null)
            {
                return null;
            }
            if (type == typeof(string))
            {
                return typeof(TextInputField);
            }
            if (type == typeof(float))
            {
                return typeof(FloatInputField);
            }
            if (type == typeof(int))
            {
                return typeof(IntInputField); 
            }
            if (type == typeof(Enum))
            {
                return typeof(EnumInputField);
            }
            if (type == typeof(bool))
            {
                return typeof(BoolInputField);
            }
            if (type == typeof(object))
            {
                return typeof(ObjectInputField);
            }
            if (type == typeof(DialogProjectReference) ||
                type.Name == typeof(DialogProjectReference<>).Name ||
                type == typeof(DialogProjectResourceObject))
            {
                return typeof(ReferenceInputField);
            }
            if (type.GetInterface(nameof(IEditableList)) != null)
            {
                return typeof(EditableListInputField);
            }

            return null;
        }
        public static InputField GetFromFieldType(Type? fieldType)
        {
            InputField? result = null;

            if (fieldType != null)
            {
                result = Activator.CreateInstance(fieldType) as InputField;
            }

            result ??= new InvalidInputField()
            {
                Placeholder = $"Не удалось создать поле: {fieldType}"
            };

            return result;
        }
        public static InputField GetField(Type? type)
        {
            Type? fieldType = null;

            if (type != null)
            {
                fieldType = GetFieldType(type);
            }

            var result = GetFromFieldType(fieldType);

            if (result is InvalidInputField invalidField)
            {
                invalidField.Placeholder = $"Неподдерживаемый тип: {type}";
            }

            return result;
        }

        #endregion
    }
}
