using Acly;
using DialogMaker.Core;
using DialogMaker.Lib.InputFields;
using Newtonsoft.Json.Linq;

namespace DialogMaker.Lib.Converters
{
    public class ObjectEditorsConverter : Disposable, IValueConverter<object?, InputField>
    {
        public event EventHandler<CollectionItemEventArgs<InputField>>? EditorChanged;

        private readonly List<InputField> _createdFields = [];

        #region Управление

        public InputField Convert(object? Value)
        {
            var field = InputField.GetField(Value?.GetType());
            _createdFields.Add(field);

            try
            {
                field.Value = Value;
            }
            catch (Exception error)
            {
                error.Alert();
            }

            EditorChanged?.Invoke(this, new(CollectionItemAction.Add, field));

            return field;
        }
        public object? ConvertBack(InputField Value)
        {
            if (Value == null)
            {
                return null;
            }

            if (_createdFields.Remove(Value))
            {
                Clear(Value);
            }

            return Value.Value;
        }
        public void Clear()
        {
            foreach (var field in _createdFields)
            {
                Clear(field);
            }

            _createdFields.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Clear();            
        }

        private void Clear(InputField field)
        {
            EditorChanged?.Invoke(this, new(CollectionItemAction.Remove, field));
            field.Dispose();
        }

        #endregion
    }
}
