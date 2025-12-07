using DialogMaker.Core;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using DialogMaker.Core.Editor;
using DialogMaker.Lib.InputFields;

namespace DialogMaker.Editor
{
    public class DialogProjectNodeProperty : ObservableObject, IDisposable
    {
        protected DialogProjectNodeProperty(DialogProjectNode node, PropertyInfo property)
        {
            InputField = InputField.GetField(property.PropertyType);
            Node = node;
            Property = property;
            Name = property.GetName();
            Description = property.GetDescription();
            InputField.Placeholder = Name;
            InputField.Value = Value;

            node.Original.PropertyChanged += OnOriginalNodePropertyChanged;
            node.Original.PropertyChanging += OnOriginalNodePropertyChanging;
            InputField.PropertyChanged += OnInputFieldPropertyChanged;
        }
        ~DialogProjectNodeProperty()
        {
            Dispose(false);
        }

        public DialogProjectNode Node { get; }
        public string Name { get; }
        public string Description { get; }
        public object? Value
        {
            get => Property.GetValue(Node.Original);
            set => Property.SetValue(Node.Original, value);
        }
        public FrameworkElement View => InputField.View;

        protected PropertyInfo Property { get; }
        protected InputField InputField { get; } 

        #region Управление

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Node.Original.PropertyChanged -= OnOriginalNodePropertyChanged;
            Node.Original.PropertyChanging -= OnOriginalNodePropertyChanging;
            InputField.PropertyChanged -= OnInputFieldPropertyChanged;

            InputField.Dispose();
        }

        #endregion

        #region События

        protected virtual void OnValueChanging()
        {
            InvokePropertyChanging(nameof(Value));
        }
        protected virtual void OnValueChanged()
        {
            InvokePropertyChanged(nameof(Value));

            var value = Value;

            if (InputField.Value?.Equals(value) != true)
            {
                InputField.Value = value;
            }
        }

        private void OnInputFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var value = InputField.Value;

            if (Value?.Equals(value) != true)
            {
                Value = value;
            }
        }
        protected virtual void OnOriginalNodePropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == Property.Name)
            {
                OnValueChanging();
            }
        }
        protected virtual void OnOriginalNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Property.Name)
            {
                OnValueChanged();
            }
        }

        #endregion

        #region Статика

        private static readonly Dictionary<Type, Type> _allowedTypes = new()
        {
            { typeof(string), typeof(DialogProjectNodeProperty) },
            { typeof(bool), typeof(DialogProjectNodeProperty) },
            //{ typeof(float), typeof(DialogProjectNodeProperty) },
            //{ typeof(int), typeof(DialogProjectNodeProperty) },
            { typeof(Enum), typeof(DialogProjectNodeProperty) },
            { typeof(DialogProjectReference<>), typeof(DialogProjectNodeProperty) },
        };

        public static List<DialogProjectNodeProperty> GetProperties(DialogProjectNode node)
        {
            List<DialogProjectNodeProperty> result = [];

            foreach (var property in node.GetType().GetProperties(BindingFlags.Public))
            {
                if (!property.CanWrite || !property.CanWrite)
                {
                    continue;
                }

                foreach (var typeInfo in _allowedTypes)
                {
                    if (property.PropertyType == typeInfo.Key ||
                        property.PropertyType.IsEnum && typeInfo.Key.IsEnum)
                    {
                        result.Add(new(node, property));
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
