using DialogMaker.Core;
using DialogMaker.Core.Editor;
using System.Windows;

namespace DialogMaker.Lib.InputFields
{
    public abstract class InputField : ObservableObject, IDisposable
    {
        ~InputField()
        {
            Dispose(false);
        }

        public abstract string Placeholder { get; set; }
        public abstract object? Value { get; set; }
        public abstract FrameworkElement View { get; }

        #region Управление

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }

        #endregion

        #region Статика

        public static InputField GetField(Type type)
        {
            if (type == typeof(string))
            {
                return new TextInputField();
            }
            if (type == typeof(Enum))
            {
                return new EnumInputField();
            }
            if (type == typeof(bool))
            {
                return new BoolInputField();
            }
            if (type == typeof(DialogProjectReference<>) ||
                type == typeof(DialogProjectResourceObject))
            {
                return new ReferenceInputField();
            }

            throw new ArgumentException($"Неподдерживаемый тип: {type}", nameof(type));
        }

        #endregion
    }
}
