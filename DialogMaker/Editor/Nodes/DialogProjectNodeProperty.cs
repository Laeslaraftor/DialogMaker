using DialogMaker.Core;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using DialogMaker.Core.Editor;

namespace DialogMaker.Editor
{
    public abstract class DialogProjectNodeProperty : ObservableObject, IDisposable
    {
        protected DialogProjectNodeProperty(DialogProjectNode node, PropertyInfo property)
        {
            Node = node;
            Property = property;
            Name = property.GetName();
            Description = property.GetDescription();

            node.Original.PropertyChanged += OnOriginalNodePropertyChanged;
            node.Original.PropertyChanging += OnOriginalNodePropertyChanging;
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

        protected PropertyInfo Property { get; }

        #region Управление

        public abstract FrameworkElement GetView();
        public abstract void FreeView(FrameworkElement element);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Node.Original.PropertyChanged -= OnOriginalNodePropertyChanged;
            Node.Original.PropertyChanging += OnOriginalNodePropertyChanging;
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
            { typeof(float), typeof(DialogProjectNodeProperty) },
            { typeof(int), typeof(DialogProjectNodeProperty) },
            { typeof(Enum), typeof(DialogProjectNodeProperty) },
            { typeof(DialogProjectReference<>), typeof(DialogProjectNodeReferenceProperty) },
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
                        object? instance = Activator.CreateInstance(typeInfo.Value, node, property);

                        if (instance is DialogProjectNodeProperty nodeProperty)
                        {
                            result.Add(nodeProperty);
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
