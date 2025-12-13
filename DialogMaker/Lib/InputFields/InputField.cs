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

        #region Статика

        public static Type? GetFieldType(object obj)
        {
            return GetFieldType(obj.GetType());
        }
        public static Type? GetFieldType(Type type)
        {
            if (type == typeof(string))
            {
                return typeof(TextInputField);
            }
            if (type == typeof(float))
            {
                return typeof(FloatInputField);
            }
            if (type == typeof(Enum))
            {
                return typeof(EnumInputField);
            }
            if (type == typeof(bool))
            {
                return typeof(BoolInputField);
            }
            if (type.Name == typeof(DialogProjectReference<>).Name ||
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
        public static InputField GetField(Type? type)
        {
            InputField? result = null;
            Type? fieldType = null;

            if (type != null)
            {
                fieldType = GetFieldType(type);
            }
            if (fieldType != null)
            {
                result = Activator.CreateInstance(fieldType) as InputField;
            }

            result ??= new InvalidInputField()
            {
                Placeholder = $"Неподдерживаемый тип: {type}"
            };

            return result;
        }

        #endregion
    }
}
