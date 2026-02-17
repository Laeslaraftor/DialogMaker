using Acly;
using DialogMaker.Core;
using DialogMaker.Lib.InputFields;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace DialogMaker.Lib.Converters
{
    public class ObjectEditorsConverter : Disposable, ICollectionValueConverter<object?, InputField>
    {
        public event EventHandler<CollectionItemEventArgs<InputField>>? EditorChanged;

        public Type ItemsType
        {
            get
            {
                field ??= typeof(object);
                return field;
            }
            set
            {
                if (field != value)
                {
                    field = value;
                    FieldType = InputField.GetFieldType(value);
                }
            }
        }

        private Type? FieldType
        {
            get
            {
                field ??= InputField.GetFieldType(ItemsType);
                return field;
            }
            set => field = value;
        }


        private readonly List<InputField> _createdFields = [];

        #region Управление

        public InputField Convert(object? Value, int Index, IList FirstCollection, IList SecondCollection)
        {
            var fieldType = FieldType;

            if (SecondCollection.Count > Index && 
                SecondCollection[Index] is InputField createdField && 
                createdField.GetType() == fieldType)
            {
                return createdField;
            }

            var field = InputField.GetFromFieldType(fieldType);
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
        public object? ConvertBack(InputField Value, int Index, IList FirstCollection, IList SecondCollection)
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
